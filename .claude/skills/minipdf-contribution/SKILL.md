---
name: minipdf-contribution
description: "Guide a first-time MiniPdf contributor through choosing an implementation language, selecting a failing visual benchmark case, making a focused rendering fix, and validating the complete benchmark. Use when contributing rendering improvements to MiniPdf in .NET, Rust, Java, Go, Python, or Node.js."
compatibility: "Designed for Claude Code, Codex, and GitHub Copilot. Requires Git, PowerShell, the selected language toolchain, Python benchmark dependencies, an installed LibreOffice executable, and a LibreOffice core source checkout."
---

# MiniPdf Contribution

Complete one evidence-based rendering improvement from diagnosis through pull request preparation. Read `CONTRIBUTING.md` before changing code. Preserve unrelated work and do not commit, push, fork, or create a pull request without explicit user approval.

Do not use emoji in responses, newly authored documentation, commit messages, or pull request content.

## 1. Prepare LibreOffice and Its Source

For a first-time contributor, both of these prerequisites are mandatory. Check them before selecting work:

1. Confirm that a LibreOffice executable is available. Check `soffice` on `PATH` and common installation locations for the current operating system. A source checkout does not replace an installed LibreOffice executable.
2. Look for an existing LibreOffice core source checkout in a nearby development directory, including a sibling such as `../libreoffice-core`.

If the source checkout is missing, tell the user it is required for renderer-behavior research and ask them to approve or choose the clone destination. Suggest a sibling directory such as `../libreoffice-core`. After the user confirms the location, clone `https://github.com/LibreOffice/core.git` there. Do not continue the contribution workflow without this checkout, and do not build or modify LibreOffice unless the user requests it.

If the LibreOffice executable is missing, explain that cloning the source does not install LibreOffice and ask the user how they want to install or provide the executable. Do not run visual benchmarks until both the executable and source checkout are available.

## 2. Choose an Implementation

Ask which implementation language the user wants to improve and wait for an explicit answer:

| Choice | Repository path | Benchmark command |
|---|---|---|
| .NET | `src/` | `scripts/Run-DotNet-VisualBenchmark.ps1` |
| Rust | `minipdf-rs/` | `scripts/Run-Rust-VisualBenchmark.ps1` |
| Java | `minipdf-java/` | `scripts/Run-Java-VisualBenchmark.ps1` |
| Go | `minipdf-go/` | `scripts/Run-Go-VisualBenchmark.ps1` |
| Python | `minipdf-python/` | `scripts/Run-Python-VisualBenchmark.ps1` |
| Node.js | `minipdf-node/` | `scripts/Run-Node-VisualBenchmark.ps1` |

Do not choose a language on the user's behalf.

## 3. Select a Failing Case

Find the available reports for the selected language under:

```text
artifacts/<language>-benchmark/<suite>/<format>/report/comparison_report.md
```

`<language>` is `dotnet`, `rust`, `java`, `go`, `python`, or `node`; `<suite>` is `classic` or `issue`; and `<format>` is `xlsx`, `docx`, or `pptx`. For example:

```text
artifacts/java-benchmark/issue/xlsx/report/comparison_report.md
```

Inspect the report data and select one valid case whose overall score is below `0.95`. Prefer the lowest reproducible score across the available reports for that language. Exclude missing or invalid candidate/reference PDFs. If no current report exists for a suite and format, generate it with the corresponding full benchmark command before selecting a case.

Record the selected language, suite, format, case name, overall score, visual average, text similarity, page counts, and relevant comparison image paths. Preserve a copy of the complete baseline report data under `artifacts/` for the final regression comparison.

## 4. Diagnose and Improve

1. Open the candidate, Microsoft 365 reference, LibreOffice auxiliary reference, side-by-side images, and heatmaps for the weakest pages.
2. Classify the mismatch before editing: page setup, scaling, font metrics, text layout, borders, fills, drawings, headers or footers, or pagination.
3. Use the local LibreOffice core source checkout to research the relevant rendering behavior and expected semantics before editing. Record the relevant source files and symbols in the contribution evidence.
4. Locate the selected implementation code that directly controls the mismatch.
5. State one falsifiable root-cause hypothesis and one focused check that can disprove it.
6. Add or update a focused automated test, make the smallest root-cause fix, and run that test.
7. Re-run the selected case with `-Filter "<case-name>"`. Keep the change only if the intended visual result improves without a known regression.

Do not optimize only for the similarity formula. The output must remain a valid PDF and match the document semantics.

## 5. Run the Complete Visual Benchmark

After the focused case improves, run the same language, suite, and format again without `-Filter` and without `-MaxCases`. This final run must regenerate every candidate PDF and all report images for that benchmark scope.

Compare the complete final report with the preserved baseline. Reject or revise the change if any unrelated case has an unexpected page-count change, an invalid or missing PDF, missing comparison images, a `visual_avg` decrease greater than `0.002`, or an obvious broad visual regression. Review the final average and the distribution of score changes; a gain in one case must not hide widespread losses.

Run the selected implementation's relevant unit tests and `git diff --check` after the full benchmark. Report the exact commands, before-and-after scores, page counts, image paths, and any remaining risk.

## Cross-Language Visual Benchmarks

Use the following form for .NET, Rust, Java, Go, Python, and Node:

```powershell
scripts/Run-<Language>-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
```

Supported commands:

```powershell
scripts/Run-DotNet-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
scripts/Run-Rust-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
scripts/Run-Java-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
scripts/Run-Go-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
scripts/Run-Python-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
scripts/Run-Node-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx> -MaxCases <n>
```

During diagnosis, add `-Filter "<case-name>"` for a focused run. For final validation, omit both `-Filter` and `-MaxCases`:

```powershell
scripts/Run-<Language>-VisualBenchmark.ps1 -Suite <classic|issue> -Format <xlsx|docx|pptx>
```

Use only suite and format combinations supported by the benchmark runner. Keep generated PDFs, images, heatmaps, and full logs under `artifacts/`; summarize results in chat instead of embedding image data or complete reports.