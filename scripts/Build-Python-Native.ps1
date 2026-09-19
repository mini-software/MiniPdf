param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Manifest = Join-Path $RepoRoot "minipdf-python/native/Cargo.toml"
$Destination = Join-Path $RepoRoot "minipdf-python/src/minipdf/_native.pyd"

if (-not $SkipBuild) {
    $Cargo = Get-Command cargo | Select-Object -First 1 -ExpandProperty Source
    $venvPython = Join-Path $RepoRoot ".venv/Scripts/python.exe"
    if (Test-Path -LiteralPath $venvPython) {
        $env:PYO3_PYTHON = $venvPython
    }
    & $Cargo build --release --manifest-path $Manifest
    if ($LASTEXITCODE -ne 0) { throw "Python native module build failed" }
}

$Source = Join-Path $RepoRoot "minipdf-python/native/target/release"
if ($IsWindows) {
    $Source = Join-Path $Source "_native.dll"
} elseif ($IsMacOS) {
    $Source = Join-Path $Source "lib_native.dylib"
    $Destination = Join-Path $RepoRoot "minipdf-python/src/minipdf/_native.abi3.so"
} else {
    $Source = Join-Path $Source "lib_native.so"
    $Destination = Join-Path $RepoRoot "minipdf-python/src/minipdf/_native.abi3.so"
}

if (-not (Test-Path -LiteralPath $Source)) {
    throw "Python native module artifact was not found: $Source"
}
Copy-Item $Source $Destination -Force
Write-Host "Installed native extension: $Destination"
