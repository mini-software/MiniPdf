using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace MiniSoftware.Tests;

public class WmfVectorAdapterTests
{
    [Fact]
    public void ProductionPortableConversionRendersPlaceableWmf()
    {
        var png = DocxReader.TryConvertMetafileToPngPortable(CreateWmf(), 1, 1, rasterHeight: 32);
        Assert.NotNull(png);
        var pixels = EmfVectorRendererTests.Decode(png, 32, 32);
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, pixels.AsSpan((8 * 32 + 8) * 4, 4).ToArray());
        Assert.Equal(new byte[] { 0, 0, 255, 255 }, pixels.AsSpan((27 * 32 + 27) * 4, 4).ToArray());
    }

    [WindowsEmfTheory]
    [InlineData(32)]
    [InlineData(64)]
    public void PlaceableVectorWmfMatchesNativeConversion(int size)
    {
        var wmf = CreateWmf();
        var emf = WmfVectorAdapter.Convert(wmf, size, size);
        var actual = EmfVectorRendererTests.Decode(EmfVectorRenderer.Render(emf, size, size), size, size);
        var dc = CreateCompatibleDC(IntPtr.Zero);
        var native = IntPtr.Zero;
        try
        {
            Assert.NotEqual(IntPtr.Zero, dc);
            var picture = new MetafilePicture { MapMode = 8, Width = 847, Height = 847 };
            var bits = wmf.AsSpan(22).ToArray();
            native = SetWinMetaFileBits((uint)bits.Length, bits, dc, ref picture);
            Assert.NotEqual(IntPtr.Zero, native);
            var length = GetEnhMetaFileBits(native, 0, null);
            Assert.NotEqual(0u, length);
            var converted = new byte[length];
            Assert.Equal(length, GetEnhMetaFileBits(native, length, converted));
            var expected = EmfNativePlaybackTests.PlayNative(converted, size, size);
            EmfNativePlaybackTests.AssertPixelsEqual(actual, expected, size, size);
        }
        finally
        {
            if (native != IntPtr.Zero) DeleteEnhMetaFile(native);
            if (dc != IntPtr.Zero) DeleteDC(dc);
        }
    }

    [Theory]
    [InlineData("checksum")]
    [InlineData("length")]
    [InlineData("record")]
    public void MalformedWmfDoesNotReturnAnImage(string defect)
    {
        var data = CreateWmf();
        if (defect == "checksum") data[20] ^= 1;
        if (defect == "length") BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(28), uint.MaxValue);
        if (defect == "record") BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(40), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => WmfVectorAdapter.Convert(data, 32, 32));
    }

    private static byte[] CreateWmf()
    {
        byte[] Record(ushort function, params ushort[] parameters)
        {
            var record = new byte[6 + parameters.Length * 2];
            BinaryPrimitives.WriteUInt32LittleEndian(record, (uint)record.Length / 2);
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4), function);
            for (var index = 0; index < parameters.Length; index++)
                BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(6 + index * 2), parameters[index]);
            return record;
        }
        var records = new[] {
            Record(0x020B, 0, 0), Record(0x020C, 32, 32),
            Record(0x02FC, 0, 255, 0, 0), Record(0x012D, 0),
            Record(0x02FA, 5, 0, 0, 0, 0), Record(0x012D, 1), Record(0x041B, 24, 24, 4, 4),
            Record(0x012D, 0x8005), Record(0x012D, 0x8008), Record(0x01F0, 0), Record(0x01F0, 1),
            Record(0x02FC, 0, 0, 255, 0), Record(0x012D, 0), Record(0x041B, 30, 30, 26, 26), Record(0) };
        var result = new byte[40 + records.Sum(record => record.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(result, 0x9AC6CDD7);
        BinaryPrimitives.WriteInt16LittleEndian(result.AsSpan(10), 32);
        BinaryPrimitives.WriteInt16LittleEndian(result.AsSpan(12), 32);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(14), 96);
        ushort checksum = 0;
        for (var index = 0; index < 20; index += 2) checksum ^= BinaryPrimitives.ReadUInt16LittleEndian(result.AsSpan(index));
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(20), checksum);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(22), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(24), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(26), 0x300);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(28), (uint)(result.Length - 22) / 2);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(32), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(34), (uint)records.Max(record => record.Length) / 2);
        var offset = 40;
        foreach (var record in records) { record.CopyTo(result, offset); offset += record.Length; }
        return result;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MetafilePicture { internal int MapMode, Width, Height; internal IntPtr Metafile; }
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern bool DeleteEnhMetaFile(IntPtr metafile);
    [DllImport("gdi32.dll")] private static extern IntPtr SetWinMetaFileBits(uint size, byte[] data, IntPtr dc, ref MetafilePicture picture);
    [DllImport("gdi32.dll")] private static extern uint GetEnhMetaFileBits(IntPtr metafile, uint size, [Out] byte[]? data);
}
