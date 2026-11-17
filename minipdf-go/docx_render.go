package minipdf

import (
	"bytes"
	"encoding/xml"
	"errors"
	"fmt"
	"image"
	_ "image/jpeg"
	_ "image/png"
	"io"
	"math"
	"path"
	"strconv"
	"strings"
	"unicode"
)

var errDOCXFontUnavailable = errors.New("DOCX font unavailable")
var errDOCXTableLayoutUnsupported = errors.New("DOCX table layout unsupported")

type docxNode struct {
	name     string
	attrs    map[string]string
	children []*docxNode
	text     string
}

func parseDOCXNode(data []byte) (*docxNode, error) {
	root := &docxNode{}
	stack := []*docxNode{root}
	dec := xml.NewDecoder(bytes.NewReader(data))
	for {
		token, err := dec.Token()
		if err == io.EOF {
			break
		}
		if err != nil {
			return nil, err
		}
		switch t := token.(type) {
		case xml.StartElement:
			n := &docxNode{name: t.Name.Local, attrs: map[string]string{}}
			for _, a := range t.Attr {
				n.attrs[a.Name.Local] = a.Value
			}
			p := stack[len(stack)-1]
			p.children = append(p.children, n)
			stack = append(stack, n)
		case xml.EndElement:
			stack = stack[:len(stack)-1]
		case xml.CharData:
			stack[len(stack)-1].text += string(t)
		}
	}
	if len(root.children) == 0 {
		return nil, fmt.Errorf("empty DOCX XML")
	}
	return root.children[0], nil
}
func (n *docxNode) child(name string) *docxNode {
	if n != nil {
		for _, c := range n.children {
			if c.name == name {
				return c
			}
		}
	}
	return nil
}
func (n *docxNode) attr(key string) string {
	if n == nil {
		return ""
	}
	return n.attrs[key]
}
func (n *docxNode) value(name string) string { return n.child(name).attr("val") }
func (n *docxNode) descendants(name string) []*docxNode {
	var out []*docxNode
	if n != nil {
		for _, c := range n.children {
			if c.name == name {
				out = append(out, c)
			}
			out = append(out, c.descendants(name)...)
		}
	}
	return out
}
func docxNumber(s string) float64 {
	v, _ := strconv.ParseFloat(s, 64)
	if math.IsNaN(v) || math.IsInf(v, 0) {
		return 0
	}
	return v
}
func docxOn(n *docxNode) bool {
	return n != nil && n.attr("val") != "0" && n.attr("val") != "false" && n.attr("val") != "off"
}

type docxRunStyle struct {
	size         float64
	fonts        map[string]string
	bold, italic bool
	color        PDFColor
	highlight    *PDFColor
	lang         string
}
type docxParagraphStyle struct {
	run                 docxRunStyle
	before, after, line float64
	lineRule, align     string
	left, right, first  float64
	snap                bool
}
type docxStyles struct {
	nodes      map[string]*docxNode
	defaults   docxParagraphStyle
	theme      map[string]string
	fonts      map[string]*embeddedFont
	cjkMetrics bool
}

func (s *docxStyles) applyRun(style docxRunStyle, n *docxNode) docxRunStyle {
	f := map[string]string{}
	for k, v := range style.fonts {
		f[k] = v
	}
	style.fonts = f
	if n == nil {
		return style
	}
	if sz := n.child("sz"); sz != nil && docxNumber(sz.attr("val")) > 0 {
		style.size = docxNumber(sz.attr("val")) / 2
	}
	if fonts := n.child("rFonts"); fonts != nil {
		for _, key := range []string{"ascii", "hAnsi", "eastAsia", "cs"} {
			if v := fonts.attr(key); v != "" {
				style.fonts[key] = v
				delete(style.fonts, key+"Theme")
			}
			if v := fonts.attr(key + "Theme"); v != "" {
				style.fonts[key+"Theme"] = v
				delete(style.fonts, key)
			}
		}
	}
	if n.child("b") != nil {
		style.bold = docxOn(n.child("b"))
	}
	if n.child("i") != nil {
		style.italic = docxOn(n.child("i"))
	}
	if lang := n.child("lang").attr("eastAsia"); lang != "" {
		style.lang = lang
	}
	if c := n.child("color"); c != nil {
		style.color = docxColor(c.attr("val"))
	}
	if h := n.child("highlight"); h != nil {
		style.highlight = nil
		colors := map[string]string{"yellow": "FFFF00", "green": "00FF00", "cyan": "00FFFF", "magenta": "FF00FF", "blue": "0000FF", "red": "FF0000", "darkYellow": "808000"}
		if v, ok := colors[h.attr("val")]; ok {
			c := docxColor(v)
			style.highlight = &c
		}
	}
	return style
}
func (s *docxStyles) applyParagraph(style docxParagraphStyle, n *docxNode) docxParagraphStyle {
	if n == nil {
		return style
	}
	style.run = s.applyRun(style.run, n.child("rPr"))
	if p := n.child("spacing"); p != nil {
		for _, item := range []struct {
			key string
			out *float64
		}{{"before", &style.before}, {"after", &style.after}, {"line", &style.line}} {
			if v, ok := p.attrs[item.key]; ok {
				*item.out = docxNumber(v) / 20
			}
		}
		if p.attr("lineRule") != "" {
			style.lineRule = p.attr("lineRule")
		}
	}
	if p := n.child("jc"); p != nil {
		style.align = p.attr("val")
	}
	if p := n.child("ind"); p != nil {
		for _, item := range []struct {
			key string
			out *float64
		}{{"left", &style.left}, {"right", &style.right}, {"firstLine", &style.first}} {
			if v, ok := p.attrs[item.key]; ok {
				*item.out = docxNumber(v) / 20
			}
		}
		if v, ok := p.attrs["hanging"]; ok {
			style.first = -docxNumber(v) / 20
		}
	}
	if p := n.child("snapToGrid"); p != nil {
		style.snap = docxOn(p)
	}
	return style
}
func (s *docxStyles) inherit(style docxParagraphStyle, id string, seen map[string]bool) docxParagraphStyle {
	if seen[id] {
		return style
	}
	seen[id] = true
	n := s.nodes[id]
	if n == nil {
		return style
	}
	style = s.inherit(style, n.value("basedOn"), seen)
	style = s.applyParagraph(style, n.child("pPr"))
	style.run = s.applyRun(style.run, n.child("rPr"))
	return style
}
func (s *docxStyles) paragraph(n *docxNode) docxParagraphStyle {
	style := s.defaults
	pr := n.child("pPr")
	style = s.inherit(style, pr.value("pStyle"), map[string]bool{})
	return s.applyParagraph(style, pr)
}
func docxColor(value string) PDFColor {
	v, err := strconv.ParseUint(value, 16, 24)
	if err != nil {
		return PDFColorBlack
	}
	return PDFColor{Red: float64(v>>16) / 255, Green: float64((v>>8)&255) / 255, Blue: float64(v&255) / 255}
}

