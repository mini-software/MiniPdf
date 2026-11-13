package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"io"
	"math"
	"strings"
)

type xlsxColorXML struct {
	RGB   string  `xml:"rgb,attr"`
	Theme *int    `xml:"theme,attr"`
	Tint  float64 `xml:"tint,attr"`
}

type xlsxFillXML struct {
	Pattern struct {
		Type       string       `xml:"patternType,attr"`
		Foreground xlsxColorXML `xml:"fgColor"`
		Background xlsxColorXML `xml:"bgColor"`
	} `xml:"patternFill"`
}

func readXLSXTheme(files officePackage) (map[int]PDFColor, error) {
	names := []string{"lt1", "dk1", "lt2", "dk2", "accent1", "accent2", "accent3", "accent4", "accent5", "accent6", "hlink", "folHlink"}
	defaults := []string{"FFFFFF", "000000", "EEECE1", "1F497D", "4F81BD", "C0504D", "9BBB59", "8064A2", "4BACC6", "F79646", "0000FF", "800080"}
	themeName := "xl/theme/theme1.xml"
	const relationshipsName = "xl/_rels/workbook.xml.rels"
	hasThemeRelationship := false
	if _, ok := files[relationshipsName]; ok {
		data, err := files.read(relationshipsName)
		if err != nil {
			return nil, err
		}
		var relationships xlsxRelationshipsDocument
		if err := xml.Unmarshal(data, &relationships); err != nil {
			return nil, fmt.Errorf("parse %s: %w", relationshipsName, err)
		}
		for _, relationship := range relationships.Items {
			if strings.HasSuffix(relationship.Type, "/theme") {
				if relationship.TargetMode == "External" || strings.TrimSpace(relationship.Target) == "" {
					return nil, fmt.Errorf("invalid XLSX theme relationship target %q", relationship.Target)
				}
				themeName = xlsxResolveRelationshipTarget("xl/workbook.xml", relationship.Target)
				hasThemeRelationship = true
				break
			}
		}
	}
	actualName := ""
	for name := range files {
		if strings.EqualFold(name, themeName) {
			actualName = name
			break
		}
	}
	if actualName == "" {
		if hasThemeRelationship {
			return nil, fmt.Errorf("Office package theme part %q is missing", themeName)
		}
		colors := make(map[int]PDFColor, len(defaults))
		for index, value := range defaults {
			colors[index], _ = parseXLSXColor(value)
		}
		return colors, nil
	}
	data, err := files.read(actualName)
	if err != nil {
		return nil, err
	}
	colors := make(map[int]PDFColor)
	decoder := xml.NewDecoder(bytes.NewReader(data))
	inScheme := false
	for {
		token, err := decoder.Token()
		if err == io.EOF {
			break
		}
		if err != nil {
			return nil, fmt.Errorf("parse %s: %w", actualName, err)
		}
		if end, ok := token.(xml.EndElement); ok && end.Name.Local == "clrScheme" {
			inScheme = false
		}
		start, ok := token.(xml.StartElement)
		if !ok {
			continue
		}
		if start.Name.Local == "clrScheme" {
			inScheme = true
			continue
		}
		if !inScheme {
			continue
		}
		for index, name := range names {
			if start.Name.Local != name {
				continue
			}
			var entry struct {
				RGB *struct {
					Value string `xml:"val,attr"`
				} `xml:"srgbClr"`
				System *struct {
					Value string `xml:"val,attr"`
					Last  string `xml:"lastClr,attr"`
				} `xml:"sysClr"`
			}
			if err := decoder.DecodeElement(&entry, &start); err != nil {
				return nil, fmt.Errorf("parse %s: %w", actualName, err)
			}
			value := ""
			switch {
			case entry.RGB != nil:
				value = entry.RGB.Value
			case entry.System != nil:
				value = entry.System.Last
				if value == "" {
					switch entry.System.Value {
					case "window":
						value = "FFFFFF"
					case "windowText":
						value = "000000"
					}
				}
			}
			color, ok := parseXLSXColor(value)
			if !ok {
				return nil, fmt.Errorf("parse %s: unsupported or invalid theme color %s", actualName, name)
			}
			colors[index] = color
			break
		}
	}
	return colors, nil
}

func resolveXLSXColor(source xlsxColorXML, theme map[int]PDFColor) (*PDFColor, error) {
	if math.IsNaN(source.Tint) || math.IsInf(source.Tint, 0) || source.Tint < -1 || source.Tint > 1 {
		return nil, fmt.Errorf("invalid XLSX color tint %v", source.Tint)
	}
	var color PDFColor
	switch {
	case source.Theme != nil:
		var ok bool
		color, ok = theme[*source.Theme]
		if !ok {
			return nil, fmt.Errorf("unknown XLSX theme color index %d", *source.Theme)
		}
	case source.RGB != "":
		var ok bool
		color, ok = parseXLSXColor(source.RGB)
		if !ok {
			return nil, fmt.Errorf("invalid XLSX RGB color %q", source.RGB)
		}
	default:
		return nil, nil
	}
	if source.Tint != 0 {
		color = tintXLSXColor(color, source.Tint)
	}
	return &color, nil
}

func tintXLSXColor(color PDFColor, tint float64) PDFColor {
	minimum := min(color.Red, color.Green, color.Blue)
	maximum := max(color.Red, color.Green, color.Blue)
	luminance := (minimum + maximum) / 2
	chroma := maximum - minimum
	saturation, hue := 0.0, 0.0
	if chroma > 0 {
		saturation = chroma / (1 - math.Abs(2*luminance-1))
		switch maximum {
		case color.Red:
			hue = math.Mod((color.Green-color.Blue)/chroma+6, 6)
		case color.Green:
			hue = (color.Blue-color.Red)/chroma + 2
		case color.Blue:
			hue = (color.Red-color.Green)/chroma + 4
		}
	}
	// Excel tint changes HSL luminance, not the individual RGB channels.
	if tint < 0 {
		luminance *= 1 + tint
	} else {
		luminance = luminance*(1-tint) + tint
	}
	chroma = (1 - math.Abs(2*luminance-1)) * saturation
	second := chroma * (1 - math.Abs(math.Mod(hue, 2)-1))
	offset := luminance - chroma/2
	red, green, blue := 0.0, 0.0, 0.0
	switch {
	case hue < 1:
		red, green = chroma, second
	case hue < 2:
		red, green = second, chroma
	case hue < 3:
		green, blue = chroma, second
	case hue < 4:
		green, blue = second, chroma
	case hue < 5:
		red, blue = second, chroma
	default:
		red, blue = chroma, second
	}
	return PDFColor{
		Red: math.Round((red+offset)*255) / 255, Green: math.Round((green+offset)*255) / 255,
		Blue: math.Round((blue+offset)*255) / 255,
	}
}
