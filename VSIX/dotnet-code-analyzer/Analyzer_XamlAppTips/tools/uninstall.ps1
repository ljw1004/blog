param($installPath, $toolsPath, $package, $project)

$analyzerPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzerPath "Analyzer_XamlAppTips.dll"

$project.Object.AnalyzerReferences.Remove("$analyzerFilePath")