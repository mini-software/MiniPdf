package io.github.minisoftware.minipdf;

import java.util.Optional;

public final class ConversionOptions {
    private static final ConversionOptions DEFAULTS = new ConversionOptions(null);

    private final PageSize pageSize;

    private ConversionOptions(PageSize pageSize) {
        this.pageSize = pageSize;
    }

    public static ConversionOptions defaults() {
        return DEFAULTS;
    }

    public static ConversionOptions withPageSize(PageSize pageSize) {
        return new ConversionOptions(java.util.Objects.requireNonNull(pageSize, "pageSize"));
    }

    public Optional<PageSize> pageSize() {
        return Optional.ofNullable(pageSize);
    }
}