package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"strconv"
	"strings"
)

func convertPPTX(input []byte, options ConversionOptions) ([]byte, error) {
	files, err := openOfficePackage(input)
	if err != nil {
		return nil, err
	}
	presentationXML, err := files.read("ppt/presentation.xml")
	if err != nil {
		return nil, err
	}
	pageSize, err := extractSlideSize(presentationXML)
	if err != nil {
		return nil, fmt.Errorf("parse ppt/presentation.xml: %w", err)
	}
	slides := sortedPackageParts(files, "ppt/slides/", ".xml")
	if len(slides) == 0 {
		return nil, fmt.Errorf("Office package part %q is missing", "ppt/slides/slide1.xml")
	}
	pages := make([]textPage, 0, len(slides))
	for _, name := range slides {
		slideXML, readErr := files.read(name)
		if readErr != nil {
			return nil, readErr
		}
		lines, parseErr := extractSlideText(slideXML)
		if parseErr != nil {
			return nil, fmt.Errorf("parse %s: %w", name, parseErr)
		}
		pages = append(pages, textPage{lines: lines, size: pageSize})
	}
	return renderTextPages(pages, options), nil
}

func extractSlideSize(data []byte) (PageSize, error) {
	pageSize := PageSize{Width: 720, Height: 540}
	decoder := xml.NewDecoder(bytes.NewReader(data))
	for {
		token, err := decoder.Token()
		if err != nil {
			if err.Error() == "EOF" {
				return pageSize, nil
			}
			return PageSize{}, err
		}
		start, ok := token.(xml.StartElement)
		if !ok || start.Name.Local != "sldSz" {
			continue
		}
		width, widthErr := strconv.ParseFloat(attrValue(start, "cx"), 64)
		height, heightErr := strconv.ParseFloat(attrValue(start, "cy"), 64)
		if widthErr == nil && heightErr == nil && width > 0 && height > 0 {
			pageSize = PageSize{Width: width / 12700, Height: height / 12700}
		}
		return pageSize, nil
	}
}

func extractSlideText(data []byte) ([]string, error) {
	decoder := xml.NewDecoder(bytes.NewReader(data))
	var lines []string
	var paragraph strings.Builder
	paragraphDepth := 0
	for {
		token, err := decoder.Token()
		if err != nil {
			if err.Error() == "EOF" {
				break
			}
			return nil, err
		}
		switch element := token.(type) {
		case xml.StartElement:
			switch element.Name.Local {
			case "p":
				paragraphDepth++
			case "t":
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return nil, err
				}
				paragraph.WriteString(text)
			case "br":
				paragraph.WriteByte('\n')
			case "tab":
				paragraph.WriteString("    ")
			}
		case xml.EndElement:
			if element.Name.Local == "p" && paragraphDepth > 0 {
				paragraphDepth--
				if paragraphDepth == 0 {
					lines = append(lines, strings.Split(paragraph.String(), "\n")...)
					paragraph.Reset()
				}
			}
		}
	}
	if len(lines) == 0 {
		lines = append(lines, "Empty PPTX slide")
	}
	return lines, nil
}
