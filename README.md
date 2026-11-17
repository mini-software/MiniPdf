<div align="center">

**Lightweight, fast and native Office-to-PDF library and command-line tools for .NET.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

English | <a href="documents/README.zh-CN.md">简体中文</a> | <a href="documents/README.zh-TW.md">繁體中文</a> | <a href="documents/README.ja.md">日本語</a> | <a href="documents/README.ko.md">한국어</a> | <a href="documents/README.it.md">Italiano</a> | <a href="documents/README.fr.md">Français</a>

**[Online Demo](https://mini-software.github.io/MiniPdf/)** · **[Releases](https://github.com/mini-software/MiniPdf/releases)** · **[Report an issue](https://github.com/mini-software/MiniPdf/issues)**

Your star or donation can make MiniPdf better

🤝 **Looking for co-developers:** [Quick contribution](#quick-contribution)

</div>

This repository now maintains **MiniPdf for .NET** only. Rust, Java, Go,
Python, and Node.js have moved to their own [PdfLibrarys repositories](https://github.com/PdfLibrarys).
The implementation links below point to those independent repositories.

MiniPdf converts Office documents directly to PDF without Microsoft Office,
LibreOffice, Adobe Acrobat, or COM automation at runtime. 

## Choose an implementation

| Implementation | Inputs | Interfaces | Maturity | Documentation | Visual results |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Library, CLI, Native AOT binaries | Stable | **[.NET guide](documents/README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Stable | **[Rust guide](https://github.com/PdfLibrarys/minipdf-rs/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx)** |
| Java | XLSX, DOCX, PPTX | Library, CLI | Experimental | **[Java source](https://github.com/PdfLibrarys/minipdf-java)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=docx)** |
| Python | DOCX | Package, CLI | Stable | **[Python guide](https://github.com/PdfLibrarys/minipdf-python/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=python&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=python&suite=issue&format=docx)** |
| Node.js | XLSX, DOCX, PPTX | Native package | Stable | **[Node.js guide](https://github.com/PdfLibrarys/minipdf-node/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=node&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=node&suite=issue&format=docx)** |
| Go | XLSX, DOCX, PPTX | Package, CLI | Experimental | **[Go guide](https://github.com/PdfLibrarys/minipdf-go/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=go&suite=issue&format=xlsx)** |

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

See the [.NET guide](documents/README.nuget.md) for usage.

## Why MiniPdf

- **No office suite required**: conversion runs inside your application or CLI.
- **Small deployment surface**: minimal dependencies and no external process.
- **Server and CI friendly**: works in containers, cloud services, and pipelines.
- **Open development**: Apache 2.0 licensed with reproducible visual benchmarks.

MiniPdf targets practical document conversion, not complete Microsoft Office
layout compatibility. Complex templates may render differently; use the online
demo or benchmark reports to evaluate representative files.

<a id="quick-contribution"></a>

## Quick contribution

Open a clean fork or clone in any coding agent, then paste this prompt into the
agent chat:

```text
Run the minipdf-contribution skill.
```

## Project resources

| Resource | Description |
|---|---|
| [Online demo](https://mini-software.github.io/MiniPdf/) | Try conversion in a browser |
| [.NET documentation](documents/README.nuget.md) | Stable library and CLI usage |
| [Rust documentation](https://github.com/PdfLibrarys/minipdf-rs/blob/main/README.md) | Experimental crate and CLI usage |
| [Java implementation](https://github.com/PdfLibrarys/minipdf-java) | Experimental Maven library and CLI source |
| [Python documentation](https://github.com/PdfLibrarys/minipdf-python/blob/main/README.md) | Experimental package and CLI usage |
| [Node.js documentation](https://github.com/PdfLibrarys/minipdf-node/blob/main/README.md) | Experimental native package usage |
| [Go documentation](https://github.com/PdfLibrarys/minipdf-go/blob/main/README.md) | Experimental package and CLI usage |
| [.NET XLSX benchmark](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | Visual comparison results for spreadsheets |
| [.NET DOCX benchmark](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | Visual comparison results for documents |
| [Community governance](GOVERNANCE.md) | Decisions, roles, voting, and maintainer selection |
| [Roadmap](ROADMAP.md) | Project scope, implementation status, and current priorities |
| [Security](SECURITY.md) | Private vulnerability reporting and supported versions |
| [Contributing](CONTRIBUTING.md) | Development setup, tests, reviews, and provenance requirements |
| [GitHub releases](https://github.com/mini-software/MiniPdf/releases) | Packages and standalone binaries |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bugs, compatibility reports, and feature requests |

## License

[Apache License 2.0](LICENSE). Commercial use is welcome
