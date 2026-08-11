namespace MiniSoftware;

/// <summary>
/// Options for extracting LLM-friendly content from Office documents.
/// </summary>
public sealed class MiniPdfContentOptions
{
    /// <summary>Optional Excel sheet names to extract. Null extracts all visible sheets unless SheetIndexes is specified.</summary>
    public string[]? Sheets { get; set; }

    /// <summary>Optional 1-based Excel sheet indexes to extract. Null extracts all visible sheets unless Sheets is specified.</summary>
    public int[]? SheetIndexes { get; set; }

    /// <summary>Maximum number of rows to extract from each worksheet.</summary>
    public int? MaxRows { get; set; }

    /// <summary>Maximum number of columns to extract from each worksheet.</summary>
    public int? MaxColumns { get; set; }

    /// <summary>Include metadata placeholders for embedded images and charts.</summary>
    public bool IncludeMediaMetadata { get; set; } = true;
}

/// <summary>
/// A normalized, source-ordered representation of content extracted from an Office document.
/// </summary>
public sealed class MiniPdfDocumentContent
{
    internal MiniPdfDocumentContent(string sourceFormat, string? sourceName, IReadOnlyList<MiniPdfContentSection> sections)
    {
        SourceFormat = sourceFormat;
        SourceName = sourceName;
        Sections = sections;
    }

    /// <summary>Version of the serialized content schema.</summary>
    public int SchemaVersion => 1;

    /// <summary>Lower-case source format: docx, xlsx, or pptx.</summary>
    public string SourceFormat { get; }

    /// <summary>Source file name when extraction used a file path.</summary>
    public string? SourceName { get; }

    /// <summary>Source-ordered document, worksheet, or slide sections.</summary>
    public IReadOnlyList<MiniPdfContentSection> Sections { get; }
}

/// <summary>A logical source section such as a document body, worksheet, or slide.</summary>
public sealed class MiniPdfContentSection
{
    internal MiniPdfContentSection(string kind, int index, string? title, IReadOnlyList<MiniPdfContentBlock> blocks)
    {
        Kind = kind;
        Index = index;
        Title = title;
        Blocks = blocks;
    }

    /// <summary>Section kind: document, worksheet, or slide.</summary>
    public string Kind { get; }

    /// <summary>One-based section index.</summary>
    public int Index { get; }

    /// <summary>Worksheet name, slide title, or optional document title.</summary>
    public string? Title { get; }

    /// <summary>Source-ordered content blocks.</summary>
    public IReadOnlyList<MiniPdfContentBlock> Blocks { get; }
}

/// <summary>Base type for normalized content blocks.</summary>
public abstract class MiniPdfContentBlock
{
    internal MiniPdfContentBlock(string kind, int sourceIndex)
    {
        Kind = kind;
        SourceIndex = sourceIndex;
    }

    /// <summary>Block kind.</summary>
    public string Kind { get; }

    /// <summary>Zero-based source element index within the section.</summary>
    public int SourceIndex { get; }
}

/// <summary>A heading block.</summary>
public sealed class MiniPdfHeadingBlock : MiniPdfContentBlock
{
    internal MiniPdfHeadingBlock(int sourceIndex, int level, IReadOnlyList<MiniPdfContentRun> runs)
        : base("heading", sourceIndex)
    {
        Level = level;
        Runs = runs;
    }

    /// <summary>Heading level from 1 through 6.</summary>
    public int Level { get; }

    /// <summary>Inline heading content.</summary>
    public IReadOnlyList<MiniPdfContentRun> Runs { get; }
}

/// <summary>A paragraph block.</summary>
public sealed class MiniPdfParagraphBlock : MiniPdfContentBlock
{
    internal MiniPdfParagraphBlock(int sourceIndex, IReadOnlyList<MiniPdfContentRun> runs)
        : base("paragraph", sourceIndex) => Runs = runs;

    /// <summary>Inline paragraph content.</summary>
    public IReadOnlyList<MiniPdfContentRun> Runs { get; }
}

/// <summary>A contiguous list block.</summary>
public sealed class MiniPdfListBlock : MiniPdfContentBlock
{
    internal MiniPdfListBlock(int sourceIndex, IReadOnlyList<MiniPdfContentListItem> items)
        : base("list", sourceIndex) => Items = items;

    /// <summary>List items in source order.</summary>
    public IReadOnlyList<MiniPdfContentListItem> Items { get; }
}

/// <summary>One item in a normalized list.</summary>
public sealed class MiniPdfContentListItem
{
    internal MiniPdfContentListItem(int level, bool ordered, string? marker, IReadOnlyList<MiniPdfContentRun> runs)
    {
        Level = level;
        Ordered = ordered;
        Marker = marker;
        Runs = runs;
    }

    /// <summary>Zero-based nesting level.</summary>
    public int Level { get; }

    /// <summary>Whether the source list is ordered.</summary>
    public bool Ordered { get; }

    /// <summary>Source marker when available.</summary>
    public string? Marker { get; }

