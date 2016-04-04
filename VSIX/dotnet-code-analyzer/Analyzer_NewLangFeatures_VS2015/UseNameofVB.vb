Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax


<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class UseNameofAnalyzerVB
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
        Dim container = ctx.Node.FirstAncestorOrSelf(Of TypeBlockSyntax)
        If container Is Nothing Then Return
        Dim containerSymbol = ctx.SemanticModel.GetDeclaredSymbol(container)
        If Not containerSymbol.MemberNames.Contains(literal) Then
            ' Failing that, look for parameter names
            Dim member = ctx.Node.FirstAncestorOrSelf(Of MethodBlockSyntax)
            If member Is Nothing Then Return
            Dim memberSymbol = ctx.SemanticModel.GetDeclaredSymbol(member)
            If Not (From p In memberSymbol.Parameters Select p.Name).Contains(literal) Then Return
        End If

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, ctx.Node.GetLocation))
    End Sub

End Class


' NameOf is not yet implemented in VB so we can't writer a fix for it
