package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"strconv"
	"strings"
)

const (
	xlsxColumnWidthPoints       = 5.62
	xlsxDefaultRowHeightScale   = 14.6 / 15
	calibriToHelveticaFontScale = 0.73
	calibriRegularWidthScale    = 90 / calibriToHelveticaFontScale
	calibriBoldWidthScale       = 80 / calibriToHelveticaFontScale
	helveticaBaselineRatio      = 0.45
)

type xlsxStyle struct {
	font       xlsxFont
	fill       *PDFColor
	border     xlsxBorder
	numberFmt  int
	numberCode string
	align      string
	vertical   string
	shrink     bool
	padding    float64
}

type xlsxFont struct {
	name     string
	size     float64
	color    PDFColor
	bold     bool
	italic   bool
	embedded *embeddedFont
}

type xlsxBorder struct {
	left, right, top, bottom                         float64
	leftDouble, rightDouble, topDouble, bottomDouble bool
}

type xlsxCell struct {
	row, column int
	value       string
	formula     string
	style       int
	numeric     bool
	formatted   string
}

type xlsxMerge struct {
	startRow, startColumn int
	endRow, endColumn     int
}

type xlsxGrid struct {
	cells                                map[[2]int]xlsxCell
	columnWidths                         map[int]float64
	rowHeights                           map[int]float64
	merges                               []xlsxMerge
	charts                               []xlsxChart
	pictures                             []xlsxPicture
	horizontalCentered, verticalCentered bool
	footerMargin                         float64
	headerFooter                         xlsxHeaderFooter
	conditionalFills                     []xlsxConditionalFormat
	maxRow, maxColumn                    int
	dimensionRow                         int
	dimensionColumn                      int
	defaultRowHeight                     float64
	pageSize                             PageSize
	margins                              Margins
	hiddenRows                           map[int]bool
	hiddenColumns                        map[int]bool
	rowBreaks                            map[int]bool
	columnBreaks                         map[int]bool
	printLayout                          bool
	printScale                           float64
	fitToPage                            bool
	fitWidth                             int
	fitHeight                            int
	overThenDown                         bool
}

type xlsxStylesDocument struct {
	Fonts struct {
		Items []struct {
			Bold   *struct{} `xml:"b"`
			Italic *struct{} `xml:"i"`
			Name   struct {
				Value string `xml:"val,attr"`
			} `xml:"name"`
			Size struct {
				Value float64 `xml:"val,attr"`
			} `xml:"sz"`
			Color xlsxColorXML `xml:"color"`
		} `xml:"font"`
	} `xml:"fonts"`
	NumberFormats struct {
		Items []struct {
			ID   int    `xml:"numFmtId,attr"`
			Code string `xml:"formatCode,attr"`
		} `xml:"numFmt"`
	} `xml:"numFmts"`
	Fills struct {
		Items []xlsxFillXML `xml:"fill"`
	} `xml:"fills"`
	Borders struct {
		Items []struct {
			Left struct {
				Style string `xml:"style,attr"`
			} `xml:"left"`
			Right struct {
				Style string `xml:"style,attr"`
			} `xml:"right"`
			Top struct {
				Style string `xml:"style,attr"`
			} `xml:"top"`
			Bottom struct {
				Style string `xml:"style,attr"`
			} `xml:"bottom"`
		} `xml:"border"`
	} `xml:"borders"`
	CellFormats struct {
		Items []struct {
			FontID    int `xml:"fontId,attr"`
			FillID    int `xml:"fillId,attr"`
			BorderID  int `xml:"borderId,attr"`
			NumberFmt int `xml:"numFmtId,attr"`
			Alignment struct {
				Horizontal string `xml:"horizontal,attr"`
				Vertical   string `xml:"vertical,attr"`
				Shrink     bool   `xml:"shrinkToFit,attr"`
			} `xml:"alignment"`
		} `xml:"xf"`
	} `xml:"cellXfs"`
	DifferentialFormats struct {
		Items []struct {
			Fill *xlsxFillXML `xml:"fill"`
		} `xml:"dxf"`
	} `xml:"dxfs"`
}

func needsCalculatedXLSXGrid(data []byte) bool {
	var worksheet struct {
		Cells []struct {
			Formula *string `xml:"f"`
			Value   *string `xml:"v"`
		} `xml:"sheetData>row>c"`
	}
	if xml.Unmarshal(data, &worksheet) != nil {
		return false
	}
	formulaCount := 0
	for _, cell := range worksheet.Cells {
		if cell.Formula == nil {
			continue
		}
		formulaCount++
		if cell.Value != nil && strings.TrimSpace(*cell.Value) != "" {
			return false
		}
	}
	return formulaCount > 0
}

