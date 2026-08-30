---
name: skill-minipdf-contribution
description: "Run an automated .NET or Rust MiniPdf contribution loop that selects the two largest XLSX or DOCX visual differences, attempts each fix up to three times with rollback, runs full regression checks, and prepares or creates a PR. Use when: contributing compute time, fixing low-score Office-to-PDF images, running the self-evolution loop, or preparing a benchmark-backed MiniPdf PR."
argument-hint: "Choose .NET or Rust; optionally specify XLSX or DOCX, a benchmark case, or a score threshold"
user-invocable: true
disable-model-invocation: false
---

# MiniPdf Contribution Loop

Turn local compute time into a focused, reproducible MiniPdf rendering improvement for either implementation. This Copilot skill is one adapter for the vendor-neutral workflow in `CONTRIBUTING.md`; Claude Code, Cursor, Codex, and terminal agents use the same controller. In VS Code Chat, run `/skill-minipdf-contribution .NET` or `/skill-minipdf-contribution Rust`.

## Automatic Path

Run this once from the repository root:

```powershell
$loop = ".\scripts\Invoke-MiniPdfContributionLoop.ps1"
& $loop -Action Start -Implementation dotnet
# or
& $loop -Action Start -Implementation rust
```

`.NET` is the default when `-Implementation` is omitted. `Start` requires a clean working tree, runs implementation-specific preflight, installs benchmark Python packages, creates a local `improve/<implementation>-visual-parity-*` branch, builds fresh isolated XLSX/DOCX baselines for the current HEAD, selects the two documents with the lowest page-level visual scores for that renderer, and stores the choice in `.git/minipdf-contribution-loop/`.

For Rust, `Start` first builds fresh isolated XLSX and DOCX baseline reports from the shared classic corpus. The current Rust benchmark uses Microsoft 365 as the primary scored reference and LibreOffice as an auxiliary reference. It therefore requires Cargo, desktop Excel and Word, Python, and LibreOffice. The .NET path requires the .NET SDK, Python, and LibreOffice.

For each returned candidate, in order:

```powershell
& $loop -Action Begin -Format <xlsx-or-docx> -CaseName <case-name>
# Diagnose, add a focused test, and make one small root-cause edit.
& $loop -Action Evaluate
```

`Begin` regenerates a fresh LibreOffice reference, records the baseline, and creates a Git tree checkpoint without changing branch history. `Evaluate` reruns the focused candidate and accepts only when `overall_score` improves by at least `0.0001` and `visual_avg` does not decrease. Otherwise it restores the checkpoint automatically. Try at most three times for that candidate; after the third rejection its status becomes `skipped`, so continue with the other selected candidate.

After one or more candidates are accepted, run:

```powershell
& $loop -Action Validate
& $loop -Action Pr
```

`Validate` runs `dotnet test` for .NET or `cargo test --workspace` for Rust, then regenerates every XLSX and DOCX candidate against the exact references captured at `Start`. It rejects missing/invalid cases, page-count changes, or any `visual_avg` drop greater than `0.002`. Text extraction can vary with installed fonts, so full-suite `overall_score` changes are reported but are not the image-regression gate. `Pr` includes the implementation in its evidence, reports whether authenticated GitHub CLI is available, and prints ready push/`gh pr create` commands or browser steps. After explicit approval, commit and push the branch, then use `-Action Pr -CreatePullRequest` to open the PR automatically.

If both candidates are skipped, do not create a PR. Report the three measured attempts for each case and end the run. A later run can use refreshed benchmark reports to select the next lowest pair.

## Safety and Scope

- Work from the MiniPdf repository root.
- Start only from a clean working tree; this is what makes automatic rollback safe.
- Support XLSX and DOCX for both .NET and Rust; PPTX remains .NET-only and is outside this loop.
- Use LibreOffice as the .NET rendering reference. The Rust benchmark currently scores Microsoft 365 as primary and displays LibreOffice as auxiliary.
- Never commit, push, create a fork, or open a PR without explicit user approval.
- Do not treat a stale report as proof of an improvement. Re-run the focused benchmark with a fresh reference.

## 1. Run Preflight

Run:

```powershell
.\.github\skills\skill-minipdf-contribution\scripts\preflight.ps1
```

The script checks Git, GitHub CLI authentication, Python, LibreOffice, and local LibreOffice/POI source trees. Pass `-Implementation dotnet` to require the .NET SDK, or `-Implementation rust` to require Cargo plus desktop Excel and Word.

Git, Python, and LibreOffice are shared requirements. The selected implementation additionally requires either the .NET SDK or Rust/Cargo; Rust visual baselines also require desktop Excel and Word. GitHub CLI and source trees are optional but strongly recommended. If a source tree is missing, give clone instructions; do not download it automatically.

Use `-Json` when another tool or agent needs structured results.

## 2. Select Work

Run:

```powershell
.\.github\skills\skill-minipdf-contribution\scripts\select-candidates.ps1
```

Defaults:

- Reports: `tests/MiniPdf.Benchmark/reports/comparison_report.json` and `tests/MiniPdf.Benchmark/reports_docx/comparison_report.json`
- Candidate count: 2
- High-score threshold: `0.95`

