package io.github.minisoftware.minipdf;

import java.io.ByteArrayOutputStream;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

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
        return List.copyOf(pages);
    }

    public byte[] toBytes() {
        int objectCount = 4 + pages.size() * 2;
        List<byte[]> objects = new ArrayList<>(objectCount);
        objects.add(ascii("<< /Type /Catalog /Pages 2 0 R >>"));
        objects.add(ascii(pagesObject()));
        objects.add(ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
        objects.add(ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"));

        for (int index = 0; index < pages.size(); index++) {
            PdfPage page = pages.get(index);
            int contentObjectNumber = 6 + index * 2;
            objects.add(ascii(pageObject(page, contentObjectNumber)));
            objects.add(streamObject(pageContent(page)));
        }

        ByteArrayOutputStream output = new ByteArrayOutputStream();
        write(output, "%PDF-1.4\n");
        output.writeBytes(new byte[]{'%', (byte) 0xE2, (byte) 0xE3, (byte) 0xCF, (byte) 0xD3, '\n'});

        List<Integer> offsets = new ArrayList<>(objectCount);
        for (int index = 0; index < objects.size(); index++) {
            offsets.add(output.size());
            write(output, (index + 1) + " 0 obj\n");
            output.writeBytes(objects.get(index));
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

    private String pagesObject() {
        StringBuilder kids = new StringBuilder();
        for (int index = 0; index < pages.size(); index++) {
            kids.append(5 + index * 2).append(" 0 R ");
        }
        return "<< /Type /Pages /Kids [ " + kids + "] /Count " + pages.size() + " >>";
    }

    private static String pageObject(PdfPage page, int contentObjectNumber) {
        return "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 "
                + number(page.width()) + ' ' + number(page.height())
                + "] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents "
                + contentObjectNumber + " 0 R >>";
    }

    private static byte[] pageContent(PdfPage page) {
        ByteArrayOutputStream content = new ByteArrayOutputStream();
        for (PdfPage.TextOperation operation : page.operations()) {
            write(content, "BT\n");
            write(content, (operation.bold() ? "/F2 " : "/F1 ") + number(operation.size()) + " Tf\n");
            write(content, number(operation.color().red()) + ' '
                    + number(operation.color().green()) + ' '
                    + number(operation.color().blue()) + " rg\n");
            write(content, "1 0 0 1 " + number(operation.x()) + ' ' + number(operation.y()) + " Tm\n(");
            content.writeBytes(escapeText(operation.text()));
            write(content, ") Tj\nET\n");
        }
        return content.toByteArray();
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
                escaped.writeBytes(ascii("\\r"));
            } else if (unsigned == '\n') {
                escaped.writeBytes(ascii("\\n"));
            } else {
                escaped.write(unsigned);
            }
        }
        return escaped.toByteArray();
    }

    private static byte[] streamObject(byte[] stream) {
        ByteArrayOutputStream object = new ByteArrayOutputStream();
        write(object, "<< /Length " + stream.length + " >>\nstream\n");
        object.writeBytes(stream);
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
        output.writeBytes(ascii(value));
    }
}