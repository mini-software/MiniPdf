package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.nio.charset.StandardCharsets;
import java.util.Map;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;

class OoxmlSecurityTest {
    @Test
    void rejectsTraversalEntryNames() throws Exception {
        byte[] archive = packageWith(Map.of("../word/document.xml", "<document/>"));

        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.detectOfficeFormat(archive));

        assertSame(MiniPdfException.Kind.ZIP_PACKAGE, exception.kind());
    }

    @Test
    void rejectsExternalEntities() throws Exception {
        String document = "<!DOCTYPE document [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]>"
                + "<w:document xmlns:w=\"urn:w\"><w:body><w:p><w:r><w:t>&xxe;</w:t></w:r>"
                + "</w:p></w:body></w:document>";
        byte[] docx = packageWith(Map.of("word/document.xml", document));

        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.convertBytesToPdf(docx));

        assertSame(MiniPdfException.Kind.XML_PARSE, exception.kind());
    }

    private static byte[] packageWith(Map<String, String> entries) throws Exception {
        ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        try (ZipOutputStream archive = new ZipOutputStream(bytes)) {
            for (Map.Entry<String, String> entry : entries.entrySet()) {
                archive.putNextEntry(new ZipEntry(entry.getKey()));
                archive.write(entry.getValue().getBytes(StandardCharsets.UTF_8));
                archive.closeEntry();
            }
        }
        return bytes.toByteArray();
    }
}