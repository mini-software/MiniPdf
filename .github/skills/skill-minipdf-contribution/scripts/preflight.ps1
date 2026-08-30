[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$LibreOfficeSourceRoot,
    [string]$PoiSourceRoot,
    [ValidateSet("dotnet", "rust")]
    [string]$Implementation = "dotnet",
    [switch]$Json
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not $RepositoryRoot) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\..\.."))
}

function Get-CommandPath([string[]]$Names) {
    foreach ($name in $Names) {
        $command = Get-Command $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($command) { return $command.Source }
    }
    return $null
}

function Resolve-LibreOfficePath {
    if ($env:LIBREOFFICE_PATH -and (Test-Path -LiteralPath $env:LIBREOFFICE_PATH -PathType Leaf)) {
        return (Resolve-Path -LiteralPath $env:LIBREOFFICE_PATH).Path
    }
    $commandPath = Get-CommandPath @("soffice", "libreoffice")
    if ($commandPath) { return $commandPath }
    $candidates = @((Join-Path $env:ProgramFiles "LibreOffice\program\soffice.exe"))
    if (${env:ProgramFiles(x86)}) {
        $candidates += Join-Path ${env:ProgramFiles(x86)} "LibreOffice\program\soffice.exe"
    }
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }
    return $null
}

function Resolve-SourceRoot([string]$ExplicitPath, [string]$EnvironmentPath, [string]$DefaultPath) {
    $candidate = if ($ExplicitPath) { $ExplicitPath } elseif ($EnvironmentPath) { $EnvironmentPath } else { $DefaultPath }
    if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Container)) {
        return (Resolve-Path -LiteralPath $candidate).Path
    }
    return $candidate
}

$gitPath = Get-CommandPath @("git")
$dotnetPath = Get-CommandPath @("dotnet")
$dotnetSdks = if ($dotnetPath) { @(& $dotnetPath --list-sdks) } else { @() }
$dotnet9Available = @($dotnetSdks | Where-Object { $_ -match '^9\.' }).Count -gt 0
$cargoPath = Get-CommandPath @("cargo")
if (-not $cargoPath) {
    $cargoCandidate = Join-Path $env:USERPROFILE ".cargo\bin\cargo.exe"
    if (Test-Path -LiteralPath $cargoCandidate -PathType Leaf) { $cargoPath = $cargoCandidate }
}
$repositoryPython = @(
    (Join-Path $RepositoryRoot ".venv\Scripts\python.exe"),
    (Join-Path $RepositoryRoot ".venv/bin/python")
) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
$pythonPath = if ($repositoryPython) { $repositoryPython } else { Get-CommandPath @("python", "python3") }
$pythonVersion = if ($pythonPath) { & $pythonPath -c "import platform; print(platform.python_version())" } else { $null }
$pythonSupported = $false
if ($pythonVersion) { $pythonSupported = [version]$pythonVersion -ge [version]"3.10" }
$ghPath = Get-CommandPath @("gh")
$libreOfficePath = Resolve-LibreOfficePath
$officeAvailable = $false
if ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT) {
    try {
        $officeAvailable = $null -ne [Type]::GetTypeFromProgID("Excel.Application") -and
            $null -ne [Type]::GetTypeFromProgID("Word.Application")
    } catch {
        $officeAvailable = $false
    }
}
$resolvedLibreOfficeSource = Resolve-SourceRoot $LibreOfficeSourceRoot $env:LIBREOFFICE_SOURCE_ROOT "D:\git\libreoffice-core"
$resolvedPoiSource = Resolve-SourceRoot $PoiSourceRoot $env:POI_SOURCE_ROOT "D:\git\poi"

$ghAuthenticated = $false
if ($ghPath) {
    & $ghPath auth status *> $null
    $ghAuthenticated = $LASTEXITCODE -eq 0
}

