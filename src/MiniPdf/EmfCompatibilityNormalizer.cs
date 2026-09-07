namespace MiniSoftware;

internal static class EmfCompatibilityNormalizer
{
    private const uint EmrHeader = 1;
    private const uint EmrEof = 14;
    private const uint EmrSetPixelV = 15;
    private const uint EmrMoveToEx = 27;
    private const uint EmrCreatePen = 38;
    private const uint EmrLineTo = 54;
    private const uint EmrComment = 70;
    private const uint EmrExtCreateFontIndirectW = 82;
    private const uint EmrExtTextOutW = 84;
    private const uint EmrPolygon16 = 86;
    private const uint EmrPolyline16 = 87;
    private const uint EmfPlusIdentifier = 0x2B464D45;

    internal static double GetCanvasAspectRatio(byte[] source, long fallbackWidth, long fallbackHeight)
    {
        if (source.Length >= 40 && ReadUInt32(source, 0) == EmrHeader)
        {
            var width = Math.Abs((long)ReadInt32(source, 32) - ReadInt32(source, 24));
            var height = Math.Abs((long)ReadInt32(source, 36) - ReadInt32(source, 28));
            if (width > 0 && height > 0)
                return (double)width / height;
        }

        return fallbackWidth > 0 && fallbackHeight > 0 ? (double)fallbackWidth / fallbackHeight : 1.0;
    }

