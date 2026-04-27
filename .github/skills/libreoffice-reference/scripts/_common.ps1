<#
.SYNOPSIS
    Resolves a vault alias to a vault root path.
.DESCRIPTION
    Internal helper used by list-vault.ps1 and search-vault.ps1.
#>
function Resolve-VaultRoot {
    [CmdletBinding()]
    param(
        [ValidateSet('LibreOffice', 'POI', 'OfficeToPdf')]
        [string]$Vault = 'LibreOffice',
        [string]$VaultRoot
    )

    if ($VaultRoot) { return $VaultRoot }

    if ($env:OBSIDIAN_VAULT_ROOT) {
        $base = $env:OBSIDIAN_VAULT_ROOT
    } else {
        $base = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Obsidian Vault'
    }

    switch ($Vault) {
        'LibreOffice' { return Join-Path $base 'LibreOffice-Word-to-PDF' }
        'POI'         { return Join-Path $base 'Apache-POI-Word' }
        'OfficeToPdf' { return Join-Path $base 'Office-to-PDF' }
    }
}
