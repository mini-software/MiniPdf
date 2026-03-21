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



### Single File / Filtered Run
```powershell
scripts/Run-Benchmark.ps1 -Filter "border"           # only cases containing "border"
scripts/Run-Benchmark_docx.ps1 -Filter "heading"
scripts/Run-Benchmark_issues.ps1 -Filter "sa8000"    # -Filter is mandatory for issues
```

