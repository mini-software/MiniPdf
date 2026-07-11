<#
.SYNOPSIS
    Regex-search the local LibreOffice or Apache POI source tree.
.PARAMETER Pattern
    Regex pattern to search for (case-insensitive by default).
.PARAMETER Source
    Source alias: LibreOffice (default) | POI.
.PARAMETER Folder
    Optional subfolder to scope the search (for example, sc/source/filter/oox or poi-ooxml/src/main/java).
.PARAMETER SourceRoot
    Explicit source root override; defaults to LIBREOFFICE_SOURCE_ROOT / POI_SOURCE_ROOT or the local D:\git paths.
.PARAMETER Glob
    Optional ripgrep glob(s). Defaults to common source and markdown file types.
.PARAMETER MaxResults
    Maximum result lines to print. Default 80.
.EXAMPLE
    .\search-source.ps1 -Source LibreOffice -Pattern "ScaleToPagesX" -Folder sc
.EXAMPLE
    .\search-source.ps1 -Source POI -Pattern "XWPFStyle" -Folder poi-ooxml/src/main/java
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Pattern,
    [ValidateSet('LibreOffice', 'POI')]
    [string]$Source = 'LibreOffice',
    [string]$Folder,
    [string]$SourceRoot,
    [string[]]$Glob,
    [int]$MaxResults = 80
)

. (Join-Path $PSScriptRoot '_common.ps1')
$SourceRoot = Resolve-SourceRoot -Source $Source -SourceRoot $SourceRoot

if (-not (Test-Path -LiteralPath $SourceRoot -PathType Container)) {
    Write-Error "Source root not found: $SourceRoot"
    exit 1
}

$root = if ($Folder) { Join-Path $SourceRoot $Folder } else { $SourceRoot }
if (-not (Test-Path -LiteralPath $root -PathType Container)) {
    Write-Error "Folder not found: $root"
    exit 1
}

if (-not $Glob -or $Glob.Count -eq 0) {
    $Glob = @('*.cxx', '*.hxx', '*.h', '*.cpp', '*.java', '*.xml', '*.xcs', '*.mk', '*.md')
}

$rg = Get-Command rg -ErrorAction SilentlyContinue
if ($rg) {
    $rgArgs = @('--line-number', '--ignore-case', '--glob', '!.git/**')
    foreach ($item in $Glob) {
        $rgArgs += @('--glob', $item)
    }

    $results = & $rg.Source @rgArgs -- $Pattern $root |
        ForEach-Object {
            if ($_.StartsWith($SourceRoot, [StringComparison]::OrdinalIgnoreCase)) {
                $_.Substring($SourceRoot.Length + 1)
            } else {
                $_
            }
        }

    $results | Select-Object -First $MaxResults
    if ($results.Count -gt 0) { exit 0 }
    exit 1
}

Get-ChildItem -LiteralPath $root -Recurse -File |
    Where-Object { $Glob -contains ('*' + $_.Extension) } |
    Select-String -Pattern $Pattern -CaseSensitive:$false |
    Select-Object -First $MaxResults |
    ForEach-Object {
        $rel = $_.Path.Substring($SourceRoot.Length + 1)
        "{0}:{1}: {2}" -f $rel, $_.LineNumber, $_.Line.Trim()
    }
