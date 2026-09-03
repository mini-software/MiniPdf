package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"strconv"
	"strings"
)

func convertDOCX(input []byte, options ConversionOptions) ([]byte, error) {
	files, err := openOfficePackage(input)
	if err != nil {
		return nil, err
	}
	documentXML, err := files.read("word/document.xml")
	if err != nil {
		return nil, err
	}
	pages, pageSize, err := extractDOCX(documentXML)
	if err != nil {
		return nil, fmt.Errorf("parse word/document.xml: %w", err)
	}
	textPages := make([]textPage, len(pages))
	for index, lines := range pages {
		textPages[index] = textPage{lines: lines, size: pageSize}
	}
	return renderTextPages(textPages, options), nil
}

func extractDOCX(data []byte) ([][]string, PageSize, error) {
	decoder := xml.NewDecoder(bytes.NewReader(data))
	pageSize := PageSizeA4
	pages := [][]string{{}}
	var paragraph strings.Builder
	paragraphDepth := 0

	for {
		token, err := decoder.Token()
		if err != nil {
			if err.Error() == "EOF" {
				break
			}
			return nil, PageSize{}, err
		}
		switch element := token.(type) {
		case xml.StartElement:
			switch element.Name.Local {
			case "p":
				paragraphDepth++
			case "t":
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return nil, PageSize{}, err
				}
				paragraph.WriteString(text)
			case "tab":
				paragraph.WriteByte('\t')
			case "br":
				if attrValue(element, "type") == "page" {
					appendDOCXParagraph(&pages[len(pages)-1], paragraph.String())
					paragraph.Reset()
					pages = append(pages, []string{})
				} else {
					paragraph.WriteByte('\n')
				}
			case "pgSz":
				width, widthErr := strconv.ParseFloat(attrValue(element, "w"), 64)
				height, heightErr := strconv.ParseFloat(attrValue(element, "h"), 64)
				if widthErr == nil && heightErr == nil && width > 0 && height > 0 {
					pageSize = PageSize{Width: width / 20, Height: height / 20}
				}
			}
		case xml.EndElement:
			if element.Name.Local == "p" && paragraphDepth > 0 {
				paragraphDepth--
				if paragraphDepth == 0 {
					appendDOCXParagraph(&pages[len(pages)-1], paragraph.String())
					paragraph.Reset()
				}
			}
		}
	}
	if paragraph.Len() > 0 {
		appendDOCXParagraph(&pages[len(pages)-1], paragraph.String())
	}
	if len(pages) == 1 && len(pages[0]) == 0 {
		pages[0] = append(pages[0], "Empty DOCX document")
	}
	return pages, pageSize, nil
}

func appendDOCXParagraph(lines *[]string, paragraph string) {
	*lines = append(*lines, strings.Split(paragraph, "\n")...)
}
