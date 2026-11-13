package minipdf

import (
	"bytes"
	"image"
	"image/color"
	"image/png"
	"math"
	"strings"
	"testing"

	"golang.org/x/image/font/gofont/gobold"
	"golang.org/x/image/font/gofont/goregular"
)

func TestPrintedXLSXNumberFormats(t *testing.T) {
	for _, test := range []struct {
		value    string
		id       int
		code     string
		date1904 bool
		want     string
	}{
		{"46046", 164, `dd\-mmm\-yyyy`, false, "24-Jan-2026"},
		{"1", 164, "yyyy-mm-dd", false, "1900-01-01"},
		{"0", 164, "dd-mmm-yyyy", false, "00-Jan-1900"},
		{"1", 164, "dddd", false, "Sunday"},
		{"59", 164, "yyyy-mm-dd", false, "1900-02-28"},
		{"60", 164, "yyyy-mm-dd", false, "1900-02-29"},
		{"61", 164, "yyyy-mm-dd", false, "1900-03-01"},
		{"0", 164, "yyyy-mm-dd", true, "1904-01-01"},
		{"46087", 14, "", false, "3/6/2026"},
		{"61859", 3, "", false, "61,859"},
		{"-1234.567", 4, "", false, "-1,234.57"},
		{"1.25", 165, "0.0", false, "1.3"},
		{"1.5", 165, "0.##", false, "1.5"},
		{"0.125", 9, "", false, "13%"},
		{"132.22999999999999", 0, "", false, "132.23"},
		{"123.4", 164, "[Red]0.00", false, "123.4"},
		{"-123.4", 164, "[Red]0.00", false, "-123.4"},
		{"123.4", 164, `0.0"days"`, false, "123.4"},
	} {
		value, err := formatPrintedXLSXValue(test.value, test.id, test.code, test.date1904)
		if err != nil || value != test.want {
			t.Errorf("%q format %d/%q date1904=%v = %q, %v; want %q", test.value, test.id, test.code, test.date1904, value, err, test.want)
		}
	}
	for _, value := range []string{"invalid", "NaN", "Inf"} {
		if _, err := formatPrintedXLSXValue(value, 0, "", false); err == nil {
			t.Errorf("invalid numeric value %q accepted", value)
		}
		if _, err := formatPrintedXLSXValue("2957004", 164, "yyyy-mm-dd", true); err == nil {
			t.Fatal("1904 date beyond year 9999 accepted")
		}
	}
}

func TestXLSXPrintCenteringIncludesRepeatedColumns(t *testing.T) {
	grid, err := readXLSXGrid([]byte(`<worksheet><printOptions horizontalCentered="1" verticalCentered="1"/><sheetData>`+
		`<row r="1"><c r="A1" t="inlineStr"><is><t>Title</t></is></c><c r="B1" t="inlineStr"><is><t>Body</t></is></c></row>`+
		`</sheetData></worksheet>`), nil)
	if err != nil {
		t.Fatal(err)
	}
	document := NewPDFDocument()
	settings := xlsxPrintSettings{areas: []xlsxMerge{{startRow: 1, endRow: 1, startColumn: 2, endColumn: 2}}, columns: xlsxRange{start: 1, end: 1}}
	if err := renderXLSXPrintGrid(document, grid, nil, settings, ConversionOptions{}); err != nil {
		t.Fatal(err)
	}
	width := grid.columnWidth(1) + grid.columnWidth(2)
	wantX := (grid.pageSize.Width-width)/2 + 1.5
	for _, operation := range document.pages[0].operations {
		text, ok := operation.(textOperation)
		if ok && text.text == "Title" {
			if math.Abs(text.x-wantX) > 1e-6 {
				t.Errorf("repeated column x = %g, want %g", text.x, wantX)
			}
			if text.y < grid.pageSize.Height/2-20 || text.y > grid.pageSize.Height/2+20 {
				t.Errorf("vertical centering y = %g", text.y)
			}
			return
		}
	}
	t.Fatal("missing repeated column")
}

