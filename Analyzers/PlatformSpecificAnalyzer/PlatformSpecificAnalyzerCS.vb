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
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax

' FOR EASIER F5 DEBUGGING OF THIS ANALYZER:
' Set "PlatformSpecificAnalyzer" as your startup project. Then under MyProject > Debug, set
' StartAction: external program
'     C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
' Command-line arguments: 
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_CS\DemoUWP_CS.sln" /RootSuffix Analyzer




<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class PlatformSpecificAnalyzerCS
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
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.IdentifierName)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.SimpleMemberAccessExpression)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.QualifiedName)
        context.RegisterCodeBlockEndAction(
            Sub(c)
                For Each span In reports.Values
                    c.ReportDiagnostic(Diagnostic.Create(Rule, span))
                Next
            End Sub)
    End Sub

    Public Sub AnalyzeExpression(context As SyntaxNodeAnalysisContext, reports As Dictionary(Of Integer, Location))
        If context.Node.Parent.Kind = SyntaxKind.SimpleMemberAccessExpression Then Return ' will be handled at higher level
        If context.Node.Parent.Kind = SyntaxKind.QualifiedName Then Return

        If context.Node.Parent.Kind = SyntaxKind.InvocationExpression AndAlso context.Node Is CType(context.Node.Parent, InvocationExpressionSyntax).Expression Then
            ' <target>(...)
            Dim invocationExpression = CType(context.Node.Parent, InvocationExpressionSyntax)
            Dim target = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol ' points to the method after overload resolution
            If Not PlatformSpecificAnalyzerVB.IsTargetPlatformSpecific(target) Then Return
        ElseIf context.Node.Parent.Kind = SyntaxKind.ObjectCreationExpression AndAlso context.Node Is CType(context.Node.Parent, ObjectCreationExpressionSyntax).Type Then
            ' New <target>
            Dim objectCreationExpression = CType(context.Node.Parent, ObjectCreationExpressionSyntax)
            Dim target = context.SemanticModel.GetSymbolInfo(objectCreationExpression).Symbol ' points to the constructor after overload resolution
            If Not PlatformSpecificAnalyzerVB.IsTargetPlatformSpecific(target) Then Return
        Else
            ' f<target>(...)
            ' <target> x = ...
            ' Action x = <target>  -- note that following code does pick the right overload
            ' <target> += delegate -- the following code does recognize events
            ' nameof(<target>) -- this should really be allowed, but I can't be bothered
            ' Note that all FIELD ACCESS is allowed to platform-specific fields.
            Dim target = context.SemanticModel.GetSymbolInfo(context.Node).Symbol
            If target Is Nothing Then Return
            If target.Kind <> SymbolKind.Method AndAlso target.Kind <> SymbolKind.Event AndAlso target.Kind <> SymbolKind.Property Then Return
            If Not PlatformSpecificAnalyzerVB.IsTargetPlatformSpecific(target) Then Return
        End If


        ' Is this expression inside a method/constructor/property that claims to be platform-specific?
        Dim containingBlock = context.Node.FirstAncestorOrSelf(Of BlockSyntax)
        Dim containingMember As MemberDeclarationSyntax = containingBlock?.FirstAncestorOrSelf(Of BaseMethodDeclarationSyntax) ' for constructors and methods
        If containingBlock Is Nothing OrElse TypeOf containingBlock?.Parent Is AccessorDeclarationSyntax Then containingMember = context.Node.FirstAncestorOrSelf(Of PropertyDeclarationSyntax)
        If containingMember IsNot Nothing Then
            Dim containingMemberSymbol = context.SemanticModel.GetDeclaredSymbol(containingMember)
            If PlatformSpecificAnalyzerVB.HasPlatformSpecificAttribute(containingMemberSymbol) Then Return
        End If


        ' Is this invocation properly guarded? See readme.txt for explanations.
        If IsProperlyGuarded(context.Node, context.SemanticModel) Then Return
        If containingBlock IsNot Nothing Then
            For Each ret In containingBlock.DescendantNodes.OfType(Of ReturnStatementSyntax)
                If IsProperlyGuarded(ret, context.SemanticModel) Then Return
            Next
        End If

        ' We'll report only a single diagnostic per line, the first.
        Dim loc = context.Node.GetLocation
        If Not loc.IsInSource Then Return
        Dim line = loc.GetLineSpan().StartLinePosition.Line
        If reports.ContainsKey(line) AndAlso reports(line).SourceSpan.Start <= loc.SourceSpan.Start Then Return
        reports(line) = loc
    End Sub

    Function IsProperlyGuarded(node As SyntaxNode, semanticModel As SemanticModel) As Boolean
        For Each symbol In GetGuards(node, semanticModel)
            If symbol.ContainingType?.Name = "ApiInformation" Then Return True
            If PlatformSpecificAnalyzerVB.HasPlatformSpecificAttribute(symbol) Then Return True
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
        Dim check = node.FirstAncestorOrSelf(Of IfStatementSyntax)
        While check IsNot Nothing
            Yield check.Condition
            check = check.Parent.FirstAncestorOrSelf(Of IfStatementSyntax)
        End While
    End Function

End Class




