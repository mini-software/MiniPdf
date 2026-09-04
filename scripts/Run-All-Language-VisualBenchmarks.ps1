param()

$ErrorActionPreference = "Stop"
foreach ($Language in @("dotnet", "rust", "java", "go", "python", "node")) {
    Write-Host "`n=== $Language visual benchmark ==="
    & (Join-Path $PSScriptRoot "Invoke-LanguageVisualBenchmark.ps1") -Language $Language @args
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}