func TestXLSXFooterUsesFinalDocumentPageCountWithoutMutation(t *testing.T) {
	grid, err := readXLSXGrid([]byte(`<worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>Body</t></is></c></row></sheetData>`+
		`<headerFooter><oddFooter>&amp;L&amp;6 Left &amp;&amp; literal&#10;Second&amp;CPage &amp;P of &amp;N&amp;R&amp;A</oddFooter></headerFooter></worksheet>`), nil)
	if err != nil {
		t.Fatal(err)
	}
	if err := grid.prepareFooters(nil, "Report"); err != nil {
		t.Fatal(err)
	}
	doc := NewPDFDocument()
	settings := xlsxPrintSettings{areas: []xlsxMerge{{startRow: 1, endRow: 1, startColumn: 1, endColumn: 1}}}
	for i := 0; i < 2; i++ {
		if err := renderXLSXPrintGrid(doc, grid, nil, settings, ConversionOptions{}); err != nil {
			t.Fatal(err)
		}
	}
	count := len(doc.pages[0].operations)
	pdf := doc.Bytes()
	for _, text := range []string{"Left & literal", "Second", "Report"} {
		if !bytes.Contains(pdf, []byte(text)) {
			t.Errorf("missing footer %q", text)
		}
	}
	for page := 1; page <= 2; page++ {
		operations := doc.pages[page-1].decorations(page, 2)
		var text strings.Builder
		for _, operation := range operations {
			text.WriteString(operation.(textOperation).text)
		}
		if !strings.Contains(text.String(), "Page "+string(rune('0'+page))+" of 2") {
			t.Errorf("footer = %q", text.String())
		}
	}
	if len(doc.pages[0].operations) != count || !bytes.Equal(pdf, doc.Bytes()) {
		t.Fatal("PDF serialization mutated or duplicated footers")
	}
}

func TestXLSXFooterFirstEvenAndUnscaledVariants(t *testing.T) {
	grid, err := readXLSXGrid([]byte(`<worksheet><headerFooter differentFirst="1" differentOddEven="1" scaleWithDoc="0">`+
		`<oddFooter>&amp;6Odd</oddFooter><evenFooter>&amp;6Even</evenFooter><firstFooter>&amp;6First</firstFooter>`+
		`</headerFooter></worksheet>`), nil)
	if err != nil {
		t.Fatal(err)
	}
	if err := grid.prepareFooters(nil, "Sheet"); err != nil {
		t.Fatal(err)
	}
	for _, test := range []struct {
		local, global int
		want          string
	}{{1, 3, "First"}, {2, 4, "Even"}, {3, 5, "Odd"}} {
		operations := xlsxFooterDecoration(grid, 0.5, test.local)(test.global, 5)
		if len(operations) != 1 {
			t.Fatal(operations)
		}
		text := operations[0].(textOperation)
		if text.text != test.want || text.fontSize != 6 {
			t.Errorf("footer = %#v", text)
		}
	}
	if _, err := parseXLSXFooter(`&"Unclosed`, xlsxFont{size: 11}, ""); err == nil {
		t.Fatal("unclosed footer font accepted")
	}
}

func TestXLSXPictureCropRelationshipAndTitleRepeat(t *testing.T) {
	source := image.NewRGBA(image.Rect(0, 0, 8, 6))
	for y := 0; y < 6; y++ {
		for x := 0; x < 8; x++ {
			source.Set(x, y, color.RGBA{R: uint8(x * 20), G: uint8(y * 30), A: 255})
		}
	}
	var encoded bytes.Buffer
	if err := png.Encode(&encoded, source); err != nil {
		t.Fatal(err)
	}
	entries := map[string]string{
		"xl/worksheets/sheet1.xml":            `<worksheet/>`,
		"xl/worksheets/_rels/sheet1.xml.rels": `<Relationships><Relationship Type="urn:office/drawing" Target="../drawings/drawing1.xml"/></Relationships>`,
		"xl/drawings/drawing1.xml": `<wsDr><oneCellAnchor><from><col>0</col><row>0</row></from><ext cx="254000" cy="127000"/>` +
			`<pic><blipFill><blip embed="image"/><srcRect l="25000" r="25000"/></blipFill></pic><clientData/></oneCellAnchor></wsDr>`,
		"xl/drawings/_rels/drawing1.xml.rels": `<Relationships><Relationship Id="image" Type="urn:office/image" Target="../media/logo.png"/></Relationships>`,
		"xl/media/logo.png":                   string(encoded.Bytes()),
	}
	grid, err := readXLSXGrid([]byte(`<worksheet><sheetData><row r="1"/><row r="2"/><row r="3"/></sheetData>`+
		`<rowBreaks><brk id="2" man="1"/></rowBreaks></worksheet>`), nil)
	if err != nil {
		t.Fatal(err)
	}
	pictures, err := readXLSXPictures(xlsxStylePackage(t, entries), "xl/worksheets/sheet1.xml", grid)
	if err != nil {
		t.Fatal(err)
	}
	if len(pictures) != 1 || pictures[0].image.pixelWidth != 4 || pictures[0].image.pixelHeight != 6 {
		t.Fatalf("pictures = %#v", pictures)
	}
	grid.pictures = pictures
	doc := NewPDFDocument()
	settings := xlsxPrintSettings{areas: []xlsxMerge{{startRow: 1, endRow: 3, startColumn: 1, endColumn: 1}}, rows: xlsxRange{start: 1, end: 1}}
	if err := renderXLSXPrintGrid(doc, grid, nil, settings, ConversionOptions{}); err != nil {
		t.Fatal(err)
	}
	if len(doc.pages) != 2 {
		t.Fatal("missing page break")
	}
	for _, page := range doc.pages {
		images := 0
		for _, operation := range page.operations {
			if _, ok := operation.(imageOperation); ok {
				images++
			}
		}
		if images != 1 {
			t.Errorf("repeated logo count = %d, want 1", images)
		}
	}
	if !bytes.Contains(doc.Bytes(), []byte("/ASCIIHexDecode /FlateDecode")) {
		t.Fatal("missing inline raster image")
	}
	delete(entries, "xl/drawings/_rels/drawing1.xml.rels")
	if _, err := readXLSXPictures(xlsxStylePackage(t, entries), "xl/worksheets/sheet1.xml", grid); err == nil {
		t.Fatal("missing picture relationship part accepted")
	}
}

