package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.OfficeFormat;

import java.util.Locale;

public final class OfficePackageDetector {
    private OfficePackageDetector() {
    }

    public static OfficeFormat detect(byte[] input) throws MiniPdfException {
        OoxmlPackage archive = OoxmlPackage.open(input);
        for (String entryName : archive.entryNames()) {
            String name = entryName.toLowerCase(Locale.ROOT);
            if (name.startsWith("word/")) {
                return OfficeFormat.DOCX;
            }
            if (name.startsWith("xl/")) {
                return OfficeFormat.XLSX;
            }
            if (name.startsWith("ppt/")) {
                return OfficeFormat.PPTX;
            }
        }
        return OfficeFormat.UNKNOWN;
    }
}