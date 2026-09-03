package minipdf

import (
	"archive/zip"
	"bytes"
	"errors"
	"fmt"
	"math"
	"os"
	"path/filepath"
	"strings"
	"sync"
)

var (
	ErrUnsupportedFormat = errors.New("unsupported or unknown Office document format")
	PageSizeA4           = PageSize{Width: 595.28, Height: 841.89}
	PageSizeLetter       = PageSize{Width: 612, Height: 792}
)

type OfficeFormat string

const (
	OfficeFormatUnknown OfficeFormat = "unknown"
	OfficeFormatXLSX    OfficeFormat = "xlsx"
	OfficeFormatDOCX    OfficeFormat = "docx"
	OfficeFormatPPTX    OfficeFormat = "pptx"
)

type PageSize struct {
	Width  float64
	Height float64
}

func NewPageSize(width, height float64) (PageSize, error) {
	if math.IsNaN(width) || math.IsNaN(height) || math.IsInf(width, 0) || math.IsInf(height, 0) || width <= 0 || height <= 0 {
		return PageSize{}, errors.New("page width and height must be positive finite values")
	}
	return PageSize{Width: width, Height: height}, nil
}

type ConversionOptions struct {
	PageSize *PageSize
}

type RegisteredFont struct {
	Name string
	Data []byte
}

var fontRegistry struct {
	sync.RWMutex
	fonts []RegisteredFont
}

func RegisterFont(name string, data []byte) {
	fontRegistry.Lock()
	defer fontRegistry.Unlock()
	fontRegistry.fonts = append(fontRegistry.fonts, RegisteredFont{Name: name, Data: bytes.Clone(data)})
}

func RegisteredFonts() []RegisteredFont {
	fontRegistry.RLock()
	defer fontRegistry.RUnlock()
	fonts := make([]RegisteredFont, len(fontRegistry.fonts))
	for index, font := range fontRegistry.fonts {
		fonts[index] = RegisteredFont{Name: font.Name, Data: bytes.Clone(font.Data)}
	}
	return fonts
}

func DetectOfficeFormat(input []byte) (OfficeFormat, error) {
	reader, err := zip.NewReader(bytes.NewReader(input), int64(len(input)))
	if err != nil {
		return OfficeFormatUnknown, fmt.Errorf("open Office package: %w", err)
	}
	for _, file := range reader.File {
		name := strings.ReplaceAll(file.Name, `\`, "/")
		switch {
		case strings.HasPrefix(name, "word/"):
			return OfficeFormatDOCX, nil
		case strings.HasPrefix(name, "xl/"):
			return OfficeFormatXLSX, nil
		case strings.HasPrefix(name, "ppt/"):
			return OfficeFormatPPTX, nil
		}
	}
	return OfficeFormatUnknown, nil
}

func ConvertToPDF(inputPath, outputPath string) error {
	return ConvertToPDFWithOptions(inputPath, outputPath, ConversionOptions{})
}

func ConvertToPDFWithOptions(inputPath, outputPath string, options ConversionOptions) error {
	pdf, err := ConvertToPDFBytesWithOptions(inputPath, options)
	if err != nil {
		return err
	}
	if err := os.WriteFile(outputPath, pdf, 0o644); err != nil {
		return fmt.Errorf("write PDF: %w", err)
	}
	return nil
}

func ConvertToPDFBytes(inputPath string) ([]byte, error) {
	return ConvertToPDFBytesWithOptions(inputPath, ConversionOptions{})
}

func ConvertToPDFBytesWithOptions(inputPath string, options ConversionOptions) ([]byte, error) {
	input, err := os.ReadFile(inputPath)
	if err != nil {
		return nil, fmt.Errorf("read input: %w", err)
	}
	extension := strings.ToLower(filepath.Ext(inputPath))
	format := OfficeFormatUnknown
	switch extension {
	case ".docx":
		format = OfficeFormatDOCX
	case ".xlsx":
		format = OfficeFormatXLSX
	case ".pptx":
		format = OfficeFormatPPTX
	}
	return convertBytesAs(input, format, options)
}

func ConvertBytesToPDF(input []byte) ([]byte, error) {
	return ConvertBytesToPDFWithOptions(input, ConversionOptions{})
}

func ConvertBytesToPDFWithOptions(input []byte, options ConversionOptions) ([]byte, error) {
	format, err := DetectOfficeFormat(input)
	if err != nil {
		return nil, err
	}
	return convertBytesAs(input, format, options)
}

func convertBytesAs(input []byte, format OfficeFormat, options ConversionOptions) ([]byte, error) {
	if format == OfficeFormatUnknown {
		detected, err := DetectOfficeFormat(input)
		if err != nil {
			return nil, err
		}
		format = detected
	}
	switch format {
	case OfficeFormatDOCX:
		return convertDOCX(input, options)
	case OfficeFormatXLSX:
		return convertXLSX(input, options)
	case OfficeFormatPPTX:
		return convertPPTX(input, options)
	default:
		return nil, ErrUnsupportedFormat
	}
}
