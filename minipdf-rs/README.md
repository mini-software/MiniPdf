# MiniPdf Rust

This directory contains the experimental Rust implementation of MiniPdf. It is an independent Rust workspace with a reusable `minipdf` library crate and a `minipdf` CLI binary.

The first implementation focuses on proving the end-to-end shape: detect `.xlsx` and `.docx` ZIP packages, extract basic workbook/document text, render simple PDF pages, and keep the CLI close to the existing .NET tool.

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

## Current Scope

- Supports `.xlsx` and `.docx` package detection.
- Extracts shared strings and basic worksheet cell values from `.xlsx`.
- Extracts paragraph text from `.docx`.
- Writes valid PDF 1.4 files using built-in Helvetica fonts.
- Supports the existing CLI shape: shorthand input, `convert`, `-o/--output`, and `--fonts`.

## Known Gaps

This is not yet feature-equivalent with the .NET implementation. The next quality milestones are Unicode font embedding, XLSX styles/merged cells/page setup, DOCX table/style/layout support, images, and benchmark integration against the existing LibreOffice comparison pipeline.