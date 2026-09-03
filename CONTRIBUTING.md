# Contributing to MiniPdf

Thank you for helping MiniPdf. Contributions include code, documentation,
tests, issue triage, design discussion, release verification, and user support.

Please follow the `CODE_OF_CONDUCT.md`. Project decisions and the path to
maintainership are documented in `GOVERNANCE.md`; current priorities are in
`ROADMAP.md`.

## Before You Start

- Search existing issues and discussions before opening a new one.
- Use an issue or discussion for a significant API, architecture, dependency,
	compatibility, governance, or release-process change before implementation.
- Do not submit confidential documents, personal data, proprietary fonts, or
	test files that you do not have permission to redistribute.
- Report suspected vulnerabilities through `SECURITY.md`, not a public issue.

By submitting a contribution, you represent that you have the right to submit
it under the repository's Apache License 2.0. Clearly identify third-party code,
fixtures, images, fonts, or generated material and include their source and
license. MiniPdf does not currently require a separate contributor license
agreement.

## Development Setup

Clone your fork and create a focused branch. Keep unrelated formatting,
generated artifacts, and benchmark refreshes out of the change.

### .NET

Install a supported .NET SDK, then run:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test tests/MiniPdf.Tests --configuration Release
```

### Rust

Install the stable Rust toolchain, then run:

```powershell
Set-Location minipdf-rs
cargo fmt --check
cargo clippy --workspace --all-targets -- -D warnings
cargo test --workspace
```

### Documentation

Keep commands, APIs, benchmark results, and report links consistent. Changes to
the English `README.md` must also be applied to the corresponding translated
README files under `documents/`.

## Making a Change

1. Reproduce the behavior with the smallest useful test or fixture.
2. Make one focused change at the owning implementation boundary.
3. Add or update automated tests for behavior changes.
4. Run the relevant .NET or Rust checks above.
5. For rendering changes, run the narrowest applicable visual benchmark.
6. Explain compatibility, security, provenance, and documentation effects in
	 the pull request.

Useful focused benchmark commands include:

```powershell
scripts/Run-Benchmark.ps1 -Filter "border"
scripts/Run-Benchmark_docx.ps1 -Filter "heading"
scripts/Run-Benchmark_issues.ps1 -Filter "sa8000"
scripts/Run-Rust-Benchmark.ps1 -Filter "classic01" -MinimumScore 0.99
```

Generated PDFs, PNGs, heatmaps, and reports should only be committed when they
are canonical project evidence requested by the relevant workflow. Otherwise,
attach them to the pull request or retain them as CI artifacts.

## Pull Requests

Pull requests should be small enough to review and should include:

- the problem and intended behavior;
- linked issues or design discussions when applicable;
- tests and exact validation commands;
- before-and-after benchmark evidence for rendering changes;
- public API or compatibility impact;
- the source and license of new third-party material; and
- documentation updates for user-visible behavior.

At least one maintainer approval is currently required before merge. The
project aims to provide an initial review within seven days, but complex visual
rendering and security changes may take longer. Authors should respond to review
questions and avoid force-pushing after review begins unless rebasing is needed.

Maintainers may request that a large change be split, discussed publicly, or
reworked to preserve compatibility and project scope. Reviews assess the change,
not the contributor.

## Earning More Responsibility

Sustained contributions and sound judgment can lead to maintainer
responsibility. Code volume is not the only form of merit: reviews,
documentation, issue triage, release verification, security work, and community
support all count. See `GOVERNANCE.md` for the selection process and
`MAINTAINERS.md` for areas that need additional ownership.

## Automated Rendering Improvement

The automated rendering-improvement loop works with any coding agent that can
edit repository files and run PowerShell, including GitHub Copilot, Claude Code,
Cursor, and Codex. The easiest way to start is to paste this prompt into the
agent chat:

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop for dotnet from start to finish. Diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

Replace `dotnet` with `rust` to work on the Rust implementation. The agent
integrations are convenience prompts; the workflow and safety gates live in
one vendor-neutral command:

```powershell
# Choose one implementation.
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

`Start` requires a clean working tree. It checks prerequisites, creates an
implementation-specific branch, builds fresh XLSX and DOCX baselines, and
selects the two largest visual differences.

For each selected candidate, run:

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Begin -Format <xlsx-or-docx> -CaseName <case-name>
# Let the coding agent diagnose, test, and make one focused root-cause change.
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Evaluate
```

An attempt is accepted only when its overall score improves and its visual score
does not decrease. A rejected or failed attempt is restored automatically. Each
candidate gets at most three attempts before the loop skips it.

After at least one accepted improvement:

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Validate
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Pr
```

`Validate` runs the selected implementation's full test suite and XLSX/DOCX
visual regression gate. `Pr` generates benchmark evidence and either a GitHub
CLI command or browser instructions. Committing, pushing, and opening the pull
request still require explicit user approval.

### Agent Shortcuts

| Agent | Shortcut |
|---|---|
| GitHub Copilot | `/skill-minipdf-contribution .NET` or `/skill-minipdf-contribution Rust` |
| Claude Code | `/minipdf-contribution dotnet` or `/minipdf-contribution rust` |
| Cursor | `/minipdf-contribution dotnet` or `/minipdf-contribution rust` |
| Codex | Ask: `Run the MiniPdf contribution loop for dotnet` or `... for rust` |
| Any terminal agent | Run the vendor-neutral PowerShell commands above |

Agents must preserve unrelated changes and must not commit, push, fork, or open
a pull request without explicit approval.