# MiniPdf

[![NuGet](https://img.shields.io/nuget/v/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![GitHub stars](https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github)](https://github.com/shps951023/MiniPdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

English | [简体中文](README.zh-CN.md) | [繁体中文](documents/README.zh-TW.md) | [日本語](documents/README.ja.md) | [한국어](documents/README.ko.md) | [Italiano](documents/README.it.md) | [Français](documents/README.fr.md)

A minimal, zero-dependency .NET library for converting office files to PDF.

Online Demo: https://mini-software.github.io/MiniPdf/

## Features

- Excel to PDF conversion (`.xlsx`)
- Word to PDF conversion (`.docx`)
- Zero dependencies (uses built-in .NET APIs only)
- Valid PDF 1.4 output

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

// File to byte array
byte[] pdfBytes = MiniPdf.ConvertToPdf("data.xlsx");

// Stream to byte array
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytesFromStream = MiniPdf.ConvertToPdf(stream);
```

## Benchmark

MiniPdf output is compared against LibreOffice as the reference renderer across 373 test cases.



Detailed reports:

- [XLSX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports/comparison_report.md)
- [DOCX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)
- [Issue Files Xlsx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_xlsx/comparison_report.md)
- [Issue Files Docx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_docx/comparison_report.md)

## Links

- Source code: https://github.com/shps951023/MiniPdf
- License: [Apache-2.0](LICENSE)
