package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;

class OfficeFormatDetectionTest {
    @Test
    void detectsOfficeFormatsFromPackageEntries() throws Exception {
        assertSame(OfficeFormat.DOCX, MiniPdf.detectOfficeFormat(packageWith("word/document.xml")));
        assertSame(OfficeFormat.XLSX, MiniPdf.detectOfficeFormat(packageWith("xl/workbook.xml")));
        assertSame(OfficeFormat.PPTX, MiniPdf.detectOfficeFormat(packageWith("ppt/presentation.xml")));
    }

    @Test
    void returnsUnknownForOtherZipPackages() throws Exception {
        assertSame(OfficeFormat.UNKNOWN, MiniPdf.detectOfficeFormat(packageWith("custom/data.xml")));
    }

    @Test
    void normalizesBackslashesAndCase() throws Exception {
        assertSame(OfficeFormat.DOCX, MiniPdf.detectOfficeFormat(packageWith("WORD\\document.xml")));
    }

    @Test
    void rejectsNonZipInput() {
        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.detectOfficeFormat("not a zip".getBytes()));

        assertSame(MiniPdfException.Kind.ZIP_PACKAGE, exception.kind());
        assertEquals("input is not a ZIP package", exception.getMessage());
    }

    @Test
    void unsupportedPptxReportsTheFormatBoundary() throws Exception {
        MiniPdfException exception = assertThrows(
                MiniPdfException.class,
                () -> MiniPdf.convertBytesToPdf(packageWith("ppt/presentation.xml")));

        assertSame(MiniPdfException.Kind.UNSUPPORTED_FORMAT, exception.kind());
        assertEquals("unsupported or unknown Office document format", exception.getMessage());
    }

    private static byte[] packageWith(String entryName) throws IOException {
        ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        try (ZipOutputStream archive = new ZipOutputStream(bytes)) {
            archive.putNextEntry(new ZipEntry(entryName));
            archive.write("<root/>".getBytes());
            archive.closeEntry();
        }
        return bytes.toByteArray();
    }
}