<#
.SYNOPSIS
    One-click PPTX benchmark: convert PPTX -> PDF (MiniPdf + LibreOffice) -> compare -> report.

.DESCRIPTION
    This script orchestrates the MiniPdf PPTX comparison pipeline on Windows.
    Copy PPTX files into tests/MiniPdf.Scripts/output_pptx before running the full pipeline.

.EXAMPLE
    .\scripts\Run-Benchmark_pptx.ps1
    .\scripts\Run-Benchmark_pptx.ps1 -CompareOnly
    .\scripts\Run-Benchmark_pptx.ps1 -SkipReference
#>

param(
    [switch]$CompareOnly,
    [switch]$SkipMiniPdf,
    [switch]$SkipReference,
    [switch]$SkipInstall,
    [string]$Filter,
    [string]$SourceDir,
    [string]$MiniPdfDir,
    [string]$ReferenceDir,
    [string]$ReportDir,
    [string]$Manifest,
    [string]$ReportScope = "shared",
    [switch]$CompositeImages,
    [string]$CandidateLabel = "MiniPdf",
    [string]$ReferenceLabel = "LibreOffice Reference"
)

$ErrorActionPreference = "Continue"
$ScriptRoot = Split-Path -Parent $PSScriptRoot
$BenchmarkDir = Join-Path (Join-Path $ScriptRoot "tests") "MiniPdf.Benchmark"

function Resolve-BenchmarkPath([string]$PathValue) {
    if (-not $PathValue) { return $null }
    if ([System.IO.Path]::IsPathRooted($PathValue)) { return $PathValue }
    return Join-Path $ScriptRoot $PathValue
}

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  MiniPdf PPTX Benchmark Pipeline" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

if (-not $SkipInstall) {
    Write-Host "[Step 0] Installing Python dependencies..." -ForegroundColor Yellow
    pip install pymupdf Pillow --quiet 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  WARNING: pip install had issues. Continuing anyway..." -ForegroundColor DarkYellow
    } else {
        Write-Host "  OK" -ForegroundColor Green
    }
}

$pyArgs = @()
if ($CompareOnly) { $pyArgs += "--compare-only" }
if ($SkipMiniPdf) { $pyArgs += "--skip-minipdf" }
if ($SkipReference) { $pyArgs += "--skip-reference" }
if ($Filter) { $pyArgs += "--filter"; $pyArgs += $Filter }
if ($SourceDir) { $pyArgs += "--source-dir"; $pyArgs += (Resolve-BenchmarkPath $SourceDir) }
if ($MiniPdfDir) { $pyArgs += "--minipdf-dir"; $pyArgs += (Resolve-BenchmarkPath $MiniPdfDir) }
if ($ReferenceDir) { $pyArgs += "--reference-dir"; $pyArgs += (Resolve-BenchmarkPath $ReferenceDir) }
if ($ReportDir) { $pyArgs += "--report-dir"; $pyArgs += (Resolve-BenchmarkPath $ReportDir) }
if ($Manifest) { $pyArgs += "--manifest"; $pyArgs += (Resolve-BenchmarkPath $Manifest) }
if ($ReportScope -ne "shared") { $pyArgs += "--report-scope"; $pyArgs += $ReportScope }
if ($CompositeImages) { $pyArgs += "--composite-images" }
if ($CandidateLabel -ne "MiniPdf") { $pyArgs += "--candidate-label"; $pyArgs += $CandidateLabel }
if ($ReferenceLabel -ne "LibreOffice Reference") { $pyArgs += "--reference-label"; $pyArgs += $ReferenceLabel }

Write-Host "`n[Running] python run_benchmark_pptx.py $($pyArgs -join ' ')`n" -ForegroundColor Yellow
Push-Location $BenchmarkDir
try {
    python run_benchmark_pptx.py @pyArgs
} finally {
    Pop-Location
}

$reportDirPath = if ($ReportDir) { Resolve-BenchmarkPath $ReportDir } else { Join-Path $BenchmarkDir "reports_pptx" }
$reportPath = Join-Path $reportDirPath "comparison_report.md"
if (Test-Path $reportPath) {
    Write-Host "`n[Done] Report: $reportPath" -ForegroundColor Green
    Write-Host "Opening report..." -ForegroundColor Cyan
    $code = Get-Command code -ErrorAction SilentlyContinue
    if ($code) {
        code $reportPath
    } else {
        Start-Process notepad.exe -ArgumentList $reportPath
    }
} else {
    Write-Host "`nNo report generated. Check the output above for errors." -ForegroundColor Red
}