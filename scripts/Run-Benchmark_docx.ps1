<#
.SYNOPSIS
    One-click DOCX benchmark: generate DOCX -> convert to PDF (MiniPdf + LibreOffice) -> compare -> report.

.DESCRIPTION
    This script orchestrates the full MiniPdf DOCX self-evolution pipeline on Windows.
    It installs Python dependencies, runs all steps, and opens the final report.

.EXAMPLE
    .\scripts\Run-Benchmark_docx.ps1
    .\scripts\Run-Benchmark_docx.ps1 -CompareOnly
    .\scripts\Run-Benchmark_docx.ps1 -SkipReference
#>

param(
    [switch]$CompareOnly,
    [switch]$SkipGenerate,
    [switch]$SkipMiniPdf,
    [switch]$SkipReference,
    [switch]$SkipInstall,
    [switch]$WithOffice,
    [switch]$SkipOffice
)

$ErrorActionPreference = "Continue"
$ScriptRoot = Split-Path -Parent $PSScriptRoot
$BenchmarkDir = Join-Path (Join-Path $ScriptRoot "tests") "MiniPdf.Benchmark"

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  MiniPdf DOCX Benchmark Pipeline" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

# Step 0: Install Python dependencies
if (-not $SkipInstall) {
    Write-Host "[Step 0] Installing Python dependencies..." -ForegroundColor Yellow
    pip install python-docx pymupdf Pillow --quiet 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  WARNING: pip install had issues. Continuing anyway..." -ForegroundColor DarkYellow
    } else {
        Write-Host "  OK" -ForegroundColor Green
    }
}

# Build args for Python pipeline
$pyArgs = @()
if ($CompareOnly) { $pyArgs += "--compare-only" }
if ($SkipGenerate) { $pyArgs += "--skip-generate" }
if ($SkipMiniPdf) { $pyArgs += "--skip-minipdf" }
if ($SkipReference) { $pyArgs += "--skip-reference" }
if ($WithOffice) { $pyArgs += "--with-office" }
if ($SkipOffice) { $pyArgs += "--skip-office" }

# Run the benchmark pipeline
Write-Host "`n[Running] python run_benchmark_docx.py $($pyArgs -join ' ')`n" -ForegroundColor Yellow
Push-Location $BenchmarkDir
try {
    python run_benchmark_docx.py @pyArgs
} finally {
    Pop-Location
}

# Open the report if it exists
$reportPath = Join-Path (Join-Path $BenchmarkDir "reports_docx") "comparison_report.md"
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
