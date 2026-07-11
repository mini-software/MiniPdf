<#
.SYNOPSIS
    Save a reusable source-research note into the local Obsidian LLM WIKI.
.PARAMETER Title
    Note title. Also used for the file name after removing invalid path characters.
.PARAMETER Source
    Source alias for organization: LibreOffice | POI | OfficeToPdf.
.PARAMETER Topic
    Optional short topic tag or folder label.
.PARAMETER WikiRoot
    Explicit LLM WIKI root override. Defaults to OBSIDIAN_LLM_WIKI_ROOT or <Obsidian Vault>\LLM WIKI.
.PARAMETER Body
    Markdown body. If omitted, the script reads markdown from the pipeline/stdin.
.PARAMETER Append
    Append to an existing note instead of replacing it.
.EXAMPLE
    "Finding text" | .\save-wiki-note.ps1 -Title "Calc fitToPage scaling" -Source LibreOffice -Topic XLSX
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Title,
    [ValidateSet('LibreOffice', 'POI', 'OfficeToPdf')]
    [string]$Source = 'LibreOffice',
    [string]$Topic,
    [string]$WikiRoot,
    [string]$Body,
    [Parameter(ValueFromPipeline = $true)]
    [string[]]$InputObject,
    [switch]$Append
)

. (Join-Path $PSScriptRoot '_common.ps1')
$WikiRoot = Resolve-WikiRoot -WikiRoot $WikiRoot
$folder = Join-Path (Join-Path $WikiRoot 'Office-Conversion') $Source
if ($Topic) { $folder = Join-Path $folder $Topic }

New-Item -ItemType Directory -Path $folder -Force | Out-Null

$invalidChars = [IO.Path]::GetInvalidFileNameChars() -join ''
$invalidPattern = '[{0}]' -f [Regex]::Escape($invalidChars)
$fileName = ([Regex]::Replace($Title, $invalidPattern, '-')).Trim()
if (-not $fileName) { $fileName = 'Untitled' }
$path = Join-Path $folder ($fileName + '.md')

if (-not $Body) {
    $Body = if ($InputObject) {
        ($InputObject -join [Environment]::NewLine).TrimEnd()
    } else {
        ($input | Out-String).TrimEnd()
    }
}

$stamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$content = @"
---
title: $Title
source: $Source
topic: $Topic
created: $stamp
---

$Body
"@

if ($Append -and (Test-Path -LiteralPath $path)) {
    Add-Content -LiteralPath $path -Value "`n`n## Update $stamp`n`n$Body" -Encoding UTF8
} else {
    Set-Content -LiteralPath $path -Value $content -Encoding UTF8
}

Write-Output $path
