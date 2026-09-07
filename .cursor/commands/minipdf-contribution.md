---
description: Run the automated MiniPdf rendering contribution loop for .NET or Rust
---

Read `CONTRIBUTING.md`, especially "Automated Rendering Improvement".
Requested implementation: `$ARGUMENTS`

Infer `dotnet` or `rust` from the command arguments. When omitted, ask the user
to choose `dotnet` or `rust` and wait for an explicit answer before running any
command. Pass that choice explicitly; do not use `auto`. Execute
`scripts/Invoke-MiniPdfContributionLoop.ps1` through
Start, Begin/Evaluate (at most three attempts for the single selected case),
Validate, and Pr. Diagnose and fix the root cause between Begin and Evaluate.
Keep images and full logs in artifacts and return only paths and compact
summaries. Preserve unrelated changes, and do not commit, push, fork, or create
the pull request without explicit approval.