func readDOCXStyles(files officePackage) (*docxStyles, error) {
	s := &docxStyles{nodes: map[string]*docxNode{}, theme: map[string]string{}, fonts: map[string]*embeddedFont{}, defaults: docxParagraphStyle{run: docxRunStyle{size: 12, fonts: map[string]string{"ascii": "Arial", "eastAsia": "SimSun"}}, snap: true}}
	if _, ok := files["word/settings.xml"]; ok {
		data, err := files.read("word/settings.xml")
		if err != nil {
			return nil, err
		}
		root, err := parseDOCXNode(data)
		if err != nil {
			return nil, err
		}
		s.cjkMetrics = docxOn(root.child("compat").child("useFELayout"))
	}
	if _, ok := files["word/theme/theme1.xml"]; ok {
		data, err := files.read("word/theme/theme1.xml")
		if err != nil {
			return nil, err
		}
		root, err := parseDOCXNode(data)
		if err != nil {
			return nil, err
		}
		for _, group := range []string{"major", "minor"} {
			for _, n := range root.descendants(group + "Font") {
				s.theme[group+"HAnsi"] = n.child("latin").attr("typeface")
				s.theme[group+"Ascii"] = s.theme[group+"HAnsi"]
				s.theme[group+"EastAsia"] = n.child("ea").attr("typeface")
				for _, f := range n.children {
					if f.name == "font" {
						s.theme[group+":"+f.attr("script")] = f.attr("typeface")
					}
				}
			}
		}
	}
	if _, ok := files["word/styles.xml"]; ok {
		data, err := files.read("word/styles.xml")
		if err != nil {
			return nil, err
		}
		root, err := parseDOCXNode(data)
		if err != nil {
			return nil, err
		}
		d := root.child("docDefaults")
		s.defaults.run = s.applyRun(s.defaults.run, d.child("rPrDefault").child("rPr"))
		s.defaults = s.applyParagraph(s.defaults, d.child("pPrDefault").child("pPr"))
		normal := ""
		for _, n := range root.children {
			if n.name == "style" {
				s.nodes[n.attr("styleId")] = n
				if n.attr("type") == "paragraph" && n.attr("default") == "1" {
					normal = n.attr("styleId")
				}
			}
		}
		s.defaults = s.inherit(s.defaults, normal, map[string]bool{})
	}
	return s, nil
}
func (s *docxStyles) font(style docxRunStyle, r rune) (*embeddedFont, error) {
	key := "ascii"
	if r > 127 {
		key = "hAnsi"
	}
	if docxAsian(r) {
		key = "eastAsia"
	}
	name := style.fonts[key]
	if theme := style.fonts[key+"Theme"]; theme != "" {
		name = s.theme[theme]
		if strings.HasSuffix(theme, "EastAsia") && name == "" {
			script := "Hans"
			if strings.HasPrefix(style.lang, "ja") {
				script = "Jpan"
			}
			if strings.HasPrefix(style.lang, "ko") {
				script = "Hang"
			}
			if style.lang == "zh-TW" || style.lang == "zh-HK" {
				script = "Hant"
			}
			name = s.theme[strings.TrimSuffix(theme, "EastAsia")+":"+script]
		}
	}
	if name == "" {
		name = "Arial"
		if docxAsian(r) {
			name = "SimSun"
		}
	}
	cache := fmt.Sprintf("%s/%t/%t", name, style.bold, style.italic)
	f, ok := s.fonts[cache]
	if !ok {
		var err error
		f, err = loadDOCXFont(name, style.bold, style.italic)
		if err != nil {
			return nil, err
		}
		s.fonts[cache] = f
	}
	if r > 127 && (f == nil || !f.canEncode(string(r))) {
		fallback := "Arial"
		if docxAsian(r) {
			fallback = "SimSun"
		}
		fallbackKey := fmt.Sprintf("%s/%t/%t", fallback, style.bold, style.italic)
		if fallbackKey != cache {
			fallbackFont, ok := s.fonts[fallbackKey]
			if !ok {
				var err error
				fallbackFont, err = loadDOCXFont(fallback, style.bold, style.italic)
				if err != nil {
					return nil, err
				}
				s.fonts[fallbackKey] = fallbackFont
			}
			if fallbackFont != nil && fallbackFont.canEncode(string(r)) {
				return fallbackFont, nil
			}
		}
	}
	return f, nil
}
func docxAsian(r rune) bool {
	return unicode.Is(unicode.Han, r) || unicode.Is(unicode.Hangul, r) || unicode.Is(unicode.Hiragana, r) || unicode.Is(unicode.Katakana, r) || r >= 0x3000 && r <= 0x303f || r >= 0xff00 && r <= 0xffef || r == 0x25a1
}

