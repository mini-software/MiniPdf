<#
.SYNOPSIS
    Runs one MiniPdf implementation against the shared XLSX, DOCX, and PPTX corpus.

.DESCRIPTION
    Every language reads tests/MiniPdf.Benchmark/shared-office-corpus.json and writes
    isolated candidates and reports below artifacts/benchmark/<language>. LibreOffice
    reference PDFs are shared by content hash across all language runs.

.EXAMPLE
    .\scripts\Run-Java-VisualBenchmark.ps1 -Format all -MaxCasesPerFormat 1
    .\scripts\Run-Python-VisualBenchmark.ps1 -Format pptx -Filter "Asian Pacific"
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dotnet", "rust", "java", "go", "python", "node")]
    [string]$Language,
    [ValidateSet("all", "xlsx", "docx", "pptx")]
    [string]$Format = "all",
    [string]$Filter,
    [int]$MaxCasesPerFormat = 0,
    [int]$MaxComparePages = 0,
    [double]$MinimumScore = 0.0,
    [string]$CorpusManifest = "tests/MiniPdf.Benchmark/shared-office-corpus.json",
    [string]$ArtifactRoot,
    [switch]$SkipBuild,
    [switch]$SkipReference,
    [switch]$SkipCompare,
    [switch]$ForceReference
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ($MaxCasesPerFormat -lt 0) { throw "MaxCasesPerFormat cannot be negative." }
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

