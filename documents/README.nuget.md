[![NuGet](https://img.shields.io/nuget/v/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiniPdf.svg)](https://www.nuget.org/packages/MiniPdf)
[![GitHub stars](https://img.shields.io/github/stars/shps951023/MiniPdf?logo=github)](https://github.com/shps951023/MiniPdf)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)

English | [简体中文](README.zh-CN.md) | [繁体中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Italiano](README.it.md) | [Français](README.fr.md)

A minimal, lightweight .NET library for converting office files to PDF.

Online Demo: https://mini-software.github.io/MiniPdf/

## Features

- Excel to PDF conversion (`.xlsx`)
- Word to PDF conversion (`.docx`)
- PowerPoint to PDF conversion (`.pptx`)
- PDF merge with top-level bookmarks
- Minimal dependencies — lightweight; relies almost entirely on built-in .NET APIs
- Serverless-ready — no COM, no Office installation, no Adobe Acrobat — runs anywhere .NET runs
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

// Render selected Excel sheets by name or 1-based index (null renders all sheets)
MiniPdf.ConvertToPdf("data.xlsx", "selected.pdf", sheets: new[] { "Summary", "Details" });
MiniPdf.ConvertToPdf("data.xlsx", "selected.pdf", sheetIndexes: new[] { 1, 3 });

// Stream to byte array
using var stream = File.OpenRead("data.xlsx");
byte[] pdfBytesFromStream = MiniPdf.ConvertToPdf(stream);

// Merge PDFs and add bookmarks
MiniPdf.MergePdf(new[] { "cover.pdf", "body.pdf" }, "merged.pdf", new PdfMergeOptions
{
  BookmarkTitles = new[] { "Cover", "Body" },
  Bookmarks = new[] { new PdfBookmark("Body page 2", 2) },
});
```

## PDF Merge Usage

### Merge Files

```csharp
using MiniSoftware;

MiniPdf.MergePdf(
  new[] { "cover.pdf", "chapter-1.pdf", "chapter-2.pdf" },
  "book.pdf");
```

Input order is preserved, so pages from `cover.pdf` appear first, followed by `chapter-1.pdf`, then `chapter-2.pdf`.

### Add One Bookmark Per Source PDF

Use `BookmarkTitles` when each input PDF should become a top-level bookmark. The number of titles must match the number of input PDFs.

```csharp
MiniPdf.MergePdf(
  new[] { "cover.pdf", "chapter-1.pdf", "chapter-2.pdf" },
  "book-with-bookmarks.pdf",
  new PdfMergeOptions
  {
    BookmarkTitles = new[] { "Cover", "Chapter 1", "Chapter 2" },
  });
```

### Add Bookmarks To Specific Pages

Use `PdfBookmark` for explicit page targets. `PageIndex` is zero-based and refers to the final merged PDF.

```csharp
MiniPdf.MergePdf(
  new[] { "cover.pdf", "chapter-1.pdf", "chapter-2.pdf" },
  "book-with-custom-bookmarks.pdf",
  new PdfMergeOptions
  {
    Bookmarks = new[]
    {
      new PdfBookmark("Start", 0),
      new PdfBookmark("Chapter 2 - page 3", 8),
    },
  });
```

### Return A Byte Array

```csharp
byte[] mergedPdf = MiniPdf.MergePdf(
  new[] { "cover.pdf", "chapter-1.pdf" },
  new PdfMergeOptions
  {
    BookmarkTitles = new[] { "Cover", "Chapter 1" },
  });
```

Supported inputs are unencrypted PDFs that use classic xref tables. Encrypted PDFs and xref-stream-only PDFs throw `NotSupportedException`.

## Benchmark

MiniPdf output is compared against LibreOffice as the reference renderer across 373 test cases.



Detailed reports:

- [XLSX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports/comparison_report.md)
- [DOCX Benchmark Report](https://github.com/mini-software/MiniPdf/blob/main/tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)
- [Issue Files Xlsx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_xlsx/comparison_report.md)
- [Issue Files Docx Report](https://github.com/mini-software/MiniPdf/blob/main/tests/Issue_Files/reports_docx/comparison_report.md)

## Links

- Source code: https://github.com/shps951023/MiniPdf
- License: [Apache-2.0](https://github.com/shps951023/MiniPdf/blob/main/LICENSE)