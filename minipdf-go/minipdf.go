package minipdf

import (
	"bytes"
	"errors"
	"fmt"
	"io"
	"math"
	"os"
	"path/filepath"
	"strings"
	"sync"
)

var (
	ErrUnsupportedFormat = errors.New("unsupported or unknown Office document format")
	ErrInvalidPackage    = errors.New("invalid Office package")
	ErrInvalidInput      = errors.New("invalid input")
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
		return PageSize{}, fmt.Errorf("%w: page width and height must be positive finite values", ErrInvalidInput)
	}
	return PageSize{Width: width, Height: height}, nil
}

type Margins struct {
	Left   float64
	Top    float64
	Right  float64
	Bottom float64
}

// NewMargins creates validated page margins measured in PDF points.
func NewMargins(left, top, right, bottom float64) (Margins, error) {
	values := []float64{left, top, right, bottom}
	for _, value := range values {
		if math.IsNaN(value) || math.IsInf(value, 0) || value < 0 {
			return Margins{}, fmt.Errorf("%w: margins must be non-negative finite values", ErrInvalidInput)
		}
	}
	return Margins{Left: left, Top: top, Right: right, Bottom: bottom}, nil
}

type ConversionOptions struct {
	PageSize *PageSize
	// Margins overrides DOCX page margins in PDF points.
	Margins *Margins
	// Compress applies Flate compression to PDF page content streams.
	Compress bool
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

// ClearRegisteredFonts removes all process-wide font registrations.
func ClearRegisteredFonts() {
	fontRegistry.Lock()
	defer fontRegistry.Unlock()
	fontRegistry.fonts = nil
}

// ConvertReaderToPDF reads an Office package and returns the converted PDF.
func ConvertReaderToPDF(input io.Reader) ([]byte, error) {
	return ConvertReaderToPDFWithOptions(input, ConversionOptions{})
}

// ConvertReaderToPDFWithOptions reads an Office package and returns the converted PDF.
func ConvertReaderToPDFWithOptions(input io.Reader, options ConversionOptions) ([]byte, error) {
	data, err := io.ReadAll(io.LimitReader(input, int64(defaultOfficePackageLimits.maxTotalSize)+1))
	if err != nil {
		return nil, fmt.Errorf("read input: %w", err)
	}
	if uint64(len(data)) > defaultOfficePackageLimits.maxTotalSize {
		return nil, fmt.Errorf("%w: input exceeds the configured size limit", ErrInvalidPackage)
	}
	return ConvertBytesToPDFWithOptions(data, options)
}

// ConvertReaderToWriter converts an Office package and writes the PDF to output.
func ConvertReaderToWriter(input io.Reader, output io.Writer, options ConversionOptions) error {
	pdf, err := ConvertReaderToPDFWithOptions(input, options)
	if err != nil {
		return err
	}
	if _, err := io.Copy(output, bytes.NewReader(pdf)); err != nil {
		return fmt.Errorf("write PDF: %w", err)
	}
	return nil
}

func DetectOfficeFormat(input []byte) (OfficeFormat, error) {
	files, err := openOfficePackage(input)
	if err != nil {
		return OfficeFormatUnknown, err
	}
	for name := range files {
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
	if options.Margins != nil && format != OfficeFormatDOCX {
		return nil, fmt.Errorf("%w: margin overrides apply only to DOCX input", ErrInvalidInput)
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
