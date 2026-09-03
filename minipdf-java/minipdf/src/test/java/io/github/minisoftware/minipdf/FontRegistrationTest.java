package io.github.minisoftware.minipdf;

import org.junit.jupiter.api.Test;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.assertArrayEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

class FontRegistrationTest {
    @Test
    void registrationDefensivelyCopiesFontData() {
        byte[] data = {1, 2, 3};
        String name = "test-" + UUID.randomUUID();

        MiniPdf.registerFont(name, data);
        data[0] = 9;

        RegisteredFont registered = MiniPdf.registeredFonts().stream()
                .filter(font -> font.name().equals(name))
                .findFirst()
                .orElseThrow();
        byte[] returned = registered.data();
        returned[1] = 9;

        assertArrayEquals(new byte[]{1, 2, 3}, registered.data());
    }

    @Test
    void rejectsBlankFontNames() {
        assertThrows(IllegalArgumentException.class, () -> MiniPdf.registerFont(" ", new byte[]{1}));
    }
}