Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.Rename
Imports Microsoft.CodeAnalysis.Text

' CheckedExceptions
'
' Some methods. They indicate this with [Throws], or can provide some
' user-specified but analyzer-ignored information such as [Throws("network")].
' This analyzer checks that any method which invokes such a method either
' has try/catch around it, or has its own [Throws] attribute.
' This analyzer also has hard-coded conventions about certain standard APIs
' that [Throws("network")].
'
' Limitation1: This analyzer doesn't yet work with constructors (i.e. doesn't
' examine constructor-calls for whether the constructor might throw, and doesn't
' have a notion of [Throws] attributes on constructors). Likewise, it doesn;t
' deal with initializers that might throw.
'
' Limitation2: This analyzer doesn't yet work with properties either.
'
' Limitation3: This analyzer doesn't have knowledge of *which* exceptions you should
' be catching. Its designed role is merely as a safeguard, to remind you that you should be
' catching exceptions. It won't help in cases where you're already catching some other
' kind of exception, but now should also be catching additional kinds of exceptions.
'
' Limitation4: This analyzer doesn't and can't work through lambdas and tasks.
' It can't because to do so you'd need a type system like List<Action[throws]>, and the
' CLR type system doesn't do that.
' In the case of Task, the analyzer will flag the point where you invoked the Task-returning
' method rather than (more correctly) flagging the point where you await the Task.
' In the case of lambdas, again the analyzer will flag the point within the lambda where you
' invoke a [Throws] method, and will be happy if you catch it either within or without the lambda.

<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class CheckedExceptionsAnalyzerCS
    Implements ISyntaxNodeAnalyzer(Of SyntaxKind)

    Friend Shared RuleAttribute As New DiagnosticDescriptor("SAFE001", "Handle exceptions", "This method might throw{0}", "Safety", DiagnosticSeverity.Warning, True)
    Friend Shared RuleNetwork As New DiagnosticDescriptor("SAFE002", "Handle exceptions from network APIs", "This method might throw{0}", "Safety", DiagnosticSeverity.Warning, True)

    Public ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor) Implements IDiagnosticAnalyzer.SupportedDiagnostics
        Get
            Return ImmutableArray.Create(RuleAttribute, RuleNetwork)
        End Get
    End Property

    Public ReadOnly Property SyntaxKindsOfInterest As ImmutableArray(Of SyntaxKind) Implements ISyntaxNodeAnalyzer(Of SyntaxKind).SyntaxKindsOfInterest
        Get
            Return ImmutableArray.Create(SyntaxKind.InvocationExpression)
        End Get
    End Property

    Public Sub AnalyzeNode(node As SyntaxNode, semanticModel As SemanticModel, addDiagnostic As Action(Of Diagnostic), options As AnalyzerOptions, cancellationToken As CancellationToken) Implements ISyntaxNodeAnalyzer(Of SyntaxKind).AnalyzeNode
        Dim invocationExpression = CType(node, InvocationExpressionSyntax)

        ' If there are any catch blocks, then we can pre-empty further analysis
        While True
            Dim tryNode = node.FirstAncestorOrSelf(Of TryStatementSyntax)
            If tryNode Is Nothing Then Exit While
            If tryNode.Catches.Count > 0 Then Return
            node = tryNode
        End While

        ' Otherwise, let's first find what things this method already declares it [Throws]
        Dim containingMember = invocationExpression.FirstAncestorOrSelf(Of MemberDeclarationSyntax)
        Dim containingMethod = TryCast(containingMember, MethodDeclarationSyntax)
        If containingMethod Is Nothing Then Return
        Dim containingMethodSymbol = semanticModel.GetDeclaredSymbol(containingMethod)
        If containingMethodSymbol Is Nothing Then Return
        Dim containingThrows As New List(Of String)
        For Each attr In containingMethodSymbol.GetAttributes()
            If attr.AttributeClass.ToDisplayString() <> GetType(Finglebing.ThrowsAttribute).FullName Then Continue For
            If attr.ConstructorArguments.Length = 0 Then containingThrows.Add("") Else containingThrows.Add(CStr(attr.ConstructorArguments(0).Value))
        Next
        If containingThrows.Contains("") Then Return

        ' And find what the target [Throws]
        Dim target = semanticModel.GetSymbolInfo(invocationExpression.Expression)
        Dim targetName = target.Symbol.ToDisplayString()
        Dim targetThrows As New List(Of String)
        Dim rule As DiagnosticDescriptor
        If targetName = "System.Net.Http.HttpClient.GetStringAsync(string)" Then
            targetThrows.Add("network")
            rule = RuleNetwork
        Else
            If target.Symbol Is Nothing Then Return
            For Each attr In target.Symbol.GetAttributes()
                If attr.AttributeClass.ToDisplayString() <> GetType(Finglebing.ThrowsAttribute).FullName Then Continue For
                If attr.ConstructorArguments.Length = 0 Then targetThrows.Add("") Else targetThrows.Add(CStr(attr.ConstructorArguments(0).Value))
            Next
            rule = RuleAttribute
        End If

        ' Figure out which target [Throws] are unhandled by the method
        Dim unhandledThrows = (From s In targetThrows
                               Where Not containingThrows.Contains(s)
                               Order By s Distinct).ToArray()
        If unhandledThrows.Length = 0 Then Return
        If unhandledThrows.Contains("") Then unhandledThrows = {""}

        Dim arg = ""
        If unhandledThrows.Length > 1 OrElse unhandledThrows(0) <> "" Then
            arg = " '" & String.Join(",", unhandledThrows) & "'"
        End If

        addDiagnostic(Diagnostic.Create(rule, node.GetLocation, arg))
    End Sub

End Class


