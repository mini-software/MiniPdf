using System.Buffers.Binary;
using System.Runtime.InteropServices;
using static MiniSoftware.Tests.EmfVectorRendererTests;

namespace MiniSoftware.Tests;

public sealed class WindowsEmfTheoryAttribute : TheoryAttribute
{
    public WindowsEmfTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows()) Skip = "Native EMF playback comparisons require Windows GDI.";
    }
}

public class EmfNativePlaybackTests
{
    [WindowsGlyphTheory]
    [InlineData(28f, 28f, false)]
    [InlineData(28f, 28f, true)]
    [InlineData(27.447916f, 27.407408f, false)]
    [InlineData(27.447916f, 27.407408f, true)]
    [InlineData(2f, 1f, false)]
    [InlineData(2f, 1f, true)]
    [InlineData(1f, 2f, false)]
    [InlineData(1f, 2f, true)]
    public void CompatibleTextScaleMetadataMatchesNativeReference(float xScale, float yScale, bool explicitSpacing)
    {
        var ratio = yScale / xScale;
        byte[] Metafile(float x, float y, bool transformedReference = false)
        {
            var advance = transformedReference ? (int)Math.Round(12 / ratio) : 12;
            var text = TextRecord(4, 24, new ushort[] { 65, 65 }, 0, explicitSpacing ? new[] { advance, advance } : null);
            Set(text, 24, transformedReference ? 2u : 1u);
            Set(text, 28, BitConverter.SingleToUInt32Bits(x));
            Set(text, 32, BitConverter.SingleToUInt32Bits(y));
            var records = new List<byte[]> { FontRecord("Arial"), Record(37, 1), Record(18, 1), Record(22, 24) };
            if (transformedReference)
                records.Add(Record(35, BitConverter.SingleToUInt32Bits(ratio), 0, 0, 0x3F800000,
                    BitConverter.SingleToUInt32Bits(4 - 4 * ratio), 0));
            records.Add(text);
            return FrameMetafile("sparse-bounds", records.ToArray());
        }
        var expected = PlayNative(Metafile(1, 1, true), 32, 32);
        var emf = Metafile(xScale, yScale);
        var native = PlayNative(emf, 32, 32);
        Assert.Equal(expected.Where((_, index) => index % 4 != 3),
            native.Where((_, index) => index % 4 != 3));

        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        Assert.Equal(EmfVectorRenderer.Render(Metafile(1, 1, true), 32, 32, (_, _, _) => font),
            EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font));
    }

    [WindowsEmfTheory]
    [InlineData("sparse-bounds")]
    [InlineData("offset-frame")]
    [InlineData("different-device-resolutions")]
    [InlineData("micrometers")]
    [InlineData("single-pixel-bounds")]
    public void PlacesInkRelativeToPhysicalFrameRatherThanDrawingBounds(string scenario)
    {
        var emf = FrameMetafile(scenario);
        const int size = 64;

        var actual = Decode(EmfVectorRenderer.Render(emf, size, size), size, size);
        var expected = PlayNative(emf, size, size);

        AssertPixelsEqual(actual, expected, size, size);
    }

    [WindowsEmfTheory]
    [InlineData("reset")]
    [InlineData("copy")]
    [InlineData("intersect")]
    [InlineData("empty")]
    [InlineData("device-coordinates")]
    [InlineData("metadata")]
    [InlineData("stock-font")]
    public void ClippingAndMetadataMatchNativePlayback(string scenario)
    {
        var records = new List<byte[]> { Record(37, 0x80000008), Record(37, 0x80000004) };
        if (scenario == "reset") { records.Add(Record(30, 8, 8, 12, 12)); records.Add(Record(75, 0, 5)); }
        if (scenario == "copy") records.Add(Region(5));
        if (scenario == "intersect") { records.Add(Record(30, 4, 4, 10, 10)); records.Add(Region(1)); }
        if (scenario == "empty") records.Add(Record(75, 32, 5, 32, 1, 0, 0, 0, 0, 0, 0));
        if (scenario == "stock-font") records.Add(Record(37, 0x8000000D));
        if (scenario == "device-coordinates")
        {
            records.Add(Record(17, 8)); records.Add(Record(9, 32, 32)); records.Add(Record(11, 64, 64));
            records.Add(Record(35, 0x3F800000, 0, 0, 0x3F800000, 0x40800000, 0x40800000));
            records.Add(Region(5));
        }
        if (scenario == "metadata")
        {
            records.Add(Record(70, 4, 0x12345678));
            records.Add(Record(70, 28, 0x43494447, 2, 0, 0, 32, 32, 0));
            records.Add(Record(48, 0x8000000F));
        }
        records.Add(Record(3, 0, 0, 31, 31, 4, 0, 0, 32, 0, 32, 32, 0, 32));
        if (scenario == "metadata") records.Add(Record(70, 8, 0x43494447, 3));
        var emf = FrameMetafile("sparse-bounds", records.ToArray());

        var actual = Decode(EmfVectorRenderer.Render(emf, 64, 64), 64, 64);
        var expected = PlayNative(emf, 64, 64);

        AssertPixelsEqual(actual, expected, 64, 64);
    }

    internal static byte[] Region(uint mode) =>
        Record(75, 48, mode, 32, 1, 1, 16, 8, 8, 12, 12, 8, 8, 12, 12);

    [WindowsEmfTheory]
    [InlineData(0x000000u)]
    [InlineData(0xFFFFFFu)]
    [InlineData(0x123456u)]
    public void PaletteRgbColorsMatchNativeTrueColorPlayback(uint color)
    {
        byte[] Metafile(uint value)
        {
            var text = TextRecord(0, 0, Array.Empty<ushort>(), 2);
            Set(text, 56, 20); Set(text, 60, 4); Set(text, 64, 24); Set(text, 68, 8);
            return FrameMetafile("sparse-bounds", Record(37, 0x80000008),
                Record(39, 1, 0, value, 0), Record(37, 1), Record(43, 4, 4, 12, 12),
                Record(38, 2, 0, 0, 0, value), Record(37, 2), Record(27, 4, 16), Record(54, 12, 16),
                Record(15, 20, 20, value), Record(24, value), Record(25, value), text);
        }
        var expected = PlayNative(Metafile(color), 32, 32);
        var emf = Metafile(color | 0x02000000);
        var native = PlayNative(emf, 32, 32);
        Assert.Equal(expected.Where((_, index) => index % 4 != 3),
            native.Where((_, index) => index % 4 != 3));

        var actual = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);
        AssertPixelsEqual(actual, expected, 32, 32);
    }

    [WindowsEmfTheory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(4u)]
    [InlineData(11u)]
    [InlineData(13u)]
    [InlineData(16u)]
    public void DestinationIndependentRasterOperationsMatchNativePlayback(uint operation)
    {
        var text = TextRecord(0, 0, Array.Empty<ushort>(), 2);
        Set(text, 56, 20); Set(text, 60, 4); Set(text, 64, 24); Set(text, 68, 8);
        var emf = FrameMetafile("sparse-bounds", Record(37, 0x80000008), Record(39, 1, 0, 0x112233, 0),
            Record(37, 1), Record(43, 0, 0, 32, 32), Record(20, operation), Record(39, 2, 0, 0x123456, 0),
            Record(37, 2), Record(43, 4, 4, 12, 12), Record(38, 3, 0, 0, 0, 0x654321),
            Record(37, 3), Record(27, 4, 16), Record(54, 12, 16), Record(15, 20, 20, 0xABCDEF),
            Record(25, 0x345678), text);
        var expected = PlayNative(emf, 32, 32);

        var actual = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);
        AssertPixelsEqual(actual, expected, 32, 32);
    }

    [WindowsEmfTheory]
    [InlineData(128, false)]
    [InlineData(128, true)]
    [InlineData(255, false)]
    [InlineData(255, true)]
    public void ReservedAlphaBlendFlagsMatchNativePlayback(int flags, bool perPixelAlpha)
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 64, 128 }, alphaBlend: true, imageWidth: 1);
        var blend = 0x00FF0000u | (perPixelAlpha ? 0x01000000u : 0);
        Set(bitmap, 40, blend);
        var expected = PlayNative(FrameMetafile("sparse-bounds", bitmap), 32, 32);
        Assert.Contains(expected, value => value != 255);

        Set(bitmap, 40, blend | (uint)flags << 8);
        var emf = FrameMetafile("sparse-bounds", bitmap);
        var native = PlayNative(emf, 32, 32);
        Assert.Equal(expected.Where((_, index) => index % 4 != 3),
            native.Where((_, index) => index % 4 != 3));

        var actual = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);
        AssertPixelsEqual(actual, expected, 32, 32);
    }

    [WindowsEmfTheory]
    [InlineData(0, "upscale")]
    [InlineData(1, "identity")]
    [InlineData(1, "downscale")]
    [InlineData(1, "nonuniform")]
    public void CreatedThinPensMatchNativeScaling(uint penWidth, string scenario)
    {
        var records = new List<byte[]> { Record(38, 1, 0, penWidth, 0, 0), Record(37, 1) };
        if (scenario is "upscale" or "identity")
        {
            records.Add(Record(27, 4, 8)); records.Add(Record(54, 20, 8));
        }
        else
        {
            records.Add(Record(17, 8)); records.Add(Record(9, 256, 256));
            records.Add(Record(11, 32, scenario == "nonuniform" ? 16u : 32u));
            records.Add(Record(27, 32, scenario == "nonuniform" ? 128u : 64u));
            records.Add(Record(54, 128, scenario == "nonuniform" ? 128u : 64u));
        }
        var emf = FrameMetafile("sparse-bounds", records.ToArray());
        var size = scenario == "identity" ? 32 : 64;
        var expected = PlayNative(emf, size, size);
        var actual = Decode(EmfVectorRenderer.Render(emf, size, size), size, size);

        AssertPixelsEqual(actual, expected, size, size);
    }

    [WindowsEmfTheory]
    [InlineData(64)]
    public void RejectsScaledPensRequiringWideRasterization(int size)
    {
        var emf = FrameMetafile("sparse-bounds", Record(38, 1, 0, 1, 0, 0), Record(37, 1),
            Record(27, 4, 8), Record(54, 20, 8));
        var expected = PlayNative(emf, size, size);
        Assert.Equal(0, expected[(15 * size + 9) * 4]);
        Assert.Equal(0, expected[(16 * size + 9) * 4]);

        var error = Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, size, size));
        Assert.Contains("wider than one device pixel", error.Message);
    }

    internal static byte[] FrameMetafile(string scenario, params byte[][] records)
    {
        var emf = Build(records.Length == 0 ? new[]
        {
            Record(37, 0x80000008), Record(37, 0x80000004),
            Record(3, 8, 8, 15, 15, 4, 8, 8, 16, 8, 16, 16, 8, 16)
        } : records);
        if (scenario == "single-pixel-bounds") emf = Build(Record(15, 8, 8, 0x0000FF));
        Set(emf, 8, 8); Set(emf, 12, 8); Set(emf, 16, 15); Set(emf, 20, 15);
        if (records.Length != 0) { Set(emf, 8, 0); Set(emf, 12, 0); Set(emf, 16, 63); Set(emf, 20, 63); }
        Set(emf, 32, 800); Set(emf, 36, 800);
        Set(emf, 72, 32); Set(emf, 76, 32); Set(emf, 80, 8); Set(emf, 84, 8);
        if (scenario == "single-pixel-bounds") { Set(emf, 16, 8); Set(emf, 20, 8); }
        if (scenario == "offset-frame")
        {
            Set(emf, 24, 200); Set(emf, 28, 200); Set(emf, 32, 1000); Set(emf, 36, 1000);
        }
        if (scenario == "different-device-resolutions") { Set(emf, 80, 16); Set(emf, 32, 1600); }
        if (scenario == "micrometers")
        {
            var extended = new byte[emf.Length + 20];
            Buffer.BlockCopy(emf, 0, extended, 0, 88);
            Buffer.BlockCopy(emf, 88, extended, 108, emf.Length - 88);
            emf = extended;
            Set(emf, 4, 108); Set(emf, 48, (uint)emf.Length);
            Set(emf, 100, 16000); Set(emf, 104, 16000);
        }
        return emf;
    }

    internal static void Set(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);

    internal static void AssertPixelsEqual(byte[] actual, byte[] expected, int width, int height)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * 4;
                var alpha = actual[offset + 3];
                for (var channel = 0; channel < 3; channel++)
                {
                    var value = (actual[offset + channel] * alpha + 255 * (255 - alpha) + 127) / 255;
                    Assert.True(value == expected[offset + 2 - channel],
                        $"Native EMF mismatch at ({x}, {y}), channel {channel}: " +
                        $"managed {value}, native {expected[offset + 2 - channel]}.");
                }
            }
        }
    }

    internal static byte[] PlayNative(byte[] emf, int width, int height)
    {
        var metafile = SetEnhMetaFileBits((uint)emf.Length, emf);
        Assert.NotEqual(IntPtr.Zero, metafile);
        var dc = CreateCompatibleDC(IntPtr.Zero);
        var bitmap = IntPtr.Zero;
        var previous = IntPtr.Zero;
        try
        {
            Assert.NotEqual(IntPtr.Zero, dc);
            var info = new byte[40];
            BinaryPrimitives.WriteInt32LittleEndian(info, 40);
            BinaryPrimitives.WriteInt32LittleEndian(info.AsSpan(4), width);
            BinaryPrimitives.WriteInt32LittleEndian(info.AsSpan(8), -height);
            BinaryPrimitives.WriteInt16LittleEndian(info.AsSpan(12), 1);
            BinaryPrimitives.WriteInt16LittleEndian(info.AsSpan(14), 32);
            bitmap = CreateDIBSection(dc, info, 0, out var bits, IntPtr.Zero, 0);
            Assert.NotEqual(IntPtr.Zero, bitmap);
            previous = SelectObject(dc, bitmap);
            Assert.NotEqual(IntPtr.Zero, previous);
            var pixels = Enumerable.Repeat((byte)255, width * height * 4).ToArray();
            Marshal.Copy(pixels, 0, bits, pixels.Length);
            var bounds = new NativeRect { Right = width, Bottom = height };
            Assert.True(PlayEnhMetaFile(dc, metafile, ref bounds));
            Assert.True(GdiFlush());
            Marshal.Copy(bits, pixels, 0, pixels.Length);
            return pixels;
        }
        finally
        {
            if (previous != IntPtr.Zero) SelectObject(dc, previous);
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (dc != IntPtr.Zero) DeleteDC(dc);
            DeleteEnhMetaFile(metafile);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { internal int Left, Top, Right, Bottom; }
    [DllImport("gdi32.dll")]
    private static extern IntPtr SetEnhMetaFileBits(uint length, byte[] bytes);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteEnhMetaFile(IntPtr metafile);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDIBSection(IntPtr dc, byte[] info, uint usage, out IntPtr bits, IntPtr section, uint offset);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr value);
    [DllImport("gdi32.dll")]
    private static extern bool PlayEnhMetaFile(IntPtr dc, IntPtr metafile, ref NativeRect bounds);
    [DllImport("gdi32.dll")]
    private static extern bool GdiFlush();
}