type docxGlyph struct {
	r                      rune
	style                  docxRunStyle
	font                   *embeddedFont
	width, ascent, descent float64
}
type docxLine struct {
	glyphs                []docxGlyph
	width, height, ascent float64
}
type docxParagraph struct {
	style  docxParagraphStyle
	lines  []docxLine
	height float64
}
type docxImage struct {
	op        imageOperation
	floating  bool
	left, top float64
}

func (s *docxStyles) layoutParagraph(n *docxNode, width, grid float64) (docxParagraph, error) {
	p := docxParagraph{style: s.paragraph(n)}
	var glyphs []docxGlyph
	var visit func(*docxNode, docxRunStyle) error
	visit = func(n *docxNode, style docxRunStyle) error {
		if n.name == "pPr" || n.name == "rPr" || n.name == "pict" || n.name == "drawing" {
			return nil
		}
		if n.name == "r" {
			if id := n.child("rPr").value("rStyle"); id != "" {
				inherited := s.inherit(docxParagraphStyle{run: style}, id, map[string]bool{})
				style = inherited.run
			}
			style = s.applyRun(style, n.child("rPr"))
		}
		text := ""
		if n.name == "t" {
			text = n.text
		}
		if n.name == "tab" {
			text = "    "
		}
		if n.name == "br" || n.name == "cr" {
			text = "\n"
		}
		for _, r := range text {
			if r == '\n' {
				glyphs = append(glyphs, docxGlyph{r: r})
				continue
			}
			f, err := s.font(style, r)
			if err != nil {
				return err
			}
			g := docxGlyph{r: r, style: style, font: f, width: style.size * .5, ascent: style.size * .9, descent: style.size * .25}
			if docxAsian(r) {
				g.width = style.size
			}
			if f != nil {
				if !f.canEncode(string(r)) {
					return fmt.Errorf("DOCX font %q has no glyph for U+%04X", f.name, r)
				}
				g.width = measureXLSXFont(string(r), style.size, f)
				g.ascent = f.ascent * style.size
				g.descent = f.descent * style.size
				// Writer fntcache.cxx, lcl_ApplyCjkHeightAdjustment: Word's
				// FE layout scales metrics for fonts advertising CJK code pages.
				if s.cjkMetrics && grid == 0 && docxFontCJK(f.data) {
					g.ascent *= 1.27
					g.descent *= 1.27
				}
			} else if r > 127 {
				return fmt.Errorf("DOCX character U+%04X requires an installed or registered font: %w", r, errDOCXFontUnavailable)
			}
			glyphs = append(glyphs, g)
		}
		for _, c := range n.children {
			if err := visit(c, style); err != nil {
				return err
			}
		}
		return nil
	}
	if err := visit(n, p.style.run); err != nil {
		return p, err
	}
	emptyFont, err := s.font(p.style.run, ' ')
	if err != nil {
		return p, err
	}
	// Word's automatic East Asian/Latin spacing is a quarter-em at script boundaries.
	for i := 1; i < len(glyphs); i++ {
		a, b := glyphs[i-1].r, glyphs[i].r
		if (unicode.Is(unicode.Han, a) && docxLatin(b)) || (docxLatin(a) && unicode.Is(unicode.Han, b)) {
			glyphs[i-1].width += math.Min(glyphs[i-1].style.size, glyphs[i].style.size) / 4
		}
	}
	available := math.Max(1, width-p.style.left-p.style.right)
	var line docxLine
	flush := func() {
		if len(line.glyphs) == 0 {
			f := emptyFont
			line.ascent = p.style.run.size * .9
			line.height = p.style.run.size * 1.3
			if f != nil {
				line.ascent = f.ascent * p.style.run.size
				line.height = (f.ascent + f.descent) * p.style.run.size
				if s.cjkMetrics && grid == 0 && docxFontCJK(f.data) {
					line.ascent *= 1.27
					line.height *= 1.27
				}
			}
		}
		line.height = math.Max(line.height, line.ascent)
		if grid > 0 && p.style.snap {
			h := math.Ceil((line.height-.001)/grid) * grid
			line.ascent += (h - line.height) / 2
			line.height = h
		}
		if p.style.line > 0 {
			if p.style.lineRule == "exact" {
				line.height = p.style.line
				line.ascent = math.Min(line.ascent, line.height*.8)
			} else if p.style.lineRule == "atLeast" {
				line.height = math.Max(line.height, p.style.line)
			} else {
				h := line.height * p.style.line / 12
				if grid > 0 && p.style.snap {
					h = math.Ceil((h-.001)/grid) * grid
				}
				line.ascent += (h - line.height) / 2
				line.height = h
			}
		}
		p.height += line.height
		p.lines = append(p.lines, line)
		line = docxLine{}
	}
	for i := 0; i < len(glyphs); i++ {
		g := glyphs[i]
		if g.r == '\n' {
			flush()
			continue
		}
		limit := available
		if len(p.lines) == 0 {
			limit -= p.style.first
		}
		if line.width+g.width > limit+.01 && len(line.glyphs) > 0 && !(len(line.glyphs) == 1 && (strings.ContainsRune("（([【《「『", line.glyphs[0].r) || strings.ContainsRune("），。！？、；：)]】》」』", g.r))) {
			var carry []docxGlyph
			last := line.glyphs[len(line.glyphs)-1]
			if strings.ContainsRune("（([【《「『", last.r) || strings.ContainsRune("），。！？、；：)]】》」』", g.r) {
				carry = []docxGlyph{last}
				line.glyphs = line.glyphs[:len(line.glyphs)-1]
				line.width -= last.width
			} else if docxLatin(g.r) && docxLatin(last.r) {
				for j := len(line.glyphs) - 1; j >= 0; j-- {
					if unicode.IsSpace(line.glyphs[j].r) {
						carry = append(carry, line.glyphs[j+1:]...)
						line.glyphs = line.glyphs[:j+1]
						line.width = 0
						for _, c := range line.glyphs {
							line.width += c.width
						}
						break
					}
				}
			}
			flush()
			for _, c := range carry {
				line.glyphs = append(line.glyphs, c)
				line.width += c.width
				line.ascent = math.Max(line.ascent, c.ascent)
				line.height = math.Max(line.height, c.ascent+c.descent)
			}
		}
		line.glyphs = append(line.glyphs, g)
		line.width += g.width
		line.ascent = math.Max(line.ascent, g.ascent)
		line.height = math.Max(line.height, g.ascent+g.descent)
	}
	flush()
	p.height += p.style.before + p.style.after
	return p, nil
}
func docxLatin(r rune) bool { return r < 128 && (unicode.IsLetter(r) || unicode.IsDigit(r)) }

