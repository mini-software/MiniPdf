---
name: minipdf-contribution
description: "Run the vendor-neutral MiniPdf .NET or Rust rendering contribution loop with automatic low-score selection, three-attempt rollback, full regression validation, and PR preparation. Use when: donating compute, improving visual benchmark scores, or preparing a benchmark-backed MiniPdf contribution."
argument-hint: "Choose dotnet or rust"
---

# MiniPdf Contribution Loop

Read `CONTRIBUTING.md`, especially "Donate Compute with Any Coding Agent", and
execute `scripts/Invoke-MiniPdfContributionLoop.ps1` as the source of truth.

Choose `dotnet` or `rust`, then carry the workflow through Start, focused
Begin/Evaluate attempts, Validate, and Pr. Make one evidence-driven root-cause
change per attempt. Preserve unrelated changes. Never commit, push, fork, or
open a pull request without explicit user approval.