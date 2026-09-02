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
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="options">Optional conversion settings.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, MiniPdfConversionOptions? options = null)
    {
        options = PrepareConversionOptions(options);
        var document = ConvertPathToDocument(inputPath, options);
        document.Save(outputPath, CreatePdfSaveOptions(options));
    }

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, MiniPdfConversionOptions? options = null)
    {
        options = PrepareConversionOptions(options);
        var document = ConvertPathToDocument(inputPath, options);
        return document.ToArray(CreatePdfSaveOptions(options));
    }

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="options">Optional conversion settings.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, MiniPdfConversionOptions? options = null)
    {
        using var outputStream = new MemoryStream();
        ConvertToPdf(inputStream, outputStream, options);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) directly to a PDF stream.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="outputStream">Writable stream that receives the PDF data.</param>
    /// <param name="options">Optional conversion settings.</param>
    public static void ConvertToPdf(Stream inputStream, Stream outputStream, MiniPdfConversionOptions? options = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNull(outputStream);
#else
        if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));
        if (outputStream is null) throw new ArgumentNullException(nameof(outputStream));
#endif
        if (!outputStream.CanWrite)
            throw new ArgumentException("Output stream must be writable.", nameof(outputStream));

        options = PrepareConversionOptions(options);
        var document = ConvertStreamToDocument(inputStream, options);
        document.Save(outputStream, CreatePdfSaveOptions(options));
    }

    private static MiniPdfConversionOptions PrepareConversionOptions(MiniPdfConversionOptions? options)
    {
        options ??= new MiniPdfConversionOptions();
        ValidateConversionOptions(options);
        options.Diagnostics?.Reset();
        return options;
    }

    private static PdfDocument ConvertPathToDocument(string inputPath, MiniPdfConversionOptions options)
    {
        var extension = Path.GetExtension(inputPath);
        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return ExcelToPdfConverter.Convert(inputPath, CreateExcelOptions(options));
        if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            return DocxToPdfConverter.Convert(inputPath);
        }
        if (extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
            return PptxToPdfConverter.Convert(inputPath);
        }

        throw new NotSupportedException($"Unsupported file type '{extension}'. Supported formats: .xlsx, .docx, .pptx.");
    }

    private static PdfDocument ConvertStreamToDocument(Stream inputStream, MiniPdfConversionOptions options)
    {
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

            switch (format)
            {
                case OfficeFormat.Docx:
                    ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                    return DocxToPdfConverter.Convert(seekable);
                case OfficeFormat.Pptx:
                    ThrowIfXlsxOnlyOptionsSpecifiedForNonXlsx(options);
                    return PptxToPdfConverter.Convert(seekable);
                case OfficeFormat.Xlsx:
                    return ExcelToPdfConverter.Convert(seekable, CreateExcelOptions(options));
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

}
