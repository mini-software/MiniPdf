- English
- No Emoji
- .NET security policy
- System can't auto git push 

## Documentation Workflow

When updating README.md or documentation, sync the corresponding translated README files under `documents/` in the same change. Keep API examples, CLI examples, command tables, benchmark numbers, and report links aligned across languages.

## Issue Summary Workflow
When user says "summary on issue #N", use `gh issue comment N --body-file -` 
to post a summary of completed work to the GitHub issue.

## Testing Workflow

After making code changes, run the relevant validation before reporting completion.

### Unit Tests
```powershell
dotnet test tests/MiniPdf.Tests
```



### Single File / Filtered Run
```powershell
scripts/Run-Benchmark.ps1 -Filter "border"           # only cases containing "border"
scripts/Run-Benchmark_docx.ps1 -Filter "heading"
scripts/Run-Benchmark_issues.ps1 -Filter "sa8000"    # focused issue run
scripts/Run-Benchmark_issues.ps1 -All                 # full issue benchmark
```

## Classic Benchmark Refresh Workflow

When updating all XLSX classic examples, canonical benchmark reports, or stale GitHub README benchmark images, use the `refresh-classic-benchmarks` skill (`/refresh-classic-benchmarks`).

## Automated Contribution Loop

All coding agents, including Copilot, Claude Code, Cursor, and Codex, must use
the vendor-neutral workflow in `CONTRIBUTING.md`. The executable entry point is:

```powershell
scripts/Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
scripts/Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

Preserve unrelated changes. Do not commit, push, fork, or open a pull request
without explicit user approval.


