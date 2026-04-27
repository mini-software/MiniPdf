<#
.SYNOPSIS
    Benchmark Issue_Files: convert user-reported xlsx/docx to PDF (MiniPdf + LibreOffice) → compare → report.

.DESCRIPTION
    Converts files in tests/Issue_Files/xlsx and tests/Issue_Files/docx using both
    MiniPdf and LibreOffice, then runs compare_pdfs.py to produce a comparison report.
    When -Filter is omitted, all issue files (xlsx + docx) are processed.

.EXAMPLE
    .\scripts\Run-Benchmark_issues.ps1                              # run ALL issue xlsx + docx
    .\scripts\Run-Benchmark_issues.ps1 -Filter "sa8000"             # only files matching "sa8000"
    .\scripts\Run-Benchmark_issues.ps1 -Filter "sa8000" -SkipReference
    .\scripts\Run-Benchmark_issues.ps1 -Filter "sa8000" -CompareOnly
    .\scripts\Run-Benchmark_issues.ps1 -Filter "sa8000" -SingleFile # convert via published .NET single-file CLI
#>

param(
    [string]$Filter,
    [switch]$CompareOnly,
    [switch]$SkipMiniPdf,
    [switch]$SkipReference,
    [switch]$SkipInstall,
    [switch]$WithOffice,
    [switch]$SkipOffice,
    [switch]$SingleFile,
    [string]$SingleFileRid = "win-x64",
    [ValidateSet("libre", "office")]
    [string]$Engine = "office"
)

$ErrorActionPreference = "Continue"
$ScriptRoot = Split-Path -Parent $PSScriptRoot
$IssueDir = Join-Path (Join-Path $ScriptRoot "tests") "Issue_Files"
$BenchmarkDir = Join-Path (Join-Path $ScriptRoot "tests") "MiniPdf.Benchmark"
$ScriptsDir = Join-Path (Join-Path $ScriptRoot "tests") "MiniPdf.Scripts"
$CliProject = Join-Path (Join-Path (Join-Path $ScriptRoot "src") "MiniPdf.Cli") "MiniPdf.Cli.csproj"
$SingleFilePublishDir = Join-Path (Join-Path (Join-Path $IssueDir "_singlefile") "MiniPdf.Cli") $SingleFileRid
$SingleFileExe = Join-Path $SingleFilePublishDir "MiniPdf.Cli.exe"

# Issue source dirs
$XlsxIssueDir = Join-Path $IssueDir "xlsx"
$DocxIssueDir = Join-Path $IssueDir "docx"

# MiniPdf output dirs
$MiniPdfXlsx = Join-Path $IssueDir "minipdf_xlsx"
$MiniPdfDocx = Join-Path $IssueDir "minipdf_docx"

# LibreOffice reference output dirs
$RefXlsx = Join-Path $IssueDir "reference_xlsx"
$RefDocx = Join-Path $IssueDir "reference_docx"

# Office output dirs
$OfficeXlsx = Join-Path $IssueDir "office_xlsx"
$OfficeDocx = Join-Path $IssueDir "office_docx"

# Report dirs
$ReportXlsx = Join-Path $IssueDir "reports_xlsx"
$ReportDocx = Join-Path $IssueDir "reports_docx"

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  MiniPdf Issue Files Benchmark" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

if ($SingleFile) {
    Write-Host "  MiniPdf mode: .NET single-file CLI ($SingleFileRid)" -ForegroundColor DarkCyan
} else {
    Write-Host "  MiniPdf mode: dotnet run (script)" -ForegroundColor DarkCyan
}

function Ensure-SingleFileCli {
    if (-not $SingleFile) { return }

    if (-not (Test-Path $CliProject)) {
        throw "MiniPdf CLI project not found: $CliProject"
    }

    if (-not (Test-Path $SingleFilePublishDir)) {
        New-Item -ItemType Directory -Path $SingleFilePublishDir -Force | Out-Null
    }

    if (-not (Test-Path $SingleFileExe)) {
        Write-Host "[Build] Publishing MiniPdf single-file CLI..." -ForegroundColor Yellow
        dotnet publish $CliProject -c Release -r $SingleFileRid --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o $SingleFilePublishDir
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for single-file CLI."
        }
    }
}

