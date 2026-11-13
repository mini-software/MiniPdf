package minipdf

import (
	"fmt"
	"regexp"
	"sort"
	"strconv"
	"strings"
)

type xlsxConditionalFormat struct {
	References string                `xml:"sqref,attr"`
	Rules      []xlsxConditionalRule `xml:"cfRule"`
	areas      []xlsxMerge
}

type xlsxConditionalRule struct {
	Type       string   `xml:"type,attr"`
	Operator   string   `xml:"operator,attr"`
	Text       string   `xml:"text,attr"`
	Format     *int     `xml:"dxfId,attr"`
	Priority   int      `xml:"priority,attr"`
	StopIfTrue bool     `xml:"stopIfTrue,attr"`
	Formulas   []string `xml:"formula"`
}

var xlsxConditionalSearch = regexp.MustCompile(`(?i)^\s*ISNUMBER\(\s*SEARCH\(\s*"((?:[^"]|"")*)"\s*,\s*(\$?[A-Z]+\$?[1-9][0-9]*)\s*\)\s*\)\s*(?:=\s*TRUE(?:\(\))?)?\s*$`)
var xlsxConditionalReference = regexp.MustCompile(`(?i)^(\$?)([A-Z]+)(\$?)([1-9][0-9]*)$`)

func (grid *xlsxGrid) applyConditionalFills(styles []xlsxStyle, fills []*PDFColor, scopes []xlsxMerge) ([]xlsxStyle, error) {
	type locatedRule struct {
		rule   xlsxConditionalRule
		format xlsxConditionalFormat
	}
	var rules []locatedRule
	for _, format := range grid.conditionalFills {
		for _, rule := range format.Rules {
			if rule.Priority <= 0 {
				return nil, fmt.Errorf("invalid XLSX conditional formatting priority %d", rule.Priority)
			}
			if rule.Format != nil && (*rule.Format < 0 || *rule.Format >= len(fills)) {
				return nil, fmt.Errorf("unknown XLSX differential format index %d", *rule.Format)
			}
			rules = append(rules, locatedRule{rule, format})
		}
	}
	sort.SliceStable(rules, func(i, j int) bool { return rules[i].rule.Priority < rules[j].rule.Priority })
	implicitCells := make(map[[2]int]bool)
	for _, format := range grid.conditionalFills {
		for _, area := range format.areas {
			for _, scope := range scopes {
				for row := max(area.startRow, scope.startRow); row <= min(area.endRow, scope.endRow); row++ {
					for column := max(area.startColumn, scope.startColumn); column <= min(area.endColumn, scope.endColumn); column++ {
						key := [2]int{row, column}
						if _, exists := grid.cells[key]; !exists {
							grid.cells[key] = xlsxCell{row: row, column: column, numeric: true}
							implicitCells[key] = true
						}
					}
				}
			}
		}
	}
	for key, cell := range grid.cells {
		var fill *PDFColor
		for _, located := range rules {
			inRange := false
			for _, area := range located.format.areas {
				if cell.row >= area.startRow && cell.row <= area.endRow && cell.column >= area.startColumn && cell.column <= area.endColumn {
					inRange = true
					break
				}
			}
			if !inRange || !grid.matchesConditionalRule(located.rule, cell, located.format.areas[0]) {
				continue
			}
			if fill == nil && located.rule.Format != nil {
				fill = fills[*located.rule.Format]
			}
			if located.rule.StopIfTrue {
				break
			}
		}
		if fill != nil {
			style := styleAt(styles, cell.style)
			style.fill = fill
			cell.style = len(styles)
			styles = append(styles, style)
			grid.cells[key] = cell
		} else if implicitCells[key] {
			delete(grid.cells, key)
		}
	}
	return styles, nil
}

