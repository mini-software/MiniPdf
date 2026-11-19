namespace MiniSoftware;

internal static class WmfVectorAdapter
{
    internal static bool IsWmf(byte[] data) => data.Length >= 18 &&
        (Read32(data, 0) == 0x9AC6CDD7 || (Read16(data, 2) == 9 && Read16(data, 4) == 0x300));

    internal static byte[] Convert(byte[] data, int fallbackWidth, int fallbackHeight)
    {
        var header = 0;
        int left = 0, top = 0, right = fallbackWidth, bottom = fallbackHeight;
        if (Read32(data, 0) == 0x9AC6CDD7)
        {
            Require(data, 0, 22);
            ushort checksum = 0;
            for (var offset = 0; offset < 20; offset += 2) checksum ^= Read16(data, offset);
            if (checksum != Read16(data, 20) || Read16(data, 14) == 0 || Read32(data, 16) != 0)
                throw new InvalidDataException("Invalid placeable WMF header.");
            left = Signed(data, 6); top = Signed(data, 8); right = Signed(data, 10); bottom = Signed(data, 12);
            header = 22;
        }
        Require(data, header, 18);
        var objects = Read16(data, header + 10);
        if (Read16(data, header) is not (1 or 2) || Read16(data, header + 2) != 9 ||
            Read16(data, header + 4) != 0x300 || (long)Read32(data, header + 6) * 2 != data.Length - header ||
            Read16(data, header + 16) != 0 || right <= left || bottom <= top || objects > 65534)
            throw new InvalidDataException("Invalid WMF header or dimensions.");
        var allocated = new bool[objects];
        var records = new List<byte[]> { Record(17, 8), Record(10, left, top), Record(9, right - left, bottom - top),
            Record(12, left, top), Record(11, right - left, bottom - top) };
        var ended = false;
        for (var offset = header + 18; offset < data.Length;)
        {
            if (records.Count > 1_000_000) throw new InvalidDataException("WMF record limit exceeded.");
            Require(data, offset, 6);
            var words = Read32(data, offset);
            if (words < 3 || words > Read32(data, header + 12) || (long)words * 2 > data.Length - offset)
                throw new InvalidDataException("Invalid WMF record size.");
            var end = offset + (int)words * 2;
            var function = Read16(data, offset + 4);
            var p = offset + 6;
            int I(int index) { if (p + index * 2 > end - 2) throw new InvalidDataException("Truncated WMF parameters."); return Signed(data, p + index * 2); }
            uint U(int index) { if (p + index * 2 > end - 2) throw new InvalidDataException("Truncated WMF parameters."); return Read16(data, p + index * 2); }
            uint Color(int index) { if (p + index * 2 > end - 4) throw new InvalidDataException("Truncated WMF color."); return Read32(data, p + index * 2); }
            int NewObject()
            {
                var index = Array.FindIndex(allocated, value => !value);
                if (index < 0) throw new InvalidDataException("WMF object table is full.");
                allocated[index] = true;
                return index + 1;
            }
            switch (function)
            {
                case 0:
                    if (words != 3 || end != data.Length) throw new InvalidDataException("Invalid WMF EOF.");
                    ended = true;
                    break;
                case 0x001E: records.Add(Record(33)); break;
                case 0x0127: records.Add(Record(34, I(0))); break;
                case 0x0102: records.Add(Record(18, (int)U(0))); break;
                case 0x0103: records.Add(Record(17, I(0))); break;
                case 0x0104: records.Add(Record(20, (int)U(0))); break;
                case 0x0106: records.Add(Record(19, (int)U(0))); break;
                case 0x012E: records.Add(Record(22, (int)U(0))); break;
                case 0x0201: records.Add(Record(25, unchecked((int)Color(0)))); break;
                case 0x0209: records.Add(Record(24, unchecked((int)Color(0)))); break;
                case 0x020B: records.Add(Record(10, I(1), I(0))); break;
                case 0x020C: records.Add(Record(9, I(1), I(0))); break;
                case 0x020D: records.Add(Record(12, I(1), I(0))); break;
                case 0x020E: records.Add(Record(11, I(1), I(0))); break;
                case 0x0214: records.Add(Record(27, I(1), I(0))); break;
                case 0x0213: records.Add(Record(54, I(1), I(0))); break;
                case 0x0416: records.Add(Record(30, I(3), I(2), I(1), I(0))); break;
                // Native WMF conversion subtracts one from the lower-right EMF rectangle coordinates.
                case 0x041B: records.Add(Record(43, I(3), I(2), I(1) - 1, I(0) - 1)); break;
                case 0x012D:
                case 0x01F0:
                    var selected = (int)U(0);
                    if (function == 0x012D && (selected & 0x8000) != 0)
                    {
                        records.Add(Record(37, unchecked((int)(0x80000000u | (uint)(selected & 0x7FFF)))));
                        break;
                    }
                    if (selected >= allocated.Length || !allocated[selected]) throw new InvalidDataException("Unknown WMF object.");
                    records.Add(Record(function == 0x012D ? 37 : 40, selected + 1));
                    if (function == 0x01F0) allocated[selected] = false;
                    break;
                case 0x02FA: records.Add(Record(38, NewObject(), (int)U(0), I(1), I(2), unchecked((int)Color(3)))); break;
                case 0x02FC: records.Add(Record(39, NewObject(), (int)U(0), unchecked((int)Color(1)), (int)U(3))); break;
                case 0x02FB:
                    if (end - p < 18) throw new InvalidDataException("Truncated WMF font.");
                    var font = new byte[332];
                    Write32(font, 0, 82); Write32(font, 4, (uint)font.Length); Write32(font, 8, (uint)NewObject());
                    for (var index = 0; index < 5; index++) Write32(font, 12 + index * 4, unchecked((uint)I(index)));
                    Buffer.BlockCopy(data, p + 10, font, 32, 8);
                    for (var index = 0; index < 31 && p + 18 + index < end && data[p + 18 + index] != 0; index++)
                    {
                        if (data[p + 18 + index] > 0x7F) throw new NotSupportedException("WMF font names require supported character-set decoding.");
                        font[40 + index * 2] = data[p + 18 + index];
                    }
                    records.Add(font);
                    break;
                case 0x0521:
                    var textLength = (int)U(0);
                    var reference = p + 2 + ((textLength + 1) & ~1);
                    if (reference > end - 4) throw new InvalidDataException("Truncated WMF text.");
                    var text = new byte[(76 + textLength * 2 + 3) & ~3];
                    Write32(text, 0, 84); Write32(text, 4, (uint)text.Length); Write32(text, 24, 2);
                    Write32(text, 28, 0x3F800000); Write32(text, 32, 0x3F800000);
                    Write32(text, 36, unchecked((uint)Signed(data, reference + 2))); Write32(text, 40, unchecked((uint)Signed(data, reference)));
                    Write32(text, 44, (uint)textLength); Write32(text, 48, 76);
                    for (var index = 0; index < textLength; index++)
                    {
                        if (data[p + 2 + index] > 0x7F) throw new NotSupportedException("WMF text requires supported character-set decoding.");
                        text[76 + index * 2] = data[p + 2 + index];
                    }
                    records.Add(text);
                    break;
                case 0x0324:
                case 0x0325:
                    var count = (int)U(0);
                    if (count < 2 || count > (end - p - 2) / 4) throw new InvalidDataException("Invalid WMF polygon.");
                    var points = new int[5 + count * 2];
                    points[4] = count;
                    for (var index = 0; index < count; index++)
                    {
                        points[5 + index * 2] = I(1 + index * 2);
                        points[6 + index * 2] = I(2 + index * 2);
                    }
                    records.Add(Record(function == 0x0324 ? 3 : 4, points));
                    break;
                default: throw new NotSupportedException($"WMF record 0x{function:X4} is not supported by the independent adapter.");
            }
            offset = end;
        }
        if (!ended) throw new InvalidDataException("Missing WMF EOF.");
        records.Add(Record(14, 0, 0, 20));
        var size = 88 + records.Sum(record => record.Length);
        var emf = new byte[size];
        Write32(emf, 0, 1); Write32(emf, 4, 88);
        Write32(emf, 8, (uint)left); Write32(emf, 12, (uint)top); Write32(emf, 16, (uint)right); Write32(emf, 20, (uint)bottom);
        Write32(emf, 40, 0x464D4520); Write32(emf, 44, 0x10000); Write32(emf, 48, (uint)size);
        Write32(emf, 52, (uint)records.Count + 1); Write32(emf, 56, (uint)objects + 1);
        var destination = 88;
        foreach (var record in records) { Buffer.BlockCopy(record, 0, emf, destination, record.Length); destination += record.Length; }
        return emf;
    }

    private static byte[] Record(int type, params int[] values)
    {
        var result = new byte[8 + values.Length * 4];
        Write32(result, 0, (uint)type); Write32(result, 4, (uint)result.Length);
        for (var index = 0; index < values.Length; index++) Write32(result, 8 + index * 4, unchecked((uint)values[index]));
        return result;
    }
    private static void Require(byte[] data, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > data.Length - length) throw new InvalidDataException("Truncated WMF data.");
    }
    private static ushort Read16(byte[] data, int offset)
    {
        Require(data, offset, 2); return (ushort)(data[offset] | data[offset + 1] << 8);
    }
    private static int Signed(byte[] data, int offset) => (short)Read16(data, offset);
    private static uint Read32(byte[] data, int offset)
    {
        Require(data, offset, 4); return (uint)(data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16 | data[offset + 3] << 24);
    }
    private static void Write32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value; data[offset + 1] = (byte)(value >> 8); data[offset + 2] = (byte)(value >> 16); data[offset + 3] = (byte)(value >> 24);
    }
}
