Imports System.Collections.Immutable
Imports System.Composition
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.Simplification
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

' FOR EASIER F5 DEBUGGING OF THIS ANALYZER:
' Set "PlatformSpecificAnalyzer" as your startup project. Then under MyProject > Debug, det
' StartAction: external program
'     C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
' Command-line arguments: 
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_VB\DemoUWP_VB.sln" /RootSuffix Analyzer



' PlatformSpecific
'
' Some platform methods come from a specific UWP platform extension SDK.
' And some methods might have the [PlatformSpecific] attribute on them.
' And some fields/properties might have the [PlatformSpecific] attribute too.
'
' This analyzer checks that any method which invokes a method that's in
' a specific UWP platform extension SDK, or invokes a method with [PlatformSpecific]
' attribute on it, precedes that invocation with an "If" statement
' that either calls into ApiInformation.IsTypePresent, or accesses a field
' with [PlatformSpecific] attribute on it.

' Limitation1: This analyzer only works with methods. It only examines user-written
' methods for their contents, and it only looks for invocations of platform-specific
' methods. It should be able to look at all user-written code, and should look for
' all kinds of member access.
'
' Limitation2: This analyzer doesn't have knowledge of *which* platform is specific.
' Its designed role is merely as a safeguard, to remind you that you should be guarding.
' It won't help in cases where you're already using some other specific guard but has
' platform-specific member accesses that aren't covered by that other specific guard.
' (In any case, users typically use their "sentinal canary" undocumented knowledge
' that if one type is present then a whole load of other types are also present).
'
' Limitation3: This analyzer doesn't deal with UWP "min-version". It should!
'
' Limitation4: This analyzer doesn't and can't work through lambdas.
' In other words it can't track whether a delegate contains platform-specific member
' accesses. It can't because to do so you'd need a type system like
' List<Action[PlatformSpecific]>, and the CLR type system doesn't do that.



