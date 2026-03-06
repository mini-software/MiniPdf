using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

public class DocxToPdfConverterTests
{
    [Fact]
    public void Convert_SimpleDocx_ProducesValidPdf()
    {
        using var docxStream = CreateSimpleDocx("Hello World", "This is a test paragraph.");

        var doc = DocxToPdfConverter.Convert(docxStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Hello World", content);
        Assert.Contains("This is a test paragraph.", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void Convert_WithOptions_UsesCustomSettings()
    {
        using var docxStream = CreateSimpleDocx("Test Header", "Test content");

        var options = new DocxToPdfConverter.ConversionOptions
        {
            FontSize = 14,
            MarginLeft = 72,
            PageWidth = 595, // A4
            PageHeight = 842, // A4
        };

        var doc = DocxToPdfConverter.Convert(docxStream, options);
        Assert.True(doc.Pages.Count >= 1);
        var bytes = doc.ToArray();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Convert_EmptyDocx_CreatesAtLeastOnePage()
    {
        using var docxStream = CreateSimpleDocx();

        var doc = DocxToPdfConverter.Convert(docxStream);
        Assert.True(doc.Pages.Count >= 1);
    }

    [Fact]
    public void ConvertToFile_CreatesOutputFile()
    {
        var docxPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.docx");
        var pdfPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");

        try
        {
            using (var fs = File.Create(docxPath))
            using (var docxStream = CreateSimpleDocx("Test", "File conversion test"))
            {
                docxStream.CopyTo(fs);
            }

            DocxToPdfConverter.ConvertToFile(docxPath, pdfPath);

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(docxPath)) File.Delete(docxPath);
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public void Convert_ManyParagraphs_CreatesMultiplePages()
    {
        var paragraphs = new string[80];
        for (var i = 0; i < 80; i++)
            paragraphs[i] = $"This is paragraph number {i} with enough text to occupy vertical space on the page.";

        using var docxStream = CreateSimpleDocx(paragraphs);
        var doc = DocxToPdfConverter.Convert(docxStream);

        Assert.True(doc.Pages.Count >= 2, $"Expected at least 2 pages, got {doc.Pages.Count}");
    }

    [Fact]
    public void Convert_WithTable_RendersCellText()
    {
        using var docxStream = CreateDocxWithTable(
            new[] { "Name", "Age" },
            new[] { "Alice", "30" },
            new[] { "Bob", "25" }
        );

        var doc = DocxToPdfConverter.Convert(docxStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("Name", content);
        Assert.Contains("Alice", content);
        Assert.Contains("Bob", content);
    }

    [Fact]
    public void Convert_WithBoldText_ProducesPdf()
    {
        using var docxStream = CreateDocxWithBold("Bold text here");

        var doc = DocxToPdfConverter.Convert(docxStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("Bold text here", content);
    }

    [Fact]
    public void Convert_ViaPublicApi_AutoDetectsDocx()
    {
        var docxPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.docx");
        var pdfPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");

        try
        {
            using (var fs = File.Create(docxPath))
            using (var docxStream = CreateSimpleDocx("Public API Test"))
            {
                docxStream.CopyTo(fs);
            }

            MiniPdf.ConvertToPdf(docxPath, pdfPath);

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(bytes));
            Assert.Contains("Public API Test", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(docxPath)) File.Delete(docxPath);
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public void ConvertDocxToPdf_StreamApi_Works()
    {
        using var docxStream = CreateSimpleDocx("Stream API Test");

        var bytes = MiniPdf.ConvertDocxToPdf(docxStream);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Stream API Test", content);
    }

    // ── Helper: Create minimal DOCX ─────────────────────────────────────

    private static MemoryStream CreateSimpleDocx(params string[] paragraphs)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);

            var bodyXml = new StringBuilder();
            foreach (var text in paragraphs)
            {
                bodyXml.Append($"""
                    <w:p>
                      <w:r>
                        <w:t>{EscapeXml(text)}</w:t>
                      </w:r>
                    </w:p>
                    """);
            }

            AddEntry(archive, "word/document.xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                  <w:body>
                    {bodyXml}
                  </w:body>
                </w:document>
                """);
        }

        ms.Position = 0;
        return ms;
    }

    private static MemoryStream CreateDocxWithTable(params string[][] rows)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);

            var tableXml = new StringBuilder();
            tableXml.Append("<w:tbl>");
            foreach (var row in rows)
            {
                tableXml.Append("<w:tr>");
                foreach (var cell in row)
                {
                    tableXml.Append($"""
                        <w:tc>
                          <w:p>
                            <w:r>
                              <w:t>{EscapeXml(cell)}</w:t>
                            </w:r>
                          </w:p>
                        </w:tc>
                        """);
                }
                tableXml.Append("</w:tr>");
            }
            tableXml.Append("</w:tbl>");

            AddEntry(archive, "word/document.xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                  <w:body>
                    {tableXml}
                  </w:body>
                </w:document>
                """);
        }

        ms.Position = 0;
        return ms;
    }

    private static MemoryStream CreateDocxWithBold(string boldText)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "word/document.xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                  <w:body>
                    <w:p>
                      <w:r>
                        <w:rPr>
                          <w:b/>
                        </w:rPr>
                        <w:t>{EscapeXml(boldText)}</w:t>
                      </w:r>
                    </w:p>
                  </w:body>
                </w:document>
                """);
        }

        ms.Position = 0;
        return ms;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
