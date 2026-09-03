package io.github.minisoftware.minipdf;

import java.util.Objects;

public final class PageSize {
    public static final PageSize A4 = new PageSize(595.28f, 841.89f);
    public static final PageSize LETTER = new PageSize(612.0f, 792.0f);

    private final float width;
    private final float height;

    private PageSize(float width, float height) {
        this.width = width;
        this.height = height;
    }

    public static PageSize of(float width, float height) throws MiniPdfException {
        if (!Float.isFinite(width) || !Float.isFinite(height) || width <= 0.0f || height <= 0.0f) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.INVALID_INPUT,
                    "page width and height must be positive finite values");
        }
        return new PageSize(width, height);
    }

    public float width() {
        return width;
    }

    public float height() {
        return height;
    }

    @Override
    public boolean equals(Object value) {
        if (this == value) {
            return true;
        }
        if (!(value instanceof PageSize other)) {
            return false;
        }
        return Float.compare(width, other.width) == 0 && Float.compare(height, other.height) == 0;
    }

    @Override
    public int hashCode() {
        return Objects.hash(width, height);
    }

    @Override
    public String toString() {
        return "PageSize[width=" + width + ", height=" + height + ']';
    }
}