<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class PlatformSpecificAnalyzerVB
    Inherits DiagnosticAnalyzer

    Friend Shared Rule As New DiagnosticDescriptor("UWP001", "Platform-specific", "Platform-specific code", "Safety", DiagnosticSeverity.Warning, True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeInvocation, SyntaxKind.InvocationExpression)
    End Sub

    Public Sub AnalyzeInvocation(context As SyntaxNodeAnalysisContext)
        Dim invocationExpression = CType(context.Node, InvocationExpressionSyntax)

        ' Is this an invocation of a Windows.* method that's outside the UWP common platform?
        ' HACK: this code works for the current 10069 SDK of UWP. At VS2015 RTM, the list
        ' of common target assemblies might have to be tweaked. And when the next revision of
        ' the UWP platform comes out, then we'll have to look into picking up versions
        ' and looking at TargetMinVersion in the .vbproj.
        Dim targetMethod = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol
        If targetMethod Is Nothing Then Return
        If targetMethod.ContainingNamespace Is Nothing OrElse Not targetMethod.ContainingNamespace.ToDisplayString.StartsWith("Windows.") Then Return
        If targetMethod.ContainingType.Name = "ApiInformation" Then Return
        Dim targetAssembly = targetMethod.ContainingAssembly.Name
        If targetAssembly = "Windows.Foundation.FoundationContract" OrElse
                targetAssembly = "Windows.Foundation.UniversalApiContract" OrElse
                targetAssembly = "Windows.Networking.Connectivity.WwanContract" Then Return


        ' Is this invocation outside a method?
        Dim containingMember = invocationExpression.FirstAncestorOrSelf(Of MethodBlockBaseSyntax)
        Dim containingMethod = TryCast(containingMember, MethodBlockSyntax)
        If containingMethod Is Nothing Then context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation)) : Return

        ' Does the containing method/type/assembly claim to be platform-specific?
        Dim containingMethodSymbol = context.SemanticModel.GetDeclaredSymbol(containingMethod)
        If containingMethodSymbol Is Nothing Then Return
        If HasPlatformSpecificAttribute(containingMethodSymbol) Then Return
        Dim ancestorTypeSymbol = containingMethodSymbol.ContainingType
        While ancestorTypeSymbol IsNot Nothing
            If HasPlatformSpecificAttribute(ancestorTypeSymbol) Then Return
            ancestorTypeSymbol = ancestorTypeSymbol.ContainingType
        End While
        If HasPlatformSpecificAttribute(containingMethodSymbol.ContainingAssembly) Then Return

        ' Is this invocation properly guarded? Here are some proper guards...
        ' Case 1: If ApiInformation.IsTypePresent("xyz") Then xyz.f()
        ' Case 2: Dim b = ApiInformation.IsTypePresent(xyz)
        '         If b Then xyz.f()
        ' Case 3: If Not ApiInformation.IsTypePresent(xyz) Then Return
        '         xyz.f()
        ' Case 4: If Not ApiInformation.IsTypePresent(xyz) Then
        '         Else
        '            xyz.f()
        '         End If
        ' Case 5: If GlobalState.FeatureAllowed Then xyz.f()
        '         where the FeatureAllowed field/property Is Like "b" above
        ' Case 6: Select Case False
        '            Case ApiInformation.IsTypePresent(xyz) :
        '            Case Else : xyz.f()
        '          End Select
        ' Case 7: If(ApiInformation.IsTypePresent(xyz), xyz.f(), 0)
        '
        ' In an ideal world I'd like to have dataflow ability, and check whether the invocationExpression
        ' is reachable via a path where none of the conditions along the way have data flowing into
        ' then that might be influenced by ApiInformation.IsTypePresent or by a global field/property.
        ' In the absence of dataflow, I'll fall back on a heuristic...
        '
        ' Rejected heuristic: walk backwards from the current InvocationExpression,
        ' up through all syntacticaly preceding expressions, and see if any of them
        ' mentioned ApiInformation.IsTypePresent or a global field/property. This
        ' would have almost no false positives (except in case of GoTo and Do/Loop Until cases)
        ' but I think has too many false negatives.
        '
        ' Adopted heuristic: enforce the coding style that you should keep things
        ' simple. You must either have this call to xyz.f() or a Return statement inside the
        ' positive branch of an appropriately-conditioned "If" block.

        If IsWellGuarded(invocationExpression, context.SemanticModel) Then Return
        For Each ret In containingMethod.DescendantNodes.OfType(Of ReturnStatementSyntax)
            If IsWellGuarded(ret, context.SemanticModel) Then Return
        Next

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation))
    End Sub

    Shared Function HasPlatformSpecificAttribute(symbol As ISymbol) As Boolean
        For Each attr In symbol.GetAttributes
            If attr.AttributeClass.ToDisplayString() = GetType(PlatformSpecificAttribute).FullName Then Return True
        Next
        Return False
    End Function

    Function IsWellGuarded(node As SyntaxNode, semanticModel As SemanticModel) As Boolean
        For Each symbol In GetGuards(node, semanticModel)
            If symbol.ContainingType?.Name = "ApiInformation" Then Return True
            If HasPlatformSpecificAttribute(symbol) Then Return True
        Next
        Return False
    End Function

    Shared Iterator Function GetGuards(node As SyntaxNode, semanticModel As SemanticModel) As IEnumerable(Of ISymbol)
        For Each condition In GetConditions(node)

            ' First check for invocations of ApiInformation.IsTypePresent
            For Each invocation In condition.DescendantNodesAndSelf.OfType(Of InvocationExpressionSyntax)
                Dim targetMethod = semanticModel.GetSymbolInfo(invocation).Symbol
                If targetMethod?.ContainingType?.Name = "ApiInformation" Then Yield targetMethod
            Next

            ' Next check for any property/field access
            Dim accesses1 = condition.DescendantNodesAndSelf.OfType(Of MemberAccessExpressionSyntax).Select(Function(n) semanticModel.GetSymbolInfo(n).Symbol)
            Dim accesses2 = condition.DescendantNodesAndSelf.OfType(Of IdentifierNameSyntax).Select(Function(n) semanticModel.GetSymbolInfo(n).Symbol)
            For Each symbol In accesses1.Concat(accesses2)
                If symbol?.Kind = SymbolKind.Field OrElse symbol?.Kind = SymbolKind.Property Then Yield symbol
            Next
        Next
    End Function

    Shared Iterator Function GetConditions(node As SyntaxNode) As IEnumerable(Of ExpressionSyntax)
        Dim check1 = node.FirstAncestorOrSelf(Of MultiLineIfBlockSyntax)
        While check1 IsNot Nothing
            Yield check1.IfStatement.Condition
            check1 = check1.Parent.FirstAncestorOrSelf(Of MultiLineIfBlockSyntax)
        End While
        '
        Dim check2 = node.FirstAncestorOrSelf(Of SingleLineIfStatementSyntax)
        While check2 IsNot Nothing
            Yield check2.Condition
            check2 = check2.Parent.FirstAncestorOrSelf(Of SingleLineIfStatementSyntax)
        End While
    End Function

