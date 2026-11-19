namespace MiniSoftware;

// Unhinted outlines decoded from the OpenType glyf/loca specification.
internal sealed class TrueTypeGlyphReader
{
    internal readonly struct Point
    {
        internal readonly double X, Y;
        internal readonly bool OnCurve;
        internal Point(double x, double y, bool onCurve) { X = x; Y = y; OnCurve = onCurve; }
    }

    private readonly byte[] data;
    private readonly Dictionary<string, (int Offset, int Length)> tables = new();
    private readonly int[] locations;
    private readonly ushort[] advances;
    private readonly short[] bearings;
    internal int UnitsPerEm { get; }
    internal int CellAscent
    {
        get { var os2 = Table("OS/2"); os2.Skip(74); return os2.U16(); }
    }
    internal bool NeedsSyntheticBold
    {
        get
        {
            var os2 = Table("OS/2");
            os2.Skip(4);
            var weight = os2.U16();
            os2.Skip(56);
            return weight > 0 && weight < 600 && (os2.U16() & 0x20) == 0;
        }
    }
    internal int CellHeight
    {
        get
        {
            var os2 = Table("OS/2");
            os2.Skip(74);
            var height = os2.U16() + os2.U16();
            if (height == 0) throw new InvalidDataException("Invalid TrueType Windows cell height.");
            return height;
        }
    }
    internal int AverageWidth
    {
        get
        {
            var os2 = Table("OS/2");
            os2.Skip(2);
            var average = os2.I16();
            if (average <= 0) throw new InvalidDataException("Invalid TrueType average character width.");
            return average;
        }
    }
    private int remainingWork;

    internal TrueTypeGlyphReader(byte[] data)
    {
        this.data = data ?? throw new ArgumentNullException(nameof(data));
        var directory = new Reader(data, 0, data.Length);
        if (directory.U32() != 0x00010000)
            throw new NotSupportedException("Metafile fonts require standalone TrueType glyf outlines.");
        var count = directory.U16();
        directory.Skip(6);
        for (var index = 0; index < count; index++)
        {
            var tag = new string(new[] { (char)directory.Byte(), (char)directory.Byte(),
                (char)directory.Byte(), (char)directory.Byte() });
            directory.Skip(4);
            var offset = directory.U32();
            var length = directory.U32();
            if (offset > data.Length || length > data.Length - offset || tables.ContainsKey(tag))
                throw new InvalidDataException("Invalid or duplicate TrueType table range.");
            tables.Add(tag, ((int)offset, (int)length));
        }
        var head = Table("head");
        head.Skip(18);
        UnitsPerEm = head.U16();
        if (UnitsPerEm < 16 || UnitsPerEm > 16384)
            throw new InvalidDataException("Invalid TrueType units per em.");
        head.Skip(30);
        var locationFormat = head.I16();
        if (locationFormat != 0 && locationFormat != 1)
            throw new InvalidDataException("Invalid TrueType loca format.");
        var maxp = Table("maxp");
        maxp.Skip(4);
        var glyphCount = maxp.U16();
        if (glyphCount == 0) throw new InvalidDataException("Empty TrueType glyph set.");
        var glyf = Table("glyf");
        var loca = Table("loca");
        locations = new int[glyphCount + 1];
        for (var index = 0; index < locations.Length; index++)
        {
            var relative = locationFormat == 0 ? (uint)loca.U16() * 2 : loca.U32();
            if (relative > glyf.Length || (index > 0 && relative < locations[index - 1]))
                throw new InvalidDataException("Invalid TrueType glyph location.");
            locations[index] = (int)relative;
        }
        var hhea = Table("hhea");
        hhea.Skip(34);
        var metricCount = hhea.U16();
        if (metricCount == 0 || metricCount > glyphCount)
            throw new InvalidDataException("Invalid TrueType horizontal metric count.");
        var hmtx = Table("hmtx");
        advances = new ushort[glyphCount];
        bearings = new short[glyphCount];
        for (var index = 0; index < glyphCount; index++)
        {
            advances[index] = index < metricCount ? hmtx.U16() : advances[index - 1];
            bearings[index] = hmtx.I16();
        }
    }

    internal int Advance(ushort glyph)
    {
        CheckGlyph(glyph);
        return advances[glyph];
    }