The selector chooses each document's lowest comparable visual-score page, then ranks distinct documents by page visual score, visual average, and overall score. Invalid PDFs and incomplete report rows are excluded. Use `-Json` for structured output or override inputs with `-ReportPath`, `-Count`, and `-HighScoreThreshold`.

If the mode is `improve-existing`, attempt the returned cases in order. Two cases are a work budget, not a requirement to force two unrelated changes into one PR.

If the mode is `create-new`, follow section 5.

## 3. Reproduce a Candidate Manually

The controller's `Begin` action performs this step automatically. Use these commands only for diagnosis or recovery.

Re-run one case at a time with a fresh LibreOffice reference and heatmaps:

```powershell
# XLSX
.\scripts\Run-Benchmark.ps1 -SkipInstall -SkipGenerate -Engine libre -Filter "<case>" -ForceReference -Heatmaps `
	-MiniPdfDir "artifacts\skill-minipdf-contribution\xlsx\minipdf" `
	-ReferenceDir "artifacts\skill-minipdf-contribution\xlsx\reference" `
	-ReportDir "artifacts\skill-minipdf-contribution\xlsx\report"

# DOCX
.\scripts\Run-Benchmark_docx.ps1 -SkipInstall -SkipGenerate -Engine libre -Filter "<case>" -ForceReference -Heatmaps `
	-MiniPdfDir "artifacts\skill-minipdf-contribution\docx\minipdf" `
	-ReferenceDir "artifacts\skill-minipdf-contribution\docx\reference" `
	-ReportDir "artifacts\skill-minipdf-contribution\docx\report"

# Rust XLSX or DOCX
.\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format <xlsx-or-docx> -Filter "<case>" `
	-MinimumScore 0 -ForceReference
```

Record the baseline `overall_score`, `visual_avg`, lowest page score, page counts, and heatmap paths. Confirm both PDFs exist and the reference was regenerated rather than skipped.

Open the candidate, reference, and heatmap images for the lowest-scoring page. Classify the mismatch before editing: page setup, scale, font metrics, text layout, borders/fills, images/drawings, headers/footers, or pagination.

## 4. Diagnose and Fix

1. Load the `libreoffice-reference` skill before designing an Office rendering heuristic.
2. Search the local LibreOffice source for renderer behavior and POI source for OOXML interpretation.
3. Locate the nearest selected implementation code that directly computes the mismatched output (`src/MiniPdf` or `minipdf-rs`).
4. State one falsifiable local hypothesis and identify a focused check.
5. Add or update a focused unit test, then make the smallest root-cause fix.
6. Re-run the focused test and the same benchmark command.
7. Keep a change only when the evidence improves the intended behavior without creating a known regression.

If two candidates share one root cause, validate both and keep them together. If they need unrelated fixes, complete one coherent change and recommend a separate PR for the other.

## 5. Add a New Validation Document

Use this path only when all valid report entries meet the high-score threshold.

1. Inspect `tests/MiniPdf.Scripts/generate_classic_xlsx.py` and `generate_classic_docx.py`.
2. Search existing generator names, document content, and tests for the proposed Office behavior.
3. Choose the next available format-specific case number and a descriptive unique name.
4. Add a deterministic generator with fixed content, dimensions, styles, and metadata. Do not use random values, current timestamps, network assets, or machine-specific paths.
5. Generate at most two new documents, then benchmark each against a fresh LibreOffice reference.
6. Prefer fixing the lower-scoring new case. Keep the generated source document and generator change aligned with repository conventions.

Good additions isolate one previously uncovered behavior. Avoid documents that merely vary text or colors already represented by the corpus.

## 6. Validate

The controller's `Validate` action is the required final gate. During an attempt, run the narrowest relevant test first. For manual recovery, the minimum checks are:

```powershell
# .NET
dotnet test tests/MiniPdf.Tests -c Release

# Rust
cargo test --manifest-path minipdf-rs/Cargo.toml --workspace

git diff --check
git status --short
```

For every changed benchmark case, report before/after scores, page counts, and visual evidence. Do not claim improvement based only on unit tests or only on an overall average. A PR is permitted only when the controller records `ValidationApproved: true`.

## 7. Prepare the Pull Request

Use [the PR evidence template](./assets/pull-request-template.md). Include the root cause, why the behavior matches LibreOffice/OOXML semantics, tests, and before/after benchmark evidence.

Check PR tooling at execution time:

```powershell
git --version
gh --version
gh auth status
```

When `gh` is installed and authenticated, the controller can execute this command after the user approves commit and push:

```powershell
gh pr create --repo mini-software/MiniPdf --base main --head <fork-owner>:<branch> --title "<title>" --body-file <body-file>
```

Equivalent controller command:

```powershell
& $loop -Action Pr -CreatePullRequest
```

When `gh` is unavailable or unauthenticated, provide these exact manual steps:

1. Fork `https://github.com/mini-software/MiniPdf` in the browser.
2. Add the fork as a remote and push the approved branch: `git push -u <fork-remote> <branch>`.
3. Open `https://github.com/mini-software/MiniPdf/compare/main...<fork-owner>:<branch>?expand=1`.
4. Paste the prepared title and PR body, review the changed files, and submit.

Never publish benchmark artifacts from `artifacts/`; use tracked canonical report locations only when the contribution intentionally updates them.