End Class




<ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
Public Class PlatformSpecificFixerVB
    Inherits CodeFixProvider

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create("UWP001")
        End Get
    End Property

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
        Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

        Dim diagnostic = context.Diagnostics.First
        Dim span = diagnostic.Location.SourceSpan
        Dim invocationExpression = root.FindToken(span.Start).Parent.AncestorsAndSelf().OfType(Of InvocationExpressionSyntax)().First

        ' Introduce a guard? (only if it is a method, i.e. somewhere that allows code)
        ' Mark method as platform-specific?
        Dim containingMember = invocationExpression.FirstAncestorOrSelf(Of MethodBlockBaseSyntax)
        Dim containingMethod = TryCast(containingMember, MethodBlockSyntax)
        If containingMethod IsNot Nothing Then
            Dim act1 = CodeAction.Create("Add 'If ApiInformation.IsTypePresent'", Function(c) IntroduceGuardAsync(context.Document, invocationExpression, c))
            context.RegisterCodeFix(act1, diagnostic)
            Dim methodName = If(containingMethod.Kind = SyntaxKind.SubBlock, "Sub", "Function")
            methodName &= " " & containingMethod.SubOrFunctionStatement.Identifier.Text
            Dim act2 = CodeAction.Create($"Mark '{methodName}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(containingMethod.SubOrFunctionStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c))
            context.RegisterCodeFix(act2, diagnostic)
        End If

        ' Mark the type as platform-specific?
        Dim containingType = containingMember.FirstAncestorOrSelf(Of ClassBlockSyntax)
        If containingType IsNot Nothing Then
            Dim className = "Class " & containingType.ClassStatement.Identifier.Text
            Dim act3 = CodeAction.Create($"Mark '{className}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(containingType.ClassStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c))
            context.RegisterCodeFix(act3, diagnostic)
        End If

        ' Mark some of the conditions as platform-specific?
        Dim semanticModel = Await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(False)
        For Each symbol In PlatformSpecificAnalyzerVB.GetGuards(invocationExpression, semanticModel)
            If symbol.ContainingType.Name = "ApiInformation" Then Continue For
            If Not symbol.Locations.First.IsInSource Then Return
            Dim act4 As CodeAction = Nothing
            Dim symbolSyntax = Await symbol.DeclaringSyntaxReferences.First.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(False)
            Dim fieldSyntax = TryCast(symbolSyntax.Parent.Parent, FieldDeclarationSyntax)
            Dim propSyntax = TryCast(symbolSyntax, PropertyStatementSyntax)
            If fieldSyntax IsNot Nothing Then
                act4 = CodeAction.Create($"Mark field '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(fieldSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c))
            ElseIf propSyntax IsNot Nothing Then
                act4 = CodeAction.Create($"Mark property '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c))
            End If
            If act4 IsNot Nothing Then context.RegisterCodeFix(act4, diagnostic)
        Next
    End Function

    Private Async Function AddPlatformSpecificAttributeAsync(Of T As SyntaxNode)(oldSyntax As T, f As Func(Of T, AttributeListSyntax, T), solution As Solution, cancellationToken As CancellationToken) As Task(Of Solution)
        ' + <System.Runtime.CompilerServices.PlatformSpecific>
        '   Sub/Class/Dim/Property p

        Dim oldRoot = Await oldSyntax.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(False)
        Dim temp As SyntaxNode = Nothing
        Dim oldDocument = (From p In solution.Projects From d In p.Documents Where d.TryGetSyntaxRoot(temp) AndAlso temp Is oldRoot Select d).First
        '
        Dim id = SyntaxFactory.ParseTypeName("System.Runtime.CompilerServices.PlatformSpecific")
        Dim attr = SyntaxFactory.Attribute(id)
        Dim attrs = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)).WithAdditionalAnnotations(Simplifier.Annotation)
        '
        Dim newSyntax = f(oldSyntax, attrs)
        Dim newRoot = oldRoot.ReplaceNode(oldSyntax, newSyntax)
        Return solution.WithDocumentSyntaxRoot(oldDocument.Id, newRoot)
    End Function

    Private Async Function IntroduceGuardAsync(document As Document, invocationExpression As InvocationExpressionSyntax, cancellationToken As CancellationToken) As Task(Of Document)
        ' + If Windows.Foundation.Metadata.ApiInformation.IsTypePresent(targetContainingType) Then
        '       old-statement
        ' + End If

        Try
            Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
            Dim targetMethod = semanticModel.GetSymbolInfo(invocationExpression).Symbol
            If targetMethod Is Nothing Then Return document
            Dim targetContainingType = targetMethod.ContainingType.ToDisplayString()

            Dim oldStatement = invocationExpression.FirstAncestorOrSelf(Of StatementSyntax)()

            Dim isTypePresent = SyntaxFactory.ParseName("Windows.Foundation.Metadata.ApiInformation.IsTypePresent").WithAdditionalAnnotations(Simplifier.Annotation)
            Dim tok = SyntaxFactory.StringLiteralToken($"""{targetContainingType}""", targetContainingType)
            Dim typeName = SyntaxFactory.StringLiteralExpression(tok)
            Dim typeNameArg As ArgumentSyntax = SyntaxFactory.SimpleArgument(typeName)
            Dim typeNameArgs = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(typeNameArg))
            Dim invoke = SyntaxFactory.InvocationExpression(isTypePresent, typeNameArgs)
            Dim ifStatement = SyntaxFactory.IfStatement(invoke)
            Dim endIfStatement = SyntaxFactory.EndIfStatement()
            Dim oldLeadingTrivia = oldStatement.GetLeadingTrivia()
            Dim stmts = SyntaxFactory.SingletonList(oldStatement.WithoutLeadingTrivia())
            Dim ifBlock = SyntaxFactory.MultiLineIfBlock(ifStatement).WithStatements(stmts).WithLeadingTrivia(oldLeadingTrivia).WithAdditionalAnnotations(Formatter.Annotation)

            Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
            Dim newRoot = oldRoot.ReplaceNode(oldStatement, ifBlock)
            Return document.WithSyntaxRoot(newRoot)

            Await Task.Delay(0)
            Return document
        Catch ex As Exception
            Stop
            Return document
        End Try
        '' Compute new uppercase name.
        'Dim identifierToken = typeStmt.Identifier
        'Dim newName = identifierToken.Text.ToUpperInvariant()

        '' Get the symbol representing the type to be renamed.

        '' Produce a new solution that has all references to that type renamed, including the declaration.
        'Dim originalSolution = document.Project.Solution
        'Dim optionSet = originalSolution.Workspace.Options
        'Dim newSolution = Await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(False)

        '' Return the new solution with the now-uppercase type name.
        'Return newSolution
    End Function

    Private Shared ReadOnly Property StatementKinds As ImmutableArray(Of SyntaxKind)
        Get
            Static Dim v As ImmutableArray(Of SyntaxKind) =
                Function()
                    Dim b = ImmutableArray.CreateBuilder(Of SyntaxKind)
                    For i = CType(0, SyntaxKind) To CType(800, SyntaxKind)
                        If i.ToString().EndsWith("Statement") Then b.Add(i)
                    Next
                    Return b.ToImmutableArray()
                End Function()
            Return v
        End Get
    End Property
End Class