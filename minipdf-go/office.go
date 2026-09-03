package minipdf

import (
	"archive/zip"
	"bytes"
	"encoding/xml"
	"fmt"
	"io"
	"path"
	"sort"
	"strconv"
	"strings"
	"unicode"
)

type officePackage map[string]*zip.File

type textPage struct {
	lines []string
	size  PageSize
}

func openOfficePackage(input []byte) (officePackage, error) {
	reader, err := zip.NewReader(bytes.NewReader(input), int64(len(input)))
	if err != nil {
		return nil, fmt.Errorf("open Office package: %w", err)
	}
	files := make(officePackage, len(reader.File))
	for _, file := range reader.File {
		files[strings.ReplaceAll(file.Name, `\`, "/")] = file
	}
	return files, nil
}

func (files officePackage) read(name string) ([]byte, error) {
	file, ok := files[name]
	if !ok {
		return nil, fmt.Errorf("Office package part %q is missing", name)
	}
	reader, err := file.Open()
	if err != nil {
		return nil, fmt.Errorf("open Office package part %q: %w", name, err)
	}
	defer reader.Close()
	data, err := io.ReadAll(reader)
	if err != nil {
		return nil, fmt.Errorf("read Office package part %q: %w", name, err)
	}
	return data, nil
}

func renderTextPages(pages []textPage, options ConversionOptions) []byte {
	document := NewPDFDocument()
	for _, sourcePage := range pages {
		pageSize := sourcePage.size
		if options.PageSize != nil {
			pageSize = *options.PageSize
		}
		addTextPages(document, sourcePage.lines, pageSize)
	}
	return document.Bytes()
}

func addTextPages(document *PDFDocument, lines []string, pageSize PageSize) {
	const (
		margin   = 54.0
		fontSize = 11.0
		leading  = 15.0
	)
	maxCharacters := int((pageSize.Width - margin*2) / (fontSize * 0.52))
	if maxCharacters < 10 {
		maxCharacters = 10
	}
	wrapped := make([]string, 0, len(lines))
	for _, line := range lines {
		wrapped = append(wrapped, wrapText(line, maxCharacters)...)
	}
	if len(wrapped) == 0 {
		wrapped = append(wrapped, "")
	}

	page := document.AddPage(pageSize.Width, pageSize.Height)
	y := pageSize.Height - margin
	for _, line := range wrapped {
		if y < margin {
			page = document.AddPage(pageSize.Width, pageSize.Height)
			y = pageSize.Height - margin
		}
		page.AddText(line, margin, y, fontSize, PDFColorBlack, false)
		y -= leading
	}
}

func wrapText(text string, limit int) []string {
	if text == "" {
		return []string{""}
	}
	var lines []string
	for _, explicitLine := range strings.Split(text, "\n") {
		runes := []rune(explicitLine)
		for len(runes) > limit {
			breakAt := limit
			for index := limit; index > 0; index-- {
				if unicode.IsSpace(runes[index-1]) {
					breakAt = index
					break
				}
			}
			lines = append(lines, strings.TrimSpace(string(runes[:breakAt])))
			runes = []rune(strings.TrimLeftFunc(string(runes[breakAt:]), unicode.IsSpace))
		}
		lines = append(lines, string(runes))
	}
	return lines
}

func attrValue(start xml.StartElement, localName string) string {
	for _, attribute := range start.Attr {
		if attribute.Name.Local == localName {
			return attribute.Value
		}
	}
	return ""
}

func sortedPackageParts(files officePackage, prefix, extension string) []string {
	var names []string
	for name := range files {
		if strings.HasPrefix(name, prefix) && strings.HasSuffix(name, extension) && !strings.Contains(name, "/_rels/") {
			names = append(names, name)
		}
	}
	sort.Slice(names, func(left, right int) bool {
		leftNumber := trailingNumber(strings.TrimSuffix(path.Base(names[left]), extension))
		rightNumber := trailingNumber(strings.TrimSuffix(path.Base(names[right]), extension))
		if leftNumber == rightNumber {
			return names[left] < names[right]
		}
		return leftNumber < rightNumber
	})
	return names
}

func trailingNumber(value string) int {
	start := len(value)
	for start > 0 && value[start-1] >= '0' && value[start-1] <= '9' {
		start--
	}
	number, err := strconv.Atoi(value[start:])
	if err != nil {
		return 0
	}
	return number
}
