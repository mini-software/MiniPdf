using System.IO.Compression;

namespace MiniSoftware;

/// <summary>
/// Options for Office-to-PDF conversion.
/// </summary>
public sealed class MiniPdfConversionOptions
{
    /// <summary>Optional collector populated with diagnostics for this conversion.</summary>
    public MiniPdfConversionDiagnostics? Diagnostics { get; set; }

    /// <summary>Optional Excel sheet names to render. Null renders all visible sheets unless SheetIndexes is specified.</summary>
    public string[]? Sheets { get; set; }

    /// <summary>Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless Sheets is specified.</summary>
    public int[]? SheetIndexes { get; set; }

    /// <summary>Compress PDF page content streams using FlateDecode.</summary>
    public bool Compress { get; set; }

    /// <summary>Maximum number of worksheet rows to render from each Excel sheet or print area.</summary>
    public int? MaxRows { get; set; }

    /// <summary>Maximum number of worksheet columns to render from each Excel sheet or print area.</summary>
    public int? MaxColumns { get; set; }

    /// <summary>Override Excel worksheet orientation. True renders XLSX sheets in landscape; false renders portrait.</summary>
    public bool? Landscape { get; set; }

    /// <summary>Override Excel fit-to-page mode. When true, wide XLSX sheets are scaled to fit the page width.</summary>
    public bool? FitToPage { get; set; }

    /// <summary>Number of horizontal pages to fit each Excel sheet to. 0 means unlimited.</summary>
    public int? FitToWidth { get; set; }

    /// <summary>Number of vertical pages to fit each Excel sheet to. 0 means unlimited.</summary>
    public int? FitToHeight { get; set; }

    /// <summary>Excel print scale percentage for XLSX sheets (10-400). Values below 100 fit more content per page.</summary>
    public int? PrintScale { get; set; }

    /// <summary>Target minimum number of worksheet rows per PDF page. Applies by fitting the sheet height to a derived page count.</summary>
    public int? RowsPerPage { get; set; }

    /// <summary>Optional culture used to format numeric and date values from Excel cells. Null uses the invariant culture (current behavior).</summary>
    public System.Globalization.CultureInfo? Culture { get; set; }
}

/// <summary>
/// Collects non-fatal diagnostics produced while converting an Office document.
/// </summary>
public sealed class MiniPdfConversionDiagnostics
{
    private readonly Dictionary<string, int> _missingFonts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Requested font families that could not be resolved without fallback.</summary>
    public IReadOnlyList<MiniPdfMissingFont> MissingFonts
    {
        get
        {
            lock (_missingFonts)
                return _missingFonts
                    .OrderByDescending(item => item.Value)
                    .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(item => new MiniPdfMissingFont(item.Key, item.Value))
                    .ToArray();
        }
    }

    internal void Reset()
    {
        lock (_missingFonts)
            _missingFonts.Clear();
    }

    internal void RecordMissingFont(string fontFamily, int occurrenceCount)
    {
        if (string.IsNullOrWhiteSpace(fontFamily) || occurrenceCount <= 0) return;
        lock (_missingFonts)
        {
            _missingFonts.TryGetValue(fontFamily, out var existingCount);
            _missingFonts[fontFamily] = existingCount + occurrenceCount;
        }
    }
}

/// <summary>Describes one unresolved requested font family.</summary>
public sealed class MiniPdfMissingFont
{
    internal MiniPdfMissingFont(string fontFamily, int occurrenceCount)
    {
        FontFamily = fontFamily;
        OccurrenceCount = occurrenceCount;
    }

    /// <summary>The font family requested by the source Office document.</summary>
    public string FontFamily { get; }

    /// <summary>Number of text blocks that requested this font family.</summary>
    public int OccurrenceCount { get; }
}

/// <summary>
/// Options for merging multiple PDF files into one PDF.
/// </summary>
public sealed class PdfMergeOptions
{
    /// <summary>
    /// Optional bookmark titles to add at the first page of each input PDF. When provided,
    /// the number of titles must match the number of input PDFs.
    /// </summary>
    public IList<string>? BookmarkTitles { get; set; }

