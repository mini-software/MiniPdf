package minipdf

import (
	"encoding/xml"
	"fmt"
	"math"
	"path"
	"strconv"
	"strings"
)

const xlsxEmuPerPoint = 12700

type xlsxChartSeries struct {
	name       string
	categories []string
	values     []float64
}

type xlsxChart struct {
	anchorRow, anchorColumn    int
	rowOffset, columnOffset    float64
	width, height              float64
	title, categoryTitle       string
	valueTitle, legendPosition string
	gapWidth                   float64
	series                     []xlsxChartSeries
}

type xlsxRelationshipsDocument struct {
	Items []struct {
		ID     string `xml:"Id,attr"`
		Type   string `xml:"Type,attr"`
		Target string `xml:"Target,attr"`
	} `xml:"Relationship"`
}

type xlsxDrawingMarker struct {
	Column       int   `xml:"col"`
	ColumnOffset int64 `xml:"colOff"`
	Row          int   `xml:"row"`
	RowOffset    int64 `xml:"rowOff"`
}

type xlsxChartAnchor struct {
	From      xlsxDrawingMarker `xml:"from"`
	To        xlsxDrawingMarker `xml:"to"`
	Extension struct {
		Width  int64 `xml:"cx,attr"`
		Height int64 `xml:"cy,attr"`
	} `xml:"ext"`
	Frame struct {
		Reference struct {
			ID string `xml:"id,attr"`
		} `xml:"graphic>graphicData>chart"`
	} `xml:"graphicFrame"`
}

type xlsxDrawingDocument struct {
	OneCell []xlsxChartAnchor `xml:"oneCellAnchor"`
	TwoCell []xlsxChartAnchor `xml:"twoCellAnchor"`
}

type xlsxChartTitleXML struct {
	Runs []string `xml:"tx>rich>p>r>t"`
}

type xlsxChartSeriesXML struct {
	Name        string `xml:"tx>v"`
	NameFormula string `xml:"tx>strRef>f"`
	CategoryNum string `xml:"cat>numRef>f"`
	CategoryStr string `xml:"cat>strRef>f"`
	Value       string `xml:"val>numRef>f"`
}

type xlsxChartDocument struct {
	Chart struct {
		Title xlsxChartTitleXML `xml:"title"`
		Plot  struct {
			Bar struct {
				Series   []xlsxChartSeriesXML `xml:"ser"`
				GapWidth struct {
					Value float64 `xml:"val,attr"`
				} `xml:"gapWidth"`
			} `xml:"barChart"`
			CategoryAxis struct {
				Title xlsxChartTitleXML `xml:"title"`
			} `xml:"catAx"`
			ValueAxis struct {
				Title xlsxChartTitleXML `xml:"title"`
			} `xml:"valAx"`
		} `xml:"plotArea"`
		Legend struct {
			Position struct {
				Value string `xml:"val,attr"`
			} `xml:"legendPos"`
		} `xml:"legend"`
	} `xml:"chart"`
}

