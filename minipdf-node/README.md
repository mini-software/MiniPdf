# MiniPdf for Node.js

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](../LICENSE)

Experimental native Node.js bindings for the MiniPdf Rust conversion engine.
The package converts XLSX, DOCX, and PPTX files without requiring Microsoft
Office, LibreOffice, Adobe Acrobat, or .NET at runtime.

> The Node.js implementation is under active development and is not yet
> feature-equivalent with MiniPdf for .NET.

## Requirements

- Node.js 18 or later
- Rust 1.82 or later when building the package from source

## Installation

```powershell
npm install minipdf
```

Precompiled native addons are provided for Windows, macOS, glibc Linux, and
musl Linux on x64 and ARM64.

## Development

```powershell
Set-Location minipdf-node
npm install
npm run build
```

## Usage

Convert an Office file to a PDF on disk:

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

Return a Node.js `Buffer` instead:

```javascript
const pdf = minipdf.convertToPdfBytes('data.xlsx')
```

Convert an in-memory Office package:

```javascript
const fs = require('node:fs')
const minipdf = require('minipdf')

const input = fs.readFileSync('slides.pptx')
const pdf = minipdf.convertBytesToPdf(input)
```

Override the output page size with a preset or custom dimensions in PDF points:

```javascript
minipdf.convertToPdf('data.xlsx', 'data.pdf', {
  pageSize: minipdf.PageSize.A4
})

minipdf.convertToPdf('report.docx', 'report.pdf', {
  pageSize: { width: 400, height: 500 }
})
```

Register font bytes before conversion when the host has limited system fonts:

```javascript
const font = fs.readFileSync('fonts/NotoSansSC-Regular.ttf')
minipdf.registerFont('NotoSansSC', font)
```

The conversion functions are synchronous and occupy the calling Node.js thread.
Use a worker thread when converting documents in a latency-sensitive server.

## API

- `convertToPdf(inputPath, outputPath, options?)`
- `convertToPdfBytes(inputPath, options?)`
- `convertBytesToPdf(input, options?)`
- `detectOfficeFormat(input)`
- `registerFont(name, fontData)`
- `registeredFonts()`
- `PageSize.A4` and `PageSize.LETTER`

## Validation

```powershell
npm install
npm run build
npm test
npm pack --dry-run
```

## Publishing

The `NPM Publish` GitHub Actions workflow builds all native addons before it
publishes any package. For the first release, run it manually in `initial` mode
with a short-lived granular npm token that can publish new packages and bypass
2FA, stored as the `NPM_TOKEN` Actions secret.

After the initial release, configure `npm-publish.yml` as the trusted GitHub
Actions publisher for the main package and all eight platform packages. Future
`node-v*` GitHub releases use OIDC to stage packages for maintainer approval.