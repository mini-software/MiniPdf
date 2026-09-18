package io.github.minisoftware.minipdf;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

final class TrueTypeFontData {
    private final String pdfName;
    private final byte[] data;
    private final Map<Integer, Integer> cmap;
    private final int[] advances;
    private final int unitsPerEm;
    private final int ascent;
    private final int descent;
    private final int capHeight;
    private final int[] boundingBox;

    private TrueTypeFontData(
            String name,
            byte[] data,
            Map<Integer, Integer> cmap,
            int[] advances,
            int unitsPerEm,
            int ascent,
            int descent,
            int capHeight,
            int[] boundingBox) {
        this.pdfName = sanitizeName(name);
        this.data = data.clone();
        this.cmap = cmap;
        this.advances = advances;
        this.unitsPerEm = unitsPerEm;
        this.ascent = scale(ascent, unitsPerEm);
        this.descent = scale(descent, unitsPerEm);
        this.capHeight = scale(capHeight, unitsPerEm);
        this.boundingBox = new int[]{
                scale(boundingBox[0], unitsPerEm),
                scale(boundingBox[1], unitsPerEm),
                scale(boundingBox[2], unitsPerEm),
                scale(boundingBox[3], unitsPerEm)
        };
    }

    static TrueTypeFontData parse(String name, byte[] data) {
        if (isTrueTypeCollection(data)) {
            data = extractFirstFont(data);
        }
        if (data.length < 12) {
            throw new IllegalArgumentException("font is shorter than a TrueType header");
        }
        Map<String, Table> tables = readTables(data);
        Table os2 = tables.get("OS/2");
        if (os2 != null && os2.length >= 10) {
            int embeddingFlags = readU16(data, os2.offset + 8);
            if ((embeddingFlags & 0x0002) != 0 || (embeddingFlags & 0x0200) != 0) {
                throw new IllegalArgumentException("font license does not permit editable outline embedding");
            }
        }

        Table head = requiredTable(tables, "head");
        Table hhea = requiredTable(tables, "hhea");
        Table maxp = requiredTable(tables, "maxp");
        Table hmtx = requiredTable(tables, "hmtx");
        requireTableLength(head, 44);
        requireTableLength(hhea, 36);
        requireTableLength(maxp, 6);

        int unitsPerEm = readU16(data, head.offset + 18);
        int numberOfMetrics = readU16(data, hhea.offset + 34);
        int glyphCount = readU16(data, maxp.offset + 4);
        if (unitsPerEm <= 0 || numberOfMetrics <= 0 || glyphCount <= 0 || numberOfMetrics > glyphCount) {
            throw new IllegalArgumentException("font contains invalid horizontal metrics");
        }
        requireTableLength(hmtx, numberOfMetrics * 4);

        Map<Integer, Integer> cmap = readCmap(data, requiredTable(tables, "cmap"));
        if (cmap.isEmpty()) {
            throw new IllegalArgumentException("font does not contain a supported Unicode cmap");
        }
        for (int glyph : cmap.values()) {
            if (glyph <= 0 || glyph >= glyphCount) {
                throw new IllegalArgumentException("font cmap references an invalid glyph");
            }
        }
        int[] advances = new int[glyphCount];
        int lastAdvance = 0;
        for (int glyph = 0; glyph < numberOfMetrics; glyph++) {
            lastAdvance = readU16(data, hmtx.offset + glyph * 4);
            advances[glyph] = lastAdvance;
        }
        for (int glyph = numberOfMetrics; glyph < glyphCount; glyph++) {
            advances[glyph] = lastAdvance;
        }

        int[] box = new int[]{
                readS16(data, head.offset + 36),
                readS16(data, head.offset + 38),
                readS16(data, head.offset + 40),
                readS16(data, head.offset + 42)
        };
        int ascent = readS16(data, hhea.offset + 4);
        int descent = readS16(data, hhea.offset + 6);
        int capHeight = ascent;
        if (os2 != null && os2.length >= 72) {
            ascent = readS16(data, os2.offset + 68);
            descent = readS16(data, os2.offset + 70);
            if (os2.length >= 90) {
                capHeight = readS16(data, os2.offset + 88);
            } else {
                capHeight = ascent;
            }
        }
        return new TrueTypeFontData(
                name, data, cmap, advances, unitsPerEm, ascent, descent, capHeight, box);
    }

