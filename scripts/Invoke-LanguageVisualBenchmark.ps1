<#
.SYNOPSIS
    Runs one MiniPdf implementation against the repository's visual benchmark fixtures.

.DESCRIPTION
    Uses Microsoft 365 as the primary scored reference and LibreOffice as the
    required auxiliary reference. Outputs are isolated below
    artifacts/<language>-benchmark/<suite>/<format>.

.EXAMPLE
    .\scripts\Run-Java-VisualBenchmark.ps1 -Suite classic -Format xlsx -MaxCases 1
    .\scripts\Run-Python-VisualBenchmark.ps1 -Suite issue -Format pptx -Filter "Asian Pacific"
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dotnet", "rust", "java", "go", "python", "node")]
    [string]$Language,
    [ValidateSet("all", "classic", "issue")]
    [string]$Suite = "all",
    [ValidateSet("xlsx", "docx", "pptx")]
    [string]$Format = "xlsx",
    [ValidateSet("o365", "office", "libre")]
    [string]$Engine = "o365",
    [string]$Filter,
    [int]$MaxCases = 0,
    [int]$MaxComparePages = 15,
    [double]$MinimumScore = 0.95,
    [string]$SourceDir,
    [string]$CandidateDir,
    [string]$ReferenceDir,
    [string]$AuxiliaryReferenceDir,
    [string]$ReportDir,
    [string]$ArtifactRoot,
    [switch]$SkipCandidate,
    [switch]$SkipBuild,
    [switch]$SkipReference,
    [switch]$ForceReference
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ($MaxCases -lt 0) { throw "MaxCases cannot be negative." }
if ($MaxComparePages -lt 0) { throw "MaxComparePages cannot be negative." }
if ($MinimumScore -lt 0.0 -or $MinimumScore -gt 1.0) {
    throw "MinimumScore must be between 0.0 and 1.0."
}

function Resolve-RepoPath([string]$PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) { return $PathValue }
    return Join-Path $RepoRoot $PathValue
}

function Write-Json([object]$Value, [string]$Path) {
    $Json = $Value | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText($Path, $Json, [System.Text.UTF8Encoding]::new($false))
}

function Assert-CommandSucceeded([string]$Description) {
    if ($LASTEXITCODE -ne 0) { throw "$Description failed with exit code $LASTEXITCODE." }
}

function Find-Command([string]$Name) {
    return (Get-Command $Name -ErrorAction Stop).Source
}

function Find-Python {
    $RelativePath = if ($IsWindows) { ".venv/Scripts/python.exe" } else { ".venv/bin/python" }
    $Candidate = Join-Path $RepoRoot $RelativePath
    if (Test-Path -LiteralPath $Candidate) { return $Candidate }
    return Find-Command "python"
}