func (grid *xlsxGrid) matchesConditionalRule(rule xlsxConditionalRule, cell xlsxCell, origin xlsxMerge) bool {
	switch rule.Type {
	case "containsText":
		return matchesXLSXSearch(rule.Text, cell.value)
	case "notContainsText":
		return !matchesXLSXSearch(rule.Text, cell.value)
	case "beginsWith":
		return strings.HasPrefix(strings.ToLower(cell.value), strings.ToLower(rule.Text))
	case "endsWith":
		return strings.HasSuffix(strings.ToLower(cell.value), strings.ToLower(rule.Text))
	case "expression":
		if len(rule.Formulas) != 1 {
			return false
		}
		match := xlsxConditionalSearch.FindStringSubmatch(rule.Formulas[0])
		if match == nil {
			return false
		}
		key, ok := conditionalXLSXReference(match[2], cell, origin)
		return ok && matchesXLSXSearch(strings.ReplaceAll(match[1], `""`, `"`), grid.cells[key].value)
	case "cellIs":
		if len(rule.Formulas) == 0 || !cell.numeric {
			return false
		}
		value, ok := grid.evaluateCell([2]int{cell.row, cell.column}, make(map[[2]int]bool))
		if !ok {
			return false
		}
		bounds := make([]float64, len(rule.Formulas))
		for index, formula := range rule.Formulas {
			if key, ok := conditionalXLSXReference(strings.TrimSpace(formula), cell, origin); ok {
				bounds[index], ok = grid.evaluateCell(key, make(map[[2]int]bool))
				if !ok {
					return false
				}
			} else {
				bound, err := strconv.ParseFloat(strings.TrimSpace(formula), 64)
				if err != nil {
					return false
				}
				bounds[index] = bound
			}
		}
		switch rule.Operator {
		case "equal":
			return len(bounds) == 1 && value == bounds[0]
		case "notEqual":
			return len(bounds) == 1 && value != bounds[0]
		case "lessThan":
			return len(bounds) == 1 && value < bounds[0]
		case "lessThanOrEqual":
			return len(bounds) == 1 && value <= bounds[0]
		case "greaterThan":
			return len(bounds) == 1 && value > bounds[0]
		case "greaterThanOrEqual":
			return len(bounds) == 1 && value >= bounds[0]
		case "between":
			return len(bounds) == 2 && value >= bounds[0] && value <= bounds[1]
		case "notBetween":
			return len(bounds) == 2 && (value < bounds[0] || value > bounds[1])
		}
	}
	// Other valid conditional-rule formulas retain the existing base style.
	return false
}

func matchesXLSXSearch(pattern, value string) bool {
	if !strings.ContainsAny(pattern, "*?~") {
		return strings.Contains(strings.ToLower(value), strings.ToLower(pattern))
	}
	var expression strings.Builder
	expression.WriteString("(?is)")
	characters := []rune(pattern)
	for index := 0; index < len(characters); index++ {
		switch character := characters[index]; character {
		case '*':
			expression.WriteString(".*")
		case '?':
			expression.WriteByte('.')
		case '~':
			if index+1 < len(characters) && strings.ContainsRune("*?~", characters[index+1]) {
				index++
				expression.WriteString(regexp.QuoteMeta(string(characters[index])))
			} else {
				expression.WriteByte('~')
			}
		default:
			expression.WriteString(regexp.QuoteMeta(string(character)))
		}
	}
	return regexp.MustCompile(expression.String()).MatchString(value)
}

func conditionalXLSXReference(reference string, cell xlsxCell, origin xlsxMerge) ([2]int, bool) {
	match := xlsxConditionalReference.FindStringSubmatch(reference)
	if match == nil {
		return [2]int{}, false
	}
	row, column := xlsxCellPosition(strings.ToUpper(match[2]) + match[4])
	if match[1] == "" {
		column += cell.column - origin.startColumn
	}
	if match[3] == "" {
		row += cell.row - origin.startRow
	}
	return [2]int{row, column}, row > 0 && row <= 1048576 && column > 0 && column <= 16384
}
