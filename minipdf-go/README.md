# MiniPdf for Go

The experimental native Go implementation of MiniPdf. It provides a reusable
`minipdf` package and a dependency-free command-line application.

[Project portal](../README.md) | [Rust implementation](../minipdf-rs/README.md) |
[.NET implementation](../documents/README.nuget.md)

> The Go implementation is an early foundation and is not feature-equivalent
> with MiniPdf for .NET or Rust. It currently renders basic text from XLSX,
> DOCX, and PPTX packages, and a supported subset of DOCX tables. Complex
> Office layout is not yet supported.

## Requirements

- Go 1.22 or later
- No Microsoft Office, LibreOffice, Adobe Acrobat, or .NET runtime at runtime

## Library

```go
package main

import minipdf "github.com/mini-software/MiniPdf/minipdf-go"

func main() {
	if err := minipdf.ConvertToPDF("report.docx", "report.pdf"); err != nil {
		panic(err)
	}
}
```

Convert in-memory Office package bytes:

```go
pdf, err := minipdf.ConvertBytesToPDF(input)
```

Compress PDF page content streams:

```go
pdf, err := minipdf.ConvertBytesToPDFWithOptions(input, minipdf.ConversionOptions{
	Compress: true,
})
```

Override DOCX page margins in PDF points:

```go
margins, err := minipdf.NewMargins(36, 48, 36, 48)
if err != nil {
	panic(err)
}
pdf, err := minipdf.ConvertBytesToPDFWithOptions(input, minipdf.ConversionOptions{
	Margins: &margins,
})
```

Margin overrides are rejected for XLSX and PPTX input.

Limit XLSX output or override worksheet orientation:

```go
landscape := true
pdf, err := minipdf.ConvertBytesToPDFWithOptions(input, minipdf.ConversionOptions{
	MaxRows: 100,
	MaxColumns: 12,
	Landscape: &landscape,
})
```

Convert between streams:

```go
err := minipdf.ConvertReaderToWriter(input, output, minipdf.ConversionOptions{})
```

Register a TrueType font before conversion when built-in PDF fonts do not cover
the document text:

```go
fontData, err := os.ReadFile("fonts/NotoSans-Regular.ttf")
if err != nil {
	panic(err)
}
minipdf.RegisterFont("Noto Sans", fontData)
defer minipdf.ClearRegisteredFonts()
```

Registered `.ttf` fonts are embedded as Type0/CID fonts with ToUnicode maps.
The structured DOCX table renderer also accepts registered TrueType collections
(the first face), and resolves selected installed Windows fonts, including
SimSun/SimHei and their Chinese family names. Collections are rebuilt as standalone
sfnt fonts before PDF embedding. Font subsetting and complex-script shaping are
not yet implemented. Font binaries are not distributed with MiniPdf.

Override the output page size:

```go
size, err := minipdf.NewPageSize(400, 500)
if err != nil {
	panic(err)
}
pdf, err := minipdf.ConvertBytesToPDFWithOptions(input, minipdf.ConversionOptions{
	PageSize: &size,
})
```

## Command Line

```powershell
go install github.com/mini-software/MiniPdf/minipdf-go/cmd/minipdf@latest
minipdf report.docx
minipdf data.xlsx -o data.pdf
minipdf slides.pptx --paper-size a4
minipdf convert report.docx --page-width 400 --page-height 500
minipdf report.docx --fonts ./fonts
minipdf report.docx --compress
minipdf data.xlsx --max-rows 100 --max-columns 12 --landscape
```

## Current Scope

| Capability | Go status |
|---|---|
| XLSX to PDF | Basic cell text and shared strings |
| DOCX to PDF | Basic text; supported table documents also preserve grid widths, horizontal/vertical merges, borders, fills, run fonts, highlights, paragraph spacing/alignment, and PNG/JPEG drawings |
| PPTX to PDF | Basic slide text |
| PDF output | Dependency-free PDF 1.4 writer |
| Page size | Office geometry, A4/Letter presets, or custom points |
| Fonts | Embedded registered TTF fonts with ToUnicode; printed XLSX grids can use installed Windows Calibri/Arial variants |
| Input safety | Bounded ZIP entry count, size, expansion ratio, encryption, and path validation |
| Interfaces | Go package with file, byte, and stream APIs; native CLI |

Single-page DOCX documents with non-nested, non-floating table grids can use the
structured renderer. It resolves paragraph/style inheritance and theme fonts, uses glyph
metrics for wrapping, respects East Asian punctuation boundaries, and honors
Word's FE-layout CJK metric compatibility flag. Images use their package
relationships and DrawingML extents; simple VML text-box images use their margin
offsets and text-box insets. Image decoding is limited to 25 million pixels.
Missing or invalid embedded images produce errors rather than disappearing.

