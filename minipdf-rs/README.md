# MiniPdf for Rust

[![crates.io](https://img.shields.io/crates/v/minipdf.svg)](https://crates.io/crates/minipdf)
[![docs.rs](https://img.shields.io/docsrs/minipdf)](https://docs.rs/minipdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](../LICENSE)

The experimental native Rust implementation of MiniPdf. It provides the
reusable `minipdf` crate and the `minipdf-cli` package, whose installed binary is
named `minipdf`.

[Project portal](../README.md) ·
[crates.io](https://crates.io/crates/minipdf) ·
[API documentation](https://docs.rs/minipdf) ·
[.NET implementation](../documents/README.nuget.md)

> The Rust implementation is under active development and is not yet
> feature-equivalent with MiniPdf for .NET. It currently converts XLSX and DOCX
> files. Use the .NET implementation for PPTX, content extraction, PDF merge,
> or broader production compatibility.

## Requirements

- Rust 1.82 or later
- No Microsoft Office, LibreOffice, Adobe Acrobat, or .NET runtime at runtime

## Library

Add the crate:

```bash
cargo add minipdf
```

Convert a file to a PDF on disk:

```rust
fn main() -> minipdf::Result<()> {
    minipdf::convert_to_pdf("report.docx", "report.pdf")?;
    Ok(())
}
```

Return PDF bytes instead:

```rust
let pdf = minipdf::convert_to_pdf_bytes("data.xlsx")?;
```

For a host with limited system fonts, register font bytes before conversion:

```rust
let font = std::fs::read("fonts/NotoSansSC-Regular.ttf")?;
minipdf::register_font("NotoSansSC", font);
minipdf::convert_to_pdf("report.docx", "report.pdf")?;
```

The public API also supports in-memory Office package bytes through
`convert_bytes_to_pdf` and package inspection through `detect_office_format`.

## Command Line

Install the CLI from crates.io:

```bash
cargo install minipdf-cli
```

Convert with an automatically selected output name:

```bash
minipdf report.docx
```

Specify an output path or font directory:

```bash
minipdf data.xlsx -o data.pdf
minipdf report.docx --fonts ./fonts
minipdf convert report.docx -o report.pdf
```

## Current Scope

| Capability | Rust status |
|---|---|
| XLSX to PDF | Supported; rendering coverage is expanding |
| DOCX to PDF | Supported; rendering coverage is expanding |
| PPTX to PDF | Not supported |
| PDF merge | Not supported |
| Markdown / JSON extraction | Not supported |
| PDF output | PDF 1.4 |
| Fonts | Built-in Helvetica and registered/system fallback fonts |
| Interfaces | Rust crate and native CLI |

The converter preserves sparse worksheet row positions, paginates wide sheets
horizontally, and supports basic spreadsheet styling, drawing images, document
text, and Unicode fallback fonts. Complex Office layout can still differ from
the source application.

## Development

The Rust implementation is an independent Cargo workspace:

```text
minipdf-rs/
|-- Cargo.toml
`-- crates/
    |-- minipdf/       # Library API and conversion engine
    `-- minipdf-cli/   # CLI binary named minipdf
```

Run the standard checks from this directory:

```powershell
cargo fmt --check
cargo clippy --workspace --all-targets -- -D warnings
cargo test --workspace
cargo run -p minipdf-cli -- path\to\input.docx
```

## Visual Benchmarks

Run shared fixtures against LibreOffice from the repository root:

```powershell
.\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format xlsx
.\scripts\Run-Rust-Benchmark.ps1 -Suite classic -Format docx
.\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format xlsx
.\scripts\Run-Rust-Benchmark.ps1 -Suite issue -Format docx
.\scripts\Run-Rust-Benchmark-Matrix.ps1 -MaxComparePages 1
```

The benchmark reuses the repository fixtures, references, and PDF comparison
pipeline without executing the C# xUnit tests. Each run writes coverage data,
Markdown and JSON reports, side-by-side images, and heatmaps under
`artifacts/rust-benchmark/<suite>/<format>`.

The matrix is generated at `artifacts/rust-benchmark/benchmark_matrix.md` and
links to the fixture coverage and comparison reports from each run.

## Publishing

Both packages are published to crates.io. Repository releases tagged
`rust-v<version>` publish `minipdf` first and then `minipdf-cli` through GitHub
Actions.

## License

[Apache License 2.0](../LICENSE).