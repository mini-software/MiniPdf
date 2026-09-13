package minipdf

import (
	"archive/zip"
	"bytes"
	"errors"
	"io"
	"math"
	"testing"
)

func TestClearRegisteredFonts(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Example", []byte("font data"))

	ClearRegisteredFonts()

	if fonts := RegisteredFonts(); len(fonts) != 0 {
		t.Fatalf("RegisteredFonts() returned %d fonts after clear", len(fonts))
	}
}

func TestConvertReaderToWriter(t *testing.T) {
	input := zipPackage(t, "word/document.xml", `<w:document xmlns:w="urn:w"><w:body><w:p><w:r><w:t>Hello stream</w:t></w:r></w:p></w:body></w:document>`)
	expected, err := ConvertBytesToPDF(input)
	if err != nil {
		t.Fatal(err)
	}

	var output bytes.Buffer
	if err := ConvertReaderToWriter(bytes.NewReader(input), &output, ConversionOptions{}); err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(output.Bytes(), expected) {
		t.Fatal("stream conversion differs from byte conversion")
	}
}

func TestConvertReaderToWriterPropagatesIOErrors(t *testing.T) {
	_, err := ConvertReaderToPDF(errorReader{})
	if !errors.Is(err, errTestIO) {
		t.Fatalf("ConvertReaderToPDF() error = %v, want errTestIO", err)
	}

	input := zipPackage(t, "word/document.xml", `<w:document xmlns:w="urn:w"><w:body/></w:document>`)
	err = ConvertReaderToWriter(bytes.NewReader(input), errorWriter{}, ConversionOptions{})
	if !errors.Is(err, errTestIO) {
		t.Fatalf("ConvertReaderToWriter() error = %v, want errTestIO", err)
	}
}

var errTestIO = errors.New("test I/O error")

type errorReader struct{}

func (errorReader) Read([]byte) (int, error) {
	return 0, errTestIO
}

type errorWriter struct{}

func (errorWriter) Write([]byte) (int, error) {
	return 0, errTestIO
}

var _ io.Reader = errorReader{}
var _ io.Writer = errorWriter{}

func TestDetectOfficeFormat(t *testing.T) {
	tests := []struct {
		name   string
		entry  string
		format OfficeFormat
	}{
		{name: "docx", entry: "word/document.xml", format: OfficeFormatDOCX},
		{name: "xlsx", entry: "xl/workbook.xml", format: OfficeFormatXLSX},
		{name: "pptx", entry: "ppt/presentation.xml", format: OfficeFormatPPTX},
		{name: "unknown", entry: "custom/data.xml", format: OfficeFormatUnknown},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			format, err := DetectOfficeFormat(zipPackage(t, test.entry, "<root/>"))
			if err != nil {
				t.Fatalf("DetectOfficeFormat() error = %v", err)
			}
			if format != test.format {
				t.Fatalf("DetectOfficeFormat() = %q, want %q", format, test.format)
			}
		})
	}
}

func TestNewPageSizeRejectsInvalidDimensions(t *testing.T) {
	for _, dimensions := range [][2]float64{{0, 100}, {-1, 100}, {100, math.Inf(1)}} {
		if _, err := NewPageSize(dimensions[0], dimensions[1]); err == nil {
			t.Fatalf("NewPageSize(%v, %v) succeeded", dimensions[0], dimensions[1])
		}
	}
}

func TestUnknownPackageIsUnsupported(t *testing.T) {
	_, err := ConvertBytesToPDF(zipPackage(t, "custom/data.xml", "<root/>"))
	if !errors.Is(err, ErrUnsupportedFormat) {
		t.Fatalf("ConvertBytesToPDF() error = %v, want ErrUnsupportedFormat", err)
	}
}

func zipPackage(t *testing.T, name, content string) []byte {
	t.Helper()
	var buffer bytes.Buffer
	writer := zip.NewWriter(&buffer)
	file, err := writer.Create(name)
	if err != nil {
		t.Fatal(err)
	}
	if _, err := file.Write([]byte(content)); err != nil {
		t.Fatal(err)
	}
	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}
	return buffer.Bytes()
}
