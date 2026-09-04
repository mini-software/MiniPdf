# MiniPdf for Python

Experimental pure Python implementation of MiniPdf. It converts basic DOCX,
XLSX, and PPTX content to PDF 1.4 without requiring Microsoft Office,
LibreOffice, .NET, or Rust at runtime.

The package requires Python 3.10 or later.

## Install

```bash
pip install minipdf
```

## Python API

```python
import minipdf

minipdf.convert_to_pdf("report.docx", "report.pdf")
pdf_bytes = minipdf.convert_to_pdf_bytes("report.docx")
```

## Command Line

```bash
minipdf report.docx
minipdf workbook.xlsx -o workbook.pdf
minipdf slides.pptx -o slides.pdf
minipdf convert report.docx -o report.pdf
minipdf report.docx --paper-size a4
minipdf report.docx --page-width 400 --page-height 500
```

## Initial Scope

Version 0.1 supports document page geometry, margins, paragraphs, explicit page
breaks, and basic bold, italic, and font-size run formatting. Text currently
uses PDF built-in Latin fonts. Tables, images, embedded fonts, CJK/RTL shaping,
headers, footers, and lists are not yet supported. XLSX rendering currently
extracts common cell value types into text rows, while PPTX rendering extracts
paragraph text and preserves slide page dimensions and boundaries.
