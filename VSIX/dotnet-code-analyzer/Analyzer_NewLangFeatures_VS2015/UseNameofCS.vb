Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax

' UseNameof.
'
' The "nameof" operator is new in C#6 And VB.
' When the compiler sees "nameof(identifier_or_type)" it replaces it with a string literal
' version of the identifier or type's name. Unqualified.
'
' This analyzer looks for places where you use a string literal "id"
' If you use it inside a class/struct, and the class/struct has a member of the same name,
' then it suggests to use that name. Also if you use it inside a method, and the method has
' a parameter of the same name, then likewise.
'
' NOTE: I worry about false positives. It might be better to scale back this analyzer
' to only recommend places where we know that people typically use string literals
' of member/parameter names, e.g.
'    DependencyProperty.Register("id", ...)
'    New PropertyChangedEventArgs("id")
'    New ArgumentNullException("id")


<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class UseNameofAnalyzerCS
    Inherits DiagnosticAnalyzer

    Friend Shared Rule As New DiagnosticDescriptor("LANG001", "Use nameof operator", "The new nameof() operator is less error-prone than String literals.", "Language", DiagnosticSeverity.Warning, isEnabledByDefault:=True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeNode, SyntaxKind.StringLiteralExpression)
    End Sub

    Public Sub AnalyzeNode(ctx As SyntaxNodeAnalysisContext)
        Dim literal = CType(ctx.Node, LiteralExpressionSyntax).Token.ValueText

        ' First look for member names
        Dim container = ctx.Node.FirstAncestorOrSelf(Of TypeDeclarationSyntax)
        If container Is Nothing Then Return
        Dim containerSymbol = ctx.SemanticModel.GetDeclaredSymbol(container)
        If Not containerSymbol.MemberNames.Contains(literal) Then
            ' Failing that, look for parameter names
            Dim member = ctx.Node.FirstAncestorOrSelf(Of MethodDeclarationSyntax)
            If member Is Nothing Then Return
            Dim memberSymbol = ctx.SemanticModel.GetDeclaredSymbol(member)
            If Not (From p In memberSymbol.Parameters Select p.Name).Contains(literal) Then Return
        End If

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, ctx.Node.GetLocation))
    End Sub

End Class

<ExportCodeFixProvider("UseNameofFixerCS", LanguageNames.CSharp), [Shared]>
Public Class UseNameofFixerCS : Inherits CodeFixProvider

    Public Overrides Function GetFixableDiagnosticIds() As ImmutableArray(Of String)
        Return ImmutableArray.Create("LANG001")
    End Function

    Public Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public Overrides Async Function ComputeFixesAsync(ctx As CodeFixContext) As Task
        Await Task.Delay(0)
        Dim diagnostic = ctx.Diagnostics.First
        Dim span = diagnostic.Location.SourceSpan
        Dim act = CodeAction.Create("Use nameof()", Function(c) MakeNameofAsync(ctx.Document, span, c))
        ctx.RegisterFix(act, diagnostic)
    End Function

    Private Async Function MakeNameofAsync(document As Document, diagnosticSpan As TextSpan, cancellationToken As CancellationToken) As Task(Of Document)
        Dim root = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim SemanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim literal = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf(Of LiteralExpressionSyntax)
        Dim name = literal.Token.ValueText

        ' This fix is good regardless of whether the literal matched a member name or a parameter name
        ' Note that nameof takes a "TypeName" argument for odd language reasons, even though in this
        ' case the thing won't be a type!
        Dim newNode = SyntaxFactory.NameOfExpression("nameof", SyntaxFactory.ParseTypeName(name))

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim newRoot = oldRoot.ReplaceNode(literal, CType(newNode, SyntaxNode))
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class
