Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.MSBuild
Imports Microsoft.CodeAnalysis.Text

' This console app looks at a sample app in the solution,
' And runs all the analyzers + fixers available on it,
' And displays the output in a console app.
Module TestHarness
    Sub Main()
        MainAsync().GetAwaiter().GetResult()

        ' We have to at least mention the types in our referenced assemblies, else 
        ' the compiler won't bother emitting a reference to them
        Dim x1 As Analyzer_NewLangFeatures_VS2015.UseNameofAnalyzerCS = Nothing
        'Dim x2 As Analyzer_NewLangFeaturesVB14.UseNameOfAnalyzerVB = Nothing
        'Dim x3 As Analyzer_XamlTips.UseStaticPropertyChangedEventArgsAnalyzerCS = Nothing
        'Dim x4 As Analyzer_CheckedExceptions.CheckedExceptionsCS = Nothing
    End Sub


    Async Function MainAsync() As Task
        ' Assemble the list of analyzers And fixers from all referenced assemblies
        Dim analyzers = {New List(Of DiagnosticAnalyzer), New List(Of DiagnosticAnalyzer)}
        Dim fixers = {New List(Of CodeFixProvider), New List(Of CodeFixProvider)}
        Const VB = 0, CSHARP = 1
        Dim languages As New Dictionary(Of String, Integer) From {{LanguageNames.CSharp, CSHARP}, {LanguageNames.VisualBasic, VB}}
        For Each dll In GetType(TestHarness).Assembly.GetReferencedAssemblies()
            If dll.Name = "mscorlib" OrElse dll.Name.StartsWith("Microsoft.CodeAnalysis") Then Continue For
            Dim assembly = AppDomain.CurrentDomain.Load(dll)
            For Each type In assembly.GetTypes()
                For Each attr In type.CustomAttributes
                    If attr.AttributeType.Name = "DiagnosticAnalyzerAttribute" Then
                        analyzers(languages(attr.ConstructorArguments(0).Value.ToString)).Add(CType(Activator.CreateInstance(type), DiagnosticAnalyzer))
                    End If
                    If attr.AttributeType.Name = "ExportCodeFixProviderAttribute" Then
                        Dim args = attr.ConstructorArguments(1).Value
                        For Each arg In CType(args, IEnumerable)
                            Dim trimmed = arg.ToString().Trim(""""c)
                            fixers(languages(trimmed)).Add(CType(Activator.CreateInstance(type), CodeFixProvider))
                        Next
                    End If
                Next
            Next
        Next

        ' Now run our chosen analyzers and fixers on the sample project
        Dim workspace = MSBuildWorkspace.Create()
        Dim project = Await workspace.OpenProjectAsync("..\..\..\DemoAppConsoleCS\DemoAppConsoleCS.csproj")
        Dim compilation = Await project.GetCompilationAsync()

        Dim LANG = languages(project.Language)
        Console.WriteLine("Analyzers: {0}", String.Join(", ", From a In analyzers(LANG) Select a.GetType().Name))
        Console.WriteLine("Fixers: {0}", String.Join(", ", From f In fixers(LANG) Select f.GetType().Name))
        Console.WriteLine("---")


        ' treeAnalyzer.Compilation = compilation; // another bit of the above hack

        Dim compilation2 As Compilation = Nothing
        Dim driver = AnalyzerDriver.Create(compilation, analyzers(LANG).AsImmutable(), Nothing, compilation2, CancellationToken.None)
        Dim diagnostics_internal = compilation2.GetDiagnostics()
        Dim diagnostics = Await driver.GetDiagnosticsAsync()

        ' For each diagnostic, we'll print it out, and then try running each suggested fix
        For Each diagnostic In diagnostics

            ' We'll print the name of the diagnostic, and ^^^ to indicate where the squiggle goes
            Console.WriteLine("DIAGNOSIS: {0}:{1}", diagnostic.Id, diagnostic.GetMessage())
            If diagnostic.Location.IsInSource Then
                Dim span = diagnostic.Location.SourceSpan
                Dim sourceText = diagnostic.Location.SourceTree.GetText()
                Dim sourceSpan = sourceText.Lines.GetLinePositionSpan(span)
                For i = sourceSpan.Start.Line To sourceSpan.End.Line
                    Dim line = sourceText.Lines(i).ToString()
                    Console.WriteLine("   {1}", i + 1, line)
                    Dim left = 0 : If i = sourceSpan.Start.Line Then left = sourceSpan.Start.Character
                    Dim length = line.Length - left : If i = sourceSpan.End.Line Then length = sourceSpan.End.Character - left
                    Console.WriteLine("   {0}{1}", New String(" "c, left), New String("^"c, length))
                Next i
            End If

            ' Now run each of the suggested fixes
            Dim oldDocument = project.GetDocument(diagnostic.Location.SourceTree)
            For Each fixer In fixers(LANG)
                Dim fixableIds = fixer.GetFixableDiagnosticIds().ToList()
                If Not fixableIds.Contains(diagnostic.Id) Then Continue For
                Dim fixes As New List(Of CodeAction)
                Dim context = New CodeFixContext(oldDocument, diagnostic, Sub(a, d) fixes.Add(a), CancellationToken.None)
                Await fixer.ComputeFixesAsync(context)
                For Each fixup In fixes
                    ' For each fix, we'll print the name of the fix
                    Console.WriteLine("AFTER {0}:", fixup.Title)
                    Dim operations = Await fixup.GetOperationsAsync(CancellationToken.None)
                    Dim solution = operations.OfType(Of ApplyChangesOperation).Single.ChangedSolution
                    Dim newDocument = solution.GetDocument(oldDocument.Id)

                    ' Then we'll print a "diff" of the effects of the suggested fix.
                    ' It would be more elegant to use Meyer's diff algorithm.
                    ' This code gives slightly worse output, but is less complex.
                    Dim oldText = Await oldDocument.GetTextAsync()
                    Dim newText = Await newDocument.GetTextAsync()
                    Dim changes = Await newDocument.GetTextChangesAsync(oldDocument)
                    Dim lineChanges As New List(Of Tuple(Of LinePositionSpan, LinePositionSpan))
                    Dim runningDiff = 0
                    For Each change In changes
                        Dim oldLineSpan = oldText.Lines.GetLinePositionSpan(change.Span)
                        Dim newSpan As New TextSpan(change.Span.Start + runningDiff, change.NewText.Length)
                        Dim newLineSpan = newText.Lines.GetLinePositionSpan(newSpan)
                        runningDiff += change.NewText.Length - change.Span.Length
                        If lineChanges.Count > 0 AndAlso lineChanges.Last.Item1.End.Line >= oldLineSpan.Start.Line Then
                            oldLineSpan = New LinePositionSpan(lineChanges.Last.Item1.Start, oldLineSpan.End)
                            newLineSpan = New LinePositionSpan(lineChanges.Last.Item2.Start, newLineSpan.End)
                            lineChanges(lineChanges.Count - 1) = Tuple.Create(oldLineSpan, newLineSpan)
                        Else
                            lineChanges.Add(Tuple.Create(oldLineSpan, newLineSpan))
                        End If
                    Next change
                    For Each change In lineChanges
                        Dim isPureAddition = (change.Item1.Start = change.Item1.End)
                        Dim isPureRemoval = (change.Item2.Start = change.Item2.End)
                        Dim oldChar = If(isPureRemoval, "- ", "< ")
                        Dim newChar = If(isPureAddition, " +", " >")
                        If Not isPureAddition Then
                            For i = change.Item1.Start.Line To change.Item1.End.Line
                                Console.WriteLine("{0} {1}", oldChar, oldText.Lines(i))
                            Next
                        End If
                        If Not isPureRemoval Then
                            For i = change.Item2.Start.Line To change.Item2.End.Line
                                Console.WriteLine("{0} {1}", newChar, newText.Lines(i))
                            Next
                        End If
                    Next change
                    Console.WriteLine()
                Next fixup
            Next fixer

        Next diagnostic
    End Function
End Module

'Static Async Task MainAsync()
'    {

'        // HACK: the existing AnalyzerDriver just bypasses syntax node analyzers, so I have To drive them manually
'        var syntaxNodeAnalyzers = analyzers.OfType < ISyntaxNodeAnalyzer < SyntaxKind >> ().ToList();
'        var otherAnalyzers = analyzers.Where(a >=!(a Is ISyntaxNodeAnalyzer < SyntaxKind >)).ToList();
'        var treeAnalyzer = New HackSyntaxTreeAnalyzer();
'        var rules = New List < DiagnosticDescriptor > ();
'        foreach (var analyzer in syntaxNodeAnalyzers) rules.AddRange(analyzer.SupportedDiagnostics);
'        treeAnalyzer.SupportedDiagnostics = ImmutableArray.CreateRange(rules);
'        treeAnalyzer.SyntaxNodeAnalyzers = syntaxNodeAnalyzers;
'        otherAnalyzers.Add(treeAnalyzer);
'        analyzers = otherAnalyzers;
'        // END HACK


'    }
'}

'Public Class HackSyntaxTreeAnalyzer :  ISyntaxTreeAnalyzer
'{
'    Public List<ISyntaxNodeAnalyzer<SyntaxKind>> SyntaxNodeAnalyzers { Get; Set; }
'    Public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { Get; Set; }
'    Public Compilation Compilation { Get; Set; }
'    Public void AnalyzeSyntaxTree(SyntaxTree tree, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
'    {
'        var semanticModel = Compilation.GetSemanticModel(tree);
'        foreach (var analyzer in SyntaxNodeAnalyzers)
'        {
'            var interests = analyzer.SyntaxKindsOfInterest.ToList();
'            foreach (var node in tree.GetRoot().DescendantNodes())
'            {
'                If (!interests.Contains(node.CSharpKind())) Continue;
'                analyzer.AnalyzeNode(node, semanticModel, addDiagnostic, options, cancellationToken);
'            }
'        }
'    }
'}
