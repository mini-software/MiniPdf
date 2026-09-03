package minipdf

import (
	"bytes"
	"regexp"
	"strconv"
	"testing"
)

func TestPDFDocumentWritesValidEnvelope(t *testing.T) {
	document := NewPDFDocument()
	page := document.AddPage(PageSizeA4.Width, PageSizeA4.Height)
	page.AddText("Hello from Go MiniPdf", 72, 760, 14, PDFColorBlack, false)

	pdf := document.Bytes()
	if !bytes.HasPrefix(pdf, []byte("%PDF-1.4")) {
		t.Fatal("PDF header is missing")
	}
	if !bytes.HasSuffix(pdf, []byte("%%EOF\n")) {
		t.Fatal("PDF EOF marker is missing")
	}
}

func TestPDFStreamLengthsAreExact(t *testing.T) {
	document := NewPDFDocument()
	document.AddPage(300, 400).AddText("Hello", 20, 350, 12, PDFColorBlack, false)
	pdf := document.Bytes()

	pattern := regexp.MustCompile(`/Length ([0-9]+) >>\nstream\n`)
	matches := pattern.FindAllSubmatchIndex(pdf, -1)
	if len(matches) == 0 {
		t.Fatal("no PDF streams found")
	}
	for _, match := range matches {
		declared, err := strconv.Atoi(string(pdf[match[2]:match[3]]))
		if err != nil {
			t.Fatal(err)
		}
		streamStart := match[1]
		streamEndOffset := bytes.Index(pdf[streamStart:], []byte("endstream"))
		if streamEndOffset < 0 {
			t.Fatal("stream terminator is missing")
		}
		actual := streamEndOffset
		if declared != actual {
			t.Fatalf("stream length = %d, want %d", declared, actual)
		}
	}
}