func readXLSXStyles(files officePackage) ([]xlsxStyle, []*PDFColor, error) {
	if _, ok := files["xl/styles.xml"]; !ok {
		return nil, nil, nil
	}
	data, err := files.read("xl/styles.xml")
	if err != nil {
		return nil, nil, err
	}
	var document xlsxStylesDocument
	if err := xml.Unmarshal(data, &document); err != nil {
		return nil, nil, fmt.Errorf("parse xl/styles.xml: %w", err)
	}
	theme, err := readXLSXTheme(files)
	if err != nil {
		return nil, nil, err
	}
	styles := make([]xlsxStyle, len(document.CellFormats.Items))
	fonts := make(map[int]*embeddedFont)
	type fontKey struct {
		name         string
		bold, italic bool
	}
	variants := make(map[fontKey]*embeddedFont)
	for index, font := range document.Fonts.Items {
		key := fontKey{strings.ToLower(font.Name.Value), font.Bold != nil, font.Italic != nil}
		embedded, exists := variants[key]
		if !exists {
			embedded, err = readXLSXFont(font.Name.Value, key.bold, key.italic)
			if err != nil {
				return nil, nil, err
			}
			variants[key] = embedded
		}
		fonts[index] = embedded
	}
	for index, format := range document.CellFormats.Items {
		style := xlsxStyle{
			font: xlsxFont{size: 11, color: PDFColorBlack}, numberFmt: format.NumberFmt,
			align: format.Alignment.Horizontal, vertical: format.Alignment.Vertical, shrink: format.Alignment.Shrink,
		}
		if format.FontID >= 0 && format.FontID < len(document.Fonts.Items) {
			font := document.Fonts.Items[format.FontID]
			style.font.name = font.Name.Value
			if font.Size.Value > 0 {
				style.font.size = font.Size.Value
			}
			style.font.bold = font.Bold != nil
			style.font.italic = font.Italic != nil
			style.font.embedded = fonts[format.FontID]
			color, err := resolveXLSXColor(font.Color, theme)
			if err != nil {
				return nil, nil, fmt.Errorf("parse xl/styles.xml font %d: %w", format.FontID, err)
			}
			if color != nil {
				style.font.color = *color
			}
		}
		if format.FillID >= 0 && format.FillID < len(document.Fills.Items) {
			fill := document.Fills.Items[format.FillID].Pattern
			if fill.Type == "solid" {
				color, err := resolveXLSXColor(fill.Foreground, theme)
				if err != nil {
					return nil, nil, fmt.Errorf("parse xl/styles.xml fill %d: %w", format.FillID, err)
				}
				style.fill = color
			}
		}
		if format.BorderID >= 0 && format.BorderID < len(document.Borders.Items) {
			border := document.Borders.Items[format.BorderID]
			style.border = xlsxBorder{
				left: borderWidth(border.Left.Style), right: borderWidth(border.Right.Style),
				top: borderWidth(border.Top.Style), bottom: borderWidth(border.Bottom.Style),
				leftDouble: border.Left.Style == "double", rightDouble: border.Right.Style == "double",
				topDouble: border.Top.Style == "double", bottomDouble: border.Bottom.Style == "double",
			}
		}
		styles[index] = style
		for _, number := range document.NumberFormats.Items {
			if number.ID == format.NumberFmt {
				styles[index].numberCode = number.Code
				break
			}
		}
	}
	differentialFills := make([]*PDFColor, len(document.DifferentialFormats.Items))
	for index, format := range document.DifferentialFormats.Items {
		if format.Fill == nil {
			continue
		}
		pattern := format.Fill.Pattern
		if pattern.Type != "" && pattern.Type != "solid" {
			continue
		}
		color := pattern.Background
		if color.RGB == "" && color.Theme == nil {
			color = pattern.Foreground
		}
		differentialFills[index], err = resolveXLSXColor(color, theme)
		if err != nil {
			return nil, nil, fmt.Errorf("parse xl/styles.xml differential fill %d: %w", index, err)
		}
	}
	return styles, differentialFills, nil
}

func parseXLSXColor(value string) (PDFColor, bool) {
	if len(value) < 6 {
		return PDFColor{}, false
	}
	parsed, err := strconv.ParseUint(value[len(value)-6:], 16, 32)
	if err != nil {
		return PDFColor{}, false
	}
	return PDFColor{Red: float64(parsed>>16) / 255, Green: float64(parsed>>8&0xff) / 255, Blue: float64(parsed&0xff) / 255}, true
}

func borderWidth(style string) float64 {
	switch style {
	case "hair":
		return 0.25
	case "medium", "mediumDashed", "mediumDashDot", "mediumDashDotDot":
		return 1.5
	case "thick", "double":
		return 2
	case "thin", "dashed", "dotted", "dashDot", "dashDotDot":
		return 0.75
	default:
		return 0
	}
}