function Find-Maven {
    $Command = Get-Command mvn -ErrorAction SilentlyContinue
    if ($Command) { return $Command.Source }
    foreach ($MavenRoot in @($env:MAVEN_HOME, $env:M2_HOME)) {
        if ($MavenRoot) {
            $Candidate = Join-Path $MavenRoot $(if ($IsWindows) { "bin/mvn.cmd" } else { "bin/mvn" })
            if (Test-Path -LiteralPath $Candidate) { return $Candidate }
        }
    }
    if ($IsWindows) {
        $Candidate = Get-ChildItem (Join-Path $env:LOCALAPPDATA "Programs/Apache Maven") `
            -Recurse -Filter "mvn.cmd" -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
        if ($Candidate) { return $Candidate }
    }
    throw "Maven was not found. Add mvn to PATH or set MAVEN_HOME."
}

function Find-JavaExecutable {
    $ExecutableName = if ($IsWindows) { "java.exe" } else { "java" }
    if ($IsWindows) {
        foreach ($JavaRoot in @(
            (Join-Path $env:ProgramFiles "Eclipse Adoptium"),
            (Join-Path $env:ProgramFiles "Java")
        )) {
            if (-not (Test-Path -LiteralPath $JavaRoot)) { continue }
            $Candidate = Get-ChildItem -LiteralPath $JavaRoot -Directory -Filter "jdk*" |
                Sort-Object Name -Descending |
                ForEach-Object { Join-Path $_.FullName "bin/java.exe" } |
                Where-Object { Test-Path -LiteralPath $_ } |
                Select-Object -First 1
            if ($Candidate) { return $Candidate }
        }
    }
    if ($env:JAVA_HOME) {
        $Candidate = Join-Path $env:JAVA_HOME "bin/$ExecutableName"
        if (Test-Path -LiteralPath $Candidate) { return $Candidate }
    }
    $Command = Get-Command java -ErrorAction SilentlyContinue
    if ($Command) { return $Command.Source }
    throw "Java was not found. Install JDK 17 or newer, or set JAVA_HOME."
}

function Find-Go {
    $ExecutableName = if ($IsWindows) { "go.exe" } else { "go" }
    if ($env:GOROOT) {
        $Candidate = Join-Path $env:GOROOT "bin/$ExecutableName"
        if (Test-Path -LiteralPath $Candidate) { return $Candidate }
    }
    if ($IsWindows) {
        foreach ($Candidate in @(
            (Join-Path $env:ProgramFiles "Go/bin/go.exe"),
            (Join-Path $env:LOCALAPPDATA "Programs/Go/bin/go.exe")
        )) {
            if (Test-Path -LiteralPath $Candidate) { return $Candidate }
        }
    }
    $Command = Get-Command go -ErrorAction SilentlyContinue
    if ($Command) { return $Command.Source }
    throw "Go was not found. Install Go 1.22 or newer, or set GOROOT."
}

function Test-Pdf([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    $Bytes = [System.IO.File]::ReadAllBytes($Path)
    return $Bytes.Length -ge 5 -and [System.Text.Encoding]::ASCII.GetString($Bytes, 0, 5) -eq "%PDF-"
}

$Defaults = @{
    "classic:xlsx" = @{
        Source = "tests/MiniPdf.Scripts/output"
        LibreReference = "tests/MiniPdf.Benchmark/reference_pdfs"
        OfficeReference = "tests/MiniPdf.Benchmark/office_pdfs"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs.py"
        SourceArgument = "--xlsx-dir"
        OfficeLabel = "Microsoft 365 Excel Reference"
    }
    "classic:docx" = @{
        Source = "tests/MiniPdf.Scripts/output_docx"
        LibreReference = "tests/MiniPdf.Benchmark/reference_pdfs_docx"
        OfficeReference = "tests/MiniPdf.Benchmark/office_pdfs_docx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_docx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_docx.py"
        SourceArgument = "--docx-dir"
        OfficeLabel = "Microsoft 365 Word Reference"
    }
    "issue:xlsx" = @{
        Source = "tests/Issue_Files/xlsx"
        LibreReference = "tests/Issue_Files/reference_xlsx"
        OfficeReference = "tests/Issue_Files/office_xlsx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs.py"
        SourceArgument = "--xlsx-dir"
        OfficeLabel = "Microsoft 365 Excel Reference"
    }
    "issue:docx" = @{
        Source = "tests/Issue_Files/docx"
        LibreReference = "tests/Issue_Files/reference_docx"
        OfficeReference = "tests/Issue_Files/office_docx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_docx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_docx.py"
        SourceArgument = "--docx-dir"
        OfficeLabel = "Microsoft 365 Word Reference"
    }
    "issue:pptx" = @{
        Source = "tests/Issue_Files/pptx"
        LibreReference = "tests/Issue_Files/reference_pptx"
        OfficeReference = "tests/Issue_Files/office_pptx"
        LibreReferenceScript = "tests/MiniPdf.Benchmark/generate_reference_pdfs_pptx.py"
        OfficeReferenceScript = "tests/MiniPdf.Benchmark/generate_office_pdfs_pptx.py"
        SourceArgument = "--pptx-dir"
        OfficeLabel = "Microsoft 365 PowerPoint Reference"
    }
}

$SuiteNames = if ($Suite -eq "all") { @("classic", "issue") } else { @($Suite) }
$SuiteConfigs = @($SuiteNames | ForEach-Object {
    $SuiteName = $_
    $SuiteConfig = $Defaults["$SuiteName`:$Format"]
    if ($SuiteConfig) {
        [pscustomobject]@{ Suite = $SuiteName; Config = $SuiteConfig }
    }
})
if ($SuiteConfigs.Count -eq 0) {
    throw "No $Language benchmark fixtures are configured for suite=$Suite format=$Format."
}
$Config = $SuiteConfigs[0].Config
if ($Engine -eq "libre") {
    Write-Warning "-Engine libre is retained for compatibility. Microsoft 365 remains the primary scored reference; LibreOffice is auxiliary."
}

