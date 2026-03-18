# MiniPdf CLI

[![NuGet](https://img.shields.io/nuget/v/MiniPdf.Cli.svg)](https://www.nuget.org/packages/MiniPdf.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiniPdf.Cli.svg)](https://www.nuget.org/packages/MiniPdf.Cli)
[![GitHub stars](https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github)](https://github.com/shps951023/MiniPdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

A command-line tool to convert Excel (`.xlsx`) and Word (`.docx`) files to PDF.  
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

# Specify output path
minipdf report.docx -o /path/to/output.pdf

# Register custom fonts (for containers / Blazor WASM)
minipdf report.docx --fonts ./Fonts
```

## Commands

| Command | Description |
|---------|-------------|
| `minipdf <file>` | Convert `.xlsx` / `.docx` to PDF |
| `minipdf convert <file> -o <out>` | Convert with explicit output path |
| `minipdf --version` | Show version |
| `minipdf --help` | Show help |

## Features

- No COM, no Office installation, no LibreOffice — runs anywhere .NET runs
- Single command, zero config
- Custom font registration for headless / container environments
- Apache 2.0 licensed — free for commercial use

## Links

- Library package: [MiniPdf on NuGet](https://www.nuget.org/packages/MiniPdf)
- Source code: https://github.com/shps951023/MiniPdf
- License: [Apache-2.0](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)
