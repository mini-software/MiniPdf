# MiniPdf Rust

This directory contains the experimental Rust implementation of MiniPdf. It is an independent Rust workspace with a reusable `minipdf` library crate and a `minipdf` CLI binary.

The first implementation focuses on proving the end-to-end shape: detect `.xlsx` and `.docx` ZIP packages, extract basic workbook/document text, render simple PDF pages, and keep the CLI close to the existing .NET tool.

## Install

```powershell
cargo add minipdf
cargo install minipdf-cli
```

Rust packages are published to crates.io, not NuGet. Repository releases tagged
`rust-v<version>` publish both packages through GitHub Actions.

## Layout

```text
minipdf-rs/
├── Cargo.toml
└── crates/
    ├── minipdf/       # Library API and conversion engine
    └── minipdf-cli/   # CLI binary named minipdf
```

## Commands

```powershell
cd minipdf-rs
cargo fmt --check
cargo clippy --workspace --all-targets -- -D warnings
cargo test --workspace
cargo run -p minipdf-cli -- convert path\to\input.xlsx -o output.pdf
cargo run -p minipdf-cli -- path\to\input.docx
```

Run the shared visual fixtures against LibreOffice from the repository root:

```powershell
.\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format xlsx
.\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format docx
.\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format xlsx
.\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format docx
.\scripts\Run-Rust-Benchmark-Matrix.ps1 -MaxComparePages 1
```

The benchmark reuses the same on-disk fixtures, references, and PDF comparison
pipeline as the .NET visual benchmarks. It does not execute the C# xUnit test
methods or their assertions. Each run writes a coverage manifest, Markdown/JSON
report, side-by-side images, and pixel-difference heatmaps under
`artifacts/rust-benchmark/<suite>/<format>` and fails on missing output or scores
below `-MinimumScore`.

Open the [Rust benchmark matrix](../artifacts/rust-benchmark/benchmark_matrix.md)
to browse the published XLSX comparison reports and images. Canonical XLSX
reports are version-controlled; candidate PDFs, logs, focused runs, and DOCX
reports remain local build artifacts.

## Current Scope

- Supports `.xlsx` and `.docx` package detection.
- Extracts shared strings and basic worksheet cell values from `.xlsx`.
- Extracts paragraph text from `.docx`.
- Writes valid PDF 1.4 files using built-in Helvetica plus subsetted Unicode fallback fonts.
- Renders basic unstyled XLSX sheets with spreadsheet-compatible page geometry,
  text overflow, and General-format numeric alignment.
- Preserves sparse worksheet row positions and paginates wide sheets horizontally
    instead of dropping columns beyond the first page.
- Supports the existing CLI shape: shorthand input, `convert`, `-o/--output`, and `--fonts`.

## Known Gaps

This is not yet feature-equivalent with the .NET implementation. The next quality milestones are XLSX styles/merged cells/page setup, DOCX table/style/layout support, images, and broader benchmark convergence against the existing LibreOffice comparison pipeline.