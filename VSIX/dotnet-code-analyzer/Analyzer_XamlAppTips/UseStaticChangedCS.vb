Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax
Imports Microsoft.CodeAnalysis.Formatting

' UseStaticPropertyChangedEventArgs
'
' When you implement INotifyPropertyChanged, if one of your properties changes,
' you are expected to fire off a PropertyChanged event. This takes a
' PropertyChangedEventArgs argument. People sometimes construct a New
' instance of the PropertyChangedEventArgs class every single time the
' property changes. This amounts of a lot of heap allocations, which
' hurt performance in XAML apps.
'
' This analyzer looks for every place where you construct an instance of PropertyChangedEventArgs
' in something other than a static field. Its fix Is to instead construct the instance
' in a static field.
'
' NOTE: This analyzer doesn't do work to avoid name-clashes. If the static field already
' exists, it will give poor results.


<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class UseNameofAnalyzerCS
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
        If objectCreation.ArgumentList Is Nothing OrElse objectCreation.ArgumentList.Arguments.Count <> 1 Then Return
        Dim argument = objectCreation.ArgumentList.Arguments(0).Expression
        If Not argument.IsKind(SyntaxKind.StringLiteralExpression) AndAlso Not argument.IsKind(SyntaxKind.NameOfExpression) Then Return

        Dim containingMember = objectCreationType.FirstAncestorOrSelf(Of MemberDeclarationSyntax)
        Dim containingField = TryCast(containingMember, FieldDeclarationSyntax)
        If containingField IsNot Nothing Then
            Dim staticModifiers = From tk In containingField.Modifiers Where tk.CSharpKind = SyntaxKind.StaticKeyword
            If staticModifiers.Any Then Return
        End If

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.GetLocation))
    End Sub

End Class

<ExportCodeFixProvider("UseNameofFixerCS", LanguageNames.CSharp), [Shared]>
Public Class UseNameofFixerCS : Inherits CodeFixProvider

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
        Dim act = CodeAction.Create("Use static PropertyChangedEventArgs", Function(c) MakeStaticPropertyChangedEventArgsAsync(ctx.Document, span, c))
        ctx.RegisterFix(act, diagnostic)
    End Function

    Private Async Function MakeStaticPropertyChangedEventArgsAsync(document As Document, diagnosticSpan As TextSpan, cancellationToken As CancellationToken) As Task(Of Document)
        Dim root = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)

        Dim SemanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim objectCreation = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf(Of ObjectCreationExpressionSyntax)
        Dim argument = objectCreation.ArgumentList.Arguments(0).Expression
        Dim name As String = ""
        If argument.IsKind(SyntaxKind.StringLiteralExpression) Then
            name = CType(argument, LiteralExpressionSyntax).Token.ValueText
        ElseIf argument.IsKind(SyntaxKind.NameOfExpression) Then
            argument = CType(argument, NameOfExpressionSyntax).Argument
            If argument.IsKind(SyntaxKind.QualifiedName) Then
                name = CType(argument, QualifiedNameSyntax).Right.Identifier.ValueText
            Else
                name = argument.ToString()
            End If
        End If
        name = New String((From c In name.ToCharArray Where Char.IsLetter(c)).ToArray)

        Dim initializer = SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(objectCreation.Type).WithArgumentList(objectCreation.ArgumentList))
        Dim variableDeclarator = SyntaxFactory.VariableDeclarator("_" & name & "Changed").WithInitializer(initializer)
        Dim variableDeclaration = SyntaxFactory.VariableDeclaration(objectCreation.Type, SyntaxFactory.SingletonSeparatedList(variableDeclarator))
        Dim modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
        Dim fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration).WithModifiers(modifiers).
            WithAdditionalAnnotations(Formatter.Annotation)

        Dim reference = SyntaxFactory.IdentifierName(variableDeclarator.Identifier)

        Dim container = objectCreation.FirstAncestorOrSelf(Of ClassDeclarationSyntax)
        Dim newContainer = container.ReplaceNode(objectCreation, CType(reference, SyntaxNode))

        newContainer = newContainer.WithMembers(newContainer.Members.Add(fieldDeclaration))

        Dim newRoot = root.ReplaceNode(container, newContainer)
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class
