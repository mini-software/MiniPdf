package minipdf

import (
	"bytes"
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
	appendPDF(*bytes.Buffer)
}

type PDFDocument struct {
	pages []*PDFPage
}

type PDFPage struct {
	Width      float64
	Height     float64
	operations []pdfOperation
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
		text: text, x: x, y: y, fontSize: fontSize, color: color, bold: bold,
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
	text           string
	x, y, fontSize float64
	color          PDFColor
	bold           bool
}

func (operation textOperation) appendPDF(buffer *bytes.Buffer) {
	font := "F1"
	if operation.bold {
		font = "F2"
	}
	fmt.Fprintf(buffer, "BT /%s %s Tf %s %s %s rg %s %s Td (%s) Tj ET\n",
		font,
		pdfNumber(operation.fontSize),
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

func (operation rectOperation) appendPDF(buffer *bytes.Buffer) {
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

func (operation lineOperation) appendPDF(buffer *bytes.Buffer) {
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
	pages := document.pages
	if len(pages) == 0 {
		pages = []*PDFPage{{Width: PageSizeA4.Width, Height: PageSizeA4.Height}}
	}

	pageCount := len(pages)
	objects := make([][]byte, 4+pageCount*2)
	objects[0] = []byte("<< /Type /Catalog /Pages 2 0 R >>")

	pageReferences := make([]string, pageCount)
	for index := range pages {
		pageReferences[index] = fmt.Sprintf("%d 0 R", 5+index*2)
	}
	objects[1] = []byte(fmt.Sprintf("<< /Type /Pages /Count %d /Kids [%s] >>", pageCount, strings.Join(pageReferences, " ")))
	objects[2] = []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>")
	objects[3] = []byte("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>")

	for index, page := range pages {
		pageObjectNumber := 5 + index*2
		contentObjectNumber := pageObjectNumber + 1
		objects[pageObjectNumber-1] = []byte(fmt.Sprintf(
			"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 %s %s] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents %d 0 R >>",
			pdfNumber(page.Width), pdfNumber(page.Height), contentObjectNumber,
		))
		var content bytes.Buffer
		for _, operation := range page.operations {
			operation.appendPDF(&content)
		}
		objects[contentObjectNumber-1] = []byte(fmt.Sprintf("<< /Length %d >>\nstream\n%sendstream", content.Len(), content.String()))
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
