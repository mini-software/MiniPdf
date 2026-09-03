package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"strconv"
	"strings"
)

func convertXLSX(input []byte, options ConversionOptions) ([]byte, error) {
	files, err := openOfficePackage(input)
	if err != nil {
		return nil, err
	}
	sharedStrings, err := readSharedStrings(files)
	if err != nil {
		return nil, err
	}
	worksheets := sortedPackageParts(files, "xl/worksheets/", ".xml")
	if len(worksheets) == 0 {
		return nil, errorsNewMissingWorksheets()
	}
	pages := make([]textPage, 0, len(worksheets))
	for index, name := range worksheets {
		worksheetXML, readErr := files.read(name)
		if readErr != nil {
			return nil, readErr
		}
		lines, pageSize, parseErr := extractWorksheet(worksheetXML, sharedStrings)
		if parseErr != nil {
			return nil, fmt.Errorf("parse %s: %w", name, parseErr)
		}
		lines = append([]string{fmt.Sprintf("Sheet %d", index+1)}, lines...)
		pages = append(pages, textPage{lines: lines, size: pageSize})
	}
	return renderTextPages(pages, options), nil
}

func errorsNewMissingWorksheets() error {
	return fmt.Errorf("Office package part %q is missing", "xl/worksheets/sheet1.xml")
}

func readSharedStrings(files officePackage) ([]string, error) {
	if _, ok := files["xl/sharedStrings.xml"]; !ok {
		return nil, nil
	}
	data, err := files.read("xl/sharedStrings.xml")
	if err != nil {
		return nil, err
	}
	decoder := xml.NewDecoder(bytes.NewReader(data))
	var values []string
	var current strings.Builder
	inString := false
	for {
		token, tokenErr := decoder.Token()
		if tokenErr != nil {
			if tokenErr.Error() == "EOF" {
				break
			}
			return nil, fmt.Errorf("parse xl/sharedStrings.xml: %w", tokenErr)
		}
		switch element := token.(type) {
		case xml.StartElement:
			if element.Name.Local == "si" {
				inString = true
				current.Reset()
			} else if element.Name.Local == "t" && inString {
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return nil, err
				}
				current.WriteString(text)
			}
		case xml.EndElement:
			if element.Name.Local == "si" && inString {
				values = append(values, current.String())
				inString = false
			}
		}
	}
	return values, nil
}

func extractWorksheet(data []byte, sharedStrings []string) ([]string, PageSize, error) {
	decoder := xml.NewDecoder(bytes.NewReader(data))
	pageSize := PageSizeA4
	var lines []string
	var row []string
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
			case "row":
				row = nil
			case "c":
				value, cellErr := decodeWorksheetCell(decoder, element, sharedStrings)
				if cellErr != nil {
					return nil, PageSize{}, cellErr
				}
				row = append(row, value)
			case "pageSetup":
				if attrValue(element, "paperSize") == "1" {
					pageSize = PageSizeLetter
				}
				if attrValue(element, "orientation") == "landscape" {
					pageSize.Width, pageSize.Height = pageSize.Height, pageSize.Width
				}
			}
		case xml.EndElement:
			if element.Name.Local == "row" {
				lines = append(lines, strings.Join(row, "\t"))
			}
		}
	}
	return lines, pageSize, nil
}

func decodeWorksheetCell(decoder *xml.Decoder, start xml.StartElement, sharedStrings []string) (string, error) {
	cellType := attrValue(start, "t")
	var value strings.Builder
	depth := 1
	for depth > 0 {
		token, err := decoder.Token()
		if err != nil {
			return "", err
		}
		switch element := token.(type) {
		case xml.StartElement:
			depth++
			if element.Name.Local == "v" || element.Name.Local == "t" {
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return "", err
				}
				depth--
				value.WriteString(text)
			}
		case xml.EndElement:
			depth--
		}
	}
	if cellType == "s" {
		index, err := strconv.Atoi(strings.TrimSpace(value.String()))
		if err == nil && index >= 0 && index < len(sharedStrings) {
			return sharedStrings[index], nil
		}
	}
	return value.String(), nil
}
