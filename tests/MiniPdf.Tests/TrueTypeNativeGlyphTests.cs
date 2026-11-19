using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace MiniSoftware.Tests;

public sealed class WindowsGlyphTheoryAttribute : TheoryAttribute
{
    public WindowsGlyphTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows()) Skip = "Native unhinted outline comparisons require Windows GDI.";
        else if (!File.Exists(TrueTypeNativeGlyphTests.FontPath)) Skip = "Native outline comparisons require installed Arial.";
    }
}

public class TrueTypeNativeGlyphTests
{
    internal static string FontPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");

    [WindowsGlyphTheory]
    [InlineData('A')]
    [InlineData('B')]
    [InlineData('O')]
    [InlineData('S')]
    [InlineData('g')]
    public void DecodedContoursMatchNativeUnhintedOutlines(char character)
    {
        var font = new TrueTypeGlyphReader(File.ReadAllBytes(FontPath));
        var glyph = font.GlyphForCharacter(character);
        var actual = Segments(font.ReadGlyph(glyph));
        var native = NativeOutline(character, font.UnitsPerEm, out var advance);
        var expected = Segments(native);

        Assert.NotEmpty(expected);
        Assert.Equal(advance, font.Advance(glyph));
        Assert.True(actual.SetEquals(expected),
            $"Unhinted {character} outline differs: {actual.Count} decoded segments, {expected.Count} native segments; " +
            $"first unexpected {actual.Except(expected).FirstOrDefault()}, first missing {expected.Except(actual).FirstOrDefault()}.");
    }

    [WindowsEmfTheory]
    [InlineData('ồ')]
    [InlineData('ề')]
    [InlineData('ẫ')]
    [InlineData('ủ')]
    public void SegoeVietnameseCompositesMatchNativeUnhintedOutlines(char character)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf"));
        var font = new TrueTypeGlyphReader(bytes);
        var glyph = font.GlyphForCharacter(character);
        var native = NativeOutline(character, font.UnitsPerEm, out var advance, face: "Segoe UI");

