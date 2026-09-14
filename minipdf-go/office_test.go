package minipdf

import (
	"archive/zip"
	"bytes"
	"errors"
	"strings"
	"testing"
)

func TestOpenOfficePackageRejectsUnsafePaths(t *testing.T) {
	input := officePackageEntries(t, []packageEntry{
		{name: "word/document.xml", content: "<document/>"},
		{name: "../escape.xml", content: "<escape/>"},
	})

	_, err := openOfficePackage(input)
	assertPackageErrorContains(t, err, "unsafe entry path")
	if !errors.Is(err, ErrInvalidPackage) {
		t.Fatalf("error = %v, want ErrInvalidPackage", err)
	}
}

func TestOpenOfficePackageRejectsDuplicateNormalizedPaths(t *testing.T) {
	input := officePackageEntries(t, []packageEntry{
		{name: "word/document.xml", content: "<document/>"},
		{name: `word\document.xml`, content: "<duplicate/>"},
	})

	_, err := openOfficePackage(input)
	assertPackageErrorContains(t, err, "duplicate entry")
}

func TestValidateOfficePackageLimits(t *testing.T) {
	baseLimits := officePackageLimits{
		maxEntries:       10,
		maxEntrySize:     100,
		maxTotalSize:     200,
		maxExpansionRate: 10,
	}
	tests := []struct {
		name      string
		entries   []*zip.File
		inputSize uint64
		limits    officePackageLimits
		message   string
	}{
		{
			name: "entry count",
			entries: []*zip.File{
				zipFile("word/document.xml", 1, 1, 0),
				zipFile("word/styles.xml", 1, 1, 0),
			},
			inputSize: 2,
			limits:    officePackageLimits{maxEntries: 1, maxEntrySize: 100, maxTotalSize: 200, maxExpansionRate: 10},
			message:   "too many entries",
		},
		{
			name:      "entry size",
			entries:   []*zip.File{zipFile("word/document.xml", 10, 101, 0)},
			inputSize: 10,
			limits:    baseLimits,
			message:   "entry \"word/document.xml\" expands beyond",
		},
		{
			name: "total size",
			entries: []*zip.File{
				zipFile("word/document.xml", 60, 60, 0),
				zipFile("word/styles.xml", 60, 60, 0),
			},
			inputSize: 120,
			limits:    officePackageLimits{maxEntries: 10, maxEntrySize: 100, maxTotalSize: 100, maxExpansionRate: 10},
			message:   "package expands beyond",
		},
		{
			name:      "entry expansion ratio",
			entries:   []*zip.File{zipFile("word/document.xml", 10, 101, 0)},
			inputSize: 101,
			limits:    officePackageLimits{maxEntries: 10, maxEntrySize: 200, maxTotalSize: 200, maxExpansionRate: 10},
			message:   "entry \"word/document.xml\" exceeds",
		},
		{
			name:      "package expansion ratio",
			entries:   []*zip.File{zipFile("word/document.xml", 30, 30, 0)},
			inputSize: 2,
			limits:    baseLimits,
			message:   "package exceeds",
		},
		{
			name:      "encrypted entry",
			entries:   []*zip.File{zipFile("word/document.xml", 1, 1, 0x1)},
			inputSize: 1,
			limits:    baseLimits,
			message:   "encrypted entry",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			_, err := validateOfficePackage(test.entries, test.inputSize, test.limits)
			assertPackageErrorContains(t, err, test.message)
			if !errors.Is(err, ErrInvalidPackage) {
				t.Fatalf("error = %v, want ErrInvalidPackage", err)
			}
		})
	}
}

