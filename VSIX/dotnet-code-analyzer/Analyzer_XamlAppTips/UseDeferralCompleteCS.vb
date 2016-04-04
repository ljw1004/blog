Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax
Imports Microsoft.CodeAnalysis.Formatting

' UseDeferralComplete
'
' The way WinRT does async events Is by the "deferral" pattern.
' If your event handler Is async, then you should call GetDeferral before your first await,
' And then deferral.Complete once your method has finished.
'
' This analyzer detects if you failed to call GetDeferral, And if you failed to call Complete.
'
' Limitation1: So far the analyzer only detects the deferral pattern In OnSuspending. It could be
' easily extended to detect the other deferral APIs in use in WinRT.
'
' Limitation2: The analyzer gives False positives If you Call deferral.Complete In some subsidiary
' method. But this Is reasonable. You shouldn't try to be fancy by delegating that call to Complete.
'
' Limitation3: The analyzer doesn't examine code-flow. It merely checks that calls to GetDeferral/Complete
' are present *somewhere* in the body of your event handler. I figure that's fine. Detailed flow
' analysis Is too fussy.



<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class UseDeferralCompleteCS
    Inherits DiagnosticAnalyzer

    Friend Shared RuleAsync As New DiagnosticDescriptor("XT002", "Use GetDeferral", "In an async OnSuspending method, you should use the GetDeferral pattern", "XAML", DiagnosticSeverity.Warning, True)
    Friend Shared RuleComplete As New DiagnosticDescriptor("XT003", "Use deferral.Complete", "If you call GetDeferral, then you should eventually call deferral.Complete", "XAML", DiagnosticSeverity.Warning, True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(RuleAsync, RuleComplete)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeNode, SyntaxKind.MethodDeclaration)
    End Sub

    Public Sub AnalyzeNode(ctx As SyntaxNodeAnalysisContext)
        Dim methodDeclaration = CType(ctx.Node, MethodDeclarationSyntax)
        If methodDeclaration.Identifier.ValueText <> "OnSuspending" Then Return
        If methodDeclaration.ParameterList.Parameters.Count <> 2 Then Return
        Dim param2 = TryCast(methodDeclaration.ParameterList.Parameters(1).Type, IdentifierNameSyntax)
        If param2 Is Nothing OrElse param2.Identifier.ValueText <> "SuspendingEventArgs" Then Return

        Dim methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(methodDeclaration)
        Dim isAsync = (methodSymbol.IsAsync)
        Dim getsDeferral As InvocationExpressionSyntax = Nothing
        Dim completesDeferral As InvocationExpressionSyntax = Nothing

        Dim invocations = methodDeclaration.DescendantNodes().OfType(Of InvocationExpressionSyntax)
        For Each invocation In invocations
            Dim target = ctx.SemanticModel.GetSymbolInfo(invocation.Expression)
            If target.Symbol Is Nothing Then Continue For
            If target.Symbol.Name = "GetDeferral" AndAlso target.Symbol.ContainingType.Name = "SuspendingOperation" Then getsDeferral = invocation
            If target.Symbol.Name = "Complete" AndAlso target.Symbol.ContainingType.Name = "SuspendingDeferral" Then completesDeferral = invocation
            ' Q. Should we use the fully-qualified name of the containing type? I don't know. I figure if anyone creates
            ' a type called SuspendingOperation with a method called GetDeferral, then they'd appreciate this diagnostic.
        Next

        If isAsync AndAlso getsDeferral Is Nothing Then
            Dim asyncModifiers = From m In methodDeclaration.Modifiers Where m.CSharpContextualKind() = SyntaxKind.AsyncKeyword
            ctx.ReportDiagnostic(Diagnostic.Create(RuleAsync, asyncModifiers.Single.GetLocation))
        End If

        If getsDeferral IsNot Nothing AndAlso completesDeferral Is Nothing Then
            ctx.ReportDiagnostic(Diagnostic.Create(RuleComplete, getsDeferral.GetLocation))
        End If
    End Sub

End Class