    /// <summary>
    /// Optional page-specific bookmarks to add to the merged PDF. PageIndex is zero-based
    /// in the final merged document.
    /// </summary>
    public IList<PdfBookmark>? Bookmarks { get; set; }
}

/// <summary>
/// Represents a top-level PDF bookmark targeting a page in the merged document.
/// </summary>
public sealed class PdfBookmark
{
    /// <summary>
    /// Creates a PDF bookmark.
    /// </summary>
    /// <param name="title">Bookmark title.</param>
    /// <param name="pageIndex">Zero-based page index in the merged PDF.</param>
    public PdfBookmark(string title, int pageIndex)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(title);
#else
        if (title is null) throw new ArgumentNullException(nameof(title));
#endif
        if (pageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "PageIndex must be zero or greater.");

        Title = title;
        PageIndex = pageIndex;
    }

    /// <summary>Bookmark title.</summary>
    public string Title { get; }

    /// <summary>Zero-based page index in the merged PDF.</summary>
    public int PageIndex { get; }
}

/// <summary>
/// Main entry point for MiniPdf operations.
/// Provides simple methods for converting files to PDF format.
/// </summary>
public static class MiniPdf
{
    private static readonly List<(string Name, byte[] Data)> _registeredFonts = new();

    /// <summary>
    /// Registers a TrueType (.ttf) or TrueType Collection (.ttc) font for use in PDF generation.
    /// This is required for environments where system fonts are unavailable (e.g. Blazor WASM).
    /// </summary>
    /// <param name="name">A descriptive name for the font (e.g. "NotoSansSC").</param>
    /// <param name="fontData">The raw bytes of the .ttf or .ttc font file.</param>
    public static void RegisterFont(string name, byte[] fontData)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(fontData);
#else
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (fontData is null) throw new ArgumentNullException(nameof(fontData));
#endif
        lock (_registeredFonts)
            _registeredFonts.Add((name, fontData));
    }

    /// <summary>
    /// Returns a snapshot of all registered fonts.
    /// </summary>
    internal static List<(string Name, byte[] Data)> GetRegisteredFonts()
    {
        lock (_registeredFonts)
            return new List<(string, byte[])>(_registeredFonts);
    }

    /// <summary>
    /// Merges multiple PDF files into a single PDF file.
    /// </summary>
    /// <param name="inputPaths">Input PDF file paths in merge order.</param>
    /// <param name="outputPath">Path for the merged PDF file.</param>
    public static void MergePdf(IEnumerable<string> inputPaths, string outputPath)
        => MergePdf(inputPaths, outputPath, (PdfMergeOptions?)null);

    /// <summary>
    /// Merges multiple PDF files into a single PDF file and optionally adds bookmarks.
    /// </summary>
    /// <param name="inputPaths">Input PDF file paths in merge order.</param>
    /// <param name="outputPath">Path for the merged PDF file.</param>
    /// <param name="options">Optional merge settings.</param>
    public static void MergePdf(IEnumerable<string> inputPaths, string outputPath, PdfMergeOptions? options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputPaths);
        ArgumentNullException.ThrowIfNull(outputPath);
#else
        if (inputPaths is null) throw new ArgumentNullException(nameof(inputPaths));
        if (outputPath is null) throw new ArgumentNullException(nameof(outputPath));
#endif
        var bytes = MergePdf(inputPaths, options);
        File.WriteAllBytes(outputPath, bytes);
    }

    /// <summary>
    /// Merges multiple PDF files into a single PDF byte array.
    /// </summary>
    /// <param name="inputPaths">Input PDF file paths in merge order.</param>
    /// <returns>A byte array containing the merged PDF data.</returns>
    public static byte[] MergePdf(IEnumerable<string> inputPaths)
        => MergePdf(inputPaths, (PdfMergeOptions?)null);

    /// <summary>
    /// Merges multiple PDF files into a single PDF byte array and optionally adds bookmarks.
    /// </summary>
    /// <param name="inputPaths">Input PDF file paths in merge order.</param>
    /// <param name="options">Optional merge settings.</param>
    /// <returns>A byte array containing the merged PDF data.</returns>
    public static byte[] MergePdf(IEnumerable<string> inputPaths, PdfMergeOptions? options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputPaths);
