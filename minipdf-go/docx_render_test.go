package minipdf

import (
	"bytes"
	"encoding/binary"
	"errors"
	"fmt"
	"image"
	"image/png"
	"math"
	"strings"
	"testing"

	"golang.org/x/image/font/gofont/gobolditalic"
	"golang.org/x/image/font/gofont/goregular"
	"golang.org/x/image/font/sfnt"
)

func docxTestStyles(t *testing.T, styles string) *docxStyles {
	t.Helper()
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	files, err := openOfficePackage(officePackageBytes(t, map[string]string{"word/styles.xml": styles}))
	if err != nil {
		t.Fatal(err)
	}
	s, err := readDOCXStyles(files)
	if err != nil {
		t.Fatal(err)
	}
	return s
}

func TestDOCXTableGridMergedCellsAndInheritedStyles(t *testing.T) {
	s := docxTestStyles(t, `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test" hAnsi="Portable Test"/><sz val="20"/></rPr></rPrDefault></docDefaults><style type="paragraph" default="1" styleId="normal"><rPr><sz val="20"/></rPr></style><style styleId="table"><tblPr><tblBorders><top val="single" sz="8"/><left val="single" sz="8"/><right val="single" sz="8"/><bottom val="single" sz="8"/><insideH val="single" sz="8"/><insideV val="single" sz="8"/></tblBorders><tblCellMar><left w="0"/><right w="0"/></tblCellMar></tblPr></style></styles>`)
	n, err := parseDOCXNode([]byte(`<tbl><tblPr><tblStyle val="table"/><jc val="center"/></tblPr><tblGrid><gridCol w="1200"/><gridCol w="1800"/><gridCol w="1800"/></tblGrid><tr><trPr><trHeight val="400"/></trPr><tc><tcPr><vMerge val="restart"/><vAlign val="center"/></tcPr><p><r><t>Merged</t></r></p></tc><tc><tcPr><gridSpan val="2"/></tcPr><p><r><rPr><highlight val="yellow"/></rPr><t>Wide</t></r></p></tc></tr><tr><trPr><trHeight val="400"/></trPr><tc><tcPr><vMerge/></tcPr><p/></tc><tc><p><r><t>Left</t></r></p></tc><tc><p><r><t>Right</t></r></p></tc></tr></tbl>`))
	if err != nil {
		t.Fatal(err)
	}
	table, err := s.table(n, 300)
	if err != nil {
		t.Fatal(err)
	}
	if table.width != 240 || table.cells[0].rowSpan != 2 || !table.cells[2].continuation || table.cells[1].width != 180 {
		t.Fatalf("incorrect table geometry: %+v", table)
	}
	page := NewPDFDocument().AddPage(400, 500)
	drawDOCXTable(page, table, 30, 50)
	var merged []textOperation
	var highlighted bool
	for _, op := range page.operations {
		switch op := op.(type) {
		case textOperation:
			if op.text == "M" {
				merged = append(merged, op)
			}
		case rectOperation:
			highlighted = highlighted || op.color == (PDFColor{Red: 1, Green: 1})
		}
	}
	if len(merged) != 1 || merged[0].x != 30 || merged[0].y >= 450 || merged[0].y <= 410 || !highlighted {
		t.Fatalf("merged text or highlight not rendered: %+v, highlight=%t", merged, highlighted)
	}
	if table.cells[0].borders["left"].width != 1 {
		t.Fatal("table style border was lost")
	}
}

