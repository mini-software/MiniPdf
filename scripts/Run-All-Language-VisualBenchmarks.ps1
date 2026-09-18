[CmdletBinding()]
param(
    [switch]$Publish,
    [string]$PagesRepository = (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "minipdf-web-page"),
    [ValidateSet("classic", "issue")]
    [string]$Suite,
    [ValidateSet("xlsx", "docx", "pptx")]
    [string]$Format,
    [ValidateSet("o365", "office", "libre")]
    [string]$Engine,
    [string]$Filter,
    [int]$MaxCases,
    [int]$MaxComparePages,
    [double]$MinimumScore,
    [string]$SourceDir,
    [string]$CandidateDir,
    [string]$ReferenceDir,
    [string]$AuxiliaryReferenceDir,
    [string]$ReportDir,
    [string]$ArtifactRoot,
    [switch]$SkipCandidate,
    [switch]$SkipBuild,
    [switch]$SkipReference,
    [switch]$ForceReference
)

$ErrorActionPreference = "Stop"
$BenchmarkParameters = @{}
foreach ($Entry in $PSBoundParameters.GetEnumerator()) {
    if ($Entry.Key -notin @("Publish", "PagesRepository")) {
        $BenchmarkParameters[$Entry.Key] = $Entry.Value
    }
}

foreach ($Language in @("dotnet", "rust", "java", "go", "python", "node")) {
    Write-Host "`n=== $Language visual benchmark ==="
    & (Join-Path $PSScriptRoot "Invoke-LanguageVisualBenchmark.ps1") -Language $Language @BenchmarkParameters
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if ($Publish) {
    Write-Host "`n=== Publish visual benchmark dashboard ==="
    & (Join-Path $PSScriptRoot "Publish-VisualBenchmarkPages.ps1") -DestinationRoot $PagesRepository -Clean
}