    internal static byte[] NormalizeForMiniPdfDrawing(byte[] source, int targetWidth, int targetHeight)
    {
        if (source.Length < 88 || ReadUInt32(source, 0) != EmrHeader)
            return source;

        using var output = new MemoryStream(source.Length);
        long windowOriginX = ReadInt32(source, 8);
        long windowOriginY = ReadInt32(source, 12);
        var windowExtentX = Math.Max(1L, (long)ReadInt32(source, 16) - windowOriginX);
        var windowExtentY = Math.Max(1L, (long)ReadInt32(source, 20) - windowOriginY);
        long viewportOriginX = 0;
        long viewportOriginY = 0;
        long viewportExtentX = targetWidth;
        long viewportExtentY = targetHeight;
        var offset = 0;
        var recordCount = 0u;

        while (offset + 8 <= source.Length)
        {
            var type = ReadUInt32(source, offset);
            var size = ReadUInt32(source, offset + 4);
            if (size < 8 || size > int.MaxValue || offset + (long)size > source.Length)
                return source;

            var record = new byte[(int)size];
            Buffer.BlockCopy(source, offset, record, 0, record.Length);

            switch (type)
            {
                case 9 when record.Length >= 16: // EMR_SETWINDOWEXTEX
                    windowExtentX = ReadInt32(record, 8);
                    windowExtentY = ReadInt32(record, 12);
                    break;
                case 10 when record.Length >= 16: // EMR_SETWINDOWORGEX
                    windowOriginX = ReadInt32(record, 8);
                    windowOriginY = ReadInt32(record, 12);
                    break;
                case 11 when record.Length >= 16: // EMR_SETVIEWPORTEXTEX
                    viewportExtentX = ReadInt32(record, 8);
                    viewportExtentY = ReadInt32(record, 12);
                    break;
                case 12 when record.Length >= 16: // EMR_SETVIEWPORTORGEX
                    viewportOriginX = ReadInt32(record, 8);
                    viewportOriginY = ReadInt32(record, 12);
                    break;
                case EmrSetPixelV:
                case EmrMoveToEx:
                case EmrLineTo:
                    TransformPoint(record, 8);
                    break;
                case EmrCreatePen:
                    NormalizeCreatePen(record);
                    break;
                case EmrExtCreateFontIndirectW:
                    NormalizeFont(record);
                    break;
                case EmrExtTextOutW:
                    NormalizeText(record);
                    break;
                case EmrPolygon16:
                case EmrPolyline16:
                    if (record.Length < 28) break;
                    TransformRectangle(record, 8);
                    var pointCount = ReadUInt32(record, 24);
                    if (pointCount <= (record.Length - 28) / 4)
                    {
                        for (var pointIndex = 0; pointIndex < pointCount; pointIndex++)
                            TransformPoint16(record, 28 + pointIndex * 4);
                    }
                    break;
            }

            var isMappingRecord = type is 9 or 10 or 11 or 12;
            var isNonEmfPlusComment = type == EmrComment
                && (record.Length < 16 || ReadUInt32(record, 12) != EmfPlusIdentifier);
            if (!isMappingRecord && !isNonEmfPlusComment)
            {
                output.Write(record, 0, record.Length);
                recordCount++;
            }

            offset += record.Length;
            if (type == EmrEof) break;
        }

        var normalized = output.ToArray();
        if (normalized.Length >= 56)
        {
            WriteInt32(normalized, 8, 0);
            WriteInt32(normalized, 12, 0);
            WriteInt32(normalized, 16, targetWidth);
            WriteInt32(normalized, 20, targetHeight);
            WriteUInt32(normalized, 48, (uint)normalized.Length);
            WriteUInt32(normalized, 52, recordCount);
        }
        return normalized;

        int MapX(int value)
        {
            var deviceValue = windowExtentX == 0
                ? value
                : viewportOriginX + ((long)value - windowOriginX) * (double)viewportExtentX / windowExtentX;
            var canvasWidth = Math.Max(1d, Math.Abs((double)viewportExtentX));
            return ClampInt32(deviceValue * targetWidth / canvasWidth);
        }

        int MapY(int value)
        {
            var deviceValue = windowExtentY == 0
                ? value
                : viewportOriginY + ((long)value - windowOriginY) * (double)viewportExtentY / windowExtentY;
            var canvasHeight = Math.Max(1d, Math.Abs((double)viewportExtentY));
            return ClampInt32(deviceValue * targetHeight / canvasHeight);
        }

        void TransformPoint(byte[] bytes, int position)
        {
            if (position + 8 > bytes.Length) return;
            WriteInt32(bytes, position, MapX(ReadInt32(bytes, position)));
            WriteInt32(bytes, position + 4, MapY(ReadInt32(bytes, position + 4)));
        }

        void TransformPoint16(byte[] bytes, int position)
        {
            if (position + 4 > bytes.Length) return;
            WriteInt16(bytes, position, ClampInt16(MapX(ReadInt16(bytes, position))));
            WriteInt16(bytes, position + 2, ClampInt16(MapY(ReadInt16(bytes, position + 2))));
        }

        void TransformRectangle(byte[] bytes, int position)
        {
            TransformPoint(bytes, position);
            TransformPoint(bytes, position + 8);
        }

        void NormalizeCreatePen(byte[] bytes)
        {
            if (bytes.Length < 28) return;
            var style = ReadUInt32(bytes, 12);
            var width = Math.Max(1L, Math.Abs((long)MapX(ReadInt32(bytes, 16)) - MapX(0)));
            var color = ReadUInt32(bytes, 24);
            WriteUInt16(bytes, 12, (ushort)style);
            WriteInt16(bytes, 14, ClampInt16(width));
            WriteInt16(bytes, 16, 0);
            WriteUInt32(bytes, 18, color);
        }

        void NormalizeFont(byte[] bytes)
        {
            if (bytes.Length < 104) return;
            var height = MapY(ReadInt32(bytes, 12)) - MapY(0);
            var width = MapX(ReadInt32(bytes, 16)) - MapX(0);
            var escapement = ReadInt32(bytes, 20);
            var orientation = ReadInt32(bytes, 24);
            var weight = ReadInt32(bytes, 28);
            var italic = bytes[32];
            var underline = bytes[33];
            var strikeout = bytes[34];
            var characterSet = bytes[35];
            var outPrecision = bytes[36];
            var clipPrecision = bytes[37];
            var pitchAndFamily = bytes[39];

            WriteInt16(bytes, 12, ClampInt16(height));
            WriteInt16(bytes, 14, ClampInt16(width));
            WriteInt16(bytes, 16, ClampInt16(escapement));
            WriteInt16(bytes, 18, ClampInt16(orientation));
            WriteInt16(bytes, 20, ClampInt16(weight));
            bytes[22] = italic;
            bytes[23] = underline;
            bytes[24] = strikeout;
            bytes[25] = characterSet;
            bytes[26] = outPrecision;
            bytes[27] = clipPrecision;
            bytes[28] = pitchAndFamily;

            Array.Clear(bytes, 29, 32);
            var familyName = System.Text.Encoding.ASCII.GetBytes("Noto Sans SC");
            Buffer.BlockCopy(familyName, 0, bytes, 29, familyName.Length);
        }

        void NormalizeText(byte[] bytes)
        {
            if (bytes.Length < 76) return;
            TransformRectangle(bytes, 8);
            TransformPoint(bytes, 36);
            TransformRectangle(bytes, 56);

            var characterCount = ReadUInt32(bytes, 44);
            var stringOffset = ReadUInt32(bytes, 48);
            var stringByteCount = characterCount <= int.MaxValue / 2 ? (int)characterCount * 2 : 0;
            if (stringByteCount <= 0 || stringOffset > int.MaxValue ||
                stringOffset + (long)stringByteCount > bytes.Length || 76L + stringByteCount > bytes.Length)
                return;

            if (stringOffset != 76)
                Buffer.BlockCopy(bytes, (int)stringOffset, bytes, 76, stringByteCount);
            WriteUInt32(bytes, 44, (uint)stringByteCount);
            WriteUInt32(bytes, 48, 40);
            WriteUInt32(bytes, 72, (uint)(40 + stringByteCount));
        }
    }

    private static int ClampInt32(double value)
    {
        if (double.IsNaN(value)) return 0;
        if (value <= int.MinValue) return int.MinValue;
        if (value >= int.MaxValue) return int.MaxValue;
        return (int)Math.Round(value);
    }

    private static short ClampInt16(long value) => (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value));

    private static short ReadInt16(byte[] bytes, int offset) => BitConverter.ToInt16(bytes, offset);

    private static int ReadInt32(byte[] bytes, int offset) => BitConverter.ToInt32(bytes, offset);

    private static uint ReadUInt32(byte[] bytes, int offset) => BitConverter.ToUInt32(bytes, offset);

    private static void WriteInt16(byte[] bytes, int offset, short value) =>
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, sizeof(short));

    private static void WriteUInt16(byte[] bytes, int offset, ushort value) =>
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, sizeof(ushort));

    private static void WriteInt32(byte[] bytes, int offset, int value) =>
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, sizeof(int));

    private static void WriteUInt32(byte[] bytes, int offset, uint value) =>
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, sizeof(uint));
}