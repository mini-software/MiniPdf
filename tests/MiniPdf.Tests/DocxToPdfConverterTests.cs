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

      [Fact]
      public void Convert_FooterPageFieldWithSwitch_RendersPageNumber()
      {
        using var docxStream = CreateDocxWithFooterPageField("PAGE   \\* MERGEFORMAT");

        var doc = DocxToPdfConverter.Convert(docxStream);

        Assert.True(doc.Pages.Count >= 1);
        var texts = doc.Pages[0].TextBlocks.Select(b => b.Text).ToList();
        Assert.Contains("1", texts, StringComparer.Ordinal);
      }

    // ── Helper: Create minimal DOCX ─────────────────────────────────────

    [Fact]
    public void Convert_DocxWithPngImage_ProducesValidXrefOffsets()
    {
        // Create a DOCX with an embedded RGBA PNG image to exercise SMask code path
        using var docxStream = CreateDocxWithPngImage();
        var doc = DocxToPdfConverter.Convert(docxStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("%%EOF", content);

        // Validate every xref entry points to the correct object
        AssertXrefOffsetsAreCorrect(bytes);
    }

    private static void AssertXrefOffsetsAreCorrect(byte[] pdfBytes)
    {
        var text = Encoding.GetEncoding("iso-8859-1").GetString(pdfBytes);

        // Locate xref via startxref
        var startxrefIdx = text.LastIndexOf("startxref", StringComparison.Ordinal);
        Assert.True(startxrefIdx >= 0, "startxref not found");
        var afterStartxref = text.Substring(startxrefIdx + "startxref".Length).Trim();
        var xrefOffset = int.Parse(afterStartxref.Split('\n')[0].Trim());

        Assert.True(text.Substring(xrefOffset).StartsWith("xref\n"), "xref keyword not found at startxref offset");

        var xrefSection = text.Substring(xrefOffset);
        var trailerIdx = xrefSection.IndexOf("trailer", StringComparison.Ordinal);
        Assert.True(trailerIdx > 0, "trailer not found");

        var lines = xrefSection.Substring(0, trailerIdx).Trim().Split('\n');
        var parts = lines[1].Split(' ');
        var startObj = int.Parse(parts[0]);
        var numObjs = int.Parse(parts[1]);

        for (var i = 2; i < 2 + numObjs && i < lines.Length; i++)
        {
            var entry = lines[i];
            if (entry.Contains('f'))
                continue;
            var offset = int.Parse(entry.Substring(0, 10));
            var objNum = startObj + i - 2;
            var expected = $"{objNum} 0 obj";
            var actual = Encoding.GetEncoding("iso-8859-1").GetString(
                pdfBytes, offset, Math.Min(expected.Length, pdfBytes.Length - offset));
            Assert.True(actual == expected,
                $"xref obj {objNum}: offset {offset} points to '{actual}', expected '{expected}'");
        }
    }

    private static MemoryStream CreateDocxWithPngImage()
    {
        var ms = new MemoryStream();

        // Create a minimal 4x4 RGBA PNG (with alpha channel to trigger SMask)
        var pngBytes = CreateMinimalRgbaPng(4, 4);

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="png" ContentType="image/png"/>
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

            AddEntry(archive, "word/_rels/document.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId10" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/image1.png"/>
                </Relationships>
                """);

            AddEntry(archive, "word/document.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                            xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                            xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
                            xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                            xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture">
                  <w:body>
                    <w:p>
                      <w:r><w:t>Before image</w:t></w:r>
                    </w:p>
                    <w:p>
                      <w:r>
                        <w:drawing>
                          <wp:inline distT="0" distB="0" distL="0" distR="0">
                            <wp:extent cx="914400" cy="914400"/>
                            <wp:docPr id="1" name="Picture 1"/>
                            <a:graphic>
                              <a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">
                                <pic:pic>
                                  <pic:nvPicPr>
                                    <pic:cNvPr id="1" name="image1.png"/>
                                    <pic:cNvPicPr/>
                                  </pic:nvPicPr>
                                  <pic:blipFill>
                                    <a:blip r:embed="rId10"/>
                                    <a:stretch><a:fillRect/></a:stretch>
                                  </pic:blipFill>
                                  <pic:spPr>
                                    <a:xfrm>
                                      <a:off x="0" y="0"/>
                                      <a:ext cx="914400" cy="914400"/>
                                    </a:xfrm>
                                    <a:prstGeom prst="rect"><a:avLst/></a:prstGeom>
                                  </pic:spPr>
                                </pic:pic>
                              </a:graphicData>
                            </a:graphic>
                          </wp:inline>
                        </w:drawing>
                      </w:r>
                    </w:p>
                    <w:p>
                      <w:r><w:t>After image</w:t></w:r>
                    </w:p>
                  </w:body>
                </w:document>
                """);

            // Add the PNG image as binary entry
            var imgEntry = archive.CreateEntry("word/media/image1.png");
            using (var imgStream = imgEntry.Open())
                imgStream.Write(pngBytes, 0, pngBytes.Length);
        }

        ms.Position = 0;
        return ms;
    }

    /// <summary>Creates a minimal valid RGBA PNG file (with alpha channel).</summary>
    private static byte[] CreateMinimalRgbaPng(int width, int height)
    {
        using var ms = new MemoryStream();
        // PNG signature
        ms.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

        // IHDR chunk
        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 6;  // color type: RGBA
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace
        WritePngChunk(ms, "IHDR", ihdr);

        // IDAT chunk: raw scanlines with filter byte 0 (None) per row
        var rawData = new byte[height * (1 + width * 4)]; // filter byte + RGBA per pixel
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * (1 + width * 4);
            rawData[rowStart] = 0; // filter: None
            for (var x = 0; x < width; x++)
            {
                var px = rowStart + 1 + x * 4;
                rawData[px] = 0x33;     // R
                rawData[px + 1] = 0x66; // G
                rawData[px + 2] = 0x99; // B
                rawData[px + 3] = 0x80; // A (semi-transparent)
            }
        }

        // Compress with zlib
        using var compressedMs = new MemoryStream();
        using (var deflate = new System.IO.Compression.ZLibStream(compressedMs,
            System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
            deflate.Write(rawData, 0, rawData.Length);
        WritePngChunk(ms, "IDAT", compressedMs.ToArray());

        // IEND chunk
        WritePngChunk(ms, "IEND", Array.Empty<byte>());

        return ms.ToArray();
    }

    private static void WritePngChunk(MemoryStream ms, string type, byte[] data)
    {
        var lenBytes = new byte[4];
        WriteBigEndian(lenBytes, 0, data.Length);
        ms.Write(lenBytes, 0, 4);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        ms.Write(typeBytes, 0, 4);
        ms.Write(data, 0, data.Length);

        // CRC32 over type + data
        var crcInput = new byte[4 + data.Length];
        Array.Copy(typeBytes, 0, crcInput, 0, 4);
        Array.Copy(data, 0, crcInput, 4, data.Length);
        var crc = ComputeCrc32(crcInput);
        var crcBytes = new byte[4];
        WriteBigEndian(crcBytes, 0, (int)crc);
        ms.Write(crcBytes, 0, 4);
    }

    private static void WriteBigEndian(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value >> 24);
        buf[offset + 1] = (byte)(value >> 16);
        buf[offset + 2] = (byte)(value >> 8);
        buf[offset + 3] = (byte)value;
    }

    private static uint ComputeCrc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (var j = 0; j < 8; j++)
                crc = (crc >> 1) ^ (0xEDB88320 & ~((crc & 1) - 1));
        }
        return crc ^ 0xFFFFFFFF;
    }

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

    private static MemoryStream CreateDocxWithFooterPageField(string fieldInstruction)
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
                  <Override PartName="/word/footer1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "word/_rels/document.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId10" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer1.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "word/document.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <w:body>
                    <w:p>
                      <w:r><w:t>Body text</w:t></w:r>
                    </w:p>
                    <w:sectPr>
                      <w:footerReference w:type="default" r:id="rId10"/>
                    </w:sectPr>
                  </w:body>
                </w:document>
                """);

            AddEntry(archive, "word/footer1.xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:ftr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                  <w:p>
                    <w:pPr>
                      <w:jc w:val="center"/>
                    </w:pPr>
                    <w:r><w:fldChar w:fldCharType="begin"/></w:r>
                    <w:r><w:instrText>{EscapeXml(fieldInstruction)}</w:instrText></w:r>
                    <w:r><w:fldChar w:fldCharType="separate"/></w:r>
                    <w:r><w:t>9</w:t></w:r>
                    <w:r><w:fldChar w:fldCharType="end"/></w:r>
                  </w:p>
                </w:ftr>
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