func renderXLSXWorksheets(files officePackage, worksheetNames []string, sharedStrings []string, styles []xlsxStyle, differentialFills []*PDFColor, options ConversionOptions, printSettings map[string]xlsxPrintSettings) ([]byte, error) {
	document := NewPDFDocument()
	for _, name := range worksheetNames {
		if err := renderXLSXWorksheet(document, files, name, sharedStrings, styles, differentialFills, options, printSettings[strings.ToLower(name)]); err != nil {
			return nil, err
		}
	}
	return document.BytesWithOptions(PDFSaveOptions{Compress: options.Compress}), nil
}

func renderXLSXWorksheet(document *PDFDocument, files officePackage, name string, sharedStrings []string, styles []xlsxStyle, differentialFills []*PDFColor, options ConversionOptions, settings xlsxPrintSettings) error {
	data, err := files.read(name)
	if err != nil {
		return err
	}
	grid, err := readXLSXGrid(data, sharedStrings)
	if err != nil {
		return fmt.Errorf("parse %s: %w", name, err)
	}
	grid.charts, err = readXLSXCharts(files, name, &grid)
	if err != nil {
		return err
	}
	grid.applyOptions(options)
	if len(settings.areas) > 0 {
		grid.pictures, err = readXLSXPictures(files, name, grid)
		if err != nil {
			return err
		}
		for key, cell := range grid.cells {
			if cell.value != "" && cell.numeric {
				style := styleAt(styles, cell.style)
				cell.formatted, err = formatPrintedXLSXValue(cell.value, style.numberFmt, style.numberCode, settings.date1904)
				if err != nil {
					return fmt.Errorf("format %s cell %d/%d: %w", name, cell.row, cell.column, err)
				}
				grid.cells[key] = cell
			}
		}
		if err := grid.prepareFooters(styles, settings.sheetName); err != nil {
			return fmt.Errorf("parse %s footer: %w", name, err)
		}
		var scopes []xlsxMerge
		for _, area := range settings.areas {
			area = limitXLSXPrintArea(grid, area, options)
			if area.endRow < area.startRow || area.endColumn < area.startColumn {
				continue
			}
			scopes = append(scopes, area)
			if settings.rows.start > 0 {
				scopes = append(scopes, xlsxMerge{startRow: settings.rows.start, endRow: settings.rows.end, startColumn: area.startColumn, endColumn: area.endColumn})
			}
			if settings.columns.start > 0 {
				scopes = append(scopes, xlsxMerge{startRow: area.startRow, endRow: area.endRow, startColumn: settings.columns.start, endColumn: settings.columns.end})
			}
			if settings.rows.start > 0 && settings.columns.start > 0 {
				scopes = append(scopes, xlsxMerge{startRow: settings.rows.start, endRow: settings.rows.end, startColumn: settings.columns.start, endColumn: settings.columns.end})
			}
		}
		styles, err = grid.applyConditionalFills(styles, differentialFills, scopes)
		if err != nil {
			return fmt.Errorf("render %s conditional fills: %w", name, err)
		}
		if err := renderXLSXPrintGrid(document, grid, styles, settings, options); err != nil {
			return fmt.Errorf("render %s print area: %w", name, err)
		}
	} else {
		renderXLSXGrid(document, grid, styles)
	}
	return nil
}

