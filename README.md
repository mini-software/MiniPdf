<div align="center">

# MiniPdf

**Lightweight Office-to-PDF libraries and command-line tools for .NET and Rust.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

English | <a href="documents/README.zh-CN.md">简体中文</a> | <a href="documents/README.zh-TW.md">繁體中文</a> | <a href="documents/README.ja.md">日本語</a> | <a href="documents/README.ko.md">한국어</a> | <a href="documents/README.it.md">Italiano</a> | <a href="documents/README.fr.md">Français</a>

**[Online Demo](https://mini-software.github.io/MiniPdf/)** · **[Releases](https://github.com/mini-software/MiniPdf/releases)** · **[Report an issue](https://github.com/mini-software/MiniPdf/issues)**

Your star or donation helps sustain the project.

</div>

MiniPdf converts Office documents directly to PDF without Microsoft Office,
LibreOffice, Adobe Acrobat, or COM automation at runtime. Choose the
implementation that matches your project.

## Choose an implementation

| | .NET | Rust |
|---|---|---|
| Inputs | XLSX, DOCX, PPTX | XLSX, DOCX |
| Interfaces | .NET library, CLI, standalone Native AOT binaries | Rust crate, CLI |
| Documentation | **[Open the .NET guide](documents/README.nuget.md)** | **[Open the Rust guide](minipdf-rs/README.md)** |

## Quick start

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

Prefer a command-line tool?

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

The [.NET guide](documents/README.nuget.md) covers conversion, custom fonts,
CLI options, and deployment.

### Rust

```bash
cargo add minipdf
cargo install minipdf-cli
```

```rust
minipdf::convert_to_pdf("report.docx", "report.pdf")?;
```

```bash
minipdf report.docx -o report.pdf
```

The [Rust guide](minipdf-rs/README.md) documents the crate API, CLI, supported
features, known gaps, and development workflow.

## Why MiniPdf

- **No office suite required**: conversion runs inside your application or CLI.
- **Small deployment surface**: minimal dependencies and no external process.
- **Server and CI friendly**: works in containers, cloud services, and pipelines.
- **Native command-line options**: .NET Native AOT releases and a Rust CLI.
- **Open development**: Apache 2.0 licensed with reproducible visual benchmarks.

MiniPdf targets practical document conversion, not complete Microsoft Office
layout compatibility. Complex templates may render differently; use the online
demo or benchmark reports to evaluate representative files.

## Contribute compute time

Open a clean fork or clone in GitHub Copilot, Claude Code, Cursor, Codex, or any
coding agent that can edit files and run PowerShell. The universal entry point is:

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

The contribution loop checks the toolchain, installs benchmark Python packages,
creates a local branch, and selects the chosen renderer's two lowest-scoring
visual differences. The agent gets up to three attempts per case; an attempt
that does not improve the score is automatically rolled back before moving on.
Accepted changes must pass that implementation's full test suite and XLSX/DOCX
visual benchmarks without a meaningful regression before a pull request is allowed.

Both paths require Git, Python 3.10+, and LibreOffice. Choose .NET with the .NET
9 SDK, or Rust with Cargo plus desktop Excel and Word for its current primary
visual references. The workflow checks whether authenticated GitHub CLI is
available; if it is not, it generates the PR title, body, push command, and browser URL instead. See the
[contribution guide](CONTRIBUTING.md) for Copilot, Claude Code, Cursor, Codex,
terminal shortcuts, and safety rules. The workflow never commits, pushes, or
opens a PR without your approval.

## Project resources

| Resource | Description |
|---|---|
| [Online demo](https://mini-software.github.io/MiniPdf/) | Try conversion in a browser |
| [.NET documentation](documents/README.nuget.md) | Stable library and CLI usage |
| [Rust documentation](minipdf-rs/README.md) | Experimental crate and CLI usage |
| [.NET XLSX benchmark](tests/MiniPdf.Benchmark/reports/comparison_report.md) | Visual comparison results for spreadsheets |
| [.NET DOCX benchmark](tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | Visual comparison results for documents |
| [Rust XLSX benchmark](artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust spreadsheet visual comparison results |
| [Rust DOCX benchmark](artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust document visual comparison results |
| [Rust benchmark workflow](scripts/Run-Rust-Benchmark.ps1) | Generate fixture coverage and comparison reports |
| [GitHub releases](https://github.com/mini-software/MiniPdf/releases) | Packages and standalone binaries |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bugs, compatibility reports, and feature requests |

## License

[Apache License 2.0](LICENSE). Commercial use is welcome; retain the required
notices and attribution.

Your [star](https://github.com/mini-software/MiniPdf) or [donation](https://mini-software.github.io/) helps sustain the project.
