using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

/// <summary>
/// Covers page-anchored wpg groups: anchor placement, preset and style-driven fills,
/// and text boxes that sit inside a group. All documents are synthetic.
/// </summary>
public class DocxGroupShapeTests
{
    private const float EmuPerPoint = 12700f;

    // 172.8pt x 718.56pt group, placed 4% from the page's left edge and centred vertically.
    private const string SidebarGroup = """
        <wp:anchor behindDoc="1">
          <mc:AlternateContent>
            <mc:Choice Requires="wp14">
              <wp:positionH relativeFrom="page"><wp14:pctPosHOffset>4000</wp14:pctPosHOffset></wp:positionH>
            </mc:Choice>
            <mc:Fallback>
              <wp:positionH relativeFrom="page"><wp:posOffset>310896</wp:posOffset></wp:positionH>
            </mc:Fallback>
          </mc:AlternateContent>
          <wp:positionV relativeFrom="page"><wp:align>center</wp:align></wp:positionV>
          <wp:extent cx="2194560" cy="9125712"/>
          <wp:wrapNone/>
          <a:graphic><a:graphicData><wpg:wgp>
            <wpg:grpSpPr><a:xfrm>
              <a:off x="0" y="0"/><a:ext cx="2194560" cy="9125712"/>
              <a:chOff x="0" y="0"/><a:chExt cx="2194560" cy="9125712"/>
            </a:xfrm></wpg:grpSpPr>
            <wps:wsp>
              <wps:spPr>
                <a:xfrm><a:off x="0" y="0"/><a:ext cx="194535" cy="9125712"/></a:xfrm>
                <a:prstGeom prst="rect"/>
                <a:solidFill><a:srgbClr val="336699"/></a:solidFill>
                <a:ln><a:noFill/></a:ln>
              </wps:spPr>
            </wps:wsp>
            <wps:wsp>
              <wps:spPr>
                <a:xfrm><a:off x="0" y="1466850"/><a:ext cx="2194560" cy="552055"/></a:xfrm>
                <a:prstGeom prst="homePlate"/>
                <a:ln><a:noFill/></a:ln>
              </wps:spPr>
              <wps:style>
                <a:lnRef idx="2"><a:srgbClr val="000000"/></a:lnRef>
                <a:fillRef idx="1"><a:srgbClr val="0000FF"/></a:fillRef>
                <a:fontRef idx="minor"><a:srgbClr val="FFFFFF"/></a:fontRef>
              </wps:style>
              <wps:txbx><w:txbxContent>
                <w:p><w:pPr><w:jc w:val="right"/></w:pPr><w:r><w:t>Dated</w:t></w:r></w:p>
              </w:txbxContent></wps:txbx>
              <wps:bodyPr lIns="91440" tIns="0" rIns="182880" bIns="0" anchor="ctr"/>
            </wps:wsp>
          </wpg:wgp></a:graphicData></a:graphic>
        </wp:anchor>
        """;

    /// <summary>
    /// An anchor with a wp14 percentage position and centre alignment must place the group
    /// against the page, not at the margin origin.
    /// </summary>
    [Fact]
    public void Convert_PageAnchoredGroup_PlacesShapesByPercentAndCentering()
    {
        using var stream = CreateDocx(SidebarGroup);

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        // Frame left = 4% of the 612pt page; frame top = (792 - 718.56) / 2 below the page top.
        var bar = Assert.Single(page.RectBlocks, rect => Math.Abs(rect.Width - 194535 / EmuPerPoint) < 0.1f);
        Assert.InRange(bar.X, 24.3f, 24.7f);
        Assert.InRange(bar.Y, 36.5f, 36.9f);
        Assert.InRange(bar.Height, 718.4f, 718.7f);
    }

    /// <summary>
    /// A homePlate preset must keep its pointed tip, and a fill that only comes from the
    /// shape style's fillRef must still be painted.
    /// </summary>
    [Fact]
    public void Convert_HomePlateWithStyleFill_DrawsPointedShapeInStyleColour()
    {
        using var stream = CreateDocx(SidebarGroup);

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        var pentagon = Assert.Single(page.PolygonBlocks);
        Assert.True(pentagon.FillColor.B > 0.99f && pentagon.FillColor.R < 0.01f);
        var points = pentagon.Subpaths?[0] ?? pentagon.Points;
        Assert.Equal(5, points.Count);
        var tip = points.OrderByDescending(point => point.X).First();
        var tipX = 24.48f + 2194560 / EmuPerPoint;
        Assert.InRange(tip.X, tipX - 0.2f, tipX + 0.2f);
        // The tip sits at mid-height; no other point may share its X.
        Assert.Single(points, point => Math.Abs(point.X - tip.X) < 0.01f);
    }

