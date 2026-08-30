# Contributing to MiniPdf

## Donate Compute with Any Coding Agent

The automated rendering-improvement loop works with any coding agent that can
edit repository files and run PowerShell, including GitHub Copilot, Claude Code,
Cursor, and Codex. The agent integrations are convenience prompts; the workflow
and safety gates live in one vendor-neutral command:

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

## Agent Shortcuts

| Agent | Shortcut |
|---|---|
| GitHub Copilot | `/skill-minipdf-contribution .NET` or `/skill-minipdf-contribution Rust` |
| Claude Code | `/minipdf-contribution dotnet` or `/minipdf-contribution rust` |
| Cursor | `/minipdf-contribution dotnet` or `/minipdf-contribution rust` |
| Codex | Ask: `Run the MiniPdf contribution loop for dotnet` or `... for rust` |
| Any terminal agent | Run the vendor-neutral PowerShell commands above |

Agents must preserve unrelated changes and must not commit, push, fork, or open
a pull request without explicit approval.