    private static boolean isTrueTypeCollection(byte[] data) {
        return data.length >= 4
                && data[0] == 't'
                && data[1] == 't'
                && data[2] == 'c'
                && data[3] == 'f';
    }

    private static byte[] extractFirstFont(byte[] collection) {
        requireRange(collection, 0, 16);
        long fontCount = readU32(collection, 8);
        if (fontCount <= 0 || fontCount > 1024) {
            throw new IllegalArgumentException("font collection contains an invalid face count");
        }
        requireRange(collection, 12, (int) fontCount * 4);
        long faceOffsetValue = readU32(collection, 12);
        if (faceOffsetValue > Integer.MAX_VALUE) {
            throw new IllegalArgumentException("font collection face offset is too large");
        }
        int faceOffset = (int) faceOffsetValue;
        requireRange(collection, faceOffset, 12);
        int tableCount = readU16(collection, faceOffset + 4);
        requireRange(collection, faceOffset + 12, tableCount * 16);

        List<CollectionTable> tables = new ArrayList<>(tableCount);
        long outputLength = 12L + tableCount * 16L;
        for (int index = 0; index < tableCount; index++) {
            int record = faceOffset + 12 + index * 16;
            long sourceOffsetValue = readU32(collection, record + 8);
            long lengthValue = readU32(collection, record + 12);
            if (sourceOffsetValue > Integer.MAX_VALUE || lengthValue > Integer.MAX_VALUE) {
                throw new IllegalArgumentException("font collection table is too large");
            }
            int sourceOffset = (int) sourceOffsetValue;
            int length = (int) lengthValue;
            requireRange(collection, sourceOffset, length);
            outputLength = align4(outputLength);
            if (outputLength > Integer.MAX_VALUE - length) {
                throw new IllegalArgumentException("extracted font is too large");
            }
            tables.add(new CollectionTable(record, sourceOffset, length, (int) outputLength));
            outputLength += length;
        }
        outputLength = align4(outputLength);
        if (outputLength > Integer.MAX_VALUE) {
            throw new IllegalArgumentException("extracted font is too large");
        }

        byte[] font = new byte[(int) outputLength];
        System.arraycopy(collection, faceOffset, font, 0, 12);
        int headOffset = -1;
        for (int index = 0; index < tables.size(); index++) {
            CollectionTable table = tables.get(index);
            int destinationRecord = 12 + index * 16;
            System.arraycopy(collection, table.sourceRecord, font, destinationRecord, 8);
            writeU32(font, destinationRecord + 8, table.destinationOffset);
            writeU32(font, destinationRecord + 12, table.length);
            System.arraycopy(collection, table.sourceOffset, font, table.destinationOffset, table.length);
            if (font[destinationRecord] == 'h'
                    && font[destinationRecord + 1] == 'e'
                    && font[destinationRecord + 2] == 'a'
                    && font[destinationRecord + 3] == 'd') {
                headOffset = table.destinationOffset;
            }
        }
        if (headOffset >= 0) {
            requireRange(font, headOffset, 12);
            writeU32(font, headOffset + 8, 0);
            writeU32(font, headOffset + 8, 0xB1B0AFBAL - checksum(font));
        }
        return font;
    }

    private static long align4(long value) {
        return (value + 3L) & ~3L;
    }

