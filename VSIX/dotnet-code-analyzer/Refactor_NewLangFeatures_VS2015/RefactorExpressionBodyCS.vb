Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax
Imports Microsoft.CodeAnalysis.Formatting

<ExportCodeRefactoringProvider("RefactorExpressionBodyCS", LanguageNames.CSharp), [Shared]>
Class RefactorExpressionBodyCS
    Inherits CodeRefactoringProvider

    Public Overrides Async Function ComputeRefactoringsAsync(context As CodeRefactoringContext) As Task
        Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)
        Dim node = root.FindNode(context.Span)

        ' Only offer a refactoring if the selected node is within a method with a single return statement
        Dim method = node.FirstAncestorOrSelf(Of MethodDeclarationSyntax)
        If method Is Nothing OrElse method.Body Is Nothing OrElse method.ExpressionBody IsNot Nothing Then Return
        If method.Body.ChildNodes.Count <> 1 Then Return
        Dim returnStatement = TryCast(method.Body.ChildNodes(0), ReturnStatementSyntax)
        If returnStatement Is Nothing OrElse returnStatement.Expression Is Nothing Then Return

        Dim action = CodeAction.Create("Use expression-bodied member", Function(c) UseExpressionBodyAsync(context.Document, returnStatement, c))
        context.RegisterRefactoring(action)
    End Function

    Async Function UseExpressionBodyAsync(document As Document, returnStatement As ReturnStatementSyntax, cancel As CancellationToken) As Task(Of Document)
        Dim method = returnStatement.FirstAncestorOrSelf(Of MethodDeclarationSyntax)
        Dim newBody = SyntaxFactory.ArrowExpressionClause(returnStatement.Expression)
        Dim newMethod = method.WithBody(Nothing).WithExpressionBody(newBody).
            WithSemicolonToken(returnStatement.SemicolonToken).
            WithTrailingTrivia(returnStatement.GetTrailingTrivia).
            WithAdditionalAnnotations(Formatter.Annotation)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancel)
        Dim newRoot = oldRoot.ReplaceNode(method, newMethod)
        Return document.WithSyntaxRoot(newRoot)
    End Function


End Class

