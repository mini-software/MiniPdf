# MiniPdf for Go

The experimental native Go implementation of MiniPdf. It provides a reusable
`minipdf` package and a dependency-free command-line application.

[Project portal](../README.md) | [Rust implementation](../minipdf-rs/README.md) |
[.NET implementation](../documents/README.nuget.md)

> The Go implementation is an early foundation and is not feature-equivalent
> with MiniPdf for .NET or Rust. It currently renders basic text from XLSX,
> DOCX, and PPTX packages. Complex Office layout is not yet supported.

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
```

## Current Scope

| Capability | Go status |
|---|---|
| XLSX to PDF | Basic cell text and shared strings |
| DOCX to PDF | Basic paragraphs, tabs, line breaks, and explicit page breaks |
| PPTX to PDF | Basic slide text |
| PDF output | Dependency-free PDF 1.4 writer |
| Page size | Office geometry, A4/Letter presets, or custom points |
| Fonts | Registration API reserved; embedding is not implemented yet |
| Interfaces | Go package and native CLI |

The initial renderer deliberately does not claim support for Office styles,
images, tables, charts, themes, formulas, merged cells, or font embedding.

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
git tag minipdf-go/v0.1.0
git push origin minipdf-go/v0.1.0
```

The [Go Release workflow](../.github/workflows/go-release.yml) validates the
module, builds Windows, Linux, and macOS CLI archives for AMD64 and ARM64,
writes SHA-256 checksums, and creates the GitHub Release. No package registry
token is required. The public Go module proxy discovers the library from the
same tag.

Verify the published module:

```powershell
go list -m github.com/mini-software/MiniPdf/minipdf-go@v0.1.0
go install github.com/mini-software/MiniPdf/minipdf-go/cmd/minipdf@v0.1.0
minipdf --version
```

The workflow can also be run manually for an existing tag. It intentionally
does not create tags, so a manual run cannot publish an untagged Go module.

## License

[Apache License 2.0](../LICENSE).