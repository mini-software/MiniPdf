package minipdf

import (
	"bytes"
	"fmt"
	"strings"
	"testing"
)

func TestXLSXConditionalResultFillsAreDataDriven(t *testing.T) {
	for _, result := range []string{"ACCEPT", "reject", "hold", "pending"} {
		t.Run(result, func(t *testing.T) {
			worksheet := `<worksheet><sheetData>` +
				`<row r="2"><c r="E2" t="inlineStr"><is><t>Title</t></is></c><c r="F2"/></row>` +
				fmt.Sprintf(`<row r="16"><c r="H16" t="inlineStr"><is><t>%s</t></is></c></row>`, result) +
				`</sheetData><conditionalFormatting sqref="E2:F2">` +
				`<cfRule type="expression" dxfId="0" priority="1"><formula>ISNUMBER(SEARCH("accept",$H$16))=TRUE</formula></cfRule>` +
				`<cfRule type="expression" dxfId="0" priority="2"><formula>ISNUMBER(SEARCH("reject",$H$16))=TRUE</formula></cfRule>` +
				`<cfRule type="expression" dxfId="0" priority="3"><formula>ISNUMBER(SEARCH("hold",$H$16))=TRUE</formula></cfRule>` +
				`</conditionalFormatting><conditionalFormatting sqref="H16">` +
				`<cfRule type="containsText" text="Accept" dxfId="1" priority="4"/>` +
				`<cfRule type="containsText" text="reject" dxfId="2" priority="5"/>` +
				`<cfRule type="containsText" text="hold" dxfId="3" priority="6"/>` +
				`</conditionalFormatting></worksheet>`
			grid, err := readXLSXGrid([]byte(worksheet), nil)
			if err != nil {
				t.Fatal(err)
			}
			red, _ := parseXLSXColor("FF0000")
			green, _ := parseXLSXColor("92D050")
			yellow, _ := parseXLSXColor("FFFF00")
			white := PDFColorWhite
			styles, err := grid.applyConditionalFills([]xlsxStyle{{fill: &red}}, []*PDFColor{&white, &green, &red, &yellow}, nil)
			if err != nil {
				t.Fatal(err)
			}
			wantTitle, wantResult := white, green
			switch result {
			case "reject":
				wantResult = red
			case "hold":
				wantResult = yellow
			case "pending":
				wantTitle, wantResult = red, red
			}
			for _, key := range [][2]int{{2, 5}, {2, 6}, {16, 8}} {
				want := wantTitle
				if key[0] == 16 {
					want = wantResult
				}
				style := styleAt(styles, grid.cells[key].style)
				if style.fill == nil || *style.fill != want {
					t.Errorf("cell %v fill = %v, want %v", key, style.fill, want)
				}
			}
		})
	}
}

func TestXLSXConditionalFillsPriorityStopAndRelativeReferences(t *testing.T) {
	for _, test := range []struct {
		name, rules string
		want        PDFColor
	}{
		{"priority", `<cfRule type="containsText" text="match" dxfId="1" priority="2"/><cfRule type="containsText" text="match" dxfId="0" priority="1"/>`, PDFColorWhite},
		{"stop without fill", `<cfRule type="containsText" text="match" priority="1" stopIfTrue="1"/><cfRule type="containsText" text="match" dxfId="0" priority="2"/>`, PDFColorBlack},
		{"relative column", `<cfRule type="expression" dxfId="0" priority="1"><formula>ISNUMBER(SEARCH("match",B1))</formula></cfRule>`, PDFColorWhite},
		{"mixed absolute row", `<cfRule type="expression" dxfId="0" priority="1"><formula>ISNUMBER(SEARCH("match",B$1))</formula></cfRule>`, PDFColorWhite},
	} {
		t.Run(test.name, func(t *testing.T) {
			grid, err := readXLSXGrid([]byte(`<worksheet><sheetData><row r="1">`+
				`<c r="A1" t="inlineStr"><is><t>match</t></is></c><c r="B1" t="inlineStr"><is><t>match</t></is></c>`+
				`<c r="C1" t="inlineStr"><is><t>match</t></is></c></row></sheetData>`+
				`<conditionalFormatting sqref="A1:B1">`+test.rules+`</conditionalFormatting></worksheet>`), nil)
			if err != nil {
				t.Fatal(err)
			}
			black, white := PDFColorBlack, PDFColorWhite
			styles, err := grid.applyConditionalFills([]xlsxStyle{{fill: &black}}, []*PDFColor{&white, &black}, nil)
			if err != nil {
				t.Fatal(err)
			}
			for _, column := range []int{1, 2} {
				if fill := styleAt(styles, grid.cells[[2]int{1, column}].style).fill; fill == nil || *fill != test.want {
					t.Errorf("column %d fill = %v, want %v", column, fill, test.want)
				}
			}
			if fill := styleAt(styles, grid.cells[[2]int{1, 3}].style).fill; *fill != black {
				t.Fatal("conditional fill applied outside sqref")
			}
		})
	}
}

