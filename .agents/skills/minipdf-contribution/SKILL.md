---
name: minipdf-contribution
description: "Run the vendor-neutral MiniPdf .NET or Rust rendering contribution loop with automatic toolchain detection, random implementation selection, low-score selection, three-attempt rollback, full regression validation, and PR preparation. Use when: donating compute, improving visual benchmark scores, or preparing a benchmark-backed MiniPdf contribution."
argument-hint: "Optionally choose dotnet or rust"
---

# MiniPdf Contribution Loop

Read `CONTRIBUTING.md`, especially "Donate Compute with Any Coding Agent", and
execute `scripts/Invoke-MiniPdfContributionLoop.ps1` as the source of truth.

Run `Start` without an implementation to detect installed .NET and Rust
toolchains and randomly choose an available implementation, or explicitly pass
`dotnet` or `rust`. Then carry the workflow through focused Begin/Evaluate
attempts, Validate, and Pr. Make one evidence-driven root-cause change per
attempt. Preserve unrelated changes. Never commit, push, fork, or open a pull
request without explicit user approval.