func TestConvertDOCXToPDF(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body><w:p><w:r><w:t>Hello DOCX</w:t></w:r></w:p><w:sectPr><w:pgSz w:w="12240" w:h="15840"/></w:sectPr></w:body></w:document>`,
	})

	pdf, err := ConvertBytesToPDF(input)
	assertPDFContains(t, pdf, err, "Hello DOCX", "/MediaBox [0 0 612 792]")
}

func TestConversionCompressionOption(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body><w:p><w:r><w:t>Hello compressed DOCX</w:t></w:r></w:p></w:body></w:document>`,
	})

	pdf, err := ConvertBytesToPDFWithOptions(input, ConversionOptions{Compress: true})
	assertPDFContains(t, pdf, err, "/Filter /FlateDecode")
}

func TestDOCXSectionMarginsAndOverride(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body><w:p><w:r><w:t>Margin text</w:t></w:r></w:p><w:sectPr><w:pgSz w:w="12240" w:h="15840"/><w:pgMar w:top="720" w:right="1440" w:bottom="720" w:left="1440"/></w:sectPr></w:body></w:document>`,
	})

	pdf, err := ConvertBytesToPDF(input)
	assertPDFContains(t, pdf, err, "72 756 Td")

	margins, err := NewMargins(20, 30, 40, 50)
	if err != nil {
		t.Fatal(err)
	}
	pdf, err = ConvertBytesToPDFWithOptions(input, ConversionOptions{Margins: &margins})
	assertPDFContains(t, pdf, err, "20 762 Td")
}

func TestDOCXMarginOverrideIsFormatSpecific(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"xl/workbook.xml":          `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
	})
	margins, err := NewMargins(20, 30, 40, 50)
	if err != nil {
		t.Fatal(err)
	}

	_, err = ConvertBytesToPDFWithOptions(input, ConversionOptions{Margins: &margins})
	if !errors.Is(err, ErrInvalidInput) {
		t.Fatalf("error = %v, want ErrInvalidInput", err)
	}
}

