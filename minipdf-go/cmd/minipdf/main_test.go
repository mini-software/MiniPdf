package main

import (
	"bytes"
	"errors"
	"os"
	"path/filepath"
	"testing"

	minipdf "github.com/mini-software/MiniPdf/minipdf-go"
)

func TestParseArgumentsAcceptsVersionWithoutInput(t *testing.T) {
	_, err := parseArguments([]string{"--version"})
	if !errors.Is(err, errVersion) {
		t.Fatalf("parseArguments() error = %v, want errVersion", err)
	}
}

func TestConversionOptionsRejectsMixedPageSizes(t *testing.T) {
	_, err := conversionOptions(cliOptions{paperSize: "a4", pageWidth: 300, pageHeight: 400})
	if err == nil {
		t.Fatal("conversionOptions() accepted preset and custom page sizes")
	}
}

func TestParseArgumentsAcceptsFontDirectory(t *testing.T) {
	options, err := parseArguments([]string{"report.docx", "--fonts", "fonts"})
	if err != nil {
		t.Fatal(err)
	}
	if options.fontDirectory != "fonts" {
		t.Fatalf("fontDirectory = %q, want %q", options.fontDirectory, "fonts")
	}
}

func TestRegisterFontsFromDirectory(t *testing.T) {
	minipdf.ClearRegisteredFonts()
	t.Cleanup(minipdf.ClearRegisteredFonts)
	directory := t.TempDir()
	fontData := []byte("test font")
	if err := os.WriteFile(filepath.Join(directory, "Example.TTF"), fontData, 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(directory, "ignored.txt"), []byte("ignored"), 0o600); err != nil {
		t.Fatal(err)
	}

	if err := registerFontsFromDirectory(directory); err != nil {
		t.Fatal(err)
	}
	fonts := minipdf.RegisteredFonts()
	if len(fonts) != 1 || fonts[0].Name != "Example" || !bytes.Equal(fonts[0].Data, fontData) {
		t.Fatalf("registered fonts = %#v", fonts)
	}
}
