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
	styles, err := readXLSXStyles(files)
	if err != nil {
		return nil, err
	}
	if len(styles) > 0 {
		needsGridRendering := false
		for _, name := range worksheets {
			worksheetXML, readErr := files.read(name)
			if readErr != nil {
				return nil, readErr
			}
			if needsCalculatedXLSXGrid(worksheetXML) {
				needsGridRendering = true
				break
			}
		}
		if needsGridRendering {
			return renderXLSXWorksheets(files, worksheets, sharedStrings, styles, options)
		}
	}
	pages := make([]textPage, 0, len(worksheets))
	fallbackPageSize := PageSizeA4
	for _, name := range worksheets {
		worksheetXML, readErr := files.read(name)
		if readErr != nil {
			return nil, readErr
		}
		lines, pageSize, parseErr := extractWorksheet(worksheetXML, sharedStrings)
		if parseErr != nil {
			return nil, fmt.Errorf("parse %s: %w", name, parseErr)
		}
		lines = limitWorksheet(lines, options.MaxRows, options.MaxColumns)
		if options.Landscape != nil {
			isLandscape := pageSize.Width > pageSize.Height
			if *options.Landscape != isLandscape {
				pageSize.Width, pageSize.Height = pageSize.Height, pageSize.Width
			}
		}
		fallbackPageSize = pageSize
		if len(lines) == 0 {
			continue
		}
		effectivePageSize := pageSize
		if options.PageSize != nil {
			effectivePageSize = *options.PageSize
		}
		for _, group := range splitWorksheetColumnGroups(lines, 9) {
			overflowPages := [][]string{group}
			noWrap := false
			if worksheetColumnCount(group) == 1 {
				overflowPages = splitWorksheetTextOverflow(group, textCharactersPerLine(effectivePageSize, Margins{}))
				noWrap = len(overflowPages) > 1
			}
			for _, overflowPage := range overflowPages {
				overflowPage = append([]string{""}, overflowPage...)
				pages = append(pages, textPage{lines: overflowPage, size: pageSize, noWrap: noWrap})
			}
		}
	}
	if len(pages) == 0 {
		pages = append(pages, textPage{size: fallbackPageSize})
	}
	return renderTextPages(pages, options), nil
}

func limitWorksheet(lines []string, maxRows, maxColumns int) []string {
	if maxRows > 0 && len(lines) > maxRows {
		lines = lines[:maxRows]
	}
	if maxColumns <= 0 {
		return lines
	}
	limited := make([]string, len(lines))
	for index, line := range lines {
		cells := strings.Split(line, "\t")
		if len(cells) > maxColumns {
			cells = cells[:maxColumns]
		}
		limited[index] = strings.Join(cells, "\t")
	}
	return limited
}

func splitWorksheetColumnGroups(lines []string, columnsPerPage int) [][]string {
	maximumColumns := worksheetColumnCount(lines)
	if maximumColumns <= columnsPerPage || columnsPerPage <= 0 {
		return [][]string{lines}
	}

	groups := make([][]string, 0, (maximumColumns+columnsPerPage-1)/columnsPerPage)
	for start := 0; start < maximumColumns; start += columnsPerPage {
		end := min(start+columnsPerPage, maximumColumns)
		group := make([]string, len(lines))
		for rowIndex, line := range lines {
			cells := strings.Split(line, "\t")
			if start >= len(cells) {
				continue
			}
			rowEnd := min(end, len(cells))
			group[rowIndex] = strings.Join(cells[start:rowEnd], "\t")
		}
		groups = append(groups, group)
	}
	return groups
}

func worksheetColumnCount(lines []string) int {
	maximum := 0
	for _, line := range lines {
		maximum = max(maximum, len(strings.Split(line, "\t")))
	}
	return maximum
}

func splitWorksheetTextOverflow(lines []string, charactersPerPage int) [][]string {
	if charactersPerPage <= 0 {
		return [][]string{lines}
	}
	maximumCharacters := 0
	rows := make([][]rune, len(lines))
	for index, line := range lines {
		rows[index] = []rune(line)
		maximumCharacters = max(maximumCharacters, len(rows[index]))
	}
	pageCount := max(1, (maximumCharacters+charactersPerPage-1)/charactersPerPage)
	pages := make([][]string, pageCount)
	for pageIndex := range pages {
		start := pageIndex * charactersPerPage
		end := start + charactersPerPage
		pages[pageIndex] = make([]string, len(rows))
		for rowIndex, row := range rows {
			if start >= len(row) {
				continue
			}
			pages[pageIndex][rowIndex] = string(row[start:min(end, len(row))])
		}
	}
	return pages
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
	rowNumber := 0
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
				rowNumber = len(lines) + 1
				if parsed, parseErr := strconv.Atoi(attrValue(element, "r")); parseErr == nil && parsed > 0 {
					rowNumber = parsed
				}
			case "c":
				columnNumber := worksheetColumnIndex(attrValue(element, "r"))
				for len(row)+1 < columnNumber {
					row = append(row, "")
				}
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
				for len(lines)+1 < rowNumber {
					lines = append(lines, "")
				}
				lines = append(lines, strings.Join(row, "\t"))
			}
		}
	}
	return lines, pageSize, nil
}

func worksheetColumnIndex(reference string) int {
	column := 0
	for _, character := range reference {
		if character >= 'a' && character <= 'z' {
			character -= 'a' - 'A'
		}
		if character < 'A' || character > 'Z' {
			break
		}
		column = column*26 + int(character-'A'+1)
	}
	return column
}

func decodeWorksheetCell(decoder *xml.Decoder, start xml.StartElement, sharedStrings []string) (string, error) {
	value, _, err := decodeWorksheetCellContent(decoder, start, sharedStrings)
	return value, err
}

func decodeWorksheetCellContent(decoder *xml.Decoder, start xml.StartElement, sharedStrings []string) (string, string, error) {
	cellType := attrValue(start, "t")
	var value strings.Builder
	var formula string
	depth := 1
	for depth > 0 {
		token, err := decoder.Token()
		if err != nil {
			return "", "", err
		}
		switch element := token.(type) {
		case xml.StartElement:
			depth++
			if element.Name.Local == "v" || element.Name.Local == "t" || element.Name.Local == "f" {
				var text string
				if err := decoder.DecodeElement(&text, &element); err != nil {
					return "", "", err
				}
				depth--
				if element.Name.Local == "f" {
					formula = text
				} else {
					value.WriteString(text)
				}
			}
		case xml.EndElement:
			depth--
		}
	}
	rawValue := value.String()
	if cellType == "s" {
		index, err := strconv.Atoi(strings.TrimSpace(rawValue))
		if err == nil && index >= 0 && index < len(sharedStrings) {
			return sharedStrings[index], formula, nil
		}
	}
	if cellType == "b" {
		switch strings.TrimSpace(rawValue) {
		case "1":
			return "TRUE", formula, nil
		case "0":
			return "FALSE", formula, nil
		}
	}
	return rawValue, formula, nil
}
