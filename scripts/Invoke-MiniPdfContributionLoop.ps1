<#
.SYNOPSIS
    Run the MiniPdf contribution loop from any coding agent or terminal.

.EXAMPLE
    .\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start

.EXAMPLE
    .\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("Start", "Begin", "Evaluate", "Validate", "Pr", "Status")]
    [string]$Action,
    [ValidateSet("auto", "dotnet", "rust")]
    [string]$Implementation = "auto",
    [ValidateSet("xlsx", "docx")]
    [string]$Format,
    [string]$CaseName,
    [ValidateRange(0.0, 1.0)]
    [double]$MinimumImprovement = 0.0001,
    [ValidateRange(0.0, 1.0)]
    [double]$RegressionTolerance = 0.002,
    [string]$Title,
    [switch]$CreatePullRequest,
    [switch]$Json
)

$controller = Join-Path (Split-Path -Parent $PSScriptRoot) ".github\skills\skill-minipdf-contribution\scripts\contribution-loop.ps1"
if (-not (Test-Path -LiteralPath $controller -PathType Leaf)) {
    throw "MiniPdf contribution controller was not found: $controller"
}

& $controller @PSBoundParameters