func drawDOCXParagraph(page *PDFPage, p docxParagraph, x, top, width float64) {
	top += p.style.before
	for i, line := range p.lines {
		left := x + p.style.left
		if i == 0 {
			left += p.style.first
		}
		switch p.style.align {
		case "center":
			left += (width - p.style.left - p.style.right - line.width) / 2
		case "right":
			left += width - p.style.left - p.style.right - line.width
		}
		baseline := page.Height - top - line.ascent
		for _, g := range line.glyphs {
			if g.style.highlight != nil {
				page.AddRect(left, page.Height-top-line.height, g.width, line.height, *g.style.highlight)
			}
			page.operations = append(page.operations, textOperation{text: string(g.r), x: left, y: baseline, fontSize: g.style.size, horizontalScale: 100, color: g.style.color, bold: g.style.bold, italic: g.style.italic, embedded: g.font, synthesize: true})
			left += g.width
		}
		top += line.height
	}
}

type docxBorder struct {
	style string
	width float64
	color PDFColor
}

func readDOCXBorder(n *docxNode) docxBorder {
	if n == nil {
		return docxBorder{}
	}
	return docxBorder{style: n.attr("val"), width: docxNumber(n.attr("sz")) / 8, color: docxColor(n.attr("color"))}
}
func docxBorders(n *docxNode, inherited map[string]docxBorder) map[string]docxBorder {
	b := map[string]docxBorder{}
	for k, v := range inherited {
		b[k] = v
	}
	if n != nil {
		for _, c := range n.children {
			b[c.name] = readDOCXBorder(c)
		}
	}
	return b
}

type docxCell struct {
	column, span, row, rowSpan int
	continuation               bool
	width                      float64
	paras                      []docxParagraph
	contentHeight              float64
	margins                    Margins
	borders                    map[string]docxBorder
	align                      string
	fill                       *PDFColor
}
type docxTable struct {
	cells         []*docxCell
	columns, rows []float64
	width         float64
	align         string
	indent        float64
}

