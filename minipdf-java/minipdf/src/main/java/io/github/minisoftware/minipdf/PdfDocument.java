package io.github.minisoftware.minipdf;

import java.io.ByteArrayOutputStream;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Locale;
import java.util.SortedSet;
import java.util.TreeSet;

public final class PdfDocument {
    private static final Charset PDF_TEXT_ENCODING = Charset.forName("windows-1252");

    private final List<PdfPage> pages = new ArrayList<>();

    public PdfPage addPage(float width, float height) {
        if (!Float.isFinite(width) || !Float.isFinite(height) || width <= 0.0f || height <= 0.0f) {
            throw new IllegalArgumentException("page width and height must be positive finite values");
        }
        PdfPage page = new PdfPage(width, height);
        pages.add(page);
        return page;
    }

    public List<PdfPage> pages() {
        return Collections.unmodifiableList(new ArrayList<>(pages));
    }

    public byte[] toBytes() {
        TrueTypeFontData embeddedFont = findEmbeddedFont();
        SortedSet<Integer> unicodeCodePoints = unicodeCodePoints(embeddedFont);
        int embeddedFontObjectCount = embeddedFont == null ? 0 : 6;
        int firstPageObjectNumber = 5 + embeddedFontObjectCount;
        int objectCount = 4 + embeddedFontObjectCount + pages.size() * 2;
        List<byte[]> objects = new ArrayList<>(objectCount);
        objects.add(ascii("<< /Type /Catalog /Pages 2 0 R >>"));
        objects.add(ascii(pagesObject(firstPageObjectNumber)));
        objects.add(ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
        objects.add(ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"));

        if (embeddedFont != null) {
            byte[] fontBytes = embeddedFont.data();
            objects.add(streamObject(embeddedFont.toUnicode(unicodeCodePoints)));
            objects.add(ascii("<< /Type /FontDescriptor /FontName /" + embeddedFont.pdfName()
                + " /Flags 32 /FontBBox [" + embeddedFont.boundingBox() + "]"
                + " /ItalicAngle 0 /Ascent " + embeddedFont.ascent()
                + " /Descent " + embeddedFont.descent()
                + " /CapHeight " + embeddedFont.capHeight()
                + " /StemV 80 /FontFile2 9 0 R >>"));
            objects.add(ascii("<< /Type /Font /Subtype /CIDFontType2 /BaseFont /" + embeddedFont.pdfName()
                + " /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >>"
                + " /FontDescriptor 6 0 R /W " + embeddedFont.widths(unicodeCodePoints)
                + " /CIDToGIDMap 10 0 R >>"));
            objects.add(ascii("<< /Type /Font /Subtype /Type0 /BaseFont /" + embeddedFont.pdfName()
                + " /Encoding /Identity-H /DescendantFonts [7 0 R] /ToUnicode 5 0 R >>"));
            objects.add(streamObject(fontBytes, "/Length1 " + fontBytes.length));
            objects.add(streamObject(embeddedFont.cidToGidMap(unicodeCodePoints)));
        }

        for (int index = 0; index < pages.size(); index++) {
            PdfPage page = pages.get(index);
            int contentObjectNumber = firstPageObjectNumber + index * 2 + 1;
            objects.add(ascii(pageObject(page, contentObjectNumber, embeddedFont != null)));
            objects.add(streamObject(pageContent(page, embeddedFont)));
        }

        ByteArrayOutputStream output = new ByteArrayOutputStream();
        write(output, "%PDF-1.4\n");
        write(output, new byte[]{'%', (byte) 0xE2, (byte) 0xE3, (byte) 0xCF, (byte) 0xD3, '\n'});

        List<Integer> offsets = new ArrayList<>(objectCount);
        for (int index = 0; index < objects.size(); index++) {
            offsets.add(output.size());
            write(output, (index + 1) + " 0 obj\n");
            write(output, objects.get(index));
            write(output, "\nendobj\n");
        }

        int xrefOffset = output.size();
        write(output, "xref\n0 " + (objectCount + 1) + "\n");
        write(output, "0000000000 65535 f \n");
        for (int offset : offsets) {
            write(output, String.format(Locale.ROOT, "%010d 00000 n \n", offset));
        }
        write(output, "trailer\n<< /Size " + (objectCount + 1) + " /Root 1 0 R >>\n");
        write(output, "startxref\n" + xrefOffset + "\n%%EOF\n");
        return output.toByteArray();
    }

    private String pagesObject(int firstPageObjectNumber) {
        StringBuilder kids = new StringBuilder();
        for (int index = 0; index < pages.size(); index++) {
            kids.append(firstPageObjectNumber + index * 2).append(" 0 R ");
        }
        return "<< /Type /Pages /Kids [ " + kids + "] /Count " + pages.size() + " >>";
    }

    private static String pageObject(PdfPage page, int contentObjectNumber, boolean hasEmbeddedFont) {
        return "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 "
                + number(page.width()) + ' ' + number(page.height())
                + "] /Resources << /Font << /F1 3 0 R /F2 4 0 R"
                + (hasEmbeddedFont ? " /F3 8 0 R" : "")
                + " >> >> /Contents "
                + contentObjectNumber + " 0 R >>";
    }

    private static byte[] pageContent(PdfPage page, TrueTypeFontData embeddedFont) {
        ByteArrayOutputStream content = new ByteArrayOutputStream();
        for (PdfPage.TextOperation operation : page.operations()) {
            boolean useEmbeddedFont = embeddedFont != null
                    && requiresUnicode(operation.text())
                    && embeddedFont.supports(codePoints(operation.text()));
            write(content, "BT\n");
            write(content, useEmbeddedFont
                    ? "/F3 " + number(operation.size()) + " Tf\n"
                    : (operation.bold() ? "/F2 " : "/F1 ") + number(operation.size()) + " Tf\n");
            write(content, number(operation.color().red()) + ' '
                    + number(operation.color().green()) + ' '
                    + number(operation.color().blue()) + " rg\n");
            write(content, "1 0 0 1 " + number(operation.x()) + ' ' + number(operation.y()) + " Tm\n");
            if (useEmbeddedFont) {
                write(content, '<' + unicodeHex(operation.text()) + "> Tj\nET\n");
            } else {
                write(content, "(");
                write(content, escapeText(operation.text()));
                write(content, ") Tj\nET\n");
            }
        }
        return content.toByteArray();
    }

    private SortedSet<Integer> unicodeCodePoints(TrueTypeFontData font) {
        SortedSet<Integer> codePoints = new TreeSet<>();
        if (font == null) {
            return codePoints;
        }
        for (PdfPage page : pages) {
            for (PdfPage.TextOperation operation : page.operations()) {
                if (requiresUnicode(operation.text()) && font.supports(codePoints(operation.text()))) {
                    operation.text().codePoints().forEach(codePoints::add);
                }
            }
        }
        return codePoints;
    }

    private TrueTypeFontData findEmbeddedFont() {
        for (RegisteredFont registeredFont : MiniPdf.registeredFonts()) {
            try {
                TrueTypeFontData font = TrueTypeFontData.parse(registeredFont.name(), registeredFont.data());
                for (PdfPage page : pages) {
                    for (PdfPage.TextOperation operation : page.operations()) {
                        if (requiresUnicode(operation.text()) && font.supports(codePoints(operation.text()))) {
                            return font;
                        }
                    }
                }
            } catch (IllegalArgumentException ignored) {
                // Try the next registered font.
            }
        }
        return null;
    }

    private static SortedSet<Integer> codePoints(String text) {
        SortedSet<Integer> codePoints = new TreeSet<>();
        text.codePoints().forEach(codePoints::add);
        return codePoints;
    }

    private static boolean requiresUnicode(String text) {
        return text.codePoints().anyMatch(codePoint -> codePoint > 255);
    }

    private static String unicodeHex(String text) {
        StringBuilder result = new StringBuilder(text.length() * 4);
        for (int index = 0; index < text.length(); index++) {
            result.append(String.format(Locale.ROOT, "%04X", (int) text.charAt(index)));
        }
        return result.toString();
    }

    private static byte[] escapeText(String text) {
        byte[] encoded = text.getBytes(PDF_TEXT_ENCODING);
        ByteArrayOutputStream escaped = new ByteArrayOutputStream(encoded.length);
        for (byte value : encoded) {
            int unsigned = Byte.toUnsignedInt(value);
            if (unsigned == '(' || unsigned == ')' || unsigned == '\\') {
                escaped.write('\\');
                escaped.write(unsigned);
            } else if (unsigned == '\r') {
                write(escaped, ascii("\\r"));
            } else if (unsigned == '\n') {
                write(escaped, ascii("\\n"));
            } else {
                escaped.write(unsigned);
            }
        }
        return escaped.toByteArray();
    }

    private static byte[] streamObject(byte[] stream) {
        return streamObject(stream, "");
    }

    private static byte[] streamObject(byte[] stream, String additionalEntries) {
        ByteArrayOutputStream object = new ByteArrayOutputStream();
        write(object, "<< /Length " + stream.length
                + (additionalEntries.isEmpty() ? "" : " " + additionalEntries)
                + " >>\nstream\n");
        write(object, stream);
        write(object, "\nendstream");
        return object.toByteArray();
    }

    private static String number(float value) {
        if (value == Math.rint(value)) {
            return Long.toString((long) value);
        }
        String text = String.format(Locale.ROOT, "%.4f", value);
        return text.replaceFirst("0+$", "").replaceFirst("\\.$", "");
    }

    private static byte[] ascii(String value) {
        return value.getBytes(StandardCharsets.ISO_8859_1);
    }

    private static void write(ByteArrayOutputStream output, String value) {
        write(output, ascii(value));
    }

    private static void write(ByteArrayOutputStream output, byte[] value) {
        output.write(value, 0, value.length);
    }
}