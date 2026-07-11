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

    $base = if ($VaultRoot) {
        $VaultRoot
    } elseif ($env:OBSIDIAN_VAULT_ROOT) {
        $env:OBSIDIAN_VAULT_ROOT
    } else {
        Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Obsidian Vault'
    }

    $candidates = @(switch ($Vault) {
        'LibreOffice' { 'LibreOffice-to-PDF'; 'LibreOffice-Word-to-PDF' }
        'POI'         { 'Apache-POI-Word' }
        'OfficeToPdf' { 'Office-to-PDF' }
    })

    if (Test-Path -LiteralPath $base -PathType Container) {
        $baseLeaf = Split-Path -Leaf $base
        if ($candidates -contains $baseLeaf) { return $base }

        foreach ($candidate in $candidates) {
            $candidatePath = Join-Path $base $candidate
            if (Test-Path -LiteralPath $candidatePath -PathType Container) {
                return $candidatePath
            }
        }
    }

    return Join-Path $base $candidates[0]
}

function Resolve-SourceRoot {
    [CmdletBinding()]
    param(
        [ValidateSet('LibreOffice', 'POI')]
        [string]$Source = 'LibreOffice',
        [string]$SourceRoot
    )

    if ($SourceRoot) { return $SourceRoot }

    switch ($Source) {
        'LibreOffice' {
            if ($env:LIBREOFFICE_SOURCE_ROOT) { return $env:LIBREOFFICE_SOURCE_ROOT }
            return 'D:\git\libreoffice-core'
        }
        'POI' {
            if ($env:POI_SOURCE_ROOT) { return $env:POI_SOURCE_ROOT }
            return 'D:\git\poi'
        }
    }
}

function Resolve-WikiRoot {
    [CmdletBinding()]
    param([string]$WikiRoot)

    if ($WikiRoot) { return $WikiRoot }
    if ($env:OBSIDIAN_LLM_WIKI_ROOT) { return $env:OBSIDIAN_LLM_WIKI_ROOT }

    $base = if ($env:OBSIDIAN_VAULT_ROOT) {
        $env:OBSIDIAN_VAULT_ROOT
    } else {
        Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Obsidian Vault'
    }

    return Join-Path $base 'LLM WIKI'
}
