[![NuGet](https://img.shields.io/nuget/v/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![GitHub stars](https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github)](https://github.com/shps951023/MiniPdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)

# MiniPdf for .NET

The stable MiniPdf implementation for converting Office files to PDF from .NET
applications or the command line.
It runs without Microsoft Office, LibreOffice, Adobe Acrobat, or COM automation.

[Project home](https://github.com/mini-software/MiniPdf) ·
[Online demo](https://mini-software.github.io/MiniPdf/) ·
[Rust implementation](https://github.com/mini-software/MiniPdf/tree/main/minipdf-rs)

## Features

- Excel to PDF conversion (`.xlsx`)
- Word to PDF conversion (`.docx`)
- PowerPoint to PDF conversion (`.pptx`)
- Minimal dependencies — lightweight; relies almost entirely on built-in .NET APIs
- Serverless-ready — no COM, no Office installation, no Adobe Acrobat — runs anywhere .NET runs
- .NET library, global tool, and standalone Native AOT binaries
- Valid PDF 1.4 output
- 100% open-source & free — Apache 2.0 licensed, commercial use welcome; just keep the attribution. PRs & contributions are even better!

## Install

```bash
dotnet add package MiniPdf
```

## Usage

```csharp
using MiniSoftware;

// Excel to PDF
MiniPdf.ConvertToPdf("data.xlsx", "output.pdf");

// Word to PDF
MiniPdf.ConvertToPdf("report.docx", "output.pdf");

// PowerPoint to PDF
MiniPdf.ConvertToPdf("slides.pptx", "output.pdf");

// File to byte array
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// Render selected Excel sheets by name or 1-based index
MiniPdf.ConvertToPdf("data.xlsx", "selected.pdf", new MiniPdfConversionOptions
{
  Sheets = new[] { "Summary", "Details" },
  SheetIndexes = new[] { 1, 3 },
});

// Stream to byte array
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytesFromStream = MiniPdf.ConvertToPdf(stream);

// Stream directly to another stream without allocating the complete PDF byte array
stream.Position = 0;
using var output = File.Create("output.pdf");
MiniPdf.ConvertToPdf(stream, output);
```

### Conversion Options

```csharp
MiniPdf.ConvertToPdf("data.xlsx", "compact.pdf", new MiniPdfConversionOptions
{
  Sheets = new[] { "Summary", "Details" },
  Compress = true,
  FitToPage = true,
  Landscape = true,
  PrintScale = 70,
  RowsPerPage = 80,
});
```

Sheet selection and layout options apply to XLSX input. `Compress` controls PDF
content-stream compression for every supported format.

### Custom Fonts

Register TrueType `.ttf` or `.ttc` fonts once at application startup when the
host has limited system fonts, such as a container or Blazor WebAssembly app.

```csharp
MiniPdf.RegisterFont("NotoSansSC", File.ReadAllBytes("Fonts/NotoSansSC-Regular.ttf"));
MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

## Command Line

Install the .NET global tool:

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

Standalone Native AOT binaries for Windows, Linux, and macOS are available on
the [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) page.
See the [CLI reference](https://github.com/mini-software/MiniPdf/blob/main/documents/README.nuget.cli.md)
for commands and options.

## Benchmark

MiniPdf output is compared against LibreOffice as the reference renderer across 373 test cases.



Detailed reports:

- [XLSX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports/comparison_report.md)
- [DOCX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)
- [Issue Files Xlsx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_xlsx/comparison_report.md)
- [Issue Files Docx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_docx/comparison_report.md)

## Links

- Project portal: https://github.com/mini-software/MiniPdf
- .NET CLI: https://www.nuget.org/packages/MiniPdf.Cli
- API source: https://github.com/mini-software/MiniPdf/blob/main/src/MiniPdf/MiniPdf.cs
- License: [Apache-2.0](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)