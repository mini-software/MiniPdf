package io.github.minisoftware.minipdf;

import java.util.Objects;

public final class PdfColor {
    public static final PdfColor BLACK = new PdfColor(0.0f, 0.0f, 0.0f);
    public static final PdfColor WHITE = new PdfColor(1.0f, 1.0f, 1.0f);

    private final float red;
    private final float green;
    private final float blue;

    public PdfColor(float red, float green, float blue) {
        validate(red, "red");
        validate(green, "green");
        validate(blue, "blue");
        this.red = red;
        this.green = green;
        this.blue = blue;
    }

    public float red() {
        return red;
    }

    public float green() {
        return green;
    }

    public float blue() {
        return blue;
    }

    private static void validate(float component, String name) {
        if (!Float.isFinite(component) || component < 0.0f || component > 1.0f) {
            throw new IllegalArgumentException(name + " must be a finite value from 0 to 1");
        }
    }

    @Override
    public boolean equals(Object value) {
        if (this == value) {
            return true;
        }
        if (!(value instanceof PdfColor)) {
            return false;
        }
        PdfColor other = (PdfColor) value;
        return Float.compare(red, other.red) == 0
                && Float.compare(green, other.green) == 0
                && Float.compare(blue, other.blue) == 0;
    }

    @Override
    public int hashCode() {
        return Objects.hash(red, green, blue);
    }

    @Override
    public String toString() {
        return "PdfColor[red=" + red + ", green=" + green + ", blue=" + blue + ']';
    }
}