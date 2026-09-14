# Cross-Language Feature Parity

Last verified: 2026-09-14

This document tracks implementation gaps between MiniPdf for .NET, Rust,
Java, Go, Python, and Node.js. It is an engineering backlog, not a claim of
complete Microsoft Office compatibility.

.NET is the current functional reference because it has the broadest renderer
and compatibility coverage. It is not automatically the correct behavior: a
shared security or conformance gap can exist in every implementation.

## Status Legend

| Mark | Meaning |
|---|---|
| `I` | Implemented with source and test or benchmark evidence |
| `P` | Partially implemented or materially narrower than the .NET reference |
| `M` | Missing or currently ineffective |
| `B` | Node.js behavior inherited from the Rust engine |
| `?` | Evidence is insufficient; verify before changing the status |

## Summary Matrix

| Capability | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| XLSX to PDF | I | P | P | P | P | B |
| DOCX to PDF | I | P | P | P | P | B |
| PPTX to PDF | P | P | P | P | P | B |
| Path input and file output | I | I | I | I | I | B |
| In-memory input and PDF bytes | I | I | I | I | I | B |
| Stream input and output | I | M | M | I | M | M |
| Format detection API | Internal | I | I | I | I | B |
| Page-size override | DOCX only | I | I | I | I | B |
| Margin override | I | M | M | I | M | M |
| XLSX conversion controls | I | M | M | P | M | M |
| PDF compression option | I | M | M | I | M | M |
| Missing-font diagnostics | I | M | M | M | M | M |
| Effective custom font embedding | I | I | I | I | M | B |
| Native CLI | I | I | I | I | I | M |
| Tracked visual benchmark evidence | I | P | P | M | M | M |
| Bounded OOXML package loading | M | M | I | I | I | B/M |

Notes:

- Node.js is a native binding over the Rust engine. Rendering fixes normally
  belong in `minipdf-rs`; Node-specific work should focus on API exposure,
  asynchronous execution, packaging, and binding tests.
- `P` does not mean the same depth in every language. The detailed matrices
  below identify the important differences.
- The public .NET page-size and margin overrides currently apply to DOCX. The
  other implementations expose a common page-size override for all formats.

## Public API and CLI Gaps

The .NET reference exposes stream conversion, diagnostics, PDF compression,
DOCX margins, sheet selection, XLSX limits, orientation, fit-to-page controls,
print scale, rows-per-page, and culture-aware value formatting in
[`MiniPdf.cs`](src/MiniPdf/MiniPdf.cs).

| Gap relative to .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|
| Input/output stream API | M | M | I | M | M |
| Conversion diagnostics | M | M | M | M | M |
| PDF stream compression option | M | M | I | M | M |
| DOCX margin override | M | M | I | M | M |
| XLSX sheet selection | M | M | M | M | M |
| XLSX row/column limits | M | M | I | M | M |
| XLSX orientation/fit/scale controls | M | M | P | M | M |
| Culture-aware XLSX formatting | M | M | M | M | M |
| Font clear API | M | I | I | M | M |
| Registered-font list API | I | I | I | I | I |
| CLI font directory | I | I | I | M | M |
| CLI advanced XLSX options | M | M | P | M | M |
| Native CLI distribution | I | I | I | I | M |
| Non-blocking/async conversion API | M | M | M | M | M |

Evidence:

- Rust currently exposes only `page_size` in
  [`ConversionOptions`](minipdf-rs/crates/minipdf/src/lib.rs).
- Java currently exposes only `pageSize` in
  [`ConversionOptions`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/ConversionOptions.java).
- Go currently exposes page size, DOCX margins, PDF compression, XLSX row and
  column limits, and XLSX orientation in
  [`ConversionOptions`](minipdf-go/minipdf.go).
- Python currently exposes only `page_size` in
  [`ConversionOptions`](minipdf-python/src/minipdf/options.py).
- Node.js exposes synchronous Rust conversion bindings in
  [`lib.rs`](minipdf-node/src/lib.rs); callers must use a worker thread to avoid
  blocking the Node.js event loop.

Go also provides bounded `io.Reader` input, `io.Writer` output, and complete
register/list/clear font lifecycle APIs. Conversion still builds the PDF bytes
in memory before writing them.

Before adding every .NET option to every language, define a versioned common
options contract. Format-specific options should be capability-gated and
should fail clearly when used with the wrong input format.

## DOCX Rendering Gaps

