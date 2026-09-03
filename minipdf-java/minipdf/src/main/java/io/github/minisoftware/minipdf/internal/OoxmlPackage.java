package io.github.minisoftware.minipdf.internal;

import io.github.minisoftware.minipdf.MiniPdfException;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.zip.ZipEntry;
import java.util.zip.ZipException;
import java.util.zip.ZipInputStream;

public final class OoxmlPackage {
    private static final int MAX_ENTRIES = 10_000;
    private static final long MAX_ENTRY_SIZE = 128L * 1024L * 1024L;
    private static final long MAX_TOTAL_SIZE = 512L * 1024L * 1024L;
    private static final long MAX_EXPANSION_RATIO = 200L;

    private final Map<String, byte[]> entries;

    private OoxmlPackage(Map<String, byte[]> entries) {
        this.entries = Collections.unmodifiableMap(entries);
    }

    public static OoxmlPackage open(byte[] input) throws MiniPdfException {
        if (!hasZipSignature(input)) {
            throw invalidPackage("input is not a ZIP package");
        }

        Map<String, byte[]> entries = new LinkedHashMap<>();
        long totalSize = 0;
        try (ZipInputStream archive = new ZipInputStream(new ByteArrayInputStream(input))) {
            ZipEntry entry;
            while ((entry = archive.getNextEntry()) != null) {
                if (entries.size() >= MAX_ENTRIES) {
                    throw invalidPackage("ZIP package contains too many entries");
                }
                String name = normalizeEntryName(entry.getName());
                if (entry.isDirectory()) {
                    continue;
                }
                if (entries.containsKey(name)) {
                    throw invalidPackage("ZIP package contains duplicate entry: " + name);
                }

                ByteArrayOutputStream content = new ByteArrayOutputStream();
                byte[] buffer = new byte[8192];
                int read;
                while ((read = archive.read(buffer)) != -1) {
                    if ((long) content.size() + read > MAX_ENTRY_SIZE) {
                        throw invalidPackage("ZIP entry expands beyond the configured limit: " + name);
                    }
                    content.write(buffer, 0, read);
                }
                byte[] bytes = content.toByteArray();
                totalSize += bytes.length;
                if (totalSize > MAX_TOTAL_SIZE
                        || (input.length > 0 && totalSize / input.length > MAX_EXPANSION_RATIO)) {
                    throw invalidPackage("ZIP package expands beyond the configured limit");
                }
                entries.put(name, bytes);
            }
        } catch (ZipException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.ZIP_PACKAGE,
                    "invalid ZIP package: " + exception.getMessage(),
                    exception);
        } catch (IOException exception) {
            throw new MiniPdfException(
                    MiniPdfException.Kind.IO,
                    "I/O error while reading ZIP package: " + exception.getMessage(),
                    exception);
        }
        return new OoxmlPackage(entries);
    }

    public Set<String> entryNames() {
        return entries.keySet();
    }

    public Optional<byte[]> entry(String name) {
        byte[] bytes = entries.get(name);
        return bytes == null ? Optional.empty() : Optional.of(bytes.clone());
    }

    public Optional<String> text(String name) {
        return entry(name).map(bytes -> new String(bytes, StandardCharsets.UTF_8));
    }

    private static String normalizeEntryName(String entryName) throws MiniPdfException {
        String normalized = entryName.replace('\\', '/');
        if (normalized.startsWith("/") || normalized.contains(":") || normalized.indexOf('\0') >= 0) {
            throw invalidPackage("ZIP package contains an unsafe entry path: " + entryName);
        }
        for (String segment : normalized.split("/")) {
            if (segment.equals("..")) {
                throw invalidPackage("ZIP package contains an unsafe entry path: " + entryName);
            }
        }
        return normalized;
    }

    private static boolean hasZipSignature(byte[] input) {
        if (input == null || input.length < 4 || input[0] != 'P' || input[1] != 'K') {
            return false;
        }
        return (input[2] == 3 && input[3] == 4)
                || (input[2] == 5 && input[3] == 6)
                || (input[2] == 7 && input[3] == 8);
    }

    private static MiniPdfException invalidPackage(String message) {
        return new MiniPdfException(MiniPdfException.Kind.ZIP_PACKAGE, message);
    }
}