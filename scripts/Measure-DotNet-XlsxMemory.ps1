#requires -Version 7.0
<#
.SYNOPSIS
    Measures isolated-process peak working sets and verifies identical XLSX PDF output.
.EXAMPLE
    .\scripts\Measure-DotNet-XlsxMemory.ps1 -RowCounts 20000,60000
#>
param(
    [ValidateNotNullOrEmpty()]
    [int[]]$RowCounts = @(20000, 60000)
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutputDir = Join-Path $RepoRoot "artifacts\xlsx-low-memory\memory"
$Project = Join-Path $RepoRoot "src\MiniPdf.Cli\MiniPdf.Cli.csproj"
$Cli = Join-Path $RepoRoot "src\MiniPdf.Cli\bin\Release\net9.0\MiniPdf.Cli.dll"
$Dotnet = (Get-Command dotnet -ErrorAction Stop).Source
if ($RowCounts | Where-Object { $_ -lt 1 -or $_ -gt 1048576 }) {
    throw "RowCounts must be between 1 and 1048576."
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
& $Dotnet build $Project -c Release --no-restore *> (Join-Path $OutputDir "build.log")
if ($LASTEXITCODE -ne 0) { throw "CLI build failed. See $OutputDir\build.log." }

$Generator = @'
import io, pathlib, sys, zipfile
directory = pathlib.Path(sys.argv[1])
ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
for count in map(int, sys.argv[2:]):
    with zipfile.ZipFile(directory / f"table-{count}.xlsx", "w", zipfile.ZIP_DEFLATED) as z:
        z.writestr("xl/workbook.xml", f'<workbook xmlns="{ns}"><sheets><sheet name="Data" sheetId="1"/></sheets></workbook>')
        with io.TextIOWrapper(z.open("xl/sharedStrings.xml", "w"), encoding="utf-8") as out:
            out.write(f'<sst xmlns="{ns}">')
            for i in range(1, count+1):
                out.write(f'<si><t>Record {i:08d}</t></si>')
            out.write('</sst>')
        with io.TextIOWrapper(z.open("xl/worksheets/sheet1.xml", "w"), encoding="utf-8") as out:
            out.write(f'<worksheet xmlns="{ns}"><sheetFormatPr defaultRowHeight="15" defaultColWidth="12"/><cols><col min="1" max="8" width="12" customWidth="1"/></cols><sheetData>')
            for i in range(1, count+1):
                out.write(f'<row r="{i}"><c r="A{i}" t="s"><v>{i-1}</v></c>')
                for col in "BCDEFGH":
                    out.write(f'<c r="{col}{i}"><v>{i*10+ord(col)}</v></c>')
                out.write('</row>')
            out.write('</sheetData><pageSetup paperSize="9" fitToHeight="0"/></worksheet>')
'@
$Generator | python - $OutputDir @RowCounts
if ($LASTEXITCODE -ne 0) { throw "Generating the memory benchmark workbooks failed." }

$Results = @()
foreach ($Rows in ($RowCounts | Sort-Object -Unique)) {
    foreach ($LowMemory in @($false, $true)) {
        $Mode = if ($LowMemory) { "low-memory" } else { "default" }
        $Pdf = Join-Path $OutputDir "table-$Rows-$Mode.pdf"
        $Info = [System.Diagnostics.ProcessStartInfo]::new($Dotnet)
        $Info.UseShellExecute = $false
        $Info.RedirectStandardOutput = $true
        $Info.RedirectStandardError = $true
        foreach ($Argument in @($Cli, (Join-Path $OutputDir "table-$Rows.xlsx"), "-o", $Pdf)) {
            $Info.ArgumentList.Add($Argument)
        }
        if ($LowMemory) { $Info.ArgumentList.Add("--low-memory") }
        $Process = [System.Diagnostics.Process]::new()
        $Process.StartInfo = $Info
        $Timer = [System.Diagnostics.Stopwatch]::StartNew()
        $Peak = 0L
        $Started = $false
        try {
            $Started = $Process.Start()
            if (-not $Started) { throw "Could not start the conversion process." }
            $Stdout = $Process.StandardOutput.ReadToEndAsync()
            $Stderr = $Process.StandardError.ReadToEndAsync()
            while (-not $Process.HasExited) {
                $Process.Refresh()
                $Peak = [Math]::Max($Peak, $Process.PeakWorkingSet64)
                Start-Sleep -Milliseconds 20
            }
            $Process.WaitForExit()
            $Timer.Stop()
            $Output = $Stdout.GetAwaiter().GetResult()
            $ErrorOutput = $Stderr.GetAwaiter().GetResult()
            [System.IO.File]::WriteAllText((Join-Path $OutputDir "table-$Rows-$Mode.log"), $Output + $ErrorOutput)
            if ($Process.ExitCode -ne 0) { throw "Conversion failed: $ErrorOutput" }
            if ($Peak -eq 0) { throw "No memory sample was captured." }
            $Result = [pscustomobject]@{
                Rows = $Rows
                Columns = 8
                Mode = $Mode
                PeakWorkingSetBytes = $Peak
                Seconds = [Math]::Round($Timer.Elapsed.TotalSeconds, 3)
                PdfBytes = (Get-Item -LiteralPath $Pdf).Length
                Sha256 = (Get-FileHash -LiteralPath $Pdf -Algorithm SHA256).Hash
            }
            $Results += $Result
            $Result | Format-List
        }
        finally {
            if ($Started -and -not $Process.HasExited) { Stop-Process -Id $Process.Id }
            $Process.Dispose()
        }
    }
    $Pair = @($Results | Where-Object Rows -eq $Rows)
    if ($Pair[0].Sha256 -ne $Pair[1].Sha256) { throw "PDF output differs for $Rows rows." }
}
$Results | ConvertTo-Json | Set-Content (Join-Path $OutputDir "results.json") -Encoding utf8
$Largest = @($Results | Where-Object Rows -eq ($RowCounts | Measure-Object -Maximum).Maximum)
$Ratio = $Largest[1].PeakWorkingSetBytes / $Largest[0].PeakWorkingSetBytes
if ($Ratio -gt 0.7) { throw "LowMemory must reduce peak working set by at least 30% on the largest case; ratio was $Ratio." }
Write-Host ("Largest-case peak working-set reduction: {0:P1}" -f (1 - $Ratio))