func readXLSXGrid(data []byte, sharedStrings []string) (xlsxGrid, error) {
	grid := xlsxGrid{
		cells: make(map[[2]int]xlsxCell), columnWidths: make(map[int]float64), rowHeights: make(map[int]float64),
		defaultRowHeight: 15, pageSize: PageSizeA4, margins: Margins{Left: 54, Right: 54, Top: 72, Bottom: 72},
		hiddenRows: make(map[int]bool), hiddenColumns: make(map[int]bool),
		rowBreaks: make(map[int]bool), columnBreaks: make(map[int]bool),
		printScale: 1, fitWidth: 1, fitHeight: 1,
		footerMargin: 36,
	}
	breakKind := ""
	decoder := xml.NewDecoder(bytes.NewReader(data))
	for {
		token, err := decoder.Token()
		if err != nil {
			if err.Error() == "EOF" {
				break
			}
			return xlsxGrid{}, err
		}
		start, ok := token.(xml.StartElement)
		if !ok {
			if end, ok := token.(xml.EndElement); ok && (end.Name.Local == "rowBreaks" || end.Name.Local == "colBreaks") {
				breakKind = ""
			}
			continue
		}
		switch start.Name.Local {
		case "printOptions":
			grid.horizontalCentered = attrValue(start, "horizontalCentered") == "1" || attrValue(start, "horizontalCentered") == "true"
			grid.verticalCentered = attrValue(start, "verticalCentered") == "1" || attrValue(start, "verticalCentered") == "true"
		case "headerFooter":
			if err := decoder.DecodeElement(&grid.headerFooter, &start); err != nil {
				return xlsxGrid{}, err
			}
		case "conditionalFormatting":
			var format xlsxConditionalFormat
			if err := decoder.DecodeElement(&format, &start); err != nil {
				return xlsxGrid{}, err
			}
			for _, reference := range strings.Fields(format.References) {
				reference = strings.ReplaceAll(reference, "$", "")
				if !strings.Contains(reference, ":") {
					reference += ":" + reference
				}
				area, ok := parseXLSXMerge(reference)
				endpoints := strings.Split(reference, ":")
				if !ok || len(endpoints) != 2 || !xlsxConditionalReference.MatchString(endpoints[0]) ||
					!xlsxConditionalReference.MatchString(endpoints[1]) || area.endRow > 1048576 || area.endColumn > 16384 {
					return xlsxGrid{}, fmt.Errorf("invalid XLSX conditional formatting range %q", reference)
				}
				format.areas = append(format.areas, area)
			}
			if len(format.areas) == 0 {
				return xlsxGrid{}, fmt.Errorf("missing XLSX conditional formatting range")
			}
			grid.conditionalFills = append(grid.conditionalFills, format)
		case "dimension":
			reference := attrValue(start, "ref")
			parts := strings.Split(reference, ":")
			grid.dimensionRow, grid.dimensionColumn = xlsxCellPosition(parts[len(parts)-1])
		case "sheetFormatPr":
			if height, err := strconv.ParseFloat(attrValue(start, "defaultRowHeight"), 64); err == nil && height > 0 {
				grid.defaultRowHeight = height
			}
		case "col":
			minimum, _ := strconv.Atoi(attrValue(start, "min"))
			maximum, _ := strconv.Atoi(attrValue(start, "max"))
			width, _ := strconv.ParseFloat(attrValue(start, "width"), 64)
			maximum = min(maximum, 16384)
			for column := minimum; column <= maximum && column > 0; column++ {
				grid.columnWidths[column] = width * xlsxColumnWidthPoints
				grid.hiddenColumns[column] = attrValue(start, "hidden") == "1" || attrValue(start, "hidden") == "true"
			}
		case "row":
			row, _ := strconv.Atoi(attrValue(start, "r"))
			if height, err := strconv.ParseFloat(attrValue(start, "ht"), 64); err == nil && height > 0 {
				grid.rowHeights[row] = height
			}
			grid.maxRow = max(grid.maxRow, row)
			grid.hiddenRows[row] = attrValue(start, "hidden") == "1" || attrValue(start, "hidden") == "true"
		case "c":
			reference := attrValue(start, "r")
			row, column := xlsxCellPosition(reference)
			value, formula, err := decodeWorksheetCellContent(decoder, start, sharedStrings)
			if err != nil {
				return xlsxGrid{}, err
			}
			style, _ := strconv.Atoi(attrValue(start, "s"))
			cellType := attrValue(start, "t")
			grid.cells[[2]int{row, column}] = xlsxCell{row: row, column: column, value: value, formula: formula, style: style, numeric: cellType == "" || cellType == "n"}
			grid.maxRow = max(grid.maxRow, row)
			grid.maxColumn = max(grid.maxColumn, column)
		case "mergeCell":
			if merge, ok := parseXLSXMerge(attrValue(start, "ref")); ok {
				grid.merges = append(grid.merges, merge)
			}
		case "pageMargins":
			grid.footerMargin = inchesToPoints(attrValue(start, "footer"), 36)
			grid.margins = Margins{
				Left: inchesToPoints(attrValue(start, "left"), 54), Right: inchesToPoints(attrValue(start, "right"), 54),
				Top: inchesToPoints(attrValue(start, "top"), 72), Bottom: inchesToPoints(attrValue(start, "bottom"), 72),
			}
		case "pageSetup":
			grid.overThenDown = attrValue(start, "pageOrder") == "overThenDown"
			if value := attrValue(start, "scale"); value != "" {
				scale, err := strconv.Atoi(value)
				if err != nil || (scale != 0 && scale < 10) || scale > 400 {
					return xlsxGrid{}, fmt.Errorf("invalid XLSX print scale %q", value)
				}
				if scale > 0 {
					grid.printScale = float64(scale) / 100
				}
			}
			for _, setting := range []struct {
				name   string
				target *int
			}{{"fitToWidth", &grid.fitWidth}, {"fitToHeight", &grid.fitHeight}} {
				if value := attrValue(start, setting.name); value != "" {
					count, err := strconv.Atoi(value)
					if err != nil || count < 0 {
						return xlsxGrid{}, fmt.Errorf("invalid XLSX %s %q", setting.name, value)
					}
					*setting.target = count
				}
			}
			if attrValue(start, "paperSize") == "1" {
				grid.pageSize = PageSizeLetter
			}
			if attrValue(start, "orientation") == "landscape" {
				grid.pageSize.Width, grid.pageSize.Height = grid.pageSize.Height, grid.pageSize.Width
			}
		case "pageSetUpPr":
			grid.fitToPage = attrValue(start, "fitToPage") == "1" || attrValue(start, "fitToPage") == "true"
		case "rowBreaks", "colBreaks":
			breakKind = start.Name.Local
		case "brk":
			if attrValue(start, "man") == "1" || attrValue(start, "man") == "true" {
				index, err := strconv.Atoi(attrValue(start, "id"))
				if err != nil || index < 0 {
					return xlsxGrid{}, fmt.Errorf("invalid XLSX manual page break")
				}
				if breakKind == "rowBreaks" {
					grid.rowBreaks[index] = true
				} else if breakKind == "colBreaks" {
					grid.columnBreaks[index] = true
				}
			}
		}
	}
	if grid.dimensionRow > 0 && grid.dimensionColumn > 0 {
		grid.maxRow = grid.dimensionRow
		grid.maxColumn = grid.dimensionColumn
	}
	grid.evaluateFormulas()
	return grid, nil
}

