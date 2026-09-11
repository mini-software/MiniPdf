using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

/// <summary>
/// Covers DrawingML parsing behavior that is independent of repository issue fixtures.
/// </summary>
public class DocxDrawingTests
{
    /// <summary>
    /// A run containing multiple drawings must process each drawing independently when a later
    /// grouped drawing carries text box content.
    /// </summary>
    [Fact]
    public void Read_MultipleDrawingsInTextBoxHostRun_ReadsEachTopLevelDrawing()
    {
        using var stream = CreateDocxWithMultipleDrawingsAndGroupTextBox();

        var document = DocxReader.Read(stream);
        var shapes = document.Elements
            .OfType<DocxParagraph>()
            .SelectMany(paragraph => paragraph.Shapes ?? [])
            .ToArray();

        Assert.Equal(2, shapes.Length);
        Assert.Contains(shapes, shape => shape.FillColor.R > 0.99f && shape.FillColor.B < 0.01f);
        Assert.Contains(shapes, shape => shape.FillColor.B > 0.99f && shape.FillColor.R < 0.01f);
    }

    /// <summary>
    /// Creates a minimal DOCX whose single run has a simple shape followed by a grouped shape
    /// with an empty text box.
    /// </summary>
    private static MemoryStream CreateDocxWithMultipleDrawingsAndGroupTextBox()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
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
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                            xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
                            xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                            xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"
                            xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                  <w:body>
                    <w:p>
                      <w:r>
                        <w:drawing>
                          <wp:anchor behindDoc="1">
                            <wp:positionH relativeFrom="page"><wp:posOffset>0</wp:posOffset></wp:positionH>
                            <wp:positionV relativeFrom="page"><wp:posOffset>0</wp:posOffset></wp:positionV>
                            <wp:extent cx="914400" cy="914400"/>
                            <a:graphic><a:graphicData><wps:wsp><wps:spPr>
                              <a:solidFill><a:srgbClr val="FF0000"/></a:solidFill>
                              <a:prstGeom prst="rect"/>
                            </wps:spPr></wps:wsp></a:graphicData></a:graphic>
                          </wp:anchor>
                        </w:drawing>
                        <w:drawing>
                          <wp:anchor behindDoc="1">
                            <wp:positionH relativeFrom="page"><wp:posOffset>914400</wp:posOffset></wp:positionH>
                            <wp:positionV relativeFrom="page"><wp:posOffset>0</wp:posOffset></wp:positionV>
                            <wp:extent cx="914400" cy="914400"/>
                            <a:graphic><a:graphicData><wpg:wgp>
                              <wpg:grpSpPr><a:xfrm>
                                <a:off x="0" y="0"/><a:ext cx="914400" cy="914400"/>
                                <a:chOff x="0" y="0"/><a:chExt cx="914400" cy="914400"/>
                              </a:xfrm></wpg:grpSpPr>
                              <wps:wsp>
                                <wps:spPr>
                                  <a:xfrm><a:off x="0" y="0"/><a:ext cx="914400" cy="914400"/></a:xfrm>
                                  <a:solidFill><a:srgbClr val="0000FF"/></a:solidFill>
                                  <a:prstGeom prst="rect"/>
                                </wps:spPr>
                                <wps:txbx><w:txbxContent><w:p/></w:txbxContent></wps:txbx>
                              </wps:wsp>
                            </wpg:wgp></a:graphicData></a:graphic>
                          </wp:anchor>
                        </w:drawing>
                      </w:r>
                    </w:p>
                  </w:body>
                </w:document>
                """);
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Adds a UTF-8 XML part to a DOCX archive.
    /// </summary>
    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }
}