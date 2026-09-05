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

### Cross-Language Visual Benchmarks

Use the following form for .NET, Rust, Java, Go, Python, and Node:

```powershell
scripts/Run-<Language>-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
```

Rust may also use `scripts/Run-Rust-Benchmark.ps1`; it forwards to the same
shared runner. Microsoft 365 is always the primary scored reference and
LibreOffice is the required auxiliary reference. `-Engine` does not switch the
primary reference. The default minimum score is `0.95`; use `-SkipCandidate`
only when the corresponding candidate PDFs already exist under
`artifacts/<language>-benchmark/<suite>/<format>/candidates`.

## Classic Benchmark Refresh Workflow

When updating all XLSX classic examples, canonical benchmark reports, or stale GitHub README benchmark images, use the `refresh-classic-benchmarks` skill (`/refresh-classic-benchmarks`).

## Automated Contribution Loop

All coding agents, including Copilot, Claude Code, Cursor, and Codex, must use
the vendor-neutral workflow in `CONTRIBUTING.md`. The executable entry point is:

```powershell
scripts/Invoke-MiniPdfContributionLoop.ps1 -Action Start
```

The default detects installed .NET and Rust toolchains and randomly chooses an
available implementation. Pass `-Implementation dotnet` or
`-Implementation rust` to override it.

Preserve unrelated changes. Do not commit, push, fork, or open a pull request
without explicit user approval.