| Feature | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| Paragraphs, runs, tabs, line breaks | I | I | P | P | I | B |
| Bold, italic, size, color, underline | I | P | M | M | P | B |
| Paragraph spacing and alignment | I | P | M | M | P | B |
| Explicit page breaks | I | I | P | I | I | B |
| Section page size and margins | I | I | P | I | I | B |
| Tables and cell borders | I | P | M | M | M | B |
| Merged table cells | I | P | M | M | M | B |
| Lists and numbering | I | P | M | M | M | B |
| Headers and footers | I | M | M | M | M | B/M |
| Footnotes/endnotes | I | M | M | M | M | B/M |
| Columns | I | M | M | M | M | B/M |
| Images and VML drawings | I | P | M | M | M | B |
| Floating/anchored objects | P | P | M | M | M | B |
| TOC and field-result layout | P | M | M | M | M | B/M |
| CJK and RTL shaping/fallback | I | P | M | M | M | B |

Primary implementation evidence:

- .NET: [`DocxReader.cs`](src/MiniPdf/DocxReader.cs) and
  [`DocxToPdfConverter.cs`](src/MiniPdf/DocxToPdfConverter.cs)
- Rust: [`docx.rs`](minipdf-rs/crates/minipdf/src/docx.rs)
- Java: [`DocxConverter.java`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/internal/docx/DocxConverter.java)
- Go: [`docx.go`](minipdf-go/docx.go)
- Python: [`docx.py`](minipdf-python/src/minipdf/docx.py)

## XLSX Rendering Gaps

| Feature | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| Shared/inline strings and scalar values | I | I | I | I | I | B |
| Number/date format rendering | I | P | P | M | M | B |
| Formula cached values | I | P | P | M | M | B |
| Fonts, fills, borders, alignment | I | P | P | M | M | B |
| Row heights and column widths | I | I | P | P | M | B |
| Merged cells | I | I | I | M | M | B |
| Hidden rows, columns, and sheets | I | P | P | M | M | B |
| Print area and print titles | I | P | P | M | M | B |
| Paper size, margins, orientation | I | P | P | P | M | B |
| Fit-to-page and print scale | I | P | P | M | M | B |
| Conditional and table styles | P | P | P | M | M | B |
| Raster images | I | P | P | M | M | B |
| VML/vector drawings | I | P | P | M | M | B |
| Charts | P | M | M | M | M | B/M |

Primary implementation evidence:

- .NET: [`ExcelReader.cs`](src/MiniPdf/ExcelReader.cs) and
  [`ExcelToPdfConverter.cs`](src/MiniPdf/ExcelToPdfConverter.cs)
- Rust: [`xlsx.rs`](minipdf-rs/crates/minipdf/src/xlsx.rs)
- Java: [`PoiXlsxRenderer.java`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/internal/xlsx/PoiXlsxRenderer.java)
- Go: [`xlsx.go`](minipdf-go/xlsx.go)
- Python: [`xlsx.py`](minipdf-python/src/minipdf/xlsx.py)

Java XLSX support is substantially broader than Java DOCX/PPTX support because
it uses Apache POI and PDFBox. It should not be described as uniformly
"text-only" across every Office format.

## PPTX Rendering Gaps

PPTX support remains partial in every implementation, including .NET.

| Feature | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| Slide dimensions and one page per slide | I | I | I | I | I | B |
| Theme and placeholder text | I | P | M | M | M | B |
| Text style and paragraph layout | P | P | M | M | M | B |
| Basic shapes and connectors | P | P | M | M | M | B |
| Shape transforms and geometry | P | P | M | M | M | B |
| Raster images | I | P | M | M | M | B |
| SVG images | P | P | M | M | M | B |
| Tables | P | P | M | M | M | B |
| SmartArt fallback | P | P | M | M | M | B |
| Charts | M | M | M | M | M | M |
| Animations, transitions, media | M | M | M | M | M | M |

Primary implementation evidence:

- .NET: [`PptxReader.cs`](src/MiniPdf/PptxReader.cs) and
  [`PptxToPdfConverter.cs`](src/MiniPdf/PptxToPdfConverter.cs)
- Rust: [`pptx.rs`](minipdf-rs/crates/minipdf/src/pptx.rs)
- Java: [`PptxConverter.java`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/internal/pptx/PptxConverter.java)
- Go: [`pptx.go`](minipdf-go/pptx.go)
- Python: [`pptx.py`](minipdf-python/src/minipdf/pptx.py)

## Font, Image, and PDF Writer Gaps

