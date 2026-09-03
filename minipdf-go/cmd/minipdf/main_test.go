package main

import (
	"errors"
	"testing"
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