        Assert.Equal(advance, font.Advance(glyph));
        Assert.True(Segments(font.ReadGlyph(glyph)).SetEquals(Segments(native)), "Vietnamese composite outlines must preserve their shared horizontal origin.");
    }

    [WindowsGlyphTheory]
    [InlineData('A')]
    [InlineData('g')]
    public void PositiveCellHeightsMatchNativeUnhintedFontSizing(char character)
    {
        var bytes = File.ReadAllBytes(FontPath);
        var font = new TrueTypeGlyphReader(bytes);
        var offset = TrueTypeGlyphReaderTests.TableOffset(bytes, "OS/2");
        var cellHeight = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 74)) +
            BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 76));
        var actual = Segments(font.ReadGlyph(font.GlyphForCharacter(character)));
        var native = NativeOutline(character, font.UnitsPerEm, out var advance, cellHeight);

        Assert.True(actual.SetEquals(Segments(native)), "Positive cell height must select the matching em size.");
        Assert.Equal(font.Advance(font.GlyphForCharacter(character)), advance);
    }

    [WindowsGlyphTheory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void RequestedAverageWidthsMatchNativeHorizontalScaling(int factor, bool positiveHeight)
    {
        var bytes = File.ReadAllBytes(FontPath);
        var font = new TrueTypeGlyphReader(bytes);
        var offset = TrueTypeGlyphReaderTests.TableOffset(bytes, "OS/2");
        var average = BinaryPrimitives.ReadInt16BigEndian(bytes.AsSpan(offset + 2));
        var glyph = font.GlyphForCharacter('A');
        var expected = font.ReadGlyph(glyph).Select(contour => contour.Select(point =>
            new TrueTypeGlyphReader.Point(point.X * factor, point.Y, point.OnCurve)).ToList()).ToList();
        var cellHeight = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 74)) +
            BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 76));
        var native = NativeOutline('A', font.UnitsPerEm, out var advance,
            cellHeight: positiveHeight ? cellHeight : null, width: average * factor);

        Assert.True(Segments(expected).SetEquals(Segments(native)), "Requested width must scale only the horizontal outline.");
        Assert.Equal(font.Advance(glyph) * factor, advance);
    }

    private static HashSet<(long, long, long, long, long, long)> Segments(List<List<TrueTypeGlyphReader.Point>> contours)
    {
        var result = new HashSet<(long, long, long, long, long, long)>();
        foreach (var contour in contours)
        {
            var first = contour[0];
            var last = contour[contour.Count - 1];
            var start = first.OnCurve ? (first.X, first.Y) : last.OnCurve ? (last.X, last.Y) : Mid(last, first);
            var current = start;
            for (var index = first.OnCurve ? 1 : 0; index < contour.Count;)
            {
                var point = contour[index];
                if (point.OnCurve) { Line((point.X, point.Y)); index++; }
                else
                {
                    var next = contour[(index + 1) % contour.Count];
                    var end = next.OnCurve ? (next.X, next.Y) : Mid(point, next);
                    Add(current, (point.X, point.Y), end);
                    current = end;
                    index += next.OnCurve ? 2 : 1;
                }
            }
            Line(start);

            void Line((double X, double Y) end)
            {
                if (current != end) Add(current, ((current.Item1 + end.X) / 2, (current.Item2 + end.Y) / 2), end);
                current = end;
            }
            void Add((double X, double Y) a, (double X, double Y) b, (double X, double Y) c) =>
                result.Add((Q(a.X), Q(a.Y), Q(b.X), Q(b.Y), Q(c.X), Q(c.Y)));
        }
        return result;

        static long Q(double value) => (long)Math.Round(value * 128);
        static (double, double) Mid(TrueTypeGlyphReader.Point a, TrueTypeGlyphReader.Point b) => ((a.X + b.X) / 2, (a.Y + b.Y) / 2);
    }

    private static List<List<TrueTypeGlyphReader.Point>> NativeOutline(char character, int unitsPerEm, out int advance,
        int? cellHeight = null, int width = 0, string face = "Arial")
    {
        var dc = CreateCompatibleDC(IntPtr.Zero);
        Assert.NotEqual(IntPtr.Zero, dc);
        var font = IntPtr.Zero;
        var previous = IntPtr.Zero;
        try
        {
            font = CreateFontW(cellHeight ?? -unitsPerEm, width, 0, 0, 400, 0, 0, 0, 1, 0, 0, 3, 0, face);
            Assert.NotEqual(IntPtr.Zero, font);
            previous = SelectObject(dc, font);
            Assert.NotEqual(IntPtr.Zero, previous);
            var transform = new Matrix { M11 = 65536, M22 = 65536 };
            const uint unhintedNative = 0x102;
            var length = GetGlyphOutlineW(dc, character, unhintedNative, out var metrics, 0, null, ref transform);
            Assert.NotEqual(uint.MaxValue, length);
            var bytes = new byte[length];
            Assert.Equal(length, GetGlyphOutlineW(dc, character, unhintedNative, out metrics, length, bytes, ref transform));
            advance = metrics.AdvanceX;
            var contours = new List<List<TrueTypeGlyphReader.Point>>();
            for (var offset = 0; offset < bytes.Length;)
            {
                var end = checked(offset + (int)U32(offset));
                Assert.InRange(end, offset + 16, bytes.Length);
                Assert.Equal(24u, U32(offset + 4));
                var contour = new List<TrueTypeGlyphReader.Point> { Point(offset + 8, true) };
                for (var curve = offset + 16; curve < end;)
                {
                    var type = U16(curve);
                    var count = U16(curve + 2);
                    Assert.True(type == 1 || type == 2, $"Unexpected native quadratic outline primitive {type}.");
                    Assert.True(count >= (type == 1 ? 1 : 2));
                    Assert.True(curve + 4 + count * 8 <= end);
                    for (var index = 0; index < count; index++)
                        contour.Add(Point(curve + 4 + index * 8, type == 1 || index == count - 1));
                    curve += 4 + count * 8;
                }
                contours.Add(contour);
                offset = end;
            }
            return contours;

            ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
            uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
            TrueTypeGlyphReader.Point Point(int offset, bool onCurve) => new(
                BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset)) / 65536d,
                BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset + 4)) / 65536d, onCurve);
        }
        finally
        {
            if (previous != IntPtr.Zero) SelectObject(dc, previous);
            if (font != IntPtr.Zero) DeleteObject(font);
            DeleteDC(dc);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Matrix { internal int M11, M12, M21, M22; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Metrics { internal uint Width, Height; internal int OriginX, OriginY; internal short AdvanceX, AdvanceY; }

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern IntPtr CreateFontW(int height, int width, int escapement, int orientation, int weight,
        uint italic, uint underline, uint strikeout, uint charset, uint precision, uint clip, uint quality, uint pitch, string face);
    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern uint GetGlyphOutlineW(IntPtr dc, uint character, uint format,
        out Metrics metrics, uint length, [Out] byte[]? buffer, ref Matrix transform);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr value);
}
