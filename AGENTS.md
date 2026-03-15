- English
- .NET security policy
- System can't auto git push 

## Issue Summary Workflow
When user says "summary on issue #N", use `gh issue comment N --body-file -` 
to post a summary of completed work to the GitHub issue.

## Testing Workflow

### Unit Tests
```powershell
dotnet test tests/MiniPdf.Tests
```

### Benchmarks (Excel / DOCX / Issues)
```powershell
scripts/Run-Benchmark.ps1          # Excel (150+ cases)
scripts/Run-Benchmark_docx.ps1     # DOCX
scripts/Run-Benchmark_issues.ps1   # User-reported issues
```
Flags: `-CompareOnly`, `-SkipGenerate`, `-SkipReference`, `-SkipMiniPdf`

### Single File / Filtered Run
```powershell
scripts/Run-Benchmark.ps1 -Filter "border"           # only cases containing "border"
scripts/Run-Benchmark_docx.ps1 -Filter "heading"
scripts/Run-Benchmark_issues.ps1 -Filter "sa8000"
```

### Quick Compare Only
```powershell
cd tests/MiniPdf.Benchmark
python compare_pdfs.py
```

### Scoring
`overall = text×0.4 + visual×0.4 + pages×0.2` — report in `tests/MiniPdf.Benchmark/reports/`