    private static long checksum(byte[] data) {
        long sum = 0;
        for (int offset = 0; offset < data.length; offset += 4) {
            long value = (long) Byte.toUnsignedInt(data[offset]) << 24;
            if (offset + 1 < data.length) {
                value |= (long) Byte.toUnsignedInt(data[offset + 1]) << 16;
            }
            if (offset + 2 < data.length) {
                value |= (long) Byte.toUnsignedInt(data[offset + 2]) << 8;
            }
            if (offset + 3 < data.length) {
                value |= Byte.toUnsignedInt(data[offset + 3]);
            }
            sum = (sum + value) & 0xFFFFFFFFL;
        }
        return sum;
    }

    boolean supports(Set<Integer> codePoints) {
        for (int codePoint : codePoints) {
            Integer glyph = cmap.get(codePoint);
            if (codePoint > 0xFFFF || glyph == null || glyph <= 0 || glyph >= advances.length) {
                return false;
            }
        }
        return true;
    }

    String pdfName() {
        return pdfName;
    }

    byte[] data() {
        return data.clone();
    }

    int ascent() {
        return ascent;
    }

    int descent() {
        return descent;
    }

    int capHeight() {
        return capHeight;
    }

    String boundingBox() {
        return boundingBox[0] + " " + boundingBox[1] + " " + boundingBox[2] + " " + boundingBox[3];
    }

    String widths(Set<Integer> codePoints) {
        StringBuilder result = new StringBuilder("[");
        for (int codePoint : codePoints) {
            Integer glyph = cmap.get(codePoint);
            if (glyph != null && glyph < advances.length) {
                result.append(codePoint).append(" [")
                        .append((int) (advances[glyph] * 1000L / unitsPerEm))
                        .append("] ");
            }
        }
        return result.append(']').toString();
    }

    byte[] cidToGidMap(Set<Integer> codePoints) {
        byte[] mapping = new byte[65536 * 2];
        for (int codePoint : codePoints) {
            Integer glyph = cmap.get(codePoint);
            if (glyph != null) {
                mapping[codePoint * 2] = (byte) (glyph >> 8);
                mapping[codePoint * 2 + 1] = (byte) (glyph & 0xFF);
            }
        }
        return mapping;
    }

    byte[] toUnicode(Set<Integer> codePoints) {
        StringBuilder result = new StringBuilder();
        result.append("/CIDInit /ProcSet findresource begin\n")
                .append("12 dict begin\nbegincmap\n")
                .append("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n")
                .append("/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n")
                .append("1 begincodespacerange\n<0000> <FFFF>\nendcodespacerange\n");
        int offset = 0;
        Integer[] points = codePoints.toArray(new Integer[0]);
        while (offset < points.length) {
            int count = Math.min(100, points.length - offset);
            result.append(count).append(" beginbfchar\n");
            for (int index = 0; index < count; index++) {
                int codePoint = points[offset + index];
                result.append(String.format("<%04X> <%04X>\n", codePoint, codePoint));
            }
            result.append("endbfchar\n");
            offset += count;
        }
        result.append("endcmap\nCMapName currentdict /CMap defineresource pop\nend\nend\n");
        return result.toString().getBytes(StandardCharsets.ISO_8859_1);
    }

    private static Map<String, Table> readTables(byte[] data) {
        int tableCount = readU16(data, 4);
        requireRange(data, 12, tableCount * 16);
        Map<String, Table> tables = new HashMap<>();
        for (int index = 0; index < tableCount; index++) {
            int record = 12 + index * 16;
            String tag = new String(data, record, 4, StandardCharsets.ISO_8859_1);
            long offset = readU32(data, record + 8);
            long length = readU32(data, record + 12);
            if (offset > Integer.MAX_VALUE || length > Integer.MAX_VALUE) {
                throw new IllegalArgumentException("font table is too large");
            }
            requireRange(data, (int) offset, (int) length);
            tables.put(tag, new Table((int) offset, (int) length));
        }
        return tables;
    }