| Area | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| Registered and system font fallback | I | I | P | P | M | B |
| Font embedding and subsetting | I | P | P | P | M | B |
| ToUnicode/CID output | I | P | P | I | M | B |
| JPEG/PNG with transparency | I | P | P for XLSX | M | M | B |
| SVG rendering | P | P for PPTX | M | M | M | B |
| EMF/WMF handling | P | P | P for XLSX | M | M | B |
| Public low-level PDF construction API | M | P | P | P | M | M |

The low-level PDF API is inconsistent: Rust, Java, and Go expose writer types,
while .NET keeps comparable types internal and Node.js does not bind Rust's
writer. Decide whether PDF construction is a supported product API before
attempting parity. If it is not, make implementation-specific writer types
internal in the next breaking release.

## OOXML Security Gaps

Java, Go, and Python now enforce explicit package-loading limits.
Java rejects excessive entry counts, oversized or highly compressed entries,
unsafe paths, duplicate entries, and XML external entities in
[`OoxmlPackage.java`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/internal/OoxmlPackage.java)
and
[`SecureXml.java`](minipdf-java/minipdf/src/main/java/io/github/minisoftware/minipdf/internal/SecureXml.java).
Python applies package limits and path checks in
[`office.py`](minipdf-python/src/minipdf/office.py).
Go applies entry count, entry and total size, expansion ratio, encryption,
duplicate path, and unsafe path checks in
[`office.go`](minipdf-go/office.go), exposed through `ErrInvalidPackage`.

.NET, Rust, and therefore Node.js still need the same bounded OOXML package
contract. Cross-language conformance fixtures must cover:

- maximum entry count, per-entry size, and total uncompressed size;
- maximum compression ratio and encrypted-entry rejection;
- duplicate normalized path rejection;
- absolute path, drive prefix, and `..` traversal rejection;
- external relationship and XML DTD/entity blocking;
- consistent error categories and malformed-package tests.

Security behavior should converge before rendering APIs are expanded.

## Test and Benchmark Evidence Gaps

All six implementations have language-specific visual benchmark runners under
[`scripts`](scripts), but tracked result coverage is incomplete.

| Evidence | .NET | Rust | Java | Go | Python | Node.js |
|---|---:|---:|---:|---:|---:|---:|
| Unit tests for public API | I | I | I | P | P | P |
| Malformed/security fixtures | P | P | I | P | I | B/P |
| Classic XLSX report | I | I | M | M | M | M |
| Classic DOCX report | I | I | M | M | M | M |
| Issue XLSX report | I | I | I | M | M | M |
| Issue DOCX report | I | M | M | M | M | M |
| Issue PPTX report | I | M | M | M | M | M |
| Classic PPTX corpus/report | M | M | M | M | M | M |

A feature should not move from `P` to `I` based only on a parser or API being
present. Require a focused unit test plus a reproducible visual benchmark case.

### Go Validation Progress

- 2026-09-14: a five-case classic XLSX baseline scored `0.8093` and reported a
  one-byte page content-stream length error in every candidate PDF. After the
  writer fix, the same five cases retained `0.8093` with no PDF structure
  errors. See the local
  [`before`](artifacts/go-parity-baseline/report/comparison_report.md) and
  [`after`](artifacts/go-parity-stream-fixed/report/comparison_report.md)
  reports.
- 2026-09-14: removing synthetic `Sheet N` text while preserving worksheet
  vertical spacing raised the same five-case average from `0.8093` to `0.9365`.
  The empty-workbook case improved from `0.5995` to `1.0`, and no case
  regressed. See the local
  [`report`](artifacts/go-parity-sheet-spacing/report/comparison_report.md).
- 2026-09-14: nine-column horizontal pagination raised the wide-table case from
  `0.6867` with `1/3` pages to `0.9953` with `3/3` pages. The five-case average
  reached `0.9975`; an apparent `classic02` text-score fluctuation was confirmed
  unrelated because its before/after candidate SHA-256 hashes were identical.
  See the local
  [`report`](artifacts/go-parity-xlsx-pagination-final/report/comparison_report.md).
- 2026-09-14: parsing native DOCX `w:pgMar` values and adding validated margin
  overrides raised the five-case classic DOCX average from `0.9872` to `0.9889`;
  all cases improved or held. See the local
  [`report`](artifacts/go-parity-docx-margins/report/comparison_report.md).
- 2026-09-14: horizontally paginating overflowing single-column XLSX text
  raised `classic09_long_text` from `0.1840` with `1/12` pages to `0.8942`
  with `12/12` pages. The complete 191-case classic XLSX average increased
  from `0.8436` to `0.8473`, with all unrelated case scores unchanged. See the
  local [`report`](artifacts/go-benchmark/classic/xlsx/report/comparison_report.md).
