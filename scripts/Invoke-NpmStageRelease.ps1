<#
.SYNOPSIS
    Approve staged npm packages and verify their publication on the public registry.

.DESCRIPTION
    npm >= 11.15 publishes via two-phase "staged publishing": CI stages packages with
    `npm stage publish` and a maintainer must approve each staged package with
    `npm stage approve <stage-id>` (browser-based 2FA is required for every approval).

    This script:
      1. Lists staged packages (`npm stage list --json`).
      2. Approves platform packages first and the main `minipdf` package last.
      3. Verifies the registry: checks `latest` and every optionalDependency of the
         main package for the expected version.

.PARAMETER NpmCli
    npm version spec run through npx (default: npm@11.15.0).

.PARAMETER Version
    Expected release version for verification. Defaults to the version of staged
    entries, or the current `latest` on the registry when nothing is staged.

.PARAMETER ListOnly
    Only list staged packages.

.PARAMETER ApproveOnly
    Approve staged packages without registry verification.

.PARAMETER VerifyOnly
    Only verify the registry without listing or approving.
#>
[CmdletBinding()]
param(
    [string]$NpmCli = 'npm@11.15.0',
    [string]$Version,
    [switch]$ListOnly,
    [switch]$ApproveOnly,
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'

function Invoke-Npm {
    param([string[]]$NpmArgs)
    & npx -y $NpmCli @NpmArgs
    if ($LASTEXITCODE -ne 0) {
        throw "npx $NpmCli $($NpmArgs -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Get-StagedEntries {
    $raw = & npx -y $NpmCli stage list --json 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "npx $NpmCli stage list failed with exit code $LASTEXITCODE"
    }
    if ([string]::IsNullOrWhiteSpace($raw)) { return @() }
    $parsed = $raw | ConvertFrom-Json
    if ($null -eq $parsed) { return @() }
    if ($parsed -is [System.Array]) { return @($parsed) }
    return @($parsed)
}

function Get-RegistryPackage {
    param([string]$PackageName)
    $encoded = $PackageName -replace '/', '%2f'
    $uri = "https://registry.npmjs.org/$encoded"
    $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -ne 200) {
        throw "GET $uri returned $($response.StatusCode)"
    }
    return $response.Content | ConvertFrom-Json
}

function Show-StagedEntries {
    param([object[]]$Entries)
    if ($Entries.Count -eq 0) {
        Write-Host 'No staged packages.'
        return
    }
    $Entries |
        Select-Object id, name, version, tag, status |
        Format-Table -AutoSize
}

# Stage 1: list
$entries = @(Get-StagedEntries)
$expectedVersion = if ($Version) {
    $Version
} elseif ($entries.Count -gt 0) {
    $entries[0].version
} else {
    (Get-RegistryPackage 'minipdf').'dist-tags'.latest
}

if ($ListOnly) {
    Show-StagedEntries -Entries $entries
    return
}

# Stage 2: approve platform packages first, main package last
if (-not $VerifyOnly -and $entries.Count -gt 0) {
    $ordered = @($entries | Where-Object { $_.name -ne 'minipdf' }) +
        @($entries | Where-Object { $_.name -eq 'minipdf' })
    foreach ($entry in $ordered) {
        Write-Host "Approving $($entry.name)@$($entry.version) ($($entry.id)) ..."
        Invoke-Npm @('stage', 'approve', $entry.id)
        Write-Host "Approved $($entry.name)@$($entry.version)."
    }
    if ($ApproveOnly) { return }
}

# Stage 3: verify the registry
if (-not $ApproveOnly -or $VerifyOnly) {
    $main = Get-RegistryPackage 'minipdf'
    $latest = $main.'dist-tags'.latest
    $failed = $false

    Write-Host "Expected version: $expectedVersion"
    Write-Host ('minipdf  {0}  latest={1}' -f
        ($(if ($latest -eq $expectedVersion) { 'OK ' } else { 'FAIL' })),
        $latest)
    if ($latest -ne $expectedVersion) { $failed = $true }

    $optional = $main.versions.$latest.optionalDependencies
    if ($null -ne $optional) {
        foreach ($name in $optional.PSObject.Properties.Name) {
            $pkg = Get-RegistryPackage $name
            $hasVersion = $pkg.versions.PSObject.Properties.Name -contains $expectedVersion
            $status = if ($hasVersion) { 'OK ' } else { 'FAIL' }
            Write-Host ('{0}  {1}={2}' -f $name, $expectedVersion, $status)
            if (-not $hasVersion) { $failed = $true }
        }
    }

    if ($failed) {
        Write-Error "Verification failed for version $expectedVersion."
        exit 1
    }
    Write-Host "Verification passed for version $expectedVersion."
}
