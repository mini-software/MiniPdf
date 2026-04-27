<#
.SYNOPSIS
    Regex-search an Office reference vault.
.PARAMETER Pattern
    Regex pattern to search for (case-insensitive).
.PARAMETER Vault
    Vault alias: LibreOffice (default) | POI | OfficeToPdf.
.PARAMETER Folder
    Optional subfolder to scope the search (e.g. 13-Page-Layout).
.PARAMETER VaultRoot
    Explicit vault path; overrides -Vault when provided.
.PARAMETER Context
    Number of context lines to show around each match. Default 1.
.EXAMPLE
    .\search-vault.ps1 -Pattern "sectPr|orientation" -Folder 13-Page-Layout
.EXAMPLE
    .\search-vault.ps1 -Vault POI -Pattern "XWPFStyle|styleId"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Pattern,
    [ValidateSet('LibreOffice', 'POI', 'OfficeToPdf')]
    [string]$Vault = 'LibreOffice',
    [string]$Folder,
    [string]$VaultRoot,
    [int]$Context = 1
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
    Select-String -Pattern $Pattern -Context $Context, $Context -CaseSensitive:$false |
    ForEach-Object {
        $rel = $_.Path.Substring($VaultRoot.Length + 1)
        "{0}:{1}: {2}" -f $rel, $_.LineNumber, $_.Line.Trim()
    }
