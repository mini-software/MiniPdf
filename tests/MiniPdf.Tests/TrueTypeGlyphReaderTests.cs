using System.Buffers.Binary;

namespace MiniSoftware.Tests;

public class TrueTypeGlyphReaderTests
{
    [Fact]
    public void ReadsContoursAndHorizontalMetricsWithoutDrawingDependencies()
    {
        var font = new TrueTypeGlyphReader(CreateFont(Box()));
        var contours = font.ReadGlyph(1);

        Assert.Equal(1000, font.UnitsPerEm);
        Assert.Equal(600, font.Advance(1));
        Assert.Equal(1, font.GlyphForCharacter('A'));
        Assert.Equal(2, contours.Count);
        Assert.Equal(new[] { (0d, 0d), (0d, 700d), (500d, 700d), (500d, 0d) },
            contours[0].Select(point => (point.X, point.Y)));
        Assert.All(contours.SelectMany(contour => contour), point => Assert.True(point.OnCurve));
        Assert.Empty(font.ReadGlyph(0));
    }

    [Fact]
    public void DecodesRepeatedFlagsAndUnsignedShortCoordinateDeltas()
    {
        var glyph = new byte[24];
        Write16(glyph, 0, 1);
        Write16(glyph, 10, 3);
        glyph[14] = 0x3F;
        glyph[15] = 3;
        new byte[] { 10, 20, 30, 40, 20, 20, 20, 20 }.CopyTo(glyph, 16);
        var font = new TrueTypeGlyphReader(CreateFont(glyph));

        Assert.Equal(new[] { (10d, 20d), (30d, 40d), (60d, 60d), (100d, 80d) },
            Assert.Single(font.ReadGlyph(1)).Select(point => (point.X, point.Y)));
    }

    [Theory]
    [InlineData(0x000B, 100, 200)]
    [InlineData(0x080B, 50, 100)]
    [InlineData(0x100B, 100, 200)]
    public void ScalesCompositePointsAndHonorsOffsetCoordinateSystem(ushort flags, int expectedX, int expectedY)
    {
        var font = new TrueTypeGlyphReader(CreateFont(Box(), Composite(flags, 1, 100, 200, 8192)));

        var contours = font.ReadGlyph(2);

        Assert.Equal((expectedX, expectedY), ((int)contours[0][0].X, (int)contours[0][0].Y));
        Assert.Equal((expectedX + 250, expectedY + 350), ((int)contours[0][2].X, (int)contours[0][2].Y));
    }

    [Fact]
    public void AppliesCompositeMatrixInOpenTypeRowVectorOrder()
    {
        var font = new TrueTypeGlyphReader(CreateFont(Box(), Composite(0x83, 1, -100, 200, 0, 16384, -16384, 0)));

        var point = font.ReadGlyph(2)[0][2];

        Assert.Equal(-800, point.X);
        Assert.Equal(700, point.Y);
    }

    [Fact]
    public void ReadsSignedByteCompositeOffsets()
    {
        var glyph = new byte[16];
        Write16(glyph, 0, -1); Write16(glyph, 10, 2); Write16(glyph, 12, 1);
        glyph[14] = unchecked((byte)-10); glyph[15] = unchecked((byte)-20);
        var font = new TrueTypeGlyphReader(CreateFont(Box(), glyph));

        var point = font.ReadGlyph(2)[0][0];

        Assert.Equal(-10, point.X);
        Assert.Equal(-20, point.Y);
    }

    [Fact]
    public void LooksUpIndirectFormatFourGlyphsAndAppliesDelta()
    {
        var cmap = new byte[36];
        Write16(cmap, 0, 4); Write16(cmap, 2, cmap.Length); Write16(cmap, 6, 4);
        Write16(cmap, 14, 66); Write16(cmap, 16, 0xFFFF);
        Write16(cmap, 20, 65); Write16(cmap, 22, 0xFFFF);
        Write16(cmap, 24, 1); Write16(cmap, 26, 1); Write16(cmap, 28, 4);
        Write16(cmap, 32, 1); Write16(cmap, 34, 2);
        var font = new TrueTypeGlyphReader(ReplaceCmap(CreateFont(Box(), Box(), Box()), cmap, 1));

        Assert.Equal(2, font.GlyphForCharacter('A'));
        Assert.Equal(3, font.GlyphForCharacter('B'));
        Assert.Throws<NotSupportedException>(() => font.GlyphForCharacter('@'));
    }

