package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdf;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import io.github.minisoftware.minipdf.PdfColor;
import io.github.minisoftware.minipdf.PdfDocument;
import io.github.minisoftware.minipdf.PdfPage;
import io.github.minisoftware.minipdf.RegisteredFont;
import org.apache.fontbox.ttf.TTFParser;
import org.apache.fontbox.ttf.TrueTypeCollection;
import org.apache.fontbox.ttf.TrueTypeFont;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.pdmodel.font.PDType0Font;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

public final class SimplePdfTextRenderer {
    private static final float MARGIN = 54.0f;
    private static final float FONT_SIZE = 11.0f;
    private static final float LINE_HEIGHT = 15.0f;

    private SimplePdfTextRenderer() {
    }

    public static byte[] render(List<String> sourceLines, ConversionOptions options) throws MiniPdfException {
        return renderPages(Collections.singletonList(sourceLines), options, PageSize.A4);
    }

    public static byte[] renderPages(
            List<List<String>> sourcePages,
            ConversionOptions options,
            PageSize defaultPageSize) throws MiniPdfException {
        PageSize size = options.pageSize().orElse(defaultPageSize);
        byte[] nativePdf = renderNative(sourcePages, size);
        if (usesNativeRegisteredFont(nativePdf, sourcePages, size)) {
            return nativePdf;
        }
        byte[] unicodeFontPdf = renderWithUnicodeFont(sourcePages, size);
        if (unicodeFontPdf != null) {
            return unicodeFontPdf;
        }
        return nativePdf;
    }

