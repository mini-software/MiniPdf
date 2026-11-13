package minipdf

import (
	"bytes"
	"compress/zlib"
	"fmt"
	"strconv"
	"strings"
	"unicode"
)

type PDFColor struct {
	Red   float64
	Green float64
	Blue  float64
}

var (
	PDFColorBlack       = PDFColor{}
	PDFColorWhite       = PDFColor{Red: 1, Green: 1, Blue: 1}
	PDFColorLightGray   = PDFColor{Red: 0.92, Green: 0.92, Blue: 0.92}
	PDFColorTableHeader = PDFColor{Red: 0.86, Green: 0.91, Blue: 0.96}
)

type pdfOperation interface {
	appendPDF(*bytes.Buffer, *embeddedFont)
}

type PDFDocument struct {
	pages []*PDFPage
}

// PDFSaveOptions controls PDF serialization.
type PDFSaveOptions struct {
	Compress bool
}

type PDFPage struct {
	Width       float64
	Height      float64
	operations  []pdfOperation
	decorations func(int, int) []pdfOperation
}

func NewPDFDocument() *PDFDocument {
	return &PDFDocument{}
}

func (document *PDFDocument) AddPage(width, height float64) *PDFPage {
	page := &PDFPage{Width: width, Height: height}
	document.pages = append(document.pages, page)
	return page
}

func (page *PDFPage) AddText(text string, x, y, fontSize float64, color PDFColor, bold bool) {
	page.operations = append(page.operations, textOperation{
		text: text, x: x, y: y, fontSize: fontSize, horizontalScale: 100, color: color, bold: bold,
	})
}

func (page *PDFPage) addScaledText(text string, x, y, fontSize, horizontalScale float64, color PDFColor, bold bool) {
	page.operations = append(page.operations, textOperation{
		text: text, x: x, y: y, fontSize: fontSize, horizontalScale: horizontalScale, color: color, bold: bold,
	})
}

func (page *PDFPage) AddRect(x, y, width, height float64, color PDFColor) {
	page.operations = append(page.operations, rectOperation{
		x: x, y: y, width: width, height: height, color: color,
	})
}

func (page *PDFPage) AddLine(x1, y1, x2, y2 float64, color PDFColor, width float64) {
	page.operations = append(page.operations, lineOperation{
		x1: x1, y1: y1, x2: x2, y2: y2, color: color, width: width,
	})
}

type textOperation struct {
	text            string
	x, y, fontSize  float64
	horizontalScale float64
	color           PDFColor
	bold            bool
	italic          bool
	embedded        *embeddedFont
}

func (operation textOperation) appendPDF(buffer *bytes.Buffer, embedded *embeddedFont) {
	if operation.embedded != nil {
		embedded = operation.embedded
	}
	horizontalScale := ""
	if operation.horizontalScale != 0 && operation.horizontalScale != 100 {
		horizontalScale = pdfNumber(operation.horizontalScale) + " Tz "
	}
	if embedded != nil {
		if encoded, ok := embedded.encode(operation.text); ok {
			fmt.Fprintf(buffer, "BT /%s %s Tf %s%s %s %s rg %s %s Td <%s> Tj ET\n",
				embedded.resource,
				pdfNumber(operation.fontSize),
				horizontalScale,
				pdfNumber(operation.color.Red),
				pdfNumber(operation.color.Green),
				pdfNumber(operation.color.Blue),
				pdfNumber(operation.x),
				pdfNumber(operation.y),
				encoded,
			)
			return
		}
	}
	font := "F1"
	if operation.bold {
		font = "F2"
	}
	if operation.italic {
		font = "F3"
		if operation.bold {
			font = "F4"
		}
	}
	fmt.Fprintf(buffer, "BT /%s %s Tf %s%s %s %s rg %s %s Td (%s) Tj ET\n",
		font,
		pdfNumber(operation.fontSize),
		horizontalScale,
		pdfNumber(operation.color.Red),
		pdfNumber(operation.color.Green),
		pdfNumber(operation.color.Blue),
		pdfNumber(operation.x),
		pdfNumber(operation.y),
		escapePDFText(operation.text),
	)
}

type rectOperation struct {
	x, y, width, height float64
	color               PDFColor
}

func (operation rectOperation) appendPDF(buffer *bytes.Buffer, _ *embeddedFont) {
	fmt.Fprintf(buffer, "%s %s %s rg %s %s %s %s re f\n",
		pdfNumber(operation.color.Red),
		pdfNumber(operation.color.Green),
		pdfNumber(operation.color.Blue),
		pdfNumber(operation.x),
		pdfNumber(operation.y),
		pdfNumber(operation.width),
		pdfNumber(operation.height),
	)
}

type lineOperation struct {
	x1, y1, x2, y2 float64
	color          PDFColor
	width          float64
}

func (operation lineOperation) appendPDF(buffer *bytes.Buffer, _ *embeddedFont) {
	fmt.Fprintf(buffer, "%s %s %s RG %s w %s %s m %s %s l S\n",
		pdfNumber(operation.color.Red),
		pdfNumber(operation.color.Green),
		pdfNumber(operation.color.Blue),
		pdfNumber(operation.width),
		pdfNumber(operation.x1),
		pdfNumber(operation.y1),
		pdfNumber(operation.x2),
		pdfNumber(operation.y2),
	)
}

func (document *PDFDocument) Bytes() []byte {
	return document.BytesWithOptions(PDFSaveOptions{})
}

