<#
.SYNOPSIS
    Compare Rust MiniPdf against the repository's shared visual fixtures and references.

.DESCRIPTION
    Reuses the same on-disk classic/issue fixtures and PDF comparison pipeline as
    the .NET benchmarks. Microsoft 365 is the default reference engine, with
    LibreOffice available through -Engine libre. It does not execute C# xUnit tests.

.EXAMPLE
    .\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format xlsx
    .\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format xlsx -Engine office -Filter "classic180" -ForceReference
    .\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format xlsx -Engine libre
    .\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format docx -Filter "classic01"
    .\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format xlsx
    .\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format docx -Filter "SA8000"
#>

param(
    [ValidateSet("classic", "issue")]
    [string]$Suite = "classic",
    [ValidateSet("xlsx", "docx", "pptx")]
    [string]$Format = "xlsx",
    [ValidateSet("office", "libre")]
    [string]$Engine = "office",
    [string]$Filter,
    [int]$MaxCases = 0,
    [int]$MaxComparePages = 0,
    [string]$SourceDir,
    [string]$CandidateDir,
    [string]$ReferenceDir,
    [string]$ReportDir,
    [double]$MinimumScore = 0.95,
    [switch]$SkipCandidate,
    [switch]$ForceReference,
    [switch]$SkipReference
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ($Format -eq "pptx") {
    throw "Rust MiniPdf does not support .pptx. Supported formats: xlsx, docx."
}
if ($MaxCases -lt 0) {
    throw "MaxCases must be zero (all cases) or a positive number."
}
if ($MaxComparePages -lt 0) {
    throw "MaxComparePages must be zero (all pages) or a positive number."
}

function Resolve-RepoPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) { return $PathValue }
    return Join-Path $RepoRoot $PathValue
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
}

$Config = $Defaults["$Suite`:$Format"]
$SourceDir = Resolve-RepoPath $(if ($SourceDir) { $SourceDir } else { $Config.Source })
$DefaultReference = if ($Engine -eq "office") { $Config.OfficeReference } else { $Config.LibreReference }
$ReferenceDir = Resolve-RepoPath $(if ($ReferenceDir) { $ReferenceDir } else { $DefaultReference })
$ReferenceLabel = if ($Engine -eq "office") { $Config.OfficeLabel } else { "LibreOffice Reference" }
$IsFocusedRun = -not [string]::IsNullOrWhiteSpace($Filter) -or $MaxCases -gt 0
$DefaultArtifactRoot = "artifacts/rust-benchmark/$Suite/$Format"
if ($IsFocusedRun) {
    $FilterLabel = if ([string]::IsNullOrWhiteSpace($Filter)) { "all" } else { $Filter -replace '[^A-Za-z0-9._-]', '_' }
    $CaseLabel = if ($MaxCases -gt 0) { "max-$MaxCases" } else { "all" }
    $DefaultArtifactRoot = "artifacts/rust-benchmark/focused/$Suite/$Format/$FilterLabel-$CaseLabel"
}
$CandidateDir = Resolve-RepoPath $(if ($CandidateDir) { $CandidateDir } else { "$DefaultArtifactRoot/candidates" })
$ReportDir = Resolve-RepoPath $(if ($ReportDir) { $ReportDir } else { "$DefaultArtifactRoot/report" })

$Cargo = Join-Path $env:USERPROFILE ".cargo/bin/cargo.exe"
$Python = Join-Path $RepoRoot ".venv/Scripts/python.exe"
if (-not (Test-Path $Cargo)) { $Cargo = (Get-Command cargo -ErrorAction Stop).Source }
if (-not (Test-Path $Python)) { $Python = (Get-Command python -ErrorAction Stop).Source }

$CargoManifest = Join-Path $RepoRoot "minipdf-rs/Cargo.toml"
$ReferenceScript = Resolve-RepoPath $(if ($Engine -eq "office") { $Config.OfficeReferenceScript } else { $Config.LibreReferenceScript })
$CompareScript = Join-Path $RepoRoot "tests/MiniPdf.Benchmark/compare_pdfs.py"
$ComparisonManifest = Join-Path $ReportDir "comparison_manifest.json"
$CoverageManifest = Join-Path $ReportDir "benchmark_coverage.json"

$SourceFiles = @(Get-ChildItem $SourceDir -File -Filter "*.$Format" | Where-Object {
    -not $Filter -or $_.BaseName -like "*$Filter*"
} | Sort-Object Name)
if ($MaxCases -gt 0) {
    $SourceFiles = @($SourceFiles | Select-Object -First $MaxCases)
}
if ($SourceFiles.Count -eq 0) {
    throw "No .$Format files matched '$Filter' in $SourceDir"
}

New-Item -ItemType Directory -Force -Path $CandidateDir, $ReferenceDir, $ReportDir | Out-Null

