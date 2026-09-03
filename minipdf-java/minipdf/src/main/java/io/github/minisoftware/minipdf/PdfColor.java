package io.github.minisoftware.minipdf;

public record PdfColor(float red, float green, float blue) {
    public static final PdfColor BLACK = new PdfColor(0.0f, 0.0f, 0.0f);
    public static final PdfColor WHITE = new PdfColor(1.0f, 1.0f, 1.0f);

    public PdfColor {
        validate(red, "red");
        validate(green, "green");
        validate(blue, "blue");
    }

    private static void validate(float component, String name) {
        if (!Float.isFinite(component) || component < 0.0f || component > 1.0f) {
            throw new IllegalArgumentException(name + " must be a finite value from 0 to 1");
        }
    }
}