<ExportCodeFixProvider(LanguageNames.CSharp), [Shared]>
Public Class PlatformSpecificFixerCS
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
        Dim node = root.FindToken(span.Start).Parent

        ' Which node are we interested in? -- the largest IdentifierName/SimpleMemberAccess/QualifiedName
        ' that encompasses the bottom node...
        While node.Kind <> SyntaxKind.IdentifierName AndAlso node.Kind <> SyntaxKind.SimpleMemberAccessExpression AndAlso node.Kind <> SyntaxKind.QualifiedName
            node = node.Parent
            If node Is Nothing Then Return
        End While
        While True
            If node.Parent?.Kind = SyntaxKind.SimpleMemberAccessExpression Then node = node.Parent : Continue While
            If node.Parent?.Kind = SyntaxKind.QualifiedName Then node = node.Parent : Continue While
            Exit While
        End While


        ' Introduce a guard? (only if it is a method/accessor/constructor, i.e. somewhere that allows code)
        Dim containingBlock = node.FirstAncestorOrSelf(Of BlockSyntax)
        If containingBlock IsNot Nothing Then
            Dim act1 = CodeAction.Create("Add 'If ApiInformation.IsTypePresent'", Function(c) IntroduceGuardAsync(context.Document, node, c), "PlatformSpecificGuard")
            context.RegisterCodeFix(act1, diagnostic)
        End If


        ' Mark method/property/constructor as platform-specific?
        If containingBlock Is Nothing OrElse TypeOf containingBlock.Parent Is AccessorDeclarationSyntax Then
            Dim propDeclaration = node.FirstAncestorOrSelf(Of PropertyDeclarationSyntax)
            If propDeclaration Is Nothing Then Return
            Dim name = $"property '{propDeclaration.Identifier.Text}'"
            Dim act2 = CodeAction.Create($"Mark {name} as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        ElseIf containingBlock.Parent.Kind = SyntaxKind.MethodDeclaration Then
            Dim methodDeclaration = containingBlock.FirstAncestorOrSelf(Of MethodDeclarationSyntax)
            Dim name = $"method '{methodDeclaration.Identifier.Text}'"
            Dim act2 = CodeAction.Create($"Mark {name} as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(methodDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        ElseIf containingBlock.Parent.Kind = SyntaxKind.ConstructorDeclaration Then
            Dim methodDeclaration = containingBlock.FirstAncestorOrSelf(Of ConstructorDeclarationSyntax)
            Dim name = $"constructor '{methodDeclaration.Identifier.Text}'"
            Dim act2 = CodeAction.Create($"Mark {name} as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(methodDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        End If


        ' Mark some of the conditions as platform-specific?
        Dim semanticModel = Await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(False)
        For Each symbol In PlatformSpecificAnalyzerCS.GetGuards(node, semanticModel)
            If symbol.ContainingType.Name = "ApiInformation" Then Continue For
            If Not symbol.Locations.First.IsInSource Then Return
            Dim act4 As CodeAction = Nothing
            Dim symbolSyntax = Await symbol.DeclaringSyntaxReferences.First.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(False)
            Dim fieldSyntax = TryCast(symbolSyntax.Parent.Parent, FieldDeclarationSyntax)
            Dim propSyntax = TryCast(symbolSyntax, PropertyDeclarationSyntax)
            If fieldSyntax IsNot Nothing Then
                act4 = CodeAction.Create($"Mark field '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(fieldSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
            ElseIf propSyntax IsNot Nothing Then
                act4 = CodeAction.Create($"Mark property '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
            End If
            If act4 IsNot Nothing Then context.RegisterCodeFix(act4, diagnostic)
        Next
    End Function

    Private Async Function AddPlatformSpecificAttributeAsync(Of T As SyntaxNode)(oldSyntax As T, f As Func(Of T, AttributeListSyntax, T), solution As Solution, cancellationToken As CancellationToken) As Task(Of Solution)
        ' + [System.Runtime.CompilerServices.PlatformSpecific]
        '   type p // method/class/field/property

        Dim oldRoot = Await oldSyntax.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(False)
        Dim temp As SyntaxNode = Nothing
        Dim oldDocument = (From p In solution.Projects From d In p.Documents Where d.TryGetSyntaxRoot(temp) AndAlso temp Is oldRoot Select d).First
        '
        Dim id = SyntaxFactory.ParseName("System.Runtime.CompilerServices.PlatformSpecific")
        Dim attr = SyntaxFactory.Attribute(id)
        Dim attrs = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)).WithAdditionalAnnotations(Simplifier.Annotation)
        '
        Dim newSyntax = f(oldSyntax, attrs)
        Dim newRoot = oldRoot.ReplaceNode(oldSyntax, newSyntax)
        Return solution.WithDocumentSyntaxRoot(oldDocument.Id, newRoot)
    End Function

    Private Async Function IntroduceGuardAsync(document As Document, node As SyntaxNode, cancellationToken As CancellationToken) As Task(Of Document)
        ' + if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(targetContainingType)) {
        '       old-statement
        ' + }

        Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim target = semanticModel.GetSymbolInfo(node).Symbol
        If target Is Nothing Then Return document
        Dim targetType = If(target.Kind = SymbolKind.NamedType, target, target.ContainingType)
        Dim targetName = If(targetType Is Nothing, "", targetType.ToDisplayString)
        If Not targetName.StartsWith("Windows.") Then targetName = "???"

        Dim oldStatement = node.FirstAncestorOrSelf(Of StatementSyntax)()
        Dim oldLeadingTrivia = oldStatement.GetLeadingTrivia()
        '
        Dim conditionReceiver = SyntaxFactory.ParseName("Windows.Foundation.Metadata.ApiInformation.IsTypePresent").WithAdditionalAnnotations(Simplifier.Annotation)
        Dim conditionString = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(targetName))
        Dim conditionArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(SyntaxFactory.Argument(conditionString)))
        Dim condition = SyntaxFactory.InvocationExpression(conditionReceiver, conditionArgument)
        '
        Dim thenStatements = SyntaxFactory.Block(oldStatement.WithoutLeadingTrivia())
        Dim ifStatement = SyntaxFactory.IfStatement(condition, thenStatements).WithLeadingTrivia(oldLeadingTrivia).WithAdditionalAnnotations(Formatter.Annotation)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim newRoot = oldRoot.ReplaceNode(oldStatement, ifStatement)
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class
