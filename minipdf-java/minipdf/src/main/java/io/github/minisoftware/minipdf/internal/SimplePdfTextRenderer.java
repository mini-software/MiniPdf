package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdf;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.PdfColor;
import io.github.minisoftware.minipdf.PdfDocument;
import io.github.minisoftware.minipdf.PdfPage;
import io.github.minisoftware.minipdf.RegisteredFont;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.pdmodel.font.PDType0Font;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public final class SimplePdfTextRenderer {
    private static final float MARGIN = 54.0f;
    private static final float FONT_SIZE = 11.0f;
    private static final float LINE_HEIGHT = 15.0f;

    private SimplePdfTextRenderer() {
    }

    public static byte[] render(List<String> sourceLines, ConversionOptions options) throws MiniPdfException {
        return renderPages(List.of(sourceLines), options, PageSize.A4);
    }

    public static byte[] renderPages(
            List<List<String>> sourcePages,
            ConversionOptions options,
            PageSize defaultPageSize) throws MiniPdfException {
        PageSize size = options.pageSize().orElse(defaultPageSize);
        byte[] registeredFontPdf = renderWithRegisteredFont(sourcePages, size);
        if (registeredFontPdf != null) {
            return registeredFontPdf;
        }

        PdfDocument document = new PdfDocument();
        int maxCharacters = Math.max(1, (int) ((size.width() - MARGIN * 2.0f) / (FONT_SIZE * 0.52f)));

        for (List<String> sourcePage : sourcePages) {
            PdfPage page = document.addPage(size.width(), size.height());
            float y = size.height() - MARGIN;
            for (String sourceLine : sourcePage) {
                for (String line : wrap(sourceLine, maxCharacters)) {
                    if (y < MARGIN) {
                        page = document.addPage(size.width(), size.height());
                        y = size.height() - MARGIN;
                    }
                    page.addText(line, MARGIN, y, FONT_SIZE, PdfColor.BLACK, false);
                    y -= LINE_HEIGHT;
                }
            }
        }
        return document.toBytes();
    }

    private static byte[] renderWithRegisteredFont(List<List<String>> sourcePages, PageSize size)
            throws MiniPdfException {
        if (MiniPdf.registeredFonts().isEmpty()) {
            return null;
        }

        try (PDDocument document = new PDDocument()) {
            PDFont font = loadFont(document, sourcePages);
            if (font == null) {
                return null;
            }

            int maxCharacters = Math.max(1, (int) ((size.width() - MARGIN * 2.0f) / (FONT_SIZE * 0.52f)));
            for (List<String> sourcePage : sourcePages) {
                PDPage page = addPage(document, size);
                PDPageContentStream content = new PDPageContentStream(document, page);
                float y = size.height() - MARGIN;
                for (String sourceLine : sourcePage) {
                    for (String line : wrap(sourceLine, maxCharacters)) {
                        if (y < MARGIN) {
                            content.close();
                            page = addPage(document, size);
                            content = new PDPageContentStream(document, page);
                            y = size.height() - MARGIN;
                        }
                        content.beginText();
                        content.setFont(font, FONT_SIZE);
                        content.newLineAtOffset(MARGIN, y);
                        content.showText(line);
                        content.endText();
                        y -= LINE_HEIGHT;
                    }
                }
                content.close();
            }

            ByteArrayOutputStream output = new ByteArrayOutputStream();
            document.save(output);
            return output.toByteArray();
        } catch (IOException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.IO,
                    "failed to render PDF with a registered font: " + exception.getMessage(),
                    exception);
        }
    }

    private static PDFont loadFont(PDDocument document, List<List<String>> sourcePages) {
        for (RegisteredFont registeredFont : MiniPdf.registeredFonts()) {
            try {
                PDFont font = PDType0Font.load(
                        document,
                        new ByteArrayInputStream(registeredFont.data()),
                        true);
                if (supports(font, sourcePages)) {
                    return font;
                }
            } catch (IOException | IllegalArgumentException ignored) {
                // Try the next registered font.
            }
        }
        return null;
    }

    private static boolean supports(PDFont font, List<List<String>> sourcePages) {
        try {
            for (List<String> sourcePage : sourcePages) {
                for (String line : sourcePage) {
                    font.getStringWidth(line);
                }
            }
            return true;
        } catch (IOException | IllegalArgumentException exception) {
            return false;
        }
    }

    private static PDPage addPage(PDDocument document, PageSize size) {
        PDPage page = new PDPage(new PDRectangle(size.width(), size.height()));
        document.addPage(page);
        return page;
    }

    private static List<String> wrap(String value, int maxCharacters) {
        if (value.isEmpty()) {
            return List.of("");
        }
        List<String> lines = new ArrayList<>();
        String remaining = value;
        while (remaining.length() > maxCharacters) {
            int split = remaining.lastIndexOf(' ', maxCharacters);
            if (split <= 0) {
                split = maxCharacters;
            }
            lines.add(remaining.substring(0, split));
            remaining = remaining.substring(split).stripLeading();
        }
        lines.add(remaining);
        return lines;
    }
}