    private static Map<Integer, Integer> readCmap(byte[] data, Table cmapTable) {
        requireTableLength(cmapTable, 4);
        int count = readU16(data, cmapTable.offset + 2);
        requireTableRange(cmapTable, 4, count * 8);
        int tableEnd = cmapTable.offset + cmapTable.length;
        for (int pass = 0; pass < 2; pass++) {
            for (int index = 0; index < count; index++) {
                int record = cmapTable.offset + 4 + index * 8;
                int platform = readU16(data, record);
                int encoding = readU16(data, record + 2);
                long relative = readU32(data, record + 4);
                if (relative > Integer.MAX_VALUE || relative >= cmapTable.length) {
                    throw new IllegalArgumentException("font contains an invalid cmap subtable offset");
                }
                int offset = cmapTable.offset + (int) relative;
                requireTableRange(cmapTable, (int) relative, 2);
                int format = readU16(data, offset);
                boolean preferred = pass == 0
                        ? format == 12 && ((platform == 3 && encoding == 10) || platform == 0)
                        : format == 4 && ((platform == 3 && encoding == 1) || platform == 0);
                if (preferred) {
                    Map<Integer, Integer> mapping = format == 12
                            ? readCmapFormat12(data, offset, tableEnd)
                            : readCmapFormat4(data, offset, tableEnd);
                    if (!mapping.isEmpty()) {
                        return mapping;
                    }
                }
            }
        }
        return new HashMap<>();
    }

    private static Map<Integer, Integer> readCmapFormat4(byte[] data, int offset, int tableEnd) {
        requireBoundedRange(data, offset, 16, tableEnd);
        int length = readU16(data, offset + 2);
        requireBoundedRange(data, offset, length, tableEnd);
        int segmentCount = readU16(data, offset + 6) / 2;
        int endCodes = offset + 14;
        int startCodes = endCodes + segmentCount * 2 + 2;
        int deltas = startCodes + segmentCount * 2;
        int rangeOffsets = deltas + segmentCount * 2;
        requireBoundedRange(data, rangeOffsets, segmentCount * 2, offset + length);
        Map<Integer, Integer> mapping = new HashMap<>();
        int previousEnd = -1;
        for (int segment = 0; segment < segmentCount; segment++) {
            int end = readU16(data, endCodes + segment * 2);
            int start = readU16(data, startCodes + segment * 2);
            int delta = readS16(data, deltas + segment * 2);
            int rangeOffset = readU16(data, rangeOffsets + segment * 2);
            if (start == 0xFFFF) {
                break;
            }
            if (start > end || start <= previousEnd) {
                throw new IllegalArgumentException("font contains overlapping cmap segments");
            }
            previousEnd = end;
            for (int codePoint = start; codePoint <= end; codePoint++) {
                int glyph;
                if (rangeOffset == 0) {
                    glyph = (codePoint + delta) & 0xFFFF;
                } else {
                    int glyphOffset = rangeOffsets + segment * 2 + rangeOffset + (codePoint - start) * 2;
                    if (glyphOffset + 2 > offset + length) {
                        continue;
                    }
                    glyph = readU16(data, glyphOffset);
                    if (glyph != 0) {
                        glyph = (glyph + delta) & 0xFFFF;
                    }
                }
                if (glyph != 0) {
                    mapping.put(codePoint, glyph);
                }
            }
        }
        return mapping;
    }

