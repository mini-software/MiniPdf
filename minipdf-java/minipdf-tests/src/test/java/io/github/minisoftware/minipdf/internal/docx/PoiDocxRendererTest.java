package io.github.minisoftware.minipdf.internal.docx;

import org.apache.pdfbox.pdmodel.font.PDType1Font;
import org.apache.pdfbox.pdmodel.font.PDFont;
import org.apache.pdfbox.pdmodel.font.Standard14Fonts;
import org.apache.poi.xwpf.usermodel.LineSpacingRule;
import org.apache.poi.xwpf.usermodel.XWPFDocument;
import org.apache.poi.xwpf.usermodel.XWPFParagraph;
import org.apache.poi.xwpf.usermodel.XWPFRun;
import org.apache.poi.xwpf.usermodel.XWPFTableCell;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTPPr;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTBorder;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.CTP;
import org.openxmlformats.schemas.wordprocessingml.x2006.main.STBorder;
import org.junit.jupiter.api.Test;

import java.util.Arrays;
import java.util.List;
import java.util.stream.Collectors;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertSame;

class PoiDocxRendererTest {
    @Test
    void preservesExplicitlyBorderlessTableEdges() {
        CTBorder noBorder = CTBorder.Factory.newInstance();
        noBorder.setVal(STBorder.NONE);

        assertEquals("none", PoiDocxRenderer.resolveBorderStyle(null, noBorder));
        assertEquals("none", PoiDocxRenderer.resolveBorderStyle(noBorder, null));
        assertNull(PoiDocxRenderer.resolveBorderStyle(null, null));
    }