$Cases = @($SourceFiles | ForEach-Object {
    [pscustomobject]@{
        name = $_.BaseName
        case_id = $_.BaseName
        suite = $Suite
        format = $Format
        source_path = [System.IO.Path]::GetRelativePath($RepoRoot, $_.FullName).Replace("\", "/")
        conversion_status = "pending"
        conversion_exit_code = $null
        candidate_exists = $false
        reference_exists = $false
    }
})

[pscustomobject]@{ cases = $Cases } | ConvertTo-Json -Depth 5 | Set-Content $ComparisonManifest -Encoding UTF8

Write-Host "Rust benchmark matrix: suite=$Suite format=$Format engine=$Engine selected=$($Cases.Count)"
Write-Host "Shared fixtures only; C# xUnit assertions are not executed by this command."

$Cli = Join-Path $RepoRoot "minipdf-rs/target/debug/minipdf.exe"
if (-not $SkipCandidate) {
    & $Cargo build --manifest-path $CargoManifest -p minipdf-cli
    if ($LASTEXITCODE -ne 0) { throw "Rust CLI build failed." }

    for ($Index = 0; $Index -lt $SourceFiles.Count; $Index++) {
        $SourceFile = $SourceFiles[$Index]
        $OutputFile = Join-Path $CandidateDir ($SourceFile.BaseName + ".pdf")
        if (Test-Path $OutputFile) { Remove-Item $OutputFile -Force }

        & $Cli $SourceFile.FullName -o $OutputFile
        $ExitCode = $LASTEXITCODE
        $Cases[$Index].conversion_exit_code = $ExitCode
        $Cases[$Index].candidate_exists = Test-Path $OutputFile
        $Cases[$Index].conversion_status = if ($ExitCode -eq 0 -and $Cases[$Index].candidate_exists) { "passed" } else { "failed" }
    }
} else {
    for ($Index = 0; $Index -lt $SourceFiles.Count; $Index++) {
        $OutputFile = Join-Path $CandidateDir ($SourceFiles[$Index].BaseName + ".pdf")
        $Cases[$Index].candidate_exists = Test-Path $OutputFile
        $Cases[$Index].conversion_status = if ($Cases[$Index].candidate_exists) { "passed" } else { "failed" }
    }
}

if (-not $SkipReference) {
    $ReferenceFilters = if ($MaxCases -gt 0) { @($SourceFiles.BaseName) } else { @($Filter) }
    foreach ($ReferenceFilter in $ReferenceFilters) {
        $ReferenceArgs = @($ReferenceScript, $Config.SourceArgument, $SourceDir, "--pdf-dir", $ReferenceDir)
        if ($ReferenceFilter) { $ReferenceArgs += @("--filter", $ReferenceFilter) }
        if ($ForceReference) { $ReferenceArgs += "--force" }
        & $Python @ReferenceArgs
        if ($LASTEXITCODE -ne 0) { throw "$ReferenceLabel generation failed." }
    }
}

foreach ($Case in $Cases) {
    $Case.reference_exists = Test-Path (Join-Path $ReferenceDir ($Case.name + ".pdf"))
}

$PassedConversions = @($Cases | Where-Object { $_.conversion_status -eq "passed" }).Count
$FailedConversions = $Cases.Count - $PassedConversions
$MissingReferences = @($Cases | Where-Object { -not $_.reference_exists }).Count
$Coverage = [pscustomobject]@{
    suite = $Suite
    format = $Format
    reference_engine = $Engine
    reference_label = $ReferenceLabel
    fixture_scope = "shared-on-disk-fixtures"
    executes_dotnet_xunit = $false
    max_compare_pages = $MaxComparePages
    selected_cases = $Cases.Count
    passed_conversions = $PassedConversions
    failed_conversions = $FailedConversions
    missing_references = $MissingReferences
    comparison_completed = $false
    comparison_results = 0
    average_score = $null
    cases = $Cases
}
$Coverage | ConvertTo-Json -Depth 6 | Set-Content $CoverageManifest -Encoding UTF8

$CompareArgs = @(
    $CompareScript,
    "--minipdf-dir", $CandidateDir,
    "--reference-dir", $ReferenceDir,
    "--report-dir", $ReportDir,
    "--manifest", $ComparisonManifest,
    "--report-scope", "rust-$Suite-$Format",
    "--composite-images",
    "--heatmaps",
    "--candidate-label", "Rust MiniPdf",
    "--reference-label", $ReferenceLabel
)
if ($MaxComparePages -gt 0) { $CompareArgs += @("--max-pages", $MaxComparePages) }
& $Python @CompareArgs
if ($LASTEXITCODE -ne 0) { throw "PDF comparison failed." }

$Results = @(Get-Content (Join-Path $ReportDir "comparison_report.json") -Raw | ConvertFrom-Json)
$BelowThreshold = @($Results | Where-Object { $null -eq $_.overall_score -or $_.overall_score -lt $MinimumScore })
$Average = ($Results | Where-Object { $null -ne $_.overall_score } | Measure-Object -Property overall_score -Average).Average
$Coverage.comparison_completed = $true
$Coverage.comparison_results = $Results.Count
$Coverage.average_score = $Average
$Coverage | ConvertTo-Json -Depth 6 | Set-Content $CoverageManifest -Encoding UTF8

Write-Host "Coverage: selected=$($Cases.Count), converted=$PassedConversions, failed=$FailedConversions, missing references=$MissingReferences"
Write-Host "Visual results: compared=$($Results.Count), average score=$([math]::Round($Average, 4))"
Write-Host "Coverage manifest: $CoverageManifest"
Write-Host "Visual report: $(Join-Path $ReportDir 'comparison_report.md')"

if ($FailedConversions -gt 0 -or $MissingReferences -gt 0 -or $BelowThreshold.Count -gt 0) {
    $ThresholdFailures = ($BelowThreshold | ForEach-Object { "$($_.name)=$($_.overall_score)" }) -join ", "
    throw "Rust benchmark failed: conversion failures=$FailedConversions, missing references=$MissingReferences, below $MinimumScore=[$ThresholdFailures]"
}