func readXLSXCharts(files officePackage, worksheetName string, grid *xlsxGrid) ([]xlsxChart, error) {
	relationshipsName := xlsxRelationshipPartName(worksheetName)
	_, ok := files[relationshipsName]
	if !ok {
		return nil, nil
	}
	relationshipsData, err := files.read(relationshipsName)
	if err != nil {
		return nil, err
	}
	var relationships xlsxRelationshipsDocument
	if err := xml.Unmarshal(relationshipsData, &relationships); err != nil {
		return nil, fmt.Errorf("parse %s: %w", relationshipsName, err)
	}
	drawingName := ""
	for _, relationship := range relationships.Items {
		if strings.HasSuffix(relationship.Type, "/drawing") {
			drawingName = xlsxResolveRelationshipTarget(worksheetName, relationship.Target)
			break
		}
	}
	if drawingName == "" {
		return nil, nil
	}

	drawingData, err := files.read(drawingName)
	if err != nil {
		return nil, err
	}
	drawingRelationshipsName := xlsxRelationshipPartName(drawingName)
	drawingRelationshipsData, err := files.read(drawingRelationshipsName)
	if err != nil {
		return nil, err
	}
	var drawingRelationships xlsxRelationshipsDocument
	if err := xml.Unmarshal(drawingRelationshipsData, &drawingRelationships); err != nil {
		return nil, fmt.Errorf("parse %s: %w", drawingRelationshipsName, err)
	}
	chartParts := make(map[string]string)
	for _, relationship := range drawingRelationships.Items {
		if strings.HasSuffix(relationship.Type, "/chart") {
			chartParts[relationship.ID] = xlsxResolveRelationshipTarget(drawingName, relationship.Target)
		}
	}

	var drawing xlsxDrawingDocument
	if err := xml.Unmarshal(drawingData, &drawing); err != nil {
		return nil, fmt.Errorf("parse %s: %w", drawingName, err)
	}
	anchors := append(drawing.OneCell, drawing.TwoCell...)
	charts := make([]xlsxChart, 0, len(anchors))
	for _, anchor := range anchors {
		chartName := chartParts[anchor.Frame.Reference.ID]
		if chartName == "" {
			continue
		}
		width := float64(anchor.Extension.Width) / xlsxEmuPerPoint
		height := float64(anchor.Extension.Height) / xlsxEmuPerPoint
		if width <= 0 {
			width = grid.columnOffset(anchor.To.Column) - grid.columnOffset(anchor.From.Column)
		}
		if height <= 0 {
			height = grid.rowOffset(anchor.To.Row) - grid.rowOffset(anchor.From.Row)
		}
		if width <= 0 || height <= 0 {
			continue
		}
		chartData, err := files.read(chartName)
		if err != nil {
			return nil, err
		}
		chart, ok := parseXLSXChart(chartData, *grid)
		if !ok {
			continue
		}
		chart.anchorRow = anchor.From.Row
		chart.anchorColumn = anchor.From.Column
		chart.rowOffset = float64(anchor.From.RowOffset) / xlsxEmuPerPoint
		chart.columnOffset = float64(anchor.From.ColumnOffset) / xlsxEmuPerPoint
		chart.width, chart.height = width, height
		charts = append(charts, chart)
		grid.extendForChart(chart)
	}
	return charts, nil
}

func xlsxRelationshipPartName(partName string) string {
	return path.Join(path.Dir(partName), "_rels", path.Base(partName)+".rels")
}

func xlsxResolveRelationshipTarget(partName, target string) string {
	if strings.HasPrefix(target, "/") {
		return strings.TrimPrefix(path.Clean(target), "/")
	}
	return path.Clean(path.Join(path.Dir(partName), target))
}

func parseXLSXChart(data []byte, grid xlsxGrid) (xlsxChart, bool) {
	var document xlsxChartDocument
	if xml.Unmarshal(data, &document) != nil || len(document.Chart.Plot.Bar.Series) == 0 {
		return xlsxChart{}, false
	}
	chart := xlsxChart{
		title:          xlsxChartTitle(document.Chart.Title),
		categoryTitle:  xlsxChartTitle(document.Chart.Plot.CategoryAxis.Title),
		valueTitle:     xlsxChartTitle(document.Chart.Plot.ValueAxis.Title),
		legendPosition: document.Chart.Legend.Position.Value,
		gapWidth:       document.Chart.Plot.Bar.GapWidth.Value,
	}
	if chart.gapWidth <= 0 {
		chart.gapWidth = 150
	}
	for _, source := range document.Chart.Plot.Bar.Series {
		name := source.Name
		if name == "" {
			values := xlsxReferenceValues(grid, source.NameFormula)
			if len(values) > 0 {
				name = values[0]
			}
		}
		categoryFormula := source.CategoryStr
		if categoryFormula == "" {
			categoryFormula = source.CategoryNum
		}
		valueStrings := xlsxReferenceValues(grid, source.Value)
		values := make([]float64, len(valueStrings))
		for index, value := range valueStrings {
			values[index], _ = strconv.ParseFloat(strings.TrimSpace(value), 64)
		}
		chart.series = append(chart.series, xlsxChartSeries{
			name: name, categories: xlsxReferenceValues(grid, categoryFormula), values: values,
		})
	}
	return chart, len(chart.series) > 0
}

func xlsxChartTitle(title xlsxChartTitleXML) string {
	return strings.Join(title.Runs, "")
}