func (s *docxStyles) table(n *docxNode, available float64) (docxTable, error) {
	t := docxTable{align: n.child("tblPr").value("jc")}
	pr := n.child("tblPr")
	var inherited map[string]docxBorder
	tm := Margins{Left: 5.4, Right: 5.4}
	var styleChain func(string, map[string]bool)
	styleChain = func(id string, seen map[string]bool) {
		if seen[id] {
			return
		}
		seen[id] = true
		if st := s.nodes[id]; st != nil {
			styleChain(st.value("basedOn"), seen)
			inherited = docxBorders(st.child("tblPr").child("tblBorders"), inherited)
			tm = docxCellMargins(st.child("tblPr").child("tblCellMar"), tm)
		}
	}
	styleChain(pr.value("tblStyle"), map[string]bool{})
	inherited = docxBorders(pr.child("tblBorders"), inherited)
	tm = docxCellMargins(pr.child("tblCellMar"), tm)
	t.indent = docxNumber(pr.child("tblInd").attr("w")) / 20
	for _, col := range n.child("tblGrid").children {
		if col.name == "gridCol" {
			t.columns = append(t.columns, docxNumber(col.attr("w"))/20)
		}
	}
	for _, w := range t.columns {
		t.width += w
	}
	if w := pr.child("tblW"); w.attr("type") == "dxa" && docxNumber(w.attr("w")) > 0 {
		t.width = docxNumber(w.attr("w")) / 20
	}
	if w := pr.child("tblW"); w.attr("type") == "pct" {
		t.width = available * docxNumber(w.attr("w")) / 5000
	}
	gridWidth := 0.0
	for _, w := range t.columns {
		gridWidth += w
	}
	if gridWidth <= 0 || t.width <= 0 {
		return t, fmt.Errorf("DOCX table has no positive grid")
	}
	for i := range t.columns {
		t.columns[i] *= t.width / gridWidth
	}
	active := map[int]*docxCell{}
	rowCount := 0
	for _, row := range n.children {
		if row.name == "tr" {
			rowCount++
		}
	}
	for _, row := range n.children {
		if row.name != "tr" {
			continue
		}
		ri := len(t.rows)
		height := docxNumber(row.child("trPr").child("trHeight").attr("val")) / 20
		t.rows = append(t.rows, height)
		column := 0
		for _, node := range row.children {
			if node.name != "tc" {
				continue
			}
			cp := node.child("tcPr")
			span := int(docxNumber(cp.child("gridSpan").attr("val")))
			if span < 1 {
				span = 1
			}
			if column+span > len(t.columns) {
				return t, fmt.Errorf("DOCX cell span exceeds table grid: %w", errDOCXTableLayoutUnsupported)
			}
			cell := &docxCell{column: column, span: span, row: ri, rowSpan: 1, margins: docxCellMargins(cp.child("tcMar"), tm), align: cp.value("vAlign"), borders: map[string]docxBorder{}}
			for i := column; i < column+span; i++ {
				cell.width += t.columns[i]
			}
			for _, edge := range []string{"top", "left", "bottom", "right"} {
				key := edge
				if edge == "top" && ri > 0 {
					key = "insideH"
				}
				if edge == "bottom" && ri < rowCount-1 {
					key = "insideH"
				}
				if edge == "left" && column > 0 {
					key = "insideV"
				}
				if edge == "right" && column+span < len(t.columns) {
					key = "insideV"
				}
				cell.borders[edge] = inherited[key]
			}
			cell.borders = docxBorders(cp.child("tcBorders"), cell.borders)
			if shd := cp.child("shd"); shd != nil && shd.attr("fill") != "" && shd.attr("fill") != "auto" {
				c := docxColor(shd.attr("fill"))
				cell.fill = &c
			}
			if merge := cp.child("vMerge"); merge != nil {
				if merge.attr("val") == "restart" {
					active[column] = cell
				} else if start := active[column]; start != nil && start.span == span {
					start.rowSpan++
					cell.continuation = true
					start.borders["bottom"] = cell.borders["bottom"]
				} else {
					return t, fmt.Errorf("DOCX vertical merge has no matching start: %w", errDOCXTableLayoutUnsupported)
				}
			} else {
				delete(active, column)
			}
			if !cell.continuation {
				for _, pn := range node.children {
					if pn.name == "p" {
						p, err := s.layoutParagraph(pn, math.Max(1, cell.width-cell.margins.Left-cell.margins.Right), 0)
						if err != nil {
							return t, err
						}
						cell.paras = append(cell.paras, p)
						cell.contentHeight += p.height
					}
				}
			}
			t.cells = append(t.cells, cell)
			column += span
		}
		if column != len(t.columns) {
			return t, fmt.Errorf("DOCX row does not cover table grid: %w", errDOCXTableLayoutUnsupported)
		}
	}
	for _, c := range t.cells {
		if c.continuation || c.rowSpan > 1 {
			continue
		}
		needed := c.contentHeight + c.margins.Top + c.margins.Bottom + docxBorderWidth(c.borders["top"]) + docxBorderWidth(c.borders["bottom"])
		t.rows[c.row] = math.Max(t.rows[c.row], needed)
	}
	for _, c := range t.cells {
		if c.continuation || c.rowSpan == 1 {
			continue
		}
		height := 0.0
		for i := c.row; i < c.row+c.rowSpan; i++ {
			height += t.rows[i]
		}
		needed := c.contentHeight + c.margins.Top + c.margins.Bottom + docxBorderWidth(c.borders["top"]) + docxBorderWidth(c.borders["bottom"])
		if needed > height {
			extra := (needed - height) / float64(c.rowSpan)
			for i := c.row; i < c.row+c.rowSpan; i++ {
				t.rows[i] += extra
			}
		}
	}
	return t, nil
}
func docxCellMargins(n *docxNode, m Margins) Margins {
	for _, item := range []struct {
		key string
		out *float64
	}{{"left", &m.Left}, {"right", &m.Right}, {"top", &m.Top}, {"bottom", &m.Bottom}} {
		if p := n.child(item.key); p != nil {
			*item.out = docxNumber(p.attr("w")) / 20
		}
	}
	return m
}
func docxBorderWidth(b docxBorder) float64 {
	if b.style == "" || b.style == "nil" || b.style == "none" {
		return 0
	}
	return math.Max(.25, b.width)
}
func drawDOCXBorder(page *PDFPage, b docxBorder, x1, y1, x2, y2 float64) {
	w := docxBorderWidth(b)
	if w == 0 {
		return
	}
	if b.style == "dotted" || b.style == "dotDash" || b.style == "dashed" {
		dash := w * 2
		if b.style == "dashed" {
			dash = w * 6
		}
		page.operations = append(page.operations, lineOperation{x1: x1, y1: y1, x2: x2, y2: y2, color: b.color, width: w, dash: []float64{dash, w * 2}})
		return
	}
	page.AddLine(x1, y1, x2, y2, b.color, w)
}
func drawDOCXTable(page *PDFPage, t docxTable, x, top float64) {
	xs := []float64{x}
	ys := []float64{top}
	for _, w := range t.columns {
		xs = append(xs, xs[len(xs)-1]+w)
	}
	for _, h := range t.rows {
		ys = append(ys, ys[len(ys)-1]+h)
	}
	for _, c := range t.cells {
		if c.continuation {
			continue
		}
		left, right := xs[c.column], xs[c.column+c.span]
		upper, lower := ys[c.row], ys[c.row+c.rowSpan]
		if c.fill != nil {
			page.AddRect(left, page.Height-lower, right-left, lower-upper, *c.fill)
		}
		drawDOCXBorder(page, c.borders["top"], left, page.Height-upper, right, page.Height-upper)
		drawDOCXBorder(page, c.borders["bottom"], left, page.Height-lower, right, page.Height-lower)
		drawDOCXBorder(page, c.borders["left"], left, page.Height-upper, left, page.Height-lower)
		drawDOCXBorder(page, c.borders["right"], right, page.Height-upper, right, page.Height-lower)
		contentTop := upper + c.margins.Top + docxBorderWidth(c.borders["top"])
		space := lower - upper - c.margins.Top - c.margins.Bottom - docxBorderWidth(c.borders["top"]) - docxBorderWidth(c.borders["bottom"]) - c.contentHeight
		if c.align == "center" {
			contentTop += space / 2
		}
		if c.align == "bottom" {
			contentTop += space
		}
		for _, p := range c.paras {
			drawDOCXParagraph(page, p, left+c.margins.Left, contentTop, c.width-c.margins.Left-c.margins.Right)
			contentTop += p.height
		}
	}
}