function Get-LibreOfficePath {
    $Command = Get-Command soffice -ErrorAction SilentlyContinue
    if ($Command) {
        $ConsoleLauncher = [System.IO.Path]::ChangeExtension($Command.Source, ".com")
        if ($IsWindows -and (Test-Path -LiteralPath $ConsoleLauncher)) { return $ConsoleLauncher }
        return $Command.Source
    }
    $Candidates = @(
        (Join-Path $env:ProgramFiles "LibreOffice/program/soffice.com"),
        (Join-Path ${env:ProgramFiles(x86)} "LibreOffice/program/soffice.com"),
        (Join-Path $env:ProgramFiles "LibreOffice/program/soffice.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "LibreOffice/program/soffice.exe")
    )
    foreach ($Candidate in $Candidates) {
        if ($Candidate -and (Test-Path -LiteralPath $Candidate)) { return $Candidate }
    }
    throw "LibreOffice soffice was not found. Install LibreOffice or use -SkipReference."
}

$CorpusManifest = Resolve-RepoPath $CorpusManifest
if (-not (Test-Path -LiteralPath $CorpusManifest)) {
    throw "Shared corpus manifest not found: $CorpusManifest"
}
$Corpus = Get-Content -LiteralPath $CorpusManifest -Raw | ConvertFrom-Json
$SelectedCases = [System.Collections.Generic.List[object]]::new()
$CaseSources = @{}

foreach ($Source in $Corpus.sources) {
    if ($Format -ne "all" -and $Source.format -ne $Format) { continue }
    $SourceRoot = Resolve-RepoPath $Source.root
    if (-not (Test-Path -LiteralPath $SourceRoot -PathType Container)) {
        throw "Corpus source directory not found: $SourceRoot"
    }
    $Files = @(Get-ChildItem -LiteralPath $SourceRoot -File -Filter $Source.pattern | Where-Object {
        -not $Filter -or $_.Name -like "*$Filter*"
    } | Sort-Object Name)
    if ($MaxCasesPerFormat -gt 0) {
        $Files = @($Files | Select-Object -First $MaxCasesPerFormat)
    }
    foreach ($File in $Files) {
        $Hash = (Get-FileHash -LiteralPath $File.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $SafeStem = ($File.BaseName -replace '[^A-Za-z0-9._-]', '_').Trim('_')
        if (-not $SafeStem) { $SafeStem = "fixture" }
        $CaseId = "$($Source.format)--$SafeStem--$($Hash.Substring(0, 10))"
        if ($CaseSources.ContainsKey($CaseId)) { throw "Duplicate benchmark case id: $CaseId" }
        $RelativePath = [System.IO.Path]::GetRelativePath($RepoRoot, $File.FullName).Replace("\", "/")
        $Case = [pscustomobject]@{
            name = $CaseId
            case_id = $CaseId
            display_name = $File.BaseName
            suite = "shared-office"
            format = $Source.format
            source_path = $RelativePath
            source_sha256 = $Hash
            conversion_status = "pending"
            conversion_exit_code = $null
            candidate_exists = $false
            reference_exists = $false
        }
        $SelectedCases.Add($Case)
        $CaseSources[$CaseId] = $File.FullName
    }
}

if ($SelectedCases.Count -eq 0) {
    throw "No shared corpus cases matched format=$Format filter='$Filter'."
}

$ArtifactRoot = Resolve-RepoPath $(if ($ArtifactRoot) { $ArtifactRoot } else { "artifacts/benchmark" })
$SharedRoot = Join-Path $ArtifactRoot "shared"
$LanguageRoot = Join-Path $ArtifactRoot $Language
$CandidateDir = Join-Path $LanguageRoot "candidates"
$ReportDir = Join-Path $LanguageRoot "report"
$ReferenceDir = Join-Path $SharedRoot "libreoffice-reference"
$ResolvedManifest = Join-Path $LanguageRoot "resolved-manifest.json"
$CoverageManifest = Join-Path $LanguageRoot "benchmark-coverage.json"
$ReferenceWorkDir = Join-Path $SharedRoot "reference-work"

New-Item -ItemType Directory -Force -Path $CandidateDir, $ReportDir, $ReferenceDir, $ReferenceWorkDir | Out-Null
Write-Json ([pscustomobject]@{
    corpus = [System.IO.Path]::GetRelativePath($RepoRoot, $CorpusManifest).Replace("\", "/")
    corpus_version = $Corpus.version
    cases = $SelectedCases
}) $ResolvedManifest

$Tools = @{}
if (-not $SkipBuild) {
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
            $GoOutput = Join-Path $LanguageRoot $(if ($IsWindows) { "minipdf-go.exe" } else { "minipdf-go" })
            Push-Location (Join-Path $RepoRoot "minipdf-go")
            try { & $Tools.go build -o $GoOutput ./cmd/minipdf } finally { Pop-Location }
            Assert-CommandSucceeded "Go CLI build"
        }
        "python" {
            $Tools.python = if (Test-Path (Join-Path $RepoRoot ".venv/Scripts/python.exe")) {
                Join-Path $RepoRoot ".venv/Scripts/python.exe"
            } else { Find-Command "python" }
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
        $Tools.cli = Join-Path $LanguageRoot $(if ($IsWindows) { "minipdf-go.exe" } else { "minipdf-go" })
    }
    "python" {
        if (-not $Tools.python) {
            $Tools.python = if (Test-Path (Join-Path $RepoRoot ".venv/Scripts/python.exe")) {
                Join-Path $RepoRoot ".venv/Scripts/python.exe"
            } else { Find-Command "python" }
        }
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

if (-not $SkipReference) {
    $Soffice = Get-LibreOfficePath
    foreach ($Case in $SelectedCases) {
        $ReferencePath = Join-Path $ReferenceDir ($Case.name + ".pdf")
        if ($ForceReference -or -not (Test-Pdf $ReferencePath)) {
            Get-ChildItem -LiteralPath $ReferenceWorkDir -File -Filter "*.pdf" | Remove-Item -Force
            $ProfileDir = Join-Path $ReferenceWorkDir ("profile-" + [System.Guid]::NewGuid().ToString("N"))
            New-Item -ItemType Directory -Force -Path $ProfileDir | Out-Null
            $ProfileUri = ([System.Uri]$ProfileDir).AbsoluteUri
            try {
                & $Soffice --headless --norestore "-env:UserInstallation=$ProfileUri" `
                    --convert-to pdf --outdir $ReferenceWorkDir $CaseSources[$Case.case_id]
                Assert-CommandSucceeded "LibreOffice conversion for $($Case.source_path)"
            } finally {
                Remove-Item -LiteralPath $ProfileDir -Recurse -Force -ErrorAction SilentlyContinue
            }
            $GeneratedPath = Join-Path $ReferenceWorkDir ([System.IO.Path]::GetFileNameWithoutExtension($CaseSources[$Case.case_id]) + ".pdf")
            if (-not (Test-Pdf $GeneratedPath)) { throw "LibreOffice did not produce a valid PDF for $($Case.source_path)." }
            Move-Item -LiteralPath $GeneratedPath -Destination $ReferencePath -Force
        }
    }
}

foreach ($Case in $SelectedCases) {
    $Case.reference_exists = Test-Pdf (Join-Path $ReferenceDir ($Case.name + ".pdf"))
}

$PassedConversions = @($SelectedCases | Where-Object conversion_status -eq "passed").Count
$MissingReferences = @($SelectedCases | Where-Object reference_exists -eq $false).Count
$Coverage = [pscustomobject]@{
    language = $Language
    corpus_manifest = [System.IO.Path]::GetRelativePath($RepoRoot, $CorpusManifest).Replace("\", "/")
    resolved_manifest = [System.IO.Path]::GetRelativePath($RepoRoot, $ResolvedManifest).Replace("\", "/")
    fixture_scope = "shared-git-tracked-office-corpus"
    selected_cases = $SelectedCases.Count
    passed_conversions = $PassedConversions
    failed_conversions = $SelectedCases.Count - $PassedConversions
    missing_references = $MissingReferences
    comparison_completed = $false
    comparison_results = 0
    average_score = $null
    cases = $SelectedCases
}
Write-Json $Coverage $CoverageManifest
Write-Json ([pscustomobject]@{
    corpus = [System.IO.Path]::GetRelativePath($RepoRoot, $CorpusManifest).Replace("\", "/")
    corpus_version = $Corpus.version
    cases = $SelectedCases
}) $ResolvedManifest

if (-not $SkipCompare) {
    if ($MissingReferences -gt 0) {
        throw "$MissingReferences shared reference PDFs are missing. Run without -SkipReference."
    }
    $Python = if (Test-Path (Join-Path $RepoRoot ".venv/Scripts/python.exe")) {
        Join-Path $RepoRoot ".venv/Scripts/python.exe"
    } else { Find-Command "python" }
    $CompareArgs = @(
        (Join-Path $RepoRoot "tests/MiniPdf.Benchmark/compare_pdfs.py"),
        "--minipdf-dir", $CandidateDir,
        "--reference-dir", $ReferenceDir,
        "--report-dir", $ReportDir,
        "--manifest", $ResolvedManifest,
        "--report-scope", "$Language-shared-office",
        "--candidate-label", "$Language MiniPdf",
        "--reference-label", "LibreOffice",
        "--composite-images",
        "--heatmaps"
    )
    if ($MaxComparePages -gt 0) { $CompareArgs += @("--max-pages", $MaxComparePages) }
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
    if ($BelowThreshold.Count -gt 0) {
        throw "$($BelowThreshold.Count) cases scored below MinimumScore=$MinimumScore."
    }
}

Write-Host "$Language benchmark: selected=$($SelectedCases.Count), converted=$PassedConversions, missing references=$MissingReferences"
Write-Host "Coverage: $CoverageManifest"
if (-not $SkipCompare) { Write-Host "Report: $(Join-Path $ReportDir 'comparison_report.md')" }

if ($PassedConversions -ne $SelectedCases.Count) {
    throw "$Language candidate conversion failed for $($SelectedCases.Count - $PassedConversions) cases."
}