    [Fact]
    public void LooksUpFormatTwelveGroupsWithoutFillingCharacterGaps()
    {
        var cmap = new byte[40];
        Write16(cmap, 0, 12); Write32(cmap, 4, (uint)cmap.Length); Write32(cmap, 12, 2);
        Write32(cmap, 16, 65); Write32(cmap, 20, 65); Write32(cmap, 24, 1);
        Write32(cmap, 28, 67); Write32(cmap, 32, 67); Write32(cmap, 36, 2);
        var font = new TrueTypeGlyphReader(ReplaceCmap(CreateFont(Box(), Box()), cmap, 10));

        Assert.Equal(1, font.GlyphForCharacter('A'));
        Assert.Equal(2, font.GlyphForCharacter('C'));
        Assert.Throws<NotSupportedException>(() => font.GlyphForCharacter('B'));
    }

    [Fact]
    public void ReadsShortLocaOffsetsAndReusesFinalLongHorizontalMetric()
    {
        var bytes = CreateFont(Box(), Box());
        var head = TableOffset(bytes, "head");
        var loca = TableOffset(bytes, "loca");
        Write16(bytes, head + 50, 0);
        var glyphSize = Box().Length;
        Write16(bytes, loca, 0); Write16(bytes, loca + 2, 0);
        Write16(bytes, loca + 4, glyphSize / 2); Write16(bytes, loca + 6, glyphSize);
        Write16(bytes, TableOffset(bytes, "hhea") + 34, 1);
        var font = new TrueTypeGlyphReader(bytes);

        Assert.Equal(600, font.Advance(2));
        Assert.Equal(2, font.ReadGlyph(2).Count);
    }

    [Theory]
    [InlineData("cycle")]
    [InlineData("glyph-index")]
    [InlineData("truncated")]
    [InlineData("repeat")]
    [InlineData("transform-flags")]
    [InlineData("location")]
    [InlineData("table-range")]
    public void RejectsMalformedGlyphsAndFontRanges(string defect)
    {
        var glyph = Box();
        if (defect == "cycle") glyph = Composite(3, 1, 0, 0);
        if (defect == "glyph-index") glyph = Composite(3, 300, 0, 0);
        if (defect == "truncated") Array.Resize(ref glyph, 16);
        if (defect == "repeat") { glyph[16] = 9; glyph[17] = 255; }
        if (defect == "transform-flags") glyph = Composite(0x4B, 1, 0, 0);
        var bytes = CreateFont(glyph);
        if (defect == "location") Write32(bytes, TableOffset(bytes, "loca") + 4, uint.MaxValue);
        if (defect == "table-range") Write32(bytes, 20, uint.MaxValue);

        Assert.Throws<InvalidDataException>(() => new TrueTypeGlyphReader(bytes).ReadGlyph(1));
    }

    [Theory]
    [InlineData(1)]
    public void RejectsPointAttachedComposites(ushort flags)
    {
        var font = new TrueTypeGlyphReader(CreateFont(Box(), Composite(flags, 1, 0, 0)));

        Assert.Throws<NotSupportedException>(() => font.ReadGlyph(2));
    }

