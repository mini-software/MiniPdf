package io.github.minisoftware.minipdf;

import io.github.minisoftware.minipdf.internal.OfficePackageDetector;
import io.github.minisoftware.minipdf.internal.docx.DocxConverter;
import io.github.minisoftware.minipdf.internal.xlsx.XlsxConverter;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CopyOnWriteArrayList;

public final class MiniPdf {
    private static final CopyOnWriteArrayList<RegisteredFont> REGISTERED_FONTS = new CopyOnWriteArrayList<>();

    private MiniPdf() {
    }

    public static void registerFont(String name, byte[] fontData) {
        if (name == null || name.isBlank()) {
            throw new IllegalArgumentException("font name must not be blank");
        }
        REGISTERED_FONTS.add(new RegisteredFont(name, fontData));
    }

    public static List<RegisteredFont> registeredFonts() {
        return List.copyOf(REGISTERED_FONTS);
    }

    public static OfficeFormat detectOfficeFormat(byte[] input) throws MiniPdfException {
        return OfficePackageDetector.detect(Objects.requireNonNull(input, "input"));
    }

    public static byte[] convertToPdfBytes(Path inputPath) throws MiniPdfException {
        return convertToPdfBytes(inputPath, ConversionOptions.defaults());
    }

    public static byte[] convertToPdfBytes(Path inputPath, ConversionOptions options) throws MiniPdfException {
        Objects.requireNonNull(inputPath, "inputPath");
        Objects.requireNonNull(options, "options");
        try {
            return convertBytesToPdf(Files.readAllBytes(inputPath), options);
        } catch (IOException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.IO,
                    "I/O error: " + exception.getMessage(),
                    exception);
        }
    }

    public static byte[] convertBytesToPdf(byte[] input) throws MiniPdfException {
        return convertBytesToPdf(input, ConversionOptions.defaults());
    }

    public static byte[] convertBytesToPdf(byte[] input, ConversionOptions options) throws MiniPdfException {
        Objects.requireNonNull(options, "options");
        OfficeFormat format = detectOfficeFormat(input);
        return switch (format) {
            case DOCX -> DocxConverter.convert(input, options);
            case XLSX -> XlsxConverter.convert(input, options);
            case PPTX, UNKNOWN -> throw new MiniPdfException(
                MiniPdfException.Kind.UNSUPPORTED_FORMAT,
                "unsupported or unknown Office document format");
        };
    }

    public static void convertToPdf(Path inputPath, Path outputPath) throws MiniPdfException {
        convertToPdf(inputPath, outputPath, ConversionOptions.defaults());
    }

    public static void convertToPdf(Path inputPath, Path outputPath, ConversionOptions options)
            throws MiniPdfException {
        Objects.requireNonNull(outputPath, "outputPath");
        byte[] pdf = convertToPdfBytes(inputPath, options);
        try {
            Files.write(outputPath, pdf);
        } catch (IOException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.IO,
                    "I/O error: " + exception.getMessage(),
                    exception);
        }
    }
}