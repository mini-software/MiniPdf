package minipdf

import (
	"fmt"
	"math"
	"strings"
)

type xlsxPrintSettings struct {
	areas         []xlsxMerge
	rows, columns xlsxRange
	date1904      bool
	sheetName     string
}

func parseXLSXPrintRanges(value string) ([]xlsxMerge, error) {
	var references []string
	start := 0
	quoted := false
	for index, character := range value {
		switch character {
		case '\'':
			quoted = !quoted
		case ',':
			if !quoted {
				references = append(references, value[start:index])
				start = index + 1
			}
		}
	}
	if quoted {
		return nil, fmt.Errorf("unclosed sheet name in print range %q", value)
	}
	references = append(references, value[start:])
	var ranges []xlsxMerge
	for _, reference := range references {
		reference = strings.TrimSpace(reference)
		if index := strings.LastIndex(reference, "!"); index >= 0 {
			reference = reference[index+1:]
		}
		reference = strings.ReplaceAll(reference, "$", "")
		parts := strings.Split(reference, ":")
		if len(parts) == 1 {
			parts = append(parts, parts[0])
		}
		if len(parts) != 2 {
			return nil, fmt.Errorf("unsupported print range %q", reference)
		}
		startRow, startColumn := xlsxCellPosition(parts[0])
		endRow, endColumn := xlsxCellPosition(parts[1])
		switch {
		case startRow == 0 && endRow == 0 && startColumn > 0 && endColumn > 0 &&
			strings.Trim(parts[0], "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") == "" &&
			strings.Trim(parts[1], "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") == "":
			startRow, endRow = 1, 1048576
		case startColumn == 0 && endColumn == 0 && startRow > 0 && endRow > 0:
			startColumn, endColumn = 1, 16384
		}
		if startRow <= 0 || startColumn <= 0 || endRow < startRow || endColumn < startColumn ||
			endRow > 1048576 || endColumn > 16384 {
			return nil, fmt.Errorf("invalid print range %q", reference)
		}
		ranges = append(ranges, xlsxMerge{startRow: startRow, endRow: endRow, startColumn: startColumn, endColumn: endColumn})
	}
	return ranges, nil
}

func xlsxRangeSize(span xlsxRange, size func(int) float64) float64 {
	total := 0.0
	for index := span.start; index > 0 && index <= span.end; index++ {
		total += size(index)
	}
	return total
}

func xlsxPrintBands(span xlsxRange, available float64, size func(int) float64, titles xlsxRange, breaks map[int]bool) ([]xlsxRange, error) {
	var bands []xlsxRange
	titleSize := xlsxRangeSize(titles, size)
	for start := span.start; start <= span.end; {
		capacity := available
		if titles.start > 0 && start > titles.end {
			capacity -= titleSize
		}
		if capacity <= 0 {
			return nil, fmt.Errorf("XLSX print titles exceed printable page size")
		}
		end := start
		used := 0.0
		for end <= span.end {
			if end > start && (breaks[end-1] || used+size(end) > capacity+1e-6) {
				break
			}
			used += size(end)
			end++
		}
		bands = append(bands, xlsxRange{start: start, end: end - 1})
		start = end
	}
	return bands, nil
}

func limitXLSXPrintArea(grid xlsxGrid, area xlsxMerge, options ConversionOptions) xlsxMerge {
	if area.endRow == 1048576 {
		area.endRow = grid.maxRow
	}
	if area.endColumn == 16384 {
		area.endColumn = grid.maxColumn
	}
	if options.MaxRows > 0 {
		area.endRow = min(area.endRow, options.MaxRows)
	}
	if options.MaxColumns > 0 {
		area.endColumn = min(area.endColumn, options.MaxColumns)
	}
	return area
}

