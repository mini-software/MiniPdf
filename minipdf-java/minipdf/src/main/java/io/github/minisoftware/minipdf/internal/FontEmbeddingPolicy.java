package io.github.minisoftware.minipdf.internal;

public final class FontEmbeddingPolicy {
    private FontEmbeddingPolicy() {
    }

    public static boolean shouldSubset() {
        return shouldSubset(System.getProperty("java.specification.version"));
    }

    static boolean shouldSubset(String javaSpecificationVersion) {
        return !"10".equals(javaSpecificationVersion);
    }
}