func TestDOCXRunFontAndParagraphInheritance(t *testing.T) {
	s := docxTestStyles(t, `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test" hAnsi="Portable Test"/><sz val="20"/></rPr></rPrDefault></docDefaults><style styleId="base"><pPr><spacing before="120" after="80"/><jc val="center"/></pPr><rPr><color val="FF0000"/></rPr></style><style styleId="derived"><basedOn val="base"/><rPr><sz val="28"/></rPr></style></styles>`)
	n, err := parseDOCXNode([]byte(`<p><pPr><pStyle val="derived"/><spacing before="40"/></pPr><r><t>A</t></r><r><rPr><sz val="18"/><color val="0000FF"/></rPr><t>Ω</t></r></p>`))
	if err != nil {
		t.Fatal(err)
	}

	p, err := s.layoutParagraph(n, 200, 0)
	if err != nil {
		t.Fatal(err)
	}
	if p.style.before != 2 || p.style.after != 4 || p.style.align != "center" {
		t.Fatalf("paragraph inheritance lost: %+v", p.style)
	}
	g := p.lines[0].glyphs
	if len(g) != 2 || g[0].style.size != 14 || g[1].style.size != 9 || g[1].font == nil || g[0].style.color.Red != 1 || g[1].style.color.Blue != 1 {
		t.Fatalf("run inheritance lost: %+v", g)
	}
	if math.Abs(p.lines[0].width-(measureXLSXFont("A", 14, g[0].font)+measureXLSXFont("Ω", 9, g[1].font))) > .001 {
		t.Fatal("layout did not use glyph advances")
	}
}

func TestDOCXWrappingKeepsLatinWordsTogether(t *testing.T) {
	s := docxTestStyles(t, `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test"/><sz val="20"/></rPr></rPrDefault></docDefaults></styles>`)
	n, err := parseDOCXNode([]byte(`<p><r><t>alpha beta</t></r></p>`))
	if err != nil {
		t.Fatal(err)
	}
	f, err := s.font(s.defaults.run, 'a')
	if err != nil {
		t.Fatal(err)
	}
	p, err := s.layoutParagraph(n, measureXLSXFont("alpha be", 10, f), 0)
	if err != nil {
		t.Fatal(err)
	}
	var lines []string
	for _, line := range p.lines {
		var text strings.Builder
		for _, g := range line.glyphs {
			text.WriteRune(g.r)
		}
		lines = append(lines, text.String())
	}
	if strings.Join(lines, "|") != "alpha |beta" {
		t.Fatalf("Latin word split at arbitrary glyph: %v", lines)
	}
}

func TestDOCXCollectionFontRebasesTables(t *testing.T) {
	collection := make([]byte, 16+len(goregular.TTF))
	copy(collection, "ttcf")
	binary.BigEndian.PutUint32(collection[4:8], 0x10000)
	binary.BigEndian.PutUint32(collection[8:12], 1)
	binary.BigEndian.PutUint32(collection[12:16], 16)
	copy(collection[16:], goregular.TTF)
	n := int(binary.BigEndian.Uint16(collection[20:22]))
	for i := 0; i < n; i++ {
		r := collection[28+i*16 : 44+i*16]
		binary.BigEndian.PutUint32(r[8:12], binary.BigEndian.Uint32(r[8:12])+16)
	}
	standalone, err := docxCollectionFont(collection, 0)
	if err != nil {
		t.Fatal(err)
	}
	f, err := sfnt.Parse(standalone)
	if err != nil {
		t.Fatal(err)
	}
	glyph, err := f.GlyphIndex(nil, 'Ω')
	if err != nil || glyph == 0 {
		t.Fatalf("collection glyph lost: %d %v", glyph, err)
	}
	if _, err = docxCollectionFont(collection, 1); err == nil {
		t.Fatal("out-of-range collection index accepted")
	}
	broken := bytes.Clone(collection)
	binary.BigEndian.PutUint32(broken[12:16], uint32(len(broken)))
	if _, err = docxCollectionFont(broken, 0); err == nil {
		t.Fatal("invalid collection offset accepted")
	}
}

func TestDOCXEquivalentFontInstancesUseCanonicalPDFResource(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	a, err := loadDOCXFont("Portable Test", false, false)
	if err != nil {
		t.Fatal(err)
	}
	b, err := loadDOCXFont("Portable Test", false, false)
	if err != nil {
		t.Fatal(err)
	}
	d := NewPDFDocument()
	p := d.AddPage(200, 200)
	p.operations = append(p.operations, textOperation{text: "A", x: 10, y: 100, fontSize: 12, embedded: a}, textOperation{text: "Ω", x: 30, y: 100, fontSize: 12, embedded: b})
	pdf := d.Bytes()
	if a.resource == "" || b.resource != a.resource || bytes.Count(pdf, []byte("/Subtype /Type0")) != 1 || bytes.Contains(pdf, []byte("BT / ")) {
		t.Fatal("equivalent font instances have unresolved PDF resources")
	}
	encoded, ok := b.encode("Ω")
	if !ok || !bytes.Contains(pdf, []byte("BT /"+a.resource+" 12 Tf 0 0 0 rg 30 100 Td <"+encoded+"> Tj")) || bytes.Contains(pdf, []byte("30 100 Td (?)")) {
		t.Fatal("alias Unicode text did not use the canonical embedded glyph resource")
	}
}

