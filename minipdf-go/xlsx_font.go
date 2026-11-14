package minipdf

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strings"

	"golang.org/x/image/font"
	"golang.org/x/image/font/sfnt"
	"golang.org/x/image/math/fixed"
)

func readXLSXFont(name string, bold, italic bool) (*embeddedFont, error) {
	variant := name
	if bold {
		variant += " Bold"
	}
	if italic {
		variant += " Italic"
	}
	normalize := func(value string) string {
		return strings.ToLower(strings.NewReplacer(" ", "", "-", "").Replace(value))
	}
	newFont := func(data []byte) (*embeddedFont, error) {
		parsed, err := sfnt.Parse(data)
		if err != nil {
			return nil, fmt.Errorf("parse XLSX font %q: %w", variant, err)
		}
		if parsed.UnitsPerEm() == 0 {
			return nil, fmt.Errorf("invalid XLSX font %q units-per-em", variant)
		}
		advances := make([]float64, parsed.NumGlyphs())
		metrics, err := parsed.Metrics(nil, fixed.Int26_6(parsed.UnitsPerEm()), font.HintingNone)
		if err != nil {
			return nil, fmt.Errorf("read XLSX font %q metrics: %w", variant, err)
		}
		for index := range advances {
			advance, err := parsed.GlyphAdvance(nil, sfnt.GlyphIndex(index), fixed.Int26_6(parsed.UnitsPerEm()), font.HintingNone)
			if err != nil {
				return nil, fmt.Errorf("read XLSX font %q glyph metrics: %w", variant, err)
			}
			advances[index] = float64(advance) / float64(parsed.UnitsPerEm())
		}
		return &embeddedFont{name: sanitizePDFFontName(variant), data: data, font: parsed, advances: advances,
			ascent: float64(metrics.Ascent) / float64(parsed.UnitsPerEm()), descent: float64(metrics.Descent) / float64(parsed.UnitsPerEm()),
			runeToGlyph: make(map[rune]sfnt.GlyphIndex), glyphToRune: make(map[sfnt.GlyphIndex]rune)}, nil
	}
	for _, registered := range RegisteredFonts() {
		if normalize(registered.Name) == normalize(variant) {
			return newFont(registered.Data)
		}
	}
	if runtime.GOOS != "windows" {
		return nil, nil
	}
	index := 0
	if bold {
		index++
	}
	if italic {
		index += 2
	}
	var files []string
	switch strings.ToLower(name) {
	case "calibri":
		files = []string{"calibri.ttf", "calibrib.ttf", "calibrii.ttf", "calibriz.ttf"}
	case "arial":
		files = []string{"arial.ttf", "arialbd.ttf", "ariali.ttf", "arialbi.ttf"}
	default:
		return nil, nil
	}
	windows := os.Getenv("WINDIR")
	if windows == "" {
		return nil, nil
	}
	path := filepath.Join(windows, "Fonts", files[index])
	data, err := os.ReadFile(path)
	if os.IsNotExist(err) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("read XLSX font %s: %w", path, err)
	}
	return newFont(data)
}

func measureXLSXFont(text string, size float64, embedded *embeddedFont) float64 {
	embedded.collect(text)
	width := 0.0
	for _, character := range normalizedFontRunes(text) {
		width += embedded.advances[embedded.runeToGlyph[character]] * size
	}
	return width
}
