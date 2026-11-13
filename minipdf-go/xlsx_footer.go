package minipdf

import (
	"fmt"
	"strconv"
	"strings"
)

type xlsxHeaderFooter struct {
	Odd              string `xml:"oddFooter"`
	Even             string `xml:"evenFooter"`
	First            string `xml:"firstFooter"`
	DifferentFirst   bool   `xml:"differentFirst,attr"`
	DifferentOddEven bool   `xml:"differentOddEven,attr"`
	ScaleWithDoc     *bool  `xml:"scaleWithDoc,attr"`
	runs             [3][]xlsxFooterRun
}

type xlsxFooterRun struct {
	text          string
	field         byte
	portion, line int
	font          xlsxFont
}

func parseXLSXFooter(value string, defaultFont xlsxFont, sheetName string) ([]xlsxFooterRun, error) {
	current := defaultFont
	if current.size <= 0 {
		current.size = 11
	}
	portion := 1
	lines := [3]int{}
	var text strings.Builder
	var runs []xlsxFooterRun
	flush := func() {
		if text.Len() > 0 {
			runs = append(runs, xlsxFooterRun{text: text.String(), portion: portion, line: lines[portion], font: current})
			text.Reset()
		}
	}
	for index := 0; index < len(value); index++ {
		switch value[index] {
		case '\n':
			flush()
			lines[portion]++
		case '&':
			index++
			if index >= len(value) {
				return nil, fmt.Errorf("incomplete XLSX footer control code")
			}
			switch token := value[index]; token {
			case '&':
				text.WriteByte('&')
			case 'L', 'C', 'R':
				flush()
				portion = map[byte]int{'L': 0, 'C': 1, 'R': 2}[token]
			case 'P', 'N':
				flush()
				runs = append(runs, xlsxFooterRun{field: token, portion: portion, line: lines[portion], font: current})
			case 'A':
				text.WriteString(sheetName)
			case 'B', 'I':
				flush()
				if token == 'B' {
					current.bold = !current.bold
				} else {
					current.italic = !current.italic
				}
				var err error
				current.embedded, err = readXLSXFont(current.name, current.bold, current.italic)
				if err != nil {
					return nil, err
				}
			case '"':
				flush()
				end := strings.IndexByte(value[index+1:], '"')
				if end < 0 {
					return nil, fmt.Errorf("unclosed XLSX footer font")
				}
				descriptor := strings.SplitN(value[index+1:index+1+end], ",", 2)
				current.name = descriptor[0]
				current.bold, current.italic = false, false
				if len(descriptor) > 1 {
					current.bold = strings.Contains(strings.ToLower(descriptor[1]), "bold")
					current.italic = strings.Contains(strings.ToLower(descriptor[1]), "italic")
				}
				var err error
				current.embedded, err = readXLSXFont(current.name, current.bold, current.italic)
				if err != nil {
					return nil, err
				}
				index += end + 1
			default:
				if token >= '0' && token <= '9' {
					flush()
					end := index + 1
					for end < len(value) && ((value[end] >= '0' && value[end] <= '9') || value[end] == '.') {
						end++
					}
					size, err := strconv.ParseFloat(value[index:end], 64)
					if err != nil || size <= 0 || size > 409 {
						return nil, fmt.Errorf("invalid XLSX footer font size %q", value[index:end])
					}
					current.size = size
					index = end - 1
				} else {
					return nil, fmt.Errorf("unsupported XLSX footer control code &%c", token)
				}
			}
		default:
			text.WriteByte(value[index])
		}
	}
	flush()
	return runs, nil
}

func (grid *xlsxGrid) prepareFooters(styles []xlsxStyle, sheetName string) error {
	defaultFont := styleAt(styles, 0).font
	for index, value := range []string{grid.headerFooter.Odd, grid.headerFooter.Even, grid.headerFooter.First} {
		runs, err := parseXLSXFooter(value, defaultFont, sheetName)
		if err != nil {
			return err
		}
		grid.headerFooter.runs[index] = runs
	}
	return nil
}

func xlsxFooterDecoration(grid xlsxGrid, scale float64, localPage int) func(int, int) []pdfOperation {
	if grid.headerFooter.Odd == "" && grid.headerFooter.Even == "" && grid.headerFooter.First == "" {
		return nil
	}
	if grid.headerFooter.ScaleWithDoc != nil && !*grid.headerFooter.ScaleWithDoc {
		scale = 1
	}
	return func(pageNumber, total int) []pdfOperation {
		runs := grid.headerFooter.runs[0]
		if grid.headerFooter.DifferentFirst && localPage == 1 {
			runs = grid.headerFooter.runs[2]
		} else if grid.headerFooter.DifferentOddEven && pageNumber%2 == 0 {
			runs = grid.headerFooter.runs[1]
		}
		type lineKey struct{ portion, line int }
		widths := make(map[lineKey]float64)
		counts := [3]int{}
		var operations []textOperation
		keys := make([]lineKey, 0, len(runs))
		lineHeight := 0.0
		for _, run := range runs {
			value := run.text
			if run.field == 'P' {
				value = strconv.Itoa(pageNumber)
			} else if run.field == 'N' {
				value = strconv.Itoa(total)
			}
			size := run.font.size * scale
			lineHeight = max(lineHeight, size*1.4)
			counts[run.portion] = max(counts[run.portion], run.line+1)
			embedded := run.font.embedded
			if embedded != nil && !embedded.canEncode(value) {
				embedded = nil
			}
			width := measureHelveticaText(value, size, 100, run.font.bold)
			if embedded != nil {
				width = measureXLSXFont(value, size, embedded)
			}
			key := lineKey{run.portion, run.line}
			operations = append(operations, textOperation{text: value, x: widths[key], fontSize: size, horizontalScale: 100, color: run.font.color,
				bold: run.font.bold, italic: run.font.italic, embedded: embedded})
			keys = append(keys, key)
			widths[key] += width
		}
		result := make([]pdfOperation, 0, len(operations))
		for index, operation := range operations {
			key := keys[index]
			switch key.portion {
			case 0:
				operation.x += grid.margins.Left
			case 1:
				operation.x += (grid.pageSize.Width - widths[key]) / 2
			case 2:
				operation.x += grid.pageSize.Width - grid.margins.Right - widths[key]
			}
			operation.y = grid.footerMargin + float64(counts[key.portion]-key.line-1)*lineHeight + operation.fontSize*0.3
			result = append(result, operation)
		}
		return result
	}
}
