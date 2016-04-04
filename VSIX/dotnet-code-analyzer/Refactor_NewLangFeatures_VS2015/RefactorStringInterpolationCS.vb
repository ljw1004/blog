Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax

<ExportCodeRefactoringProvider("RefactorIntoStringInterpolationCS", LanguageNames.CSharp), [Shared]>
Class RefactorIntoStringInterpolationCS
    Inherits CodeRefactoringProvider

    Public Overrides Async Function ComputeRefactoringsAsync(context As CodeRefactoringContext) As Task
        Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)
        Dim node = root.FindNode(context.Span)

        ' Only offer a refactoring if the selected node is within a call to Console.WriteLine
        Dim invoke = node.FirstAncestorOrSelf(Of InvocationExpressionSyntax)
        If invoke Is Nothing Then Return
        Dim target = TryCast(invoke.Expression, MemberAccessExpressionSyntax)
        If target Is Nothing Then Return
        If target.Name?.ToString() <> "WriteLine" Then Return
        If invoke.ArgumentList?.Arguments.Count < 2 Then Return
        Dim arg0 = TryCast(invoke.ArgumentList.Arguments(0).Expression, LiteralExpressionSyntax)
        If arg0 Is Nothing OrElse Not arg0.IsKind(SyntaxKind.StringLiteralExpression) Then Return
        Dim fmtstring = arg0.Token.ValueText
        If Not fmtstring.Contains("{0}") Then Return

        Dim action = CodeAction.Create("Use string interpolation", Function(c) UseStringInterpolationAsync(context.Document, invoke, fmtstring, c))
        context.RegisterRefactoring(action)
    End Function

    Async Function UseStringInterpolationAsync(document As Document, invoke As InvocationExpressionSyntax, fmtstring As String, cancel As CancellationToken) As Task(Of Document)
        Dim args = invoke.ArgumentList.Arguments

        Dim separators = fmtstring.Split({"{0}", "{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}", "{9}"}, StringSplitOptions.None).ToList
        For i = 0 To separators.Count - 1
            If i = 0 Then separators(i) = """" & separators(i)
            If i > 0 Then separators(i) = "}" & separators(i)
            If i + 1 < separators.Count Then separators(i) &= "\{"
            If i + 1 = separators.Count Then separators(i) &= """"
        Next
        Dim firstSeparator = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringStartToken, separators.First(), separators.First(), SyntaxTriviaList.Empty)
        Dim lastSeparator = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringEndToken, separators.Last(), separators.Last(), SyntaxTriviaList.Empty)
        separators.RemoveAt(separators.Count - 1)
        separators.RemoveAt(0)
        Dim intseps = (From s In separators Select SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringMidToken, s, s, SyntaxTriviaList.Empty)).ToList
        Dim intargs = (From e In args Skip 1 Select SyntaxFactory.InterpolatedStringInsert(e.Expression.WithoutLeadingTrivia().WithoutTrailingTrivia())).ToList

        Dim intseplist = SyntaxFactory.SeparatedList(intargs, intseps)
        Dim intstring = SyntaxFactory.InterpolatedString(firstSeparator, intseplist, lastSeparator)
        Dim newintarg = SyntaxFactory.Argument(intstring)
        Dim newintargslist = SyntaxFactory.SingletonSeparatedList(newintarg)
        Dim newintarglist = invoke.ArgumentList.WithArguments(newintargslist)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancel)
        Dim newRoot = oldRoot.ReplaceNode(invoke.ArgumentList, newintarglist)
        Dim newDoc = document.WithSyntaxRoot(newRoot)

        Return newDoc
    End Function


End Class