func (grid *xlsxGrid) evaluateFormulas() {
	visiting := make(map[[2]int]bool)
	for key, cell := range grid.cells {
		if cell.value == "" && cell.formula != "" {
			if value, ok := grid.evaluateCell(key, visiting); ok {
				cell.value = strconv.FormatFloat(value, 'f', -1, 64)
				grid.cells[key] = cell
			}
		}
	}
}

func (grid *xlsxGrid) evaluateCell(key [2]int, visiting map[[2]int]bool) (float64, bool) {
	cell, exists := grid.cells[key]
	if !exists {
		return 0, true
	}
	if cell.value != "" {
		value, err := strconv.ParseFloat(strings.TrimSpace(cell.value), 64)
		return value, err == nil
	}
	if cell.formula == "" || visiting[key] {
		return 0, false
	}
	visiting[key] = true
	defer delete(visiting, key)
	formula := strings.TrimSpace(strings.TrimPrefix(cell.formula, "="))
	if strings.HasPrefix(strings.ToUpper(formula), "SUM(") && strings.HasSuffix(formula, ")") {
		merge, ok := parseXLSXMerge(formula[4 : len(formula)-1])
		if !ok {
			return 0, false
		}
		total := 0.0
		for row := merge.startRow; row <= merge.endRow; row++ {
			for column := merge.startColumn; column <= merge.endColumn; column++ {
				value, ok := grid.evaluateCell([2]int{row, column}, visiting)
				if !ok {
					return 0, false
				}
				total += value
			}
		}
		return total, true
	}
	total := 0.0
	for _, reference := range strings.Split(formula, "+") {
		row, column := xlsxCellPosition(strings.TrimSpace(reference))
		if row == 0 || column == 0 {
			return 0, false
		}
		value, ok := grid.evaluateCell([2]int{row, column}, visiting)
		if !ok {
			return 0, false
		}
		total += value
	}
	return total, true
}

func xlsxCellPosition(reference string) (int, int) {
	column := worksheetColumnIndex(reference)
	index := 0
	for index < len(reference) && ((reference[index] >= 'A' && reference[index] <= 'Z') || (reference[index] >= 'a' && reference[index] <= 'z')) {
		index++
	}
	row, _ := strconv.Atoi(reference[index:])
	return row, column
}

func parseXLSXMerge(reference string) (xlsxMerge, bool) {
	parts := strings.Split(reference, ":")
	if len(parts) != 2 {
		return xlsxMerge{}, false
	}
	startRow, startColumn := xlsxCellPosition(parts[0])
	endRow, endColumn := xlsxCellPosition(parts[1])
	return xlsxMerge{startRow: startRow, startColumn: startColumn, endRow: endRow, endColumn: endColumn}, startRow > 0 && startColumn > 0 && endRow >= startRow && endColumn >= startColumn
}

func inchesToPoints(value string, fallback float64) float64 {
	inches, err := strconv.ParseFloat(value, 64)
	if err != nil || inches < 0 {
		return fallback
	}
	return inches * 72
}

func (grid *xlsxGrid) applyOptions(options ConversionOptions) {
	if options.MaxRows > 0 {
		grid.maxRow = min(grid.maxRow, options.MaxRows)
	}
	if options.MaxColumns > 0 {
		grid.maxColumn = min(grid.maxColumn, options.MaxColumns)
	}
	if options.PageSize != nil {
		grid.pageSize = *options.PageSize
	}
	if options.Margins != nil {
		grid.margins = *options.Margins
	}
	if options.Landscape != nil && (*options.Landscape != (grid.pageSize.Width > grid.pageSize.Height)) {
		grid.pageSize.Width, grid.pageSize.Height = grid.pageSize.Height, grid.pageSize.Width
	}
}