function Convert-WithSingleFileCli {
    param(
        [string]$InputDir,
        [string]$OutputDir,
        [string]$Extension,
        [string]$FilterPattern
    )

    Ensure-SingleFileCli

    $files = Get-ChildItem -Path $InputDir -Filter "*.$Extension" -ErrorAction SilentlyContinue | Sort-Object Name
    if ($FilterPattern) {
        $files = $files | Where-Object { $_.BaseName -like "*$FilterPattern*" }
    }

    if (-not $files -or $files.Count -eq 0) {
        Write-Host "No .$Extension files found in $InputDir" -ForegroundColor DarkYellow
        return
    }

    $passed = 0
    $failed = 0

    foreach ($f in $files) {
        $pdfPath = Join-Path $OutputDir ($f.BaseName + ".pdf")
        & $SingleFileExe convert $f.FullName -o $pdfPath
        if ($LASTEXITCODE -eq 0) {
            $passed++
        } else {
            $failed++
            Write-Host "  ERR $($f.Name)" -ForegroundColor Red
        }
    }

    Write-Host "  Done via single-file CLI. Passed: $passed, Failed: $failed, Total: $($files.Count)" -ForegroundColor Cyan
}

# Step 0: Install Python dependencies
if (-not $SkipInstall) {
    Write-Host "[Step 0] Installing Python dependencies..." -ForegroundColor Yellow
    pip install openpyxl pymupdf python-docx Pillow --quiet 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  WARNING: pip install had issues. Continuing anyway..." -ForegroundColor DarkYellow
    } else {
        Write-Host "  OK" -ForegroundColor Green
    }
}

# Ensure output dirs
foreach ($d in @($MiniPdfXlsx, $MiniPdfDocx, $RefXlsx, $RefDocx, $OfficeXlsx, $OfficeDocx, $ReportXlsx, $ReportDocx)) {
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
}

# ── XLSX ──
$xlsxFiles = Get-ChildItem -Path $XlsxIssueDir -Filter "*.xlsx" -ErrorAction SilentlyContinue
if ($Filter) {
    $xlsxFiles = $xlsxFiles | Where-Object { $_.BaseName -like "*$Filter*" }
}
if ($xlsxFiles -and $xlsxFiles.Count -gt 0) {
    $cnt = $xlsxFiles.Count
    Write-Host "`n--- XLSX Issue Files: $cnt files ---" -ForegroundColor Cyan

    if (-not $CompareOnly -and -not $SkipMiniPdf) {
        Write-Host '[Step 1] Converting XLSX -> PDF (MiniPdf)...' -ForegroundColor Yellow
        if ($SingleFile) {
            Convert-WithSingleFileCli -InputDir $XlsxIssueDir -OutputDir $MiniPdfXlsx -Extension "xlsx" -FilterPattern $Filter
        } else {
            Push-Location $ScriptsDir
            try {
                $convertArgs = @("convert_xlsx_to_pdf.cs", "--", $XlsxIssueDir, $MiniPdfXlsx)
                if ($Filter) { $convertArgs += $Filter }
                dotnet run --no-cache @convertArgs
            } finally {
                Pop-Location
            }
        }
    }

    if (-not $CompareOnly -and -not $SkipReference) {
        if ($Engine -eq 'office') {
            Write-Host '[Step 2] Converting XLSX -> PDF (Office / Excel COM)...' -ForegroundColor Yellow
            Push-Location $BenchmarkDir
            try {
                $refArgs = @("generate_office_pdfs.py", "--xlsx-dir", $XlsxIssueDir, "--pdf-dir", $RefXlsx)
                if ($Filter) { $refArgs += @("--filter", $Filter) }
                python @refArgs
            } finally {
                Pop-Location
            }
        } else {
            Write-Host '[Step 2] Converting XLSX -> PDF (LibreOffice)...' -ForegroundColor Yellow
            Push-Location $BenchmarkDir
            try {
                $refArgs = @("generate_reference_pdfs.py", "--xlsx-dir", $XlsxIssueDir, "--pdf-dir", $RefXlsx)
                if ($Filter) { $refArgs += @("--filter", $Filter) }
                python @refArgs
            } finally {
                Pop-Location
            }
        }
    }

    if ($WithOffice -and -not $CompareOnly -and -not $SkipOffice) {
        Write-Host '[Step 2b] Converting XLSX -> PDF (Office / Excel COM)...' -ForegroundColor Yellow
        Push-Location $BenchmarkDir
        try {
            $officeArgs = @("generate_office_pdfs.py", "--xlsx-dir", $XlsxIssueDir, "--pdf-dir", $OfficeXlsx)
            python @officeArgs
        } finally {
            Pop-Location
        }
    }

    Write-Host "[Step 3] Comparing XLSX PDFs..." -ForegroundColor Yellow
    $compareArgs = @("compare_pdfs.py", "--minipdf-dir", $MiniPdfXlsx, "--reference-dir", $RefXlsx, "--report-dir", $ReportXlsx)
    if ($WithOffice -and (Test-Path $OfficeXlsx)) {
        $compareArgs += @("--office-dir", $OfficeXlsx)
    }
    if ($Filter) { $compareArgs += @("--filter", $Filter) }
    Push-Location $BenchmarkDir
    try {
        python @compareArgs
    } finally {
        Pop-Location
    }
} else {
    Write-Host "No XLSX files in Issue_Files/xlsx — skipping." -ForegroundColor DarkYellow
}

