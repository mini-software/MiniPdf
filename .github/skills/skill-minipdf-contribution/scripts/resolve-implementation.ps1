[CmdletBinding()]
param(
    [ValidateSet("auto", "dotnet", "rust")]
    [string]$Implementation = "auto"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ($Implementation -ne "auto") {
    return $Implementation
}

$available = @(
    if (Get-Command dotnet -ErrorAction SilentlyContinue) { "dotnet" }
    if (Get-Command cargo -ErrorAction SilentlyContinue) { "rust" }
)

if ($available.Count -eq 0) {
    throw "Automatic implementation selection requires the .NET SDK or Rust/Cargo."
}

return Get-Random -InputObject $available