    internal ushort GlyphForCharacter(ushort character)
    {
        var cmap = Table("cmap");
        if (cmap.U16() != 0) throw new InvalidDataException("Invalid TrueType cmap version.");
        var count = cmap.U16();
        if (count > 256) throw new InvalidDataException("TrueType cmap subtable limit exceeded.");
        Reader? selected = null;
        var selectedFormat = 0;
        for (var index = 0; index < count; index++)
        {
            var platform = cmap.U16();
            var encoding = cmap.U16();
            var offset = cmap.U32();
            if (platform != 0 && !(platform == 3 && (encoding == 1 || encoding == 10))) continue;
            var subtable = Table("cmap").Slice(offset);
            var format = subtable.U16();
            if (format != 4 && format != 12) continue;
            if (format == 12) subtable.Skip(2);
            var length = format == 4 ? subtable.U16() : subtable.U32();
            var bounded = Table("cmap").Slice(offset, length);
            if (selected == null || format > selectedFormat) { selected = bounded; selectedFormat = format; }
        }
        if (selected == null) throw new NotSupportedException("No supported Unicode cmap in the metafile font.");
        var table = selected;
        ushort glyph = 0;
        if (selectedFormat == 12)
        {
            table.Skip(12);
            var groups = table.U32();
            if (groups > (table.Length - 16) / 12)
                throw new InvalidDataException("Invalid TrueType cmap group count.");
            var low = 0;
            var high = (int)groups - 1;
            while (low <= high)
            {
                var middle = low + (high - low) / 2;
                var group = table.At(16 + middle * 12);
                var start = group.U32();
                var end = group.U32();
                var firstGlyph = group.U32();
                if (start > end || end > 0x10FFFF)
                    throw new InvalidDataException("Invalid TrueType cmap character range.");
                if (character >= start && character <= end)
                {
                    var value = (ulong)firstGlyph + character - start;
                    if (value > ushort.MaxValue) throw new InvalidDataException("Invalid TrueType cmap glyph.");
                    glyph = (ushort)value;
                    break;
                }
                if (character < start) high = middle - 1;
                else low = middle + 1;
            }
        }
        else
        {
            table.Skip(6);
            var twiceCount = table.U16();
            var segments = twiceCount / 2;
            if (segments == 0 || twiceCount % 2 != 0 || 16 + segments * 8 > table.Length)
                throw new InvalidDataException("Invalid TrueType cmap segment count.");
            var low = 0;
            var high = segments - 1;
            while (low <= high)
            {
                var index = low + (high - low) / 2;
                var end = table.At(14 + index * 2).U16();
                var start = table.At(16 + segments * 2 + index * 2).U16();
                if (start > end) throw new InvalidDataException("Invalid TrueType cmap segment.");
                if (character < start) { high = index - 1; continue; }
                if (character > end) { low = index + 1; continue; }
                var delta = table.At(16 + segments * 4 + index * 2).I16();
                var rangePosition = 16 + segments * 6 + index * 2;
                var range = table.At(rangePosition).U16();
                glyph = range == 0 ? unchecked((ushort)(character + delta))
                    : table.At(rangePosition + range + (character - start) * 2).U16();
                if (range != 0 && glyph != 0) glyph = unchecked((ushort)(glyph + delta));
                break;
            }
        }
        CheckGlyph(glyph);
        if (glyph == 0) throw new NotSupportedException($"Metafile font has no glyph for U+{character:X4}.");
        return glyph;
    }

    internal List<List<Point>> ReadGlyph(ushort glyph)
    {
        remainingWork = 1_000_000;
        return ReadGlyph(glyph, new HashSet<ushort>());
    }

    private List<List<Point>> ReadGlyph(ushort glyph, HashSet<ushort> ancestors)
    {
        CheckGlyph(glyph);
        if (ancestors.Count >= 16 || !ancestors.Add(glyph))
            throw new InvalidDataException("Cyclic or excessively nested TrueType composite glyph.");
        Spend(1);
        var result = new List<List<Point>>();
        var start = locations[glyph];
        var length = locations[glyph + 1] - start;
        if (length != 0)
        {
            var reader = Table("glyf").Slice((uint)start, (uint)length);
            var contourCount = reader.I16();
            reader.Skip(8);
            if (contourCount > 0) ReadSimple(reader, contourCount, result);
            else if (contourCount < 0) ReadComposite(reader, ancestors, result, glyph);
        }
        ancestors.Remove(glyph);
        return result;
    }

    private void ReadSimple(Reader reader, int contourCount, List<List<Point>> result)
    {
        var ends = new ushort[contourCount];
        for (var index = 0; index < contourCount; index++)
        {
            ends[index] = reader.U16();
            if (index > 0 && ends[index] <= ends[index - 1])
                throw new InvalidDataException("Invalid TrueType contour endpoint.");
        }
        var count = ends[contourCount - 1] + 1;
        Spend(count);
        reader.Skip(reader.U16());
        var flags = new byte[count];
        for (var index = 0; index < count;)
        {
            var flag = reader.Byte();
            if ((flag & 0x80) != 0) throw new InvalidDataException("Reserved TrueType point flag.");
            var repeat = (flag & 8) == 0 ? 1 : reader.Byte() + 1;
            if (repeat > count - index) throw new InvalidDataException("Excessive TrueType flag repetition.");
            for (var end = index + repeat; index < end; index++) flags[index] = flag;
        }
        var xs = Coordinates(2, 16);
        var ys = Coordinates(4, 32);
        var first = 0;
        foreach (var end in ends)
        {
            var points = new List<Point>();
            for (; first <= end; first++) points.Add(new Point(xs[first], ys[first], (flags[first] & 1) != 0));
            result.Add(points);
        }

        int[] Coordinates(int shortFlag, int sameFlag)
        {
            var coordinates = new int[count];
            var value = 0;
            for (var index = 0; index < count; index++)
            {
                var flag = flags[index];
                var delta = (flag & shortFlag) != 0 ? reader.Byte() * ((flag & sameFlag) != 0 ? 1 : -1)
                    : (flag & sameFlag) != 0 ? 0 : reader.I16();
                value = checked(value + delta);
                coordinates[index] = value;
            }
            return coordinates;
        }
    }

