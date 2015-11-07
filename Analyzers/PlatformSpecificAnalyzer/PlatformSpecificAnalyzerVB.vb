Imports System.Collections.Immutable
Imports System.Composition
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.Simplification
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

' FOR EASIER F5 DEBUGGING OF THIS ANALYZER:
' Until https://github.com/dotnet/roslyn/issues/4542 is fixed, you can't F5 on a PCL.
' So instead, set up DummyTestLauncher as your startup project and F5 it. You should
' set its debug tab:
' StartAction: external program
'     C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
' Command-line arguments: 
'     "C:\Users\lwischik\Source\Repos\blog\Analyzers\DemoUWP_VB\DemoUWP_VB.sln" /RootSuffix Analyzer

Public Enum PlatformKind
    Unchecked ' .NET and Win8.1
    Uwp ' the core UWP platform
    Ext ' Desktop, Mobile, IOT, Xbox extension SDK
    User ' from when the user put a *Specific attribute on something
End Enum

Public Structure Platform
    Public Kind As PlatformKind
    Public Version As String
    Public Sub New(kind As PlatformKind, Optional version As String = Nothing)
        Me.Kind = kind
        Me.Version = version
        Select Case kind
            Case PlatformKind.Unchecked : If version IsNot Nothing Then Throw New ArgumentException("No version expected")
            Case PlatformKind.Uwp, PlatformKind.Ext : If version <> "10240" Then Throw New ArgumentException("Only known SDK is 10240")
            Case PlatformKind.User : If Not version?.EndsWith("Specific") Then Throw New ArgumentException("User specific should end in Specific")
        End Select
    End Sub
End Structure


