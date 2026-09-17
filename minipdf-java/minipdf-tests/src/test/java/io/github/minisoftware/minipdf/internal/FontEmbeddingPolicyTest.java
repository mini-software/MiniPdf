package io.github.minisoftware.minipdf.internal;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

class FontEmbeddingPolicyTest {
    @Test
    void disablesSubsettingOnlyForJdk10() {
        assertTrue(FontEmbeddingPolicy.shouldSubset("1.8"));
        assertTrue(FontEmbeddingPolicy.shouldSubset("9"));
        assertFalse(FontEmbeddingPolicy.shouldSubset("10"));
        assertTrue(FontEmbeddingPolicy.shouldSubset("11"));
        assertTrue(FontEmbeddingPolicy.shouldSubset("25"));
    }
}