type xlsxRange struct{ start, end int }

func renderXLSXGrid(document *PDFDocument, grid xlsxGrid, styles []xlsxStyle) {
	if grid.maxRow == 0 || grid.maxColumn == 0 {
		document.AddPage(grid.pageSize.Width, grid.pageSize.Height)
		return
	}
	columnBands := xlsxBands(grid.maxColumn, grid.pageSize.Width-grid.margins.Left-grid.margins.Right, func(index int) float64 { return grid.columnWidth(index) })
	rowBands := xlsxBands(grid.maxRow, grid.pageSize.Height-grid.margins.Top-grid.margins.Bottom, func(index int) float64 { return grid.rowHeight(index) })
	for _, columnBand := range columnBands {
		for _, rowBand := range rowBands {
			page := document.AddPage(grid.pageSize.Width, grid.pageSize.Height)
			renderXLSXBand(page, grid, styles, rowBand, columnBand)
		}
	}
}

func xlsxBands(count int, available float64, size func(int) float64) []xlsxRange {
	var bands []xlsxRange
	for start := 1; start <= count; {
		end := start
		used := 0.0
		for end <= count && (used+size(end) <= available || end == start) {
			used += size(end)
			end++
		}
		bands = append(bands, xlsxRange{start: start, end: end - 1})
		start = end
	}
	return bands
}

func (grid xlsxGrid) columnWidth(column int) float64 {
	if grid.printLayout && grid.hiddenColumns[column] {
		return 0
	}
	scale := grid.layoutScale()
	if width := grid.columnWidths[column]; width > 0 && (grid.printLayout || grid.dimensionColumn == 0 || column <= grid.dimensionColumn) {
		return width * scale
	}
	return 8.43 * xlsxColumnWidthPoints * scale
}

func (grid xlsxGrid) rowHeight(row int) float64 {
	if grid.printLayout && grid.hiddenRows[row] {
		return 0
	}
	scale := grid.layoutScale()
	if height := grid.rowHeights[row]; height > 0 {
		return height * scale
	}
	return grid.defaultRowHeight * xlsxDefaultRowHeightScale * scale
}

func (grid xlsxGrid) layoutScale() float64 {
	if grid.printLayout {
		return grid.printScale
	}
	return 1
}

func renderXLSXBand(page *PDFPage, grid xlsxGrid, styles []xlsxStyle, rows, columns xlsxRange) {
	type cellText struct {
		cell                xlsxCell
		x, y, width, height float64
		style               xlsxStyle
	}
	var textCells []cellText
	paintCell := func(cell xlsxCell, x, y, width, height float64, style xlsxStyle, drawText bool) {
		if grid.printLayout {
			renderXLSXCell(page, cell, x, y, width, height, style, false)
			if drawText && width > 0 && height > 0 {
				textCells = append(textCells, cellText{cell: cell, x: x, y: y, width: width, height: height, style: style})
			}
		} else {
			renderXLSXCell(page, cell, x, y, width, height, style, drawText)
		}
	}
	xPositions := map[int]float64{columns.start: grid.margins.Left}
	for column := columns.start; column <= columns.end; column++ {
		xPositions[column+1] = xPositions[column] + grid.columnWidth(column)
	}
	yPositions := map[int]float64{rows.start: grid.pageSize.Height - grid.margins.Top}
	for row := rows.start; row <= rows.end; row++ {
		yPositions[row+1] = yPositions[row] - grid.rowHeight(row)
	}
	merged := make(map[[2]int]bool)
	for _, merge := range grid.merges {
		for row := merge.startRow; row <= merge.endRow; row++ {
			for column := merge.startColumn; column <= merge.endColumn; column++ {
				merged[[2]int{row, column}] = true
			}
		}
		if merge.endRow < rows.start || merge.startRow > rows.end || merge.endColumn < columns.start || merge.startColumn > columns.end {
			continue
		}
		cell := grid.cells[[2]int{merge.startRow, merge.startColumn}]
		startColumn := max(merge.startColumn, columns.start)
		endColumn := min(merge.endColumn, columns.end)
		startRow := max(merge.startRow, rows.start)
		endRow := min(merge.endRow, rows.end)
		paintCell(cell, xPositions[startColumn], yPositions[endRow+1], xPositions[endColumn+1]-xPositions[startColumn], yPositions[startRow]-yPositions[endRow+1], grid.scaledStyle(styles, cell.style), merge.startColumn >= columns.start && merge.startColumn <= columns.end)
	}
	for row := rows.start; row <= rows.end; row++ {
		for column := columns.start; column <= columns.end; column++ {
			if grid.rowHeight(row) == 0 || grid.columnWidth(column) == 0 {
				continue
			}
			if merged[[2]int{row, column}] {
				continue
			}
			cell, exists := grid.cells[[2]int{row, column}]
			if !exists {
				continue
			}
			paintCell(cell, xPositions[column], yPositions[row+1], grid.columnWidth(column), grid.rowHeight(row), grid.scaledStyle(styles, cell.style), true)
		}
	}
	for _, text := range textCells {
		renderXLSXCellText(page, text.cell, text.x, text.y, text.width, text.height, text.style)
	}
	renderXLSXCharts(page, grid, rows, columns)
	if grid.printLayout {
		renderXLSXPictures(page, grid, rows, columns)
	}
}

