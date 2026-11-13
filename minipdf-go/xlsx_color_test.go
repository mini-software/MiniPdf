package minipdf

import (
	"math"
	"strings"
	"testing"
)

func xlsxStylePackage(t *testing.T, entries map[string]string) officePackage {
	t.Helper()
	files, err := openOfficePackage(officePackageBytes(t, entries))
	if err != nil {
		t.Fatal(err)
	}
	return files
}

func TestXLSXThemeColorsAndTint(t *testing.T) {
	theme, err := readXLSXTheme(xlsxStylePackage(t, map[string]string{"xl/styles.xml": "<styleSheet/>"}))
	if err != nil {
		t.Fatal(err)
	}
	for _, test := range []struct {
		index int
		tint  float64
		want  string
	}{
		{0, 0, "FFFFFF"}, {1, 0, "000000"}, {2, 0, "EEECE1"}, {3, 0, "1F497D"},
		{6, 0.6, "D7E4BD"}, {6, -0.5, "4F6228"}, {0, -0.5, "808080"},
		{1, 0.5, "808080"}, {4, -1, "000000"}, {4, 1, "FFFFFF"},
	} {
		color, err := resolveXLSXColor(xlsxColorXML{Theme: &test.index, Tint: test.tint}, theme)
		want, _ := parseXLSXColor(test.want)
		if err != nil || color == nil || *color != want {
			t.Errorf("theme %d tint %g = %v, %v; want %s", test.index, test.tint, color, err, test.want)
		}
	}
	color, err := resolveXLSXColor(xlsxColorXML{RGB: "FF92D050"}, theme)
	want, _ := parseXLSXColor("92D050")
	if err != nil || color == nil || *color != want {
		t.Fatalf("RGB = %v, %v; want %v", color, err, want)
	}
	for _, index := range []int{-1, 12} {
		if _, err := resolveXLSXColor(xlsxColorXML{Theme: &index}, theme); err == nil {
			t.Errorf("invalid theme index %d accepted", index)
		}
	}
	for _, tint := range []float64{-1.1, 1.1, math.NaN(), math.Inf(1)} {
		if _, err := resolveXLSXColor(xlsxColorXML{RGB: "FFFFFF", Tint: tint}, theme); err == nil {
			t.Errorf("invalid tint %v accepted", tint)
		}
	}
	if _, err := resolveXLSXColor(xlsxColorXML{RGB: "invalid"}, theme); err == nil {
		t.Fatal("invalid RGB accepted")
	}
	for _, test := range []struct{ source, want string }{
		{"FF0000", "FF8080"}, {"FFFF00", "FFFF80"}, {"00FF00", "80FF80"},
		{"00FFFF", "80FFFF"}, {"0000FF", "8080FF"}, {"FF00FF", "FF80FF"},
	} {
		color, err := resolveXLSXColor(xlsxColorXML{RGB: test.source, Tint: 0.5}, theme)
		want, _ := parseXLSXColor(test.want)
		if err != nil || color == nil || *color != want {
			t.Errorf("RGB %s tint 0.5 = %v, %v; want %s", test.source, color, err, test.want)
		}
	}
}

func TestXLSXStylesUseRelatedThemeForFontsFillsAndDifferentialFills(t *testing.T) {
	entries := map[string]string{
		"xl/_rels/workbook.xml.rels": `<Relationships><Relationship Type="urn:office/theme" Target="theme/custom.xml"/></Relationships>`,
		"xl/theme/custom.xml": `<theme><themeElements><clrScheme>` +
			`<dk1><sysClr val="windowText" lastClr="123456"/></dk1>` +
			`<lt1><sysClr val="window" lastClr="FFFFFF"/></lt1>` +
			`<accent3><srgbClr val="9BBB59"/></accent3></clrScheme></themeElements></theme>`,
		"xl/styles.xml": `<styleSheet><fonts><font><sz val="11"/><color theme="1"/></font></fonts>` +
			`<fills><fill><patternFill patternType="solid"><fgColor theme="6" tint="0.6"/></patternFill></fill></fills>` +
			`<cellXfs><xf fontId="0" fillId="0"/></cellXfs>` +
			`<dxfs><dxf><fill><patternFill><fgColor rgb="FF000000"/><bgColor theme="0"/></patternFill></fill></dxf></dxfs></styleSheet>`,
	}
	styles, fills, err := readXLSXStyles(xlsxStylePackage(t, entries))
	if err != nil {
		t.Fatal(err)
	}
	font, _ := parseXLSXColor("123456")
	fill, _ := parseXLSXColor("D7E4BD")
	if len(styles) != 1 || styles[0].font.color != font || styles[0].fill == nil || *styles[0].fill != fill {
		t.Fatalf("styles = %#v", styles)
	}
	if len(fills) != 1 || fills[0] == nil || *fills[0] != PDFColorWhite {
		t.Fatalf("differential fills = %#v", fills)
	}
	for _, test := range []struct {
		name, part, value, want string
	}{
		{"missing target", "xl/_rels/workbook.xml.rels", `<Relationships><Relationship Type="urn:office/theme" Target="theme/missing.xml"/></Relationships>`, "missing"},
		{"external target", "xl/_rels/workbook.xml.rels", `<Relationships><Relationship Type="urn:office/theme" Target="https://example.invalid/theme" TargetMode="External"/></Relationships>`, "invalid"},
		{"malformed theme", "xl/theme/custom.xml", "<theme>", "parse"},
		{"missing indexed theme entry", "xl/theme/custom.xml", "<theme/>", "unknown"},
		{"invalid theme color", "xl/theme/custom.xml", `<theme><clrScheme><lt1><srgbClr val="invalid"/></lt1></clrScheme></theme>`, "invalid"},
	} {
		t.Run(test.name, func(t *testing.T) {
			changed := make(map[string]string, len(entries))
			for name, value := range entries {
				changed[name] = value
			}
			changed[test.part] = test.value
			_, _, err := readXLSXStyles(xlsxStylePackage(t, changed))
			if err == nil || !strings.Contains(err.Error(), test.want) {
				t.Fatalf("error = %v, want %q", err, test.want)
			}
		})
	}
	entries["xl/theme/custom.xml"] = strings.ReplaceAll(entries["xl/theme/custom.xml"], ` lastClr="123456"`, "")
	entries["xl/theme/custom.xml"] = strings.ReplaceAll(entries["xl/theme/custom.xml"], ` lastClr="FFFFFF"`, "")
	fallbackStyles, fallbackFills, err := readXLSXStyles(xlsxStylePackage(t, entries))
	if err != nil || fallbackStyles[0].font.color != PDFColorBlack || *fallbackFills[0] != PDFColorWhite {
		t.Fatalf("system-color fallback = %#v, %#v, %v", fallbackStyles, fallbackFills, err)
	}
}
