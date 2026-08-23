<#
.SYNOPSIS
    Run and summarize every Rust-supported shared visual benchmark suite.

.DESCRIPTION
    Covers classic/issue XLSX and DOCX fixtures. This does not execute C# xUnit
    tests. PPTX is excluded because the Rust converter does not support it.
#>

param(
    [int]$MaxComparePages = 1,
    [double]$MinimumScore = 0,
    [switch]$SkipReference,
    [switch]$ForceReference,
    [switch]$ReportOnly
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Runner = Join-Path $PSScriptRoot "Run-Rust-Benchmark.ps1"
$ArtifactRoot = Join-Path $RepoRoot "artifacts/rust-benchmark"
$MatrixJson = Join-Path $ArtifactRoot "benchmark_matrix.json"
$MatrixMarkdown = Join-Path $ArtifactRoot "benchmark_matrix.md"
$Targets = @(
    [pscustomobject]@{ Suite = "classic"; Format = "xlsx" },
    [pscustomobject]@{ Suite = "classic"; Format = "docx" },
    [pscustomobject]@{ Suite = "issue"; Format = "xlsx" },
    [pscustomobject]@{ Suite = "issue"; Format = "docx" }
)

if (-not $ReportOnly) {
    foreach ($Target in $Targets) {
        $Arguments = @{
            Suite = $Target.Suite
            Format = $Target.Format
            MaxComparePages = $MaxComparePages
            MinimumScore = $MinimumScore
        }
        if ($SkipReference) { $Arguments.SkipReference = $true }
        if ($ForceReference) { $Arguments.ForceReference = $true }
        & $Runner @Arguments
    }
}

$Rows = foreach ($Target in $Targets) {
    $CoveragePath = Join-Path $ArtifactRoot "$($Target.Suite)/$($Target.Format)/report/benchmark_coverage.json"
    if (-not (Test-Path $CoveragePath)) {
        throw "Missing coverage manifest: $CoveragePath"
    }
    $Coverage = Get-Content $CoveragePath -Raw | ConvertFrom-Json
    [pscustomobject]@{
        suite = $Coverage.suite
        format = $Coverage.format
        report = if ($Coverage.format -eq "xlsx") { "$($Coverage.suite)/$($Coverage.format)/report/comparison_report.md" } else { $null }
        selected_cases = $Coverage.selected_cases
        passed_conversions = $Coverage.passed_conversions
        failed_conversions = $Coverage.failed_conversions
        missing_references = $Coverage.missing_references
        comparison_completed = $Coverage.comparison_completed
        comparison_results = $Coverage.comparison_results
        max_compare_pages = $Coverage.max_compare_pages
        average_score = $Coverage.average_score
        coverage_manifest = [System.IO.Path]::GetRelativePath($RepoRoot, $CoveragePath).Replace("\", "/")
    }
}

$Summary = [pscustomobject]@{
    fixture_scope = "shared-on-disk-fixtures"
    executes_dotnet_xunit = $false
    rust_supported_formats = @("xlsx", "docx")
    unsupported_formats = @("pptx")
    selected_cases = ($Rows | Measure-Object selected_cases -Sum).Sum
    passed_conversions = ($Rows | Measure-Object passed_conversions -Sum).Sum
    failed_conversions = ($Rows | Measure-Object failed_conversions -Sum).Sum
    missing_references = ($Rows | Measure-Object missing_references -Sum).Sum
    comparison_results = ($Rows | Measure-Object comparison_results -Sum).Sum
    comparison_completed = @($Rows | Where-Object { -not $_.comparison_completed }).Count -eq 0
    suites = @($Rows)
}

New-Item -ItemType Directory -Force -Path $ArtifactRoot | Out-Null
$Summary | ConvertTo-Json -Depth 5 | Set-Content $MatrixJson -Encoding UTF8

$Markdown = @(
    "# Rust Shared Visual Benchmark Matrix",
    "",
    "> This matrix reuses the repository's on-disk fixtures and visual comparison pipeline. It does not execute C# xUnit tests or assertions.",
    "",
    "| Suite | Format | Report | Selected | Converted | Compared | Missing refs | First pages | Average score | Complete |",
    "|---|---:|---|---:|---:|---:|---:|---:|---:|---:|"
)
foreach ($Row in $Rows) {
    $Score = if ($null -eq $Row.average_score) { "N/A" } else { "{0:N4}" -f $Row.average_score }
    $ReportLink = if ($Row.report) { "[View comparison]($($Row.report))" } else { "Local artifact" }
    $Markdown += "| $($Row.suite) | $($Row.format) | $ReportLink | $($Row.selected_cases) | $($Row.passed_conversions) | $($Row.comparison_results) | $($Row.missing_references) | $($Row.max_compare_pages) | $Score | $($Row.comparison_completed) |"
}
$Markdown += @(
    "",
    "Total shared fixtures: **$($Summary.selected_cases)**",
    "",
    "Rust-supported formats: XLSX, DOCX. PPTX is not supported and is not counted."
)
$Markdown | Set-Content $MatrixMarkdown -Encoding UTF8

Write-Host "Rust shared visual matrix: selected=$($Summary.selected_cases), converted=$($Summary.passed_conversions), compared=$($Summary.comparison_results), complete=$($Summary.comparison_completed)"
Write-Host "Matrix report: $MatrixMarkdown"

if (-not $Summary.comparison_completed -or $Summary.failed_conversions -gt 0 -or $Summary.missing_references -gt 0) {
    throw "Rust shared visual benchmark matrix is incomplete."
}