using System.Buffers.Binary;
using System.IO.Compression;

namespace MiniSoftware.Tests;

public class EmfVectorRendererTests
{
    [Theory]
    [InlineData(0x01000000u)]
    [InlineData(0x03000000u)]
    [InlineData(0xFF000000u)]
    public void RejectsIndexedOrUnsupportedColorEncodings(uint color)
    {
        Assert.Throws<NotSupportedException>(() =>
            EmfVectorRenderer.Render(Build(Record(24, color)), 32, 32));
    }

    [Theory]
    [InlineData(0u, 255, 255, 255, 255)]
    [InlineData(1u, 0, 0, 0, 255)]
    [InlineData(4u, 169, 203, 237, 255)]
    [InlineData(11u, 0, 0, 0, 0)]
    [InlineData(13u, 86, 52, 18, 255)]
    [InlineData(16u, 255, 255, 255, 255)]
    public void RasterOperationsPreserveTransparencyAndDoNotAffectTextOrBitmaps(
        uint operation, byte red, byte green, byte blue, byte alpha)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 255, 0 }, alphaBlend: true, imageWidth: 1);
        Set(bitmap, 24, 16); Set(bitmap, 28, 16); Set(bitmap, 32, 4); Set(bitmap, 36, 4);
        Set(bitmap, 40, 0x00FF0000);
        var emf = Build(Record(20, operation), Record(15, 30, 30, 0x123456), bitmap,
            FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), Record(24, 0x020000FF),
            TextRecord(4, 24, new ushort[] { 1 }, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 30, 30, red, green, blue, alpha);
        AssertPixel(pixels, 32, 17, 17, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 5, 11, 255, 0, 0, 255);
    }

    [Fact]
    public void RestoresRasterOperationAlongWithDeviceContext()
    {
        var emf = Build(Record(20, 0), Record(33), Record(20, 1), Record(15, 4, 4, 0x123456),
            Record(34, uint.MaxValue), Record(15, 5, 4, 0x123456), Record(20, 13), Record(15, 6, 4, 0x123456));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 4, 4, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 5, 4, 255, 255, 255, 255);
        AssertPixel(pixels, 32, 6, 4, 86, 52, 18, 255);
    }

    [Theory]
    [InlineData(6u)]
    [InlineData(7u)]
    [InlineData(9u)]
    [InlineData(17u)]
    public void UnsupportedRasterOperationsDoNotReturnPartialImages(uint operation)
    {
        Assert.Throws<NotSupportedException>(() =>
            EmfVectorRenderer.Render(Build(Record(15, 4, 4, 0x123456), Record(20, operation)), 32, 32));
    }

    [Fact]
    public void StockFontSelectionClearsExplicitFontWithoutSuppressingOtherDrawing()
    {
        var pixel = Record(15, 4, 4, 0x123456);
        var emf = Build(Record(37, 0x8000000D), pixel);
        Assert.Equal(EmfVectorRenderer.Render(Build(pixel), 32, 32), EmfVectorRenderer.Render(emf, 32, 32));

        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = Build(FontRecord(), Record(37, 1), Record(37, 0x8000000D), Record(18, 1),
            Record(22, 24), TextRecord(4, 24, new ushort[] { 1 }, 16));
        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(text, 32, 32, (_, _, _) => font));
    }

    [Fact]
    public void RestoresExplicitFontAfterStockFontSelection()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = TextRecord(4, 24, new ushort[] { 1 }, 16);
        var baseline = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), text);
        var saved = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), Record(33),
            Record(37, 0x8000000D), Record(34, uint.MaxValue), text);

        Assert.Equal(EmfVectorRenderer.Render(baseline, 32, 32, (_, _, _) => font),
            EmfVectorRenderer.Render(saved, 32, 32, (_, _, _) => font));
    }

    [Fact]
    public void RendersPolygonAndLineWithIndependentPngEncoding()
    {
        var emf = Build(
            Record(39, 1, 0, 0x0000FF, 0),
            Record(37, 1),
            Record(37, 0x80000008),
            Polygon16((4, 4), (28, 4), (4, 28)),
            Record(38, 2, 0, 0, 0, 0xFF0000),
            Record(37, 2),
            Record(27, 4, 30),
            Record(54, 28, 30));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 6, 6, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 26, 26, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 15, 30, 0, 0, 255, 255);
        AssertPixel(pixels, 32, 15, 29, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 28, 30, 0, 0, 0, 0);
    }

    [Fact]
    public void RectangleOutlineExcludesRightAndBottomEdges()
    {
        var emf = Build(Record(37, 0x80000005), Record(43, 4, 4, 12, 12));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 4, 4, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 11, 11, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 12, 8, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 8, 12, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 8, 8, 0, 0, 0, 0);
    }

    [Fact]
    public void CroppingUsesRenderedPixelsAndPreservesAlpha()
    {
        var emf = Build(Record(37, 0x80000008), Record(39, 1, 0, 0x0000FF, 0), Record(37, 1), Record(43, 4, 4, 20, 20));
        var png = EmfVectorRenderer.RenderCropped(emf, 32, 32, null, 0.25f, 0.125f, 0.25f, 0.125f);
        var pixels = Decode(png, 16, 24);

        AssertPixel(pixels, 16, 1, 1, 255, 0, 0, 255);
        AssertPixel(pixels, 16, 12, 16, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(0u, 4u)]
    [InlineData(2u, 28u)]
    [InlineData(6u, 16u)]
    public void TopAlignedTextUsesTheFontCellAscent(uint alignment, uint referenceX)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        byte[] RenderText(uint align, uint y) => EmfVectorRenderer.Render(
            Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, align),
                TextRecord(referenceX, y, new ushort[] { 1, 1 }, 16)), 32, 32, (_, _, _) => font);

        Assert.Equal(RenderText(alignment + 24, 24), RenderText(alignment, 4));
    }

    [Theory]
    [InlineData('ề')]
    [InlineData('修')]
    public void SimpleUnicodeTextUsesCmapInsteadOfRequiringGlyphIndices(char character)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var cmap = TrueTypeGlyphReaderTests.TableOffset(font, "cmap");
        TrueTypeGlyphReaderTests.Write16(font, cmap + 26, character);
        TrueTypeGlyphReaderTests.Write16(font, cmap + 32, character);
        TrueTypeGlyphReaderTests.Write16(font, cmap + 36, 1 - character);
        byte[] RenderText(ushort value, uint options) => EmfVectorRenderer.Render(
            Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24),
                TextRecord(4, 24, new[] { value }, options)), 32, 32, (_, _, _) => font);

        Assert.Equal(RenderText(1, 16), RenderText(character, 0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SyntheticBoldOverstrikesWithoutReplacingTheFamily(bool suppliedAdvances)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        TrueTypeGlyphReaderTests.Write16(font, TrueTypeGlyphReaderTests.TableOffset(font, "OS/2") + 4, 400);
        var definition = FontRecord(); Set(definition, 28, 700);
        var emf = Build(definition, Record(37, 1), Record(18, 1), Record(22, 24),
            TextRecord(4, 24, new ushort[] { 1, 1 }, 16, suppliedAdvances ? new[] { 12, 12 } : null));
        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 14, 11, 0, 0, 0, 255);
        AssertPixel(pixels, 32, suppliedAdvances ? 17 : 18, 11, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void HonorsPolygonFillRule(uint fillMode, bool centerIsFilled)
    {
        var emf = Build(
            Record(37, 0x80000008),
            Record(37, 0x80000004),
            Record(19, fillMode),
            Polygon16((4, 4), (28, 4), (28, 28), (4, 28), (4, 4),
                (10, 10), (22, 10), (22, 22), (10, 22), (10, 10), (4, 4)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 16, 0, 0, 0, centerIsFilled ? (byte)255 : (byte)0);
        AssertPixel(pixels, 32, 6, 16, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void RestoresMappingBrushAndClipTogether(int restoreLevel)
    {
        var emf = Build(
            Record(39, 1, 0, 0x0000FF, 0),
            Record(37, 1),
            Record(37, 0x80000008),
            Record(33),
            Record(17, 8),
            Record(9, 64, 64),
            Record(11, 32, 32),
            Record(30, 8, 8, 24, 24),
            Record(43, 0, 0, 64, 64),
            Record(37, 0x80000004),
            Record(34, unchecked((uint)restoreLevel)),
            Record(43, 24, 24, 30, 30));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 3, 3, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 4, 4, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 12, 12, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 25, 25, 255, 0, 0, 255);
    }

    [Fact]
    public void AppliesNegativeViewportExtentWithoutOverflowingExtremeCoordinates()
    {
        var emf = Build(
            Record(17, 8),
            Record(9, 32, 32),
            Record(11, unchecked((uint)-32), 32),
            Record(12, 32, 0),
            Record(37, 0x80000008),
            Record(37, 0x80000004),
            Record(43, 0, 0, 8, 8),
            Record(43, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 25, 4, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 4, 4, 0, 0, 0, 0);
    }

    [Fact]
    public void PhysicalFramePreservesWhitespaceAroundSparseDrawingBounds()
    {
        var emf = EmfNativePlaybackTests.FrameMetafile("sparse-bounds");

        var pixels = Decode(EmfVectorRenderer.Render(emf, 64, 64), 64, 64);

        AssertPixel(pixels, 64, 0, 0, 0, 0, 0, 0);
        AssertPixel(pixels, 64, 16, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 64, 31, 31, 0, 0, 0, 255);
        AssertPixel(pixels, 64, 32, 32, 0, 0, 0, 0);
    }

    [Fact]
    public void NullClipResetCanBeRestoredWithTheDeviceContext()
    {
        var emf = Build(Record(30, 8, 8, 12, 12), Record(33), Record(75, 0, 5),
            Record(15, 2, 2, 0x0000FF), Record(34, uint.MaxValue),
            Record(15, 4, 4, 0xFF0000), Record(15, 9, 9, 0xFF0000));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 2, 2, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 4, 4, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 9, 9, 0, 0, 255, 255);
    }

    [Fact]
    public void PrivateGroupAndOriginalWmfCommentsDoNotAddDrawing()
    {
        var emf = Build(Record(70, 3, 0x00434241),
            Record(70, 28, 0x43494447, 2, 0, 0, 32, 32, 0),
            Record(70, 24, 0x43494447, 0x80000001, 0x300, 0, 0, 0),
            Record(15, 8, 8, 0x0000FF), Record(70, 8, 0x43494447, 3));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 8, 255, 0, 0, 255);
        Assert.Single(pixels.Where((value, index) => index % 4 == 3 && value != 0));
    }

    [Theory]
    [InlineData("comment-length")]
    [InlineData("comment-padding")]
    [InlineData("group-description")]
    [InlineData("unclosed-group")]
    [InlineData("unexpected-group-end")]
    [InlineData("wmf-length")]
    [InlineData("region-size")]
    [InlineData("region-count")]
    [InlineData("region-null")]
    public void RejectsMalformedCommentsAndRegions(string defect)
    {
        var record = defect switch
        {
            "comment-length" => Record(70, uint.MaxValue),
            "comment-padding" => Record(70, 0, 0),
            "group-description" => Record(70, 28, 0x43494447, 2, 0, 0, 32, 32, uint.MaxValue),
            "unclosed-group" => Record(70, 28, 0x43494447, 2, 0, 0, 32, 32, 0),
            "unexpected-group-end" => Record(70, 8, 0x43494447, 3),
            "wmf-length" => Record(70, 24, 0x43494447, 0x80000001, 0x300, 0, 0, uint.MaxValue),
            "region-size" => Record(75, uint.MaxValue, 5),
            "region-count" => Record(75, 32, 5, 32, 1, 1, 16, 8, 8, 12, 12),
            _ => Record(75, 0, 1)
        };

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(record), 32, 32));
    }

    [Theory]
    [InlineData(0)]
    public void RenderingExtensionsAreNotSilentlySkipped(uint identifier)
    {
        var emf = Build(Record(15, 1, 1, 0x0000FF), Record(70, 4, identifier));

        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void EmptyEmfPlusCommentIsRejectedAsInvalid()
    {
        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(Record(70, 4, 0x2B464D45)), 32, 32));
    }

    [Fact]
    public void GdiOnlyEmfPlusProcessesOnlyTheGetDcScope()
    {
        var emf = Build(PlusRecord(0x4001, 1, 0xDBC01002, 0, 96, 96), Record(15, 1, 1, 0x0000FF),
            PlusRecord(0x4004), Record(15, 4, 4, 0x0000FF),
            PlusRecord(0x402B), Record(15, 8, 8, 0xFF0000), PlusRecord(0x4002));
        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);
        AssertPixel(pixels, 32, 1, 1, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 4, 4, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 8, 8, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData("drawing")]
    [InlineData("repeated-dc")]
    public void UnsupportedEmfPlusSemanticsDoNotReturnPartialPictures(string feature)
    {
        var unsupported = feature == "drawing" ? PlusRecord(0x4009, 0, 0xFFFFFFFF) : PlusRecord(0x4004);
        var emf = Build(PlusRecord(0x4001, 1, 0xDBC01002, 0, 96, 96), PlusRecord(0x4004),
            Record(15, 4, 4, 0x0000FF), unsupported, PlusRecord(0x4002));
        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Theory]
    [InlineData("missing-header")]
    [InlineData("missing-eof")]
    [InlineData("bad-size")]
    public void RejectsMalformedEmfPlusStreams(string defect)
    {
        var header = PlusRecord(0x4001, 1, 0xDBC01002, 0, 96, 96);
        if (defect == "bad-size") Set(header, 20, uint.MaxValue);
        var records = new List<byte[]>();
        if (defect != "missing-header") records.Add(header);
        records.Add(PlusRecord(0x4004));
        if (defect != "missing-eof") records.Add(PlusRecord(0x4002));
        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(records.ToArray()), 32, 32));
    }

    private static byte[] PlusRecord(ushort type, ushort flags = 0, params uint[] data) =>
        Record(70, new uint[] { (uint)(16 + data.Length * 4), 0x2B464D45,
            type | (uint)flags << 16, (uint)(12 + data.Length * 4), (uint)data.Length * 4 }.Concat(data).ToArray());

    [Theory]
    [InlineData(83)]
    [InlineData(78)]
    [InlineData(76)]
    public void UnsupportedRecordsDoNotProducePartialImages(uint recordType)
    {
        var emf = Build(Record(15, 1, 1, 0x0000FF), Record(recordType));

        var exception = Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));

        Assert.Contains(recordType.ToString(), exception.Message);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("truncated")]
    [InlineData("missing-eof")]
    [InlineData("count")]
    [InlineData("handles")]
    [InlineData("zero-extent")]
    [InlineData("restore")]
    public void RejectsMalformedInputs(string defect)
    {
        var emf = defect switch
        {
            "truncated" => Build(Record(15, 1)),
            "handles" => Build(Record(37, 3)),
            "zero-extent" => Build(Record(9, 0, 32)),
            "restore" => Build(Record(34, unchecked((uint)-1))),
            _ => Build(Record(15, 1, 1, 0x0000FF))
        };
        if (defect == "size") BinaryPrimitives.WriteUInt32LittleEndian(emf.AsSpan(92), uint.MaxValue);
        if (defect == "count") BinaryPrimitives.WriteUInt32LittleEndian(emf.AsSpan(52), 999);
        if (defect == "missing-eof")
        {
            Array.Resize(ref emf, emf.Length - 20);
            BinaryPrimitives.WriteUInt32LittleEndian(emf.AsSpan(48), (uint)emf.Length);
        }

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void RejectsZeroOutputDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmfVectorRenderer.Render(Build(), 0, 32));
    }

    [Fact]
    public void RejectsExcessiveDeviceContextNesting()
    {
        var emf = Build(Enumerable.Range(0, 1025).Select(_ => Record(33)).ToArray());

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void ClipsExtremeSetPixelCoordinatesBeforeIntegerConversion()
    {
        var emf = Build(
            Record(17, 8),
            Record(9, 1, 1),
            Record(11, int.MaxValue, int.MaxValue),
            Record(15, int.MaxValue, int.MaxValue, 0x0000FF),
            Record(15, 0, 0, 0xFF0000));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 0, 0, 0, 0, 255, 255);
        Assert.Equal(1, Enumerable.Range(0, 32 * 32).Count(index => pixels[index * 4 + 3] != 0));
    }

    [Theory]
    [InlineData(2, 8)]
    [InlineData(3, 12)]
    public void ComposesWorldTransformsInSpecifiedMultiplicationOrder(uint mode, int expectedLeft)
    {
        var emf = Build(Record(37, 0x80000008), Record(37, 0x80000004),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 4, 0 }),
            WorldRecord(36, new float[] { 2, 0, 0, 2, 0, 0 }, mode),
            Polygon16((2, 2), (4, 2), (4, 4), (2, 4)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, expectedLeft + 1, 5, 0, 0, 0, 255);
        AssertPixel(pixels, 32, expectedLeft - 1, 5, 0, 0, 0, 0);
    }

    [Fact]
    public void RotatesWorldCoordinatesBeforePageMapping()
    {
        var emf = Build(Record(37, 0x80000008), Record(37, 0x80000004),
            WorldRecord(35, new float[] { 0, 1, -1, 0, 20, 0 }),
            Polygon16((2, 2), (6, 2), (6, 6), (2, 6)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 15, 3, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 3, 3, 0, 0, 0, 0);
    }

    [Fact]
    public void RestoresWorldTransformWithDeviceContext()
    {
        var emf = Build(Record(37, 0x80000008), Record(37, 0x80000004),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 8, 4 }), Record(33),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 20, 20 }), Record(34, unchecked((uint)-1)),
            Polygon16((2, 2), (6, 2), (6, 6), (2, 6)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 11, 7, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 23, 23, 0, 0, 0, 0);
    }

    [Fact]
    public void IdentityModificationIgnoresUnusedTransformBytes()
    {
        var emf = Build(Record(37, 0x80000008), Record(37, 0x80000004),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 20, 20 }),
            WorldRecord(36, Enumerable.Repeat(float.NaN, 6).ToArray(), 1),
            Polygon16((2, 2), (6, 2), (6, 6), (2, 6)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 3, 3, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 23, 23, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsRotatedBitmapAndRectangularClipRatherThanUsingIncorrectBounds(bool bitmap)
    {
        var operation = bitmap ? BitmapRecord(24, -2, ColorPixels(24, true, false)) : Record(30, 0, 0, 12, 12);
        var emf = Build(WorldRecord(35, new float[] { 0, 1, -1, 0, 20, 0 }), operation);

        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsSingularOrNonfiniteWorldTransforms(bool nonfinite)
    {
        var emf = Build(WorldRecord(35, nonfinite
            ? new float[] { float.PositiveInfinity, 0, 0, 1, 0, 0 }
            : new float[] { 1, 1, 1, 1, 0, 0 }));

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void CapturingAPathDoesNotImmediatelyPaintIt()
    {
        var emf = Build(Record(59), Record(27, 4, 4), Record(54, 28, 4),
            Record(54, 28, 28), Record(60));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        Assert.All(pixels, value => Assert.Equal(0, value));
    }

    [Theory]
    [InlineData(62)]
    [InlineData(63)]
    [InlineData(64)]
    public void PathPaintingDistinguishesFillStrokeAndImplicitClosure(uint operation)
    {
        var emf = Build(Record(39, 1, 0, 0x0000FF, 0), Record(37, 1),
            Record(38, 2, 0, 0, 0, 0xFF0000), Record(37, 2),
            Record(59), Record(27, 4, 4), Record(54, 28, 4), Record(54, 28, 28),
            Record(54, 4, 28), Record(60), Record(operation, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 16, operation == 64 ? (byte)0 : (byte)255, 0, 0, operation == 64 ? (byte)0 : (byte)255);
        AssertPixel(pixels, 32, 16, 4, operation == 62 ? (byte)255 : (byte)0, 0, operation == 62 ? (byte)0 : (byte)255, 255);
        if (operation == 64) AssertPixel(pixels, 32, 4, 16, 0, 0, 0, 0);
        else AssertPixel(pixels, 32, 4, 16, operation == 62 ? (byte)255 : (byte)0, 0, operation == 62 ? (byte)0 : (byte)255, 255);
    }

    [Fact]
    public void PathPointsRemainInDeviceSpaceAfterTransformChanges()
    {
        var emf = Build(Record(37, 0x80000004), Record(59),
            Record(27, 4, 4), Record(54, 12, 4), Record(54, 12, 12), Record(54, 4, 12), Record(61),
            Record(60), WorldRecord(35, new float[] { 1, 0, 0, 1, 16, 16 }), Record(62, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 6, 6, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 22, 22, 0, 0, 0, 0);
    }

    [Fact]
    public void TransformChangeBeforeFirstPathSegmentUsesTheTransformedLogicalCurrentPosition()
    {
        var emf = Build(Record(37, 0x80000004), Record(59), Record(27, 4, 4),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 8, 0 }),
            Record(54, 12, 4), Record(54, 12, 12), Record(54, 4, 12), Record(61), Record(60),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 0, 0 }), Record(62, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 5, 5, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 13, 5, 0, 0, 0, 255);
    }

    [Fact]
    public void TransformChangeAfterPathSegmentBeginsANewFigureInsteadOfBridgingDevicePositions()
    {
        var emf = Build(Record(59), Record(27, 4, 4), Record(54, 12, 4),
            WorldRecord(35, new float[] { 1, 0, 0, 1, 8, 0 }),
            Record(54, 12, 12), Record(60), WorldRecord(35, new float[] { 1, 0, 0, 1, 0, 0 }),
            Record(64, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 4, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 20, 8, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 8, 0, 0, 0, 0);
    }

    [Fact]
    public void CosmeticPenRemainsOneDevicePixelWideWhenOutputIsScaled()
    {
        var emf = Build(Record(27, 4, 4), Record(54, 28, 4));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 128, 128), 128, 128);

        AssertPixel(pixels, 128, 32, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 128, 32, 15, 0, 0, 0, 0);
        AssertPixel(pixels, 128, 32, 17, 0, 0, 0, 0);
        Assert.Equal(96, Enumerable.Range(0, 128 * 128).Count(index => pixels[index * 4 + 3] != 0));
    }

    [Fact]
    public void ClipsCosmeticLineBeforeRasterizingExtremeIntegerCoordinates()
    {
        var emf = Build(Record(27, unchecked((uint)int.MinValue), 16), Record(54, int.MaxValue, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 0, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 31, 16, 0, 0, 0, 255);
        Assert.Equal(32, Enumerable.Range(0, 32 * 32).Count(index => pixels[index * 4 + 3] != 0));
    }

    [Fact]
    public void RejectsStrokeCoordinatesBeyondTheClippingPrecisionLimit()
    {
        var emf = Build(WorldRecord(35, new float[] { 1e20f, 0, 0, 1, 0, 0 }), Record(54, 28, 4));

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void PreservesCollinearBezierExcursionsWhenStartAndEndCoincide()
    {
        var emf = Build(PointsRecord(2, (8, 16), (28, 16), (-12, 16), (8, 16)));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 3, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 13, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 16, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void FillsMultiplePathFiguresTogetherSoHolesRespectTheFillRule(uint fillMode, bool filled)
    {
        var emf = Build(Record(37, 0x80000004), Record(19, fillMode), Record(59),
            Polygon16((4, 4), (28, 4), (28, 28), (4, 28)),
            Polygon16((10, 10), (22, 10), (22, 22), (10, 22)),
            Record(60), Record(62, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 16, 0, 0, 0, filled ? (byte)255 : (byte)0);
        AssertPixel(pixels, 32, 6, 16, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(85)]
    [InlineData(88)]
    public void Flattens32And16BitCubicPathsWithoutFillingTheirControlPolygon(uint type)
    {
        var continues = type is 5 or 88;
        var points = continues
            ? new[] { (4, 4), (28, 4), (28, 24) }
            : new[] { (4, 24), (4, 4), (28, 4), (28, 24) };
        var emf = Build(Record(37, 0x80000004), Record(59), Record(27, 4, 24),
            PointsRecord(type, points), Record(60), Record(62, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 8, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 16, 10, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 25, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(89)]
    public void PolylineToUpdatesCurrentPositionForSubsequentDrawing(uint type)
    {
        var emf = Build(Record(27, 4, 4), PointsRecord(type, (28, 4), (28, 28)), Record(54, 4, 28));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 4, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 28, 16, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 28, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 4, 16, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BeginAndAbortDiscardEarlierPaths(bool abort)
    {
        var records = new List<byte[]> { Record(37, 0x80000004), Record(59), Polygon16((4, 4), (12, 4), (4, 12)) };
        if (abort) records.Add(Record(68));
        records.AddRange(new[] { Record(59), Polygon16((20, 20), (28, 20), (20, 28)), Record(60), Record(62, 0, 0, 32, 32) });

        var pixels = Decode(EmfVectorRenderer.Render(Build(records.ToArray()), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 5, 5, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 21, 21, 0, 0, 0, 255);
    }

    [Fact]
    public void ClosingFigureDoesNotMoveCurrentPositionAndSubsequentLineStartsANewFigure()
    {
        var emf = Build(Record(59), Record(27, 4, 4), Record(54, 12, 4), Record(54, 12, 12),
            Record(61), Record(54, 20, 12), Record(60), Record(64, 0, 0, 32, 32));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 16, 12, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 16, 8, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData("end")]
    [InlineData("close")]
    [InlineData("paint-open")]
    [InlineData("paint-twice")]
    [InlineData("curve-count")]
    public void RejectsInvalidPathSequencesAndCubicPointCounts(string defect)
    {
        var records = defect switch
        {
            "end" => new[] { Record(60) },
            "close" => new[] { Record(59), Record(61) },
            "paint-open" => new[] { Record(59), Record(62, 0, 0, 32, 32) },
            "paint-twice" => new[] { Record(59), Record(60), Record(62, 0, 0, 32, 32), Record(62, 0, 0, 32, 32) },
            _ => new[] { PointsRecord(5, (4, 4), (12, 12)) }
        };

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(records), 32, 32));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsUnimplementedDeviceContextSavingWithPendingPath(bool restore)
    {
        var records = restore
            ? new[] { Record(33), Record(59), Record(34, unchecked((uint)-1)) }
            : new[] { Record(59), Record(33) };

        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(Build(records), 32, 32));
    }

    private static byte[] WorldRecord(uint type, float[] values, uint mode = 0)
    {
        var bytes = new byte[type == 36 ? 36 : 32];
        Set(bytes, 0, type);
        Set(bytes, 4, (uint)bytes.Length);
        for (var index = 0; index < 6; index++)
            BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(8 + index * 4), values[index]);
        if (type == 36) Set(bytes, 32, mode);
        return bytes;
    }

    private static byte[] PointsRecord(uint type, params (int X, int Y)[] points)
    {
        var stride = type >= 85 ? 4 : 8;
        var bytes = new byte[28 + points.Length * stride];
        Set(bytes, 0, type);
        Set(bytes, 4, (uint)bytes.Length);
        Set(bytes, 24, (uint)points.Length);
        for (var index = 0; index < points.Length; index++)
        {
            if (stride == 4)
            {
                BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(28 + index * stride), checked((short)points[index].X));
                BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(30 + index * stride), checked((short)points[index].Y));
            }
            else
            {
                Set(bytes, 28 + index * stride, unchecked((uint)points[index].X));
                Set(bytes, 32 + index * stride, unchecked((uint)points[index].Y));
            }
        }
        return bytes;
    }

    [Theory]
    [InlineData(16, false, false)]
    [InlineData(16, true, false)]
    [InlineData(16, false, true)]
    [InlineData(16, true, true)]
    [InlineData(24, false, false)]
    [InlineData(24, true, false)]
    [InlineData(32, false, false)]
    [InlineData(32, true, false)]
    [InlineData(32, false, true)]
    [InlineData(32, true, true)]
    public void RendersDibColorsRowOrientationAndPadding(int bitCount, bool topDown, bool bitfields)
    {
        uint[]? masks = !bitfields ? null : bitCount == 16
            ? new uint[] { 0xF800, 0x07E0, 0x001F }
            : new uint[] { 0xFF0000, 0x00FF00, 0x0000FF };
        var bitmap = BitmapRecord(bitCount, topDown ? -2 : 2, ColorPixels(bitCount, topDown, bitfields), masks: masks);
        var emf = Build(Record(21, 3), bitmap);

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 6, 6, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 17, 6, 0, 255, 0, 255);
        AssertPixel(pixels, 32, 6, 17, 0, 0, 255, 255);
        AssertPixel(pixels, 32, 17, 17, 255, 255, 255, 255);
        AssertPixel(pixels, 32, 2, 2, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CropsDibSourceFromBottomRegardlessOfStorageDirection(bool topDown)
    {
        var bitmap = BitmapRecord(24, topDown ? -2 : 2, ColorPixels(24, topDown, false));
        Set(bitmap, 36, 0);
        Set(bitmap, 44, 1);
        var emf = Build(Record(21, 3), bitmap);

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 6, 6, 0, 0, 255, 255);
        AssertPixel(pixels, 32, 17, 6, 255, 255, 255, 255);
    }

    [Fact]
    public void AppliesDibMirroringMappingAndClip()
    {
        var bitmap = BitmapRecord(24, -2, ColorPixels(24, true, false));
        Set(bitmap, 24, 40);
        Set(bitmap, 28, 8);
        Set(bitmap, 72, unchecked((uint)-32));
        Set(bitmap, 76, 32);
        var emf = Build(Record(17, 8), Record(9, 64, 64), Record(11, 32, 32),
            Record(30, 16, 8, 40, 32), Record(21, 3), bitmap);

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 6, 0, 255, 0, 255);
        AssertPixel(pixels, 32, 17, 6, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 7, 6, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 8, 16, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MirroredDibExtentsIncludeStartPixelAndExcludeEndPixel(bool horizontal)
    {
        var bitmap = BitmapRecord(24, -2, ColorPixels(24, true, false));
        Set(bitmap, horizontal ? 24 : 28, 20);
        Set(bitmap, horizontal ? 72 : 76, unchecked((uint)-16));

        var pixels = Decode(EmfVectorRenderer.Render(Build(Record(21, 3), bitmap), 32, 32), 32, 32);

        void Check(int coordinate, params byte[] expected) =>
            AssertPixel(pixels, 32, horizontal ? coordinate : 6, horizontal ? 6 : coordinate, expected);
        Check(4, 0, 0, 0, 0);
        Check(5, 0, horizontal ? (byte)255 : (byte)0, horizontal ? (byte)0 : (byte)255, 255);
        Check(12, 0, horizontal ? (byte)255 : (byte)0, horizontal ? (byte)0 : (byte)255, 255);
        Check(13, 255, 0, 0, 255);
        Check(20, 255, 0, 0, 255);
        Check(21, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AlphaBlendUsesTopLeftSourceDcCoordinatesRegardlessOfDibStorage(bool topDown)
    {
        var bitmap = BitmapRecord(32, topDown ? -2 : 2, ColorPixels(32, topDown, false), alphaBlend: true);
        Set(bitmap, 40, 0x00FF0000);
        Set(bitmap, 104, 1);

        var pixels = Decode(EmfVectorRenderer.Render(Build(bitmap), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 6, 6, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 17, 6, 0, 255, 0, 255);
    }

    [Fact]
    public void NormalizesNonstandardContiguousDibMasks()
    {
        var bitmap = BitmapRecord(16, -1, new byte[] { 16, 0, 0, 0 },
            masks: new uint[] { 0x001F, 0x03E0, 0x7C00 }, imageWidth: 1);
        var pixels = Decode(EmfVectorRenderer.Render(Build(Record(21, 3), bitmap), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 8, 132, 0, 0, 255);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(128, false)]
    [InlineData(255, false)]
    [InlineData(0, true)]
    [InlineData(128, true)]
    [InlineData(255, true)]
    public void CombinesPremultipliedPixelAndConstantAlpha(int constantAlpha, bool opaqueBackground)
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 128, 128 }, alphaBlend: true,
            masks: new uint[] { 0xFF0000, 0x00FF00, 0x0000FF }, imageWidth: 1);
        Set(bitmap, 40, 0x01000000 | (uint)constantAlpha << 16);
        var records = new List<byte[]>();
        if (opaqueBackground)
        {
            records.Add(Record(39, 1, 0, 0xFF0000, 0));
            records.Add(Record(37, 1));
            records.Add(Record(37, 0x80000008));
            records.Add(Record(43, 0, 0, 32, 32));
        }
        records.Add(bitmap);
        var pixels = Decode(EmfVectorRenderer.Render(Build(records.ToArray()), 32, 32), 32, 32);

        var alpha = (byte)Math.Round(128 * constantAlpha / 255d);
        if (opaqueBackground)
            AssertPixel(pixels, 32, 8, 8, alpha, 0, (byte)(255 - alpha), 255);
        else
            AssertPixel(pixels, 32, 8, 8, alpha == 0 ? (byte)0 : (byte)255, 0, 0, alpha);
    }

    [Fact]
    public void CompositesMultipleLayersWithoutTreatingStraightPngChannelsAsPremultiplied()
    {
        var red = BitmapRecord(32, -1, new byte[] { 0, 0, 128, 128 }, alphaBlend: true, imageWidth: 1);
        var blue = BitmapRecord(32, -1, new byte[] { 64, 0, 0, 64 }, alphaBlend: true, imageWidth: 1);

        var pixels = Decode(EmfVectorRenderer.Render(Build(red, blue), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 8, 153, 0, 102, 160);
    }

    [Fact]
    public void IgnoresReservedDibAlphaWhenPerPixelAlphaFlagIsAbsent()
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 255, 0 }, alphaBlend: true, imageWidth: 1);
        Set(bitmap, 40, 0x00800000);

        var pixels = Decode(EmfVectorRenderer.Render(Build(bitmap), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 8, 8, 255, 0, 0, 128);
    }

    [Theory]
    [InlineData(128, false)]
    [InlineData(128, true)]
    [InlineData(255, false)]
    [InlineData(255, true)]
    public void ReservedBlendFlagsPreserveConstantAndPerPixelAlpha(int flags, bool perPixelAlpha)
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 64, 128 }, alphaBlend: true, imageWidth: 1);
        var blend = 0x00800000u | (perPixelAlpha ? 0x01000000u : 0);
        Set(bitmap, 40, blend);
        var expected = EmfVectorRenderer.Render(Build(bitmap), 32, 32);

        Set(bitmap, 40, blend | (uint)flags << 8);
        var actual = EmfVectorRenderer.Render(Build(bitmap), 32, 32);

        Assert.Equal(expected, actual);
        AssertPixel(Decode(actual, 32, 32), 32, 8, 8,
            perPixelAlpha ? (byte)128 : (byte)64, 0, 0, perPixelAlpha ? (byte)64 : (byte)128);
    }

    [Theory]
    [InlineData("depth")]
    [InlineData("compression")]
    [InlineData("rop")]
    [InlineData("stretch-mode")]
    [InlineData("blend-operation")]
    [InlineData("alpha-format")]
    [InlineData("transform")]
    public void RejectsUnsupportedBitmapFeaturesInsteadOfReturningPartialImages(string feature)
    {
        var alphaBlend = feature is "blend-operation" or "alpha-format" or "transform";
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 0, 255 }, alphaBlend: alphaBlend, imageWidth: 1);
        var info = alphaBlend ? 108 : 80;
        if (feature == "depth") BinaryPrimitives.WriteUInt16LittleEndian(bitmap.AsSpan(info + 14), 8);
        if (feature == "compression") Set(bitmap, info + 16, 1);
        if (feature == "rop") Set(bitmap, 68, 0x00660046);
        if (feature == "blend-operation") Set(bitmap, 40, 0x01FF0001);
        if (feature == "alpha-format") Set(bitmap, 40, 0x02FF0000);
        if (feature == "transform") Set(bitmap, 52, 0x40000000);
        var emf = feature == "stretch-mode" ? Build(bitmap) : Build(Record(21, 3), bitmap);

        Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Theory]
    [InlineData("offset")]
    [InlineData("length")]
    [InlineData("overlap")]
    [InlineData("header")]
    [InlineData("dimensions")]
    [InlineData("planes")]
    [InlineData("pixels")]
    [InlineData("source-rectangle")]
    [InlineData("masks")]
    public void RejectsMalformedBitmapRangesAndMetadata(string defect)
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 0, 0 },
            masks: new uint[] { 0xFF0000, 0x00FF00, 0x0000FF }, imageWidth: 1);
        if (defect == "offset") Set(bitmap, 48, uint.MaxValue);
        if (defect == "length") Set(bitmap, 52, uint.MaxValue);
        if (defect == "overlap") Set(bitmap, 56, 80);
        if (defect == "header") Set(bitmap, 80, 124);
        if (defect == "dimensions") Set(bitmap, 88, 0x80000000);
        if (defect == "planes") BinaryPrimitives.WriteUInt16LittleEndian(bitmap.AsSpan(92), 2);
        if (defect == "pixels") Set(bitmap, 60, 1);
        if (defect == "source-rectangle") Set(bitmap, 32, int.MaxValue);
        if (defect == "masks") Set(bitmap, 120, 0x00FF00);

        Assert.Throws<InvalidDataException>(() =>
            EmfVectorRenderer.Render(Build(Record(21, 3), bitmap), 32, 32));
    }

    [Fact]
    public void RejectsNonpremultipliedAlphaSource()
    {
        var bitmap = BitmapRecord(32, -1, new byte[] { 0, 0, 255, 128 }, alphaBlend: true, imageWidth: 1);

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(bitmap), 32, 32));
    }

    [Theory]
    [InlineData(24u, 4u)]
    [InlineData(26u, 28u)]
    [InlineData(30u, 16u)]
    public void CompatibleScalePreservesExplicitAdvancesAndHorizontalAlignment(uint alignment, uint referenceX)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var expectedDefinition = FontRecord();
        Set(expectedDefinition, 16, 6);
        var expectedText = TextRecord(referenceX, 24, new ushort[] { 1, 1 }, 16, new[] { 12, 12 });
        var compatibleText = (byte[])expectedText.Clone();
        Set(compatibleText, 24, 1); Set(compatibleText, 28, 0x40000000);
        byte[] RenderText(byte[] definition, byte[] text) => EmfVectorRenderer.Render(
            Build(definition, Record(37, 1), Record(18, 1), Record(22, alignment), text), 32, 32, (_, _, _) => font);

        var expected = RenderText(expectedDefinition, expectedText);
        var actual = RenderText(FontRecord(), compatibleText);

        Assert.Equal(expected, actual);
        var pixels = Decode(actual, 32, 32);
        AssertPixel(pixels, 32, 5, 11, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 9, 11, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 17, 11, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(12u)]
    public void CompatibleScaleAdjustsDefaultAndRequestedFontWidthsAndNaturalAdvances(uint requestedWidth)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var definition = FontRecord(); Set(definition, 16, requestedWidth);
        var expectedDefinition = FontRecord(); Set(expectedDefinition, 16, 6);
        var expectedText = TextRecord(4, 24, new ushort[] { 1, 1 }, 16);
        var compatibleText = (byte[])expectedText.Clone();
        Set(compatibleText, 24, 1); Set(compatibleText, 28, 0x40000000);
        byte[] RenderText(byte[] face, byte[] text) => EmfVectorRenderer.Render(
            Build(face, Record(37, 1), Record(18, 1), Record(22, 24), text), 32, 32, (_, _, _) => font);

        Assert.Equal(RenderText(expectedDefinition, expectedText), RenderText(definition, compatibleText));
    }

    [Fact]
    public void CompatibleScalePreservesPairedAdvancesAndLocalClipping()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = TextRecord(4, 24, new ushort[] { 1, 1 }, 0x2014, new[] { 12, 2, 0, 0 });
        Set(text, 24, 1); Set(text, 28, 0x40000000);
        Set(text, 56, 4); Set(text, 60, 10); Set(text, 64, 21); Set(text, 68, 32);
        var emf = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), text, Record(15, 30, 30, 0xFF0000));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 17, 11, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 17, 13, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 21, 13, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 30, 30, 0, 0, 255, 255);
    }

    [Fact]
    public void AdvancedTextIgnoresCompatibleScaleFields()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = TextRecord(4, 24, new ushort[] { 1, 1 }, 16);
        var modified = (byte[])text.Clone();
        Set(modified, 28, 0x40000000); Set(modified, 32, 0);
        byte[] RenderText(byte[] record) => EmfVectorRenderer.Render(
            Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), record), 32, 32, (_, _, _) => font);

        Assert.Equal(RenderText(text), RenderText(modified));
    }

    [Theory]
    [InlineData(0f, 1f)]
    [InlineData(-1f, 1f)]
    [InlineData(1f, -1f)]
    [InlineData(float.NaN, 1f)]
    [InlineData(1f, float.PositiveInfinity)]
    public void InvalidOrUnsupportedCompatibleScalesDoNotReturnPartialImages(float xScale, float yScale)
    {
        var text = TextRecord(0, 0, Array.Empty<ushort>(), 2);
        Set(text, 24, 1); Set(text, 28, BitConverter.SingleToUInt32Bits(xScale)); Set(text, 32, BitConverter.SingleToUInt32Bits(yScale));
        var emf = Build(Record(15, 4, 4, 0x0000FF), text);

        if (!float.IsFinite(xScale) || !float.IsFinite(yScale))
            Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32));
        else
            Assert.Throws<NotSupportedException>(() => EmfVectorRenderer.Render(emf, 32, 32));
    }

    [Fact]
    public void CompatibleOpaqueTextDoesNotScaleItsRectangleAndRestoresThePreviousClip()
    {
        var text = TextRecord(0, 0, Array.Empty<ushort>(), 6);
        Set(text, 24, 1); Set(text, 28, 0x40000000);
        Set(text, 56, 4); Set(text, 60, 5); Set(text, 64, 10); Set(text, 68, 12);
        var emf = Build(Record(30, 2, 2, 16, 16), Record(25, 0x00FF00), text,
            Record(15, 12, 5, 0xFF0000), Record(15, 20, 20, 0x0000FF));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32), 32, 32);

        AssertPixel(pixels, 32, 4, 5, 0, 255, 0, 255);
        AssertPixel(pixels, 32, 10, 5, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 12, 5, 0, 0, 255, 255);
        AssertPixel(pixels, 32, 20, 20, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData(false, 24, 4)]
    [InlineData(true, 26, 28)]
    [InlineData(true, 30, 16)]
    public void RendersIndependentGlyphContoursWithHolesAndTextAlignment(bool glyphIndices, uint alignment, uint referenceX)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var emf = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, alignment),
            Record(24, 0x0000FF), TextRecord(referenceX, 24, glyphIndices ? new ushort[] { 1, 1 } : new ushort[] { 65, 65 },
                glyphIndices ? 16u : 0u));
        var names = new List<string>();

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (name, _, _) => { names.Add(name); return font; }), 32, 32);

        Assert.Equal(new[] { "Synthetic" }, names);
        AssertPixel(pixels, 32, 5, 11, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 9, 17, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 17, 11, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 15, 11, 0, 0, 0, 0);
    }

    [Fact]
    public void PositiveFontHeightsUseCellMetricsForOutlinesAndAdvances()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var positive = FontRecord();
        Set(positive, 12, 24);
        var text = TextRecord(4, 24, new ushort[] { 1, 1 }, 16);
        byte[] RenderText(byte[] definition) => EmfVectorRenderer.Render(
            Build(definition, Record(37, 1), Record(18, 1), Record(22, 24), text),
            32, 32, (_, _, _) => font);

        var expected = Decode(RenderText(FontRecord()), 32, 32);
        var actual = Decode(RenderText(positive), 32, 32);

        Assert.Equal(expected, actual);
        AssertPixel(actual, 32, 5, 11, 0, 0, 0, 255);
        AssertPixel(actual, 32, 17, 11, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsUnusableCellMetricsWithoutReturningPartialImages(bool truncated)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var cell = TrueTypeGlyphReaderTests.TableOffset(font, "OS/2");
        TrueTypeGlyphReaderTests.Write16(font, cell + 74, 0);
        TrueTypeGlyphReaderTests.Write16(font, cell + 76, 0);
        if (truncated)
        {
            for (var entry = 12; entry < 12 + BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4)) * 16; entry += 16)
                if (System.Text.Encoding.ASCII.GetString(font, entry, 4) == "OS/2")
                    TrueTypeGlyphReaderTests.Write32(font, entry + 12, 76);
        }
        var definition = FontRecord();
        Set(definition, 12, 24);
        var emf = Build(Record(15, 1, 1, 0x0000FF), definition, Record(37, 1),
            Record(18, 1), Record(22, 24), TextRecord(4, 24, new ushort[] { 1 }, 16));

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font));
    }

    [Fact]
    public void NegativeCharacterHeightsDoNotRequireCellMetrics()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var os2 = TrueTypeGlyphReaderTests.TableOffset(font, "OS/2");
        TrueTypeGlyphReaderTests.Write16(font, os2 + 74, 0);
        TrueTypeGlyphReaderTests.Write16(font, os2 + 76, 0);
        var emf = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24),
            TextRecord(4, 24, new ushort[] { 1 }, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 5, 11, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 15, 11, 0, 0, 0, 0);
    }

    [Fact]
    public void RequestedFontWidthsScaleOutlinesButNotExplicitSpacing()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var definition = FontRecord();
        Set(definition, 12, 24);
        Set(definition, 16, 6);
        byte[] RenderText(int[]? advances) => Decode(EmfVectorRenderer.Render(
            Build(definition, Record(37, 1), Record(18, 1), Record(22, 24),
                TextRecord(4, 24, new ushort[] { 1, 1 }, 16, advances)),
            32, 32, (_, _, _) => font), 32, 32);

        var measured = RenderText(null);
        var positioned = RenderText(new[] { 12, 0 });

        AssertPixel(measured, 32, 8, 11, 0, 0, 0, 255);
        AssertPixel(measured, 32, 9, 11, 0, 0, 0, 0);
        AssertPixel(measured, 32, 11, 11, 0, 0, 0, 255);
        AssertPixel(positioned, 32, 11, 11, 0, 0, 0, 0);
        AssertPixel(positioned, 32, 17, 11, 0, 0, 0, 255);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsUnusableAverageWidthWhenCustomFontWidthIsRequested(int average)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        TrueTypeGlyphReaderTests.Write16(font, TrueTypeGlyphReaderTests.TableOffset(font, "OS/2") + 2, average);
        var definition = FontRecord();
        Set(definition, 16, 6);
        var emf = Build(Record(15, 1, 1, 0x0000FF), definition, Record(37, 1),
            Record(18, 1), Record(22, 24), TextRecord(4, 24, new ushort[] { 1 }, 16));

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font));
    }

    [Fact]
    public void RendersQuadraticContoursWithImplicitOffCurveEndpoints()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Simple(
            new[] { (0, 0, false), (250, 700, false), (500, 0, false) }));
        var emf = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24),
            TextRecord(4, 24, new ushort[] { 1 }, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 9, 17, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 4, 10, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 9, 25, 0, 0, 0, 0);
    }

    [Fact]
    public void AppliesPairedTextSpacingAndRestoresTextClipForLaterDrawing()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = TextRecord(4, 24, new ushort[] { 1, 1 }, 0x2014, new[] { 12, 2, 0, 0 });
        Set(text, 56, 4); Set(text, 60, 10); Set(text, 64, 21); Set(text, 68, 32);
        var emf = Build(FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24), text, Record(15, 30, 30, 0xFF0000));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 17, 11, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 17, 13, 0, 0, 0, 255);
        AssertPixel(pixels, 32, 23, 13, 0, 0, 0, 0);
        AssertPixel(pixels, 32, 30, 30, 0, 0, 255, 255);
    }

    [Fact]
    public void EmptyOpaqueTextFillsTheSpecifiedRectangleWithoutLoadingFonts()
    {
        var text = TextRecord(0, 0, Array.Empty<ushort>(), 2);
        Set(text, 56, 4); Set(text, 60, 5); Set(text, 64, 10); Set(text, 68, 12);

        var pixels = Decode(EmfVectorRenderer.Render(Build(Record(25, 0x00FF00), text), 32, 32), 32, 32);

        AssertPixel(pixels, 32, 4, 5, 0, 255, 0, 255);
        AssertPixel(pixels, 32, 10, 5, 0, 0, 0, 0);
    }

    [Fact]
    public void RestoresFontAndTextStateAlongWithTheDeviceContext()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var small = FontRecord();
        Set(small, 8, 2); Set(small, 12, unchecked((uint)-10));
        var emf = Build(FontRecord(), small, Record(37, 1), Record(18, 1), Record(22, 24), Record(24, 0x0000FF),
            Record(33), Record(37, 2), Record(24, 0xFF0000), Record(22, 26), Record(18, 2),
            Record(34, uint.MaxValue), TextRecord(4, 24, new ushort[] { 1 }, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32, (_, _, _) => font), 32, 32);

        AssertPixel(pixels, 32, 5, 11, 255, 0, 0, 255);
        AssertPixel(pixels, 32, 9, 17, 0, 0, 0, 0);
    }

    [Theory]
    [InlineData("string-range")]
    [InlineData("spacing-range")]
    [InlineData("count")]
    [InlineData("duplicate-handle")]
    [InlineData("glyph")]
    public void RejectsMalformedTextAndSharedObjectHandles(string defect)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var text = TextRecord(4, 24, new ushort[] { 1 }, 16, new[] { 12 });
        if (defect == "string-range") Set(text, 48, uint.MaxValue);
        if (defect == "spacing-range") Set(text, 72, uint.MaxValue);
        if (defect == "count") Set(text, 44, uint.MaxValue);
        if (defect == "glyph") BinaryPrimitives.WriteUInt16LittleEndian(text.AsSpan(76), 65535);
        var records = new List<byte[]> { FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24) };
        if (defect == "duplicate-handle") records.Add(Record(39, 1, 0, 0, 0));
        records.Add(text);

        Assert.Throws<InvalidDataException>(() => EmfVectorRenderer.Render(Build(records.ToArray()), 32, 32, (_, _, _) => font));
    }

    [Theory]
    [InlineData("resolver")]
    [InlineData("bottom-layout")]
    [InlineData("rtl-layout")]
    [InlineData("current-position")]
    [InlineData("shaping")]
    [InlineData("font-height")]
    [InlineData("text-options")]
    public void UnsupportedTextDoesNotReturnPartialImages(string defect)
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var definition = FontRecord();
        var text = TextRecord(4, 24, new ushort[] { 65 }, 0);
        if (defect == "font-height") Set(definition, 12, 0);
        if (defect == "shaping") BinaryPrimitives.WriteUInt16LittleEndian(text.AsSpan(76), 0x627);
        if (defect == "text-options") Set(text, 52, 0x80);
        var alignment = defect switch { "bottom-layout" => 8u, "rtl-layout" => 280u, "current-position" => 25u, _ => 24u };
        var emf = Build(Record(15, 1, 1, 0x0000FF), definition, Record(37, 1),
            Record(18, 1), Record(22, alignment), text);

        Assert.Throws<NotSupportedException>(() =>
            EmfVectorRenderer.Render(emf, 32, 32, defect == "resolver" ? null : (_, _, _) => font));
    }

    [Fact]
    public void ResettingTextAlignmentAfterDrawingDoesNotInvalidateTheImage()
    {
        var font = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var records = new[] { FontRecord(), Record(37, 1), Record(18, 1), Record(22, 24),
            TextRecord(4, 24, new ushort[] { 1 }, 16) };
        var expected = EmfVectorRenderer.Render(Build(records), 32, 32, (_, _, _) => font);
        var actual = EmfVectorRenderer.Render(Build(records.Concat(new[] { Record(22, 0) }).ToArray()),
            32, 32, (_, _, _) => font);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(4u)]
    [InlineData(16u)]
    [InlineData(32u)]
    public void RejectsInvalidTextAlignmentValues(uint alignment)
    {
        Assert.Throws<InvalidDataException>(() =>
            EmfVectorRenderer.Render(Build(Record(22, alignment)), 32, 32));
    }

    [Fact]
    public void ResolvesStyledFacesWithoutRewritingTheirNames()
    {
        var bytes = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        var definition = FontRecord();
        System.Text.Encoding.Unicode.GetBytes("Synthetic Bold\0").CopyTo(definition, 40);
        Set(definition, 28, 700);
        definition[32] = 1;
        var requests = new List<(string, bool, bool)>();
        var emf = Build(definition, Record(37, 1), Record(18, 1), Record(22, 24), TextRecord(4, 24, new ushort[] { 1 }, 16));

        var pixels = Decode(EmfVectorRenderer.Render(emf, 32, 32,
            (name, bold, italic) => { requests.Add((name, bold, italic)); return bytes; }), 32, 32);

        Assert.Equal(new[] { ("Synthetic Bold", true, true) }, requests);
        AssertPixel(pixels, 32, 5, 11, 0, 0, 0, 255);
    }

    internal static byte[] FontRecord(string face = "Synthetic")
    {
        var bytes = new byte[332];
        Set(bytes, 0, 82); Set(bytes, 4, (uint)bytes.Length); Set(bytes, 8, 1);
        Set(bytes, 12, unchecked((uint)-20)); Set(bytes, 28, 400);
        System.Text.Encoding.Unicode.GetBytes(face + "\0").CopyTo(bytes, 40);
        return bytes;
    }

    internal static byte[] TextRecord(uint x, uint y, ushort[] values, uint options, int[]? advances = null)
    {
        var spacingOffset = (76 + values.Length * 2 + 3) & ~3;
        var bytes = new byte[spacingOffset + (advances?.Length ?? 0) * 4];
        Set(bytes, 0, 84); Set(bytes, 4, (uint)bytes.Length); Set(bytes, 24, 2);
        Set(bytes, 28, 0x3F800000); Set(bytes, 32, 0x3F800000);
        Set(bytes, 36, x); Set(bytes, 40, y); Set(bytes, 44, (uint)values.Length);
        Set(bytes, 48, 76); Set(bytes, 52, options);
        Set(bytes, 72, advances == null ? 0u : (uint)spacingOffset);
        for (var index = 0; index < values.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(76 + index * 2), values[index]);
        if (advances != null)
            for (var index = 0; index < advances.Length; index++) Set(bytes, spacingOffset + index * 4, unchecked((uint)advances[index]));
        return bytes;
    }

    private static void Set(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);

    internal static byte[] BitmapRecord(int bitCount, int signedHeight, byte[] bits,
        bool alphaBlend = false, uint[]? masks = null, int imageWidth = 2)
    {
        var fixedSize = alphaBlend ? 108 : 80;
        var infoSize = 40 + (masks?.Length ?? 0) * 4;
        var bytes = new byte[(fixedSize + infoSize + bits.Length + 3) & ~3];
        Set(bytes, 0, alphaBlend ? 114u : 81u);
        Set(bytes, 4, (uint)bytes.Length);
        Set(bytes, 24, 4);
        Set(bytes, 28, 4);
        if (alphaBlend)
        {
            Set(bytes, 32, 16);
            Set(bytes, 36, 16);
            Set(bytes, 40, 0x01FF0000);
            Set(bytes, 52, 0x3F800000);
            Set(bytes, 64, 0x3F800000);
            Set(bytes, 100, (uint)imageWidth);
            Set(bytes, 104, (uint)Math.Abs(signedHeight));
        }
        else
        {
            Set(bytes, 40, (uint)imageWidth);
            Set(bytes, 44, (uint)Math.Abs(signedHeight));
            Set(bytes, 68, 0x00CC0020);
            Set(bytes, 72, 16);
            Set(bytes, 76, 16);
        }
        var fieldsOffset = alphaBlend ? 84 : 48;
        Set(bytes, fieldsOffset, (uint)fixedSize);
        Set(bytes, fieldsOffset + 4, (uint)infoSize);
        Set(bytes, fieldsOffset + 8, (uint)(fixedSize + infoSize));
        Set(bytes, fieldsOffset + 12, (uint)bits.Length);
        Set(bytes, fixedSize, 40);
        Set(bytes, fixedSize + 4, (uint)imageWidth);
        Set(bytes, fixedSize + 8, unchecked((uint)signedHeight));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(fixedSize + 12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(fixedSize + 14), (ushort)bitCount);
        Set(bytes, fixedSize + 16, masks == null ? 0u : 3u);
        Set(bytes, fixedSize + 20, (uint)bits.Length);
        if (masks != null)
            for (var index = 0; index < masks.Length; index++) Set(bytes, fixedSize + 40 + index * 4, masks[index]);
        bits.CopyTo(bytes, fixedSize + infoSize);
        return bytes;
    }

    private static byte[] ColorPixels(int bitCount, bool topDown, bool bitfields)
    {
        var colors = bitCount == 16
            ? new uint[] { bitfields ? 0xF800u : 0x7C00u, bitfields ? 0x07E0u : 0x03E0u, 0x001F, bitfields ? 0xFFFFu : 0x7FFFu }
            : new uint[] { 0xFF0000, 0x00FF00, 0x0000FF, 0xFFFFFF };
        var stride = (2 * bitCount + 31) / 32 * 4;
        var bits = new byte[stride * 2];
        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                var position = (topDown ? y : 1 - y) * stride + x * (bitCount / 8);
                var value = colors[y * 2 + x];
                for (var channel = 0; channel < bitCount / 8; channel++) bits[position + channel] = (byte)(value >> (channel * 8));
            }
        }
        return bits;
    }

    internal static byte[] Build(params byte[][] records)
    {
        var header = new byte[88];
        void Set(int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(offset), value);
        Set(0, 1);
        Set(4, 88);
        Set(16, 32);
        Set(20, 32);
        Set(32, 847);
        Set(36, 847);
        Set(40, 0x464D4520);
        Set(44, 0x10000);
        Set(48, (uint)(88 + records.Sum(record => record.Length) + 20));
        Set(52, (uint)(records.Length + 2));
        Set(56, 16);
        using var stream = new MemoryStream();
        stream.Write(header);
        foreach (var record in records) stream.Write(record);
        stream.Write(Record(14, 0, 0, 20));
        return stream.ToArray();
    }

    internal static byte[] Record(uint type, params uint[] values)
    {
        var bytes = new byte[8 + values.Length * 4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, type);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length);
        for (var index = 0; index < values.Length; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8 + index * 4), values[index]);
        return bytes;
    }

    private static byte[] Polygon16(params (short X, short Y)[] points)
    {
        var bytes = new byte[28 + points.Length * 4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, 86);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(24), (uint)points.Length);
        for (var index = 0; index < points.Length; index++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(28 + index * 4), points[index].X);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(30 + index * 4), points[index].Y);
        }
        return bytes;
    }

    internal static byte[] Decode(byte[] png, int width, int height)
    {
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png[..8]);
        Assert.Equal(width, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)));
        Assert.Equal(height, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20)));
        using var compressed = new MemoryStream();
        for (var offset = 8; offset < png.Length;)
        {
            var size = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset));
            var chunk = png.AsSpan(offset + 4, size + 4);
            uint crc = uint.MaxValue;
            foreach (var value in chunk)
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                    crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xEDB88320;
            }
            Assert.Equal(~crc, BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset + size + 8)));
            if (chunk[..4].SequenceEqual("IDAT"u8)) compressed.Write(png, offset + 8, size);
            offset += size + 12;
        }
        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        var filtered = new byte[height * (width * 4 + 1)];
        zlib.ReadExactly(filtered);
        Assert.Equal(-1, zlib.ReadByte());
        var pixels = new byte[width * height * 4];
        for (var row = 0; row < height; row++)
        {
            Assert.Equal(0, filtered[row * (width * 4 + 1)]);
            Buffer.BlockCopy(filtered, row * (width * 4 + 1) + 1, pixels, row * width * 4, width * 4);
        }
        return pixels;
    }

    private static void AssertPixel(byte[] pixels, int width, int x, int y, params byte[] expected)
    {
        Assert.Equal(expected, pixels.AsSpan((y * width + x) * 4, 4).ToArray());
    }
}
