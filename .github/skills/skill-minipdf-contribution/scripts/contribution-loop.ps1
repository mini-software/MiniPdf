[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("Start", "Begin", "Evaluate", "Validate", "Pr", "Status")]
    [string]$Action,
    [ValidateSet("dotnet", "rust")]
    [string]$Implementation = "dotnet",
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

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\..\.."))
$gitDirectory = (& git -C $repositoryRoot rev-parse --absolute-git-dir).Trim()
if ($LASTEXITCODE -ne 0) { throw "The MiniPdf repository could not be resolved." }
$stateDirectory = Join-Path $gitDirectory "minipdf-contribution-loop"
$statePath = Join-Path $stateDirectory "state.json"
$artifactRoot = Join-Path $repositoryRoot "artifacts\skill-minipdf-contribution"

function Save-State($State) {
    New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null
    $State.UpdatedAt = (Get-Date).ToUniversalTime().ToString("o")
    $State | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $statePath -Encoding utf8
}

function Get-State {
    if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) {
        throw "No contribution loop is active. Run with -Action Start first."
    }
    return Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
}

function Write-Result($Value) {
    if ($Json) {
        $Value | ConvertTo-Json -Depth 10
    } else {
        $Value | Format-List | Out-Host
    }
}

function Invoke-CheckedCommand([scriptblock]$Command, [string]$FailureMessage) {
    & $Command
    if ($LASTEXITCODE -ne 0) { throw $FailureMessage }
}

function Resolve-Python {
    $repositoryPython = @(
        (Join-Path $repositoryRoot ".venv\Scripts\python.exe"),
        (Join-Path $repositoryRoot ".venv/bin/python")
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
    if ($repositoryPython) {
        $env:PATH = "$(Split-Path -Parent $repositoryPython);$env:PATH"
        return $repositoryPython
    }
    $command = Get-Command python, python3 -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $command) { throw "Python 3.10+ is required." }
    return $command.Source
}

function Get-CaseScore([string]$ReportPath, [string]$ExpectedName) {
    if (-not (Test-Path -LiteralPath $ReportPath -PathType Leaf)) {
        throw "Benchmark report was not generated: $ReportPath"
    }
    $data = Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json
    $entries = if ($data -is [System.Array]) {
        @($data)
    } elseif ($data.PSObject.Properties.Name -contains "results") {
        @($data.results)
    } else {
        @($data)
    }
    $entry = @($entries | Where-Object { $_.name -eq $ExpectedName }) | Select-Object -First 1
    if (-not $entry -or -not $entry.pdf_valid -or $null -eq $entry.overall_score -or $null -eq $entry.visual_avg) {
        throw "Case '$ExpectedName' has no valid comparable score in $ReportPath"
    }
    return [pscustomobject]@{
        Name = [string]$entry.name
        OverallScore = [double]$entry.overall_score
        VisualAverage = [double]$entry.visual_avg
        MiniPdfPages = [int]$entry.minipdf_pages
        ReferencePages = [int]$entry.reference_pages
        ReportPath = $ReportPath
    }
}

function Ensure-SourceDocument([string]$DocumentFormat, [string]$DocumentPath) {
    if (Test-Path -LiteralPath $DocumentPath -PathType Leaf) { return }
    $python = Resolve-Python
    $generator = if ($DocumentFormat -eq "xlsx") {
        Join-Path $repositoryRoot "tests\MiniPdf.Scripts\generate_classic_xlsx.py"
    } else {
        Join-Path $repositoryRoot "tests\MiniPdf.Scripts\generate_classic_docx.py"
    }
    Invoke-CheckedCommand { & $python $generator } "Failed to generate the $DocumentFormat benchmark corpus."
    if (-not (Test-Path -LiteralPath $DocumentPath -PathType Leaf)) {
        throw "The selected source document was not generated: $DocumentPath"
    }
}

function Get-SourceDirectory([string]$DocumentFormat) {
    if ($DocumentFormat -eq "xlsx") { return Join-Path $repositoryRoot "tests\MiniPdf.Scripts\output" }
    return Join-Path $repositoryRoot "tests\MiniPdf.Scripts\output_docx"
}

