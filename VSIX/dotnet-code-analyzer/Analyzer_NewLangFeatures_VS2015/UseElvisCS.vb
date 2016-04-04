Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax

' UseElvis.
'
' The "null-checking" operator ?. is new in C#6 And VB.
' It Is called the Elvis operator because it looks Like two eyes And a quiff.
'
' This analyzer looks for the common event-raising pattern in C#
'    var e = EventName; if (e!=null) e(args)
' And replaces it with the more readable And succinct version using the ?. operator
'    EventName?.Invoke(args)
'
' The same analyzer isn't needed in VB, since the keyword RaiseEvent already does that check.
'
' NOTE: I find it hard to generalize this, to find other cases where an analyzer
' can usefully advise you to use ?.


<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class UseElvisAnalyzerCS
    Inherits DiagnosticAnalyzer

    Friend Shared Rule As New DiagnosticDescriptor("LANG002", "Use ?. operator", "The new ?. operator is an easier way to check for null.", "Language", DiagnosticSeverity.Warning, isEnabledByDefault:=True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeNode, SyntaxKind.IfStatement)

    End Sub

    <Contracts.Pure>
    Public Sub AnalyzeNode(ctx As SyntaxNodeAnalysisContext)
        ' 1. Check for "... if (e != null) ..."
        Dim ifStatement = TryCast(ctx.Node, IfStatementSyntax)
        Dim notNull = TryCast(ifStatement.Condition, BinaryExpressionSyntax)
        If notNull Is Nothing OrElse Not notNull.IsKind(SyntaxKind.NotEqualsExpression) Then Return
        Dim notNullIdentifier = TryCast(notNull.Left, IdentifierNameSyntax)
        If notNullIdentifier Is Nothing Then Return
        Dim notNullValue = TryCast(notNull.Right, LiteralExpressionSyntax)
        If notNullValue Is Nothing OrElse Not notNullValue.IsKind(SyntaxKind.NullLiteralExpression) Then Return

        ' 2. Check for "... ... e(args)"
        Dim exprStatement = TryCast(ifStatement.Statement, ExpressionStatementSyntax)
        If exprStatement Is Nothing Then Return
        Dim invocation = TryCast(exprStatement.Expression, InvocationExpressionSyntax)
        If invocation Is Nothing Then Return
        Dim invocationIdentifier = TryCast(invocation.Expression, IdentifierNameSyntax)
        If invocationIdentifier Is Nothing Then Return
        If invocationIdentifier.Identifier.ValueText <> notNullIdentifier.Identifier.ValueText Then Return

        ' 3. Check for "var e = expr ... ..."
        Dim peers = ifStatement.Parent.ChildNodes().ToList
        Dim ifStatementIndex = peers.IndexOf(ifStatement)
        If ifStatementIndex = -1 Then Return
        Dim declarationStatement = TryCast(peers(ifStatementIndex - 1), LocalDeclarationStatementSyntax)
        If declarationStatement Is Nothing Then Return
        If declarationStatement.Declaration.Variables.Count <> 1 Then Return
        Dim variableDeclaration = declarationStatement.Declaration.Variables.Single
        If variableDeclaration.Identifier.ValueText <> notNullIdentifier.Identifier.ValueText Then Return

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, notNull.GetLocation()))
    End Sub

End Class

<ExportCodeFixProvider("UseElvisFixer", LanguageNames.CSharp), [Shared]>
Public Class UseElvisFixerCS : Inherits CodeFixProvider

    Public Overrides Function GetFixableDiagnosticIds() As ImmutableArray(Of String)
        Return ImmutableArray.Create("LANG002")
    End Function

    Public Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public Overrides Async Function ComputeFixesAsync(ctx As CodeFixContext) As Task
        Await Task.Delay(0)
        Dim diagnostic = ctx.Diagnostics.First
        Dim span = diagnostic.Location.SourceSpan
        Dim act = CodeAction.Create("Use ?.", Function(c) UseElvisAsync(ctx.Document, span, c))
        ctx.RegisterFix(act, diagnostic)
    End Function

    Private Async Function UseElvisAsync(document As Document, diagnosticSpan As TextSpan, cancellationToken As CancellationToken) As Task(Of Document)
        Dim root = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim SemanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)

        Dim ifStatement = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf(Of IfStatementSyntax)
        Dim exprStatement = TryCast(ifStatement.Statement, ExpressionStatementSyntax)
        Dim invocation = TryCast(exprStatement.Expression, InvocationExpressionSyntax)
        Dim peers = ifStatement.Parent.ChildNodes().ToList
        Dim ifStatementIndex = peers.IndexOf(ifStatement)
        Dim declarationStatement = TryCast(peers(ifStatementIndex - 1), LocalDeclarationStatementSyntax)
        Dim variableDeclaration = declarationStatement.Declaration.Variables.Single
        Dim initializer = variableDeclaration.Initializer.Value

        Dim binding2 = SyntaxFactory.MemberBindingExpression(TryCast(SyntaxFactory.ParseName("Invoke"), SimpleNameSyntax))
        Dim invocation2 = SyntaxFactory.InvocationExpression(binding2, invocation.ArgumentList)
        Dim elvis2 = SyntaxFactory.ConditionalAccessExpression(initializer, invocation2)
        Dim statement2 = SyntaxFactory.ExpressionStatement(elvis2).NormalizeWhitespace().WithLeadingTrivia(declarationStatement.GetLeadingTrivia())

        Dim newRoot = root.TrackNodes(declarationStatement, ifStatement)
        newRoot = newRoot.RemoveNode(newRoot.GetCurrentNode(ifStatement), SyntaxRemoveOptions.KeepEndOfLine Or SyntaxRemoveOptions.KeepTrailingTrivia)
        newRoot = newRoot.ReplaceNode(newRoot.GetCurrentNode(declarationStatement), CType(statement2, SyntaxNode))

        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class