    private static Map<Integer, Integer> readCmapFormat12(byte[] data, int offset, int tableEnd) {
        requireBoundedRange(data, offset, 16, tableEnd);
        long length = readU32(data, offset + 4);
        long groupCount = readU32(data, offset + 12);
        if (length > Integer.MAX_VALUE || groupCount > (length - 16) / 12) {
            throw new IllegalArgumentException("font contains an invalid format 12 cmap");
        }
        requireBoundedRange(data, offset, (int) length, tableEnd);
        Map<Integer, Integer> mapping = new HashMap<>();
        long previousEnd = -1;
        for (int group = 0; group < (int) groupCount; group++) {
            int groupOffset = offset + 16 + group * 12;
            long start = readU32(data, groupOffset);
            long declaredEnd = readU32(data, groupOffset + 4);
            long firstGlyph = readU32(data, groupOffset + 8);
            if (start > declaredEnd || start <= previousEnd) {
                throw new IllegalArgumentException("font contains overlapping cmap groups");
            }
            previousEnd = declaredEnd;
            long end = Math.min(declaredEnd, 0xFFFFL);
            for (long codePoint = start; codePoint <= end; codePoint++) {
                long glyphValue = firstGlyph + codePoint - start;
                if (glyphValue > Integer.MAX_VALUE) {
                    throw new IllegalArgumentException("font cmap glyph index is too large");
                }
                int glyph = (int) glyphValue;
                if (glyph != 0) {
                    mapping.put((int) codePoint, glyph);
                }
            }
        }
        return mapping;
    }

    private static Table requiredTable(Map<String, Table> tables, String name) {
        Table table = tables.get(name);
        if (table == null) {
            throw new IllegalArgumentException("font is missing the " + name + " table");
        }
        return table;
    }

    private static void requireTableLength(Table table, int requiredLength) {
        if (requiredLength < 0 || table.length < requiredLength) {
            throw new IllegalArgumentException("font table is shorter than required");
        }
    }

    private static void requireTableRange(Table table, int relativeOffset, int length) {
        if (relativeOffset < 0 || length < 0 || relativeOffset > table.length - length) {
            throw new IllegalArgumentException("font read exceeds its declared table");
        }
    }

    private static void requireBoundedRange(byte[] data, int offset, int length, int end) {
        requireRange(data, offset, length);
        if (offset > end - length) {
            throw new IllegalArgumentException("font read exceeds its declared table");
        }
    }

    private static int readU16(byte[] data, int offset) {
        requireRange(data, offset, 2);
        return (Byte.toUnsignedInt(data[offset]) << 8) | Byte.toUnsignedInt(data[offset + 1]);
    }

    private static int readS16(byte[] data, int offset) {
        return (short) readU16(data, offset);
    }

    private static long readU32(byte[] data, int offset) {
        requireRange(data, offset, 4);
        return ((long) Byte.toUnsignedInt(data[offset]) << 24)
                | ((long) Byte.toUnsignedInt(data[offset + 1]) << 16)
                | ((long) Byte.toUnsignedInt(data[offset + 2]) << 8)
                | Byte.toUnsignedInt(data[offset + 3]);
    }

    private static void writeU32(byte[] data, int offset, long value) {
        requireRange(data, offset, 4);
        data[offset] = (byte) (value >> 24);
        data[offset + 1] = (byte) (value >> 16);
        data[offset + 2] = (byte) (value >> 8);
        data[offset + 3] = (byte) value;
    }

    private static void requireRange(byte[] data, int offset, int length) {
        if (offset < 0 || length < 0 || offset > data.length - length) {
            throw new IllegalArgumentException("font contains an invalid table range");
        }
    }

    private static int scale(int value, int unitsPerEm) {
        return (int) Math.round(value * 1000.0 / unitsPerEm);
    }

    private static String sanitizeName(String name) {
        String sanitized = name.trim().replaceAll("[^A-Za-z0-9_-]+", "-");
        return sanitized.isEmpty() ? "RegisteredFont" : sanitized;
    }

    private static final class Table {
        private final int offset;
        private final int length;

        private Table(int offset, int length) {
            this.offset = offset;
            this.length = length;
        }
    }

    private static final class CollectionTable {
        private final int sourceRecord;
        private final int sourceOffset;
        private final int length;
        private final int destinationOffset;

        private CollectionTable(int sourceRecord, int sourceOffset, int length, int destinationOffset) {
            this.sourceRecord = sourceRecord;
            this.sourceOffset = sourceOffset;
            this.length = length;
            this.destinationOffset = destinationOffset;
        }
    }
}