func TestDOCXMissingImageRelationshipReturnsError(t *testing.T) {
	n, err := parseDOCXNode([]byte(`<p><r><drawing><inline><extent cx="127000" cy="127000"/><graphic><blip embed="missing"/></graphic></inline></drawing></r></p>`))
	if err != nil {
		t.Fatal(err)
	}
	files, err := openOfficePackage(officePackageBytes(t, map[string]string{"word/_rels/document.xml.rels": "<Relationships/>"}))
	if err != nil {
		t.Fatal(err)
	}
	_, err = readDOCXImages(files, n)
	if err == nil || !strings.Contains(err.Error(), "relationship") {
		t.Fatalf("missing image was silently discarded: %v", err)
	}
}

func TestDOCXPackagePictureExtentAndTextBoxInsets(t *testing.T) {
	var encoded bytes.Buffer
	if err := png.Encode(&encoded, image.NewRGBA(image.Rect(0, 0, 2, 3))); err != nil {
		t.Fatal(err)
	}
	files, err := openOfficePackage(officePackageBytes(t, map[string]string{"word/_rels/document.xml.rels": `<Relationships><Relationship Id="picture" Target="media/picture.png"/></Relationships>`, "word/media/picture.png": encoded.String()}))
	if err != nil {
		t.Fatal(err)
	}
	n, err := parseDOCXNode([]byte(`<p><r><pict><shape style="margin-left:1in;margin-top:2pt"><textbox inset="3pt,4pt,3pt,4pt"><txbxContent><p><r><drawing><inline><extent cx="254000" cy="381000"/><graphic><blip embed="picture"/></graphic></inline></drawing></r></p></txbxContent></textbox></shape></pict></r></p>`))
	if err != nil {
		t.Fatal(err)
	}
	images, err := readDOCXImages(files, n)
	if err != nil {
		t.Fatal(err)
	}
	if len(images) != 1 || !images[0].floating || images[0].left != 75 || images[0].top != 6 || images[0].op.width != 20 || images[0].op.height != 30 || images[0].op.pixelWidth != 2 {
		t.Fatalf("picture geometry lost: %+v", images)
	}
}

func TestDOCXFELayoutScalesCJKCapableFontMetrics(t *testing.T) {
	data := bytes.Clone(goregular.TTF)
	n := int(binary.BigEndian.Uint16(data[4:6]))
	found := false
	for i := 0; i < n; i++ {
		r := data[12+i*16 : 28+i*16]
		if string(r[:4]) == "OS/2" {
			o := int(binary.BigEndian.Uint32(r[8:12]))
			if binary.BigEndian.Uint32(r[12:16]) < 86 {
				t.Skip("portable test font has no code-page capabilities")
			}
			binary.BigEndian.PutUint32(data[o+78:o+82], 1<<18)
			found = true
		}
	}
	if !found {
		t.Fatal("test font has no OS/2 table")
	}
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("CJK capable", data)
	s := &docxStyles{nodes: map[string]*docxNode{}, fonts: map[string]*embeddedFont{}, theme: map[string]string{}, defaults: docxParagraphStyle{run: docxRunStyle{size: 10, fonts: map[string]string{"ascii": "CJK capable"}}}}
	root, err := parseDOCXNode([]byte(`<p><r><t>A</t></r></p>`))
	if err != nil {
		t.Fatal(err)
	}
	plain, err := s.layoutParagraph(root, 100, 0)
	if err != nil {
		t.Fatal(err)
	}
	s.cjkMetrics = true
	adjusted, err := s.layoutParagraph(root, 100, 0)
	if err != nil {
		t.Fatal(err)
	}
	if math.Abs(adjusted.lines[0].height/plain.lines[0].height-1.27) > .001 || adjusted.lines[0].width != plain.lines[0].width {
		t.Fatal("FE compatibility did not scale vertical metrics independently of glyph width")
	}
}

