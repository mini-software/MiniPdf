using System.IO.Compression;
using System.Text;
using System.Text.Json;
using MiniSoftware;

namespace MiniPdf.Tests;

public class ContentExtractionTests
{
    [Fact]
    public void Docx_ExtractsHeadingsParagraphsAndTables()
    {
        using var stream = CreateDocx();

        var content = MiniSoftware.MiniPdf.ExtractContent(stream);

        Assert.Equal("docx", content.SourceFormat);
        var section = Assert.Single(content.Sections);
        Assert.Equal("Document title", section.Title);
        var heading = Assert.IsType<MiniPdfHeadingBlock>(section.Blocks[0]);
        Assert.Equal(1, heading.Level);
        Assert.Equal("Document title", Assert.Single(heading.Runs).Text);
        var linkedParagraph = Assert.IsType<MiniPdfParagraphBlock>(section.Blocks[1]);
        var linkedRun = Assert.Single(linkedParagraph.Runs);
        Assert.Equal("Body link", linkedRun.Text);
        Assert.Equal("https://example.com/docx", linkedRun.Link);
        var table = Assert.IsType<MiniPdfTableBlock>(section.Blocks.Last());
        Assert.Equal("A", table.Rows[0].Cells[0].Text);
        Assert.Equal("B", table.Rows[0].Cells[1].Text);

        stream.Position = 0;
        var markdown = MiniSoftware.MiniPdf.ConvertToMarkdown(stream);
        Assert.Contains("# Document title", markdown);
        Assert.Contains("[Body link](https://example.com/docx)", markdown);
        Assert.Contains("| A | B |", markdown);

        stream.Position = 0;
        using var json = JsonDocument.Parse(MiniSoftware.MiniPdf.ConvertToJson(stream));
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("docx", json.RootElement.GetProperty("sourceFormat").GetString());
        Assert.Equal("https://example.com/docx", json.RootElement
          .GetProperty("sections")[0].GetProperty("blocks")[1].GetProperty("runs")[0].GetProperty("link").GetString());
    }

    [Fact]
    public void Xlsx_ExtractsGridWithAddressesAndLimits()
    {
        using var stream = CreateXlsx();
        var options = new MiniPdfContentOptions { MaxRows = 1, MaxColumns = 2 };

        var content = MiniSoftware.MiniPdf.ExtractContent(stream, options);

        var section = Assert.Single(content.Sections);
        Assert.Equal("worksheet", section.Kind);
        Assert.Equal("Data", section.Title);
        var table = Assert.IsType<MiniPdfTableBlock>(section.Blocks[0]);
        var row = Assert.Single(table.Rows);
        Assert.Equal(new[] { "A1", "B1" }, row.Cells.Select(cell => cell.Address).ToArray());
        Assert.Equal(new[] { "Name", "Value" }, row.Cells.Select(cell => cell.Text).ToArray());
        Assert.Equal("https://example.com/xlsx", row.Cells[0].Link);

        stream.Position = 0;
        var markdown = MiniSoftware.MiniPdf.ConvertToMarkdown(stream, options);
        Assert.Contains("# Worksheet: Data", markdown);
        Assert.Contains("| [Name](https://example.com/xlsx) | Value |", markdown);
    }

    [Fact]
    public void Pptx_UsesVisualReadingOrder()
    {
        using var stream = CreatePptx();

        var content = MiniSoftware.MiniPdf.ExtractContent(stream);

        var section = Assert.Single(content.Sections);
        Assert.Equal("slide", section.Kind);
        Assert.Equal("Top title", section.Title);
        Assert.Equal("Top title", GetBlockText(section.Blocks[0]));
        Assert.Equal("Lower body", GetBlockText(section.Blocks[1]));
        var body = Assert.IsType<MiniPdfParagraphBlock>(section.Blocks[1]);
        Assert.Equal("https://example.com/pptx", Assert.Single(body.Runs).Link);

        stream.Position = 0;
        var markdown = MiniSoftware.MiniPdf.ConvertToMarkdown(stream);
        Assert.StartsWith("# Slide 1: Top title\n\n# Top title", markdown);
        Assert.Contains("[Lower body](https://example.com/pptx)", markdown);
    }

    [Fact]
    public void ExtractContent_RestoresNonZeroSeekableStreamPosition()
    {
        using var package = CreateDocx();
        using var prefixed = new MemoryStream();
        prefixed.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        package.CopyTo(prefixed);
        prefixed.Position = 4;

        var content = MiniSoftware.MiniPdf.ExtractContent(prefixed);

        Assert.Equal("docx", content.SourceFormat);
        Assert.Equal(4, prefixed.Position);
    }