$checks = @(
    [pscustomobject]@{
        Name = "Git"; Required = $true; Available = [bool]$gitPath; Location = $gitPath
        Guidance = if ($gitPath) { $null } else { "Install Git from https://git-scm.com/downloads" }
    },
    [pscustomobject]@{
        Name = ".NET 9 SDK"; Required = $Implementation -eq "dotnet"; Available = $dotnet9Available; Location = if ($dotnet9Available) { $dotnetPath } else { $null }
        Guidance = if ($dotnet9Available) { $null } else { "Install .NET 9 SDK from https://dotnet.microsoft.com/download" }
    },
    [pscustomobject]@{
        Name = "Rust / Cargo"; Required = $Implementation -eq "rust"; Available = [bool]$cargoPath; Location = $cargoPath
        Guidance = if ($cargoPath) { $null } else { "Install Rust from https://rustup.rs/" }
    },
    [pscustomobject]@{
        Name = "Python 3.10+"; Required = $true; Available = $pythonSupported; Location = if ($pythonVersion) { "$pythonPath ($pythonVersion)" } else { $pythonPath }
        Guidance = if ($pythonSupported) { $null } else { "Install Python 3.10+ or create the repository .venv" }
    },
    [pscustomobject]@{
        Name = "LibreOffice"; Required = $true; Available = [bool]$libreOfficePath; Location = $libreOfficePath
        Guidance = if ($libreOfficePath) { $null } else { "Install LibreOffice or set LIBREOFFICE_PATH to soffice" }
    },
    [pscustomobject]@{
        Name = "Microsoft Excel and Word"; Required = $Implementation -eq "rust"; Available = $officeAvailable; Location = if ($officeAvailable) { "COM automation" } else { $null }
        Guidance = if ($officeAvailable) { $null } else { "The Rust visual workflow currently requires desktop Microsoft Excel and Word for primary reference PDFs" }
    },
    [pscustomobject]@{
        Name = "GitHub CLI"; Required = $false; Available = [bool]$ghPath; Location = $ghPath
        Guidance = if ($ghPath) { $null } else { "Optional: install from https://cli.github.com/; browser PR steps will be generated" }
    },
    [pscustomobject]@{
        Name = "GitHub CLI authentication"; Required = $false; Available = $ghAuthenticated; Location = $null
        Guidance = if ($ghAuthenticated) { $null } elseif ($ghPath) { "Run gh auth login, or use the browser PR workflow" } else { "Install and authenticate gh, or use the browser PR workflow" }
    },
    [pscustomobject]@{
        Name = "LibreOffice source"; Required = $false; Available = (Test-Path -LiteralPath $resolvedLibreOfficeSource -PathType Container); Location = $resolvedLibreOfficeSource
        Guidance = if (Test-Path -LiteralPath $resolvedLibreOfficeSource -PathType Container) { $null } else { "Clone https://github.com/LibreOffice/core and set LIBREOFFICE_SOURCE_ROOT" }
    },
    [pscustomobject]@{
        Name = "Apache POI source"; Required = $false; Available = (Test-Path -LiteralPath $resolvedPoiSource -PathType Container); Location = $resolvedPoiSource
        Guidance = if (Test-Path -LiteralPath $resolvedPoiSource -PathType Container) { $null } else { "Clone https://github.com/apache/poi and set POI_SOURCE_ROOT" }
    }
)

$missingRequired = @($checks | Where-Object { $_.Required -and -not $_.Available })
$result = [pscustomobject]@{
    RepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
    Implementation = $Implementation
    Ready = $missingRequired.Count -eq 0
    PullRequestMode = if ($ghAuthenticated) { "github-cli" } else { "browser" }
    Checks = $checks
}

if ($Json) {
    $result | ConvertTo-Json -Depth 5
} else {
    $checks | Select-Object Name, Required, Available, Location | Format-Table -AutoSize
    foreach ($check in $checks | Where-Object { -not $_.Available }) {
        Write-Host ("{0}: {1}" -f $check.Name, $check.Guidance) -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host ("Ready: {0}" -f $result.Ready)
    Write-Host ("Pull request mode: {0}" -f $result.PullRequestMode)
}

$global:LASTEXITCODE = if ($missingRequired.Count -gt 0) { 1 } else { 0 }