Imports System.Collections.Immutable
Imports System.Composition
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.Simplification
Imports Microsoft.CodeAnalysis.CSharp
Imports Microsoft.CodeAnalysis.CSharp.Syntax



<DiagnosticAnalyzer(LanguageNames.CSharp)>
Public Class PlatformSpecificAnalyzerCS
    Inherits DiagnosticAnalyzer

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(RulePlatform, RuleVersion)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        ' It would be simplest just to context.RegisterSyntaxNodeAction(...) right here.
        ' However, it just generates multiple diagnostics per line, and
        ' until bug https://github.com/dotnet/roslyn/issues/3311 in Roslyn is fixed,
        ' then it would also duplicate the "Supress" codefixes. Yuck. We'll use the
        ' following workaround for now, and will revert to the simpler version once
        ' VS2015 Update1 comes out.
        context.RegisterCodeBlockStartAction(Of SyntaxKind)(AddressOf AnalyzeCodeBlockStart)
    End Sub

    Public Sub AnalyzeCodeBlockStart(context As CodeBlockStartAnalysisContext(Of SyntaxKind))
        Dim reports As New Dictionary(Of Integer, Diagnostic)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.IdentifierName)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.SimpleMemberAccessExpression)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.QualifiedName)
        context.RegisterCodeBlockEndAction(
                Sub(c)
                    For Each diagnostic In reports.Values
                        c.ReportDiagnostic(diagnostic)
                    Next
                End Sub)
    End Sub

    Public Sub AnalyzeExpression(context As SyntaxNodeAnalysisContext, reports As Dictionary(Of Integer, Diagnostic))
        If context.Node.Parent.Kind = SyntaxKind.SimpleMemberAccessExpression Then Return ' will be handled at higher level
        If context.Node.Parent.Kind = SyntaxKind.QualifiedName Then Return
        Dim target = GetTargetOfNode(context.Node, context.SemanticModel)
        If target Is Nothing Then Return
        Dim plat = Platform.OfSymbol(target)

        ' Some quick escapes
        If plat.Kind = PlatformKind.Unchecked Then Return
        If plat.Kind = PlatformKind.Uwp AndAlso plat.Version = "10240" Then Return

        ' Is this expression inside a method/constructor/property that claims to be specific?
        Dim containingBlock = context.Node.FirstAncestorOrSelf(Of BlockSyntax)
        Dim containingMember As MemberDeclarationSyntax = containingBlock?.FirstAncestorOrSelf(Of BaseMethodDeclarationSyntax) ' for constructors and methods
        If containingBlock Is Nothing OrElse TypeOf containingBlock?.Parent Is AccessorDeclarationSyntax Then containingMember = context.Node.FirstAncestorOrSelf(Of PropertyDeclarationSyntax)
        If containingMember IsNot Nothing Then
            Dim containingMemberSymbol = context.SemanticModel.GetDeclaredSymbol(containingMember)
            If HasPlatformSpecificAttribute(containingMemberSymbol) Then Return
        End If

        ' Is this invocation properly guarded? See readme.md for explanations.
        If IsProperlyGuarded(context.Node, context.SemanticModel) Then Return
        If containingBlock IsNot Nothing Then
            For Each ret In containingBlock.DescendantNodes.OfType(Of ReturnStatementSyntax)
                If IsProperlyGuarded(ret, context.SemanticModel) Then Return
            Next
        End If

        ' Some things we can't judge whether to report until after we've looked up the project version...
        If plat.Kind = PlatformKind.Uwp AndAlso plat.Version <> "10240" Then
            Dim projMinVersion = GetTargetPlatformMinVersion(context.Options.AdditionalFiles)
            If projMinVersion >= CInt(plat.Version) Then Return
        End If

        ' We'll report only a single diagnostic per line, the first.
        Dim loc = context.Node.GetLocation
        If Not loc.IsInSource Then Return
        Dim line = loc.GetLineSpan().StartLinePosition.Line
        If reports.ContainsKey(line) AndAlso reports(line).Location.SourceSpan.Start <= loc.SourceSpan.Start Then Return
        reports(line) = Diagnostic.Create(If(plat.Kind = PlatformKind.Uwp, RuleVersion, RulePlatform), loc)
    End Sub


    Shared Function GetTargetOfNode(node As SyntaxNode, sm As SemanticModel) As ISymbol
        If node.Parent.Kind = SyntaxKind.InvocationExpression AndAlso node Is CType(node.Parent, InvocationExpressionSyntax).Expression Then
            ' <target>(...)
            Dim invocationExpression = CType(node.Parent, InvocationExpressionSyntax)
            Return sm.GetSymbolInfo(invocationExpression).Symbol ' points to the method after overload resolution
        ElseIf node.Parent.Kind = SyntaxKind.ObjectCreationExpression AndAlso node Is CType(node.Parent, ObjectCreationExpressionSyntax).Type Then
            ' New <target>
            Dim objectCreationExpression = CType(node.Parent, ObjectCreationExpressionSyntax)
            Dim target = sm.GetSymbolInfo(objectCreationExpression).Symbol ' points to the constructor after overload resolution
            Return target
        Else
            ' f<target>(...)
            ' <target> x = ...
            ' Action x = <target>  -- note that following code does pick the right overload
            ' <target> += delegate -- the following code does recognize events
            ' nameof(<target>) -- I think it's nicer to report on this, even if not technically needed
            ' Field access? I'll disallow it for enum values, and allow it for everything else
            Dim target = sm.GetSymbolInfo(node).Symbol
            If target Is Nothing Then Return Nothing
            If target.Kind = SymbolKind.Method OrElse target.Kind = SymbolKind.Event OrElse target.Kind = SymbolKind.Property Then Return target
            If target.Kind = SymbolKind.Field AndAlso target.ContainingType.TypeKind = TypeKind.Enum Then Return target
            Return Nothing
        End If
    End Function


    Function IsProperlyGuarded(node As SyntaxNode, semanticModel As SemanticModel) As Boolean
        For Each symbol In GetGuards(node, semanticModel)
            If symbol.ContainingType?.Name = "ApiInformation" Then Return True
            If HasPlatformSpecificAttribute(symbol) Then Return True
        Next
        Return False
    End Function


    Shared Iterator Function GetGuards(node As SyntaxNode, semanticModel As SemanticModel) As IEnumerable(Of ISymbol)
        For Each condition In GetConditions(node)

            ' First check for invocations of ApiInformation.IsTypePresent
            For Each invocation In condition.DescendantNodesAndSelf.OfType(Of InvocationExpressionSyntax)
                Dim targetMethod = semanticModel.GetSymbolInfo(invocation).Symbol
                If targetMethod?.ContainingType?.Name = "ApiInformation" Then Yield targetMethod
            Next

            ' Next check for any property/field access
            Dim accesses1 = condition.DescendantNodesAndSelf.OfType(Of MemberAccessExpressionSyntax).Select(Function(n) semanticModel.GetSymbolInfo(n).Symbol)
            Dim accesses2 = condition.DescendantNodesAndSelf.OfType(Of IdentifierNameSyntax).Select(Function(n) semanticModel.GetSymbolInfo(n).Symbol)
            For Each symbol In accesses1.Concat(accesses2)
                If symbol?.Kind = SymbolKind.Field OrElse symbol?.Kind = SymbolKind.Property Then Yield symbol
            Next
        Next
    End Function


    Shared Iterator Function GetConditions(node As SyntaxNode) As IEnumerable(Of ExpressionSyntax)
        Dim check = node.FirstAncestorOrSelf(Of IfStatementSyntax)
        While check IsNot Nothing
            Yield check.Condition
            check = check.Parent.FirstAncestorOrSelf(Of IfStatementSyntax)
        End While
    End Function

