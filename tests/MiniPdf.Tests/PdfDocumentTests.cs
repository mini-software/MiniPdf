namespace MiniSoftware.Tests;

public class PdfDocumentTests
{
    [Fact]
    public void AddPage_DefaultSize_CreatesUsLetterPage()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();

        Assert.Single(doc.Pages);
        Assert.Equal(612, page.Width);
        Assert.Equal(792, page.Height);
    }

    [Fact]
    public void AddPage_CustomSize_UsesProvidedDimensions()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage(width: 100, height: 200);

        Assert.Equal(100, page.Width);
        Assert.Equal(200, page.Height);
    }

    [Fact]
    public void AddText_StoresTextBlock()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        page.AddText("Hello", 10, 20, 14);

        Assert.Single(page.TextBlocks);
        var block = page.TextBlocks[0];
        Assert.Equal("Hello", block.Text);
        Assert.Equal(10, block.X);
        Assert.Equal(20, block.Y);
        Assert.Equal(14, block.FontSize);
    }

    [Fact]
    public void AddText_Chaining_ReturnsSamePage()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        var result = page.AddText("A", 0, 0).AddText("B", 0, 0);

        Assert.Same(page, result);
        Assert.Equal(2, page.TextBlocks.Count);
    }

    [Fact]
    public void Save_ProducesValidPdfHeader()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("Test", 50, 700);

        var bytes = doc.ToArray();
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void Save_ContainsTextContent()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("Hello World", 50, 700);

        var bytes = doc.ToArray();
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        Assert.Contains("Hello World", content);
        Assert.Contains("/F1", content);
        Assert.Contains("/Helvetica", content);
    }

    [Fact]
    public void Save_WithCompressedContentStreams_WritesFlateContentStream()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("Hello compressed stream", 50, 700);

        var bytes = doc.ToArray(new PdfSaveOptions { CompressContentStreams = true });
        var content = System.Text.Encoding.ASCII.GetString(bytes);
        var streamBytes = ExtractFirstStream(bytes, out var declaredLength);
        var decoded = System.Text.Encoding.ASCII.GetString(DecompressZlib(streamBytes));

        Assert.Contains("/Filter /FlateDecode", content);
        Assert.Equal(declaredLength, streamBytes.Length);
        Assert.Contains("Hello compressed stream", decoded);
    }

    [Fact]
    public void Save_MultiplePages_AllIncluded()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("Page 1", 50, 700);
        doc.AddPage().AddText("Page 2", 50, 700);

        var bytes = doc.ToArray();
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        Assert.Contains("Page 1", content);
        Assert.Contains("Page 2", content);
        Assert.Contains("/Count 2", content);
    }

    [Fact]
    public void Save_ToFile_CreatesFile()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("File test", 50, 700);

        var path = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");
        try
        {
            doc.Save(path);
            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length > 0);
            Assert.StartsWith("%PDF-1.4", System.Text.Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_EscapesSpecialCharacters()
    {
        var doc = new PdfDocument();
        doc.AddPage().AddText("Hello (world) \\ test", 50, 700);

        var bytes = doc.ToArray();
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        Assert.Contains("Hello \\(world\\) \\\\ test", content);
    }

    [Fact]
    public void AddTextWrapped_WrapsLongText()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        var longText = "This is a very long text that should be wrapped across multiple lines when rendered on the page";
        page.AddTextWrapped(longText, 50, 700, maxWidth: 200, fontSize: 12);

        // Should have created multiple text blocks
        Assert.True(page.TextBlocks.Count > 1, "Long text should wrap into multiple lines");
    }

    [Fact]
    public void AddTextWrapped_EmptyText_DoesNothing()
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        page.AddTextWrapped("", 50, 700, maxWidth: 200);

        Assert.Empty(page.TextBlocks);
    }

    [Fact]
    public void EmptyDocument_ProducesValidPdf()
    {
        var doc = new PdfDocument();
        doc.AddPage(); // Empty page

        var bytes = doc.ToArray();
        var content = System.Text.Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("%%EOF", content);
        Assert.Contains("/Type /Page", content);
    }

    private static byte[] ExtractFirstStream(byte[] pdfBytes, out int declaredLength)
    {
        var content = System.Text.Encoding.ASCII.GetString(pdfBytes);
        var lengthMarker = "/Length ";
        var lengthStart = content.IndexOf(lengthMarker, StringComparison.Ordinal);
        Assert.True(lengthStart >= 0, "PDF stream length marker was not found.");
        lengthStart += lengthMarker.Length;
        var lengthEnd = lengthStart;
        while (lengthEnd < content.Length && char.IsDigit(content[lengthEnd]))
            lengthEnd++;
        declaredLength = int.Parse(content[lengthStart..lengthEnd], System.Globalization.CultureInfo.InvariantCulture);

        var streamMarker = "stream\n";
        var streamStart = content.IndexOf(streamMarker, lengthEnd, StringComparison.Ordinal);
        Assert.True(streamStart >= 0, "PDF stream marker was not found.");
        streamStart += streamMarker.Length;

        var endStreamMarker = "\nendstream";
        var streamEnd = content.IndexOf(endStreamMarker, streamStart, StringComparison.Ordinal);
        Assert.True(streamEnd >= 0, "PDF endstream marker was not found.");

        var streamBytes = new byte[streamEnd - streamStart];
        Array.Copy(pdfBytes, streamStart, streamBytes, 0, streamBytes.Length);
        return streamBytes;
    }

    private static byte[] DecompressZlib(byte[] compressedBytes)
    {
        using var input = new MemoryStream(compressedBytes);
        using var zlib = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }
}
