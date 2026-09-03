package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class PageSizeTest {
    @Test
    void exposesStandardPageSizes() {
        assertEquals(595.28f, PageSize.A4.width());
        assertEquals(841.89f, PageSize.A4.height());
        assertEquals(612.0f, PageSize.LETTER.width());
        assertEquals(792.0f, PageSize.LETTER.height());
    }

    @Test
    void rejectsInvalidCustomDimensions() {
        for (float invalid : new float[]{0.0f, -1.0f, Float.NaN, Float.POSITIVE_INFINITY}) {
            MiniPdfException exception = assertThrows(
                    MiniPdfException.class,
                    () -> PageSize.of(invalid, 100.0f));
            assertSame(MiniPdfException.Kind.INVALID_INPUT, exception.kind());
        }
    }

    @Test
    void optionsExposeAnOptionalOverride() throws MiniPdfException {
        assertTrue(ConversionOptions.defaults().pageSize().isEmpty());
        PageSize custom = PageSize.of(400.0f, 500.0f);
        assertEquals(custom, ConversionOptions.withPageSize(custom).pageSize().orElseThrow());
    }
}