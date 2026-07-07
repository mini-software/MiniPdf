[![NuGet](https://img.shields.io/nuget/v/MiniPdf.Cli.svg)](https://www.nuget.org/packages/MiniPdf.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiniPdf.Cli.svg)](https://www.nuget.org/packages/MiniPdf.Cli)
[![GitHub stars](https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github)](https://github.com/shps951023/MiniPdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)

A command-line tool to convert Excel (`.xlsx`), Word (`.docx`), and PowerPoint (`.pptx`) files to PDF.  
Powered by [MiniPdf](https://www.nuget.org/packages/MiniPdf) — zero-dependency, pure .NET.

## Install

```bash
dotnet tool install --global MiniPdf.Cli
```

## Usage

```bash
# Convert Excel to PDF (output: data.pdf)
minipdf data.xlsx

# Convert Word to PDF
minipdf report.docx

# Convert PowerPoint to PDF
minipdf slides.pptx

# Specify output path
minipdf report.docx -o /path/to/output.pdf

# Register custom fonts (for containers / Blazor WASM)
minipdf report.docx --fonts ./Fonts

# Compress PDF content streams for large outputs
minipdf data.xlsx --compress

# Render a bounded Excel preview
minipdf data.xlsx --max-rows 200 --max-columns 20 --compress
```

## Commands

| Command | Description |
|---------|-------------|
| `minipdf <file>` | Convert `.xlsx` / `.docx` / `.pptx` to PDF |
| `minipdf convert <file> -o <out>` | Convert with explicit output path |
| `minipdf convert <file> --compress` | Compress PDF content streams |
| `minipdf data.xlsx --max-rows <n> --max-columns <n>` | Render a bounded Excel preview |
| `minipdf --version` | Show version |
| `minipdf --help` | Show help |

## Features

- No COM, no Office installation, no LibreOffice — runs anywhere .NET runs
- Single command, zero config
- Custom font registration for headless / container environments
- Optional PDF stream compression and Excel preview limits for large workbooks
- Apache 2.0 licensed — free for commercial use

## Links

- Library package: [MiniPdf on NuGet](https://www.nuget.org/packages/MiniPdf)
- Source code: https://github.com/shps951023/MiniPdf
- License: [Apache-2.0](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)