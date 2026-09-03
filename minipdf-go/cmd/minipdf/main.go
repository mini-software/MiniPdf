package main

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"

	minipdf "github.com/mini-software/MiniPdf/minipdf-go"
)

var (
	errHelp    = errors.New("help requested")
	errVersion = errors.New("version requested")
	version    = "dev"
)

type cliOptions struct {
	input      string
	output     string
	paperSize  string
	pageWidth  float64
	pageHeight float64
}

func main() {
	options, err := parseArguments(os.Args[1:])
	if errors.Is(err, errHelp) {
		printUsage()
		return
	}
	if errors.Is(err, errVersion) {
		fmt.Println("minipdf", version)
		return
	}
	if err != nil {
		fmt.Fprintln(os.Stderr, "Error:", err)
		os.Exit(1)
	}
	if err := run(options); err != nil {
		fmt.Fprintln(os.Stderr, "Error:", err)
		os.Exit(1)
	}
}

func run(options cliOptions) error {
	if _, err := os.Stat(options.input); err != nil {
		return fmt.Errorf("input file: %w", err)
	}
	extension := strings.ToLower(filepath.Ext(options.input))
	if extension != ".docx" && extension != ".xlsx" && extension != ".pptx" {
		return fmt.Errorf("unsupported file type %q; supported: .xlsx, .docx, .pptx", extension)
	}

	conversionOptions, err := conversionOptions(options)
	if err != nil {
		return err
	}
	output := options.output
	if output == "" {
		output = strings.TrimSuffix(options.input, filepath.Ext(options.input)) + ".pdf"
	}
	if err := minipdf.ConvertToPDFWithOptions(options.input, output, conversionOptions); err != nil {
		return err
	}
	fmt.Println(output)
	return nil
}

func conversionOptions(options cliOptions) (minipdf.ConversionOptions, error) {
	customPageSize := options.pageWidth != 0 || options.pageHeight != 0
	if options.paperSize != "" && customPageSize {
		return minipdf.ConversionOptions{}, errors.New("use either --paper-size or --page-width/--page-height, not both")
	}
	if (options.pageWidth == 0) != (options.pageHeight == 0) {
		return minipdf.ConversionOptions{}, errors.New("--page-width and --page-height must be specified together")
	}
	var pageSize *minipdf.PageSize
	switch strings.ToLower(options.paperSize) {
	case "":
	case "a4":
		size := minipdf.PageSizeA4
		pageSize = &size
	case "letter":
		size := minipdf.PageSizeLetter
		pageSize = &size
	default:
		return minipdf.ConversionOptions{}, fmt.Errorf("unknown paper size %q; supported: a4, letter", options.paperSize)
	}
	if customPageSize {
		size, err := minipdf.NewPageSize(options.pageWidth, options.pageHeight)
		if err != nil {
			return minipdf.ConversionOptions{}, err
		}
		pageSize = &size
	}
	return minipdf.ConversionOptions{PageSize: pageSize}, nil
}

func parseArguments(arguments []string) (cliOptions, error) {
	if len(arguments) > 0 && arguments[0] == "convert" {
		arguments = arguments[1:]
	}
	var options cliOptions
	for index := 0; index < len(arguments); index++ {
		argument := arguments[index]
		if argument == "-h" || argument == "--help" {
			return cliOptions{}, errHelp
		}
		if argument == "--version" {
			return cliOptions{}, errVersion
		}
		name, inlineValue, hasInlineValue := strings.Cut(argument, "=")
		switch name {
		case "-o", "--output", "--paper-size", "--page-width", "--page-height":
			value := inlineValue
			if !hasInlineValue {
				index++
				if index >= len(arguments) {
					return cliOptions{}, fmt.Errorf("%s requires a value", name)
				}
				value = arguments[index]
			}
			switch name {
			case "-o", "--output":
				options.output = value
			case "--paper-size":
				options.paperSize = value
			case "--page-width":
				width, err := strconv.ParseFloat(value, 64)
				if err != nil {
					return cliOptions{}, fmt.Errorf("invalid page width %q", value)
				}
				options.pageWidth = width
			case "--page-height":
				height, err := strconv.ParseFloat(value, 64)
				if err != nil {
					return cliOptions{}, fmt.Errorf("invalid page height %q", value)
				}
				options.pageHeight = height
			}
		default:
			if strings.HasPrefix(argument, "-") {
				return cliOptions{}, fmt.Errorf("unknown option %q", argument)
			}
			if options.input != "" {
				return cliOptions{}, errors.New("only one input file can be converted at a time")
			}
			options.input = argument
		}
	}
	if options.input == "" {
		return cliOptions{}, errors.New("input file is required; use --help for usage")
	}
	return options, nil
}

func printUsage() {
	fmt.Println(`MiniPdf for Go

Usage:
  minipdf INPUT [options]
  minipdf convert INPUT [options]

Options:
  -o, --output PATH          Output PDF path
      --paper-size SIZE      a4 or letter
      --page-width POINTS    Custom page width
      --page-height POINTS   Custom page height
			--version              Show the build version
  -h, --help                 Show this help`)
}