// BytesWithOptions serializes the document with the requested save behavior.
func (document *PDFDocument) BytesWithOptions(options PDFSaveOptions) []byte {
	pages := document.pages
	if len(pages) == 0 {
		pages = []*PDFPage{{Width: PageSizeA4.Width, Height: PageSizeA4.Height}}
	}

	pageCount := len(pages)
	decoratedPages := make([]*PDFPage, pageCount)
	for index, page := range pages {
		clone := *page
		clone.operations = append([]pdfOperation(nil), page.operations...)
		if page.decorations != nil {
			clone.operations = append(clone.operations, page.decorations(index+1, pageCount)...)
		}
		decoratedPages[index] = &clone
	}
	pages = decoratedPages
	objects := make([][]byte, 4)
	objects[0] = []byte("<< /Type /Catalog /Pages 2 0 R >>")
	objects[2] = []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>")
	objects[3] = []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>")
	embedded := prepareEmbeddedFont(pages)
	var fonts []*embeddedFont
	if embedded != nil {
		embedded.resource = "FU1"
		embedded.objectID = appendEmbeddedFontObjects(&objects, embedded)
		fonts = append(fonts, embedded)
	}
	seenFonts := make(map[*embeddedFont]*embeddedFont)
	hasItalic := false
	for _, page := range pages {
		for _, operation := range page.operations {
			text, ok := operation.(textOperation)
			if !ok {
				continue
			}
			hasItalic = hasItalic || text.italic
			if text.embedded == nil {
				continue
			}
			canonical := seenFonts[text.embedded]
			if canonical == nil {
				canonical = text.embedded
				for _, existing := range fonts {
					if existing.name == canonical.name && bytes.Equal(existing.data, canonical.data) {
						canonical = existing
						break
					}
				}
				seenFonts[text.embedded] = canonical
				if canonical == text.embedded {
					fonts = append(fonts, canonical)
				}
			}
			canonical.collect(text.text)
		}
	}
	for index, font := range fonts {
		if font == embedded {
			continue
		}
		for alias, canonical := range seenFonts {
			alias.resource = canonical.resource
			alias.objectID = canonical.objectID
		}
		font.resource = fmt.Sprintf("FU%d", index+1)
		font.objectID = appendEmbeddedFontObjects(&objects, font)
	}
	italicResources := ""
	if hasItalic {
		regular := appendPDFObject(&objects, []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique /Encoding /WinAnsiEncoding >>"))
		bold := appendPDFObject(&objects, []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-BoldOblique /Encoding /WinAnsiEncoding >>"))
		italicResources = fmt.Sprintf(" /F3 %d 0 R /F4 %d 0 R", regular, bold)
	}

	pageObjectStart := len(objects) + 1
	pageReferences := make([]string, pageCount)
	for index := range pages {
		pageReferences[index] = fmt.Sprintf("%d 0 R", pageObjectStart+index*2)
	}
	objects[1] = []byte(fmt.Sprintf("<< /Type /Pages /Count %d /Kids [%s] >>", pageCount, strings.Join(pageReferences, " ")))

	for index, page := range pages {
		pageObjectNumber := pageObjectStart + index*2
		contentObjectNumber := pageObjectNumber + 1
		fontResources := "/F1 3 0 R /F2 4 0 R"
		fontResources += italicResources
		for _, font := range fonts {
			fontResources += fmt.Sprintf(" /%s %d 0 R", font.resource, font.objectID)
		}
		pageObject := []byte(fmt.Sprintf(
			"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 %s %s] /Resources << /Font << %s >> >> /Contents %d 0 R >>",
			pdfNumber(page.Width), pdfNumber(page.Height), fontResources, contentObjectNumber,
		))
		var content bytes.Buffer
		for _, operation := range page.operations {
			operation.appendPDF(&content, embedded)
		}
		contentData := content.Bytes()
		filter := ""
		if options.Compress {
			var compressed bytes.Buffer
			writer := zlib.NewWriter(&compressed)
			_, _ = writer.Write(contentData)
			_ = writer.Close()
			contentData = compressed.Bytes()
			filter = " /Filter /FlateDecode"
		}
		contentObject := fmt.Appendf(nil, "<< /Length %d%s >>\nstream\n", len(contentData), filter)
		contentObject = append(contentObject, contentData...)
		contentObject = append(contentObject, []byte("\nendstream")...)
		objects = append(objects, pageObject, contentObject)
	}

	var output bytes.Buffer
	output.WriteString("%PDF-1.4\n%\xe2\xe3\xcf\xd3\n")
	offsets := make([]int, len(objects)+1)
	for index, object := range objects {
		offsets[index+1] = output.Len()
		fmt.Fprintf(&output, "%d 0 obj\n", index+1)
		output.Write(object)
		output.WriteString("\nendobj\n")
	}
	xrefOffset := output.Len()
	fmt.Fprintf(&output, "xref\n0 %d\n0000000000 65535 f \n", len(objects)+1)
	for index := 1; index < len(offsets); index++ {
		fmt.Fprintf(&output, "%010d 00000 n \n", offsets[index])
	}
	fmt.Fprintf(&output, "trailer\n<< /Size %d /Root 1 0 R >>\nstartxref\n%d\n%%%%EOF\n", len(objects)+1, xrefOffset)
	return output.Bytes()
}

func pdfNumber(value float64) string {
	return strconv.FormatFloat(value, 'f', -1, 64)
}

func escapePDFText(text string) string {
	var escaped strings.Builder
	for _, character := range text {
		switch character {
		case '\\', '(', ')':
			escaped.WriteByte('\\')
			escaped.WriteRune(character)
		case '\n', '\r', '\t':
			escaped.WriteByte(' ')
		default:
			if character <= 0xff && !unicode.IsControl(character) {
				escaped.WriteRune(character)
			} else {
				escaped.WriteByte('?')
			}
		}
	}
	return escaped.String()
}
