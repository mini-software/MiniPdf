---
description: Run the automated MiniPdf rendering contribution loop for .NET or Rust
argument-hint: "[dotnet|rust]"
---

Read `CONTRIBUTING.md`, especially "Donate Compute with Any Coding Agent".
Requested implementation: `$ARGUMENTS`

Use the requested implementation from the command arguments. When omitted, let
the controller detect installed toolchains and randomly choose an available
implementation. Execute the vendor-neutral
`scripts/Invoke-MiniPdfContributionLoop.ps1` workflow end to end: Start, up to
three Begin/Evaluate attempts per selected case, Validate, and Pr. Diagnose and
fix root causes between Begin and Evaluate. Preserve unrelated changes, and do
not commit, push, fork, or create the pull request without explicit approval.