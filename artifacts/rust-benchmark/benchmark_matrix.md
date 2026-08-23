# Rust Shared Visual Benchmark Matrix

> This matrix reuses the repository's on-disk fixtures and visual comparison pipeline. It does not execute C# xUnit tests or assertions.

| Suite | Format | Report | Selected | Converted | Compared | Missing refs | First pages | Average score | Complete |
|---|---:|---|---:|---:|---:|---:|---:|---:|---:|
| classic | xlsx | [View comparison](classic/xlsx/report/comparison_report.md) | 191 | 191 | 191 | 0 | 1 | 0.8922 | True |
| classic | docx | Local artifact | 180 | 180 | 180 | 0 | 1 | 0.8449 | True |
| issue | xlsx | [View comparison](issue/xlsx/report/comparison_report.md) | 22 | 22 | 22 | 0 | 1 | 0.5304 | True |
| issue | docx | Local artifact | 27 | 27 | 27 | 0 | 1 | 0.6121 | True |

Total shared fixtures: **420**

Rust-supported formats: XLSX, DOCX. PPTX is not supported and is not counted.
