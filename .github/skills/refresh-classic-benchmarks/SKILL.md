---
name: refresh-classic-benchmarks
description: "Refresh every MiniPdf XLSX classic benchmark artifact with current code and fresh LibreOffice references. Use when: updating all classic examples, regenerating benchmark PDFs/PNGs/heatmaps/reports, fixing stale GitHub README benchmark images, or running /refresh-classic-benchmarks."
argument-hint: "Optionally specify a filter or whether README benchmark scores should be synchronized"
user-invocable: true
disable-model-invocation: false
---

# Refresh Classic XLSX Benchmarks

Regenerate the canonical XLSX `classic` benchmark outputs, replace stale reference PDFs, validate the generated report, and synchronize README scores and translated documentation.

## Scope

Canonical paths:

- Sources: `tests/MiniPdf.Scripts/output/classic*.xlsx`
- MiniPdf PDFs: `tests/MiniPdf.Scripts/pdf_output/`
- LibreOffice PDFs: `tests/MiniPdf.Benchmark/reference_pdfs/`
- Report: `tests/MiniPdf.Benchmark/reports/`
- README gallery: `README.md` and translated files under `documents/`

Use `artifacts/` only for isolated investigation. GitHub README images must come from the canonical tracked report directory.

## Preconditions

1. Work from the repository root.
2. Inspect `git status --short` before running. Preserve unrelated user changes.
3. Confirm LibreOffice is available.
4. Confirm `scripts/Run-Benchmark.ps1` supports `-ForceReference`. Without it, existing reference PDFs may be silently reused.
5. Do not commit or push unless the user explicitly requests it. This repository cannot auto-push.

## Full Refresh

Run:

```powershell
.\scripts\Run-Benchmark.ps1 `
  -SkipInstall `
  -SkipGenerate `
  -Engine libre `
  -Filter "classic" `
  -ForceReference `
  -Heatmaps
```

Why each option matters:

- `-SkipGenerate`: preserve the exact checked benchmark corpus unless regeneration was explicitly requested.
- `-Engine libre`: keep the canonical report aligned with its LibreOffice reference label.
- `-Filter "classic"`: process all classic XLSX examples.
- `-ForceReference`: overwrite stale reference PDFs instead of skipping existing files.
- `-Heatmaps`: refresh the canonical visual-difference images.

If the user explicitly asks to regenerate the XLSX corpus too, omit `-SkipGenerate`.

## Required Checks

After the command completes:

1. Confirm MiniPdf and LibreOffice processed the same number of `classic*.xlsx` files.
2. Confirm the reference step reports zero skipped existing PDFs.
3. Parse `tests/MiniPdf.Benchmark/reports/comparison_report.json` as JSON.
4. Confirm every expected classic case appears exactly once and both PDFs exist.
5. Confirm report images are newer than their corresponding PDFs.
6. Review failed conversions, page-count changes, visual-score regressions, and text-score regressions.
7. Run:

```powershell
dotnet test tests/MiniPdf.Tests
git diff --check
git status --short
```

Do not describe the refresh as complete if the benchmark process is still running or if conversions failed.

## README Synchronization

The benchmark report generator does not automatically update README gallery percentages.

When canonical scores or gallery images change:

1. Read the generated `comparison_report.json` values.
2. Update matching percentages in `README.md` using rounded overall scores as displayed by the gallery.
3. Invoke the `sync-readme-translations` skill to copy the same benchmark numbers into:
   - `documents/README.zh-CN.md`
   - `documents/README.zh-TW.md`
   - `documents/README.ja.md`
   - `documents/README.ko.md`
   - `documents/README.it.md`
   - `documents/README.fr.md`
4. Keep image paths unchanged unless the report layout itself changed.
5. Verify old percentages no longer remain for updated cases.

## Regression Review

Compare the new report against `HEAD` before accepting bulk binary changes:

- Identify the largest visual-score improvements and regressions.
- Treat a visual-score decrease greater than `0.002` as requiring inspection.
- Inspect changed page counts and text similarity separately from visual scores.
- Open representative MiniPdf, reference, and heatmap PNGs for any suspicious case.
- Remember that an old canonical reference may itself be stale; prefer a fresh forced LibreOffice result when diagnosing.

## GitHub Publication

GitHub will not show refreshed images until canonical files are committed and pushed. Files under `artifacts/` are ignored and never update the README.

Before handing off, report:

- Number of cases converted and compared
- Failures or skipped references
- Average text, visual, and overall scores
- Largest improvements and regressions
- Changed tracked files
- Whether changes remain uncommitted

Never run `git push` automatically.