func renderXLSXPrintGrid(document *PDFDocument, grid xlsxGrid, styles []xlsxStyle, settings xlsxPrintSettings, options ConversionOptions) error {
	grid.printLayout = true
	firstPage := len(document.pages)
	availableWidth := grid.pageSize.Width - grid.margins.Left - grid.margins.Right
	availableHeight := grid.pageSize.Height - grid.margins.Top - grid.margins.Bottom
	if availableWidth <= 0 || availableHeight <= 0 {
		return fmt.Errorf("XLSX margins leave no printable page area")
	}
	for _, area := range settings.areas {
		area = limitXLSXPrintArea(grid, area, options)
		if area.endRow < area.startRow || area.endColumn < area.startColumn {
			continue
		}
		printGrid := grid
		rowBreaks, columnBreaks := grid.rowBreaks, grid.columnBreaks
		if grid.fitToPage {
			printGrid.printScale = 1
			scale := math.Inf(1)
			if grid.fitWidth > 0 {
				width := xlsxRangeSize(xlsxRange{start: area.startColumn, end: area.endColumn}, printGrid.columnWidth)
				if settings.columns.start > 0 {
					repeats := grid.fitWidth - 1
					if area.startColumn > settings.columns.end {
						repeats++
					}
					width += xlsxRangeSize(settings.columns, printGrid.columnWidth) * float64(repeats)
				}
				if width > 0 {
					scale = min(scale, availableWidth*float64(grid.fitWidth)/width)
				}
			}
			if grid.fitHeight > 0 {
				height := xlsxRangeSize(xlsxRange{start: area.startRow, end: area.endRow}, printGrid.rowHeight)
				if settings.rows.start > 0 {
					repeats := grid.fitHeight - 1
					if area.startRow > settings.rows.end {
						repeats++
					}
					height += xlsxRangeSize(settings.rows, printGrid.rowHeight) * float64(repeats)
				}
				if height > 0 {
					scale = min(scale, availableHeight*float64(grid.fitHeight)/height)
				}
			}
			if !math.IsInf(scale, 1) {
				if scale <= 0 {
					return fmt.Errorf("XLSX print titles leave no fit-to-page space")
				}
				printGrid.printScale = min(1, scale)
			}
			rowBreaks, columnBreaks = nil, nil
		}
		columnBands, err := xlsxPrintBands(xlsxRange{start: area.startColumn, end: area.endColumn}, availableWidth, printGrid.columnWidth, settings.columns, columnBreaks)
		if err != nil {
			return err
		}
		rowBands, err := xlsxPrintBands(xlsxRange{start: area.startRow, end: area.endRow}, availableHeight, printGrid.rowHeight, settings.rows, rowBreaks)
		if err != nil {
			return err
		}
		renderBand := func(rowBand, columnBand xlsxRange) {
			page := document.AddPage(grid.pageSize.Width, grid.pageSize.Height)
			bandGrid := printGrid
			if grid.horizontalCentered {
				width := xlsxRangeSize(columnBand, printGrid.columnWidth)
				if settings.columns.start > 0 && columnBand.start > settings.columns.end {
					width += xlsxRangeSize(settings.columns, printGrid.columnWidth)
				}
				bandGrid.margins.Left += max(0, (availableWidth-width)/2)
			}
			if grid.verticalCentered {
				height := xlsxRangeSize(rowBand, printGrid.rowHeight)
				if settings.rows.start > 0 && rowBand.start > settings.rows.end {
					height += xlsxRangeSize(settings.rows, printGrid.rowHeight)
				}
				bandGrid.margins.Top += max(0, (availableHeight-height)/2)
			}
			renderXLSXPrintBand(page, bandGrid, styles, rowBand, columnBand, settings)
			localPage := len(document.pages) - firstPage
			page.decorations = xlsxFooterDecoration(grid, printGrid.layoutScale(), localPage)
		}
		if grid.overThenDown {
			for _, rowBand := range rowBands {
				for _, columnBand := range columnBands {
					renderBand(rowBand, columnBand)
				}
			}
		} else {
			for _, columnBand := range columnBands {
				for _, rowBand := range rowBands {
					renderBand(rowBand, columnBand)
				}
			}
		}
	}
	return nil
}

func renderXLSXPrintBand(page *PDFPage, grid xlsxGrid, styles []xlsxStyle, rows, columns xlsxRange, settings xlsxPrintSettings) {
	body := grid
	repeatRows := settings.rows.start > 0 && rows.start > settings.rows.end
	repeatColumns := settings.columns.start > 0 && columns.start > settings.columns.end
	if repeatRows {
		body.margins.Top += xlsxRangeSize(settings.rows, grid.rowHeight)
	}
	if repeatColumns {
		body.margins.Left += xlsxRangeSize(settings.columns, grid.columnWidth)
	}
	if repeatRows {
		header := grid
		header.margins.Left = body.margins.Left
		renderXLSXBand(page, header, styles, settings.rows, columns)
	}
	if repeatColumns {
		header := grid
		header.margins.Top = body.margins.Top
		renderXLSXBand(page, header, styles, rows, settings.columns)
	}
	if repeatRows && repeatColumns {
		renderXLSXBand(page, grid, styles, settings.rows, settings.columns)
	}
	renderXLSXBand(page, body, styles, rows, columns)
}