End Class




<ExportCodeFixProvider(LanguageNames.CSharp), [Shared]>
Public Class PlatformSpecificFixerCS
    Inherits CodeFixProvider

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create("UWP001", "UWP002")
        End Get
    End Property

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
        Try
            Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)
            Dim semanticModel = Await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(False)

            ' Which node are we interested in? -- if the squiggle is over A.B().C,
            ' then we need the largest IdentifierName/SimpleMemberAccess/QualifiedName
            ' that encompasses "C" itself
            Dim diagnostic = context.Diagnostics.First
            Dim span As New Text.TextSpan(diagnostic.Location.SourceSpan.End - 1, 1)
            Dim node = root.FindToken(span.Start).Parent
            While node.Kind <> SyntaxKind.IdentifierName AndAlso node.Kind <> SyntaxKind.SimpleMemberAccessExpression AndAlso node.Kind <> SyntaxKind.QualifiedName
                node = node.Parent
                If node Is Nothing Then Return
            End While
            While True
                If node.Parent?.Kind = SyntaxKind.SimpleMemberAccessExpression Then node = node.Parent : Continue While
                If node.Parent?.Kind = SyntaxKind.QualifiedName Then node = node.Parent : Continue While
                Exit While
            End While
            Dim target = PlatformSpecificAnalyzerCS.GetTargetOfNode(node, semanticModel)
            Dim g = HowToGuard.Symbol(target)


            ' Introduce a guard? (only if it is a method/accessor/constructor, i.e. somewhere that allows code)
            Dim containingBlock = node.FirstAncestorOrSelf(Of BlockSyntax)
            If containingBlock IsNot Nothing Then
                Dim act1 = CodeAction.Create($"Add 'If ApiInformation.{g.KindOfCheck}'", Function(c) IntroduceGuardAsync(context.Document, node, g, c), "PlatformSpecificGuard")
                context.RegisterCodeFix(act1, diagnostic)
            End If

            ' Mark method/property/constructor as platform-specific?
            If containingBlock Is Nothing OrElse TypeOf containingBlock.Parent Is AccessorDeclarationSyntax Then
                Dim propDeclaration = node.FirstAncestorOrSelf(Of PropertyDeclarationSyntax)
                If propDeclaration Is Nothing Then Return
                Dim name = $"property '{propDeclaration.Identifier.Text}'"
                Dim act2 = CodeAction.Create($"Mark {name} as {g.AttributeFriendlyName}", Function(c) AddPlatformSpecificAttributeAsync(g.AttributeToIntroduce, propDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
                context.RegisterCodeFix(act2, diagnostic)
            ElseIf containingBlock.Parent.Kind = SyntaxKind.MethodDeclaration Then
                Dim methodDeclaration = containingBlock.FirstAncestorOrSelf(Of MethodDeclarationSyntax)
                Dim name = $"method '{methodDeclaration.Identifier.Text}'"
                Dim act2 = CodeAction.Create($"Mark {name} as {g.AttributeFriendlyName}", Function(c) AddPlatformSpecificAttributeAsync(g.AttributeToIntroduce, methodDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
                context.RegisterCodeFix(act2, diagnostic)
            ElseIf containingBlock.Parent.Kind = SyntaxKind.ConstructorDeclaration Then
                Dim methodDeclaration = containingBlock.FirstAncestorOrSelf(Of ConstructorDeclarationSyntax)
                Dim name = $"constructor '{methodDeclaration.Identifier.Text}'"
                Dim act2 = CodeAction.Create($"Mark {name} as {g.AttributeFriendlyName}", Function(c) AddPlatformSpecificAttributeAsync(g.AttributeToIntroduce, methodDeclaration, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
                context.RegisterCodeFix(act2, diagnostic)
            End If

            ' Mark some of the conditions as platform-specific?
            For Each symbol In PlatformSpecificAnalyzerCS.GetGuards(node, semanticModel)
                If symbol.ContainingType.Name = "ApiInformation" Then Continue For
                If Not symbol.Locations.First.IsInSource Then Return
                Dim act4 As CodeAction = Nothing
                Dim symbolSyntax = Await symbol.DeclaringSyntaxReferences.First.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(False)
                Dim fieldSyntax = TryCast(symbolSyntax.Parent.Parent, FieldDeclarationSyntax)
                Dim propSyntax = TryCast(symbolSyntax, PropertyDeclarationSyntax)
                If fieldSyntax IsNot Nothing Then
                    act4 = CodeAction.Create($"Mark field '{symbol.Name}' as {g.AttributeFriendlyName}", Function(c) AddPlatformSpecificAttributeAsync(g.AttributeToIntroduce, fieldSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
                ElseIf propSyntax IsNot Nothing Then
                    act4 = CodeAction.Create($"Mark property '{symbol.Name}' as {g.AttributeFriendlyName}", Function(c) AddPlatformSpecificAttributeAsync(g.AttributeToIntroduce, propSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
                End If
                If act4 IsNot Nothing Then context.RegisterCodeFix(act4, diagnostic)
            Next
        Catch ex As Exception
        End Try
    End Function


    Private Async Function AddPlatformSpecificAttributeAsync(Of T As SyntaxNode)(attrName As String, oldSyntax As T, f As Func(Of T, AttributeListSyntax, T), solution As Solution, cancellationToken As CancellationToken) As Task(Of Solution)
        ' + [System.Runtime.CompilerServices.PlatformSpecific]
        '   type p // method/class/field/property

        Try
            Dim oldRoot = Await oldSyntax.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(False)
            Dim temp As SyntaxNode = Nothing
            Dim oldDocument = (From p In solution.Projects From d In p.Documents Where d.TryGetSyntaxRoot(temp) AndAlso temp Is oldRoot Select d).First
            '
            Dim id = SyntaxFactory.ParseName(attrName)
            Dim attr = SyntaxFactory.Attribute(id)
            Dim attrs = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)).WithAdditionalAnnotations(Simplifier.Annotation)
            '
            Dim newSyntax = f(oldSyntax, attrs)
            Dim newRoot = oldRoot.ReplaceNode(oldSyntax, newSyntax)
            Return solution.WithDocumentSyntaxRoot(oldDocument.Id, newRoot)
        Catch ex As Exception
            Return solution
        End Try
    End Function

    Private Async Function IntroduceGuardAsync(document As Document, node As SyntaxNode, g As HowToGuard, cancellationToken As CancellationToken) As Task(Of Document)
        ' + if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(targetContainingType)) {
        '       old-statement
        ' + }
        Try
            Dim oldStatement = node.FirstAncestorOrSelf(Of StatementSyntax)()
            Dim oldLeadingTrivia = oldStatement.GetLeadingTrivia()
            '
            Dim conditionReceiver = SyntaxFactory.ParseName($"Windows.Foundation.Metadata.ApiInformation.{g.KindOfCheck}").WithAdditionalAnnotations(Simplifier.Annotation)
            Dim conditionArgument As ArgumentListSyntax
            If g.MemberToCheck Is Nothing Then
                Dim conditionString1 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(g.TypeToCheck))
                conditionArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(conditionString1)))
            Else
                Dim conditionString1 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(g.TypeToCheck))
                Dim conditionString2 = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(g.MemberToCheck))
                Dim conditions As IEnumerable(Of ArgumentSyntax) = {SyntaxFactory.Argument(conditionString1), SyntaxFactory.Argument(conditionString2)}
                conditionArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(conditions))

            End If
            Dim condition = SyntaxFactory.InvocationExpression(conditionReceiver, conditionArgument)
            '
            Dim thenStatements = SyntaxFactory.Block(oldStatement.WithoutLeadingTrivia())
            Dim ifStatement = SyntaxFactory.IfStatement(condition, thenStatements).WithLeadingTrivia(oldLeadingTrivia).WithAdditionalAnnotations(Formatter.Annotation)

            Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
            Dim newRoot = oldRoot.ReplaceNode(oldStatement, ifStatement)
            Return document.WithSyntaxRoot(newRoot)
        Catch ex As Exception
            Return document
        End Try
    End Function

End Class
