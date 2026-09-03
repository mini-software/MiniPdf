package io.github.minisoftware.minipdf;

import java.util.Objects;

public final class MiniPdfException extends Exception {
    public enum Kind {
        IO,
        ZIP_PACKAGE,
        XML_PARSE,
        UNSUPPORTED_FORMAT,
        INVALID_INPUT
    }

    private final Kind kind;

    public MiniPdfException(Kind kind, String message) {
        super(message);
        this.kind = Objects.requireNonNull(kind, "kind");
    }

    public MiniPdfException(Kind kind, String message, Throwable cause) {
        super(message, cause);
        this.kind = Objects.requireNonNull(kind, "kind");
    }

    public Kind kind() {
        return kind;
    }
}