Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.Formatting

' TODO: Also allow "NameOf" as a constant


<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class UseNameofAnalyzerVB
    Inherits DiagnosticAnalyzer

    Friend Shared Rule As New DiagnosticDescriptor("XT001", "Create PropertyChangedEventArgs statically", "Improve XAML perf by allocating PropertyChangedEventArgs just once, statically.", "XAML", DiagnosticSeverity.Warning, isEnabledByDefault:=True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeNode, SyntaxKind.ObjectCreationExpression)
    End Sub

    Public Sub AnalyzeNode(ctx As SyntaxNodeAnalysisContext)
        Dim objectCreation = TryCast(ctx.Node, ObjectCreationExpressionSyntax)
        Dim objectCreationType = TryCast(objectCreation.Type, IdentifierNameSyntax)
        If objectCreationType Is Nothing OrElse objectCreationType.Identifier.ValueText <> "PropertyChangedEventArgs" Then Return
        If objectCreation.Parent.IsKind(SyntaxKind.AsNewClause) Then Return ' low-hanging fruit: can't be bothered to cope with this
        If objectCreation.ArgumentList Is Nothing OrElse objectCreation.ArgumentList.Arguments.Count <> 1 Then Return
        Dim argumentHolder As ArgumentSyntax = objectCreation.ArgumentList.Arguments(0)
        If Not argumentHolder.IsKind(SyntaxKind.SimpleArgument) Then Return
        Dim argument = CType(argumentHolder, SimpleArgumentSyntax).Expression
        If argument.IsKind(SyntaxKind.StringLiteralExpression) Then
            ' okay
        Else
            ' TODO: also allow NameOf
            Return
        End If

        Dim containingMember = objectCreationType.FirstAncestorOrSelf(Of DeclarationStatementSyntax)
        Dim containingMemberSymbol = ctx.SemanticModel.GetDeclaredSymbol(containingMember)
        If containingMemberSymbol IsNot Nothing Then
            If containingMemberSymbol.IsShared() Then Return ' IsShared already factors in whether it's in a module
        ElseIf containingMember.IsKind(SyntaxKind.FieldDeclaration) Then
            Dim containingDeclarator = objectCreationType.FirstAncestorOrSelf(Of VariableDeclaratorSyntax)
            If containingDeclarator.Names.Count > 1 Then Return ' low-hanging fruit: can't be bothered to dig into this
            containingMemberSymbol = ctx.SemanticModel.GetDeclaredSymbol(containingDeclarator.Names(0))
            If containingMemberSymbol.IsShared() Then Return
        Else
            Return
        End If


        ctx.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.GetLocation()))
    End Sub

End Class

<ExportCodeFixProvider("UseNameofFixerVB", LanguageNames.VisualBasic), [Shared]>
Public Class UseNameofFixerVB : Inherits CodeFixProvider

    Public Overrides Function GetFixableDiagnosticIds() As ImmutableArray(Of String)
        Return ImmutableArray.Create("XT001")
    End Function

    Public Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public Overrides Async Function ComputeFixesAsync(ctx As CodeFixContext) As Task
        Await Task.Delay(0)
        Dim diagnostic = ctx.Diagnostics.First
        Dim span = diagnostic.Location.SourceSpan
        Dim act = CodeAction.Create("Use shared PropertyChangedEventArgs", Function(c) MakeStaticPropertyChangedEventArgsAsync(ctx.Document, span, c))
        ctx.RegisterFix(act, diagnostic)
    End Function

    Private Async Function MakeStaticPropertyChangedEventArgsAsync(document As Document, diagnosticSpan As TextSpan, cancellationToken As CancellationToken) As Task(Of Document)
        Dim root = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim objectCreation = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf(Of ObjectCreationExpressionSyntax)
        Dim argumentHolder As ArgumentSyntax = objectCreation.ArgumentList.Arguments(0)
        Dim argument = CType(argumentHolder, SimpleArgumentSyntax).Expression
        Dim name As String = ""
        If argument.IsKind(SyntaxKind.StringLiteralExpression) Then
            name = CType(argument, LiteralExpressionSyntax).Token.ValueText
        Else
            ' TODO: support NameOf
            Throw New Exception("unexpected argument kind")
        End If
        name = New String((From c In name.ToCharArray Where Char.IsLetter(c)).ToArray)

        Dim asNew = SyntaxFactory.AsNewClause(objectCreation)
        Dim variableDeclarator = SyntaxFactory.VariableDeclarator(SyntaxFactory.ModifiedIdentifier("_" & name & "Changed")).WithAsClause(asNew)
        Dim modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.SharedKeyword))
        Dim eol = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, vbCrLf)
        Dim fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclarator).WithModifiers(modifiers).WithTrailingTrivia(eol).
            WithAdditionalAnnotations(Formatter.Annotation)

        Dim reference = SyntaxFactory.IdentifierName(variableDeclarator.Names(0).Identifier).WithTrailingTrivia(objectCreation.GetTrailingTrivia())

        Dim container = objectCreation.FirstAncestorOrSelf(Of ClassBlockSyntax)
        Dim newContainer = container.ReplaceNode(objectCreation, CType(reference, SyntaxNode))

        newContainer = newContainer.WithMembers(newContainer.Members.Add(fieldDeclaration))

        Dim newRoot = root.ReplaceNode(container, newContainer)
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class