# ── DOCX ──
$docxFiles = Get-ChildItem -Path $DocxIssueDir -Filter "*.docx" -ErrorAction SilentlyContinue
if ($Filter) {
    $docxFiles = $docxFiles | Where-Object { $_.BaseName -like "*$Filter*" }
}
if ($docxFiles -and $docxFiles.Count -gt 0) {
    $cnt = $docxFiles.Count
    Write-Host "`n--- DOCX Issue Files: $cnt files ---" -ForegroundColor Cyan

    if (-not $CompareOnly -and -not $SkipMiniPdf) {
        Write-Host '[Step 1] Converting DOCX -> PDF (MiniPdf)...' -ForegroundColor Yellow
        if ($SingleFile) {
            Convert-WithSingleFileCli -InputDir $DocxIssueDir -OutputDir $MiniPdfDocx -Extension "docx" -FilterPattern $Filter
        } else {
            Push-Location $ScriptsDir
            try {
                $convertArgs = @("convert_docx_to_pdf.cs", "--", $DocxIssueDir, $MiniPdfDocx)
                if ($Filter) { $convertArgs += $Filter }
                dotnet run --no-cache @convertArgs
            } finally {
                Pop-Location
            }
        }
    }

    if (-not $CompareOnly -and -not $SkipReference) {
        if ($Engine -eq 'office') {
            Write-Host '[Step 2] Converting DOCX -> PDF (Office / Word COM)...' -ForegroundColor Yellow
            Push-Location $BenchmarkDir
            try {
                $refArgs = @("generate_office_pdfs_docx.py", "--docx-dir", $DocxIssueDir, "--pdf-dir", $RefDocx)
                if ($Filter) { $refArgs += @("--filter", $Filter) }
                python @refArgs
            } finally {
                Pop-Location
            }
        } else {
            Write-Host '[Step 2] Converting DOCX -> PDF (LibreOffice)...' -ForegroundColor Yellow
            Push-Location $BenchmarkDir
            try {
                $refArgs = @("generate_reference_pdfs_docx.py", "--docx-dir", $DocxIssueDir, "--pdf-dir", $RefDocx)
                if ($Filter) { $refArgs += @("--filter", $Filter) }
                python @refArgs
            } finally {
                Pop-Location
            }
        }
    }

    if ($WithOffice -and -not $CompareOnly -and -not $SkipOffice) {
        Write-Host '[Step 2b] Converting DOCX -> PDF (Office / Word COM)...' -ForegroundColor Yellow
        Push-Location $BenchmarkDir
        try {
            $officeArgs = @("generate_office_pdfs_docx.py", "--docx-dir", $DocxIssueDir, "--pdf-dir", $OfficeDocx)
            python @officeArgs
        } finally {
            Pop-Location
        }
    }

    Write-Host "[Step 3] Comparing DOCX PDFs..." -ForegroundColor Yellow
    $compareArgs = @("compare_pdfs.py", "--minipdf-dir", $MiniPdfDocx, "--reference-dir", $RefDocx, "--report-dir", $ReportDocx)
    if ($WithOffice -and (Test-Path $OfficeDocx)) {
        $compareArgs += @("--office-dir", $OfficeDocx)
    }
    if ($Filter) { $compareArgs += @("--filter", $Filter) }
    Push-Location $BenchmarkDir
    try {
        python @compareArgs
    } finally {
        Pop-Location
    }
} else {
    Write-Host "No DOCX files in Issue_Files/docx — skipping." -ForegroundColor DarkYellow
}

# ── Summary ──
Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  Done! Reports:" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$xlsxReport = Join-Path $ReportXlsx "comparison_report.md"
$docxReport = Join-Path $ReportDocx "comparison_report.md"

if (Test-Path $xlsxReport) {
    Write-Host "  XLSX: $xlsxReport" -ForegroundColor Green
}
if (Test-Path $docxReport) {
    Write-Host "  DOCX: $docxReport" -ForegroundColor Green
}

# Open first available report
$code = Get-Command code -ErrorAction SilentlyContinue
if ((Test-Path $xlsxReport) -and $code) { code $xlsxReport }
elseif ((Test-Path $docxReport) -and $code) { code $docxReport }
