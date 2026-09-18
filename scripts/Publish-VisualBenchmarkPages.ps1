[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$ArtifactsRoot = (Join-Path (Split-Path $PSScriptRoot -Parent) "artifacts"),
    [string]$DestinationRoot = (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "minipdf-web-page"),
    [switch]$Clean,
    [switch]$IncludeCompositeImages
)

$ErrorActionPreference = "Stop"

function Get-RelativeWebPath {
    param(
        [Parameter(Mandatory)] [string]$BasePath,
        [Parameter(Mandatory)] [string]$Path
    )

    return [System.IO.Path]::GetRelativePath($BasePath, $Path).Replace("\", "/")
}

function Copy-ReportDirectory {
    param(
        [Parameter(Mandatory)] [System.IO.FileInfo]$ReportFile,
        [Parameter(Mandatory)] [array]$Results,
        [Parameter(Mandatory)] [string]$Language,
        [Parameter(Mandatory)] [string]$Suite,
        [Parameter(Mandatory)] [string]$Format,
        [Parameter(Mandatory)] [string]$AssetsRoot
    )

    $ReportDirectory = $ReportFile.Directory.FullName
    $TargetDirectory = Join-Path $AssetsRoot "$Language/$Suite/$Format"
    New-Item -ItemType Directory -Path $TargetDirectory -Force | Out-Null

    foreach ($FileName in @("comparison_report.json", "comparison_report.md", "benchmark_coverage.json", "comparison_manifest.json")) {
        $SourceFile = Join-Path $ReportDirectory $FileName
        if (Test-Path $SourceFile -PathType Leaf) {
            Copy-Item $SourceFile (Join-Path $TargetDirectory $FileName) -Force
        }
    }

    $ImageNames = @($Results.diff_images | ForEach-Object {
        @($_.minipdf_img, $_.reference_img, $_.auxiliary_img, $_.heatmap_img)
    } | Where-Object { $_ } | Sort-Object -Unique)
    $CompositeNames = if ($IncludeCompositeImages) {
        @($Results.diff_images.composite_img | Where-Object { $_ } | Sort-Object -Unique)
    } else {
        @()
    }

    foreach ($AssetGroup in @(
        [pscustomobject]@{ Directory = "images"; Names = $ImageNames },
        [pscustomobject]@{ Directory = "side-by-side"; Names = $CompositeNames }
    )) {
        if ($AssetGroup.Names.Count -eq 0) { continue }
        $SourceDirectory = Join-Path $ReportDirectory $AssetGroup.Directory
        $TargetAssetDirectory = Join-Path $TargetDirectory $AssetGroup.Directory
        New-Item -ItemType Directory -Path $TargetAssetDirectory -Force | Out-Null
        foreach ($AssetName in $AssetGroup.Names) {
            $SourceAsset = Join-Path $SourceDirectory $AssetName
            if (Test-Path $SourceAsset -PathType Leaf) {
                Copy-Item $SourceAsset (Join-Path $TargetAssetDirectory $AssetName) -Force
            } else {
                Write-Warning "Referenced benchmark asset not found: $SourceAsset"
            }
        }
    }

    return $TargetDirectory
}

$ArtifactsRoot = [System.IO.Path]::GetFullPath($ArtifactsRoot)
$DestinationRoot = [System.IO.Path]::GetFullPath($DestinationRoot)
$RepoRoot = Split-Path $PSScriptRoot -Parent

if (-not (Test-Path $ArtifactsRoot -PathType Container)) {
    throw "Artifacts directory not found: $ArtifactsRoot"
}
if (-not (Test-Path (Join-Path $DestinationRoot ".git") -PathType Container)) {
    throw "GitHub Pages repository not found: $DestinationRoot"
}

$ReportSources = @(Get-ChildItem $ArtifactsRoot -Filter "comparison_report.json" -File -Recurse | ForEach-Object {
    if ($_.FullName -match "[\\/]artifacts[\\/](?<language>[^\\/]+)-benchmark[\\/](?<suite>classic|issue)[\\/](?<format>xlsx|docx|pptx)[\\/]report[\\/]comparison_report\.json$") {
        [pscustomobject]@{
            File = $_
            Language = $Matches.language
            Suite = $Matches.suite
            Format = $Matches.format
        }
    }
})

$LegacyDotNetReports = @(
    [pscustomobject]@{ Path = "tests/MiniPdf.Benchmark/reports/comparison_report.json"; Suite = "classic"; Format = "xlsx" },
    [pscustomobject]@{ Path = "tests/MiniPdf.Benchmark/reports_docx/comparison_report.json"; Suite = "classic"; Format = "docx" },
    [pscustomobject]@{ Path = "tests/Issue_Files/reports_pptx/comparison_report.json"; Suite = "issue"; Format = "pptx" }
)
foreach ($LegacyReport in $LegacyDotNetReports) {
    $LegacyPath = Join-Path $RepoRoot $LegacyReport.Path
    if (Test-Path $LegacyPath -PathType Leaf) {
        $ReportSources += [pscustomobject]@{
            File = Get-Item $LegacyPath
            Language = "dotnet"
            Suite = $LegacyReport.Suite
            Format = $LegacyReport.Format
        }
    }
}

if ($ReportSources.Count -eq 0) {
    throw "No language benchmark reports found under $ArtifactsRoot"
}

$DataRoot = Join-Path $DestinationRoot "data"
$AssetsRoot = Join-Path $DestinationRoot "assets/benchmarks"
if ($Clean) {
    foreach ($GeneratedPath in @($DataRoot, $AssetsRoot)) {
        if (Test-Path $GeneratedPath) {
            Remove-Item $GeneratedPath -Recurse -Force
        }
    }
}
New-Item -ItemType Directory -Path $DataRoot -Force | Out-Null
New-Item -ItemType Directory -Path $AssetsRoot -Force | Out-Null

$PublishedReports = foreach ($ReportSource in $ReportSources) {
    $ReportFile = $ReportSource.File
    $Language = $ReportSource.Language
    $Suite = $ReportSource.Suite
    $Format = $ReportSource.Format
    $Results = @(Get-Content $ReportFile.FullName -Raw | ConvertFrom-Json)
    if ($Results.Count -eq 1 -and $Results[0].PSObject.Properties.Name -contains "results") {
        $Results = @($Results[0].results)
    }

    $Scores = @($Results | Where-Object { $null -ne $_.overall_score })
    $AverageScore = if ($Scores.Count -gt 0) {
        [Math]::Round(($Scores | Measure-Object -Property overall_score -Average).Average, 6)
    } else {
        $null
    }

    if ($PSCmdlet.ShouldProcess("$Language/$Suite/$Format", "Publish visual benchmark report")) {
        $TargetDirectory = Copy-ReportDirectory -ReportFile $ReportFile -Results $Results -Language $Language -Suite $Suite -Format $Format -AssetsRoot $AssetsRoot
    } else {
        $TargetDirectory = Join-Path $AssetsRoot "$Language/$Suite/$Format"
    }

    [ordered]@{
        id = "$Language-$Suite-$Format"
        language = $Language
        suite = $Suite
        format = $Format
        cases = $Results.Count
        scored_cases = $Scores.Count
        average_score = $AverageScore
        report = Get-RelativeWebPath -BasePath $DestinationRoot -Path (Join-Path $TargetDirectory "comparison_report.json")
        coverage = Get-RelativeWebPath -BasePath $DestinationRoot -Path (Join-Path $TargetDirectory "benchmark_coverage.json")
        assets = Get-RelativeWebPath -BasePath $DestinationRoot -Path $TargetDirectory
        includes_composite_images = [bool]$IncludeCompositeImages
    }
}

$Manifest = [ordered]@{
    generated_at = [DateTime]::UtcNow.ToString("o")
    reports = @($PublishedReports | Sort-Object language, suite, format)
}
$ManifestPath = Join-Path $DataRoot "manifest.json"
if ($PSCmdlet.ShouldProcess($ManifestPath, "Write benchmark manifest")) {
    $Manifest | ConvertTo-Json -Depth 8 | Set-Content $ManifestPath -Encoding utf8
}

Write-Host "Published $($PublishedReports.Count) visual benchmark report(s) to $DestinationRoot"
Write-Host "Manifest: $ManifestPath"