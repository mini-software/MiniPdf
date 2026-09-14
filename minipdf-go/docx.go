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
	pages, pageSize, margins, err := extractDOCX(documentXML)
	if err != nil {
		return nil, fmt.Errorf("parse word/document.xml: %w", err)
	}
	effectivePageSize := pageSize
	if options.PageSize != nil {
		effectivePageSize = *options.PageSize
	}
	effectiveMargins := margins
	if options.Margins != nil {
		effectiveMargins = *options.Margins
	}
	if err := validateDOCXLayout(effectivePageSize, effectiveMargins); err != nil {
		return nil, err
	}
	textPages := make([]textPage, len(pages))
	for index, lines := range pages {
		textPages[index] = textPage{lines: lines, size: pageSize, margins: margins}
	}
	return renderTextPages(textPages, options), nil
}

func validateDOCXLayout(pageSize PageSize, margins Margins) error {
	if _, err := NewPageSize(pageSize.Width, pageSize.Height); err != nil {
		return err
	}
	if _, err := NewMargins(margins.Left, margins.Top, margins.Right, margins.Bottom); err != nil {
		return err
	}
	if margins.Left+margins.Right >= pageSize.Width || margins.Top+margins.Bottom >= pageSize.Height {
		return fmt.Errorf("%w: margins must leave positive page content dimensions", ErrInvalidInput)
	}
	return nil
}

func extractDOCX(data []byte) ([][]string, PageSize, Margins, error) {
	decoder := xml.NewDecoder(bytes.NewReader(data))
	pageSize := PageSizeA4
	margins := Margins{Left: 54, Top: 54, Right: 54, Bottom: 54}
	pages := [][]string{{}}
	var paragraph strings.Builder
	paragraphDepth := 0

	for {
		token, err := decoder.Token()
		if err != nil {
			if err.Error() == "EOF" {
				break
			}
			return nil, PageSize{}, Margins{}, err
		}
		switch element := token.(type) {
		case xml.StartElement:
			switch element.Name.Local {
			case "p":
				paragraphDepth++
			case "t":
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return nil, PageSize{}, Margins{}, err
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
			case "pgMar":
				left, leftErr := strconv.ParseFloat(attrValue(element, "left"), 64)
				top, topErr := strconv.ParseFloat(attrValue(element, "top"), 64)
				right, rightErr := strconv.ParseFloat(attrValue(element, "right"), 64)
				bottom, bottomErr := strconv.ParseFloat(attrValue(element, "bottom"), 64)
				if leftErr == nil && topErr == nil && rightErr == nil && bottomErr == nil && left >= 0 && top >= 0 && right >= 0 && bottom >= 0 {
					margins = Margins{Left: left / 20, Top: top / 20, Right: right / 20, Bottom: bottom / 20}
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
	return pages, pageSize, margins, nil
}

func appendDOCXParagraph(lines *[]string, paragraph string) {
	*lines = append(*lines, strings.Split(paragraph, "\n")...)
}
