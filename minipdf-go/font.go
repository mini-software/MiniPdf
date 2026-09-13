package minipdf

import (
	"bytes"
	"fmt"
	"sort"
	"strings"
	"unicode/utf16"

	"golang.org/x/image/font"
	"golang.org/x/image/font/sfnt"
	"golang.org/x/image/math/fixed"
)

type embeddedFont struct {
	name        string
	data        []byte
	font        *sfnt.Font
	runeToGlyph map[rune]sfnt.GlyphIndex
	glyphToRune map[sfnt.GlyphIndex]rune
	objectID    int
}

func prepareEmbeddedFont(pages []*PDFPage) *embeddedFont {
	for _, registered := range RegisteredFonts() {
		parsed, err := sfnt.Parse(registered.Data)
		if err != nil {
			continue
		}
		candidate := &embeddedFont{
			name:        sanitizePDFFontName(registered.Name),
			data:        registered.Data,
			font:        parsed,
			runeToGlyph: make(map[rune]sfnt.GlyphIndex),
			glyphToRune: make(map[sfnt.GlyphIndex]rune),
		}
		used := false
		for _, page := range pages {
			for _, operation := range page.operations {
				text, ok := operation.(textOperation)
				if !ok || !candidate.canEncode(text.text) {
					continue
				}
				candidate.collect(text.text)
				used = true
			}
		}
		if used {
			return candidate
		}
	}
	return nil
}

func (embedded *embeddedFont) canEncode(text string) bool {
	var buffer sfnt.Buffer
	for _, character := range normalizedFontRunes(text) {
		glyph, err := embedded.font.GlyphIndex(&buffer, character)
		if err != nil || glyph == 0 {
			return false
		}
	}
	return true
}

func (embedded *embeddedFont) collect(text string) {
	var buffer sfnt.Buffer
	for _, character := range normalizedFontRunes(text) {
		glyph, err := embedded.font.GlyphIndex(&buffer, character)
		if err != nil || glyph == 0 {
			continue
		}
		embedded.runeToGlyph[character] = glyph
		if _, exists := embedded.glyphToRune[glyph]; !exists {
			embedded.glyphToRune[glyph] = character
		}
	}
}

func (embedded *embeddedFont) encode(text string) (string, bool) {
	var encoded strings.Builder
	for _, character := range normalizedFontRunes(text) {
		glyph, ok := embedded.runeToGlyph[character]
		if !ok {
			return "", false
		}
		fmt.Fprintf(&encoded, "%04X", uint16(glyph))
	}
	return encoded.String(), true
}

func normalizedFontRunes(text string) []rune {
	characters := []rune(text)
	for index, character := range characters {
		if character == '\n' || character == '\r' || character == '\t' {
			characters[index] = ' '
		}
	}
	return characters
}

