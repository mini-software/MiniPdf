package io.github.minisoftware.minipdf;

import java.util.ArrayList;
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
        return List.copyOf(operations);
    }

    record TextOperation(String text, float x, float y, float size, PdfColor color, boolean bold) {
    }
}