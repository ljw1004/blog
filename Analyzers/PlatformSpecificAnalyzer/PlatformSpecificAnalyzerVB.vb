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
' Set "PlatformSpecificAnalyzer" as your startup project. Then under MyProject > Debug, set
' StartAction: external program
'     C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
' Command-line arguments: 
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_VB\DemoUWP_VB.sln" /RootSuffix Analyzer




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
        ' context.RegisterSyntaxNodeAction(AddressOf AnalyzeInvocation)
        ' This would be simplest. It just generates multiple diagnostics per line
        ' However, until bug https: //github.com/dotnet/roslyn/issues/3311 in Roslyn is fixed,
        ' it also gives duplicate "Supress" codefixes.
        ' So until then, we'll do work to generate only a single diagnostic per line:
        context.RegisterCodeBlockStartAction(Of SyntaxKind)(AddressOf AnalyzeCodeBlockStart)
    End Sub

    Public Sub AnalyzeCodeBlockStart(context As CodeBlockStartAnalysisContext(Of SyntaxKind))
        Dim reports As New Dictionary(Of Integer, Location)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeInvocation(c, reports), SyntaxKind.InvocationExpression)
        context.RegisterCodeBlockEndAction(
            Sub(c)
                For Each span In reports.Values
                    c.ReportDiagnostic(Diagnostic.Create(Rule, span))
                Next
            End Sub)
    End Sub

    Public Sub AnalyzeInvocation(context As SyntaxNodeAnalysisContext, reports As Dictionary(Of Integer, Location))
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
        ' HACK: I don't want to give warnings for 8.1 or PCL code. In those two targets, every Windows
        ' type is found in Windows.winmd, so that's how we'll prevent it:
        If targetAssembly = "Windows" Then Return


        ' Is this invocation outside a method?
        Dim containingMember = invocationExpression.FirstAncestorOrSelf(Of MethodBlockBaseSyntax)
        Dim containingMethod = TryCast(containingMember, MethodBlockSyntax)
        If containingMethod Is Nothing Then Return ' to consider: should we report anything here?

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

        ' Is this invocation properly guarded? See readme.txt for explanations.
        If IsProperlyGuarded(invocationExpression, context.SemanticModel) Then Return
        For Each ret In containingMethod.DescendantNodes.OfType(Of ReturnStatementSyntax)
            If IsProperlyGuarded(ret, context.SemanticModel) Then Return
        Next

        ' We'll report only a single diagnostic per line, the first.
        Dim loc = context.Node.GetLocation
        If Not loc.IsInSource Then Return
        Dim line = loc.GetLineSpan().StartLinePosition.Line
        If reports.ContainsKey(line) AndAlso reports(line).SourceSpan.Start <= loc.SourceSpan.Start Then Return
        reports(line) = loc
    End Sub

    Shared Function HasPlatformSpecificAttribute(symbol As ISymbol) As Boolean
        For Each attr In symbol.GetAttributes
            If attr.AttributeClass.Name.EndsWith("SpecificAttribute") Then Return True
        Next
        Return False
    End Function

    Function IsProperlyGuarded(node As SyntaxNode, semanticModel As SemanticModel) As Boolean
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
            Dim act1 = CodeAction.Create("Add 'If ApiInformation.IsTypePresent'", Function(c) IntroduceGuardAsync(context.Document, invocationExpression, c), "PlatformSpecificGuard")
            context.RegisterCodeFix(act1, diagnostic)
            Dim methodName = If(containingMethod.Kind = SyntaxKind.SubBlock, "Sub", "Function")
            methodName &= " " & containingMethod.SubOrFunctionStatement.Identifier.Text
            Dim act2 = CodeAction.Create($"Mark '{methodName}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(containingMethod.SubOrFunctionStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        End If

        ' Mark the type as platform-specific?
        Dim containingType = containingMember.FirstAncestorOrSelf(Of ClassBlockSyntax)
        If containingType IsNot Nothing Then
            Dim className = "Class " & containingType.ClassStatement.Identifier.Text
            Dim act3 = CodeAction.Create($"Mark '{className}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(containingType.ClassStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificClass")
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
                act4 = CodeAction.Create($"Mark field '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(fieldSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
            ElseIf propSyntax IsNot Nothing Then
                act4 = CodeAction.Create($"Mark property '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
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

        Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim targetMethod = semanticModel.GetSymbolInfo(invocationExpression).Symbol
        If targetMethod Is Nothing Then Return document
        Dim targetContainingType = targetMethod.ContainingType.ToDisplayString()

        Dim oldStatement = invocationExpression.FirstAncestorOrSelf(Of StatementSyntax)()
        Dim oldLeadingTrivia = oldStatement.GetLeadingTrivia()
        '
        Dim conditionReceiver = SyntaxFactory.ParseName("Windows.Foundation.Metadata.ApiInformation.IsTypePresent").WithAdditionalAnnotations(Simplifier.Annotation)
        Dim conditionString = SyntaxFactory.StringLiteralExpression(SyntaxFactory.StringLiteralToken($"""{targetContainingType}""", targetContainingType))
        Dim conditionArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(SyntaxFactory.SimpleArgument(conditionString)))
        Dim condition = SyntaxFactory.InvocationExpression(conditionReceiver, conditionArgument)
        '
        Dim ifStatement = SyntaxFactory.IfStatement(condition)
        Dim thenStatements = SyntaxFactory.SingletonList(oldStatement.WithoutLeadingTrivia())
        Dim ifBlock = SyntaxFactory.MultiLineIfBlock(ifStatement).WithStatements(thenStatements).WithLeadingTrivia(oldLeadingTrivia).WithAdditionalAnnotations(Formatter.Annotation)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim newRoot = oldRoot.ReplaceNode(oldStatement, ifBlock)
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class