- 2026-09-14: preserving explicit XLSX row and cell references raised
  `classic11_sparse_rows` from `0.6990` to `0.9996` and
  `classic12_sparse_columns` from `0.6993` to `0.9995`, with both candidates
  reaching `2/2` reference pages. The complete 191-case average increased from
  `0.8473` to `0.8503`, with no case regression.
- 2026-09-14: skipping worksheets with no physical rows while retaining a
  fallback page for an entirely empty workbook raised
  `classic30_mixed_empty_and_filled_sheets` from `0.6834` with `4/2` pages to
  `0.9993` with `2/2` pages. The complete average reached `0.8519`; SHA-256
  comparison against the preceding stage showed this was the only changed
  candidate among all 191 cases.
- 2026-09-14: mapping OOXML boolean cells from `1` and `0` to Excel-compatible
  `TRUE` and `FALSE` raised `classic42_boolean_values` from `0.9264` to
  `0.9869`. The complete average reached `0.8522`; SHA-256 comparison against
  the preceding stage showed this was the only changed candidate among all 191
  cases.

## Alignment Backlog

### P0: Correctness and Security

- [ ] Define one bounded OOXML package-loading contract and shared malicious
  fixtures.
- [ ] Implement the contract in .NET and Rust; Node.js inherits Rust.
- [x] Implement bounded OOXML package loading and `ErrInvalidPackage` in Go.
- [ ] Verify Java and Python against the same fixtures and align error behavior.
- [ ] Fix status documentation drift: [`ROADMAP.md`](ROADMAP.md) still says Rust
  PPTX is unsupported and omits Java, Go, Python, and Node.js.
- [ ] Fix [`README.md`](README.md), which lists Python input as DOCX only even
  though the Python dispatcher supports XLSX and PPTX.
- [ ] Align the Python package and runtime version declarations in
  [`pyproject.toml`](minipdf-python/pyproject.toml) and
  [`__init__.py`](minipdf-python/src/minipdf/__init__.py).

### P1: Rendering Conformance

- [ ] Publish complete current benchmark reports for every language across
  classic XLSX/DOCX and issue XLSX/DOCX/PPTX.
- [ ] Add a shared classic PPTX corpus and report path.
- [ ] Rust: close advanced DOCX gaps, especially headers/footers, lists,
  columns, notes, and floating objects.
- [ ] Rust: add XLSX chart rendering or explicitly declare charts out of scope.
- [ ] Java: add DOCX tables/styles/images, then PPTX styles/shapes/images.
- [x] Go: add effective registered TTF embedding with Type0/CID and ToUnicode.
- [ ] Go: add TTF subsetting, TTC support, system fallback, and complex-script
  shaping.
- [ ] Go: add XLSX styles/merges/images, DOCX tables/images, and PPTX
  shapes/images.
- [ ] Python: add effective font embedding and Unicode shaping before complex
  layout.
- [ ] Python: add DOCX tables/images, XLSX styles/merges/images, and PPTX
  shapes/images.
- [ ] .NET and Rust: define and test the intended PPTX compatibility boundary,
  including explicit non-goals for animation and media.

### P2: API and Distribution

- [ ] Define a versioned common conversion-options contract and shared option
  validation fixtures.
- [ ] Standardize format detection and error categories.
- [ ] Standardize register/list/clear font lifecycle APIs.
- [ ] Decide whether stream conversion is required in each language or whether
  bytes/file APIs are the portable contract.
- [ ] Decide whether low-level PDF writer types are public supported APIs.
- [ ] Add a non-blocking Node.js conversion API or an official worker-thread
  helper.
- [ ] Decide whether Node.js requires a CLI; do not duplicate the Rust CLI
  without a Node-specific distribution need.
- [ ] Add release gates that run unit tests and the relevant focused visual
  benchmark before publishing each package.

## Definition of Aligned

A capability is aligned only when all of the following are true:

1. The public behavior and unsupported cases are documented.
2. Shared valid, malformed, and boundary fixtures pass in every applicable
   implementation.
3. Each implementation has a focused unit test for its native API.
4. Visual output is checked against the same Microsoft 365 reference and
   LibreOffice auxiliary reference.
5. Page count, text similarity, visual score, and known deviations are recorded.
6. The package release workflow executes the required tests.

When updating this matrix, link the source, test, benchmark report, and issue or
pull request that justify each status change.