    private void ReadComposite(Reader reader, HashSet<ushort> ancestors, List<List<Point>> result, ushort parent)
    {
        ushort flags;
        var hasInstructions = false;
        do
        {
            Spend(1);
            flags = reader.U16();
            hasInstructions |= (flags & 256) != 0;
            var glyph = reader.U16();
            if ((flags & 2) == 0)
                throw new NotSupportedException("Point-attached TrueType composite glyphs are not supported.");
            var x = (flags & 1) != 0 ? reader.I16() : (sbyte)reader.Byte();
            var y = (flags & 1) != 0 ? reader.I16() : (sbyte)reader.Byte();
            var xx = 1d; var yy = 1d; var xy = 0d; var yx = 0d;
            var transforms = flags & (8 | 64 | 128);
            if (transforms == 8) xx = yy = reader.I16() / 16384d;
            else if (transforms == 64) { xx = reader.I16() / 16384d; yy = reader.I16() / 16384d; }
            else if (transforms == 128)
            {
                xx = reader.I16() / 16384d; xy = reader.I16() / 16384d;
                yx = reader.I16() / 16384d; yy = reader.I16() / 16384d;
            }
            else if (transforms != 0) throw new InvalidDataException("Conflicting TrueType composite transforms.");
            if ((flags & ~0x1FEF) != 0 || (flags & 0x1800) == 0x1800)
                throw new InvalidDataException("Invalid TrueType composite flags.");
            // ROUND_XY_TO_GRID affects device hinting, not these unhinted design coordinates.
            if ((flags & 512) != 0 && (xx != 1 || yy != 1 || xy != 0 || yx != 0 ||
                Advance(glyph) != Advance(parent) || HorizontalOrigin(glyph) != HorizontalOrigin(parent)))
                throw new NotSupportedException("Inherited TrueType composite metrics must match untransformed parent metrics.");
            double offsetX = x, offsetY = y;
            if ((flags & 0x800) != 0) { offsetX = x * xx + y * yx; offsetY = x * xy + y * yy; }
            foreach (var contour in ReadGlyph(glyph, ancestors))
            {
                Spend(contour.Count);
                result.Add(contour.Select(point => new Point(
                    point.X * xx + point.Y * yx + offsetX,
                    point.X * xy + point.Y * yy + offsetY, point.OnCurve)).ToList());
            }

        } while ((flags & 32) != 0);
        if (hasInstructions) reader.Skip(reader.U16());
    }

    private int HorizontalOrigin(ushort glyph)
    {
        var length = locations[glyph + 1] - locations[glyph];
        if (length == 0) return -bearings[glyph];
        var reader = Table("glyf").Slice((uint)locations[glyph], (uint)length);
        reader.Skip(2);
        return reader.I16() - bearings[glyph];
    }

    private Reader Table(string tag)
    {
        if (!tables.TryGetValue(tag, out var table))
            throw new NotSupportedException($"Metafile font requires a TrueType {tag} table.");
        return new Reader(data, table.Offset, table.Length);
    }

    private void CheckGlyph(ushort glyph)
    {
        if (glyph >= advances.Length) throw new InvalidDataException("Invalid TrueType glyph index.");
    }

    private void Spend(int count)
    {
        remainingWork -= count;
        if (remainingWork < 0) throw new InvalidDataException("TrueType glyph expansion limit exceeded.");
    }

    private sealed class Reader
    {
        private readonly byte[] data;
        private readonly int start, end;
        private int position;
        internal int Length => end - start;
        internal Reader(byte[] data, int start, int length)
        {
            this.data = data; this.start = start; position = start; end = start + length;
        }
        internal byte Byte()
        {
            if (position >= end) throw new InvalidDataException("Truncated TrueType table or glyph.");
            return data[position++];
        }
        internal ushort U16() => (ushort)(Byte() << 8 | Byte());
        internal short I16() => unchecked((short)U16());
        internal uint U32() => (uint)U16() << 16 | U16();
        internal void Skip(int count)
        {
            if (count < 0 || count > end - position) throw new InvalidDataException("Truncated TrueType table or glyph.");
            position += count;
        }
        internal Reader At(int offset) => Slice((uint)offset);
        internal Reader Slice(uint offset, uint? length = null)
        {
            if (offset > Length || (length.HasValue && length.Value > Length - offset))
                throw new InvalidDataException("TrueType range is outside its table.");
            return new Reader(data, start + (int)offset, length.HasValue ? (int)length.Value : Length - (int)offset);
        }
    }
}