func (grid xlsxGrid) scaledStyle(styles []xlsxStyle, index int) xlsxStyle {
	style := styleAt(styles, index)
	if !grid.printLayout {
		style.font.embedded = nil
		style.font.italic = false
		style.shrink = false
	} else {
		style.padding = 1.5 * grid.layoutScale()
	}
	scale := grid.layoutScale()
	style.font.size *= scale
	style.border.left *= scale
	style.border.right *= scale
	style.border.top *= scale
	style.border.bottom *= scale
	return style
}

func styleAt(styles []xlsxStyle, index int) xlsxStyle {
	if index >= 0 && index < len(styles) {
		return styles[index]
	}
	return xlsxStyle{font: xlsxFont{size: 11, color: PDFColorBlack}}
}

func renderXLSXCell(page *PDFPage, cell xlsxCell, x, y, width, height float64, style xlsxStyle, drawText bool) {
	if width <= 0 || height <= 0 {
		return
	}
	if style.fill != nil {
		page.AddRect(x, y, width, height, *style.fill)
	}
	if style.border.left > 0 {
		drawXLSXBorder(page, x, y, x, y+height, style.border.left, style.border.leftDouble)
	}
	if style.border.right > 0 {
		drawXLSXBorder(page, x+width, y, x+width, y+height, style.border.right, style.border.rightDouble)
	}
	if style.border.top > 0 {
		drawXLSXBorder(page, x, y+height, x+width, y+height, style.border.top, style.border.topDouble)
	}
	if style.border.bottom > 0 {
		drawXLSXBorder(page, x, y, x+width, y, style.border.bottom, style.border.bottomDouble)
	}
	if !drawText || cell.value == "" {
		return
	}
	renderXLSXCellText(page, cell, x, y, width, height, style)
}

func drawXLSXBorder(page *PDFPage, x1, y1, x2, y2, width float64, double bool) {
	if !double {
		page.AddLine(x1, y1, x2, y2, PDFColorBlack, width)
		return
	}
	offset := width / 3
	if x1 == x2 {
		page.AddLine(x1-offset, y1, x2-offset, y2, PDFColorBlack, offset)
		page.AddLine(x1+offset, y1, x2+offset, y2, PDFColorBlack, offset)
	} else {
		page.AddLine(x1, y1-offset, x2, y2-offset, PDFColorBlack, offset)
		page.AddLine(x1, y1+offset, x2, y2+offset, PDFColorBlack, offset)
	}
}

func renderXLSXCellText(page *PDFPage, cell xlsxCell, x, y, width, height float64, style xlsxStyle) {
	if cell.value == "" {
		return
	}
	text := formatXLSXValue(cell.value, cell.numeric, style.numberFmt)
	if cell.formatted != "" {
		text = cell.formatted
	}
	fontSize := style.font.size
	horizontalScale := 100.0
	embedded := style.font.embedded
	if embedded != nil && !embedded.canEncode(text) {
		embedded = nil
	}
	if embedded == nil && strings.EqualFold(style.font.name, "Calibri") {
		fontSize *= calibriToHelveticaFontScale
		horizontalScale = calibriRegularWidthScale
		if style.font.bold {
			horizontalScale = calibriBoldWidthScale
		}
	}
	textWidth := measureHelveticaText(text, fontSize, horizontalScale, style.font.bold)
	if embedded != nil {
		textWidth = measureXLSXFont(text, fontSize, embedded)
	}
	padding := style.padding
	if padding == 0 {
		padding = 2
	}
	if style.shrink && textWidth > width-2*padding && width > 2*padding {
		ratio := (width - 2*padding) / textWidth
		fontSize *= ratio
		textWidth *= ratio
	}
	textX := x + padding
	switch style.align {
	case "center":
		textX = x + (width-textWidth)/2
	case "right":
		textX = x + width - textWidth - padding
	case "left":
		textX = x + padding
	default:
		if cell.numeric {
			textX = x + width - textWidth - padding
		}
	}
	textY := y + fontSize*helveticaBaselineRatio
	if embedded != nil {
		textY = y + fontSize*embedded.descent
	}
	if style.vertical == "center" {
		textY = y + (height-fontSize)/2
	} else if style.vertical == "top" {
		textY = y + height - fontSize
		if embedded != nil {
			textY = y + height - fontSize*embedded.ascent
		}
	}
	page.operations = append(page.operations, textOperation{text: text, x: textX, y: textY, fontSize: fontSize,
		horizontalScale: horizontalScale, color: style.font.color, bold: style.font.bold, italic: style.font.italic, embedded: embedded})
}