<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class PlatformSpecificAnalyzerVB
    Inherits DiagnosticAnalyzer

    Friend Shared Rule As New DiagnosticDescriptor("UWP001", "Platform-specific", "Platform-specific code", "Safety", DiagnosticSeverity.Warning, True)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Shared Function GetTargetPlatformMinVersion(comp As Compilation, tree As SyntaxTree, ext As String) As String
        ' Hack: because of https://github.com/dotnet/roslyn/issues/6627 it's impossible
        ' to get information from the csproj in an analyzer. So I'm going to hack around it...
        ' Note that this first TryGetValue path doesn't hit the filesystem.
        Dim dir = Path.GetDirectoryName(tree.FilePath)
        Dim projFile = $"{dir}\{comp.AssemblyName}{ext}"
        Static Dim versions As New Dictionary(Of String, String)
        Static Dim times As New Dictionary(Of String, DateTime)
        Dim version As String = Nothing
        If versions.TryGetValue(projFile, version) AndAlso times(projFile) + TimeSpan.FromSeconds(30) > DateTime.Now Then Return version

        ' We don't have a reliable way to get the project file. So I'm going to hack it.
        ' projFile is the key that's used for the TryGet dictionary, while fn is our
        ' best guess as to the actual location of the proj file.
        Dim fn = projFile
        If Not File.Exists(fn) Then dir = Path.GetDirectoryName(dir) : fn = $"{dir}\{comp.AssemblyName}{ext}"
        If Not File.Exists(fn) Then dir = Path.GetDirectoryName(dir) : fn = $"{dir}\{comp.AssemblyName}{ext}"
        If Not File.Exists(fn) Then version = "unobtainable" : versions(projFile) = version : times(projFile) = DateTime.Now : Return version

        ' I've heard bad things about FileSystemWatcher, so instead I'm polling (limited to 2/minute)
        Dim lines = File.ReadAllLines(fn)
        Dim line = lines.FirstOrDefault(Function(s) s.Trim.StartsWith("<TargetPlatformMinVersion>"))
        version = line?.Replace("<TargetPlatformMinVersion>", "").Replace("</TargetPlatformMinVersion>", "").Replace("</>", "").Replace("10.0.", "").Replace(".0", "").Trim()
        versions(projFile) = version
        times(projFile) = DateTime.Now
        Return version
    End Function

    Public Overrides Sub Initialize(context As AnalysisContext)
        ' context.RegisterSyntaxNodeAction(AddressOf AnalyzeInvocation)
        ' This would be simplest. It just generates multiple diagnostics per line
        ' However, until bug https://github.com/dotnet/roslyn/issues/3311 in Roslyn is fixed,
        ' it also gives duplicate "Supress" codefixes.
        ' So until then, we'll do work to generate only a single diagnostic per line:
        context.RegisterCompilationAction(Sub(ct)
                                              'ct.Options.AdditionalFiles
                                          End Sub)

        context.RegisterCodeBlockStartAction(Of SyntaxKind)(AddressOf AnalyzeCodeBlockStart)
    End Sub

    Public Sub AnalyzeCodeBlockStart(context As CodeBlockStartAnalysisContext(Of SyntaxKind))
        Dim reports As New Dictionary(Of Integer, Location)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.IdentifierName)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.SimpleMemberAccessExpression)
        context.RegisterSyntaxNodeAction(Sub(c) AnalyzeExpression(c, reports), SyntaxKind.QualifiedName)
        context.RegisterCodeBlockEndAction(
            Sub(c)
                For Each span In reports.Values
                    c.ReportDiagnostic(Diagnostic.Create(Rule, span))
                Next
            End Sub)
    End Sub

    Public Sub AnalyzeExpression(context As SyntaxNodeAnalysisContext, reports As Dictionary(Of Integer, Location))
        If context.Node.Parent.Kind = SyntaxKind.SimpleMemberAccessExpression Then Return ' will be handled at higher level
        If context.Node.Parent.Kind = SyntaxKind.QualifiedName Then Return



        If context.Node.Parent.Kind = SyntaxKind.InvocationExpression AndAlso context.Node Is CType(context.Node.Parent, InvocationExpressionSyntax).Expression Then
            ' <target>(...)
            Dim invocationExpression = CType(context.Node.Parent, InvocationExpressionSyntax)
            Dim target = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol ' points to the method after overload resolution
            'TODO If Not IsTargetPlatformSpecific(target) Then Return
        ElseIf context.Node.Parent.Kind = SyntaxKind.AddressOfExpression Then
            ' AddressOf <target>
            Dim target = context.SemanticModel.GetSymbolInfo(context.Node).Symbol ' points to the method after overload resolution
            'TODO If Not IsTargetPlatformSpecific(target) Then Return
        ElseIf context.Node.Parent.Kind = SyntaxKind.ObjectCreationExpression AndAlso context.Node Is CType(context.Node.Parent, ObjectCreationExpressionSyntax).Type Then
            ' New <target>
            Dim objectCreationExpression = CType(context.Node.Parent, ObjectCreationExpressionSyntax)
            Dim target = context.SemanticModel.GetSymbolInfo(objectCreationExpression).Symbol ' points to the constructor after overload resolution
            'TODO If Not IsTargetPlatformSpecific(target) Then Return
        ElseIf context.Node.Parent.Kind = SyntaxKind.AddHandlerStatement AndAlso context.Node Is CType(context.Node.Parent, AddRemoveHandlerStatementSyntax).EventExpression Then
            ' AddHandler <target>, delegate
            Dim target = context.SemanticModel.GetSymbolInfo(context.Node).Symbol ' points to the event
            'TODO If Not IsTargetPlatformSpecific(target) Then Return
        ElseIf context.Node.Parent.Kind = SyntaxKind.NameOfExpression Then
            ' NameOf(<target>)
            Return
        Else
            ' f(Of <target>)(...)  -- no warning
            ' Dim x As <target> = ...  -- no warning
            ' property access -- warning
            ' field access -- no warning
            ' method access without arguments -- warning
            Dim target = context.SemanticModel.GetSymbolInfo(context.Node).Symbol
            If target Is Nothing Then Return
            If target.Kind <> SymbolKind.Property AndAlso target.Kind <> SymbolKind.Method Then Return
            'TODO If Not IsTargetPlatformSpecific(target) Then Return
        End If


        ' Is this expression inside a method/constructor/property that claims to be platform-specific?
        Dim containingMember As DeclarationStatementSyntax = context.Node.FirstAncestorOrSelf(Of MethodBlockBaseSyntax) ' used for methods, constructors and property accessors
        If TypeOf containingMember Is AccessorBlockSyntax Then containingMember = containingMember.FirstAncestorOrSelf(Of PropertyBlockSyntax)
        If containingMember IsNot Nothing Then
            Dim containingMemberSymbol = context.SemanticModel.GetDeclaredSymbol(containingMember)
            If HasPlatformSpecificAttribute(containingMemberSymbol) Then Return
        End If

        ' Is this invocation properly guarded? See readme.txt for explanations.
        If IsProperlyGuarded(context.Node, context.SemanticModel) Then Return
        If containingMember IsNot Nothing Then
            For Each ret In containingMember.DescendantNodes.OfType(Of ReturnStatementSyntax)
                If IsProperlyGuarded(ret, context.SemanticModel) Then Return
            Next
        End If

        ' We'll report only a single diagnostic per line, the first.
        Dim loc = context.Node.GetLocation
        If Not loc.IsInSource Then Return
        Dim line = loc.GetLineSpan().StartLinePosition.Line
        If reports.ContainsKey(line) AndAlso reports(line).SourceSpan.Start <= loc.SourceSpan.Start Then Return
        reports(line) = loc
    End Sub

    Shared Function GetPlatformSpecificAttribute(symbol As ISymbol) As String
        If symbol Is Nothing Then Return Nothing
        For Each attr In symbol.GetAttributes
            If attr.AttributeClass.Name.EndsWith("SpecificAttribute") Then Return attr.AttributeClass.Name.Replace("Attribute", "")
        Next
        Return Nothing
    End Function

    Shared Function HasPlatformSpecificAttribute(symbol As ISymbol) As Boolean
        Return (GetPlatformSpecificAttribute(symbol) IsNot Nothing)
    End Function

    Shared Function GetApiTarget(symbol As ISymbol) As Platform
        ' TODO: update this function to work with newer SDK versions (when they're released)

        If symbol Is Nothing Then Return New Platform(PlatformKind.Unchecked)
        If symbol.ContainingNamespace?.ToDisplayString.StartsWith("Windows.") Then
            If symbol.ContainingType?.Name = "ApiInformation" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' HACK: these three target assemblies hard-code what's found in
            ' C:\Program Files (x86)\Windows Kits\10\Platforms\UAP\10.0.10240.0\Platform.xml
            ' Once future versions of the Windows SDK come up, we should update that list.
            Dim targetAssembly = symbol.ContainingAssembly.Name
            If targetAssembly = "Windows.Foundation.FoundationContract" OrElse
                targetAssembly = "Windows.Foundation.UniversalApiContract" OrElse
                targetAssembly = "Windows.Networking.Connectivity.WwanContract" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' HACK: I don't want to give warning when analyzing code in an 8.1 or PCL project.
            ' In those two targets, every Windows type is found in Windows.winmd, so that's how we'll suppress it:
            If targetAssembly = "Windows" Then Return New Platform(PlatformKind.Unchecked)

            ' Some WinRT types like Windows.UI.Color get projected to come from this assembly:
            If targetAssembly = "System.Runtime.WindowsRuntime" Then Return New Platform(PlatformKind.Uwp, "10240")

            ' Otherwise, it came from a platform-specific part of Windows:
            Return New Platform(PlatformKind.Ext, "10240")

        Else
            Dim attr = GetPlatformSpecificAttribute(symbol)
            If attr IsNot Nothing Then Return New Platform(PlatformKind.User, attr)
            Return New Platform(PlatformKind.Unchecked)
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
        Dim check1 = node.FirstAncestorOrSelf(Of MultiLineIfBlockSyntax)
        While check1 IsNot Nothing
            Yield check1.IfStatement.Condition
            check1 = check1.Parent.FirstAncestorOrSelf(Of MultiLineIfBlockSyntax)
        End While
        '
        Dim check2 = node.FirstAncestorOrSelf(Of SingleLineIfStatementSyntax)
        While check2 IsNot Nothing
            Yield check2.Condition
            check2 = check2.Parent.FirstAncestorOrSelf(Of SingleLineIfStatementSyntax)
        End While
    End Function

