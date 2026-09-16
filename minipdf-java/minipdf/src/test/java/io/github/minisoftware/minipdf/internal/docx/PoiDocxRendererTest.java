package io.github.minisoftware.minipdf.internal.docx;

import org.apache.pdfbox.pdmodel.font.PDType1Font;
import org.apache.pdfbox.pdmodel.font.Standard14Fonts;
import org.apache.poi.xwpf.usermodel.XWPFDocument;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertSame;

class PoiDocxRendererTest {
    @Test
    void centersTextWithinAsymmetricallyIndentedLineBox() {
        assertEquals(250.0f, PoiDocxRenderer.centeredTextX(500.0f, 0.0f, -100.0f, 100.0f));
        assertEquals(200.0f, PoiDocxRenderer.centeredTextX(500.0f, 0.0f, 0.0f, 100.0f));
    }

    @Test
    void preservesNegativeParagraphIndentsButNormalizesUnsetValue() {
        assertEquals(-52.6f, PoiDocxRenderer.indentationToPoints(-1052));
        assertEquals(0.0f, PoiDocxRenderer.indentationToPoints(-1));
    }

    @Test
    void reservesOneCellInsetFromWrappingWidth() {
        assertEquals(78.4f, PoiDocxRenderer.cellContentWidth(83.8f, 5.4f));
    }

    @Test
    void wrapsLatinTextAtHyphensAndSpacesBeforeSplittingCharacters() throws Exception {
        var font = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        float fontSize = 10.5f;
        float width = font.getStringWidth("88888888<br />") / 1000.0f * fontSize + 0.1f;

        assertEquals(
                List.of("020-", "88888888<br />", "13888888888"),
                PoiDocxRenderer.wrap(font, "020-88888888<br /> 13888888888", fontSize, width));
    }

    @Test
    void countsEastAsianLatinAndDigitBoundaries() {
        assertEquals(9, PoiDocxRenderer.eastAsianBoundaryCount("2025年11月26日14时40分"));
        assertEquals(2, PoiDocxRenderer.eastAsianBoundaryCount("使用PECVD设备"));
        assertEquals(0, PoiDocxRenderer.eastAsianBoundaryCount("纯中文"));
        assertEquals(0, PoiDocxRenderer.eastAsianBoundaryCount("ASCII only"));
    }

    @Test
    void wrapsEastAsianParagraphsWithDocumentEastAsianFont() {
        var fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        var simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        var fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                simSun,
                fallback,
                fallback,
                fallback,
                fallback,
                fallback,
                "Times New Roman",
                "SimSun");

        assertSame(simSun, PoiDocxRenderer.wrappingFont(fonts, "使用PECVD设备"));
        assertSame(fallback, PoiDocxRenderer.wrappingFont(fonts, "ASCII only"));
    }

        @Test
            void resolvesFontAndBoldFromHomogeneousCellRuns() throws Exception {
            var fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
            var simHei = new PDType1Font(Standard14Fonts.FontName.COURIER);
        var times = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
            var fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                fallback,
                simHei,
                fallback,
                fallback,
                times,
                fallback,
                "Times New Roman",
                "SimSun");
            try (var document = new XWPFDocument()) {
                var cell = document.createTable(1, 1).getRow(0).getCell(0);
                var run = cell.getParagraphs().get(0).createRun();
                run.setFontFamily("SimHei", org.apache.poi.xwpf.usermodel.XWPFRun.FontCharRange.eastAsia);
                run.setText("中");

                assertSame(simHei, PoiDocxRenderer.resolvedCellFont(cell, fonts, fallback));

                run.setBold(true);
                assertSame(fallback, PoiDocxRenderer.resolvedCellFont(cell, fonts, fallback));
            }
        }

    @Test
    void splitsInheritedMixedScriptRunByDocumentFontSlots() throws Exception {
        var fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        var simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        var times = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
        var fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                simSun,
                fallback,
                fallback,
                fallback,
                times,
                fallback,
                "Times New Roman",
                "SimSun");
        try (var document = new XWPFDocument()) {
            var paragraph = document.createParagraph();
            paragraph.createRun().setText("A中B");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 10.0f);

            assertEquals(List.of("A", "中", "B"), segments.stream()
                    .map(PoiDocxRenderer.RunSegment::text)
                    .toList());
            assertSame(times, segments.get(0).font());
            assertSame(simSun, segments.get(1).font());
            assertSame(times, segments.get(2).font());
            assertEquals(0.0f, segments.get(0).leadingSpacing());
            assertEquals(2.5f, segments.get(1).leadingSpacing());
            assertEquals(2.5f, segments.get(2).leadingSpacing());
        }
    }

    @Test
    void suppressesMixedScriptSpacingWhenParagraphDisablesIt() throws Exception {
        var fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        var simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        var fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                simSun,
                fallback,
                fallback,
                fallback,
                fallback,
                fallback,
                "Times New Roman",
                "SimSun");
        try (var document = new XWPFDocument()) {
            var paragraph = document.createParagraph();
            var properties = paragraph.getCTP().addNewPPr();
            properties.addNewAutoSpaceDE().setVal(false);
            properties.addNewAutoSpaceDN().setVal(false);
            paragraph.createRun().setText("A中1");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 10.0f);

            assertEquals(List.of(0.0f, 0.0f, 0.0f), segments.stream()
                    .map(PoiDocxRenderer.RunSegment::leadingSpacing)
                    .toList());
        }
    }
}