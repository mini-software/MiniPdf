package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.PdfColor;
import io.github.minisoftware.minipdf.PdfDocument;
import io.github.minisoftware.minipdf.PdfPage;

import java.util.ArrayList;
import java.util.List;

public final class SimplePdfTextRenderer {
    private static final float MARGIN = 54.0f;
    private static final float FONT_SIZE = 11.0f;
    private static final float LINE_HEIGHT = 15.0f;

    private SimplePdfTextRenderer() {
    }

    public static byte[] render(List<String> sourceLines, ConversionOptions options) {
        return renderPages(List.of(sourceLines), options, PageSize.A4);
    }

    public static byte[] renderPages(
            List<List<String>> sourcePages,
            ConversionOptions options,
            PageSize defaultPageSize) {
        PageSize size = options.pageSize().orElse(defaultPageSize);
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