func readDOCXImages(files officePackage, n *docxNode) ([]docxImage, error) {
	if len(n.descendants("blip")) == 0 {
		return nil, nil
	}
	data, err := files.read("word/_rels/document.xml.rels")
	if err != nil {
		return nil, err
	}
	rels, err := parseDOCXNode(data)
	if err != nil {
		return nil, err
	}
	targets := map[string]string{}
	for _, r := range rels.children {
		if r.attr("TargetMode") != "External" {
			targets[r.attr("Id")] = r.attr("Target")
		}
	}
	var out []docxImage
	var visit func(*docxNode, *docxNode) error
	visit = func(n, shape *docxNode) error {
		if n.name == "shape" {
			shape = n
		}
		if n.name == "drawing" {
			blips := n.descendants("blip")
			extents := n.descendants("extent")
			if len(blips) == 0 || len(extents) == 0 {
				return fmt.Errorf("unsupported DOCX drawing")
			}
			target, ok := targets[blips[0].attr("embed")]
			if !ok {
				return fmt.Errorf("DOCX drawing image relationship is missing")
			}
			name := xlsxResolveRelationshipTarget("word/document.xml", target)
			data, err := files.read(name)
			if err != nil {
				return err
			}
			config, _, err := image.DecodeConfig(bytes.NewReader(data))
			if err != nil {
				return fmt.Errorf("read DOCX image %s: %w", name, err)
			}
			if config.Width <= 0 || config.Height <= 0 || int64(config.Width) > 25000000/int64(config.Height) {
				return fmt.Errorf("DOCX image exceeds 25 million pixels")
			}
			source, _, err := image.Decode(bytes.NewReader(data))
			if err != nil {
				return fmt.Errorf("decode DOCX image %s: %w", name, err)
			}
			op, err := newPDFImage(source, source.Bounds())
			if err != nil {
				return err
			}
			op.width = docxNumber(extents[0].attr("cx")) / 12700
			op.height = docxNumber(extents[0].attr("cy")) / 12700
			if op.width <= 0 || op.height <= 0 {
				return fmt.Errorf("DOCX image has invalid extent")
			}
			picture := docxImage{op: op}
			if shape != nil {
				picture.floating = true
				props := map[string]string{}
				for _, v := range strings.Split(shape.attr("style"), ";") {
					kv := strings.SplitN(v, ":", 2)
					if len(kv) == 2 {
						props[strings.TrimSpace(kv[0])] = strings.TrimSpace(kv[1])
					}
				}
				left, err := docxVMLLength(props["margin-left"])
				if err != nil {
					return err
				}
				top, err := docxVMLLength(props["margin-top"])
				if err != nil {
					return err
				}
				insetLeft, insetTop := 7.2, 3.6
				if inset := shape.child("textbox").attr("inset"); inset != "" {
					parts := strings.Split(inset, ",")
					if len(parts) != 4 {
						return fmt.Errorf("invalid DOCX text-box inset")
					}
					insetLeft, err = docxVMLLength(parts[0])
					if err != nil {
						return err
					}
					insetTop, err = docxVMLLength(parts[1])
					if err != nil {
						return err
					}
				}
				picture.left = left + insetLeft
				picture.top = top + insetTop
			}
			out = append(out, picture)
			return nil
		}
		for _, c := range n.children {
			if err := visit(c, shape); err != nil {
				return err
			}
		}
		return nil
	}
	if err := visit(n, nil); err != nil {
		return nil, err
	}
	return out, nil
}

