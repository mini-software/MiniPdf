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

MiniPdf provides a **.NET library, command-line tool, and standalone Native AOT binaries**
for converting XLSX, DOCX, and PPTX files to PDF.

MiniPdf converts Office documents directly to PDF without Microsoft Office,
LibreOffice, Adobe Acrobat, or COM automation at runtime. 

## .NET overview

| Implementation | Inputs | Interfaces | Maturity | Documentation | Visual results |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Library, CLI, Native AOT binaries | Stable | **[.NET guide](documents/README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |

## Quick start

### Install the library

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("workbook.xlsx", "workbook.pdf");
MiniPdf.ConvertToPdf("report.docx", "report.pdf");
MiniPdf.ConvertToPdf("slides.pptx", "slides.pdf");

byte[] pdfBytes = MiniPdf.ConvertToPdf("report.docx");
```

### Use the command line

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
minipdf workbook.xlsx -o workbook.pdf --compress
minipdf slides.pptx -o slides.pdf
minipdf --help
```

No .NET runtime installed on the destination machine? Download a standalone
Native AOT binary from [Releases](https://github.com/mini-software/MiniPdf/releases).

## Usage examples

### Convert streams

```csharp
using System.IO;
using MiniSoftware;

using var input = File.OpenRead("report.docx");
using var output = File.Create("stream-output.pdf");
MiniPdf.ConvertToPdf(input, output);
```

The caller owns the streams and disposes them. Use the overload returning
`byte[]` when an in-memory result is needed.

### Select Excel sheets and control layout

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("workbook.xlsx", "summary.pdf", new MiniPdfConversionOptions
{
    Sheets = new[] { "Summary" },
    Landscape = true,
    FitToPage = true,
    Compress = true,
});
```

Sheet names, 1-based `SheetIndexes`, orientation, print scale, and fit-to-page
options apply to XLSX input. See the [conversion options](documents/README.nuget.md#conversion-options)
for the complete list.

### Set Word page size and margins

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "a4-report.pdf", new MiniPdfConversionOptions
{
    PageSize = MiniPdfPageSize.A4,
    Margins = new MiniPdfMargins(left: 36, top: 48, right: 36, bottom: 48),
});
```

`PageSize` and `Margins` apply to DOCX input. Dimensions use PDF points:
72 points equal one inch. They do not override XLSX worksheet or PPTX slide sizes.

### Reduce memory for large, simple Excel tables

```csharp
using System.IO;
using MiniSoftware;

using var input = File.OpenRead("workbook.xlsx");
using var output = File.Create("low-memory.pdf");
MiniPdf.ConvertToPdf(input, output, new MiniPdfConversionOptions
{
    LowMemory = true,
    Compress = true,
});
```

This opt-in XLSX mode uses temporary disk storage. Unsupported features, such
as formulas, merged cells, and drawings, produce an explicit error rather than
silently falling back. Read the [supported features and limitations](documents/README.nuget.md#low-memory-xlsx-conversion-opt-in)
before enabling it. Returning `byte[]` still retains the complete PDF in memory.

### Provide fonts for headless hosts

```csharp
using System.IO;
using MiniSoftware;

MiniPdf.RegisterFont("Arial", File.ReadAllBytes(Path.Combine("Fonts", "arial.ttf")));
MiniPdf.ConvertToPdf("report.docx", "font-report.pdf");
```

Supply licensed `.ttf` or `.ttc` files that contain the required glyphs.
Register fonts once at application startup: registrations are process-wide.
MiniPdf does not distribute these font files.

See the [.NET library guide](documents/README.nuget.md) and
[CLI reference](documents/README.nuget.cli.md) for more examples and options.

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
| [CLI reference](documents/README.nuget.cli.md) | Commands, fonts, compression, and Excel preview options |
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