func TestXLSXConditionalNumericBoundsAndErrors(t *testing.T) {
	for _, operator := range []string{"between", "notBetween"} {
		grid, err := readXLSXGrid([]byte(`<worksheet><sheetData><row r="1"><c r="A1"><v>5</v></c><c r="B1"><v>15</v></c></row></sheetData>`+
			`<conditionalFormatting sqref="A1:B1"><cfRule type="cellIs" operator="`+operator+`" dxfId="0" priority="1">`+
			`<formula>0</formula><formula>10</formula></cfRule></conditionalFormatting></worksheet>`), nil)
		if err != nil {
			t.Fatal(err)
		}
		black, white := PDFColorBlack, PDFColorWhite
		styles, err := grid.applyConditionalFills([]xlsxStyle{{fill: &black}}, []*PDFColor{&white}, nil)
		if err != nil {
			t.Fatal(err)
		}
		for _, column := range []int{1, 2} {
			want := black
			if (operator == "between" && column == 1) || (operator == "notBetween" && column == 2) {
				want = white
			}
			if *styleAt(styles, grid.cells[[2]int{1, column}].style).fill != want {
				t.Errorf("%s column %d did not match numeric bounds", operator, column)
			}
		}
	}
	for _, rule := range []string{`dxfId="0" priority="0"`, `dxfId="10" priority="1"`} {
		grid, err := readXLSXGrid([]byte(`<worksheet><conditionalFormatting sqref="A1"><cfRule type="containsText" text="x" `+rule+`/></conditionalFormatting></worksheet>`), nil)
		if err != nil {
			t.Fatal(err)
		}
		if _, err := grid.applyConditionalFills(nil, nil, nil); err == nil {
			t.Fatalf("invalid rule %s accepted", rule)
		}
	}
	for _, reference := range []string{"A0:B1", "R1C1:R2C2", "A1:XFE1", "A1:A1048577", ""} {
		_, err := readXLSXGrid([]byte(`<worksheet><conditionalFormatting sqref="`+reference+`"/></worksheet>`), nil)
		if err == nil || !strings.Contains(err.Error(), "range") {
			t.Fatalf("invalid range %q error = %v", reference, err)
		}
	}
}

func TestXLSXConditionalSearchWildcards(t *testing.T) {
	for _, test := range []struct {
		pattern, value string
		want           bool
	}{
		{"ac*pt", "Status: ACCEPT", true}, {"ac?pt", "accept", false},
		{"ac??pt", "accept", true}, {"~*", "literal *", true},
		{"~*", "accept", false}, {"~~", "~", true}, {`"accept"`, `"ACCEPT"`, true},
	} {
		if got := matchesXLSXSearch(test.pattern, test.value); got != test.want {
			t.Errorf("SEARCH(%q, %q) = %v, want %v", test.pattern, test.value, got, test.want)
		}
	}
}

