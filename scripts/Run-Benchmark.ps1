<#
.SYNOPSIS
    One-click benchmark: generate Excel → convert to PDF (MiniPdf + LibreOffice) → compare → report.

.DESCRIPTION
    This script orchestrates the full MiniPdf self-evolution pipeline on Windows.
    It installs Python dependencies, runs all steps, and opens the final report.

.EXAMPLE
    .\scripts\Run-Benchmark.ps1
    .\scripts\Run-Benchmark.ps1 -CompareOnly
    .\scripts\Run-Benchmark.ps1 -SkipReference
#>

param(
    [switch]$CompareOnly,
    [switch]$SkipGenerate,
    [switch]$SkipMiniPdf,
    [switch]$SkipReference,
    [switch]$SkipInstall,
    [switch]$WithOffice,
    [switch]$SkipOffice,
    [ValidateSet("libre", "office")]
    [string]$Engine = "office",
    [string]$Filter,
    [string]$SourceDir,
    [string]$MiniPdfDir,
    [string]$ReferenceDir,
    [string]$OfficeDir,
    [string]$ReportDir,
    [string]$Manifest,
    [string]$ReportScope = "shared",
    [switch]$CompositeImages,
    [switch]$Heatmaps,
    [int]$HeatmapThreshold = 12,
    [double]$HeatmapGain = 5.0,
    [string]$CandidateLabel = "MiniPdf",
    [string]$ReferenceLabel,
    [string]$OfficeLabel = "Office"
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
Write-Host "  MiniPdf Self-Evolution Benchmark Pipeline" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

# Step 0: Install Python dependencies
if (-not $SkipInstall) {
    Write-Host "[Step 0] Installing Python dependencies..." -ForegroundColor Yellow
    pip install openpyxl pymupdf Pillow --quiet 2>$null
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
if ($Engine -ne "office") { $pyArgs += "--engine"; $pyArgs += $Engine }
if ($Filter) { $pyArgs += "--filter"; $pyArgs += $Filter }
if ($SourceDir) { $pyArgs += "--source-dir"; $pyArgs += (Resolve-BenchmarkPath $SourceDir) }
if ($MiniPdfDir) { $pyArgs += "--minipdf-dir"; $pyArgs += (Resolve-BenchmarkPath $MiniPdfDir) }
if ($ReferenceDir) { $pyArgs += "--reference-dir"; $pyArgs += (Resolve-BenchmarkPath $ReferenceDir) }
if ($OfficeDir) { $pyArgs += "--office-dir"; $pyArgs += (Resolve-BenchmarkPath $OfficeDir) }
if ($ReportDir) { $pyArgs += "--report-dir"; $pyArgs += (Resolve-BenchmarkPath $ReportDir) }
if ($Manifest) { $pyArgs += "--manifest"; $pyArgs += (Resolve-BenchmarkPath $Manifest) }
if ($ReportScope -ne "shared") { $pyArgs += "--report-scope"; $pyArgs += $ReportScope }
if ($CompositeImages) { $pyArgs += "--composite-images" }
if ($Heatmaps) {
    $pyArgs += "--heatmaps"
    $pyArgs += "--heatmap-threshold"; $pyArgs += $HeatmapThreshold
    $pyArgs += "--heatmap-gain"; $pyArgs += $HeatmapGain
}
if ($CandidateLabel -ne "MiniPdf") { $pyArgs += "--candidate-label"; $pyArgs += $CandidateLabel }
if ($ReferenceLabel) { $pyArgs += "--reference-label"; $pyArgs += $ReferenceLabel }
if ($OfficeLabel -ne "Office") { $pyArgs += "--office-label"; $pyArgs += $OfficeLabel }

# Run the benchmark pipeline
Write-Host "`n[Running] python run_benchmark.py $($pyArgs -join ' ')`n" -ForegroundColor Yellow
Push-Location $BenchmarkDir
try {
    python run_benchmark.py @pyArgs
} finally {
    Pop-Location
}

# Open the report if it exists
$reportDirPath = if ($ReportDir) { Resolve-BenchmarkPath $ReportDir } else { Join-Path $BenchmarkDir "reports" }
$reportPath = Join-Path $reportDirPath "comparison_report.md"
if (Test-Path $reportPath) {
    Write-Host "`n[Done] Report: $reportPath" -ForegroundColor Green
    Write-Host "Opening report..." -ForegroundColor Cyan
    # Open in VS Code if available, otherwise notepad
    $code = Get-Command code -ErrorAction SilentlyContinue
    if ($code) {
        code $reportPath
    } else {
        Start-Process notepad.exe -ArgumentList $reportPath
    }
} else {
    Write-Host "`nNo report generated. Check the output above for errors." -ForegroundColor Red
}
