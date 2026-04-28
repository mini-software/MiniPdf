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
    /// Converts an Excel (.xlsx) file to a PDF file.
    /// </summary>
    /// <param name="inputPath">Path to the source .xlsx file.</param>
    /// <param name="outputPath">Path for the output .pdf file.</param>
    public static void ConvertToPdf(string inputPath, string outputPath)
    {
        var ext = Path.GetExtension(inputPath);
        if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            DocxToPdfConverter.ConvertToFile(inputPath, outputPath);
        }
        else
        {
            ExcelToPdfConverter.ConvertToFile(inputPath, outputPath);
        }
    }

    /// <summary>
    /// Converts an Excel (.xlsx) file to a PDF byte array.
    /// </summary>
    /// <param name="inputPath">Path to the source .xlsx file.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(string inputPath)
    {
        var ext = Path.GetExtension(inputPath);
        if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            var doc = DocxToPdfConverter.Convert(inputPath);
            return doc.ToArray();
        }
        else
        {
            var doc = ExcelToPdfConverter.Convert(inputPath);
            return doc.ToArray();
        }
    }

    /// <summary>
    /// Converts an Office document stream (.xlsx or .docx) to a PDF byte array.
    /// The format is auto-detected by inspecting the underlying ZIP package contents.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx or .docx data.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream)
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
                        var doc = DocxToPdfConverter.Convert(seekable);
                        return doc.ToArray();
                    }
                case OfficeFormat.Xlsx:
                    {
                        var doc = ExcelToPdfConverter.Convert(seekable);
                        return doc.ToArray();
                    }
                default:
                    throw new NotSupportedException(
                        "Unable to detect Office format from stream. Supported formats: .xlsx, .docx.");
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
            }
        }
        catch (InvalidDataException)
        {
            return OfficeFormat.Unknown;
        }

        return OfficeFormat.Unknown;
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
}
