package minipdf

import (
	"archive/zip"
	"bytes"
	"testing"
)

func TestConvertDOCXToPDF(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body><w:p><w:r><w:t>Hello DOCX</w:t></w:r></w:p><w:sectPr><w:pgSz w:w="12240" w:h="15840"/></w:sectPr></w:body></w:document>`,
	})

	pdf, err := ConvertBytesToPDF(input)
	assertPDFContains(t, pdf, err, "Hello DOCX", "/MediaBox [0 0 612 792]")
}

func TestConvertXLSXToPDF(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"xl/workbook.xml":      `<?xml version="1.0"?><workbook/>`,
		"xl/sharedStrings.xml": `<?xml version="1.0"?><sst><si><t>Hello XLSX</t></si></sst>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="inlineStr"><is><t>Cell B</t></is></c></row></sheetData>` +
			`<pageSetup paperSize="1" orientation="landscape"/></worksheet>`,
	})

	pdf, err := ConvertBytesToPDF(input)
	assertPDFContains(t, pdf, err, "Hello XLSX", "Cell B", "/MediaBox [0 0 792 612]")
}

func TestConvertPPTXToPDF(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"ppt/presentation.xml":  `<?xml version="1.0"?><p:presentation xmlns:p="urn:p"><p:sldSz cx="9144000" cy="6858000"/></p:presentation>`,
		"ppt/slides/slide1.xml": `<?xml version="1.0"?><p:sld xmlns:p="urn:p" xmlns:a="urn:a"><a:p><a:r><a:t>Hello PPTX</a:t></a:r></a:p></p:sld>`,
	})

	customSize, err := NewPageSize(300, 400)
	if err != nil {
		t.Fatal(err)
	}
	pdf, err := ConvertBytesToPDFWithOptions(input, ConversionOptions{PageSize: &customSize})
	assertPDFContains(t, pdf, err, "Hello PPTX", "/MediaBox [0 0 300 400]")
}

func assertPDFContains(t *testing.T, pdf []byte, err error, values ...string) {
	t.Helper()
	if err != nil {
		t.Fatalf("conversion error = %v", err)
	}
	if !bytes.HasPrefix(pdf, []byte("%PDF-1.4")) {
		t.Fatal("conversion did not return a PDF")
	}
	for _, value := range values {
		if !bytes.Contains(pdf, []byte(value)) {
			t.Errorf("PDF does not contain %q", value)
		}
	}
}

func officePackageBytes(t *testing.T, entries map[string]string) []byte {
	t.Helper()
	var buffer bytes.Buffer
	writer := zip.NewWriter(&buffer)
	for name, content := range entries {
		file, err := writer.Create(name)
		if err != nil {
			t.Fatal(err)
		}
		if _, err := file.Write([]byte(content)); err != nil {
			t.Fatal(err)
		}
	}
	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}
	return buffer.Bytes()
}
