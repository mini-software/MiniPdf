---
name: minipdf-contribution
description: "Explicitly run the vendor-neutral MiniPdf rendering contribution loop for one selected benchmark case. Ask the user to choose dotnet or rust before running commands when no implementation is specified."
argument-hint: "Choose dotnet or rust; optionally request up to two candidates"
disable-model-invocation: true
---

# MiniPdf Contribution Loop

Read `CONTRIBUTING.md`, especially "Automated Rendering Improvement", and
execute `scripts/Invoke-MiniPdfContributionLoop.ps1` as the source of truth.

If the invocation does not specify `dotnet` or `rust`, ask the user which
implementation to use and wait for an explicit answer before running any
command. Then pass the choice with `-Implementation dotnet` or
`-Implementation rust`; do not use `auto` or rely on a default. The workflow
selects one candidate unless `-CandidateCount 2` is explicitly requested. Then
carry the workflow through focused Begin/Evaluate
attempts, Validate, and Pr. Make one evidence-driven root-cause change per
attempt. Preserve unrelated changes. Never commit, push, fork, or open a pull
request without explicit user approval. Keep images and full logs in artifacts;
return only paths and compact summaries, never base64 payloads, in chat.