#else
        if (inputPaths is null) throw new ArgumentNullException(nameof(inputPaths));
#endif
        var paths = inputPaths.ToList();
        if (paths.Count == 0)
            throw new ArgumentException("At least one input PDF is required.", nameof(inputPaths));

        var pdfs = new List<byte[]>(paths.Count);
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Input PDF paths cannot be null or whitespace.", nameof(inputPaths));
            pdfs.Add(File.ReadAllBytes(path));
        }

        return PdfMerger.Merge(pdfs, options);
    }

    /// <summary>
    /// Extracts normalized, LLM-friendly content from an Office document.
    /// </summary>
    /// <param name="inputPath">Path to a .xlsx, .docx, or .pptx file.</param>
    /// <param name="options">Optional extraction settings.</param>
    /// <returns>A normalized semantic content model.</returns>
    public static MiniPdfDocumentContent ExtractContent(string inputPath, MiniPdfContentOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputPath);
#else
        if (inputPath is null) throw new ArgumentNullException(nameof(inputPath));
#endif
        using var stream = File.OpenRead(inputPath);
        return ExtractContentCore(stream, Path.GetFileName(inputPath), options);
    }

    /// <summary>
    /// Extracts normalized, LLM-friendly content from an Office document stream.
    /// The format is auto-detected from the ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="options">Optional extraction settings.</param>
    /// <returns>A normalized semantic content model.</returns>
    public static MiniPdfDocumentContent ExtractContent(Stream inputStream, MiniPdfContentOptions? options = null)
        => ExtractContentCore(inputStream, null, options);

    /// <summary>Converts an Office document to deterministic Markdown.</summary>
    public static string ConvertToMarkdown(string inputPath, MiniPdfContentOptions? options = null)
        => DocumentMarkdownWriter.Write(ExtractContent(inputPath, options));

    /// <summary>Converts an Office document stream to deterministic Markdown.</summary>
    public static string ConvertToMarkdown(Stream inputStream, MiniPdfContentOptions? options = null)
        => DocumentMarkdownWriter.Write(ExtractContent(inputStream, options));

    /// <summary>Converts an Office document to a UTF-8 Markdown file.</summary>
    public static void ConvertToMarkdown(string inputPath, string outputPath, MiniPdfContentOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(outputPath);
#else
        if (outputPath is null) throw new ArgumentNullException(nameof(outputPath));
#endif
        File.WriteAllText(outputPath, ConvertToMarkdown(inputPath, options), new System.Text.UTF8Encoding(false));
    }

    /// <summary>Converts an Office document to deterministic JSON using content schema version 1.</summary>
    public static string ConvertToJson(string inputPath, MiniPdfContentOptions? options = null)
        => DocumentJsonWriter.Write(ExtractContent(inputPath, options));

    /// <summary>Converts an Office document stream to deterministic JSON using content schema version 1.</summary>
    public static string ConvertToJson(Stream inputStream, MiniPdfContentOptions? options = null)
        => DocumentJsonWriter.Write(ExtractContent(inputStream, options));

    /// <summary>Converts an Office document to a UTF-8 JSON file using content schema version 1.</summary>
    public static void ConvertToJson(string inputPath, string outputPath, MiniPdfContentOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(outputPath);
#else
        if (outputPath is null) throw new ArgumentNullException(nameof(outputPath));
#endif
        File.WriteAllText(outputPath, ConvertToJson(inputPath, options), new System.Text.UTF8Encoding(false));
    }

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    public static void ConvertToPdf(string inputPath, string outputPath)
        => ConvertToPdf(inputPath, outputPath, (MiniPdfConversionOptions?)null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, string[]? sheets)
        => ConvertToPdf(inputPath, outputPath, new MiniPdfConversionOptions { Sheets = sheets });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, outputPath, new MiniPdfConversionOptions { SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, string[]? sheets, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, outputPath, new MiniPdfConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="options">Optional conversion settings.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, MiniPdfConversionOptions? options)
    {
        options ??= new MiniPdfConversionOptions();
        ValidateConversionOptions(options);
        options.Diagnostics?.Reset();

        var ext = Path.GetExtension(inputPath);
        var saveOptions = CreatePdfSaveOptions(options);
        if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var doc = ExcelToPdfConverter.Convert(inputPath, CreateExcelOptions(options));
            doc.Save(outputPath, saveOptions);
        }
        else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            var doc = DocxToPdfConverter.Convert(inputPath);
            doc.Save(outputPath, saveOptions);
        }
        else if (ext.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            var doc = PptxToPdfConverter.Convert(inputPath);
            doc.Save(outputPath, saveOptions);
        }
        else
        {
            throw new NotSupportedException($"Unsupported file type '{ext}'. Supported formats: .xlsx, .docx, .pptx.");
        }
    }

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath)
        => ConvertToPdf(inputPath, (MiniPdfConversionOptions?)null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, string[]? sheets)
        => ConvertToPdf(inputPath, new MiniPdfConversionOptions { Sheets = sheets });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, new MiniPdfConversionOptions { SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, string[]? sheets, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, new MiniPdfConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, MiniPdfConversionOptions? options)
    {
        options ??= new MiniPdfConversionOptions();
        ValidateConversionOptions(options);
        options.Diagnostics?.Reset();

        var ext = Path.GetExtension(inputPath);
        var saveOptions = CreatePdfSaveOptions(options);
        if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var doc = ExcelToPdfConverter.Convert(inputPath, CreateExcelOptions(options));
            return doc.ToArray(saveOptions);
        }
        else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            var doc = DocxToPdfConverter.Convert(inputPath);
            return doc.ToArray(saveOptions);
        }
        else if (ext.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            var doc = PptxToPdfConverter.Convert(inputPath);
            return doc.ToArray(saveOptions);
        }
        else
        {
            throw new NotSupportedException($"Unsupported file type '{ext}'. Supported formats: .xlsx, .docx, .pptx.");
        }
    }

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream)
        => ConvertToPdf(inputStream, (MiniPdfConversionOptions?)null);

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, string[]? sheets)
        => ConvertToPdf(inputStream, new MiniPdfConversionOptions { Sheets = sheets });

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, int[]? sheetIndexes)
        => ConvertToPdf(inputStream, new MiniPdfConversionOptions { SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, string[]? sheets, int[]? sheetIndexes)
        => ConvertToPdf(inputStream, new MiniPdfConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, MiniPdfConversionOptions? options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputStream);
#else
        if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));