func xlsxReferenceValues(grid xlsxGrid, formula string) []string {
	if separator := strings.LastIndex(formula, "!"); separator >= 0 {
		formula = formula[separator+1:]
	}
	formula = strings.ReplaceAll(formula, "$", "")
	parts := strings.Split(formula, ":")
	if len(parts) == 0 || len(parts) > 2 {
		return nil
	}
	startRow, startColumn := xlsxCellPosition(parts[0])
	endRow, endColumn := startRow, startColumn
	if len(parts) == 2 {
		endRow, endColumn = xlsxCellPosition(parts[1])
	}
	if startRow == 0 || startColumn == 0 || endRow < startRow || endColumn < startColumn {
		return nil
	}
	values := make([]string, 0, (endRow-startRow+1)*(endColumn-startColumn+1))
	for row := startRow; row <= endRow; row++ {
		for column := startColumn; column <= endColumn; column++ {
			values = append(values, grid.cells[[2]int{row, column}].value)
		}
	}
	return values
}

func (grid xlsxGrid) columnOffset(columnCount int) float64 {
	offset := 0.0
	for column := 1; column <= columnCount; column++ {
		offset += grid.columnWidth(column)
	}
	return offset
}

func (grid xlsxGrid) rowOffset(rowCount int) float64 {
	offset := 0.0
	for row := 1; row <= rowCount; row++ {
		offset += grid.rowHeight(row)
	}
	return offset
}

func (grid *xlsxGrid) extendForChart(chart xlsxChart) {
	chartRight := grid.columnOffset(chart.anchorColumn) + chart.columnOffset + chart.width
	for column := chart.anchorColumn + 1; grid.columnOffset(column) < chartRight; column++ {
		grid.maxColumn = max(grid.maxColumn, column+1)
	}
	chartBottom := grid.rowOffset(chart.anchorRow) + chart.rowOffset + chart.height
	for row := chart.anchorRow + 1; grid.rowOffset(row) < chartBottom; row++ {
		grid.maxRow = max(grid.maxRow, row+1)
	}
}

type xlsxChartClip struct {
	page                     *PDFPage
	left, bottom, right, top float64
}

func (clip xlsxChartClip) line(x1, y1, x2, y2 float64, color PDFColor, width float64) {
	if y1 == y2 {
		if y1 < clip.bottom || y1 > clip.top {
			return
		}
		x1, x2 = max(x1, clip.left), min(x2, clip.right)
	} else if x1 == x2 {
		if x1 < clip.left || x1 > clip.right {
			return
		}
		y1, y2 = max(y1, clip.bottom), min(y2, clip.top)
	}
	if x2 >= x1 && y2 >= y1 {
		clip.page.AddLine(x1, y1, x2, y2, color, width)
	}
}

func (clip xlsxChartClip) rect(x, y, width, height float64, color PDFColor) {
	left, right := max(x, clip.left), min(x+width, clip.right)
	bottom, top := max(y, clip.bottom), min(y+height, clip.top)
	if right > left && top > bottom {
		clip.page.AddRect(left, bottom, right-left, top-bottom, color)
	}
}

func (clip xlsxChartClip) text(text string, x, y, fontSize float64, color PDFColor, bold bool) {
	if text == "" || x >= clip.right || y < clip.bottom || y > clip.top {
		return
	}
	width := float64(len([]rune(text))) * fontSize * 0.52
	if x+width <= clip.left {
		return
	}
	clip.page.AddText(text, x, y, fontSize, color, bold)
}

func renderXLSXCharts(page *PDFPage, grid xlsxGrid, rows, columns xlsxRange) {
	bandLeft := grid.columnOffset(columns.start - 1)
	bandTop := grid.rowOffset(rows.start - 1)
	bandWidth := grid.columnOffset(columns.end) - bandLeft
	bandHeight := grid.rowOffset(rows.end) - bandTop
	clip := xlsxChartClip{
		page: page, left: grid.margins.Left, right: grid.margins.Left + bandWidth,
		top:    grid.pageSize.Height - grid.margins.Top,
		bottom: grid.pageSize.Height - grid.margins.Top - bandHeight,
	}
	for _, chart := range grid.charts {
		chartLeft := grid.columnOffset(chart.anchorColumn) + chart.columnOffset
		chartTop := grid.rowOffset(chart.anchorRow) + chart.rowOffset
		if chartLeft >= bandLeft+bandWidth || chartLeft+chart.width <= bandLeft ||
			chartTop >= bandTop+bandHeight || chartTop+chart.height <= bandTop {
			continue
		}
		x := grid.margins.Left + chartLeft - bandLeft
		top := grid.pageSize.Height - grid.margins.Top - (chartTop - bandTop)
		renderXLSXClusteredChart(clip, chart, x, top)
	}
}