    /// <summary>
    /// Text in a group child is placed by the child's own rectangle inside the group frame,
    /// takes its colour from the style's fontRef, and stops short of the pentagon tip.
    /// </summary>
    [Fact]
    public void Convert_GroupedPentagonTextBox_StaysInsideShapeTextArea()
    {
        using var stream = CreateDocx(SidebarGroup);

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        var text = Assert.Single(page.TextBlocks, block => block.Text == "Dated");
        Assert.True(text.Color.R > 0.99f && text.Color.G > 0.99f && text.Color.B > 0.99f);

        // Pentagon band: 115.5pt to 159pt below the group's top edge (755.28pt from the page bottom).
        const float bandTop = 755.28f - 1466850 / EmuPerPoint;
        const float bandBottom = bandTop - 552055 / EmuPerPoint;
        Assert.InRange(text.Y, bandBottom, bandTop);

        // Text area right edge = left + width - tip/2 - rIns.
        var textAreaRight = 24.48f + 2194560 / EmuPerPoint - 552055 / EmuPerPoint * 0.25f - 182880 / EmuPerPoint;
        Assert.InRange(text.X, 24.48f, textAreaRight);
    }

    /// <summary>
    /// An outline on a freeform shape follows the path; the converter must not substitute a
    /// rectangle around its bounding box.
    /// </summary>
    [Fact]
    public void Convert_FreeformShapeWithOutline_DoesNotDrawBoundingBox()
    {
        using var stream = CreateDocx(CreateOutlinedShape("""
            <a:custGeom>
              <a:pathLst><a:path w="100" h="100">
                <a:moveTo><a:pt x="0" y="100"/></a:moveTo>
                <a:lnTo><a:pt x="100" y="100"/></a:lnTo>
                <a:lnTo><a:pt x="50" y="0"/></a:lnTo>
                <a:close/>
              </a:path></a:pathLst>
            </a:custGeom>
            """));

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        Assert.Single(page.PolygonBlocks);
        Assert.Empty(page.LineBlocks);
    }

    /// <summary>
    /// A plain rectangle keeps its outline.
    /// </summary>
    [Fact]
    public void Convert_RectangleWithOutline_DrawsFourEdges()
    {
        using var stream = CreateDocx(CreateOutlinedShape("""<a:prstGeom prst="rect"/>"""));

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        Assert.Equal(4, page.LineBlocks.Count);
    }

    [Theory]
    [InlineData("homePlate", 0)]
    [InlineData("rect", 8)]
    public void Convert_HeaderAndFooterOutlinedShapes_OnlyRectanglesDrawBoundingBox(string geometry, int expectedLines)
    {
        using var stream = CreateDocx("", CreateOutlinedShape($"<a:prstGeom prst=\"{geometry}\"/>"));

        var page = Assert.Single(DocxToPdfConverter.Convert(stream).Pages);

        Assert.Equal(expectedLines, page.LineBlocks.Count);
    }

    private static string CreateOutlinedShape(string geometry) => $"""
        <wp:anchor behindDoc="1">
          <wp:positionH relativeFrom="margin"><wp:posOffset>0</wp:posOffset></wp:positionH>
          <wp:positionV relativeFrom="margin"><wp:posOffset>0</wp:posOffset></wp:positionV>
          <wp:extent cx="1270000" cy="1270000"/>
          <wp:wrapNone/>
          <a:graphic><a:graphicData><wpg:wgp>
            <wpg:grpSpPr><a:xfrm>
              <a:off x="0" y="0"/><a:ext cx="1270000" cy="1270000"/>
              <a:chOff x="0" y="0"/><a:chExt cx="1270000" cy="1270000"/>
            </a:xfrm></wpg:grpSpPr>
            <wps:wsp>
              <wps:spPr>
                <a:xfrm><a:off x="0" y="0"/><a:ext cx="1270000" cy="1270000"/></a:xfrm>
                {geometry}
                <a:solidFill><a:srgbClr val="336699"/></a:solidFill>
                <a:ln w="12700"><a:solidFill><a:srgbClr val="000000"/></a:solidFill></a:ln>
              </wps:spPr>
            </wps:wsp>
          </wpg:wgp></a:graphicData></a:graphic>
        </wp:anchor>
        """;

    private static MemoryStream CreateDocx(string anchorXml, string? headerFooterAnchorXml = null)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                  {(headerFooterAnchorXml == null ? "" : """
                  <Override PartName="/word/header1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"/>
                  <Override PartName="/word/footer1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>
                  """)}
                </Types>
                """);
            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);
            if (headerFooterAnchorXml != null)
            {
                AddEntry(archive, "word/_rels/document.xml.rels",
                    """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/header" Target="header1.xml"/>
                      <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer1.xml"/>
                    </Relationships>
                    """);
                foreach (var (part, tag) in new[] { ("header", "hdr"), ("footer", "ftr") })
                {
                    AddEntry(archive, $"word/{part}1.xml",
                        $"""
                        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                        <w:{tag} xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                                  xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
                                  xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                                  xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"
                                  xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                          <w:p><w:r><w:drawing>{headerFooterAnchorXml}</w:drawing></w:r></w:p>
                        </w:{tag}>
                        """);
                }
            }
            AddEntry(archive, "word/document.xml",
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                            xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                            xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
                            xmlns:wp14="http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing"
                            xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                            xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                            xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"
                            xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                  <w:body>
                    <w:p><w:r><w:drawing>{anchorXml}</w:drawing></w:r></w:p>
                    {(headerFooterAnchorXml == null ? "" : """
                    <w:sectPr>
                      <w:headerReference w:type="default" r:id="rId1"/>
                      <w:footerReference w:type="default" r:id="rId2"/>
                    </w:sectPr>
                    """)}
                  </w:body>
                </w:document>
                """);
        }

        stream.Position = 0;
        return stream;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }
}
