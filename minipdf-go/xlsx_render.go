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
	font      xlsxFont
	fill      *PDFColor
	border    xlsxBorder
	numberFmt int
	align     string
	vertical  string
}

type xlsxFont struct {
	name  string
	size  float64
	color PDFColor
	bold  bool
}

type xlsxBorder struct {
	left, right, top, bottom float64
}

type xlsxCell struct {
	row, column int
	value       string
	formula     string
	style       int
	numeric     bool
}

type xlsxMerge struct {
	startRow, startColumn int
	endRow, endColumn     int
}

type xlsxGrid struct {
	cells             map[[2]int]xlsxCell
	columnWidths      map[int]float64
	rowHeights        map[int]float64
	merges            []xlsxMerge
	charts            []xlsxChart
	maxRow, maxColumn int
	dimensionRow      int
	dimensionColumn   int
	defaultRowHeight  float64
	pageSize          PageSize
	margins           Margins
}

type xlsxStylesDocument struct {
	Fonts struct {
		Items []struct {
			Bold *struct{} `xml:"b"`
			Name struct {
				Value string `xml:"val,attr"`
			} `xml:"name"`
			Size struct {
				Value float64 `xml:"val,attr"`
			} `xml:"sz"`
			Color struct {
				RGB string `xml:"rgb,attr"`
			} `xml:"color"`
		} `xml:"font"`
	} `xml:"fonts"`
	Fills struct {
		Items []struct {
			Pattern struct {
				Type  string `xml:"patternType,attr"`
				Color struct {
					RGB string `xml:"rgb,attr"`
				} `xml:"fgColor"`
			} `xml:"patternFill"`
		} `xml:"fill"`
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
			} `xml:"alignment"`
		} `xml:"xf"`
	} `xml:"cellXfs"`
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