func renderXLSXClusteredChart(clip xlsxChartClip, chart xlsxChart, x, top float64) {
	border := PDFColor{Red: 0.65, Green: 0.65, Blue: 0.65}
	clip.line(x, top, x+chart.width, top, border, 0.5)
	clip.line(x+chart.width, top-chart.height, x+chart.width, top, border, 0.5)
	clip.line(x, top-chart.height, x+chart.width, top-chart.height, border, 0.5)
	clip.line(x, top-chart.height, x, top, border, 0.5)

	const fontSize = 8.0
	titleWidth := float64(len([]rune(chart.title))) * fontSize * 0.52
	clip.text(chart.title, x+(chart.width-titleWidth)/2, top-18, fontSize, PDFColorBlack, true)
	legendWidth := 0.0
	if chart.legendPosition == "r" || chart.legendPosition == "tr" {
		legendWidth = 52
	}
	plotLeft := x + 48
	plotRight := x + chart.width - 16 - legendWidth
	plotTop := top - 34
	plotBottom := top - chart.height + 36
	plotWidth, plotHeight := plotRight-plotLeft, plotTop-plotBottom
	if plotWidth <= 20 || plotHeight <= 20 {
		return
	}

	maximum := 0.0
	for _, series := range chart.series {
		for _, value := range series.values {
			maximum = max(maximum, value)
		}
	}
	axisMaximum, step := xlsxChartAxis(maximum)
	gridColor := PDFColor{Red: 0.82, Green: 0.82, Blue: 0.82}
	for value := 0.0; value <= axisMaximum+step*0.01; value += step {
		y := plotBottom + value/axisMaximum*plotHeight
		clip.line(plotLeft, y, plotRight, y, gridColor, 0.5)
	}

	categories := 0
	for _, series := range chart.series {
		categories = max(categories, len(series.values))
	}
	if categories > 0 {
		groupWidth := plotWidth / float64(categories)
		barWidth := groupWidth / (float64(len(chart.series)) + chart.gapWidth/100)
		groupPadding := (groupWidth - barWidth*float64(len(chart.series))) / 2
		colors := []PDFColor{
			{Red: 0.31, Green: 0.506, Blue: 0.741},
			{Red: 0.753, Green: 0.314, Blue: 0.302},
		}
		for category := 0; category < categories; category++ {
			for seriesIndex, series := range chart.series {
				if category >= len(series.values) {
					continue
				}
				height := max(0.5, series.values[category]/axisMaximum*plotHeight)
				barX := plotLeft + float64(category)*groupWidth + groupPadding + float64(seriesIndex)*barWidth
				clip.rect(barX, plotBottom, barWidth, height, colors[seriesIndex%len(colors)])
			}
		}
	}
	clip.line(plotLeft, plotBottom, plotLeft, plotTop, PDFColorBlack, 0.8)
	clip.line(plotLeft, plotBottom, plotRight, plotBottom, PDFColorBlack, 0.8)
	clip.text(chart.valueTitle, x+3, plotBottom+plotHeight*0.45, 7, PDFColorBlack, true)
	categoryWidth := float64(len([]rune(chart.categoryTitle))) * 7 * 0.52
	clip.text(chart.categoryTitle, plotLeft+(plotWidth-categoryWidth)/2, plotBottom-18, 7, PDFColorBlack, true)

	if legendWidth > 0 {
		legendX := plotRight + 12
		legendY := plotBottom + plotHeight*0.5
		colors := []PDFColor{
			{Red: 0.31, Green: 0.506, Blue: 0.741},
			{Red: 0.753, Green: 0.314, Blue: 0.302},
		}
		for index, series := range chart.series {
			y := legendY - float64(index)*16
			clip.rect(legendX, y, 7, 7, colors[index%len(colors)])
			clip.text(series.name, legendX+11, y, 7, PDFColorBlack, false)
		}
	}
}

func xlsxChartAxis(maximum float64) (float64, float64) {
	if maximum <= 0 {
		return 1, 1
	}
	roughStep := maximum / 7
	magnitude := math.Pow(10, math.Floor(math.Log10(roughStep)))
	fraction := roughStep / magnitude
	niceFraction := 1.0
	if fraction > 1.5 {
		niceFraction = 2
	}
	if fraction > 3 {
		niceFraction = 5
	}
	if fraction > 7 {
		niceFraction = 10
	}
	step := niceFraction * magnitude
	return math.Ceil(maximum/step) * step, step
}
