param($installPath, $toolsPath, $package, $project)

$analyzerPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzerPath "Analyzer_NewLangFeatures_VS2015.dll"

$project.Object.AnalyzerReferences.Remove("$analyzerFilePath")