Documents requiring unsupported section changes, numbering, anchored drawings,
exact-height rows, multi-page structured layout, or referenced EMF/WMF images
retain the existing text converter. If measured paragraph, inline-image, or
table flow would cross the first page's content boundary, the entire document
uses the text converter; structured pagination, keep rules, and row splitting
are not implemented. Metafile formats are checked before
structured rendering; malformed PNG/JPEG images still produce decoding errors.
Images inside table cells, complex drawing geometry, conditional
table styles, headers/footers, and complex-script shaping remain unsupported.
Structured text uses matching registered fonts or selected licensed Windows
fonts, with Arial/SimSun glyph fallback when available. Missing Unicode glyphs
produce an explicit error on the structured path. Fonts are embedded without
subsetting, so large CJK fonts can substantially increase PDF size.

XLSX text output skips entirely empty pages within worksheets containing text,
including empty horizontal column groups and pages from trailing formatted rows.
Blank rows within nonempty pages retain their spacing. Layout-only worksheets
keep their existing blank-page behavior.

Worksheets with an explicit `_xlnm.Print_Area` use the grid renderer instead of
fixed-width text column groups. This path respects rectangular or whole-row/
column print ranges, repeated print-title rows and columns, hidden rows and
columns, percentage scaling, fit-to-page mode, manual page breaks, and horizontal/
vertical print centering. It keeps
formatted empty cells inside the print area, so printable template tables are
not discarded merely because they have no text. Sheets without a print area
retain their previous rendering path.

Grid styles resolve RGB and workbook theme colors, including HSL luminance
tints, for fonts and solid fills. Printed grids also apply conditional solid
fills for text rules, `ISNUMBER(SEARCH(...))` expressions, and numeric `cellIs`
comparisons. Text searches are case-insensitive and support Excel wildcards;
sheet-local cell references respect relative/absolute addressing. Rule priority
and `stopIfTrue` are honored.

Printed grids use matching registered font families/variants, or installed
Windows Calibri/Arial regular, bold, italic, and bold-italic TTF files when
available. Actual glyph advance widths and font ascent/descent are used for
alignment and shrink-to-fit. Missing/unrecognized font families keep the
built-in PDF font fallback; non-print-area worksheets retain their previous
font-rendering path. Register variant names such as `Calibri Bold Italic` for
portable source-font matching. Fonts are embedded without subsetting; no font
binaries are distributed with MiniPdf.

Printed numeric cells support common fixed-decimal, grouped, percentage, General,
and day/month/year date formats, including custom escaped/quoted date literals.
The workbook's 1900/1904 date system is respected. Unsupported number-format
patterns keep the stored value.

One-/two-cell anchored PNG/JPEG/GIF pictures support source cropping, print
scaling, and repetition inside print-title bands. Images are limited to
25 million pixels and transparency is composited onto white. Negative crops,
external image targets, and general vector drawings are not supported.

Printed footers support left/center/right sections, font names/sizes,
bold/italic toggles, escaped ampersands, sheet names, and final document page
number/count fields. First/even/odd footer variants and `scaleWithDoc` are
respected. Footer placement currently uses worksheet page margins; separate
`alignWithMargins="0"` geometry and header content are not implemented.
Double cell borders retain their two separate strokes.

Invalid or unsupported print-range expressions and invalid numeric print
settings return an explicit conversion error, as do invalid theme colors,
tints, conditional range/format metadata, corrupt pictures/fonts, and unsupported
footer control codes. Conditional rules outside the
supported subset retain base styling. Dynamic print-area formulas, general
conditional formulas, conditional font/border changes, table styles, indexed
colors, patterned/gradient fills, general drawings, complex text shaping, and
full Office font/layout fidelity are not supported.

## Development

```powershell
cd minipdf-go
go fmt ./...
go vet ./...
go test ./...
go run ./cmd/minipdf path\to\input.docx
```

## Publishing

Go modules are published from Git tags rather than uploaded to a package
registry. Because this module is in the `minipdf-go` repository subdirectory,
its tags must include that prefix.

After the release change has been merged to `main`, create and push a semantic
version tag from the release commit:

```powershell
git tag minipdf-go/v<version>
git push origin minipdf-go/v<version>
```

The [Go Release workflow](../.github/workflows/go-release.yml) validates the
module, builds Windows, Linux, and macOS CLI archives for AMD64 and ARM64,
writes SHA-256 checksums, and creates the GitHub Release. No package registry
token is required. The public Go module proxy discovers the library from the
same tag.

Verify the published module:

```powershell
go list -m github.com/mini-software/MiniPdf/minipdf-go@v<version>
go install github.com/mini-software/MiniPdf/minipdf-go/cmd/minipdf@v<version>
minipdf --version
```

The workflow can also be run manually for an existing tag. It intentionally
does not create tags, so a manual run cannot publish an untagged Go module.

## License

[Apache License 2.0](../LICENSE).