func docxUnsupportedMetafiles(files officePackage, body *docxNode) (bool, error) {
	blips := body.descendants("blip")
	if len(blips) == 0 {
		return false, nil
	}
	data, err := files.read("word/_rels/document.xml.rels")
	if err != nil {
		return false, err
	}
	rels, err := parseDOCXNode(data)
	if err != nil {
		return false, err
	}
	targets := map[string]string{}
	for _, rel := range rels.children {
		if rel.attr("TargetMode") != "External" {
			targets[rel.attr("Id")] = rel.attr("Target")
		}
	}
	unsupported := false
	for _, blip := range blips {
		if blip.attr("embed") == "" && blip.attr("link") != "" {
			unsupported = true
			continue
		}
		target, ok := targets[blip.attr("embed")]
		if !ok {
			return false, fmt.Errorf("DOCX drawing image relationship is missing")
		}
		name := xlsxResolveRelationshipTarget("word/document.xml", target)
		if _, ok := files[name]; !ok {
			return false, fmt.Errorf("Office package part %q is missing", name)
		}
		switch strings.ToLower(path.Ext(target)) {
		case ".emf", ".wmf":
			unsupported = true
		case ".png", ".jpg", ".jpeg":
			data, err := files.read(name)
			if err != nil {
				return false, err
			}
			if _, _, err := image.DecodeConfig(bytes.NewReader(data)); err != nil {
				return false, fmt.Errorf("read DOCX image %s: %w", name, err)
			}
		}
	}
	return unsupported, nil
}

func docxVMLLength(value string) (float64, error) {
	value = strings.TrimSpace(value)
	if value == "" {
		return 0, nil
	}
	for _, unit := range []struct {
		name  string
		scale float64
	}{{"pt", 1}, {"in", 72}, {"cm", 72 / 2.54}, {"mm", 72 / 25.4}, {"px", 72.0 / 96}} {
		if strings.HasSuffix(value, unit.name) {
			n, err := strconv.ParseFloat(strings.TrimSuffix(value, unit.name), 64)
			if err != nil || math.IsNaN(n) || math.IsInf(n, 0) {
				return 0, fmt.Errorf("invalid DOCX VML length %q", value)
			}
			return n * unit.scale, nil
		}
	}
	if value == "0" {
		return 0, nil
	}
	return 0, fmt.Errorf("unsupported DOCX VML length %q", value)
}