#endif
        options ??= new MiniPdfConversionOptions();
        ValidateConversionOptions(options);
        options.Diagnostics?.Reset();

        // Ensure we have a seekable stream so ZipArchive can read the central directory
        // and the converter can subsequently re-read the package.
        Stream seekable;
        bool ownsSeekable = false;
        if (inputStream.CanSeek)
        {
            seekable = inputStream;
        }
        else
        {
            var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            ms.Position = 0;
            seekable = ms;
            ownsSeekable = true;
        }

        try
        {
            var startPosition = seekable.Position;
            var format = DetectOfficeFormat(seekable);
            seekable.Position = startPosition;
            var saveOptions = CreatePdfSaveOptions(options);

            switch (format)
            {
                case OfficeFormat.Docx:
                    {
                        ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                        var doc = DocxToPdfConverter.Convert(seekable);
                        return doc.ToArray(saveOptions);
                    }
                case OfficeFormat.Pptx:
                    {
                        ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                        var doc = PptxToPdfConverter.Convert(seekable);
                        return doc.ToArray(saveOptions);
                    }
                case OfficeFormat.Xlsx:
                    {
                        var doc = ExcelToPdfConverter.Convert(seekable, CreateExcelOptions(options));
                        return doc.ToArray(saveOptions);
                    }
                default:
                    throw new NotSupportedException(
                        "Unable to detect Office format from stream. Supported formats: .xlsx, .docx, .pptx.");
            }
        }
        finally
        {
            if (ownsSeekable)
                seekable.Dispose();
        }
    }

    private enum OfficeFormat
    {
        Unknown,
        Xlsx,
        Docx,
        Pptx,
    }

    private static OfficeFormat DetectOfficeFormat(Stream seekableStream)
    {
        try
        {
            using var archive = new ZipArchive(seekableStream, ZipArchiveMode.Read, leaveOpen: true);
            foreach (var entry in archive.Entries)
            {
                var name = entry.FullName;
                if (name.StartsWith("word/", StringComparison.OrdinalIgnoreCase))
                    return OfficeFormat.Docx;
                if (name.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                    return OfficeFormat.Xlsx;
                if (name.StartsWith("ppt/", StringComparison.OrdinalIgnoreCase))
                    return OfficeFormat.Pptx;
            }
        }
        catch (InvalidDataException)
        {
            return OfficeFormat.Unknown;
        }

        return OfficeFormat.Unknown;
    }

    private static MiniPdfDocumentContent ExtractContentCore(
        Stream inputStream,
        string? sourceName,
        MiniPdfContentOptions? options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputStream);
#else
        if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));
