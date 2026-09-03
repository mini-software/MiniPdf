---
description: Run the automated MiniPdf rendering contribution loop for .NET or Rust
---

Read `CONTRIBUTING.md`, especially "Donate Compute with Any Coding Agent".
Requested implementation: `$ARGUMENTS`

Infer `dotnet` or `rust` from the command arguments. When omitted, let the
controller detect installed toolchains and randomly choose an available
implementation. Execute `scripts/Invoke-MiniPdfContributionLoop.ps1` through Start,
Begin/Evaluate (at most three attempts per selected case), Validate, and Pr.
Diagnose and fix the root cause between Begin and Evaluate. Preserve unrelated
changes, and do not commit, push, fork, or create the pull request without
explicit approval.