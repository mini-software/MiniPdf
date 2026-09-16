package io.github.minisoftware.minipdf;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;

public final class PdfPage {
    private final float width;
    private final float height;
    private final List<TextOperation> operations = new ArrayList<>();

    PdfPage(float width, float height) {
        this.width = width;
        this.height = height;
    }

    public float width() {
        return width;
    }

    public float height() {
        return height;
    }

    public void addText(String text, float x, float y, float size, PdfColor color, boolean bold) {
        Objects.requireNonNull(text, "text");
        Objects.requireNonNull(color, "color");
        if (!Float.isFinite(x) || !Float.isFinite(y)) {
            throw new IllegalArgumentException("text coordinates must be finite");
        }
        if (!Float.isFinite(size) || size <= 0.0f) {
            throw new IllegalArgumentException("text size must be a positive finite value");
        }
        operations.add(new TextOperation(text, x, y, size, color, bold));
    }

    List<TextOperation> operations() {
        return Collections.unmodifiableList(new ArrayList<>(operations));
    }

    static final class TextOperation {
        private final String text;
        private final float x;
        private final float y;
        private final float size;
        private final PdfColor color;
        private final boolean bold;

        TextOperation(String text, float x, float y, float size, PdfColor color, boolean bold) {
            this.text = text;
            this.x = x;
            this.y = y;
            this.size = size;
            this.color = color;
            this.bold = bold;
        }

        String text() {
            return text;
        }

        float x() {
            return x;
        }

        float y() {
            return y;
        }

        float size() {
            return size;
        }

        PdfColor color() {
            return color;
        }

        boolean bold() {
            return bold;
        }
    }
}