[CmdletBinding()]
param(
    [string[]]$ReportPath,
    [ValidateRange(1, 20)]
    [int]$Count = 2,
    [ValidateRange(0.0, 1.0)]
    [double]$HighScoreThreshold = 0.95,
    [switch]$Json
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\..\.."))

function Resolve-ReportImage([string]$ReportDirectory, [string]$ImageName) {
    if (-not $ImageName) { return $null }
    $directPath = Join-Path $ReportDirectory $ImageName
    if (Test-Path -LiteralPath $directPath -PathType Leaf) { return $directPath }
    return Join-Path (Join-Path $ReportDirectory "images") $ImageName
}

function Test-Score($Value) {
    if ($null -eq $Value) { return $false }
    try { $score = [double]$Value } catch { return $false }
    return -not [double]::IsNaN($score) -and -not [double]::IsInfinity($score) -and $score -ge 0.0 -and $score -le 1.0
}

if (-not $ReportPath) {
    $ReportPath = @(
        (Join-Path $repositoryRoot "tests\MiniPdf.Benchmark\reports\comparison_report.json"),
        (Join-Path $repositoryRoot "tests\MiniPdf.Benchmark\reports_docx\comparison_report.json")
    )
}

$resolvedReports = @($ReportPath | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | ForEach-Object {
    (Resolve-Path -LiteralPath $_).Path
})
if ($resolvedReports.Count -eq 0) {
    throw "No benchmark report was found. Run the XLSX or DOCX benchmark first, or pass -ReportPath."
}

$allCases = @()
foreach ($report in $resolvedReports) {
    $data = Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
    $entries = if ($data -is [System.Array]) {
        @($data)
    } elseif ($data.PSObject.Properties.Name -contains "results") {
        @($data.results)
    } elseif ($data.PSObject.Properties.Name -contains "name") {
        @($data)
    } else {
        throw "Unsupported report structure: $report"
    }
    $reportDirectory = Split-Path -Parent $report

    foreach ($entry in $entries) {
        if (-not $entry.name) { continue }
        $propertyNames = @($entry.PSObject.Properties.Name)
        if ($propertyNames -notcontains "overall_score" -or $propertyNames -notcontains "visual_avg" -or
            $propertyNames -notcontains "visual_scores" -or $propertyNames -notcontains "diff_images" -or
            $propertyNames -notcontains "minipdf_pages" -or $propertyNames -notcontains "reference_pages" -or
            $entry.pdf_valid -ne $true -or -not (Test-Score $entry.overall_score) -or
            -not (Test-Score $entry.visual_avg) -or [int]$entry.minipdf_pages -lt 1 -or
            [int]$entry.reference_pages -lt 1) {
            continue
        }

        $format = if ($report -match "docx" -or $entry.name -match "^docx_") { "docx" } else { "xlsx" }
        $visualScores = @($entry.visual_scores)
        $diffImages = @($entry.diff_images)
        if ($visualScores.Count -eq 0 -or $visualScores.Count -ne $diffImages.Count) { continue }
        $comparableIndexes = @(for ($index = 0; $index -lt [Math]::Min($visualScores.Count, $diffImages.Count); $index++) {
            $image = $diffImages[$index]
            $candidateImage = if ($image.minipdf_img) { Resolve-ReportImage $reportDirectory $image.minipdf_img } else { $null }
            $referenceImage = if ($image.reference_img) { Resolve-ReportImage $reportDirectory $image.reference_img } else { $null }
            if ($candidateImage -and $referenceImage -and (Test-Score $visualScores[$index]) -and
                (Test-Path -LiteralPath $candidateImage -PathType Leaf) -and
                (Test-Path -LiteralPath $referenceImage -PathType Leaf)) {
                $index
            }
        })
        if ($comparableIndexes.Count -eq 0) { continue }
        $eligibleIndexes = $comparableIndexes
        $worstIndex = $eligibleIndexes[0]
        foreach ($index in $eligibleIndexes | Select-Object -Skip 1) {
            if ([double]$visualScores[$index] -lt [double]$visualScores[$worstIndex]) {
                $worstIndex = $index
            }
        }

        $diff = if ($diffImages.Count -gt $worstIndex) { $diffImages[$worstIndex] } else { $null }
        $worstPage = if ($diff -and $diff.page) { [int]$diff.page } else { $worstIndex + 1 }
        $pageVisualScore = if ($visualScores.Count -gt $worstIndex) { [double]$visualScores[$worstIndex] } else { [double]$entry.visual_avg }
        $sourceDirectory = if ($format -eq "docx") { "output_docx" } else { "output" }

        $allCases += [pscustomobject]@{
            Format = $format
            Name = [string]$entry.name
            OverallScore = [double]$entry.overall_score
            VisualAverage = [double]$entry.visual_avg
            WorstPage = $worstPage
            PageVisualScore = $pageVisualScore
            CandidateImage = if ($diff -and $diff.minipdf_img) { Resolve-ReportImage $reportDirectory $diff.minipdf_img } else { $null }
            ReferenceImage = if ($diff -and $diff.reference_img) { Resolve-ReportImage $reportDirectory $diff.reference_img } else { $null }
            HeatmapImage = if ($diff -and $diff.PSObject.Properties.Name -contains "heatmap_img" -and $diff.heatmap_img) { Resolve-ReportImage $reportDirectory $diff.heatmap_img } else { $null }
            SourceDocument = Join-Path $repositoryRoot ("tests\MiniPdf.Scripts\{0}\{1}.{2}" -f $sourceDirectory, $entry.name, $format)
            Report = $report
        }
    }
}

$validCases = @($allCases | Where-Object { $_.OverallScore -ge 0.0 -and $_.OverallScore -le 1.0 })
if ($validCases.Count -eq 0) {
    throw "The reports contain no entries with a valid overall_score."
}
$duplicates = @($validCases | Group-Object Format, Name | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) {
    throw "Duplicate benchmark case identities were found: $($duplicates.Name -join ', ')"
}

$ranked = @($validCases | Sort-Object PageVisualScore, VisualAverage, OverallScore, Format, Name)
$allHigh = @($validCases | Where-Object { $_.OverallScore -lt $HighScoreThreshold }).Count -eq 0
$selected = if ($allHigh) { @() } else { @($ranked | Select-Object -First $Count) }
$result = [pscustomobject]@{
    Mode = if ($allHigh) { "create-new" } else { "improve-existing" }
    HighScoreThreshold = $HighScoreThreshold
    Reports = $resolvedReports
    ValidCaseCount = $validCases.Count
    Candidates = $selected
}

if ($Json) {
    $result | ConvertTo-Json -Depth 6
} else {
    Write-Host ("Mode: {0}" -f $result.Mode)
    Write-Host ("Valid cases: {0}; high-score threshold: {1:N4}" -f $result.ValidCaseCount, $HighScoreThreshold)
    if ($allHigh) {
        Write-Host "All valid cases meet the threshold. Add a deterministic, non-duplicate XLSX or DOCX validation case."
    } else {
        $selected | Select-Object Format, Name, OverallScore, VisualAverage, WorstPage, PageVisualScore | Format-Table -AutoSize
    }
}