func TestConvertXLSXPrintedConditionalFills(t *testing.T) {
	entries := map[string]string{
		"xl/workbook.xml": `<workbook><sheets><sheet name="Report"/></sheets><definedNames>` +
			`<definedName name="_xlnm.Print_Area" localSheetId="0">Report!$A$1:$B$1</definedName></definedNames></workbook>`,
		"xl/styles.xml": `<styleSheet><fonts><font><sz val="11"/></font></fonts>` +
			`<fills><fill><patternFill patternType="solid"><fgColor rgb="FFFF0000"/></patternFill></fill></fills>` +
			`<cellXfs><xf fontId="0" fillId="0"/></cellXfs><dxfs><dxf><fill><patternFill>` +
			`<bgColor theme="0"/></patternFill></fill></dxf></dxfs></styleSheet>`,
		"xl/worksheets/sheet1.xml": `<worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>Accept</t></is></c></row></sheetData>` +
			`<conditionalFormatting sqref="A1:B1"><cfRule type="expression" dxfId="0" priority="1">` +
			`<formula>ISNUMBER(SEARCH("accept",$A$1))=TRUE</formula></cfRule></conditionalFormatting></worksheet>`,
	}
	pdf, err := ConvertBytesToPDF(officePackageBytes(t, entries))
	assertPDFContains(t, pdf, err, "Accept")
	if bytes.Contains(pdf, []byte("1 0 0 rg")) {
		t.Fatal("printed grid retained red base fill instead of conditional theme-white fill")
	}
	if bytes.Count(pdf, []byte("1 1 1 rg")) != 2 {
		t.Fatal("printed grid did not paint conditional fill on both stored and implicit blank cells")
	}
	if bytes.Count(pdf, []byte("/Type /Page ")) != 1 {
		t.Fatal("conditional styles changed the printed page count")
	}
	limited, err := ConvertBytesToPDFWithOptions(officePackageBytes(t, entries), ConversionOptions{MaxColumns: 1})
	if err != nil {
		t.Fatal(err)
	}
	if bytes.Count(limited, []byte("1 1 1 rg")) != 1 {
		t.Fatal("conditional fills ignored the printed column limit")
	}
}

func TestXLSXImplicitConditionalFillsStayWithinPrintedScope(t *testing.T) {
	for _, value := range []string{"Accept", "Pending"} {
		grid, err := readXLSXGrid([]byte(`<worksheet><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>`+
			value+`</t></is></c></row></sheetData><conditionalFormatting sqref="A1:XFD1048576">`+
			`<cfRule type="expression" dxfId="0" priority="1"><formula>ISNUMBER(SEARCH("accept",$A$1))</formula>`+
			`</cfRule></conditionalFormatting></worksheet>`), nil)
		if err != nil {
			t.Fatal(err)
		}
		white := PDFColorWhite
		_, err = grid.applyConditionalFills(nil, []*PDFColor{&white}, []xlsxMerge{{startRow: 1, endRow: 2, startColumn: 1, endColumn: 2}})
		if err != nil {
			t.Fatal(err)
		}
		wantCells := 1
		if value == "Accept" {
			wantCells = 4
		}
		if len(grid.cells) != wantCells || grid.maxRow != 1 || grid.maxColumn != 1 {
			t.Errorf("%s: cells = %d, dimensions = %d/%d; conditional fills expanded print scope or retained unmatched blanks",
				value, len(grid.cells), grid.maxRow, grid.maxColumn)
		}
	}
}

func TestXLSXConditionalMixedAbsoluteReferences(t *testing.T) {
	origin := xlsxMerge{startRow: 3, startColumn: 3}
	cell := xlsxCell{row: 5, column: 4}
	for _, test := range []struct {
		reference string
		want      [2]int
	}{
		{"A1", [2]int{3, 2}}, {"$A1", [2]int{3, 1}},
		{"A$1", [2]int{1, 2}}, {"$A$1", [2]int{1, 1}},
	} {
		if got, ok := conditionalXLSXReference(test.reference, cell, origin); !ok || got != test.want {
			t.Errorf("%s = %v/%v, want %v", test.reference, got, ok, test.want)
		}
	}
}
