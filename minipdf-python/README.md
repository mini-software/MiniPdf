# MiniPdf for Python

Experimental pure Python implementation of MiniPdf. The initial release
converts basic DOCX paragraphs to PDF 1.4 without requiring Microsoft Office,
LibreOffice, .NET, or Rust at runtime.

The package requires Python 3.10 or later. XLSX support is planned; PPTX is not
currently supported.

## Install

```bash
pip install minipdf-python
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
minipdf convert report.docx -o report.pdf
minipdf report.docx --paper-size a4
minipdf report.docx --page-width 400 --page-height 500
```

## Initial Scope

Version 0.1 supports document page geometry, margins, paragraphs, explicit page
breaks, and basic bold, italic, and font-size run formatting. Text currently
uses PDF built-in Latin fonts. Tables, images, embedded fonts, CJK/RTL shaping,
headers, footers, lists, XLSX, and PPTX are not yet supported.
