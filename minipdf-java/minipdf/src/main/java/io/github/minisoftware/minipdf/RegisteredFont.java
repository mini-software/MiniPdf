package io.github.minisoftware.minipdf;

import java.util.Arrays;
import java.util.Objects;

public final class RegisteredFont {
    private final String name;
    private final byte[] data;

    RegisteredFont(String name, byte[] data) {
        this.name = Objects.requireNonNull(name, "name");
        this.data = Objects.requireNonNull(data, "data").clone();
    }

    public String name() {
        return name;
    }

    public byte[] data() {
        return data.clone();
    }

    @Override
    public boolean equals(Object value) {
        if (this == value) {
            return true;
        }
        if (!(value instanceof RegisteredFont other)) {
            return false;
        }
        return name.equals(other.name) && Arrays.equals(data, other.data);
    }

    @Override
    public int hashCode() {
        return 31 * name.hashCode() + Arrays.hashCode(data);
    }
}