func appendEmbeddedFontObjects(objects *[][]byte, embedded *embeddedFont) int {
	fontFile := fmt.Appendf(nil, "<< /Length %d /Length1 %d >>\nstream\n", len(embedded.data), len(embedded.data))
	fontFile = append(fontFile, embedded.data...)
	fontFile = append(fontFile, []byte("\nendstream")...)
	fontFileID := appendPDFObject(objects, fontFile)

	unitsPerEm := int64(embedded.font.UnitsPerEm())
	fontBounds := "-1000 -1000 2000 2000"
	ascent := int64(800)
	descent := int64(-200)
	capHeight := int64(700)
	if unitsPerEm > 0 {
		ppem := fixed.Int26_6(embedded.font.UnitsPerEm())
		if bounds, err := embedded.font.Bounds(nil, ppem, font.HintingNone); err == nil {
			fontBounds = fmt.Sprintf("%d %d %d %d",
				int64(bounds.Min.X)*1000/unitsPerEm,
				-int64(bounds.Max.Y)*1000/unitsPerEm,
				int64(bounds.Max.X)*1000/unitsPerEm,
				-int64(bounds.Min.Y)*1000/unitsPerEm,
			)
		}
		if metrics, err := embedded.font.Metrics(nil, ppem, font.HintingNone); err == nil {
			ascent = int64(metrics.Ascent) * 1000 / unitsPerEm
			descent = -int64(metrics.Descent) * 1000 / unitsPerEm
			capHeight = int64(metrics.CapHeight) * 1000 / unitsPerEm
		}
	}

	descriptorID := appendPDFObject(objects, []byte(fmt.Sprintf(
		"<< /Type /FontDescriptor /FontName /%s /Flags 32 /FontBBox [%s] /ItalicAngle 0 /Ascent %d /Descent %d /CapHeight %d /StemV 80 /FontFile2 %d 0 R >>",
		embedded.name, fontBounds, ascent, descent, capHeight, fontFileID,
	)))

	glyphs := make([]int, 0, len(embedded.glyphToRune))
	for glyph := range embedded.glyphToRune {
		glyphs = append(glyphs, int(glyph))
	}
	sort.Ints(glyphs)
	widths := make([]string, 0, len(glyphs))
	for _, glyph := range glyphs {
		width := int64(1000)
		if unitsPerEm > 0 {
			advance, err := embedded.font.GlyphAdvance(nil, sfnt.GlyphIndex(glyph), fixed.Int26_6(embedded.font.UnitsPerEm()), font.HintingNone)
			if err == nil {
				width = int64(advance) * 1000 / unitsPerEm
			}
		}
		widths = append(widths, fmt.Sprintf("%d [%d]", glyph, width))
	}
	cidFontID := appendPDFObject(objects, []byte(fmt.Sprintf(
		"<< /Type /Font /Subtype /CIDFontType2 /BaseFont /%s /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor %d 0 R /DW 1000 /W [%s] /CIDToGIDMap /Identity >>",
		embedded.name, descriptorID, strings.Join(widths, " "),
	)))

	toUnicode := buildToUnicodeCMap(embedded.glyphToRune)
	toUnicodeObject := fmt.Appendf(nil, "<< /Length %d >>\nstream\n", len(toUnicode))
	toUnicodeObject = append(toUnicodeObject, toUnicode...)
	toUnicodeObject = append(toUnicodeObject, []byte("\nendstream")...)
	toUnicodeID := appendPDFObject(objects, toUnicodeObject)

	return appendPDFObject(objects, []byte(fmt.Sprintf(
		"<< /Type /Font /Subtype /Type0 /BaseFont /%s /Encoding /Identity-H /DescendantFonts [%d 0 R] /ToUnicode %d 0 R >>",
		embedded.name, cidFontID, toUnicodeID,
	)))
}

func buildToUnicodeCMap(glyphToRune map[sfnt.GlyphIndex]rune) []byte {
	glyphs := make([]int, 0, len(glyphToRune))
	for glyph := range glyphToRune {
		glyphs = append(glyphs, int(glyph))
	}
	sort.Ints(glyphs)

	var cmap bytes.Buffer
	cmap.WriteString("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\n<0000> <FFFF>\nendcodespacerange\n")
	for start := 0; start < len(glyphs); start += 100 {
		end := min(start+100, len(glyphs))
		fmt.Fprintf(&cmap, "%d beginbfchar\n", end-start)
		for _, glyph := range glyphs[start:end] {
			fmt.Fprintf(&cmap, "<%04X> <%s>\n", glyph, utf16Hex(glyphToRune[sfnt.GlyphIndex(glyph)]))
		}
		cmap.WriteString("endbfchar\n")
	}
	cmap.WriteString("endcmap\nCMapName currentdict /CMap defineresource pop\nend\nend")
	return cmap.Bytes()
}

func utf16Hex(character rune) string {
	var encoded strings.Builder
	for _, unit := range utf16.Encode([]rune{character}) {
		fmt.Fprintf(&encoded, "%04X", unit)
	}
	return encoded.String()
}

func sanitizePDFFontName(name string) string {
	var sanitized strings.Builder
	for _, character := range name {
		if character >= 'A' && character <= 'Z' || character >= 'a' && character <= 'z' || character >= '0' && character <= '9' {
			sanitized.WriteRune(character)
		}
	}
	if sanitized.Len() == 0 {
		return "MiniPdfFont"
	}
	return sanitized.String()
}

func appendPDFObject(objects *[][]byte, object []byte) int {
	*objects = append(*objects, object)
	return len(*objects)
}