func measureHelveticaText(text string, fontSize, horizontalScale float64, bold bool) float64 {
	total := 0
	for _, character := range text {
		total += helveticaCharacterWidth(character, bold)
	}
	return float64(total) * fontSize / 1000 * horizontalScale / 100
}

func helveticaCharacterWidth(character rune, bold bool) int {
	if bold {
		switch character {
		case ' ', ',':
			return 278
		case '!', ':', ';', '(', ')', '-', '.', '/':
			return 333
		case '"':
			return 474
		case '#', '$', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9':
			return 556
		case '%':
			return 889
		case '&', 'A', 'B', 'C', 'D', 'G', 'H', 'K', 'N', 'O', 'P', 'Q', 'R', 'U', 'V', 'X', 'Y':
			return 722
		case '\'', 'I':
			return 238
		case '*':
			return 389
		case '+', '<', '=', '>', '^', '~':
			return 584
		case '?', 'E', 'S':
			return 611
		case '@':
			return 975
		case 'F', 'L', 'T', 'Z':
			return 611
		case 'J':
			return 556
		case 'M':
			return 833
		case 'W':
			return 944
		case '[', ']':
			return 333
		case '\\':
			return 278
		case '_':
			return 556
		case '`':
			return 333
		case 'a', 'c', 'e', 's', 'x', 'v':
			return 556
		case 'b', 'd', 'g', 'h', 'n', 'o', 'p', 'q', 'u':
			return 611
		case 'f', 't':
			return 333
		case 'i', 'j', 'l':
			return 278
		case 'k':
			return 556
		case 'm':
			return 889
		case 'r':
			return 389
		case 'w':
			return 778
		case 'y':
			return 556
		case 'z':
			return 500
		case '{', '}':
			return 389
		case '|':
			return 280
		}
	} else {
		switch character {
		case ' ', '!', ',', '.', '/', ':', ';', 'I', '[', '\\', ']':
			return 278
		case '"':
			return 355
		case '#', '$', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '?', 'L', '_':
			return 556
		case '%':
			return 889
		case '&', 'A', 'B', 'E', 'K', 'P', 'S', 'V', 'X', 'Y':
			return 667
		case '\'':
			return 191
		case '(', ')', '-', '`':
			return 333
		case '*':
			return 389
		case '+', '<', '=', '>', '~':
			return 584
		case '@':
			return 1015
		case 'C', 'D', 'H', 'N', 'R', 'U':
			return 722
		case 'F', 'T', 'Z':
			return 611
		case 'G', 'O', 'Q':
			return 778
		case 'J':
			return 500
		case 'M':
			return 833
		case 'W':
			return 944
		case '^':
			return 469
		case 'a', 'b', 'd', 'e', 'g', 'h', 'n', 'o', 'p', 'q', 'u':
			return 556
		case 'c', 'k', 's', 'v', 'x', 'y', 'z':
			return 500
		case 'f':
			return 278
		case 'i', 'j', 'l':
			return 222
		case 'm':
			return 833
		case 'r':
			return 333
		case 't':
			return 278
		case 'w':
			return 722
		case '{', '}':
			return 334
		case '|':
			return 260
		}
	}
	if character >= 0x1100 && (character <= 0x115f || character >= 0x2e80 && character <= 0x9fff || character >= 0xac00 && character <= 0xd7af || character >= 0xf900 && character <= 0xfaff || character >= 0xfe30 && character <= 0xfe4f || character >= 0xff01 && character <= 0xff60 || character >= 0xffe0 && character <= 0xffe6) {
		return 1000
	}
	return 556
}

func formatXLSXValue(value string, numeric bool, numberFormat int) string {
	if !numeric || numberFormat != 4 {
		return value
	}
	number, err := strconv.ParseFloat(strings.TrimSpace(value), 64)
	if err != nil {
		return value
	}
	formatted := strconv.FormatFloat(number, 'f', 2, 64)
	parts := strings.SplitN(formatted, ".", 2)
	digits := parts[0]
	sign := ""
	if strings.HasPrefix(digits, "-") {
		sign, digits = "-", digits[1:]
	}
	for index := len(digits) - 3; index > 0; index -= 3 {
		digits = digits[:index] + "," + digits[index:]
	}
	return sign + digits + "." + parts[1]
}