func TestPrintedXLSXNamedFontVariantsAndWidths(t *testing.T) {
	ClearRegisteredFonts()
	defer ClearRegisteredFonts()
	RegisterFont("Unit Font", goregular.TTF)
	RegisterFont("Unit Font Bold", gobold.TTF)
	regular, err := readXLSXFont("Unit Font", false, false)
	if err != nil {
		t.Fatal(err)
	}
	bold, err := readXLSXFont("Unit Font", true, false)
	if err != nil {
		t.Fatal(err)
	}
	if regular == nil || bold == nil || bytes.Equal(regular.data, bold.data) {
		t.Fatal("font variants were not resolved independently")
	}
	width := measureXLSXFont("00", 10, regular)
	if math.Abs(width-2*measureXLSXFont("0", 10, regular)) > 1e-9 {
		t.Fatal("font advance widths are inconsistent")
	}
	doc := NewPDFDocument()
	page := doc.AddPage(200, 200)
	renderXLSXCellText(page, xlsxCell{value: "Regular"}, 0, 100, 100, 20, xlsxStyle{font: xlsxFont{size: 10, embedded: regular}})
	renderXLSXCellText(page, xlsxCell{value: "Bold"}, 0, 70, 100, 20, xlsxStyle{font: xlsxFont{size: 10, bold: true, embedded: bold}})
	pdf := doc.Bytes()
	if bytes.Count(pdf, []byte("/Subtype /Type0")) != 2 {
		t.Fatal("PDF did not embed both named font variants")
	}
	if !bytes.Contains(pdf, []byte("/FU1")) || !bytes.Contains(pdf, []byte("/FU2")) {
		t.Fatal("missing font resources")
	}
	if !bytes.Equal(pdf, doc.Bytes()) {
		t.Fatal("font serialization is not repeatable")
	}
	shrunk := doc.AddPage(200, 200)
	renderXLSXCellText(shrunk, xlsxCell{value: "0000000000"}, 0, 0, 20, 20, xlsxStyle{shrink: true, font: xlsxFont{size: 10, embedded: regular}})
	text := shrunk.operations[0].(textOperation)
	if text.fontSize >= 10 || measureXLSXFont(text.text, text.fontSize, regular) > 16+1e-8 {
		t.Fatal("shrink-to-fit does not use the selected font's advance widths")
	}
	if math.Abs(text.y-text.fontSize*regular.descent) > 1e-8 {
		t.Fatal("bottom alignment ignored actual font descent")
	}
}

func TestXLSXDoubleBordersKeepTwoSeparateStrokes(t *testing.T) {
	page := NewPDFDocument().AddPage(100, 100)
	renderXLSXCell(page, xlsxCell{}, 10, 10, 20, 20, xlsxStyle{border: xlsxBorder{bottom: 1.5, bottomDouble: true}}, false)
	if len(page.operations) != 2 {
		t.Fatal("double border was rendered as a single thick line")
	}
	first := page.operations[0].(lineOperation)
	second := page.operations[1].(lineOperation)
	if first.y1 == second.y1 || first.width != 0.5 || second.width != 0.5 {
		t.Fatalf("double border = %#v / %#v", first, second)
	}
}
