package minipdf

import (
	"encoding/binary"
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strings"

	"golang.org/x/image/font"
	"golang.org/x/image/font/sfnt"
	"golang.org/x/image/math/fixed"
)

// A PDF FontFile2 contains a single sfnt, not the enclosing TTC. Rebuild its
// directory because offsets inside a collection are relative to the TTC file.
func docxCollectionFont(data []byte, index int) ([]byte, error) {
	if len(data) < 12 || string(data[:4]) != "ttcf" {
		if index == 0 {
			return data, nil
		}
		return nil, fmt.Errorf("font is not a collection")
	}
	count := int(binary.BigEndian.Uint32(data[8:12]))
	if index < 0 || index >= count || count > (len(data)-12)/4 {
		return nil, fmt.Errorf("invalid font collection directory")
	}
	offset := int(binary.BigEndian.Uint32(data[12+index*4 : 16+index*4]))
	if offset < 0 || offset > len(data)-12 {
		return nil, fmt.Errorf("invalid font collection offset")
	}
	n := int(binary.BigEndian.Uint16(data[offset+4 : offset+6]))
	if n > (len(data)-offset-12)/16 {
		return nil, fmt.Errorf("invalid sfnt directory")
	}
	out := append([]byte(nil), data[offset:offset+12+n*16]...)
	for i := 0; i < n; i++ {
		record := out[12+i*16 : 28+i*16]
		start := int(binary.BigEndian.Uint32(record[8:12]))
		length := int(binary.BigEndian.Uint32(record[12:16]))
		if start < 0 || length < 0 || start > len(data) || length > len(data)-start {
			return nil, fmt.Errorf("invalid sfnt table bounds")
		}
		binary.BigEndian.PutUint32(record[8:12], uint32(len(out)))
		out = append(out, data[start:start+length]...)
		for len(out)%4 != 0 {
			out = append(out, 0)
		}
	}
	// checkSumAdjustment is defined for the standalone font, not its collection.
	for i := 0; i < n; i++ {
		r := out[12+i*16 : 28+i*16]
		if string(r[:4]) == "head" {
			o := int(binary.BigEndian.Uint32(r[8:12]))
			if int(binary.BigEndian.Uint32(r[12:16])) < 12 {
				return nil, fmt.Errorf("invalid head table")
			}
			binary.BigEndian.PutUint32(out[o+8:o+12], 0)
			var sum uint32
			for j := 0; j < len(out); j += 4 {
				sum += binary.BigEndian.Uint32(out[j : j+4])
			}
			binary.BigEndian.PutUint32(out[o+8:o+12], 0xB1B0AFBA-sum)
		}
	}
	return out, nil
}

func loadDOCXFont(name string, bold, italic bool) (*embeddedFont, error) {
	variant := name
	if bold {
		variant += " Bold"
	}
	if italic {
		variant += " Italic"
	}
	var data []byte
	for _, registered := range RegisteredFonts() {
		if strings.EqualFold(registered.Name, variant) {
			data = registered.Data
			break
		}
	}
	if data == nil && (bold || italic) {
		for _, registered := range RegisteredFonts() {
			if strings.EqualFold(registered.Name, name) {
				data = registered.Data
				break
			}
		}
	}
	if data == nil && runtime.GOOS == "windows" {
		var file string
		switch strings.ToLower(name) {
		case "宋体", "simsun":
			file = "simsun.ttc"
		case "黑体", "simhei":
			file = "simhei.ttf"
		case "楷体", "kaiti":
			file = "simkai.ttf"
		case "仿宋", "fangsong":
			file = "simfang.ttf"
		case "times new roman":
			file = "times.ttf"
			if bold {
				file = "timesbd.ttf"
			}
			if italic {
				file = "timesi.ttf"
				if bold {
					file = "timesbi.ttf"
				}
			}
		default:
			f, err := readXLSXFont(name, bold, italic)
			if err == nil && f == nil && (bold || italic) {
				f, err = readXLSXFont(name, false, false)
			}
			if f != nil {
				f.nativeBold, f.nativeItalic = docxFontEmphasis(f.data)
			}
			return f, err
		}
		if root := os.Getenv("WINDIR"); root != "" {
			var err error
			data, err = os.ReadFile(filepath.Join(root, "Fonts", file))
			if os.IsNotExist(err) {
				return nil, nil
			}
			if err != nil {
				return nil, err
			}
		}
	}
	if data == nil {
		return nil, nil
	}
	data, err := docxCollectionFont(data, 0)
	if err != nil {
		return nil, fmt.Errorf("DOCX font %q: %w", name, err)
	}
	parsed, err := sfnt.Parse(data)
	if err != nil {
		return nil, fmt.Errorf("DOCX font %q: %w", name, err)
	}
	if postscript, err := parsed.Name(nil, sfnt.NameIDPostScript); err == nil && postscript != "" {
		variant = postscript
	}
	ppem := fixed.Int26_6(parsed.UnitsPerEm())
	metrics, err := parsed.Metrics(nil, ppem, font.HintingNone)
	if err != nil {
		return nil, err
	}
	result := &embeddedFont{name: sanitizePDFFontName(variant), data: data, font: parsed, runeToGlyph: make(map[rune]sfnt.GlyphIndex), glyphToRune: make(map[sfnt.GlyphIndex]rune), advances: make([]float64, parsed.NumGlyphs()), ascent: float64(metrics.Ascent) / float64(ppem), descent: float64(metrics.Descent) / float64(ppem)}
	result.nativeBold, result.nativeItalic = docxFontEmphasis(data)
	for i := range result.advances {
		a, err := parsed.GlyphAdvance(nil, sfnt.GlyphIndex(i), ppem, font.HintingNone)
		if err != nil {
			return nil, err
		}
		result.advances[i] = float64(a) / float64(ppem)
	}
	return result, nil
}

func docxFontEmphasis(data []byte) (bold, italic bool) {
	if len(data) < 12 {
		return
	}
	n := int(binary.BigEndian.Uint16(data[4:6]))
	if n > (len(data)-12)/16 {
		return
	}
	for i := 0; i < n; i++ {
		r := data[12+i*16 : 28+i*16]
		o, length := int(binary.BigEndian.Uint32(r[8:12])), int(binary.BigEndian.Uint32(r[12:16]))
		if o < 0 || length < 0 || o > len(data)-length {
			continue
		}
		if string(r[:4]) == "OS/2" && length >= 64 {
			flags := binary.BigEndian.Uint16(data[o+62 : o+64])
			return flags&(1<<5) != 0, flags&1 != 0
		}
		if string(r[:4]) == "head" && length >= 46 {
			flags := binary.BigEndian.Uint16(data[o+44 : o+46])
			bold, italic = flags&1 != 0, flags&2 != 0
		}
	}
	return
}

func docxFontCJK(data []byte) bool {
	if len(data) < 12 {
		return false
	}
	n := int(binary.BigEndian.Uint16(data[4:6]))
	if n > (len(data)-12)/16 {
		return false
	}
	for i := 0; i < n; i++ {
		r := data[12+i*16 : 28+i*16]
		if string(r[:4]) != "OS/2" {
			continue
		}
		o := int(binary.BigEndian.Uint32(r[8:12]))
		length := int(binary.BigEndian.Uint32(r[12:16]))
		if length < 86 || o < 0 || o > len(data)-length {
			return false
		}
		return binary.BigEndian.Uint32(data[o+78:o+82])&(0xf<<17) != 0
	}
	return false
}
