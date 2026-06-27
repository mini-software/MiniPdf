using System.IO.Compression;

namespace MiniSoftware;

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
    public static void ConvertToPdf(string inputPath, string outputPath)
        => ConvertToPdf(inputPath, outputPath, null, null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, string[]? sheets)
        => ConvertToPdf(inputPath, outputPath, sheets, null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, outputPath, null, sheetIndexes);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    public static void ConvertToPdf(string inputPath, string outputPath, string[]? sheets, int[]? sheetIndexes)
    {
        var ext = Path.GetExtension(inputPath);
        if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ExcelToPdfConverter.ConvertToFile(inputPath, outputPath, new ExcelToPdfConverter.ConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });
        }
        else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
            DocxToPdfConverter.ConvertToFile(inputPath, outputPath);
        }
        else if (ext.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
            PptxToPdfConverter.ConvertToFile(inputPath, outputPath);
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
        => ConvertToPdf(inputPath, (string[]?)null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, string[]? sheets)
        => ConvertToPdf(inputPath, sheets, null);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, int[]? sheetIndexes)
        => ConvertToPdf(inputPath, sheets: null, sheetIndexes: sheetIndexes);

    /// <summary>
    /// Converts an Office (.xlsx, .docx, or .pptx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source Office file.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath, string[]? sheets, int[]? sheetIndexes)
    {
        var ext = Path.GetExtension(inputPath);
        if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var doc = ExcelToPdfConverter.Convert(inputPath, new ExcelToPdfConverter.ConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });
            return doc.ToArray();
        }
        else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
            var doc = DocxToPdfConverter.Convert(inputPath);
            return doc.ToArray();
        }
        else if (ext.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
        {
            ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
            var doc = PptxToPdfConverter.Convert(inputPath);
            return doc.ToArray();
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
        => ConvertToPdf(inputStream, (string[]?)null);

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, string[]? sheets)
        => ConvertToPdf(inputStream, sheets, null);

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, int[]? sheetIndexes)
        => ConvertToPdf(inputStream, null, sheetIndexes);

    /// <summary>
    /// Converts an Office document stream (.xlsx, .docx, or .pptx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx, .docx, or .pptx data.</param>
    /// <param name="sheets">Optional Excel sheet names to render. Null renders all visible sheets unless sheetIndexes is specified.</param>
    /// <param name="sheetIndexes">Optional 1-based Excel sheet indexes to render. Null renders all visible sheets unless sheets is specified.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream, string[]? sheets, int[]? sheetIndexes)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(inputStream);
#else
        if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));
#endif

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
                    {
                        ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
                        var doc = DocxToPdfConverter.Convert(seekable);
                        return doc.ToArray();
                    }
                case OfficeFormat.Pptx:
                    {
                        ThrowIfSheetsSpecifiedForNonXlsx(sheets, sheetIndexes);
                        var doc = PptxToPdfConverter.Convert(seekable);
                        return doc.ToArray();
                    }
                case OfficeFormat.Xlsx:
                    {
                        var doc = ExcelToPdfConverter.Convert(seekable, new ExcelToPdfConverter.ConversionOptions { Sheets = sheets, SheetIndexes = sheetIndexes });
                        return doc.ToArray();
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

    private static void ThrowIfSheetsSpecifiedForNonXlsx(string[]? sheets, int[]? sheetIndexes)
    {
        if (sheets != null || sheetIndexes != null)
            throw new NotSupportedException("Sheet selection is only supported for .xlsx files.");
    }

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