    @Test
    void carriesRunHighlightIntoParagraphSegments() throws Exception {
        PDFont font = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
        PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
                font, font, font, font, font, font, font, "Times New Roman", "SimSun");
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            XWPFRun run = paragraph.createRun();
            run.setTextHighlightColor("yellow");
            run.setText("highlighted");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 12.0f);

            assertNotNull(segments.get(0).highlight());
        }
    }

    @Test
    void rendersFootnoteReferencesAsNumbersWithoutInliningFootnoteText() throws Exception {
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFRun run = document.createParagraph().createRun();
            run.getCTR().addNewFootnoteReference().setId(java.math.BigInteger.ONE);

            assertEquals("1", PoiDocxRenderer.runText(run));
        }
    }

    @Test
    void mapsWingdingsCheckSymbolToUnicode() throws Exception {
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFRun run = document.createParagraph().createRun();
            org.openxmlformats.schemas.wordprocessingml.x2006.main.CTSym symbol =
                    run.getCTR().addNewSym();
            symbol.setFont("Wingdings");
            symbol.setChar(new byte[] {(byte) 0xf0, (byte) 0xfc});

            assertEquals("\u2713", PoiDocxRenderer.runText(run));
        }
    }

    @Test
    void readsAnchoredCheckboxGeometryWithoutVmlFallbackDuplication() throws Exception {
        String xml = "<w:p xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\" "
                + "xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\" "
                + "xmlns:wps=\"http://schemas.microsoft.com/office/word/2010/wordprocessingShape\">"
                + "<w:r><w:drawing><wp:anchor><wp:positionH relativeFrom=\"column\">"
                + "<wp:posOffset>3314065</wp:posOffset></wp:positionH>"
                + "<wp:positionV relativeFrom=\"paragraph\"><wp:posOffset>208915</wp:posOffset>"
                + "</wp:positionV>"
                + "<wp:extent cx=\"257175\" cy=\"228600\"/><wps:txbx><w:txbxContent>"
                + "<w:p><w:r><w:sym w:font=\"Wingdings\" w:char=\"F0FC\"/></w:r></w:p>"
                + "</w:txbxContent></wps:txbx></wp:anchor></w:drawing></w:r></w:p>";
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = new XWPFParagraph(CTP.Factory.parse(xml), document);

            List<PoiDocxRenderer.FloatingCheckbox> checkboxes =
                    PoiDocxRenderer.floatingCheckboxes(paragraph);

            assertEquals(1, checkboxes.size());
            assertEquals(260.95f, checkboxes.get(0).offsetX(), 0.01f);
            assertEquals(16.45f, checkboxes.get(0).offsetY(), 0.01f);
            assertEquals(true, checkboxes.get(0).checked());
        }
    }

    @Test
    void preservesTabsAndAdvancesToDefaultWordTabStops() throws Exception {
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            paragraph.createRun().addTab();
            paragraph.createRun().addTab();

            assertEquals("\t\t", PoiDocxRenderer.paragraphText(paragraph));
            assertEquals(36.0f, PoiDocxRenderer.nextDefaultTabStop(20.0f));
            assertEquals(72.0f, PoiDocxRenderer.nextDefaultTabStop(36.0f));
        }
    }

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
        PDFont font = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        float fontSize = 10.5f;
        float width = font.getStringWidth("88888888<br />") / 1000.0f * fontSize + 0.1f;

        assertEquals(
                Arrays.asList("020-", "88888888<br />", "13888888888"),
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
        PDFont fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        PDFont simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
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
            PDFont fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
            PDFont simHei = new PDType1Font(Standard14Fonts.FontName.COURIER);
        PDFont times = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
            PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                fallback,
                simHei,
                fallback,
                fallback,
                times,
                fallback,
                "Times New Roman",
                "SimSun");
            try (XWPFDocument document = new XWPFDocument()) {
                XWPFTableCell cell = document.createTable(1, 1).getRow(0).getCell(0);
                XWPFRun run = cell.getParagraphs().get(0).createRun();
                run.setFontFamily("SimHei", org.apache.poi.xwpf.usermodel.XWPFRun.FontCharRange.eastAsia);
                run.setText("中");

                assertSame(simHei, PoiDocxRenderer.resolvedCellFont(cell, fonts, fallback));

                run.setBold(true);
                assertSame(fallback, PoiDocxRenderer.resolvedCellFont(cell, fonts, fallback));
            }
        }

    @Test
    void splitsInheritedMixedScriptRunByDocumentFontSlots() throws Exception {
        PDFont fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        PDFont simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        PDFont times = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
        PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                simSun,
                fallback,
                fallback,
                fallback,
                times,
                fallback,
                "Times New Roman",
                "SimSun");
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            paragraph.createRun().setText("A中B");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 10.0f);

                assertEquals(Arrays.asList("A", "中", "B"), segments.stream()
                    .map(PoiDocxRenderer.RunSegment::text)
                    .collect(Collectors.toList()));
            assertSame(times, segments.get(0).font());
            assertSame(simSun, segments.get(1).font());
            assertSame(times, segments.get(2).font());
            assertEquals(0.0f, segments.get(0).leadingSpacing());
            assertEquals(2.5f, segments.get(1).leadingSpacing());
            assertEquals(2.5f, segments.get(2).leadingSpacing());
        }
    }

    @Test
    void preservesBoldFontInParagraphSegments() throws Exception {
        PDFont regular = new PDType1Font(Standard14Fonts.FontName.TIMES_ROMAN);
        PDFont bold = new PDType1Font(Standard14Fonts.FontName.TIMES_BOLD);
        PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
                regular,
                regular,
                regular,
                regular,
                regular,
                regular,
                regular,
                "Times New Roman",
                "Times New Roman");
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            XWPFRun run = paragraph.createRun();
            run.setBold(true);
            run.setText("Heading");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 12.0f, bold);

            assertSame(bold, segments.get(0).font());
        }
    }

    @Test
    void distributesJustifiedLineWidthAcrossSpaces() {
        assertEquals(10.0f, PoiDocxRenderer.justifiedWordSpacing("one two three", 80.0f, 100.0f));
        assertEquals(0.0f, PoiDocxRenderer.justifiedWordSpacing("word", 80.0f, 100.0f));
        assertEquals(0.0f, PoiDocxRenderer.justifiedWordSpacing("one two", 110.0f, 100.0f));
    }

    @Test
    void honorsExplicitAutoLineSpacingForLatinParagraphs() throws Exception {
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            paragraph.setSpacingBetween(1.15, LineSpacingRule.AUTO);

            assertEquals(
                    15.8838f,
                    PoiDocxRenderer.paragraphLineHeight(paragraph, 12.0f, 14.4f, 18.0f, true),
                    0.001f);
            assertEquals(
                    18.0f,
                    PoiDocxRenderer.paragraphLineHeight(paragraph, 12.0f, 14.4f, 18.0f, false),
                    0.001f);
        }
    }

    @Test
    void normalizesVietnameseCombiningMarksBeforeLayout() {
        assertEquals(
                "M\u1ee5c \u0111\u00edch",
                PoiDocxRenderer.renderableText("Mu\u0323c \u0111i\u0301ch"));
    }

    @Test
    void positionsPageNumberAboveFooterBoundaryUsingFontMetrics() {
        assertEquals(53.7719f, PoiDocxRenderer.pageNumberBaseline(36.0f, 13.0f, -216.0f), 0.001f);
    }

    @Test
    void suppressesMixedScriptSpacingWhenParagraphDisablesIt() throws Exception {
        PDFont fallback = new PDType1Font(Standard14Fonts.FontName.HELVETICA);
        PDFont simSun = new PDType1Font(Standard14Fonts.FontName.COURIER);
        PoiDocxRenderer.ParagraphFonts fonts = new PoiDocxRenderer.ParagraphFonts(
                fallback,
                simSun,
                fallback,
                fallback,
                fallback,
                fallback,
                fallback,
                "Times New Roman",
                "SimSun");
        try (XWPFDocument document = new XWPFDocument()) {
            XWPFParagraph paragraph = document.createParagraph();
            CTPPr properties = paragraph.getCTP().addNewPPr();
            properties.addNewAutoSpaceDE().setVal(false);
            properties.addNewAutoSpaceDN().setVal(false);
            paragraph.createRun().setText("A中1");

            List<PoiDocxRenderer.RunSegment> segments =
                    PoiDocxRenderer.paragraphSegments(paragraph, fonts, 10.0f);

                assertEquals(Arrays.asList(0.0f, 0.0f, 0.0f), segments.stream()
                    .map(PoiDocxRenderer.RunSegment::leadingSpacing)
                    .collect(Collectors.toList()));
        }
    }
}