    [Fact]
    public void ContentOptions_RejectExcelOnlySettingsForDocx()
    {
        using var stream = CreateDocx();

        var exception = Assert.Throws<NotSupportedException>(() =>
            MiniSoftware.MiniPdf.ExtractContent(stream, new MiniPdfContentOptions { MaxRows = 1 }));

        Assert.Contains("only supported for .xlsx", exception.Message);
    }

    private static string GetBlockText(MiniPdfContentBlock block)
        => block switch
        {
            MiniPdfHeadingBlock heading => string.Concat(heading.Runs.Select(run => run.Text)),
            MiniPdfParagraphBlock paragraph => string.Concat(paragraph.Runs.Select(run => run.Text)),
            _ => string.Empty,
        };

    private static MemoryStream CreateDocx()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                </Types>
                """);
            AddEntry(archive, "_rels/.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "word/_rels/document.xml.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rIdLink" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink" Target="https://example.com/docx" TargetMode="External"/>
                </Relationships>
                """);
            AddEntry(archive, "word/document.xml",
                """
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                            xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <w:body>
                    <w:p><w:pPr><w:outlineLvl w:val="0"/></w:pPr><w:r><w:t>Document title</w:t></w:r></w:p>
                    <w:p><w:hyperlink r:id="rIdLink"><w:r><w:t>Body link</w:t></w:r></w:hyperlink></w:p>
                    <w:tbl><w:tr>
                      <w:tc><w:p><w:r><w:t>A</w:t></w:r></w:p></w:tc>
                      <w:tc><w:p><w:r><w:t>B</w:t></w:r></w:p></w:tc>
                    </w:tr></w:tbl>
                  </w:body>
                </w:document>
                """);
        }
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateXlsx()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);
            AddEntry(archive, "_rels/.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/workbook.xml",
                """
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Data" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddEntry(archive, "xl/sharedStrings.xml",
                """
                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="4" uniqueCount="4">
                  <si><t>Name</t></si><si><t>Value</t></si><si><t>Alpha</t></si><si><t>42</t></si>
                </sst>
                """);
            AddEntry(archive, "xl/worksheets/_rels/sheet1.xml.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rIdLink" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink" Target="https://example.com/xlsx" TargetMode="External"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/worksheets/sheet1.xml",
                """
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                           xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheetData>
                    <row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="s"><v>1</v></c></row>
                    <row r="2"><c r="A2" t="s"><v>2</v></c><c r="B2" t="s"><v>3</v></c></row>
                  </sheetData>
                  <hyperlinks><hyperlink ref="A1" r:id="rIdLink"/></hyperlinks>
                </worksheet>
                """);
        }
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreatePptx()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
                  <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
                </Types>
                """);
            AddEntry(archive, "_rels/.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "ppt/presentation.xml",
                """
                <p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <p:sldSz cx="9144000" cy="6858000"/>
                  <p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst>
                </p:presentation>
                """);
            AddEntry(archive, "ppt/_rels/presentation.xml.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "ppt/slides/_rels/slide1.xml.rels",
                """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rIdLink" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink" Target="https://example.com/pptx" TargetMode="External"/>
                </Relationships>
                """);
            AddEntry(archive, "ppt/slides/slide1.xml",
                """
                <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                       xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                       xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <p:cSld><p:spTree>
                    <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                    <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                    <p:sp><p:nvSpPr><p:cNvPr id="2" name="Body"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr>
                      <p:spPr><a:xfrm><a:off x="500000" y="3000000"/><a:ext cx="5000000" cy="1000000"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></p:spPr>
                      <p:txBody><a:bodyPr/><a:lstStyle/><a:p><a:r><a:rPr><a:hlinkClick r:id="rIdLink"/></a:rPr><a:t>Lower body</a:t></a:r></a:p></p:txBody>
                    </p:sp>
                    <p:sp><p:nvSpPr><p:cNvPr id="3" name="Title"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr>
                      <p:spPr><a:xfrm><a:off x="500000" y="500000"/><a:ext cx="5000000" cy="1000000"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></p:spPr>
                      <p:txBody><a:bodyPr/><a:lstStyle/><a:p><a:r><a:t>Top title</a:t></a:r></a:p></p:txBody>
                    </p:sp>
                  </p:spTree></p:cSld>
                </p:sld>
                """);
        }
        stream.Position = 0;
        return stream;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