// This structured renderer supports single-page table layouts. Page breaking,
// paragraph keep rules, and row splitting remain on the existing text path.
func renderDOCXTables(files officePackage, data []byte, size PageSize, margins Margins, options ConversionOptions) ([]byte, bool, error) {
	root, err := parseDOCXNode(data)
	if err != nil {
		return nil, false, err
	}
	body := root.child("body")
	if body == nil {
		return nil, false, nil
	}
	if len(body.descendants("numPr")) > 0 || len(body.descendants("anchor")) > 0 || len(body.descendants("pageBreakBefore")) > 0 {
		return nil, false, nil
	}
	if len(body.descendants("gridBefore")) > 0 || len(body.descendants("gridAfter")) > 0 {
		return nil, false, nil
	}
	for _, box := range body.descendants("txbxContent") {
		for _, text := range box.descendants("t") {
			if text.text != "" {
				return nil, false, nil
			}
		}
	}
	for _, br := range body.descendants("br") {
		if br.attr("type") == "page" {
			return nil, false, nil
		}
	}
	for _, height := range body.descendants("trHeight") {
		if height.attr("hRule") == "exact" {
			return nil, false, nil
		}
	}
	for _, border := range append(body.descendants("tcBorders"), body.descendants("tblBorders")...) {
		for _, edge := range border.children {
			switch edge.attr("val") {
			case "", "nil", "none", "single", "dotted", "dotDash", "dashed":
			default:
				return nil, false, nil
			}
		}
	}
	hasTable := false
	for _, n := range body.children {
		if n.name != "p" && n.name != "tbl" && n.name != "sectPr" && n.name != "bookmarkStart" && n.name != "bookmarkEnd" {
			return nil, false, nil
		}
		if n.name == "tbl" {
			hasTable = true
			if len(n.descendants("tbl")) > 0 || len(n.descendants("drawing")) > 0 || n.child("tblPr").child("tblpPr") != nil || n.child("tblGrid") == nil {
				return nil, false, nil
			}
			for _, c := range n.child("tblGrid").children {
				if c.name == "gridCol" && docxNumber(c.attr("w")) <= 0 {
					return nil, false, nil
				}
			}
		}
		if n.name == "p" && (n.child("pPr").child("sectPr") != nil || len(n.descendants("numPr")) > 0) {
			return nil, false, nil
		}
	}
	if !hasTable {
		return nil, false, nil
	}
	unsupportedImages, err := docxUnsupportedMetafiles(files, body)
	if err != nil {
		return nil, true, err
	}
	if unsupportedImages {
		return nil, false, nil
	}
	if docxUnsupportedInlineFlow(body) {
		return nil, false, nil
	}
	s, err := readDOCXStyles(files)
	if err != nil {
		return nil, true, err
	}
	grid := 0.0
	if g := body.child("sectPr").child("docGrid"); g.attr("type") == "lines" || g.attr("type") == "linesAndChars" {
		grid = docxNumber(g.attr("linePitch")) / 20
	}
	doc := NewPDFDocument()
	page := doc.AddPage(size.Width, size.Height)
	top := margins.Top
	previousAfter := 0.0
	available := size.Width - margins.Left - margins.Right
	for _, n := range body.children {
		switch n.name {
		case "p":
			p, err := s.layoutParagraph(n, available, grid)
			if err != nil {
				if errors.Is(err, errDOCXFontUnavailable) {
					return nil, false, nil
				}
				return nil, true, err
			}
			p.style.before = math.Max(0, p.style.before-previousAfter)
			p.height -= math.Min(previousAfter, s.paragraph(n).before)
			if top+p.height > size.Height-margins.Bottom {
				return nil, false, nil
			}
			drawDOCXParagraph(page, p, margins.Left, top, available)
			pictures, err := readDOCXImages(files, n)
			if err != nil {
				return nil, true, err
			}
			for _, picture := range pictures {
				op := picture.op
				op.x = margins.Left + picture.left
				op.y = size.Height - top - picture.top - op.height
				if !picture.floating {
					contentWidth := available - p.style.left - p.style.right - p.style.first
					if op.width > contentWidth {
						return nil, false, nil
					}
					op.x = margins.Left + p.style.left + p.style.first
					if p.style.align == "center" {
						op.x += (contentWidth - op.width) / 2
					} else if p.style.align == "right" {
						op.x += contentWidth - op.width
					}
					op.y = size.Height - top - p.style.before - op.height
					p.height = p.style.before + math.Max(p.height-p.style.before-p.style.after, op.height) + p.style.after
				}
				page.operations = append(page.operations, op)
			}
			if top+p.height > size.Height-margins.Bottom {
				return nil, false, nil
			}
			top += p.height
			previousAfter = p.style.after
		case "tbl":
			t, err := s.table(n, available)
			if err != nil {
				if errors.Is(err, errDOCXFontUnavailable) || errors.Is(err, errDOCXTableLayoutUnsupported) {
					return nil, false, nil
				}
				return nil, true, err
			}
			height := 0.0
			for _, h := range t.rows {
				height += h
			}
			if top+height > size.Height-margins.Bottom {
				return nil, false, nil
			}
			left := margins.Left + t.indent
			if t.align == "center" {
				left = margins.Left + (available-t.width)/2
			}
			if t.align == "right" {
				left = size.Width - margins.Right - t.width
			}
			if left < 0 || left+t.width > size.Width {
				return nil, false, nil
			}
			drawDOCXTable(page, t, left, top)
			top += height
			previousAfter = 0
		}
	}
	return doc.BytesWithOptions(PDFSaveOptions{Compress: options.Compress}), true, nil
}

func docxUnsupportedInlineFlow(body *docxNode) bool {
	for _, p := range body.descendants("p") {
		inlineCount := 0
		var countInline func(*docxNode)
		countInline = func(n *docxNode) {
			// VML pictures have floating placement, not paragraph inline flow.
			if n.name == "pict" {
				return
			}
			if n.name == "drawing" && n.child("inline") != nil {
				inlineCount++
			}
			for _, c := range n.children {
				countInline(c)
			}
		}
		countInline(p)
		if inlineCount > 1 {
			return true
		}
		if inlineCount == 1 {
			if len(p.descendants("br")) > 0 || len(p.descendants("cr")) > 0 || len(p.descendants("tab")) > 0 {
				return true
			}
			for _, text := range p.descendants("t") {
				if text.text != "" {
					return true
				}
			}
		}
	}
	return false
}
