# MiniPdf for Python

MiniPdf converts basic DOCX, XLSX, and PPTX content to PDF 1.4 without
requiring Microsoft Office or LibreOffice. Conversion runs through the Rust
MiniPdf engine in `minipdf-rs` via the PyO3 extension `minipdf._native`, which
the package build backend compiles from [`native/`](native/). A pure Python
renderer with reduced feature coverage remains as a fallback for source-tree
runs where the extension has not been built.

The package requires Python 3.10 or later, and building it requires a Rust
toolchain.

## Native Rust Engine

`minipdf._native` is a PyO3 extension that delegates conversion to the Rust
engine in `minipdf-rs`. When the module is importable, the Python API uses it
automatically and the pure Python renderers are only used as a fallback.

Installing the package builds the extension:

```bash
pip install .
```

To rebuild only the extension into the package source tree, use PowerShell:

```powershell
scripts/Build-Python-Native.ps1
```

The visual benchmark runner builds the extension automatically.

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