func TestDOCXMarginOverrideRejectsInvalidLayout(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body><w:p><w:r><w:t>Margin text</w:t></w:r></w:p><w:sectPr><w:pgSz w:w="12240" w:h="15840"/></w:sectPr></w:body></w:document>`,
	})
	invalidMargins := []Margins{
		{Left: -1},
		{Left: 400, Right: 300},
		{Top: 500, Bottom: 400},
	}
	for _, margins := range invalidMargins {
		_, err := ConvertBytesToPDFWithOptions(input, ConversionOptions{Margins: &margins})
		if !errors.Is(err, ErrInvalidInput) {
			t.Fatalf("margins = %#v, error = %v, want ErrInvalidInput", margins, err)
		}
	}
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
	if bytes.Contains(pdf, []byte("Sheet 1")) {
		t.Fatal("PDF contains a synthetic worksheet title")
	}
}

func TestConvertXLSXSkipsEmptyWorksheets(t *testing.T) {
	mixed := officePackageBytes(t, map[string]string{
		"xl/workbook.xml":          `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
		"xl/worksheets/sheet2.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1">` +
			`<c r="A1" t="inlineStr"><is><t>First sheet</t></is></c></row></sheetData></worksheet>`,
		"xl/worksheets/sheet3.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
		"xl/worksheets/sheet4.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1">` +
			`<c r="A1" t="inlineStr"><is><t>Second sheet</t></is></c></row></sheetData></worksheet>`,
	})

	pdf, err := ConvertBytesToPDF(mixed)
	assertPDFContains(t, pdf, err, "First sheet", "Second sheet")
	if pages := bytes.Count(pdf, []byte("/Type /Page ")); pages != 2 {
		t.Fatalf("mixed workbook page count = %d, want 2", pages)
	}

	allEmpty := officePackageBytes(t, map[string]string{
		"xl/workbook.xml":          `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
		"xl/worksheets/sheet2.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
	})
	pdf, err = ConvertBytesToPDF(allEmpty)
	if err != nil {
		t.Fatal(err)
	}
	if pages := bytes.Count(pdf, []byte("/Type /Page ")); pages != 1 {
		t.Fatalf("empty workbook page count = %d, want 1", pages)
	}

	layoutOnly := officePackageBytes(t, map[string]string{
		"xl/workbook.xml": `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1">` +
			`<c r="A1" t="inlineStr"><is><t>Data</t></is></c></row></sheetData></worksheet>`,
		"xl/worksheets/sheet2.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1" ht="25"/></sheetData>` +
			`<drawing r:id="rId1" xmlns:r="urn:relationships"/></worksheet>`,
		"xl/worksheets/sheet3.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
	})
	pdf, err = ConvertBytesToPDF(layoutOnly)
	if err != nil {
		t.Fatal(err)
	}
	if pages := bytes.Count(pdf, []byte("/Type /Page ")); pages != 2 {
		t.Fatalf("layout-only workbook page count = %d, want 2", pages)
	}
}

func TestSplitWorksheetColumnGroups(t *testing.T) {
	lines := []string{
		"A\tB\tC\tD\tE\tF\tG\tH\tI\tJ",
		"A1\tB1\tC1\tD1\tE1\tF1\tG1\tH1\tI1\tJ1",
	}

	groups := splitWorksheetColumnGroups(lines, 9)

	if len(groups) != 2 {
		t.Fatalf("group count = %d, want 2", len(groups))
	}
	if groups[0][0] != "A\tB\tC\tD\tE\tF\tG\tH\tI" || groups[1][0] != "J" {
		t.Fatalf("groups = %#v", groups)
	}
}

func TestSplitWorksheetTextOverflow(t *testing.T) {
	pages := splitWorksheetTextOverflow([]string{"Header", "ABCDEFGHIJK", "Short"}, 5)

	if len(pages) != 3 {
		t.Fatalf("page count = %d, want 3", len(pages))
	}
	want := [][]string{
		{"Heade", "ABCDE", "Short"},
		{"r", "FGHIJ", ""},
		{"", "K", ""},
	}
	for index := range want {
		if strings.Join(pages[index], "|") != strings.Join(want[index], "|") {
			t.Errorf("page %d = %#v, want %#v", index, pages[index], want[index])
		}
	}
}

func TestConvertXLSXPaginatesSingleColumnTextOverflow(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"xl/workbook.xml": `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>` +
			strings.Repeat("X", 1000) + `</t></is></c></row></sheetData></worksheet>`,
	})

	pdf, err := ConvertBytesToPDF(input)
	if err != nil {
		t.Fatal(err)
	}
	if pages := bytes.Count(pdf, []byte("/Type /Page ")); pages != 12 {
		t.Fatalf("PDF page count = %d, want 12", pages)
	}
}

func TestExtractWorksheetPreservesSparseRows(t *testing.T) {
	worksheet := []byte(`<?xml version="1.0"?><worksheet><sheetData>` +
		`<row r="1"><c r="A1" t="inlineStr"><is><t>First</t></is></c></row>` +
		`<row r="5"><c r="A5" t="inlineStr"><is><t>Fifth</t></is></c></row>` +
		`<row r="10"><c r="A10" t="inlineStr"><is><t>Tenth</t></is></c></row>` +
		`</sheetData></worksheet>`)

	lines, _, err := extractWorksheet(worksheet, nil)
	if err != nil {
		t.Fatal(err)
	}
	if len(lines) != 10 {
		t.Fatalf("line count = %d, want 10", len(lines))
	}
	if lines[0] != "First" || lines[4] != "Fifth" || lines[9] != "Tenth" {
		t.Fatalf("sparse row values = %#v", lines)
	}
}

func TestExtractWorksheetPreservesSparseColumns(t *testing.T) {
	worksheet := []byte(`<?xml version="1.0"?><worksheet><sheetData>` +
		`<row r="1"><c r="A1" t="inlineStr"><is><t>Left</t></is></c>` +
		`<c r="D1" t="inlineStr"><is><t>Right</t></is></c></row>` +
		`<row r="2"><c r="A2" t="inlineStr"><is><t>Data</t></is></c>` +
		`<c r="J2" t="inlineStr"><is><t>Far</t></is></c></row>` +
		`</sheetData></worksheet>`)

	lines, _, err := extractWorksheet(worksheet, nil)
	if err != nil {
		t.Fatal(err)
	}
	if lines[0] != "Left\t\t\tRight" || lines[1] != "Data\t\t\t\t\t\t\t\t\tFar" {
		t.Fatalf("sparse columns = %#v", lines)
	}
}

func TestXLSXRowColumnLimitsAndOrientation(t *testing.T) {
	input := officePackageBytes(t, map[string]string{
		"xl/workbook.xml": `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData>` +
			`<row r="1"><c r="A1" t="inlineStr"><is><t>A1</t></is></c><c r="B1" t="inlineStr"><is><t>B1</t></is></c><c r="C1" t="inlineStr"><is><t>C1</t></is></c></row>` +
			`<row r="2"><c r="A2" t="inlineStr"><is><t>A2</t></is></c></row>` +
			`</sheetData><pageSetup paperSize="1" orientation="portrait"/></worksheet>`,
	})
	landscape := true

	pdf, err := ConvertBytesToPDFWithOptions(input, ConversionOptions{
		MaxRows: 1, MaxColumns: 2, Landscape: &landscape,
	})
	assertPDFContains(t, pdf, err, "A1", "B1", "/MediaBox [0 0 792 612]")
	for _, excluded := range []string{"C1", "A2"} {
		if bytes.Contains(pdf, []byte(excluded)) {
			t.Errorf("PDF contains excluded value %q", excluded)
		}
	}
}

func TestXLSXOptionsRejectInvalidValuesAndFormats(t *testing.T) {
	xlsx := officePackageBytes(t, map[string]string{
		"xl/workbook.xml":          `<?xml version="1.0"?><workbook/>`,
		"xl/worksheets/sheet1.xml": `<?xml version="1.0"?><worksheet><sheetData/></worksheet>`,
	})
	if _, err := ConvertBytesToPDFWithOptions(xlsx, ConversionOptions{MaxRows: -1}); !errors.Is(err, ErrInvalidInput) {
		t.Fatalf("MaxRows error = %v, want ErrInvalidInput", err)
	}

	docx := officePackageBytes(t, map[string]string{
		"word/document.xml": `<?xml version="1.0"?><w:document xmlns:w="urn:word"><w:body/></w:document>`,
	})
	if _, err := ConvertBytesToPDFWithOptions(docx, ConversionOptions{MaxColumns: 1}); !errors.Is(err, ErrInvalidInput) {
		t.Fatalf("DOCX MaxColumns error = %v, want ErrInvalidInput", err)
	}
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
	packageEntries := make([]packageEntry, 0, len(entries))
	for name, content := range entries {
		packageEntries = append(packageEntries, packageEntry{name: name, content: content})
	}
	return officePackageEntries(t, packageEntries)
}

type packageEntry struct {
	name    string
	content string
}

func zipFile(name string, compressedSize, uncompressedSize uint64, flags uint16) *zip.File {
	return &zip.File{FileHeader: zip.FileHeader{
		Name:               name,
		Flags:              flags,
		CompressedSize64:   compressedSize,
		UncompressedSize64: uncompressedSize,
	}}
}

func officePackageEntries(t *testing.T, entries []packageEntry) []byte {
	t.Helper()
	var buffer bytes.Buffer
	writer := zip.NewWriter(&buffer)
	for _, entry := range entries {
		file, err := writer.Create(entry.name)
		if err != nil {
			t.Fatal(err)
		}
		if _, err := file.Write([]byte(entry.content)); err != nil {
			t.Fatal(err)
		}
	}
	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}
	return buffer.Bytes()
}

func assertPackageErrorContains(t *testing.T, err error, message string) {
	t.Helper()
	if err == nil {
		t.Fatalf("expected package error containing %q", message)
	}
	if !strings.Contains(err.Error(), message) {
		t.Fatalf("error = %q, want message containing %q", err, message)
	}
}