$ArtifactRoot = Resolve-RepoPath $(if ($ArtifactRoot) { $ArtifactRoot } else { "artifacts/$Language-benchmark/$Suite/$Format" })
$SourceGroups = if ($SourceDir) {
    @([pscustomobject]@{ Suite = $Suite; Config = $Config; Source = Resolve-RepoPath $SourceDir })
} else {
    @($SuiteConfigs | ForEach-Object {
        [pscustomobject]@{ Suite = $_.Suite; Config = $_.Config; Source = Resolve-RepoPath $_.Config.Source }
    })
}
$DefaultReferenceDir = if ($Suite -eq "all") { Join-Path $ArtifactRoot "references/o365" } else { Resolve-RepoPath $Config.OfficeReference }
$DefaultAuxiliaryReferenceDir = if ($Suite -eq "all") { Join-Path $ArtifactRoot "references/libreoffice" } else { Resolve-RepoPath $Config.LibreReference }
$ReferenceDir = Resolve-RepoPath $(if ($ReferenceDir) { $ReferenceDir } else { $DefaultReferenceDir })
$AuxiliaryReferenceDir = Resolve-RepoPath $(if ($AuxiliaryReferenceDir) { $AuxiliaryReferenceDir } else { $DefaultAuxiliaryReferenceDir })
$CandidateDir = Resolve-RepoPath $(if ($CandidateDir) { $CandidateDir } else { Join-Path $ArtifactRoot "candidates" })
$ReportDir = Resolve-RepoPath $(if ($ReportDir) { $ReportDir } else { Join-Path $ArtifactRoot "report" })
$ComparisonManifest = Join-Path $ReportDir "comparison_manifest.json"
$CoverageManifest = Join-Path $ReportDir "benchmark_coverage.json"

$SourceEntries = @($SourceGroups | ForEach-Object {
    $SourceGroup = $_
    Get-ChildItem -LiteralPath $SourceGroup.Source -File -Filter "*.$Format" | Where-Object {
        -not $Filter -or $_.BaseName -like "*$Filter*"
    } | ForEach-Object {
        [pscustomobject]@{ Suite = $SourceGroup.Suite; Config = $SourceGroup.Config; Path = $_ }
    }
} | Sort-Object { $_.Path.Name })
if ($MaxCases -gt 0) {
    $SourceEntries = @($SourceEntries | Select-Object -First $MaxCases)
}
if ($SourceEntries.Count -eq 0) {
    throw "No .$Format files matched '$Filter' in the selected $Suite source directories."
}
$DuplicateNames = @($SourceEntries | Group-Object { $_.Path.BaseName } | Where-Object Count -gt 1)
if ($DuplicateNames.Count -gt 0) {
    throw "Duplicate fixture names cannot share one visual report: $($DuplicateNames.Name -join ', ')"
}