function Ensure-SourceCorpus([string]$DocumentFormat) {
    $sourceDirectory = Get-SourceDirectory $DocumentFormat
    if (@(Get-ChildItem -LiteralPath $sourceDirectory -Filter "*.$DocumentFormat" -File -ErrorAction SilentlyContinue).Count -gt 0) {
        return $sourceDirectory
    }
    $python = Resolve-Python
    $generator = if ($DocumentFormat -eq "xlsx") {
        Join-Path $repositoryRoot "tests\MiniPdf.Scripts\generate_classic_xlsx.py"
    } else {
        Join-Path $repositoryRoot "tests\MiniPdf.Scripts\generate_classic_docx.py"
    }
    Invoke-CheckedCommand { & $python $generator } "Failed to generate the $DocumentFormat benchmark corpus."
    return $sourceDirectory
}

function Get-RustReportPath([string]$DocumentFormat) {
    return Join-Path $artifactRoot "rust-baseline\$DocumentFormat\report\comparison_report.json"
}

function Get-DotNetReportPath([string]$DocumentFormat) {
    return Join-Path $artifactRoot "dotnet-baseline\$DocumentFormat\report\comparison_report.json"
}

function Ensure-DotNetBaselineReport([string]$DocumentFormat) {
    $reportPath = Get-DotNetReportPath $DocumentFormat
    $null = Ensure-SourceCorpus $DocumentFormat
    New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
    $baselineRoot = Join-Path $artifactRoot "dotnet-baseline\$DocumentFormat"
    if (Test-Path -LiteralPath $baselineRoot) { Remove-Item -LiteralPath $baselineRoot -Recurse -Force }
    $logPath = Join-Path $artifactRoot "dotnet-baseline-$DocumentFormat.log"
    $runner = if ($DocumentFormat -eq "xlsx") {
        Join-Path $repositoryRoot "scripts\Run-Benchmark.ps1"
    } else {
        Join-Path $repositoryRoot "scripts\Run-Benchmark_docx.ps1"
    }
    & $runner -SkipInstall -SkipGenerate -Engine libre -ForceReference -NoOpen `
        -PythonPath (Resolve-Python) `
        -MiniPdfDir (Join-Path $baselineRoot "candidate") `
        -ReferenceDir (Join-Path $baselineRoot "reference") `
        -ReportDir (Join-Path $baselineRoot "report") *> $logPath
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
        Get-Content -LiteralPath $logPath -Tail 40 | Write-Host
        throw ".NET $DocumentFormat baseline generation failed. Full log: $logPath"
    }
    return $reportPath
}

