Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax



<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class UseDeferralCompleteVB
    Inherits DiagnosticAnalyzer

    Friend Shared RuleAsync As New DiagnosticDescriptor("XT002", "Use GetDeferral", "In an async OnSuspending method, you should use the GetDeferral pattern", "XAML", DiagnosticSeverity.Warning, True)
    Friend Shared RuleComplete As New DiagnosticDescriptor("XT003", "Use deferral.Complete", "If you call GetDeferral, then you should eventually call deferral.Complete", "XAML", DiagnosticSeverity.Warning, True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(RuleAsync, RuleComplete)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeNode, SyntaxKind.SubStatement)
    End Sub

    Public Sub AnalyzeNode(ctx As SyntaxNodeAnalysisContext)
        Dim methodDeclaration = CType(ctx.Node, MethodStatementSyntax)
        If methodDeclaration.Identifier.ValueText <> "OnSuspending" Then Return
        If methodDeclaration.ParameterList.Parameters.Count <> 2 Then Return
        Dim param2 = TryCast(methodDeclaration.ParameterList.Parameters(1).AsClause.Type, IdentifierNameSyntax)
        If param2 Is Nothing OrElse param2.Identifier.ValueText <> "SuspendingEventArgs" Then Return

        Dim methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(methodDeclaration)
        Dim isAsync = (methodSymbol.IsAsync)
        Dim getsDeferral As InvocationExpressionSyntax = Nothing
        Dim completesDeferral As InvocationExpressionSyntax = Nothing

        Dim invocations = methodDeclaration.Parent.DescendantNodes().OfType(Of InvocationExpressionSyntax)
        For Each invocation In invocations
            Dim target = ctx.SemanticModel.GetSymbolInfo(invocation.Expression)
            If target.Symbol Is Nothing Then Continue For
            If target.Symbol.Name = "GetDeferral" AndAlso target.Symbol.ContainingType.Name = "SuspendingOperation" Then getsDeferral = invocation
            If target.Symbol.Name = "Complete" AndAlso target.Symbol.ContainingType.Name = "SuspendingDeferral" Then completesDeferral = invocation
            ' Q. Should we use the fully-qualified name of the containing type? I don't know. I figure if anyone creates
            ' a type called SuspendingOperation with a method called GetDeferral, then they'd appreciate this diagnostic.
        Next

        If isAsync AndAlso getsDeferral Is Nothing Then
            Dim asyncModifiers = From m In methodDeclaration.Modifiers Where m.VisualBasicContextualKind() = SyntaxKind.AsyncKeyword
            ctx.ReportDiagnostic(Diagnostic.Create(RuleAsync, asyncModifiers.Single.GetLocation))
        End If

        If getsDeferral IsNot Nothing AndAlso completesDeferral Is Nothing Then
            ctx.ReportDiagnostic(Diagnostic.Create(RuleComplete, getsDeferral.GetLocation))
        End If
    End Sub

End Class
