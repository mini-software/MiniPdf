using System.Collections;
using System.Text;

namespace MiniSoftware;

// Keeps one decoded item in memory. Only the offset index grows with the document.
internal sealed class FileBackedList<T> : IList<T>, IReadOnlyList<T>, IDisposable
{
    private readonly FileStream _stream;
    private readonly BinaryReader _reader;
    private readonly BinaryWriter _writer;
    private readonly Func<BinaryReader, T> _read;
    private readonly Action<BinaryWriter, T> _write;
    private readonly List<long> _offsets = [];
    private readonly bool _writeBack;
    private int _cachedIndex = -1;
    private T _cachedItem = default!;

    internal FileBackedList(Func<BinaryReader, T> read, Action<BinaryWriter, T> write, bool writeBack = false)
    {
        _stream = CreateTemporaryStream();
        _reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        _writer = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
        _read = read;
        _write = write;
        _writeBack = writeBack;
    }

    internal static FileStream CreateTemporaryStream()
        => new(Path.Combine(Path.GetTempPath(), "minipdf-" + Guid.NewGuid().ToString("N") + ".tmp"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 65536, FileOptions.DeleteOnClose);

    public int Count => _offsets.Count;
    public bool IsReadOnly => false;
    internal string FilePath => _stream.Name;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (_cachedIndex == index) return _cachedItem;
            FlushCachedItem();
            _writer.Flush();
            _stream.Position = _offsets[index];
            _cachedItem = _read(_reader);
            _cachedIndex = index;
            return _cachedItem;
        }
        set
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            FlushCachedItem();
            _cachedIndex = index;
            _cachedItem = value;
            Store(index, value);
        }
    }

    public void Add(T item)
    {
        FlushCachedItem();
        _offsets.Add(0);
        _cachedIndex = Count - 1;
        _cachedItem = item;
        Store(_cachedIndex, item);
    }

    private void Store(int index, T item)
    {
        _stream.Position = _stream.Length;
        _offsets[index] = _stream.Position;
        _write(_writer, item);
        _writer.Flush();
    }

    private void FlushCachedItem()
    {
        if (_writeBack && _cachedIndex >= 0) Store(_cachedIndex, _cachedItem);
    }

    public void RemoveAt(int index)
    {
        if (index != Count - 1 || index < 0)
            throw new NotSupportedException("File-backed collections only support removing the last item.");
        FlushCachedItem();
        _offsets.RemoveAt(index);
        _cachedIndex = -1;
        _cachedItem = default!;
    }

    public void Clear()
    {
        _offsets.Clear();
        _cachedIndex = -1;
        _cachedItem = default!;
        _stream.SetLength(0);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++) yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int IndexOf(T item)
    {
        for (var i = 0; i < Count; i++)
            if (EqualityComparer<T>.Default.Equals(this[i], item)) return i;
        return -1;
    }

    public bool Contains(T item) => IndexOf(item) >= 0;
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Destination array is too small.", nameof(array));
        foreach (var item in this) array[arrayIndex++] = item;
    }

    public void Insert(int index, T item) => throw new NotSupportedException();
    public bool Remove(T item) => throw new NotSupportedException();

    public void Dispose()
    {
        try
        {
            _reader.Dispose();
            _writer.Dispose();
        }
        finally
        {
            _stream.Dispose();
            _cachedItem = default!;
        }
    }
}

internal static class LowMemorySerialization
{
    private static void WriteString(BinaryWriter writer, string? value)
    {
        writer.Write(value != null);
        if (value != null) writer.Write(value);
    }