func TestDOCXEastAsianPunctuationDoesNotStartOrEndWrappedLine(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	f, err := loadDOCXFont("SimSun", false, false)
	if err != nil {
		t.Fatal(err)
	}
	if f == nil {
		t.Skip("requires licensed installed SimSun")
	}
	s := &docxStyles{nodes: map[string]*docxNode{}, fonts: map[string]*embeddedFont{"SimSun/false/false": f}, theme: map[string]string{}, defaults: docxParagraphStyle{run: docxRunStyle{size: 8, fonts: map[string]string{"eastAsia": "SimSun"}}}}
	n, err := parseDOCXNode([]byte(`<p><r><t>机房（扶梯上机舱）</t></r></p>`))
	if err != nil {
		t.Fatal(err)
	}
	p, err := s.layoutParagraph(n, 24.9, 0)
	if err != nil {
		t.Fatal(err)
	}
	var lines []string
	for _, line := range p.lines {
		var text strings.Builder
		for _, g := range line.glyphs {
			text.WriteRune(g.r)
		}
		lines = append(lines, text.String())
	}
	if strings.Join(lines, "|") != "机房|（扶梯|上机|舱）" {
		t.Fatalf("invalid East Asian punctuation break: %v", lines)
	}
}

func TestDOCXUnsupportedMetafilesRetainLegacyConversion(t *testing.T) {
	document := `<document><body><p><r><t>Text preserved</t><drawing><inline><extent cx="127000" cy="127000"/><graphic><blip embed="picture"/></graphic></inline></drawing></r></p><tbl><tblPr/><tblGrid><gridCol w="1200"/></tblGrid><tr><tc><p><r><t>Cell preserved</t></r></p></tc></tr></tbl></body></document>`
	for _, extension := range []string{"emf", "WMF", "png", "jpeg"} {
		t.Run(extension, func(t *testing.T) {
			name := "word/media/image." + extension
			input := officePackageBytes(t, map[string]string{
				"word/document.xml":            document,
				"word/_rels/document.xml.rels": `<Relationships><Relationship Id="picture" Target="media/image.` + extension + `"/></Relationships>`,
				name:                           "not a raster image",
			})
			files, err := openOfficePackage(input)
			if err != nil {
				t.Fatal(err)
			}
			_, supported, err := renderDOCXTables(files, []byte(document), PageSizeA4, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
			if extension == "png" || extension == "jpeg" {
				if !supported || err == nil || !strings.Contains(err.Error(), "DOCX image") {
					t.Fatalf("malformed raster image was swallowed: supported=%t, err=%v", supported, err)
				}
				return
			}
			if supported || err != nil {
				t.Fatalf("metafile should retain legacy conversion: supported=%t, err=%v", supported, err)
			}
			pdf, err := ConvertBytesToPDF(input)
			assertPDFContains(t, pdf, err, "Text preserved", "Cell preserved")
		})
	}
}

func TestDOCXStructuredPaginationRetainsLegacyConversion(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	const table = `<tbl><tblPr/><tblGrid><gridCol w="1200"/></tblGrid><tr><trPr><trHeight val="1200"/></trPr><tc><p><r><t>Table</t></r></p></tc></tr></tbl>`
	for _, test := range []struct {
		name, tail string
		supported  bool
	}{
		{"table overflow", table, false},
		{"paragraph overflow", `<p><pPr><spacing before="400"/></pPr><r><t>Tail</t></r></p>`, false},
		{"single-page flow", `<p><r><t>Tail</t></r></p>`, true},
	} {
		t.Run(test.name, func(t *testing.T) {
			data := []byte(`<document><body>` + table + test.tail + `</body></document>`)
			input := officePackageBytes(t, map[string]string{
				"word/document.xml": string(data),
				"word/styles.xml":   `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test" hAnsi="Portable Test"/><sz val="20"/></rPr></rPrDefault></docDefaults></styles>`,
			})
			files, err := openOfficePackage(input)
			if err != nil {
				t.Fatal(err)
			}
			size := PageSize{Width: 200, Height: 100}
			margins := Margins{Left: 10, Top: 10, Right: 10, Bottom: 10}
			options := ConversionOptions{PageSize: &size, Margins: &margins}
			_, supported, err := renderDOCXTables(files, data, size, margins, options)
			if err != nil || supported != test.supported {
				t.Fatalf("single-page capability: supported=%t, err=%v", supported, err)
			}
			if test.supported {
				return
			}
			pages, sourceSize, sourceMargins, err := extractDOCX(data)
			if err != nil {
				t.Fatal(err)
			}
			var textPages []textPage
			for _, lines := range pages {
				textPages = append(textPages, textPage{lines: lines, size: sourceSize, margins: sourceMargins})
			}
			want := renderTextPages(textPages, options)
			got, err := ConvertBytesToPDFWithOptions(input, options)
			if err != nil || !bytes.Equal(got, want) {
				t.Fatalf("pagination fallback changed legacy PDF: %v", err)
			}
		})
	}
}

func TestDOCXUnsupportedLayoutKeepsTextConverter(t *testing.T) {
	data := []byte(`<document><body><p><r><t>First</t><br type="page"/><t>Second</t></r></p><tbl><tblPr/><tblGrid><gridCol w="1200"/></tblGrid><tr><tc><p/></tc></tr></tbl></body></document>`)
	_, supported, err := renderDOCXTables(nil, data, PageSizeA4, Margins{Left: 40, Right: 40, Top: 40, Bottom: 40}, ConversionOptions{})
	if err != nil || supported {
		t.Fatalf("unsupported explicit pagination was not retained on legacy path: %t, %v", supported, err)
	}
}

func TestDOCXDashRenderingHasBoundedOperations(t *testing.T) {
	for _, style := range []string{"dotted", "dotDash", "dashed"} {
		page := NewPDFDocument().AddPage(612, 792)
		drawDOCXBorder(page, docxBorder{style: style, width: .25}, 10, 10, float64(2147483647)/20, 10)
		if len(page.operations) != 1 {
			t.Fatalf("%s retained %d operations for off-page geometry", style, len(page.operations))
		}
		var stream bytes.Buffer
		page.operations[0].appendPDF(&stream, nil)
		if !strings.Contains(stream.String(), "] 0 d ") || bytes.Count(stream.Bytes(), []byte(" l S")) != 1 || !strings.HasSuffix(stream.String(), "Q\n") {
			t.Fatalf("dash stroke is not a single isolated native PDF operation: %s", &stream)
		}
	}
	s := docxTestStyles(t, docxReviewStyles)
	data := []byte(`<document><body><tbl><tblPr><tblBorders><top val="dashed" sz="2"/></tblBorders></tblPr><tblGrid><gridCol w="2147483647"/></tblGrid><tr><tc><p/></tc></tr></tbl></body></document>`)
	files, err := openOfficePackage(officePackageBytes(t, map[string]string{"word/styles.xml": docxReviewStyles}))
	if err != nil {
		t.Fatal(err)
	}
	root, err := parseDOCXNode(data)
	if err != nil {
		t.Fatal(err)
	}
	table, err := s.table(root.child("body").child("tbl"), 500)
	if err != nil {
		t.Fatal(err)
	}
	page := NewPDFDocument().AddPage(612, 792)
	drawDOCXTable(page, table, 40, 40)
	if len(page.operations) != 1 {
		t.Fatalf("full extreme-grid table retained %d operations", len(page.operations))
	}
	_, supported, err := renderDOCXTables(files, data, PageSizeA4, Margins{Left: 40, Right: 40, Top: 40, Bottom: 40}, ConversionOptions{})
	if supported || err != nil {
		t.Fatalf("table outside physical page did not decline native layout: %t %v", supported, err)
	}
}

func TestDOCXSynthesizesOnlyMissingFontEmphasis(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	for _, native := range []bool{false, true} {
		if native {
			RegisterFont("Portable Test Bold Italic", gobolditalic.TTF)
		}
		f, err := loadDOCXFont("Portable Test", true, true)
		if err != nil || f == nil {
			t.Fatalf("styled registered font unavailable: %v", err)
		}
		if f.nativeBold != native || f.nativeItalic != native {
			t.Fatalf("native emphasis incorrectly identified: %+v", f)
		}
		f.collect("Ω")
		f.resource = "FU1"
		var stream bytes.Buffer
		op := textOperation{text: "Ω", x: 10, y: 20, fontSize: 12, bold: true, italic: true, color: PDFColor{Red: 1}, embedded: f, synthesize: true}
		op.appendPDF(&stream, nil)
		if !strings.Contains(stream.String(), "/FU1 12 Tf") || strings.Contains(stream.String(), "(?)") {
			t.Fatal("emphasis lost Unicode embedding")
		}
		if native {
			if strings.Contains(stream.String(), "2 Tr") || strings.Contains(stream.String(), " Tm") {
				t.Fatal("native bold italic face was double-synthesized")
			}
		} else if !strings.Contains(stream.String(), "2 Tr") || !strings.Contains(stream.String(), "0.21256 1 10 20 Tm") || !strings.Contains(stream.String(), "1 0 0 RG") || !strings.HasSuffix(stream.String(), "Q\n") {
			t.Fatalf("missing face was not synthesized with isolated stroke/slant: %s", &stream)
		}
	}
}

func TestDOCXHangulUsesEastAsianAndKoreanThemeFonts(t *testing.T) {
	data := bytes.Clone(goregular.TTF)
	original, err := sfnt.Parse(data)
	if err != nil {
		t.Fatal(err)
	}
	glyph, err := original.GlyphIndex(nil, 'Ω')
	if err != nil {
		t.Fatal(err)
	}
	// Supply a synthetic cmap for the portable font; no host Korean font is required.
	cmap := make([]byte, 40)
	binary.BigEndian.PutUint16(cmap[2:4], 1)
	binary.BigEndian.PutUint16(cmap[4:6], 3)
	binary.BigEndian.PutUint16(cmap[6:8], 10)
	binary.BigEndian.PutUint32(cmap[8:12], 12)
	binary.BigEndian.PutUint16(cmap[12:14], 12)
	binary.BigEndian.PutUint32(cmap[16:20], 28)
	binary.BigEndian.PutUint32(cmap[24:28], 1)
	binary.BigEndian.PutUint32(cmap[28:32], 0xac00)
	binary.BigEndian.PutUint32(cmap[32:36], 0xac00)
	binary.BigEndian.PutUint32(cmap[36:40], uint32(glyph))
	for i, n := 0, int(binary.BigEndian.Uint16(data[4:6])); i < n; i++ {
		r := data[12+i*16 : 28+i*16]
		if string(r[:4]) == "cmap" {
			binary.BigEndian.PutUint32(r[8:12], uint32(len(data)))
			binary.BigEndian.PutUint32(r[12:16], uint32(len(cmap)))
		}
	}
	data = append(data, cmap...)
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Korean Test", data)
	s := &docxStyles{fonts: map[string]*embeddedFont{}, theme: map[string]string{"minor:Hang": "Korean Test"}}
	for _, fonts := range []map[string]string{{"eastAsia": "Korean Test", "hAnsi": "Wrong font"}, {"eastAsiaTheme": "minorEastAsia", "hAnsi": "Wrong font"}} {
		f, err := s.font(docxRunStyle{fonts: fonts, lang: "ko-KR"}, '가')
		if err != nil || f == nil || !bytes.Equal(f.data, data) || !f.canEncode("가") {
			t.Fatalf("Hangul did not select the East Asian Korean font: %v", err)
		}
	}
}

func TestDOCXInteriorBottomBordersAndMergeFinalEdge(t *testing.T) {
	s := docxTestStyles(t, `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test"/></rPr></rPrDefault></docDefaults></styles>`)
	for _, inside := range []string{"none", "dashed"} {
		xml := `<tbl><tblPr><tblBorders><bottom val="single" sz="16"/><insideH val="` + inside + `" sz="4"/></tblBorders></tblPr><tblGrid><gridCol w="1200"/><gridCol w="1200"/></tblGrid><tr><tc><tcPr><vMerge val="restart"/></tcPr><p/></tc><tc><p/></tc></tr><tr><tc><tcPr><vMerge/></tcPr><p/></tc><tc><tcPr><tcBorders><top val="dotted" sz="6"/></tcBorders></tcPr><p/></tc></tr></tbl>`
		n, err := parseDOCXNode([]byte(xml))
		if err != nil {
			t.Fatal(err)
		}
		table, err := s.table(n, 300)
		if err != nil {
			t.Fatal(err)
		}
		if table.cells[1].borders["bottom"].style != inside || table.cells[0].borders["bottom"].style != "single" || table.cells[3].borders["bottom"].width != 2 || table.cells[3].borders["top"].style != "dotted" {
			t.Fatalf("interior/outer/override/merge edges lost for %s", inside)
		}
	}
}

const docxReviewTable = `<tbl><tblPr/><tblGrid><gridCol w="1200"/></tblGrid><tr><tc><p><r><t>Cell</t></r></p></tc></tr></tbl>`
const docxReviewStyles = `<styles><docDefaults><rPrDefault><rPr><rFonts ascii="Portable Test" hAnsi="Portable Test"/></rPr></rPrDefault></docDefaults></styles>`

func docxReviewPackage(t *testing.T, body string, parts map[string]string) (officePackage, []byte, []byte) {
	t.Helper()
	if parts == nil {
		parts = map[string]string{}
	}
	data := []byte(`<document><body>` + body + `</body></document>`)
	parts["word/document.xml"] = string(data)
	parts["word/styles.xml"] = docxReviewStyles
	input := officePackageBytes(t, parts)
	files, err := openOfficePackage(input)
	if err != nil {
		t.Fatal(err)
	}
	return files, data, input
}

func TestDOCXUnsupportedReviewLayoutsRetainLegacy(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	var encoded bytes.Buffer
	if err := png.Encode(&encoded, image.NewRGBA(image.Rect(0, 0, 2, 3))); err != nil {
		t.Fatal(err)
	}
	picture := `<drawing><inline><extent cx="254000" cy="381000"/><graphic><blip embed="picture"/></graphic></inline></drawing>`
	for _, test := range []struct{ name, body string }{
		{"gridBefore", strings.Replace(docxReviewTable, "<tr>", `<tr><trPr><gridBefore val="1"/></trPr>`, 1)},
		{"gridAfter", strings.Replace(docxReviewTable, "<tr>", `<tr><trPr><gridAfter val="1"/></trPr>`, 1)},
		{"span", strings.Replace(docxReviewTable, "<tc>", `<tc><tcPr><gridSpan val="2"/></tcPr>`, 1)},
		{"merge", strings.Replace(docxReviewTable, "<tc>", `<tc><tcPr><vMerge/></tcPr>`, 1)},
		{"text box", `<p><r><pict><shape><textbox><txbxContent><p><r><t>Box text</t></r></p></txbxContent></textbox></shape></pict></r></p>` + docxReviewTable},
		{"mixed inline text", `<p><r><t>Before</t>` + picture + `</r></p>` + docxReviewTable},
		{"multiple inline images", `<p><r>` + picture + picture + `</r></p>` + docxReviewTable},
		{"inline break", `<p><r><br/>` + picture + `</r></p>` + docxReviewTable},
		{"linked image", `<p><r>` + strings.Replace(picture, `embed="picture"`, `link="external"`, 1) + `</r></p>` + docxReviewTable},
	} {
		t.Run(test.name, func(t *testing.T) {
			files, data, input := docxReviewPackage(t, test.body, map[string]string{"word/_rels/document.xml.rels": `<Relationships><Relationship Id="picture" Target="/word/media/image.png"/></Relationships>`, "word/media/image.png": encoded.String()})
			_, supported, err := renderDOCXTables(files, data, PageSizeA4, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
			if supported || err != nil {
				t.Fatalf("layout did not fall back: supported=%t err=%v", supported, err)
			}
			pdf, err := ConvertBytesToPDF(input)
			if err != nil {
				t.Fatal(err)
			}
			pages, size, margins, err := extractDOCX(data)
			if err != nil {
				t.Fatal(err)
			}
			var legacy []textPage
			var text []string
			for _, lines := range pages {
				legacy = append(legacy, textPage{lines: lines, size: size, margins: margins})
				text = append(text, lines...)
			}
			if !bytes.Equal(pdf, renderTextPages(legacy, ConversionOptions{})) || !strings.Contains(strings.Join(text, "\n"), "Cell") {
				t.Fatal("fallback changed legacy text conversion")
			}
			if test.name == "text box" && !strings.Contains(strings.Join(text, "\n"), "Box text") {
				t.Fatal("fallback lost text-box text")
			}
		})
	}
}

func TestDOCXImageOnlyAlignmentAndAbsoluteTargets(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	var encoded bytes.Buffer
	if err := png.Encode(&encoded, image.NewRGBA(image.Rect(0, 0, 2, 3))); err != nil {
		t.Fatal(err)
	}
	for _, test := range []struct {
		align string
		x     float64
	}{{"left", 40}, {"center", 90}, {"right", 140}} {
		body := `<p><pPr><jc val="` + test.align + `"/><spacing before="100" after="100"/></pPr><r><drawing><inline><extent cx="254000" cy="381000"/><graphic><blip embed="picture"/></graphic></inline></drawing></r></p>` + docxReviewTable
		files, data, _ := docxReviewPackage(t, body, map[string]string{"word/_rels/document.xml.rels": `<Relationships><Relationship Id="picture" Target="/word/media/image.png"/></Relationships>`, "word/media/image.png": encoded.String()})
		pdf, supported, err := renderDOCXTables(files, data, PageSize{Width: 200, Height: 300}, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
		if err != nil || !supported || !bytes.Contains(pdf, []byte(fmt.Sprintf("20 0 0 30 %s 225 cm", pdfNumber(test.x)))) {
			t.Fatalf("%s image lost absolute target/spacing/alignment: supported=%t err=%v", test.align, supported, err)
		}
	}
}

func TestDOCXGridTypeControlsLineSnapping(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	RegisterFont("Portable Test", goregular.TTF)
	var plain []byte
	for _, typ := range []string{"", "default", "snapToChars", "lines", "linesAndChars"} {
		files, data, _ := docxReviewPackage(t, `<p><r><t>Text</t></r></p>`+docxReviewTable+`<sectPr><docGrid type="`+typ+`" linePitch="720"/></sectPr>`, nil)
		pdf, supported, err := renderDOCXTables(files, data, PageSizeA4, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
		if err != nil || !supported {
			t.Fatalf("grid %s failed: %v", typ, err)
		}
		if typ == "" {
			plain = pdf
		}
		if (typ == "default" || typ == "snapToChars") && !bytes.Equal(pdf, plain) {
			t.Fatalf("inactive grid %s altered paragraph layout", typ)
		}
		if (typ == "lines" || typ == "linesAndChars") && bytes.Equal(pdf, plain) {
			t.Fatalf("active grid %s did not snap lines", typ)
		}
	}
}

func TestDOCXUnavailableFontsFallbackButMissingGlyphsFail(t *testing.T) {
	ClearRegisteredFonts()
	t.Cleanup(ClearRegisteredFonts)
	t.Setenv("WINDIR", "")
	for _, body := range []string{`<p><r><t>é</t></r></p>` + docxReviewTable, strings.Replace(docxReviewTable, "Cell", "é", 1)} {
		files, data, input := docxReviewPackage(t, body, nil)
		_, supported, err := renderDOCXTables(files, data, PageSizeA4, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
		if err != nil || supported {
			t.Fatalf("missing environment font did not fall back: %t %v", supported, err)
		}
		if _, err := ConvertBytesToPDF(input); err != nil {
			t.Fatalf("legacy font fallback failed: %v", err)
		}
	}
	RegisterFont("Portable Test", goregular.TTF)
	files, data, _ := docxReviewPackage(t, strings.Replace(docxReviewTable, "Cell", "\U0001f600", 1), nil)
	_, supported, err := renderDOCXTables(files, data, PageSizeA4, Margins{Left: 40, Top: 40, Right: 40, Bottom: 40}, ConversionOptions{})
	if !supported || err == nil || errors.Is(err, errDOCXFontUnavailable) || !strings.Contains(err.Error(), "has no glyph") {
		t.Fatalf("present font's missing glyph was silently downgraded: %t %v", supported, err)
	}
}