function Ensure-RustBaselineReport([string]$DocumentFormat) {
    $reportPath = Get-RustReportPath $DocumentFormat
    $null = Ensure-SourceCorpus $DocumentFormat
    New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
    $baselineRoot = Join-Path $artifactRoot "rust-baseline\$DocumentFormat"
    if (Test-Path -LiteralPath $baselineRoot) { Remove-Item -LiteralPath $baselineRoot -Recurse -Force }
    $logPath = Join-Path $artifactRoot "rust-baseline-$DocumentFormat.log"
    & (Join-Path $repositoryRoot "scripts\Run-Rust-Benchmark.ps1") `
        -Suite classic -Format $DocumentFormat -MinimumScore 0 -ForceReference `
        -CandidateDir (Join-Path $baselineRoot "candidate") `
        -ReferenceDir (Join-Path $baselineRoot "reference") `
        -AuxiliaryReferenceDir (Join-Path $baselineRoot "auxiliary-reference") `
        -ReportDir (Join-Path $baselineRoot "report") *> $logPath
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
        Get-Content -LiteralPath $logPath -Tail 40 | Write-Host
        throw "Rust $DocumentFormat baseline generation failed. Full log: $logPath"
    }
    return $reportPath
}

function Invoke-FocusedBenchmark(
    [string]$Renderer,
    [string]$DocumentFormat,
    [string]$DocumentName,
    [bool]$FreshReference
) {
    $formatRoot = Join-Path $artifactRoot "$Renderer\$DocumentFormat"
    $reportDirectory = Join-Path $formatRoot "report"
    if (Test-Path -LiteralPath $reportDirectory) {
        Remove-Item -LiteralPath $reportDirectory -Recurse -Force
    }
    if ($Renderer -eq "rust") {
        $referenceDirectory = Join-Path $formatRoot "reference"
        $auxiliaryDirectory = Join-Path $formatRoot "auxiliary-reference"
        if ($FreshReference) {
            Remove-Item -LiteralPath (Join-Path $referenceDirectory "$DocumentName.pdf") -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath (Join-Path $auxiliaryDirectory "$DocumentName.pdf") -Force -ErrorAction SilentlyContinue
        }
        $arguments = @{
            Suite = "classic"
            Format = $DocumentFormat
            Filter = $DocumentName
            MinimumScore = 0
            CandidateDir = (Join-Path $formatRoot "candidate")
            ReferenceDir = $referenceDirectory
            AuxiliaryReferenceDir = $auxiliaryDirectory
            ReportDir = $reportDirectory
        }
        if ($FreshReference) { $arguments.ForceReference = $true } else { $arguments.SkipReference = $true }
        & (Join-Path $repositoryRoot "scripts\Run-Rust-Benchmark.ps1") @arguments
    } else {
        if ($FreshReference) {
            $referencePdf = Join-Path (Join-Path $formatRoot "reference") "$DocumentName.pdf"
            Remove-Item -LiteralPath $referencePdf -Force -ErrorAction SilentlyContinue
        }
        $runner = if ($DocumentFormat -eq "xlsx") {
            Join-Path $repositoryRoot "scripts\Run-Benchmark.ps1"
        } else {
            Join-Path $repositoryRoot "scripts\Run-Benchmark_docx.ps1"
        }
        $arguments = @{
            SkipInstall = $true
            SkipGenerate = $true
            Engine = "libre"
            Filter = $DocumentName
            MiniPdfDir = (Join-Path $formatRoot "minipdf")
            ReferenceDir = (Join-Path $formatRoot "reference")
            ReportDir = $reportDirectory
            Heatmaps = $true
            NoOpen = $true
            PythonPath = (Resolve-Python)
        }
        if ($FreshReference) { $arguments.ForceReference = $true } else { $arguments.SkipReference = $true }
        & $runner @arguments
    }
    if ($LASTEXITCODE -ne 0) { throw "The focused $Renderer $DocumentFormat benchmark failed." }
    return Get-CaseScore (Join-Path $reportDirectory "comparison_report.json") $DocumentName
}

function New-CheckpointTree {
    New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null
    $temporaryIndex = Join-Path $stateDirectory "checkpoint.index"
    $actualIndex = (& git -C $repositoryRoot rev-parse --git-path index).Trim()
    if (-not [System.IO.Path]::IsPathRooted($actualIndex)) { $actualIndex = Join-Path $repositoryRoot $actualIndex }
    Copy-Item -LiteralPath $actualIndex -Destination $temporaryIndex -Force
    $previousIndex = $env:GIT_INDEX_FILE
    try {
        $env:GIT_INDEX_FILE = $temporaryIndex
        & git -C $repositoryRoot add -A
        if ($LASTEXITCODE -ne 0) { throw "Could not stage the temporary checkpoint index." }
        $tree = (& git -C $repositoryRoot write-tree).Trim()
        if ($LASTEXITCODE -ne 0 -or -not $tree) { throw "Could not create the attempt checkpoint." }
        return $tree
    } finally {
        $env:GIT_INDEX_FILE = $previousIndex
        Remove-Item -LiteralPath $temporaryIndex -Force -ErrorAction SilentlyContinue
    }
}

function Restore-Checkpoint([string]$Tree) {
    $untracked = @(& git -C $repositoryRoot ls-files --others --exclude-standard)
    foreach ($relativePath in $untracked) {
        $fullPath = Join-Path $repositoryRoot $relativePath
        if (Test-Path -LiteralPath $fullPath) { Remove-Item -LiteralPath $fullPath -Recurse -Force }
    }
    & git -C $repositoryRoot read-tree --reset -u $Tree
    if ($LASTEXITCODE -ne 0) { throw "Could not restore checkpoint tree $Tree." }
    & git -C $repositoryRoot reset --mixed HEAD | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not restore the normal Git index after rollback." }
}

function Compare-Reports(
    [string]$BeforePath,
    [string]$AfterPath,
    [double]$Tolerance,
    [string]$DocumentFormat,
    [string]$SourceDirectory
) {
    $before = @(Get-Content -LiteralPath $BeforePath -Raw | ConvertFrom-Json)
    $after = @(Get-Content -LiteralPath $AfterPath -Raw | ConvertFrom-Json)
    $beforeByName = @{}
    foreach ($entry in $before) {
        if ($entry.name -and $null -ne $entry.visual_avg) { $beforeByName[[string]$entry.name] = $entry }
    }
    $afterGroups = @($after | Where-Object { $_.name } | Group-Object name)
    $afterByName = @{}
    foreach ($group in $afterGroups) {
        if ($group.Count -eq 1) { $afterByName[[string]$group.Name] = $group.Group[0] }
    }
    $expectedNames = @(Get-ChildItem -LiteralPath $SourceDirectory -Filter "*.$DocumentFormat" -File | ForEach-Object BaseName)
    if ($expectedNames.Count -eq 0) { throw "No $DocumentFormat source documents were found in $SourceDirectory" }
    $regressions = @()
    foreach ($name in $expectedNames) {
        $duplicate = @($afterGroups | Where-Object { $_.Name -eq $name -and $_.Count -ne 1 }) | Select-Object -First 1
        if ($duplicate) {
            $regressions += [pscustomobject]@{ Format = $DocumentFormat; Name = $name; Metric = "duplicate"; Delta = $null }
            continue
        }
        if (-not $afterByName.ContainsKey($name)) {
            $regressions += [pscustomobject]@{ Format = $DocumentFormat; Name = $name; Metric = "missing"; Delta = $null }
            continue
        }
        $current = $afterByName[$name]
        if (-not $current.pdf_valid -or $null -eq $current.visual_avg) {
            $regressions += [pscustomobject]@{ Format = $DocumentFormat; Name = $name; Metric = "invalid"; Delta = $null }
            continue
        }
        if (-not $beforeByName.ContainsKey($name)) { continue }
        $entry = $beforeByName[$name]
        if ([int]$current.minipdf_pages -ne [int]$entry.minipdf_pages) {
            $regressions += [pscustomobject]@{
                Format = $DocumentFormat
                Name = $name
                Metric = "page_count"
                Before = [int]$entry.minipdf_pages
                After = [int]$current.minipdf_pages
                Delta = [int]$current.minipdf_pages - [int]$entry.minipdf_pages
            }
        }
        $delta = [double]$current.visual_avg - [double]$entry.visual_avg
        if ($delta -lt -$Tolerance) {
            $regressions += [pscustomobject]@{
                Format = $DocumentFormat
                Name = $name
                Metric = "visual_avg"
                Before = [double]$entry.visual_avg
                After = [double]$current.visual_avg
                Delta = $delta
            }
        }
    }
    return @($regressions)
}

$null = Resolve-Python

switch ($Action) {
    "Start" {
        $status = @(& git -C $repositoryRoot status --short --untracked-files=all)
        if ($status.Count -gt 0) {
            throw "Start requires a clean working tree so failed attempts can be restored without losing user work."
        }
        $preflightJson = & (Join-Path $PSScriptRoot "preflight.ps1") -Implementation $Implementation -Json
        $preflight = ($preflightJson | Out-String) | ConvertFrom-Json
        if (-not $preflight.Ready) { throw "Preflight failed. Follow the reported setup guidance and run Start again." }
        $python = Resolve-Python
        $pythonPackages = @("openpyxl", "pymupdf", "Pillow", "python-docx")
        if ($Implementation -eq "rust") { $pythonPackages += "pywin32" }
        Invoke-CheckedCommand {
            & $python -m pip install @pythonPackages --quiet
        } "Could not install benchmark Python dependencies."

        $branch = (& git -C $repositoryRoot branch --show-current).Trim()
        if ($branch -eq "main" -or $branch -eq "master") {
            $branch = "improve/$Implementation-visual-parity-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            & git -C $repositoryRoot switch -c $branch
            if ($LASTEXITCODE -ne 0) { throw "Could not create contribution branch $branch." }
        }

        $reportPaths = if ($Implementation -eq "rust") {
            @((Ensure-RustBaselineReport "xlsx"), (Ensure-RustBaselineReport "docx"))
        } else {
            @((Ensure-DotNetBaselineReport "xlsx"), (Ensure-DotNetBaselineReport "docx"))
        }
        $selectionJson = & (Join-Path $PSScriptRoot "select-candidates.ps1") -ReportPath $reportPaths -Json
        if ($LASTEXITCODE -ne 0) { throw "Candidate selection failed." }
        $selection = ($selectionJson | Out-String) | ConvertFrom-Json
        New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null
        $baselineDirectory = Join-Path $stateDirectory "baseline"
        New-Item -ItemType Directory -Force -Path $baselineDirectory | Out-Null
        Copy-Item $reportPaths[0] (Join-Path $baselineDirectory "xlsx.json") -Force
        Copy-Item $reportPaths[1] (Join-Path $baselineDirectory "docx.json") -Force
        $candidates = @($selection.Candidates | ForEach-Object {
            [pscustomobject]@{
                Format = $_.Format
                Name = $_.Name
                SourceDocument = $_.SourceDocument
                Attempts = 0
                Status = "pending"
            }
        })
        $state = [pscustomobject]@{
            Version = 2
            Implementation = $Implementation
            Branch = $branch
            Mode = $selection.Mode
            MaximumAttempts = 3
            Candidates = $candidates
            ActiveAttempt = $null
            Evidence = @()
            AttemptHistory = @()
            ValidationApproved = $false
            ValidatedTree = $null
            Regressions = @()
            UpdatedAt = $null
        }
        Save-State $state
        Write-Result $state
    }
    "Begin" {
        if (-not $Format -or -not $CaseName) { throw "Begin requires -Format and -CaseName." }
        $state = Get-State
        if ($state.ActiveAttempt) { throw "Evaluate or roll back the active attempt before beginning another one." }
        $candidate = @($state.Candidates | Where-Object { $_.Format -eq $Format -and $_.Name -eq $CaseName }) | Select-Object -First 1
        if (-not $candidate) { throw "'$Format/$CaseName' is not one of the selected candidates." }
        if ($candidate.Status -ne "pending") { throw "'$CaseName' is already $($candidate.Status) and cannot be reopened." }
        if ([int]$candidate.Attempts -ge [int]$state.MaximumAttempts) { throw "'$CaseName' already used all three attempts." }
        Ensure-SourceDocument $Format $candidate.SourceDocument
        $baseline = Invoke-FocusedBenchmark $state.Implementation $Format $CaseName $true
        $candidate.Attempts = [int]$candidate.Attempts + 1
        $candidate.Status = "attempting"
        $state.ActiveAttempt = [pscustomobject]@{
            Implementation = $state.Implementation
            Format = $Format
            Name = $CaseName
            Number = $candidate.Attempts
            CheckpointTree = New-CheckpointTree
            Baseline = $baseline
        }
        Save-State $state
        Write-Result $state.ActiveAttempt
    }
    "Evaluate" {
        $state = Get-State
        if (-not $state.ActiveAttempt) { throw "No attempt is active. Run Begin before Evaluate." }
        $attempt = $state.ActiveAttempt
        $candidate = @($state.Candidates | Where-Object { $_.Format -eq $attempt.Format -and $_.Name -eq $attempt.Name }) | Select-Object -First 1
        try {
            $after = Invoke-FocusedBenchmark $state.Implementation $attempt.Format $attempt.Name $false
        } catch {
            $failureMessage = $_.Exception.Message
            Restore-Checkpoint $attempt.CheckpointTree
            $candidate.Status = if ([int]$candidate.Attempts -ge [int]$state.MaximumAttempts) { "skipped" } else { "pending" }
            $failureResult = [pscustomobject]@{
                Implementation = $state.Implementation
                Format = $attempt.Format
                Name = $attempt.Name
                Attempt = $attempt.Number
                Accepted = $false
                Error = $failureMessage
                Before = $attempt.Baseline
                After = $null
            }
            $state.AttemptHistory = @($state.AttemptHistory) + $failureResult
            $state.ActiveAttempt = $null
            $state.ValidationApproved = $false
            $state.ValidatedTree = $null
            Save-State $state
            throw "Focused attempt failed and was rolled back: $failureMessage"
        }
        $overallDelta = $after.OverallScore - [double]$attempt.Baseline.OverallScore
        $visualDelta = $after.VisualAverage - [double]$attempt.Baseline.VisualAverage
        $accepted = $overallDelta -ge $MinimumImprovement -and $visualDelta -ge 0
        $result = [pscustomobject]@{
            Implementation = $state.Implementation
            Format = $attempt.Format
            Name = $attempt.Name
            Attempt = $attempt.Number
            Accepted = $accepted
            OverallDelta = $overallDelta
            VisualDelta = $visualDelta
            Before = $attempt.Baseline
            After = $after
        }
        if ($accepted) {
            $candidate.Status = "accepted"
            $state.Evidence = @($state.Evidence) + $result
        } else {
            Restore-Checkpoint $attempt.CheckpointTree
            $candidate.Status = if ([int]$candidate.Attempts -ge [int]$state.MaximumAttempts) { "skipped" } else { "pending" }
        }
        $state.AttemptHistory = @($state.AttemptHistory) + $result
        $state.ActiveAttempt = $null
        $state.ValidationApproved = $false
        $state.ValidatedTree = $null
        Save-State $state
        Write-Result $result
    }
    "Validate" {
        $state = Get-State
        if ($state.ActiveAttempt) { throw "Evaluate the active attempt before full validation." }
        if ($state.Implementation -eq "rust") {
            $cargo = Get-Command cargo -ErrorAction SilentlyContinue | Select-Object -First 1
            $cargoPath = if ($cargo) { $cargo.Source } else { Join-Path $env:USERPROFILE ".cargo\bin\cargo.exe" }
            Invoke-CheckedCommand {
                & $cargoPath test --manifest-path (Join-Path $repositoryRoot "minipdf-rs\Cargo.toml") --workspace
            } "The Rust workspace test suite failed."
        } else {
            Invoke-CheckedCommand {
                & dotnet test (Join-Path $repositoryRoot "tests\MiniPdf.Tests") -c Release
            } "The MiniPdf .NET test suite failed."
        }

        $validationRoot = Join-Path $artifactRoot "full-validation\$($state.Implementation)"
        $xlsxOutput = Join-Path $validationRoot "xlsx"
        $docxOutput = Join-Path $validationRoot "docx"
        foreach ($outputDirectory in @($xlsxOutput, $docxOutput)) {
            if (Test-Path -LiteralPath $outputDirectory) {
                Remove-Item -LiteralPath $outputDirectory -Recurse -Force
            }
            New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
        }
        $xlsxLog = Join-Path $xlsxOutput "benchmark.log"
        $docxLog = Join-Path $docxOutput "benchmark.log"
        if ($state.Implementation -eq "rust") {
            $rustBaselineRoot = Join-Path $artifactRoot "rust-baseline"
            & (Join-Path $repositoryRoot "scripts\Run-Rust-Benchmark.ps1") `
                -Suite classic -Format xlsx -MinimumScore 0 -SkipReference `
                -CandidateDir (Join-Path $xlsxOutput "candidate") `
                -ReferenceDir (Join-Path $rustBaselineRoot "xlsx\reference") `
                -AuxiliaryReferenceDir (Join-Path $rustBaselineRoot "xlsx\auxiliary-reference") `
                -ReportDir (Join-Path $xlsxOutput "report") *> $xlsxLog
        } else {
            $dotnetBaselineRoot = Join-Path $artifactRoot "dotnet-baseline"
            & (Join-Path $repositoryRoot "scripts\Run-Benchmark.ps1") -SkipInstall -SkipGenerate -SkipReference -Engine libre -NoOpen `
                -PythonPath (Resolve-Python) `
                -MiniPdfDir (Join-Path $xlsxOutput "minipdf") `
                -ReferenceDir (Join-Path $dotnetBaselineRoot "xlsx\reference") `
                -ReportDir (Join-Path $xlsxOutput "report") *> $xlsxLog
        }
        if ($LASTEXITCODE -ne 0) {
            Get-Content -LiteralPath $xlsxLog -Tail 40 | Write-Host
            throw "The full $($state.Implementation) XLSX benchmark failed. Full log: $xlsxLog"
        }
        if ($state.Implementation -eq "rust") {
            & (Join-Path $repositoryRoot "scripts\Run-Rust-Benchmark.ps1") `
                -Suite classic -Format docx -MinimumScore 0 -SkipReference `
                -CandidateDir (Join-Path $docxOutput "candidate") `
                -ReferenceDir (Join-Path $rustBaselineRoot "docx\reference") `
                -AuxiliaryReferenceDir (Join-Path $rustBaselineRoot "docx\auxiliary-reference") `
                -ReportDir (Join-Path $docxOutput "report") *> $docxLog
        } else {
            & (Join-Path $repositoryRoot "scripts\Run-Benchmark_docx.ps1") -SkipInstall -SkipGenerate -SkipReference -Engine libre -NoOpen `
                -PythonPath (Resolve-Python) `
                -MiniPdfDir (Join-Path $docxOutput "minipdf") `
                -ReferenceDir (Join-Path $dotnetBaselineRoot "docx\reference") `
                -ReportDir (Join-Path $docxOutput "report") *> $docxLog
        }
        if ($LASTEXITCODE -ne 0) {
            Get-Content -LiteralPath $docxLog -Tail 40 | Write-Host
            throw "The full $($state.Implementation) DOCX benchmark failed. Full log: $docxLog"
        }

        $baselineDirectory = Join-Path $stateDirectory "baseline"
        $regressions = @()
        $regressions += Compare-Reports (Join-Path $baselineDirectory "xlsx.json") (Join-Path $xlsxOutput "report\comparison_report.json") $RegressionTolerance "xlsx" (Join-Path $repositoryRoot "tests\MiniPdf.Scripts\output")
        $regressions += Compare-Reports (Join-Path $baselineDirectory "docx.json") (Join-Path $docxOutput "report\comparison_report.json") $RegressionTolerance "docx" (Join-Path $repositoryRoot "tests\MiniPdf.Scripts\output_docx")
        & git -C $repositoryRoot diff --check
        if ($LASTEXITCODE -ne 0) { throw "git diff --check failed." }
        $state.ValidationApproved = $regressions.Count -eq 0
        $state.ValidatedTree = if ($state.ValidationApproved) { New-CheckpointTree } else { $null }
        $state.Regressions = @($regressions)
        Save-State $state
        if ($regressions.Count -gt 0) {
            $regressions | Format-Table Format, Name, Metric, Before, After, Delta -AutoSize | Out-Host
            throw "Full validation found $($regressions.Count) visual, page-count, or completeness regression(s)."
        }
        Write-Result ([pscustomobject]@{ Implementation = $state.Implementation; Approved = $true; RegressionTolerance = $RegressionTolerance; Regressions = @() })
    }
    "Pr" {
        $state = Get-State
        if (-not $state.ValidationApproved) { throw "Run a successful -Action Validate before preparing a pull request." }
        $currentTree = New-CheckpointTree
        if (-not $state.ValidatedTree -or $currentTree -ne $state.ValidatedTree) {
            throw "The working tree changed after validation. Run -Action Validate again."
        }
        $currentBranch = (& git -C $repositoryRoot branch --show-current).Trim()
        if ($currentBranch -ne $state.Branch) { throw "Expected branch '$($state.Branch)', but '$currentBranch' is checked out." }
        $accepted = @($state.Evidence | Where-Object { $_.Accepted })
        if ($accepted.Count -eq 0) { throw "No candidate improvement was accepted; a pull request cannot be prepared." }
        $caseNames = @($accepted | ForEach-Object { $_.Name })
        if (-not $Title) { $Title = "Improve MiniPdf $($state.Implementation) rendering parity: $($caseNames -join ', ')" }
        $bodyPath = Join-Path $artifactRoot "pull-request.md"
        New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
        $rows = @($accepted | ForEach-Object {
            "| $($_.Implementation) | $($_.Name) | $($_.Format) | $([double]$_.Before.OverallScore) | $([double]$_.After.OverallScore) | $([double]$_.Before.VisualAverage) | $([double]$_.After.VisualAverage) | $($_.After.MiniPdfPages)/$($_.After.ReferencePages) |"
        })
        @(
            "## Summary"
            ""
            "Improves MiniPdf $($state.Implementation) rendering parity for the automatically selected low-score benchmark cases."
            ""
            "## Benchmark Evidence"
            ""
            "| Implementation | Case | Format | Before overall | After overall | Before visual | After visual | Pages candidate/reference |"
            "| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |"
            $rows
            ""
            "## Validation"
            ""
            "- [x] Focused $($state.Implementation) benchmarks with fresh references"
            "- [x] Full $($state.Implementation) test suite"
            "- [x] Full $($state.Implementation) XLSX and DOCX benchmark regression gate"
            "- [x] git diff --check"
        ) | Set-Content -LiteralPath $bodyPath -Encoding utf8

        $gh = Get-Command gh -ErrorAction SilentlyContinue | Select-Object -First 1
        $authenticated = $false
        if ($gh) {
            & $gh.Source auth status *> $null
            $authenticated = $LASTEXITCODE -eq 0
        }
        if ($CreatePullRequest) {
            if (-not $authenticated) {
                Write-Host "GitHub CLI is unavailable or unauthenticated. Install it from https://cli.github.com/ and run 'gh auth login', or use the browser steps below." -ForegroundColor Yellow
            } else {
                $status = @(& git -C $repositoryRoot status --short --untracked-files=all)
                if ($status.Count -gt 0) { throw "Commit the validated changes before creating the pull request." }
                & git -C $repositoryRoot rev-parse --abbrev-ref --symbolic-full-name "@{upstream}" *> $null
                if ($LASTEXITCODE -ne 0) { throw "Push the branch with -u before creating the pull request." }
                $unpushed = [int]((& git -C $repositoryRoot rev-list --count "@{upstream}..HEAD").Trim())
                if ($LASTEXITCODE -ne 0 -or $unpushed -ne 0) { throw "Push all branch commits before creating the pull request." }
                $login = (& $gh.Source api user --jq .login).Trim()
                if ($LASTEXITCODE -ne 0 -or -not $login) { throw "GitHub CLI could not resolve the authenticated login." }
                $head = "${login}:$($state.Branch)"
                & $gh.Source pr create --repo mini-software/MiniPdf --base main --head $head --title $Title --body-file $bodyPath
                if ($LASTEXITCODE -ne 0) { throw "GitHub CLI could not create the pull request. Confirm the branch is committed and pushed." }
                break
            }
        }
        $compareUrl = "https://github.com/mini-software/MiniPdf/compare/main...<fork-owner>:$($state.Branch)?expand=1"
        Write-Result ([pscustomobject]@{
            PullRequestMode = if ($authenticated) { "github-cli" } else { "browser" }
            Title = $Title
            BodyFile = $bodyPath
            PushCommand = "git push -u <fork-remote> $($state.Branch)"
            CliCommand = "gh pr create --repo mini-software/MiniPdf --base main --head <fork-owner>:$($state.Branch) --title `"$Title`" --body-file `"$bodyPath`""
            ManualSteps = @(
                "Fork https://github.com/mini-software/MiniPdf",
                "Commit the approved changes and push $($state.Branch) to your fork",
                "Open $compareUrl",
                "Paste the generated title and body, review the files, and submit"
            )
        })
    }
    "Status" {
        Write-Result (Get-State)
    }
}