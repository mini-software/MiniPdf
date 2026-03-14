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
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(fontData);
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
    /// Converts an Excel (.xlsx) stream to a PDF byte array.
    /// </summary>
    /// <param name="inputStream">Stream containing .xlsx data.</param>
    /// <returns>A byte array containing the PDF data.</returns>
    public static byte[] ConvertToPdf(Stream inputStream)
    {
        var doc = ExcelToPdfConverter.Convert(inputStream);
        return doc.ToArray();
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