if (Test-Path -LiteralPath $ReportDir) {
    Remove-Item -LiteralPath $ReportDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $CandidateDir, $ReferenceDir, $AuxiliaryReferenceDir, $ReportDir | Out-Null

$SelectedCases = @($SourceEntries | ForEach-Object {
    $SourceEntry = $_
    [pscustomobject]@{
        name = $SourceEntry.Path.BaseName
        case_id = $SourceEntry.Path.BaseName
        suite = $SourceEntry.Suite
        format = $Format
        source_path = [System.IO.Path]::GetRelativePath($RepoRoot, $SourceEntry.Path.FullName).Replace("\", "/")
        conversion_status = "pending"
        conversion_exit_code = $null
        candidate_exists = $false
        reference_exists = $false
        auxiliary_reference_exists = $false
    }
})
$CaseSources = @{}
for ($Index = 0; $Index -lt $SelectedCases.Count; $Index++) {
    $CaseSources[$SelectedCases[$Index].case_id] = $SourceEntries[$Index].Path.FullName
}
Write-Json ([pscustomobject]@{ cases = $SelectedCases }) $ComparisonManifest

$Tools = @{}
if (-not $SkipCandidate -and -not $SkipBuild) {
    switch ($Language) {
        "dotnet" {
            $Tools.dotnet = Find-Command "dotnet"
            & $Tools.dotnet build (Join-Path $RepoRoot "src/MiniPdf.Cli/MiniPdf.Cli.csproj") -c Release
            Assert-CommandSucceeded ".NET CLI build"
        }
        "rust" {
            $Tools.cargo = Find-Command "cargo"
            & $Tools.cargo build --release --manifest-path (Join-Path $RepoRoot "minipdf-rs/Cargo.toml") -p minipdf-cli
            Assert-CommandSucceeded "Rust CLI build"
        }
        "java" {
            $Tools.java = Find-JavaExecutable
            $env:JAVA_HOME = Split-Path -Parent (Split-Path -Parent $Tools.java)
            $Tools.maven = Find-Maven
            & $Tools.maven -B -ntp -f (Join-Path $RepoRoot "minipdf-java/pom.xml") package -DskipTests
            Assert-CommandSucceeded "Java CLI build"
        }
        "go" {
            $Tools.go = Find-Go
            $GoOutput = Join-Path $ArtifactRoot $(if ($IsWindows) { "minipdf-go.exe" } else { "minipdf-go" })
            Push-Location (Join-Path $RepoRoot "minipdf-go")
            try { & $Tools.go build -o $GoOutput ./cmd/minipdf } finally { Pop-Location }
            Assert-CommandSucceeded "Go CLI build"
        }
        "python" {
            $Tools.python = Find-Python
        }
        "node" {
            $Tools.npm = Find-Command "npm"
            Push-Location (Join-Path $RepoRoot "minipdf-node")
            try {
                if (Test-Path "package-lock.json") { & $Tools.npm ci } else { & $Tools.npm install }
                Assert-CommandSucceeded "Node dependency install"
                & $Tools.npm run build
                Assert-CommandSucceeded "Node native module build"
            } finally { Pop-Location }
        }
    }
}

if (-not $SkipCandidate) {
    switch ($Language) {
        "dotnet" {
            if (-not $Tools.dotnet) { $Tools.dotnet = Find-Command "dotnet" }
            $Tools.cli = Get-ChildItem (Join-Path $RepoRoot "src/MiniPdf.Cli/bin/Release") -Recurse -Filter "MiniPdf.Cli.dll" |
                Where-Object FullName -Match 'net9\.0' | Select-Object -First 1 -ExpandProperty FullName
        }
        "rust" {
            $RustName = if ($IsWindows) { "minipdf.exe" } else { "minipdf" }
            $Tools.cli = Join-Path $RepoRoot "minipdf-rs/target/release/$RustName"
        }
        "java" {
            if (-not $Tools.java) { $Tools.java = Find-JavaExecutable }
            $Tools.cli = Get-ChildItem (Join-Path $RepoRoot "minipdf-java/minipdf-cli/target") -Filter "minipdf-cli-*.jar" |
                Where-Object { $_.Name -notmatch '(sources|javadoc|original)' } |
                Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
        }
        "go" {
            $Tools.cli = Join-Path $ArtifactRoot $(if ($IsWindows) { "minipdf-go.exe" } else { "minipdf-go" })
        }
        "python" {
            if (-not $Tools.python) { $Tools.python = Find-Python }
            $env:PYTHONPATH = Join-Path $RepoRoot "minipdf-python/src"
        }
        "node" { $Tools.node = Find-Command "node" }
    }

    if ($Language -notin @("python", "node") -and (-not $Tools.cli -or -not (Test-Path -LiteralPath $Tools.cli))) {
        throw "$Language CLI artifact was not found. Run without -SkipBuild first."
    }

    foreach ($Case in $SelectedCases) {
        $InputPath = $CaseSources[$Case.case_id]
        $OutputPath = Join-Path $CandidateDir ($Case.name + ".pdf")
        if (Test-Path -LiteralPath $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }
        switch ($Language) {
            "dotnet" { & $Tools.dotnet $Tools.cli $InputPath -o $OutputPath }
            "rust" { & $Tools.cli $InputPath -o $OutputPath }
            "java" { & $Tools.java -jar $Tools.cli $InputPath -o $OutputPath }
            "go" { & $Tools.cli $InputPath -o $OutputPath }
            "python" { & $Tools.python -m minipdf $InputPath -o $OutputPath }
            "node" {
                & $Tools.node -e "require(process.argv[1]).convertToPdf(process.argv[2], process.argv[3])" `
                    (Join-Path $RepoRoot "minipdf-node") $InputPath $OutputPath
            }
        }
        $Case.conversion_exit_code = $LASTEXITCODE
        $Case.candidate_exists = Test-Pdf $OutputPath
        $Case.conversion_status = if ($LASTEXITCODE -eq 0 -and $Case.candidate_exists) { "passed" } else { "failed" }
    }
} else {
    foreach ($Case in $SelectedCases) {
        $OutputPath = Join-Path $CandidateDir ($Case.name + ".pdf")
        $Case.candidate_exists = Test-Pdf $OutputPath
        $Case.conversion_status = if ($Case.candidate_exists) { "passed" } else { "failed" }
    }
}

if (-not $SkipReference) {
    $Python = Find-Python
    foreach ($SourceGroup in $SourceGroups) {
        $GroupCases = @($SelectedCases | Where-Object suite -eq $SourceGroup.Suite)
        if ($GroupCases.Count -eq 0) { continue }
        $GroupConfig = $SourceGroup.Config
        $OfficeReferenceScript = Resolve-RepoPath $GroupConfig.OfficeReferenceScript
        $LibreReferenceScript = Resolve-RepoPath $GroupConfig.LibreReferenceScript
        $ReferenceFilters = if ($MaxCases -gt 0) { @($GroupCases.name) } else { @($Filter) }
        foreach ($ReferenceFilter in $ReferenceFilters) {
            $Providers = @(
                [pscustomobject]@{ Script = $OfficeReferenceScript; Directory = $ReferenceDir; Label = $GroupConfig.OfficeLabel },
                [pscustomobject]@{ Script = $LibreReferenceScript; Directory = $AuxiliaryReferenceDir; Label = "LibreOffice" }
            )
            foreach ($Provider in $Providers) {
                $ReferenceArgs = @($Provider.Script, $GroupConfig.SourceArgument, $SourceGroup.Source, "--pdf-dir", $Provider.Directory)
                if ($ReferenceFilter) { $ReferenceArgs += @("--filter", $ReferenceFilter) }
                if ($ForceReference) { $ReferenceArgs += "--force" }
                & $Python -X utf8 @ReferenceArgs
                Assert-CommandSucceeded "$($Provider.Label) generation"
            }
        }
    }
}

foreach ($Case in $SelectedCases) {
    $Case.reference_exists = Test-Pdf (Join-Path $ReferenceDir ($Case.name + ".pdf"))
    $Case.auxiliary_reference_exists = Test-Pdf (Join-Path $AuxiliaryReferenceDir ($Case.name + ".pdf"))
}

$PassedConversions = @($SelectedCases | Where-Object conversion_status -eq "passed").Count
$MissingReferences = @($SelectedCases | Where-Object reference_exists -eq $false).Count
$MissingAuxiliaryReferences = @($SelectedCases | Where-Object auxiliary_reference_exists -eq $false).Count
$Coverage = [pscustomobject]@{
    language = $Language
    suite = $Suite
    format = $Format
    reference_engine = "o365"
    reference_label = $Config.OfficeLabel
    auxiliary_reference_engine = "libreoffice"
    auxiliary_reference_label = "LibreOffice (auxiliary)"
    fixture_scope = "shared-on-disk-fixtures"
    executes_dotnet_xunit = $false
    max_compare_pages = $MaxComparePages
    selected_cases = $SelectedCases.Count
    passed_conversions = $PassedConversions
    failed_conversions = $SelectedCases.Count - $PassedConversions
    missing_references = $MissingReferences
    missing_auxiliary_references = $MissingAuxiliaryReferences
    comparison_completed = $false
    comparison_results = 0
    average_score = $null
    cases = $SelectedCases
}
Write-Json $Coverage $CoverageManifest

$Python = Find-Python
$CompareArgs = @(
    (Join-Path $RepoRoot "tests/MiniPdf.Benchmark/compare_pdfs.py"),
    "--minipdf-dir", $CandidateDir,
    "--reference-dir", $ReferenceDir,
    "--auxiliary-dir", $AuxiliaryReferenceDir,
    "--report-dir", $ReportDir,
    "--manifest", $ComparisonManifest,
    "--report-scope", "$Language-$Suite-$Format",
    "--candidate-label", "$Language MiniPdf",
    "--reference-label", $Config.OfficeLabel,
    "--auxiliary-label", "LibreOffice",
    "--composite-images",
    "--heatmaps"
)
$CompareArgs += @("--max-pages", $MaxComparePages)
& $Python -X utf8 @CompareArgs
Assert-CommandSucceeded "$Language visual comparison"
$Results = @(Get-Content (Join-Path $ReportDir "comparison_report.json") -Raw | ConvertFrom-Json)
$Scores = @($Results | Where-Object { $null -ne $_.overall_score })
$Coverage.comparison_completed = $true
$Coverage.comparison_results = ($Results | Measure-Object).Count
$Coverage.average_score = if ($Scores.Count -gt 0) {
    ($Scores | Measure-Object -Property overall_score -Average).Average
} else { $null }
Write-Json $Coverage $CoverageManifest
$BelowThreshold = @($Results | Where-Object { $null -eq $_.overall_score -or $_.overall_score -lt $MinimumScore })

Write-Host "$Language benchmark: suite=$Suite format=$Format selected=$($SelectedCases.Count), converted=$PassedConversions, missing O365 references=$MissingReferences, missing LibreOffice references=$MissingAuxiliaryReferences"
Write-Host "Coverage: $CoverageManifest"
Write-Host "Report: $(Join-Path $ReportDir 'comparison_report.md')"

if ($PassedConversions -ne $SelectedCases.Count -or $MissingReferences -gt 0 -or $MissingAuxiliaryReferences -gt 0 -or $BelowThreshold.Count -gt 0) {
    throw "$Language benchmark failed: conversion failures=$($SelectedCases.Count - $PassedConversions), missing O365 references=$MissingReferences, missing LibreOffice references=$MissingAuxiliaryReferences, below MinimumScore=$($BelowThreshold.Count)."
}
