package minipdf

import (
	"archive/zip"
	"bytes"
	"errors"
	"math"
	"testing"
)

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