#endif
        options ??= new MiniPdfContentOptions();
        ValidateContentOptions(options);

        var originalPosition = inputStream.CanSeek ? inputStream.Position : 0;
        Stream packageStream = inputStream;
        var ownsPackageStream = false;
        if (!inputStream.CanSeek || originalPosition != 0)
        {
            var copy = new MemoryStream();
            inputStream.CopyTo(copy);
            copy.Position = 0;
            packageStream = copy;
            ownsPackageStream = true;
        }

        try
        {
            var format = DetectOfficeFormat(packageStream);
            packageStream.Position = 0;
            switch (format)
            {
                case OfficeFormat.Docx:
                    ThrowIfContentXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                    return DocumentContentExtractor.ExtractDocx(packageStream, sourceName, options);
                case OfficeFormat.Xlsx:
                    return DocumentContentExtractor.ExtractXlsx(packageStream, sourceName, options);
                case OfficeFormat.Pptx:
                    ThrowIfContentXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                    return DocumentContentExtractor.ExtractPptx(packageStream, sourceName, options);
                default:
                    throw new NotSupportedException(
                        "Unable to detect Office format from stream. Supported formats: .xlsx, .docx, .pptx.");
            }
        }
        finally
        {
            if (ownsPackageStream)
                packageStream.Dispose();
            if (inputStream.CanSeek)
                inputStream.Position = originalPosition;
        }
    }

    private static void ValidateContentOptions(MiniPdfContentOptions options)
    {
        if (options.MaxRows.HasValue && options.MaxRows.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxRows), "MaxRows must be greater than zero.");
        if (options.MaxColumns.HasValue && options.MaxColumns.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxColumns), "MaxColumns must be greater than zero.");
        if (options.SheetIndexes?.Any(index => index <= 0) == true)
            throw new ArgumentOutOfRangeException(nameof(options.SheetIndexes), "Sheet indexes must be greater than zero.");
    }

    private static void ThrowIfContentXlsxOnlyOptionsSpecifiedForNonXlsx(MiniPdfContentOptions options)
    {
        if (options.Sheets != null || options.SheetIndexes != null ||
            options.MaxRows.HasValue || options.MaxColumns.HasValue)
            throw new NotSupportedException("Excel-specific content options are only supported for .xlsx files.");
    }

    private static void ThrowIfSheetsSpecifiedForNonXlsx(string[]? sheets, int[]? sheetIndexes)
    {
        if (sheets != null || sheetIndexes != null)
            throw new NotSupportedException("Sheet selection is only supported for .xlsx files.");
    }

    private static void ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(MiniPdfConversionOptions options)
    {
        ThrowIfSheetsSpecifiedForNonXlsx(options.Sheets, options.SheetIndexes);
        if (options.MaxRows.HasValue || options.MaxColumns.HasValue ||
            options.Landscape.HasValue || options.FitToPage.HasValue ||
            options.FitToWidth.HasValue || options.FitToHeight.HasValue ||
            options.PrintScale.HasValue || options.RowsPerPage.HasValue ||
            options.Culture != null)
            throw new NotSupportedException("Excel-specific conversion options are only supported for .xlsx files.");
    }

    private static void ValidateConversionOptions(MiniPdfConversionOptions options)
    {
        if (options.MaxRows.HasValue && options.MaxRows.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxRows), "MaxRows must be greater than zero.");
        if (options.MaxColumns.HasValue && options.MaxColumns.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxColumns), "MaxColumns must be greater than zero.");
        if (options.FitToWidth.HasValue && options.FitToWidth.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(options.FitToWidth), "FitToWidth must be zero or greater.");
        if (options.FitToHeight.HasValue && options.FitToHeight.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(options.FitToHeight), "FitToHeight must be zero or greater.");
        if (options.PrintScale.HasValue && (options.PrintScale.Value < 10 || options.PrintScale.Value > 400))
            throw new ArgumentOutOfRangeException(nameof(options.PrintScale), "PrintScale must be between 10 and 400.");
        if (options.RowsPerPage.HasValue && options.RowsPerPage.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.RowsPerPage), "RowsPerPage must be greater than zero.");
    }

    private static ExcelToPdfConverter.ConversionOptions CreateExcelOptions(MiniPdfConversionOptions options)
        => new()
        {
            Sheets = options.Sheets,
            SheetIndexes = options.SheetIndexes,
            MaxRows = options.MaxRows,
            MaxColumns = options.MaxColumns,
            Landscape = options.Landscape,
            FitToPage = options.FitToPage,
            FitToWidth = options.FitToWidth,
            FitToHeight = options.FitToHeight,
            PrintScale = options.PrintScale,
            RowsPerPage = options.RowsPerPage,
            Culture = options.Culture,
        };

    private static PdfSaveOptions? CreatePdfSaveOptions(MiniPdfConversionOptions options)
        => options.Compress || options.Diagnostics != null
            ? new PdfSaveOptions
            {
                CompressContentStreams = options.Compress,
                Diagnostics = options.Diagnostics,
            }
            : null;

    /// <summary>
    /// Converts a Word (.docx) stream to a PDF byte array.
    /// </summary>
    /// <param name="docxStream">Stream containing .docx data.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertDocxToPdf(Stream docxStream)
    {
        var doc = DocxToPdfConverter.Convert(docxStream);
        return doc.ToArray();
    }

    /// <summary>
    /// Converts a PowerPoint (.pptx) stream to a PDF byte array.
    /// </summary>
    /// <param name="pptxStream">Stream containing .pptx data.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertPptxToPdf(Stream pptxStream)
    {
        return ConvertPptxToPdfCore(pptxStream);
    }

    private static byte[] ConvertPptxToPdfCore(Stream pptxStream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(pptxStream);
#else
        if (pptxStream is null) throw new ArgumentNullException(nameof(pptxStream));
#endif

        if (pptxStream.CanSeek)
        {
            var doc = PptxToPdfConverter.Convert(pptxStream);
            return doc.ToArray();
        }

        using var ms = new MemoryStream();
        pptxStream.CopyTo(ms);
        ms.Position = 0;
        var seekableDoc = PptxToPdfConverter.Convert(ms);
        return seekableDoc.ToArray();
    }
}