    private static byte[] renderNative(List<List<String>> sourcePages, PageSize size) {
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

    private static boolean usesNativeRegisteredFont(
            byte[] pdf,
            List<List<String>> sourcePages,
            PageSize size) {
        if (MiniPdf.registeredFonts().isEmpty()) {
            return false;
        }
        String raw = new String(pdf, StandardCharsets.ISO_8859_1);
        if (!raw.contains("/Subtype /Type0")) {
            return false;
        }
        int maxCharacters = Math.max(1, (int) ((size.width() - MARGIN * 2.0f) / (FONT_SIZE * 0.52f)));
        boolean hasUnicode = false;
        for (List<String> sourcePage : sourcePages) {
            for (String sourceLine : sourcePage) {
                for (String line : wrap(sourceLine, maxCharacters)) {
                    if (line.codePoints().anyMatch(codePoint -> codePoint > 255)) {
                        hasUnicode = true;
                        if (!raw.contains('<' + unicodeHex(line) + "> Tj")) {
                            return false;
                        }
                    }
                }
            }
        }
        return hasUnicode;
    }

    private static String unicodeHex(String text) {
        StringBuilder result = new StringBuilder(text.length() * 4);
        for (int index = 0; index < text.length(); index++) {
            result.append(String.format(Locale.ROOT, "%04X", (int) text.charAt(index)));
        }
        return result.toString();
    }

    private static byte[] renderWithUnicodeFont(List<List<String>> sourcePages, PageSize size)
            throws MiniPdfException {
        boolean requiresSystemFont = sourcePages.stream()
                .flatMap(List::stream)
                .flatMapToInt(String::codePoints)
                .anyMatch(codePoint -> codePoint > 255);
        if (MiniPdf.registeredFonts().isEmpty() && !requiresSystemFont) {
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
                try {
                    float y = size.height() - MARGIN;
                    for (String sourceLine : sourcePage) {
                        for (String line : wrap(sourceLine, maxCharacters)) {
                            if (y < MARGIN) {
                                PDPageContentStream completedContent = content;
                                content = null;
                                completedContent.close();
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
                } finally {
                    if (content != null) {
                        content.close();
                    }
                }
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

    public static PDFont loadFont(PDDocument document, List<List<String>> sourcePages) {
        for (RegisteredFont registeredFont : MiniPdf.registeredFonts()) {
            try {
                PDFont font = PDType0Font.load(
                        document,
                        new ByteArrayInputStream(registeredFont.data()),
                    FontEmbeddingPolicy.shouldSubset());
                if (supports(font, sourcePages)) {
                    return font;
                }
            } catch (IOException | IllegalArgumentException ignored) {
                // Try the next registered font.
            }
        }
        return loadSystemFont(
                document,
                sourcePages,
                "simsun.ttc",
                "msyh.ttc",
                "mingliu.ttc",
                "arialuni.ttf");
    }

    public static PDFont loadSystemFont(
            PDDocument document,
            List<List<String>> sourcePages,
            String... fileNames) {
        for (Path path : systemFontPaths(fileNames)) {
            if (!Files.isRegularFile(path)) {
                continue;
            }
            try {
                PDFont font = path.getFileName().toString().toLowerCase().endsWith(".ttc")
                        ? loadCollectionFont(document, path, sourcePages)
                    : PDType0Font.load(
                        document,
                        Files.newInputStream(path),
                        FontEmbeddingPolicy.shouldSubset());
                if (font != null && supports(font, sourcePages)) {
                    return font;
                }
            } catch (IOException | IllegalArgumentException ignored) {
                // Try the next system font.
            }
        }
        return null;
    }

    /**
     * Loads a font by family name instead of file name, mirroring MiniPdf for
     * .NET's FindOfficeCloudFontByPreferredName: searches the Microsoft 365
     * cloud font cache (under %LOCALAPPDATA%/Microsoft/FontCache, in the
     * per-version CloudFonts family subfolder) and a small Windows/Fonts
     * file-name map. The requested name may carry a weight suffix ("Source
     * Sans Pro Bold") to prefer the matching sub-family face.
     */
    public static PDFont loadSystemFontByFamily(
            PDDocument document,
            List<List<String>> sourcePages,
            String... familyNames) {
        for (String family : familyNames) {
            for (Path path : familyFontPaths(family)) {
                if (!Files.isRegularFile(path)) {
                    continue;
                }
                try {
                    PDFont font = PDType0Font.load(
                            document,
                            Files.newInputStream(path),
                            FontEmbeddingPolicy.shouldSubset());
                    if (font != null && supports(font, sourcePages)) {
                        return font;
                    }
                } catch (IOException | IllegalArgumentException ignored) {
                    // Try the next candidate.
                }
            }
        }
        return null;
    }

    /**
     * Well-known Windows font families and their file names. Cloud font
     * families resolve through {@link #familyFontPaths} instead.
     */
    private static final Map<String, String> WINDOWS_FONT_FILES = new HashMap<>();

    static {
        WINDOWS_FONT_FILES.put("impact", "impact.ttf");
    }

    /**
     * Resolves a family name (optionally suffixed with " Regular" / " Bold") to
     * candidate font file paths. Exact sub-family matches win over a plain
     * family match, mirroring Word's font selection.
     */
    private static List<Path> familyFontPaths(String family) {
        List<Path> paths = new ArrayList<>();
        if (family == null || family.trim().isEmpty()) {
            return paths;
        }
        String requested = family.trim();
        String base = requested;
        String wantedSubFamily = "";
        for (String suffix : new String[] {" Bold Italic", " Bold", " Italic", " Regular"}) {
            if (requested.toLowerCase(Locale.ROOT).endsWith(suffix.toLowerCase(Locale.ROOT))) {
                wantedSubFamily = suffix.trim();
                base = requested.substring(0, requested.length() - suffix.length()).trim();
                break;
            }
        }
        // Windows\Fonts: exact known file names.
        String windows = System.getenv("WINDIR");
        if (windows != null) {
            String mapped = WINDOWS_FONT_FILES.get(base.toLowerCase(Locale.ROOT));
            if (mapped != null) {
                paths.add(Paths.get(windows, "Fonts", mapped));
            }
        }
        // Microsoft 365 cloud font cache.
        String localAppData = System.getenv("LOCALAPPDATA");
        if (localAppData == null) {
            return paths;
        }
        Path cacheRoot = Paths.get(localAppData, "Microsoft", "FontCache");
        if (!Files.isDirectory(cacheRoot)) {
            return paths;
        }
        Path exact = null;
        Path familyFallback = null;
        try (DirectoryStream<Path> versions = Files.newDirectoryStream(cacheRoot)) {
            for (Path version : versions) {
                Path familyDir = version.resolve("CloudFonts").resolve(base);
                if (!Files.isDirectory(familyDir)) {
                    continue;
                }
                try (DirectoryStream<Path> files = Files.newDirectoryStream(familyDir)) {
                    for (Path file : files) {
                        if (!isFontFile(file)) {
                            continue;
                        }
                        String[] names = fontNames(file);
                        String subFamily = names[0];
                        String postScript = names[1];
                        if (wantedSubFamily.isEmpty()) {
                            if (subFamily != null && subFamily.equalsIgnoreCase("regular")
                                    || postScript != null
                                        && postScript.toLowerCase(Locale.ROOT).contains("regular")) {
                                exact = file;
                            } else if (familyFallback == null) {
                                familyFallback = file;
                            }
                        } else if (subFamily != null && subFamily.equalsIgnoreCase(wantedSubFamily)) {
                            exact = file;
                        }
                    }
                }
                if (exact != null) {
                    break;
                }
            }
        } catch (IOException ignored) {
            // Cloud cache unavailable; keep whatever was found.
        }
        if (exact != null) {
            paths.add(exact);
        } else if (familyFallback != null) {
            paths.add(familyFallback);
        }
        return paths;
    }

    private static boolean isFontFile(Path path) {
        String name = path.getFileName().toString().toLowerCase(Locale.ROOT);
        return name.endsWith(".ttf") || name.endsWith(".otf");
    }

    /**
     * Reads {sub-family, postscript} names from a TrueType/OpenType file.
     */
    private static String[] fontNames(Path path) {
        try (InputStream stream = Files.newInputStream(path)) {
            TrueTypeFont font = new TTFParser().parseEmbedded(stream);
            return new String[] {
                font.getNaming().getFontSubFamily(),
                font.getNaming().getPostScriptName()
            };
        } catch (IOException ignored) {
            return new String[] {null, null};
        }
    }

    private static PDFont loadCollectionFont(
            PDDocument document,
            Path path,
            List<List<String>> sourcePages) throws IOException {
        PDFont[] supported = new PDFont[1];
        try (TrueTypeCollection collection = new TrueTypeCollection(path.toFile())) {
            collection.processAllFonts(font -> {
                if (supported[0] == null) {
                    PDFont candidate = PDType0Font.load(
                            document,
                            font,
                            FontEmbeddingPolicy.shouldSubset());
                    if (supports(candidate, sourcePages)) {
                        supported[0] = candidate;
                    }
                }
            });
        }
        return supported[0];
    }

    private static List<Path> systemFontPaths(String... names) {
        List<Path> paths = new ArrayList<>();
        String windows = System.getenv("WINDIR");
        if (windows != null) {
            Path fonts = Paths.get(windows, "Fonts");
            for (String name : names) {
                paths.add(fonts.resolve(name));
            }
        }
        for (String name : names) {
            paths.add(Paths.get("/usr/share/fonts/truetype/noto", name));
            paths.add(Paths.get("/usr/share/fonts/opentype/noto", name));
            paths.add(Paths.get("/System/Library/Fonts", name));
            paths.add(Paths.get("/System/Library/Fonts/Supplemental", name));
        }
        return paths;
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
            return Collections.singletonList("");
        }
        List<String> lines = new ArrayList<>();
        String remaining = value;
        while (remaining.length() > maxCharacters) {
            int split = remaining.lastIndexOf(' ', maxCharacters);
            if (split <= 0) {
                split = maxCharacters;
            }
            lines.add(remaining.substring(0, split));
            remaining = stripLeading(remaining.substring(split));
        }
        lines.add(remaining);
        return lines;
    }

    private static String stripLeading(String value) {
        int index = 0;
        while (index < value.length() && Character.isWhitespace(value.charAt(index))) {
            index++;
        }
        return value.substring(index);
    }
}