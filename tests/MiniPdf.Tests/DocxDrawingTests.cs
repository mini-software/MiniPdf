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
    /// A wrapTopAndBottom text box anchored to the page or to the margin must be exposed as a
    /// floating box on its host paragraph. Its offset is a page coordinate, not spacing, so
    /// neither the box content nor the host paragraph may be pushed down the flow by it.
    /// </summary>
    [Theory]
    [InlineData("page", 1333500, 105f)]
    [InlineData("margin", 419100, 33f)]
    public void Read_AbsoluteWrapTopAndBottomTextBox_IsFloatingBox(string relativeFrom, int posOffsetEmu, float expectedYPt)
    {
        using var stream = CreateDocxWithAbsoluteWrapTopAndBottomTextBox(relativeFrom, posOffsetEmu);

        var document = DocxReader.Read(stream);
        var paragraphs = document.Elements.OfType<DocxParagraph>().ToArray();

        Assert.DoesNotContain(paragraphs, paragraph => paragraph.Runs.Any(run => run.Text == "Boxed"));
        var host = Assert.Single(paragraphs, paragraph => paragraph.Runs.Any(run => run.Text == "Host"));
        var box = Assert.Single(host.FloatingTextBoxes ?? []);
        Assert.True(box.IsWrapTopBottom);
        Assert.Equal(relativeFrom, box.VRelativeFrom);
        Assert.InRange(box.YPt, expectedYPt - 0.1f, expectedYPt + 0.1f);
        Assert.InRange(host.SpacingBefore, -0.01f, 0.01f);
    }

    /// <summary>
    /// The text flow may not run beside a wrapTopAndBottom box: the host line stays above the
    /// box, the box text renders inside the box band, and the next paragraph resumes below it.
    /// Both anchors put the box 105pt below the page top (the margin case is 33pt below the
    /// 72pt top margin).
    /// </summary>
    [Theory]
    [InlineData("page", 1333500)]
    [InlineData("margin", 419100)]
    public void Convert_AbsoluteWrapTopAndBottomTextBox_ResumesFlowBelowBox(string relativeFrom, int posOffsetEmu)
    {
        using var stream = CreateDocxWithAbsoluteWrapTopAndBottomTextBox(relativeFrom, posOffsetEmu);

        var document = DocxToPdfConverter.Convert(stream);

        var page = Assert.Single(document.Pages);
        var host = Assert.Single(page.TextBlocks, block => block.Text == "Host");
        var boxed = Assert.Single(page.TextBlocks, block => block.Text == "Boxed");
        var after = Assert.Single(page.TextBlocks, block => block.Text == "After");

        // The box occupies 105pt to 145pt from the top of the 792pt page.
        const float bandTop = 792f - 105f;
        const float bandBottom = 792f - 145f;
        Assert.InRange(boxed.Y, bandBottom, bandTop);
        Assert.True(host.Y > bandTop, $"Host baseline {host.Y} must stay above the box top {bandTop}.");
        Assert.True(after.Y < bandBottom, $"Following baseline {after.Y} must resume below the box bottom {bandBottom}.");
    }

    [Fact]
    public void Convert_AbsoluteWrapTopAndBottomTextBox_MixedFormatContinuationLinesAvoidBox()
    {
        var continuationText = string.Join(" ", Enumerable.Repeat("continuation", 40));
        var afterParagraph = $"""
            <w:p>
              <w:r><w:rPr><w:b/></w:rPr><w:t>After </w:t></w:r>
              <w:r><w:t>{continuationText}</w:t></w:r>
            </w:p>
            """;
        using var stream = CreateDocxWithAbsoluteWrapTopAndBottomTextBox(
            "page", 1841500, afterParagraph);

        var document = DocxToPdfConverter.Convert(stream);

        var page = Assert.Single(document.Pages);
        var bodyBlocks = page.TextBlocks.Where(block => block.Text != "Boxed").ToArray();
        const float bandTop = 792f - 145f;
        const float bandBottom = 792f - 185f;
        Assert.Contains(bodyBlocks, block => block.Text.Contains("continuation") && block.Y < bandBottom);
        Assert.DoesNotContain(bodyBlocks, block => block.Y < bandTop && block.Y > bandBottom);
    }

    [Fact]
    public void Convert_OverlappingWrapTopAndBottomTextBoxes_RescansEarlierObstacles()
    {
        var afterParagraphs = string.Join("", Enumerable.Range(1, 12)
            .Select(index => $"<w:p><w:r><w:t>After {index}</w:t></w:r></w:p>"));
        using var stream = CreateDocxWithAbsoluteWrapTopAndBottomTextBox(
            "page", 2222500, afterParagraphs, secondPosOffsetEmu: 1841500);

        var document = DocxToPdfConverter.Convert(stream);

        var page = Assert.Single(document.Pages);
        var bodyBlocks = page.TextBlocks
            .Where(block => !block.Text.StartsWith("Boxed", StringComparison.Ordinal))
            .ToArray();
        const float upperBandTop = 792f - 145f;
        const float lowerBandBottom = 792f - 215f;
        Assert.Contains(bodyBlocks,
            block => block.Text.StartsWith("After", StringComparison.Ordinal) && block.Y < lowerBandBottom);
        Assert.DoesNotContain(bodyBlocks, block => block.Y < upperBandTop && block.Y > lowerBandBottom);
    }

    [Fact]
    public void Convert_WrapTopAndBottomTextBoxBelowMargin_MovesContinuationToNextPage()
    {
        var continuationText = string.Join(" ", Enumerable.Repeat("continuation", 400));
        var afterParagraph = $"""
            <w:p>
              <w:r><w:rPr><w:b/></w:rPr><w:t>After </w:t></w:r>
              <w:r><w:t>{continuationText}</w:t></w:r>
            </w:p>
            """;
        using var stream = CreateDocxWithAbsoluteWrapTopAndBottomTextBox(
            "page", 8255000, afterParagraph, boxHeightEmu: 1270000);

        var document = DocxToPdfConverter.Convert(stream);

        Assert.True(document.Pages.Count >= 2);
        Assert.DoesNotContain(document.Pages[0].TextBlocks,
            block => block.Text != "Boxed" && block.Y < 72f);
        Assert.Contains(document.Pages.Skip(1).SelectMany(page => page.TextBlocks),
            block => block.Text.Contains("continuation"));
    }

    /// <summary>
    /// Creates a minimal DOCX with three paragraphs; the second hosts a wrapTopAndBottom text
    /// box whose vertical anchor uses the given relativeFrom value and EMU offset.
    /// </summary>
    private static MemoryStream CreateDocxWithAbsoluteWrapTopAndBottomTextBox(
        string relativeFrom, int posOffsetEmu,
        string afterParagraph = "<w:p><w:r><w:t>After</w:t></w:r></w:p>",
        int boxHeightEmu = 508000, int? secondPosOffsetEmu = null)
    {
        var secondTextBoxRun = secondPosOffsetEmu.HasValue
            ? $"""
              <w:r>
                <w:drawing>
                  <wp:anchor distT="0" distB="0" distL="114300" distR="114300" simplePos="0" relativeHeight="251659265" behindDoc="0" locked="0" layoutInCell="1" allowOverlap="1">
                    <wp:simplePos x="0" y="0"/>
                    <wp:positionH relativeFrom="column"><wp:posOffset>0</wp:posOffset></wp:positionH>
                    <wp:positionV relativeFrom="{relativeFrom}"><wp:posOffset>{secondPosOffsetEmu.Value}</wp:posOffset></wp:positionV>
                    <wp:extent cx="2540000" cy="{boxHeightEmu}"/>
                    <wp:wrapTopAndBottom/>
                    <wp:docPr id="2" name="Text Box 2"/>
                    <a:graphic>
                      <a:graphicData uri="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                        <wps:wsp>
                          <wps:spPr>
                            <a:xfrm><a:off x="0" y="0"/><a:ext cx="2540000" cy="{boxHeightEmu}"/></a:xfrm>
                            <a:prstGeom prst="rect"><a:avLst/></a:prstGeom>
                            <a:noFill/>
                          </wps:spPr>
                          <wps:txbx><w:txbxContent><w:p><w:r><w:t>Boxed2</w:t></w:r></w:p></w:txbxContent></wps:txbx>
                          <wps:bodyPr/>
                        </wps:wsp>
                      </a:graphicData>
                    </a:graphic>
                  </wp:anchor>
                </w:drawing>
              </w:r>
              """
            : "";
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
                $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                            xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
                            xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                            xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                  <w:body>
                    <w:p><w:r><w:t>Intro</w:t></w:r></w:p>
                    <w:p>
                      <w:r><w:t>Host</w:t></w:r>
                      <w:r>
                        <w:drawing>
                          <wp:anchor distT="0" distB="0" distL="114300" distR="114300" simplePos="0" relativeHeight="251659264" behindDoc="0" locked="0" layoutInCell="1" allowOverlap="1">
                            <wp:simplePos x="0" y="0"/>
                            <wp:positionH relativeFrom="column"><wp:posOffset>0</wp:posOffset></wp:positionH>
                            <wp:positionV relativeFrom="{relativeFrom}"><wp:posOffset>{posOffsetEmu}</wp:posOffset></wp:positionV>
                            <wp:extent cx="2540000" cy="{boxHeightEmu}"/>
                            <wp:wrapTopAndBottom/>
                            <wp:docPr id="1" name="Text Box 1"/>
                            <a:graphic>
                              <a:graphicData uri="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">
                                <wps:wsp>
                                  <wps:spPr>
                                    <a:xfrm><a:off x="0" y="0"/><a:ext cx="2540000" cy="{boxHeightEmu}"/></a:xfrm>
                                    <a:prstGeom prst="rect"><a:avLst/></a:prstGeom>
                                    <a:noFill/>
                                  </wps:spPr>
                                  <wps:txbx>
                                    <w:txbxContent>
                                      <w:p><w:r><w:t>Boxed</w:t></w:r></w:p>
                                    </w:txbxContent>
                                  </wps:txbx>
                                  <wps:bodyPr/>
                                </wps:wsp>
                              </a:graphicData>
                            </a:graphic>
                          </wp:anchor>
                        </w:drawing>
                      </w:r>
                      {secondTextBoxRun}
                    </w:p>
                    {afterParagraph}
                    <w:sectPr>
                      <w:pgSz w:w="12240" w:h="15840"/>
                      <w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="720" w:footer="720" w:gutter="0"/>
                    </w:sectPr>
                  </w:body>
                </w:document>
                """);
        }

        stream.Position = 0;
        return stream;
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