End Class




<ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
Public Class PlatformSpecificFixerVB
    Inherits CodeFixProvider

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create("UWP001")
        End Get
    End Property

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
        Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

        Dim diagnostic = context.Diagnostics.First
        Dim span = diagnostic.Location.SourceSpan
        Dim node = root.FindToken(span.Start).Parent

        ' Which node are we interested in? -- the largest IdentifierName/SimpleMemberAccess/QualifiedName
        ' that encompasses the bottom node...
        While node.Kind <> SyntaxKind.IdentifierName AndAlso node.Kind <> SyntaxKind.SimpleMemberAccessExpression AndAlso node.Kind <> SyntaxKind.QualifiedName
            node = node.Parent
            If node Is Nothing Then Return
        End While
        While True
            If node.Parent?.Kind = SyntaxKind.SimpleMemberAccessExpression Then node = node.Parent : Continue While
            If node.Parent?.Kind = SyntaxKind.QualifiedName Then node = node.Parent : Continue While
            Exit While
        End While


        ' Introduce a guard? (only if it is a method/accessor/constructor, i.e. somewhere that allows code)
        Dim containingBody = node.FirstAncestorOrSelf(Of MethodBlockBaseSyntax)
        If containingBody IsNot Nothing Then
            Dim act1 = CodeAction.Create("Add 'If ApiInformation.IsTypePresent'", Function(c) IntroduceGuardAsync(context.Document, node, c), "PlatformSpecificGuard")
            context.RegisterCodeFix(act1, diagnostic)
        End If

        ' Mark method/property/constructor as platform-specific?
        If TypeOf containingBody Is AccessorBlockSyntax Then
            Dim propStatement = containingBody.FirstAncestorOrSelf(Of PropertyBlockSyntax).PropertyStatement
            Dim name = $"Property {propStatement.Identifier.Text}"
            Dim act2 = CodeAction.Create($"Mark '{name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        ElseIf TypeOf containingBody Is MethodBlockSyntax Then
            Dim methodStatement = CType(containingBody, MethodBlockSyntax).SubOrFunctionStatement
            Dim name = $"{If(containingBody.Kind = SyntaxKind.SubBlock, "Sub", "Function")} {methodStatement.Identifier.Text}"
            Dim act2 = CodeAction.Create($"Mark '{name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(methodStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        ElseIf TypeOf containingBody Is ConstructorBlockSyntax Then
            Dim methodStatement = CType(containingBody, ConstructorBlockSyntax).SubNewStatement
            Dim name = "Sub New"
            Dim act2 = CodeAction.Create($"Mark '{name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(methodStatement, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificMethod")
            context.RegisterCodeFix(act2, diagnostic)
        End If

        'context.Document.Project.FilePath

        ' Mark some of the conditions as platform-specific?
        Dim semanticModel = Await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(False)
        For Each symbol In PlatformSpecificAnalyzerVB.GetGuards(node, semanticModel)
            If symbol.ContainingType.Name = "ApiInformation" Then Continue For
            If Not symbol.Locations.First.IsInSource Then Return
            Dim act3 As CodeAction = Nothing
            Dim symbolSyntax = Await symbol.DeclaringSyntaxReferences.First.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(False)
            Dim fieldSyntax = TryCast(symbolSyntax.Parent.Parent, FieldDeclarationSyntax)
            Dim propSyntax = TryCast(symbolSyntax, PropertyStatementSyntax)
            If fieldSyntax IsNot Nothing Then
                act3 = CodeAction.Create($"Mark field '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(fieldSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
            ElseIf propSyntax IsNot Nothing Then
                act3 = CodeAction.Create($"Mark property '{symbol.Name}' as platform-specific", Function(c) AddPlatformSpecificAttributeAsync(propSyntax, Function(n, a) n.AddAttributeLists(a), context.Document.Project.Solution, c), "PlatformSpecificSymbol" & symbol.Name)
            End If
            If act3 IsNot Nothing Then context.RegisterCodeFix(act3, diagnostic)
        Next
    End Function

    Private Async Function AddPlatformSpecificAttributeAsync(Of T As SyntaxNode)(oldSyntax As T, f As Func(Of T, AttributeListSyntax, T), solution As Solution, cancellationToken As CancellationToken) As Task(Of Solution)
        ' + <System.Runtime.CompilerServices.PlatformSpecific>
        '   Sub/Class/Dim/Property p



        Dim oldRoot = Await oldSyntax.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(False)
        Dim temp As SyntaxNode = Nothing
        Dim oldDocument = (From p In solution.Projects From d In p.Documents Where d.TryGetSyntaxRoot(temp) AndAlso temp Is oldRoot Select d).First
        '
        Dim id = SyntaxFactory.ParseTypeName("System.Runtime.CompilerServices.PlatformSpecific")
        Dim attr = SyntaxFactory.Attribute(id)
        Dim attrs = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)).WithAdditionalAnnotations(Simplifier.Annotation)
        '
        Dim newSyntax = f(oldSyntax, attrs)
        Dim newRoot = oldRoot.ReplaceNode(oldSyntax, newSyntax)
        Return solution.WithDocumentSyntaxRoot(oldDocument.Id, newRoot)
    End Function

    Private Async Function IntroduceGuardAsync(document As Document, node As SyntaxNode, cancellationToken As CancellationToken) As Task(Of Document)
        ' + If Windows.Foundation.Metadata.ApiInformation.IsTypePresent(targetContainingType) Then
        '       old-statement
        ' + End If

        Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
        Dim target = semanticModel.GetSymbolInfo(node).Symbol
        If target Is Nothing Then Return document
        Dim targetType = If(target.Kind = SymbolKind.NamedType, target, target.ContainingType)
        Dim targetName = If(targetType Is Nothing, "", targetType.ToDisplayString)
        If Not targetName.StartsWith("Windows.") Then targetName = "???"

        Dim oldStatement = node.FirstAncestorOrSelf(Of StatementSyntax)()
        Dim oldLeadingTrivia = oldStatement.GetLeadingTrivia()
        '
        Dim conditionReceiver = SyntaxFactory.ParseName("Windows.Foundation.Metadata.ApiInformation.IsTypePresent").WithAdditionalAnnotations(Simplifier.Annotation)
        Dim conditionString = SyntaxFactory.StringLiteralExpression(SyntaxFactory.StringLiteralToken($"""{targetName}""", targetName))
        Dim conditionArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(SyntaxFactory.SimpleArgument(conditionString)))
        Dim condition = SyntaxFactory.InvocationExpression(conditionReceiver, conditionArgument)
        '
        Dim ifStatement = SyntaxFactory.IfStatement(condition)
        Dim thenStatements = SyntaxFactory.SingletonList(oldStatement.WithoutLeadingTrivia())
        Dim ifBlock = SyntaxFactory.MultiLineIfBlock(ifStatement).WithStatements(thenStatements).WithLeadingTrivia(oldLeadingTrivia).WithAdditionalAnnotations(Formatter.Annotation)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
        Dim newRoot = oldRoot.ReplaceNode(oldStatement, ifBlock)
        Return document.WithSyntaxRoot(newRoot)
    End Function

End Class