    [Fact]
    public void GridRoundingDoesNotRoundUnhintedCompositeDesignOffsets()
    {
        var font = new TrueTypeGlyphReader(CreateFont(Box(), Composite(0x80F, 1, 1, 1, 0x2000)));
        var contours = font.ReadGlyph(2);

        Assert.Equal(0.5, contours[0][0].X);
        Assert.Equal(0.5, contours[0][0].Y);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsInheritedCompositeMetricsThatDifferFromTheParent(bool bearing)
    {
        var bytes = CreateFont(Box(), Composite(0x203, 1, 0, 0));
        Write16(bytes, TableOffset(bytes, "hmtx") + 8 + (bearing ? 2 : 0), 100);
        var font = new TrueTypeGlyphReader(bytes);

        Assert.Throws<NotSupportedException>(() => font.ReadGlyph(2));
    }

    internal static byte[] Box() => Simple(
        new[] { (0, 0, true), (0, 700, true), (500, 700, true), (500, 0, true) },
        new[] { (100, 100, true), (400, 100, true), (400, 600, true), (100, 600, true) });

    internal static byte[] Simple(params (int X, int Y, bool OnCurve)[][] contours)
    {
        var points = contours.SelectMany(contour => contour).ToArray();
        var size = 12 + contours.Length * 2 + points.Length * 5;
        var bytes = new byte[(size + 1) & ~1];
        Write16(bytes, 0, contours.Length);
        var last = -1;
        for (var index = 0; index < contours.Length; index++)
        {
            last += contours[index].Length;
            Write16(bytes, 10 + index * 2, last);
        }
        var flagsOffset = 12 + contours.Length * 2;
        var previousX = 0;
        var previousY = 0;
        for (var index = 0; index < points.Length; index++)
        {
            var point = points[index];
            bytes[flagsOffset + index] = point.OnCurve ? (byte)1 : (byte)0;
            Write16(bytes, flagsOffset + points.Length + index * 2, point.X - previousX);
            Write16(bytes, flagsOffset + points.Length * 3 + index * 2, point.Y - previousY);
            previousX = point.X; previousY = point.Y;
        }
        return bytes;
    }

    internal static byte[] Composite(ushort flags, ushort glyph, short x, short y, params short[] transform)
    {
        var bytes = new byte[18 + transform.Length * 2];
        Write16(bytes, 0, -1);
        Write16(bytes, 10, flags);
        Write16(bytes, 12, glyph);
        Write16(bytes, 14, x);
        Write16(bytes, 16, y);
        for (var index = 0; index < transform.Length; index++) Write16(bytes, 18 + index * 2, transform[index]);
        return bytes;
    }

    internal static byte[] CreateFont(params byte[][] glyphs)
    {
        var count = glyphs.Length + 1;
        var head = new byte[54];
        Write16(head, 18, 1000);
        Write16(head, 50, 1);
        var hhea = new byte[36];
        Write16(hhea, 34, count);
        var os2 = new byte[78];
        Write16(os2, 2, 600);
        Write16(os2, 74, 1000);
        Write16(os2, 76, 200);
        var hmtx = new byte[count * 4];
        for (var index = 0; index < count; index++) Write16(hmtx, index * 4, 600);
        var maxp = new byte[6];
        Write32(maxp, 0, 0x10000);
        Write16(maxp, 4, count);
        var loca = new byte[(count + 1) * 4];
        var offset = 0;
        for (var index = 0; index < glyphs.Length; index++)
        {
            offset += glyphs[index].Length;
            Write32(loca, (index + 2) * 4, (uint)offset);
        }
        var cmap = new byte[44];
        Write16(cmap, 2, 1);
        Write16(cmap, 4, 3);
        Write16(cmap, 6, 1);
        Write32(cmap, 8, 12);
        Write16(cmap, 12, 4);
        Write16(cmap, 14, 32);
        Write16(cmap, 18, 4);
        Write16(cmap, 26, 0x40 + glyphs.Length);
        Write16(cmap, 28, 0xFFFF);
        Write16(cmap, 32, 0x41);
        Write16(cmap, 34, 0xFFFF);
        Write16(cmap, 36, -64);
        Write16(cmap, 38, 1);
        var tables = new Dictionary<string, byte[]>
        {
            ["head"] = head, ["hhea"] = hhea, ["hmtx"] = hmtx, ["maxp"] = maxp,
            ["loca"] = loca, ["glyf"] = glyphs.SelectMany(glyph => glyph).ToArray(),
            ["OS/2"] = os2, ["cmap"] = cmap
        };
        var dataOffset = 12 + tables.Count * 16;
        var font = new byte[dataOffset + tables.Values.Sum(table => (table.Length + 3) & ~3)];
        Write32(font, 0, 0x10000);
        Write16(font, 4, tables.Count);
        var entry = 12;
        foreach (var table in tables)
        {
            System.Text.Encoding.ASCII.GetBytes(table.Key).CopyTo(font, entry);
            Write32(font, entry + 8, (uint)dataOffset);
            Write32(font, entry + 12, (uint)table.Value.Length);
            table.Value.CopyTo(font, dataOffset);
            dataOffset += (table.Value.Length + 3) & ~3;
            entry += 16;
        }
        return font;
    }

    internal static int TableOffset(byte[] font, string tag)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4));
        for (var index = 0; index < count; index++)
            if (System.Text.Encoding.ASCII.GetString(font, 12 + index * 16, 4) == tag)
                return (int)BinaryPrimitives.ReadUInt32BigEndian(font.AsSpan(20 + index * 16));
        throw new InvalidOperationException("Synthetic font table was not found.");
    }

    private static byte[] ReplaceCmap(byte[] font, byte[] subtable, int encoding)
    {
        var offset = TableOffset(font, "cmap");
        Array.Resize(ref font, offset + 12 + subtable.Length);
        font.AsSpan(offset).Clear();
        Write16(font, offset + 2, 1); Write16(font, offset + 4, 3);
        Write16(font, offset + 6, encoding); Write32(font, offset + 8, 12);
        subtable.CopyTo(font, offset + 12);
        var count = BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4));
        for (var index = 0; index < count; index++)
            if (System.Text.Encoding.ASCII.GetString(font, 12 + index * 16, 4) == "cmap")
                Write32(font, 24 + index * 16, (uint)(12 + subtable.Length));
        return font;
    }

    internal static void Write16(byte[] bytes, int offset, int value) =>
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), unchecked((ushort)value));

    internal static void Write32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
}
