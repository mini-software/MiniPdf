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

### Python

Install Python 3.10 or later, then run:

```powershell
python -m pip install -e ".\minipdf-python[dev]"
python -m pytest minipdf-python/tests
python -m ruff check minipdf-python
python -m ruff format --check minipdf-python
python -m mypy minipdf-python/src minipdf-python/tests
python -m build minipdf-python
python -m twine check minipdf-python/dist/*
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
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Before running any command, ask me whether to use .NET or Rust and wait for my explicit choice. Then use the selected implementation to diagnose and improve one automatically selected benchmark case, validate all changes, and prepare the pull request. Keep generated images and full logs in artifacts and return only paths and compact summaries. Do not commit, push, fork, or open a pull request without my explicit approval.
```

The agent integrations are convenience prompts; the workflow and safety gates
live in one vendor-neutral command. Agents must ask the user to choose .NET or
Rust before running it, then pass the explicit implementation. The workflow
selects one candidate:

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation <dotnet-or-rust>
```

Pass `-Implementation dotnet` or `-Implementation rust` according to the user's
answer. Do not use `auto` from an agent workflow. Pass `-CandidateCount 2` only
for an explicitly requested two-case run. The selected implementation is stored
in the loop state for subsequent actions.

`Start` requires a clean working tree. It checks prerequisites, creates an
implementation-specific branch, builds fresh XLSX and DOCX baselines, and
selects the largest visual difference.

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

Keep generated PDFs, screenshots, heatmaps, reports, and complete command output
under `artifacts/`. Agent responses should contain paths, score summaries, and
only the last relevant error lines. Never embed image data, base64 payloads, or
complete logs in chat. Start a fresh chat before `Validate` when the current
context approaches 150,000 tokens or already contains large image/tool output;
the loop state in `.git/minipdf-contribution-loop/` supports resuming.

### Agent Shortcuts

| Agent | Shortcut |
|---|---|
| GitHub Copilot | `/skill-minipdf-contribution`; asks for .NET or Rust when omitted |
| Claude Code | `/minipdf-contribution`; asks for `dotnet` or `rust` when omitted |
| Cursor | `/minipdf-contribution`; asks for `dotnet` or `rust` when omitted |
| Codex | Ask: `Run the MiniPdf contribution loop`; it asks for .NET or Rust |
| Any terminal agent | Run the vendor-neutral PowerShell commands above |

Agents must preserve unrelated changes and must not commit, push, fork, or open
a pull request without explicit approval.