    private static string? ReadString(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadString() : null;
    private static void WriteFloat(BinaryWriter writer, float? value)
    {
        writer.Write(value.HasValue);
        if (value.HasValue) writer.Write(value.Value);
    }

    private static float? ReadFloat(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadSingle() : null;
    private static void WriteColor(BinaryWriter writer, PdfColor? color)
    {
        writer.Write(color.HasValue);
        if (color is { } c) { writer.Write(c.R); writer.Write(c.G); writer.Write(c.B); }
    }

    private static PdfColor? ReadColor(BinaryReader reader)
        => reader.ReadBoolean() ? new PdfColor(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) : null;

    private static void WriteBorderSide(BinaryWriter writer, BorderSide? side)
    {
        writer.Write(side != null);
        if (side != null) { writer.Write(side.Style); WriteColor(writer, side.Color); }
    }

    private static BorderSide? ReadBorderSide(BinaryReader reader)
        => reader.ReadBoolean() ? new BorderSide(reader.ReadString(), ReadColor(reader)) : null;

    internal static void WriteRow(BinaryWriter writer, List<ExcelCell> row)
    {
        writer.Write(row.Count);
        foreach (var cell in row)
        {
            writer.Write(cell.Text);
            WriteColor(writer, cell.Color);
            WriteColor(writer, cell.FillColor);
            writer.Write(cell.Alignment);
            writer.Write(cell.FontSize);
            writer.Write(cell.Bold);
            writer.Write(cell.Italic);
            writer.Write(cell.Underline);
            writer.Write(cell.Strikethrough);
            writer.Write(cell.Border != null);
            if (cell.Border is { } border)
            {
                WriteBorderSide(writer, border.Left);
                WriteBorderSide(writer, border.Right);
                WriteBorderSide(writer, border.Top);
                WriteBorderSide(writer, border.Bottom);
            }
            writer.Write(cell.VerticalAlignment);
            writer.Write(cell.WrapText);
            WriteString(writer, cell.AccountingPrefix);
            WriteString(writer, cell.FontName);
            writer.Write(cell.Indent);
            writer.Write(cell.BoldPrefixLength);
            WriteString(writer, cell.Link);
            writer.Write(cell.TextRotation);
        }
    }

    internal static List<ExcelCell> ReadRow(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var row = new List<ExcelCell>(count);
        for (var i = 0; i < count; i++)
        {
            var text = reader.ReadString();
            var color = ReadColor(reader);
            var fill = ReadColor(reader);
            var alignment = reader.ReadString();
            var size = reader.ReadSingle();
            var bold = reader.ReadBoolean();
            var italic = reader.ReadBoolean();
            var underline = reader.ReadBoolean();
            var strike = reader.ReadBoolean();
            var border = reader.ReadBoolean()
                ? new CellBorderInfo(ReadBorderSide(reader), ReadBorderSide(reader), ReadBorderSide(reader), ReadBorderSide(reader))
                : null;
            row.Add(new ExcelCell(text, color, fill, alignment, size, bold, italic, underline, strike, border,
                reader.ReadString(), reader.ReadBoolean(), ReadString(reader), ReadString(reader),
                reader.ReadInt32(), reader.ReadInt32(), ReadString(reader), reader.ReadInt32()));
        }
        return row;
    }

    internal static void WritePage(BinaryWriter writer, PdfPage page)
    {
        if (page.ImageBlocks.Count != 0 || page.EllipseBlocks.Count != 0 || page.PolygonBlocks.Count != 0 ||
            page.PathBlocks.Count != 0 || page.OverlayRects.Count != 0 || page.OverlayLines.Count != 0 ||
            page.OverlayTexts.Count != 0)
            throw new NotSupportedException("LowMemory mode only supports worksheet text, cell fills and borders.");
        writer.Write(page.Width);
        writer.Write(page.Height);
        writer.Write(page.TextBlocks.Count);
        foreach (var text in page.TextBlocks)
        {
            writer.Write(text.Text);
            writer.Write(text.X); writer.Write(text.Y); writer.Write(text.FontSize);
            WriteColor(writer, text.Color);
            writer.Write(text.ClipRect.HasValue);
            if (text.ClipRect is { } clip)
            {
                writer.Write(clip.X); writer.Write(clip.Y); writer.Write(clip.Width); writer.Write(clip.Height);
            }
            WriteFloat(writer, text.MaxWidth);
            writer.Write(text.Bold); writer.Write(text.Italic); writer.Write(text.Underline);
            writer.Write(text.CharSpacing); writer.Write(text.WordSpacing);
            WriteString(writer, text.PreferredFontName);
            WriteFloat(writer, text.UnderlineWidth);
            writer.Write(text.Strikethrough); writer.Write(text.Hidden);
        }
        writer.Write(page.RectBlocks.Count);
        foreach (var rect in page.RectBlocks)
        {
            writer.Write(rect.X); writer.Write(rect.Y); writer.Write(rect.Width); writer.Write(rect.Height);
            WriteColor(writer, rect.FillColor); writer.Write(rect.Alpha);
        }
        writer.Write(page.LineBlocks.Count);
        foreach (var line in page.LineBlocks)
        {
            writer.Write(line.X1); writer.Write(line.Y1); writer.Write(line.X2); writer.Write(line.Y2);
            WriteColor(writer, line.Color); writer.Write(line.LineWidth);
            writer.Write(line.DashPattern?.Length ?? -1);
            if (line.DashPattern != null)
                foreach (var dash in line.DashPattern) writer.Write(dash);
        }
    }

    internal static PdfPage ReadPage(BinaryReader reader)
    {
        var page = new PdfPage(reader.ReadSingle(), reader.ReadSingle());
        var texts = reader.ReadInt32();
        for (var i = 0; i < texts; i++)
        {
            var text = reader.ReadString();
            var x = reader.ReadSingle(); var y = reader.ReadSingle(); var size = reader.ReadSingle();
            var color = ReadColor(reader);
            (float, float, float, float)? clip = reader.ReadBoolean()
                ? (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) : null;
            page.AddText(text, x, y, size, color, clip, ReadFloat(reader),
                reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(),
                reader.ReadSingle(), reader.ReadSingle(), ReadString(reader), ReadFloat(reader),
                reader.ReadBoolean(), reader.ReadBoolean());
        }
        var rectangles = reader.ReadInt32();
        for (var i = 0; i < rectangles; i++)
            page.AddRectangle(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                ReadColor(reader), reader.ReadSingle());
        var lines = reader.ReadInt32();
        for (var i = 0; i < lines; i++)
        {
            var x1 = reader.ReadSingle(); var y1 = reader.ReadSingle();
            var x2 = reader.ReadSingle(); var y2 = reader.ReadSingle();
            var color = ReadColor(reader); var width = reader.ReadSingle();
            var count = reader.ReadInt32();
            float[]? dashes = count < 0 ? null : new float[count];
            if (dashes != null)
                for (var d = 0; d < count; d++) dashes[d] = reader.ReadSingle();
            page.AddLine(x1, y1, x2, y2, color, width, dashes);
        }
        return page;
    }
}