func readXLSXStyles(files officePackage) ([]xlsxStyle, error) {
	if _, ok := files["xl/styles.xml"]; !ok {
		return nil, nil
	}
	data, err := files.read("xl/styles.xml")
	if err != nil {
		return nil, err
	}
	var document xlsxStylesDocument
	if err := xml.Unmarshal(data, &document); err != nil {
		return nil, fmt.Errorf("parse xl/styles.xml: %w", err)
	}
	styles := make([]xlsxStyle, len(document.CellFormats.Items))
	for index, format := range document.CellFormats.Items {
		style := xlsxStyle{
			font: xlsxFont{size: 11, color: PDFColorBlack}, numberFmt: format.NumberFmt,
			align: format.Alignment.Horizontal, vertical: format.Alignment.Vertical,
		}
		if format.FontID >= 0 && format.FontID < len(document.Fonts.Items) {
			font := document.Fonts.Items[format.FontID]
			style.font.name = font.Name.Value
			if font.Size.Value > 0 {
				style.font.size = font.Size.Value
			}
			style.font.bold = font.Bold != nil
			if color, ok := parseXLSXColor(font.Color.RGB); ok {
				style.font.color = color
			}
		}
		if format.FillID >= 0 && format.FillID < len(document.Fills.Items) {
			fill := document.Fills.Items[format.FillID].Pattern
			if fill.Type == "solid" {
				if color, ok := parseXLSXColor(fill.Color.RGB); ok {
					style.fill = &color
				}
			}
		}
		if format.BorderID >= 0 && format.BorderID < len(document.Borders.Items) {
			border := document.Borders.Items[format.BorderID]
			style.border = xlsxBorder{
				left: borderWidth(border.Left.Style), right: borderWidth(border.Right.Style),
				top: borderWidth(border.Top.Style), bottom: borderWidth(border.Bottom.Style),
			}
		}
		styles[index] = style
	}
	return styles, nil
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

func renderXLSXWorksheets(files officePackage, worksheetNames []string, sharedStrings []string, styles []xlsxStyle, options ConversionOptions) ([]byte, error) {
	document := NewPDFDocument()
	for _, name := range worksheetNames {
		data, err := files.read(name)
		if err != nil {
			return nil, err
		}
		grid, err := readXLSXGrid(data, sharedStrings)
		if err != nil {
			return nil, fmt.Errorf("parse %s: %w", name, err)
		}
		grid.charts, err = readXLSXCharts(files, name, &grid)
		if err != nil {
			return nil, err
		}
		grid.applyOptions(options)
		renderXLSXGrid(document, grid, styles)
	}
	return document.BytesWithOptions(PDFSaveOptions{Compress: options.Compress}), nil
}

func readXLSXGrid(data []byte, sharedStrings []string) (xlsxGrid, error) {
	grid := xlsxGrid{
		cells: make(map[[2]int]xlsxCell), columnWidths: make(map[int]float64), rowHeights: make(map[int]float64),
		defaultRowHeight: 15, pageSize: PageSizeA4, margins: Margins{Left: 54, Right: 54, Top: 72, Bottom: 72},
	}
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
			continue
		}
		switch start.Name.Local {
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
			if grid.dimensionColumn > 0 {
				maximum = min(maximum, grid.dimensionColumn)
			}
			for column := minimum; column <= maximum && column > 0; column++ {
				grid.columnWidths[column] = width * xlsxColumnWidthPoints
			}
		case "row":
			row, _ := strconv.Atoi(attrValue(start, "r"))
			if height, err := strconv.ParseFloat(attrValue(start, "ht"), 64); err == nil && height > 0 {
				grid.rowHeights[row] = height
			}
			grid.maxRow = max(grid.maxRow, row)
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
			grid.margins = Margins{
				Left: inchesToPoints(attrValue(start, "left"), 54), Right: inchesToPoints(attrValue(start, "right"), 54),
				Top: inchesToPoints(attrValue(start, "top"), 72), Bottom: inchesToPoints(attrValue(start, "bottom"), 72),
			}
		case "pageSetup":
			if attrValue(start, "paperSize") == "1" {
				grid.pageSize = PageSizeLetter
			}
			if attrValue(start, "orientation") == "landscape" {
				grid.pageSize.Width, grid.pageSize.Height = grid.pageSize.Height, grid.pageSize.Width
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
	if width := grid.columnWidths[column]; width > 0 {
		return width
	}
	return 8.43 * xlsxColumnWidthPoints
}

func (grid xlsxGrid) rowHeight(row int) float64 {
	if height := grid.rowHeights[row]; height > 0 {
		return height
	}
	return grid.defaultRowHeight * xlsxDefaultRowHeightScale
}

func renderXLSXBand(page *PDFPage, grid xlsxGrid, styles []xlsxStyle, rows, columns xlsxRange) {
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
		renderXLSXCell(page, cell, xPositions[startColumn], yPositions[endRow+1], xPositions[endColumn+1]-xPositions[startColumn], yPositions[startRow]-yPositions[endRow+1], styleAt(styles, cell.style), merge.startColumn >= columns.start && merge.startColumn <= columns.end)
	}
	for row := rows.start; row <= rows.end; row++ {
		for column := columns.start; column <= columns.end; column++ {
			if merged[[2]int{row, column}] {
				continue
			}
			cell, exists := grid.cells[[2]int{row, column}]
			if !exists {
				continue
			}
			renderXLSXCell(page, cell, xPositions[column], yPositions[row+1], grid.columnWidth(column), grid.rowHeight(row), styleAt(styles, cell.style), true)
		}
	}
	renderXLSXCharts(page, grid, rows, columns)
}

func styleAt(styles []xlsxStyle, index int) xlsxStyle {
	if index >= 0 && index < len(styles) {
		return styles[index]
	}
	return xlsxStyle{font: xlsxFont{size: 11, color: PDFColorBlack}}
}

func renderXLSXCell(page *PDFPage, cell xlsxCell, x, y, width, height float64, style xlsxStyle, drawText bool) {
	if style.fill != nil {
		page.AddRect(x, y, width, height, *style.fill)
	}
	if style.border.left > 0 {
		page.AddLine(x, y, x, y+height, PDFColorBlack, style.border.left)
	}
	if style.border.right > 0 {
		page.AddLine(x+width, y, x+width, y+height, PDFColorBlack, style.border.right)
	}
	if style.border.top > 0 {
		page.AddLine(x, y+height, x+width, y+height, PDFColorBlack, style.border.top)
	}
	if style.border.bottom > 0 {
		page.AddLine(x, y, x+width, y, PDFColorBlack, style.border.bottom)
	}
	if !drawText || cell.value == "" {
		return
	}
	text := formatXLSXValue(cell.value, cell.numeric, style.numberFmt)
	fontSize := style.font.size
	horizontalScale := 100.0
	if strings.EqualFold(style.font.name, "Calibri") {
		fontSize *= calibriToHelveticaFontScale
		horizontalScale = calibriRegularWidthScale
		if style.font.bold {
			horizontalScale = calibriBoldWidthScale
		}
	}
	textWidth := measureHelveticaText(text, fontSize, horizontalScale, style.font.bold)
	textX := x + 2
	switch style.align {
	case "center":
		textX = x + (width-textWidth)/2
	case "right":
		textX = x + width - textWidth - 2
	default:
		if cell.numeric {
			textX = x + width - textWidth - 2
		}
	}
	textY := y + fontSize*helveticaBaselineRatio
	if style.vertical == "center" {
		textY = y + (height-fontSize)/2
	} else if style.vertical == "top" {
		textY = y + height - fontSize
	}
	page.addScaledText(text, textX, textY, fontSize, horizontalScale, style.font.color, style.font.bold)
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
