<#
.SYNOPSIS
    Runs one MiniPdf implementation against the repository's visual benchmark fixtures.
.DESCRIPTION
    Uses Microsoft 365 as the primary scored reference and LibreOffice as the
    required auxiliary reference. Outputs are isolated below
    artifacts/<language>-benchmark/<suite>/<format>.
.EXAMPLE
    .\scripts\Run-DotNet-VisualBenchmark.ps1 -Suite classic -Format xlsx -MaxCases 1
    .\scripts\Run-DotNet-VisualBenchmark.ps1 -Suite issue -Format pptx -Filter "Asian Pacific"
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dotnet")]
    [string]$Language,
    [ValidateSet("classic", "issue")]
    [string]$Suite = "classic",
    [ValidateSet("xlsx", "docx", "pptx")]
    [string]$Format = "xlsx",
    [ValidateSet("o365", "office", "libre")]
    [string]$Engine = "o365",
    [string]$Filter,
    [int]$MaxCases = 0,
    [int]$MaxComparePages = 0,
    [double]$MinimumScore = 0.95,
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
$RepoRoot = Split-Path -Parent $PSScriptRoot
if ($MaxCases -lt 0) { throw "MaxCases cannot be negative." }
if ($MaxComparePages -lt 0) { throw "MaxComparePages cannot be negative." }
if ($MinimumScore -lt 0.0 -or $MinimumScore -gt 1.0) {
    throw "MinimumScore must be between 0.0 and 1.0."
}
function Resolve-RepoPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) { return $PathValue }
    return Join-Path $RepoRoot $PathValue
}
function Get-RepoRelativePath([string]$PathValue) {
    $FullRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
    $FullPath = [System.IO.Path]::GetFullPath($PathValue)
    $RepoPrefix = $FullRepoRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $FullPath.StartsWith($RepoPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$PathValue' is outside repository root '$RepoRoot'."
    }
    return $FullPath.Substring($RepoPrefix.Length).Replace("\", "/")
}
function Write-Json([object]$Value, [string]$Path) {
    $Json = $Value | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText($Path, $Json, [System.Text.UTF8Encoding]::new($false))
}
function Assert-CommandSucceeded([string]$Description) {
    if ($LASTEXITCODE -ne 0) { throw "$Description failed with exit code $LASTEXITCODE." }
}
function Find-Command([string]$Name) {
    return (Get-Command $Name -ErrorAction Stop).Source
}
function Find-Python {
    $RelativePath = if ($IsWindows) { ".venv/Scripts/python.exe" } else { ".venv/bin/python" }
    $Candidate = Join-Path $RepoRoot $RelativePath
    if (Test-Path -LiteralPath $Candidate) { return $Candidate }
    return Find-Command "python"
}
function Test-Pdf([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    $Bytes = [System.IO.File]::ReadAllBytes($Path)
    return $Bytes.Length -ge 5 -and [System.Text.Encoding]::ASCII.GetString($Bytes, 0, 5) -eq "%PDF-"
}
$Defaults = @{
    "classic:xlsx" = @{
        Source = "tests/MiniPdf.Scripts/output"
        LibreReference = "tests/MiniPdf.Benchmark/reference_pdfs"
        OfficeReference = "tests/MiniPdf.Benchmark/office_pdfs"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs.py"
        SourceArgument = "--xlsx-dir"
        OfficeLabel = "Microsoft 365 Excel Reference"
    }
    "classic:docx" = @{
        Source = "tests/MiniPdf.Scripts/output_docx"
        LibreReference = "tests/MiniPdf.Benchmark/reference_pdfs_docx"
        OfficeReference = "tests/MiniPdf.Benchmark/office_pdfs_docx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_docx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_docx.py"
        SourceArgument = "--docx-dir"
        OfficeLabel = "Microsoft 365 Word Reference"
    }
    "issue:xlsx" = @{
        Source = "tests/Issue_Files/xlsx"
        LibreReference = "tests/Issue_Files/reference_xlsx"
        OfficeReference = "tests/Issue_Files/office_xlsx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs.py"
        SourceArgument = "--xlsx-dir"
        OfficeLabel = "Microsoft 365 Excel Reference"
    }
    "issue:docx" = @{
        Source = "tests/Issue_Files/docx"
        LibreReference = "tests/Issue_Files/reference_docx"
        OfficeReference = "tests/Issue_Files/office_docx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_docx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_docx.py"
        SourceArgument = "--docx-dir"
        OfficeLabel = "Microsoft 365 Word Reference"
    }
    "issue:pptx" = @{
        Source = "tests/Issue_Files/pptx"
        LibreReference = "tests/Issue_Files/reference_pptx"
        OfficeReference = "tests/Issue_Files/office_pptx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_pptx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_pptx.py"
        SourceArgument = "--pptx-dir"
        OfficeLabel = "Microsoft 365 PowerPoint Reference"
    }
}
$Config = $Defaults["$Suite`:$Format"]
if (-not $Config) {
    throw "No $Language benchmark fixtures are configured for suite=$Suite format=$Format."
}
if ($Engine -eq "libre") {
    Write-Warning "-Engine libre is retained for compatibility. Microsoft 365 remains the primary scored reference; LibreOffice is auxiliary."
}
$SourceDir = Resolve-RepoPath $(if ($SourceDir) { $SourceDir } else { $Config.Source })
$ReferenceDir = Resolve-RepoPath $(if ($ReferenceDir) { $ReferenceDir } else { $Config.OfficeReference })
$AuxiliaryReferenceDir = Resolve-RepoPath $(if ($AuxiliaryReferenceDir) { $AuxiliaryReferenceDir } else { $Config.LibreReference })
$ArtifactRoot = Resolve-RepoPath $(if ($ArtifactRoot) { $ArtifactRoot } else { "artifacts/$Language-benchmark/$Suite/$Format" })
$CandidateDir = Resolve-RepoPath $(if ($CandidateDir) { $CandidateDir } else { Join-Path $ArtifactRoot "candidates" })
$ReportDir = Resolve-RepoPath $(if ($ReportDir) { $ReportDir } else { Join-Path $ArtifactRoot "report" })
$ComparisonManifest = Join-Path $ReportDir "comparison_manifest.json"
$CoverageManifest = Join-Path $ReportDir "benchmark_coverage.json"
$SourceFiles = @(Get-ChildItem -LiteralPath $SourceDir -File -Filter "*.$Format" | Where-Object {
    -not $Filter -or $_.BaseName -like "*$Filter*"
} | Sort-Object Name)
if ($MaxCases -gt 0) {
    $SourceFiles = @($SourceFiles | Select-Object -First $MaxCases)
}
if ($SourceFiles.Count -eq 0) {
    throw "No .$Format files matched '$Filter' in $SourceDir"
}
if (Test-Path -LiteralPath $ReportDir) {
    Remove-Item -LiteralPath $ReportDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $CandidateDir, $ReferenceDir, $AuxiliaryReferenceDir, $ReportDir | Out-Null
$SelectedCases = @($SourceFiles | ForEach-Object {
    [pscustomobject]@{
        name = $_.BaseName
        case_id = $_.BaseName
        suite = $Suite
        format = $Format
        source_path = Get-RepoRelativePath $_.FullName
        conversion_status = "pending"
        conversion_exit_code = $null
        candidate_exists = $false
        reference_exists = $false
        auxiliary_reference_exists = $false
    }
})
$CaseSources = @{}
for ($Index = 0; $Index -lt $SelectedCases.Count; $Index++) {
    $CaseSources[$SelectedCases[$Index].case_id] = $SourceFiles[$Index].FullName
}
Write-Json ([pscustomobject]@{ cases = $SelectedCases }) $ComparisonManifest
$Tools = @{}
if (-not $SkipCandidate -and -not $SkipBuild) {
    switch ($Language) {
        "dotnet" {
            $Tools.dotnet = Find-Command "dotnet"
            & $Tools.dotnet build (Join-Path $RepoRoot "src/MiniPdf.Cli/MiniPdf.Cli.csproj") -c Release
            Assert-CommandSucceeded ".NET CLI build"
        }
    }
}
if (-not $SkipCandidate) {
    switch ($Language) {
        "dotnet" {
            if (-not $Tools.dotnet) { $Tools.dotnet = Find-Command "dotnet" }
            $Tools.cli = Get-ChildItem (Join-Path $RepoRoot "src/MiniPdf.Cli/bin/Release") -Recurse -Filter "MiniPdf.Cli.dll" |
                Where-Object FullName -Match 'net9\.0' | Select-Object -First 1 -ExpandProperty FullName
        }
    }
    if ((-not $Tools.cli -or -not (Test-Path -LiteralPath $Tools.cli))) {
        throw "$Language CLI artifact was not found. Run without -SkipBuild first."
    }
    foreach ($Case in $SelectedCases) {
        $InputPath = $CaseSources[$Case.case_id]
        $OutputPath = Join-Path $CandidateDir ($Case.name + ".pdf")
        if (Test-Path -LiteralPath $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }
        switch ($Language) {
            "dotnet" { & $Tools.dotnet $Tools.cli $InputPath -o $OutputPath }
        }
        $Case.conversion_exit_code = $LASTEXITCODE
        $Case.candidate_exists = Test-Pdf $OutputPath
        $Case.conversion_status = if ($LASTEXITCODE -eq 0 -and $Case.candidate_exists) { "passed" } else { "failed" }
    }
} else {
    foreach ($Case in $SelectedCases) {
        $OutputPath = Join-Path $CandidateDir ($Case.name + ".pdf")
        $Case.candidate_exists = Test-Pdf $OutputPath
        $Case.conversion_status = if ($Case.candidate_exists) { "passed" } else { "failed" }
    }
}
if (-not $SkipReference) {
    $Python = Find-Python
    $OfficeReferenceScript = Resolve-RepoPath $Config.OfficeReferenceScript
    $LibreReferenceScript = Resolve-RepoPath $Config.LibreReferenceScript
    $ReferenceFilters = if ($MaxCases -gt 0) { @($SourceFiles.BaseName) } else { @($Filter) }
    foreach ($ReferenceFilter in $ReferenceFilters) {
        $Providers = @(
            [pscustomobject]@{ Script = $OfficeReferenceScript; Directory = $ReferenceDir; Label = $Config.OfficeLabel },
            [pscustomobject]@{ Script = $LibreReferenceScript; Directory = $AuxiliaryReferenceDir; Label = "LibreOffice" }
        )
        foreach ($Provider in $Providers) {
            $ReferenceArgs = @($Provider.Script, $Config.SourceArgument, $SourceDir, "--pdf-dir", $Provider.Directory)
            if ($ReferenceFilter) { $ReferenceArgs += @("--filter", $ReferenceFilter) }
            if ($ForceReference) { $ReferenceArgs += "--force" }
            & $Python -X utf8 @ReferenceArgs
            Assert-CommandSucceeded "$($Provider.Label) generation"
        }
    }
}
foreach ($Case in $SelectedCases) {
    $Case.reference_exists = Test-Pdf (Join-Path $ReferenceDir ($Case.name + ".pdf"))
    $Case.auxiliary_reference_exists = Test-Pdf (Join-Path $AuxiliaryReferenceDir ($Case.name + ".pdf"))
}
$PassedConversions = @($SelectedCases | Where-Object conversion_status -eq "passed").Count
$MissingReferences = @($SelectedCases | Where-Object reference_exists -eq $false).Count
$MissingAuxiliaryReferences = @($SelectedCases | Where-Object auxiliary_reference_exists -eq $false).Count
$Coverage = [pscustomobject]@{
    language = $Language
    suite = $Suite
    format = $Format
    reference_engine = "o365"
    reference_label = $Config.OfficeLabel
    auxiliary_reference_engine = "libreoffice"
    auxiliary_reference_label = "LibreOffice (auxiliary)"
    fixture_scope = "shared-on-disk-fixtures"
    executes_dotnet_xunit = $false
    max_compare_pages = $MaxComparePages
    selected_cases = $SelectedCases.Count
    passed_conversions = $PassedConversions
    failed_conversions = $SelectedCases.Count - $PassedConversions
    missing_references = $MissingReferences
    missing_auxiliary_references = $MissingAuxiliaryReferences
    comparison_completed = $false
    comparison_results = 0
    average_score = $null
    cases = $SelectedCases
}
Write-Json $Coverage $CoverageManifest
$Python = Find-Python
$CompareArgs = @(
    (Join-Path $RepoRoot "tests/MiniPdf.Benchmark/compare_pdfs.py"),
    "--minipdf-dir", $CandidateDir,
    "--reference-dir", $ReferenceDir,
    "--auxiliary-dir", $AuxiliaryReferenceDir,
    "--report-dir", $ReportDir,
    "--manifest", $ComparisonManifest,
    "--report-scope", "$Language-$Suite-$Format",
    "--candidate-label", "$Language MiniPdf",
    "--reference-label", $Config.OfficeLabel,
    "--auxiliary-label", "LibreOffice",
    "--composite-images",
    "--heatmaps"
)
if ($MaxComparePages -gt 0) { $CompareArgs += @("--max-pages", $MaxComparePages) }
& $Python -X utf8 @CompareArgs
Assert-CommandSucceeded "$Language visual comparison"
$ParsedResults = Get-Content (Join-Path $ReportDir "comparison_report.json") -Raw | ConvertFrom-Json
if ($ParsedResults.PSObject.Properties.Name -contains "results") {
    $ParsedResults = $ParsedResults.results
}
$Results = @($ParsedResults | ForEach-Object { $_ })
$Scores = @($Results | Where-Object { $null -ne $_.overall_score })
$Coverage.comparison_completed = $true
$Coverage.comparison_results = ($Results | Measure-Object).Count
$Coverage.average_score = if ($Scores.Count -gt 0) {
    ($Scores | Measure-Object -Property overall_score -Average).Average
} else { $null }
Write-Json $Coverage $CoverageManifest
$BelowThreshold = @($Results | Where-Object { $null -eq $_.overall_score -or $_.overall_score -lt $MinimumScore })
Write-Host "$Language benchmark: suite=$Suite format=$Format selected=$($SelectedCases.Count), converted=$PassedConversions, missing O365 references=$MissingReferences, missing LibreOffice references=$MissingAuxiliaryReferences"
Write-Host "Coverage: $CoverageManifest"
Write-Host "Report: $(Join-Path $ReportDir 'comparison_report.md')"
if ($PassedConversions -ne $SelectedCases.Count -or $MissingReferences -gt 0 -or $MissingAuxiliaryReferences -gt 0 -or $BelowThreshold.Count -gt 0) {
    throw "$Language benchmark failed: conversion failures=$($SelectedCases.Count - $PassedConversions), missing O365 references=$MissingReferences, missing LibreOffice references=$MissingAuxiliaryReferences, below MinimumScore=$($BelowThreshold.Count)."
}