    /// <summary>Inline item content.</summary>
    public IReadOnlyList<MiniPdfContentRun> Runs { get; }
}

/// <summary>A table or worksheet grid block.</summary>
public sealed class MiniPdfTableBlock : MiniPdfContentBlock
{
    internal MiniPdfTableBlock(int sourceIndex, IReadOnlyList<MiniPdfContentTableRow> rows)
        : base("table", sourceIndex) => Rows = rows;

    /// <summary>Rows in source order.</summary>
    public IReadOnlyList<MiniPdfContentTableRow> Rows { get; }
}

/// <summary>A normalized table row.</summary>
public sealed class MiniPdfContentTableRow
{
    internal MiniPdfContentTableRow(int index, bool isHeader, IReadOnlyList<MiniPdfContentTableCell> cells)
    {
        Index = index;
        IsHeader = isHeader;
        Cells = cells;
    }

    /// <summary>Zero-based row index in the source table or worksheet.</summary>
    public int Index { get; }

    /// <summary>Whether the source explicitly marks this row as a header.</summary>
    public bool IsHeader { get; }

    /// <summary>Cells in source order.</summary>
    public IReadOnlyList<MiniPdfContentTableCell> Cells { get; }
}

/// <summary>A normalized table cell.</summary>
public sealed class MiniPdfContentTableCell
{
    internal MiniPdfContentTableCell(int rowIndex, int columnIndex, string? address, int rowSpan, int columnSpan, string text, string? link = null)
    {
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        Address = address;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        Text = text;
        Link = link;
    }

    /// <summary>Zero-based row index.</summary>
    public int RowIndex { get; }

    /// <summary>Zero-based column index.</summary>
    public int ColumnIndex { get; }

    /// <summary>Source address, such as A1 for worksheet cells.</summary>
    public string? Address { get; }

    /// <summary>Number of source rows spanned by this cell.</summary>
    public int RowSpan { get; }

    /// <summary>Number of source columns spanned by this cell.</summary>
    public int ColumnSpan { get; }

    /// <summary>Plain text cell content.</summary>
    public string Text { get; }

    /// <summary>Hyperlink target when the source cell is linked.</summary>
    public string? Link { get; }
}

/// <summary>An embedded image metadata placeholder.</summary>
public sealed class MiniPdfImageBlock : MiniPdfContentBlock
{
    internal MiniPdfImageBlock(int sourceIndex, string format, string? name, string? alternativeText, long width, long height)
        : base("image", sourceIndex)
    {
        Format = format;
        Name = name;
        AlternativeText = alternativeText;
        Width = width;
        Height = height;
    }

    /// <summary>Image format or extension.</summary>
    public string Format { get; }

    /// <summary>Source image name when available.</summary>
    public string? Name { get; }

    /// <summary>Alternative text when available.</summary>
    public string? AlternativeText { get; }

    /// <summary>Source width in EMUs when available.</summary>
    public long Width { get; }

    /// <summary>Source height in EMUs when available.</summary>
    public long Height { get; }
}

/// <summary>An embedded chart metadata placeholder.</summary>
public sealed class MiniPdfChartBlock : MiniPdfContentBlock
{
    internal MiniPdfChartBlock(int sourceIndex, string chartType, string? title, IReadOnlyList<string> seriesNames)
        : base("chart", sourceIndex)
    {
        ChartType = chartType;
        Title = title;
        SeriesNames = seriesNames;
    }

    /// <summary>Source chart type.</summary>
    public string ChartType { get; }

    /// <summary>Chart title when available.</summary>
    public string? Title { get; }

    /// <summary>Chart series names.</summary>
    public IReadOnlyList<string> SeriesNames { get; }
}

/// <summary>A footnote, comment, or speaker note block.</summary>
public sealed class MiniPdfNoteBlock : MiniPdfContentBlock
{
    internal MiniPdfNoteBlock(int sourceIndex, string noteType, string? label, string? author, string text)
        : base("note", sourceIndex)
    {
        NoteType = noteType;
        Label = label;
        Author = author;
        Text = text;
    }

    /// <summary>Note type, such as footnote, comment, or speakerNote.</summary>
    public string NoteType { get; }

    /// <summary>Source label or identifier when available.</summary>
    public string? Label { get; }

    /// <summary>Note author when available.</summary>
    public string? Author { get; }

    /// <summary>Plain text note content.</summary>
    public string Text { get; }
}

/// <summary>Inline normalized content.</summary>
public sealed class MiniPdfContentRun
{
    internal MiniPdfContentRun(string text, bool bold = false, bool italic = false, string? link = null)
    {
        Text = text;
        Bold = bold;
        Italic = italic;
        Link = link;
    }

    /// <summary>Run text.</summary>
    public string Text { get; }

    /// <summary>Whether the source run is bold.</summary>
    public bool Bold { get; }

    /// <summary>Whether the source run is italic.</summary>
    public bool Italic { get; }

    /// <summary>Hyperlink target when available.</summary>
    public string? Link { get; }
}
