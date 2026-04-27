<#
.SYNOPSIS
    List Office reference notes that exist on disk.
.PARAMETER Vault
    Vault alias: LibreOffice (default) | POI | OfficeToPdf.
.PARAMETER Folder
    Optional subfolder name (e.g. 13-Page-Layout). Lists the whole vault if omitted.
.PARAMETER VaultRoot
    Explicit vault path; overrides -Vault when provided.
.EXAMPLE
    .\list-vault.ps1
.EXAMPLE
    .\list-vault.ps1 -Folder 13-Page-Layout
.EXAMPLE
    .\list-vault.ps1 -Vault POI -Folder 05-XWPF-Styles
#>
[CmdletBinding()]
param(
    [ValidateSet('LibreOffice', 'POI', 'OfficeToPdf')]
    [string]$Vault = 'LibreOffice',
    [string]$Folder,
    [string]$VaultRoot
)

. (Join-Path $PSScriptRoot '_common.ps1')
$VaultRoot = Resolve-VaultRoot -Vault $Vault -VaultRoot $VaultRoot

if (-not (Test-Path -LiteralPath $VaultRoot)) {
    Write-Error "Vault not found: $VaultRoot"
    exit 1
}

$root = if ($Folder) { Join-Path $VaultRoot $Folder } else { $VaultRoot }
if (-not (Test-Path -LiteralPath $root)) {
    Write-Error "Folder not found: $root"
    exit 1
}

Get-ChildItem -LiteralPath $root -Recurse -File -Filter *.md |
    ForEach-Object { $_.FullName.Substring($VaultRoot.Length + 1) } |
    Sort-Object
