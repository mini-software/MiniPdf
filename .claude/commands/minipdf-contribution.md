---
description: Run the automated MiniPdf rendering contribution loop for .NET or Rust
argument-hint: "[dotnet|rust]"
---

Read `CONTRIBUTING.md`, especially "Automated Rendering Improvement".
Requested implementation: `$ARGUMENTS`

Use the requested implementation from the command arguments. When omitted, ask
the user to choose `dotnet` or `rust` and wait for an explicit answer before
running any command. Pass that choice explicitly; do not use `auto`. Execute the vendor-neutral
`scripts/Invoke-MiniPdfContributionLoop.ps1` workflow end to end: Start, up to
three Begin/Evaluate attempts for the single selected case, Validate, and Pr.
Diagnose and fix root causes between Begin and Evaluate. Keep images and full
logs in artifacts and return only paths and compact summaries. Preserve
unrelated changes, and do not commit, push, fork, or create the pull request
without explicit approval.