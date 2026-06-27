using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

public class PptxToPdfConverterTests
{
    [Fact]
    public void Convert_SimplePptx_ProducesValidPdf()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(
                new[]
                {
                    PptxShapeSpec.TextBox("Hello PPTX", 914400, 914400, 4000000, 900000, fontSize: 2400, color: "FF0000"),
                }));

        var doc = PptxToPdfConverter.Convert(pptxStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Single(doc.Pages);
        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Hello PPTX", content);
        Assert.Contains("1.000 0.000 0.000 rg", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void Convert_TwoSlides_PreservesPresentationOrder()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("First Slide", 914400, 914400, 3000000, 700000) }),
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("Second Slide", 914400, 914400, 3000000, 700000) }));

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Equal(2, doc.Pages.Count);
        Assert.Contains(doc.Pages[0].TextBlocks, block => block.Text.Contains("First Slide", StringComparison.Ordinal));
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text.Contains("Second Slide", StringComparison.Ordinal));
    }

    [Fact]
    public void Convert_CustomSlideSize_UsesPresentationDimensions()
    {
        using var pptxStream = CreatePptx(
            slideWidthEmu: 12192000,
            slideHeightEmu: 6858000,
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("Wide", 914400, 914400, 3000000, 700000) }));

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Single(doc.Pages);
        Assert.Equal(960f, doc.Pages[0].Width, precision: 2);
        Assert.Equal(540f, doc.Pages[0].Height, precision: 2);
    }

    [Fact]
    public void Convert_WithBasicShapes_RendersFillAndOutline()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(
                new[]
                {
                    PptxShapeSpec.Shape("rect", 914400, 914400, 1800000, 1000000, fillColor: "00FF00", outlineColor: "0000FF"),
                    PptxShapeSpec.Shape("ellipse", 3000000, 914400, 1300000, 1300000, fillColor: "FFFF00"),
                }));

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Contains(doc.Pages[0].RectBlocks, rect => rect.FillColor == PdfColor.FromHex("00FF00"));
        Assert.Single(doc.Pages[0].EllipseBlocks);
        Assert.True(doc.Pages[0].LineBlocks.Count >= 4);
    }

    [Fact]
    public void Convert_WithPngPicture_RendersImage()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(
                new[]
                {
                    PptxShapeSpec.Picture("rIdImage1", 914400, 914400, 1200000, 1200000),
                },
                new[]
                {
                    new PptxImageSpec("rIdImage1", "../media/image1.png", SmallPng),
                }));

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Single(doc.Pages[0].ImageBlocks);
        Assert.Equal("png", doc.Pages[0].ImageBlocks[0].Format);
    }

    [Fact]
    public void Convert_WithOfficeSvgBlip_RendersVectorPath()
    {
        using var pptxStream = CreatePptxWithOfficeSvgBlip();

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Empty(doc.Pages[0].ImageBlocks);
        Assert.NotEmpty(doc.Pages[0].PathBlocks);
        Assert.Contains(doc.Pages[0].PathBlocks, path => path.FillColor == PdfColor.FromHex("E56925"));
    }

    [Fact]
    public void Convert_WithOfficeSvgBlipCrop_AppliesSourceRectangle()
    {
        using var pptxStream = CreatePptxWithOfficeSvgBlip(" l=\"25000\" t=\"25000\" r=\"25000\" b=\"25000\"");

        var doc = PptxToPdfConverter.Convert(pptxStream);
        var move = doc.Pages[0].PathBlocks[0].Commands.First(command => command.Op == 'M');

        Assert.True(move.Values[0] < 72f, $"Expected cropped SVG path to extend left of picture frame, got x={move.Values[0]}");
    }

    [Fact]
    public void Convert_WithMasterColorMap_ResolvesLayoutBackgroundSchemeColor()
    {
        using var pptxStream = CreatePptxWithMappedLayoutBackground();

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Contains(doc.Pages[0].RectBlocks, rect =>
            rect.X == 0 && rect.Y == 0 && rect.Width == doc.Pages[0].Width && rect.Height == doc.Pages[0].Height &&
            rect.FillColor == PdfColor.White);
    }

    [Fact]
    public void Convert_WithLuminosityTransform_AdjustsHslLuminance()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(
                new[]
                {
                    new PptxShapeSpec(
                        "rect",
                        914400,
                        914400,
                        1800000,
                        1000000,
                        FillXml: "<a:solidFill><a:srgbClr val=\"B3DAD6\"><a:lumMod val=\"75000\"/></a:srgbClr></a:solidFill>"),
                }));

        var doc = PptxToPdfConverter.Convert(pptxStream);
        var transformedRect = Assert.Single(doc.Pages[0].RectBlocks, rect => rect.FillColor != PdfColor.White);

        Assert.Equal(112 / 255f, transformedRect.FillColor.R, precision: 2);
        Assert.Equal(186 / 255f, transformedRect.FillColor.G, precision: 2);
        Assert.Equal(178 / 255f, transformedRect.FillColor.B, precision: 2);
    }

    [Fact]
    public void Convert_WithInheritedPlaceholderTextLayout_PreservesAlignmentAndSpacing()
    {
        using var pptxStream = CreatePptxWithInheritedPlaceholderTextLayout();

        var doc = PptxToPdfConverter.Convert(pptxStream);
        var title = Assert.Single(doc.Pages[0].TextBlocks, block => block.Text == "Centered Title");
        var bullet = Assert.Single(doc.Pages[0].TextBlocks, block => block.Text == "\u2022");
        var bulletText = Assert.Single(doc.Pages[0].TextBlocks, block => block.Text == "Indented bullet");
        var secondHeading = Assert.Single(doc.Pages[0].TextBlocks, block => block.Text == "Second heading");

        Assert.True(title.X > 120f, $"Expected inherited center alignment to move title right, got x={title.X}");
        Assert.True(bulletText.X - bullet.X > 15f, $"Expected hanging bullet indent, got delta={bulletText.X - bullet.X}");
        Assert.True(bulletText.Y - secondHeading.Y > 35f, $"Expected empty paragraph to add vertical spacing, got delta={bulletText.Y - secondHeading.Y}");
    }

    [Fact]
    public void Convert_WithDiagramDrawing_RendersSmartArtFallback()
    {
        using var pptxStream = CreatePptxWithDiagramDrawing();

        var doc = PptxToPdfConverter.Convert(pptxStream);

        Assert.Contains(doc.Pages[0].TextBlocks, block => block.Text.Contains("SmartArt Text", StringComparison.Ordinal));
        Assert.Contains(doc.Pages[0].RectBlocks, rect => rect.FillColor == PdfColor.FromHex("2F5597"));
    }

    [Fact]
    public void ConvertToPdf_StreamApi_AutoDetectsPptx()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("AutoDetect PPTX", 914400, 914400, 3000000, 700000) }));

        var bytes = MiniPdf.ConvertToPdf(pptxStream);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("AutoDetect PPTX", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void ConvertToPdf_StreamApi_WorksWithNonSeekablePptxStream()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("NonSeekable PPTX", 914400, 914400, 3000000, 700000) }));
        using var nonSeekable = new NonSeekableStream(pptxStream);

        var bytes = MiniPdf.ConvertToPdf(nonSeekable);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("NonSeekable PPTX", content);
        Assert.True(bytes.Length > 500, $"Expected non-trivial PDF, got {bytes.Length} bytes");
    }

    [Fact]
    public void ConvertPptxToPdf_WorksWithNonSeekableStream()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("Explicit PPTX API", 914400, 914400, 3000000, 700000) }));
        using var nonSeekable = new NonSeekableStream(pptxStream);

        var bytes = MiniPdf.ConvertPptxToPdf(nonSeekable);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Explicit PPTX API", content);
    }

    [Fact]
    public void ConvertToPdf_PathApi_AutoDetectsPptxExtension()
    {
        var pptxPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pptx");
        var pdfPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");

        try
        {
            using (var file = File.Create(pptxPath))
            using (var pptxStream = CreatePptx(
                new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("Path Dispatch PPTX", 914400, 914400, 3000000, 700000) })))
            {
                pptxStream.CopyTo(file);
            }

            MiniPdf.ConvertToPdf(pptxPath, pdfPath);

            var content = Encoding.ASCII.GetString(File.ReadAllBytes(pdfPath));
            Assert.StartsWith("%PDF-1.4", content);
            Assert.Contains("Path Dispatch PPTX", content);
        }
        finally
        {
            if (File.Exists(pptxPath)) File.Delete(pptxPath);
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public void ConvertToPdf_PptxWithSheetSelection_ThrowsHelpfulError()
    {
        using var pptxStream = CreatePptx(
            new PptxSlideSpec(new[] { PptxShapeSpec.TextBox("No Sheets", 914400, 914400, 3000000, 700000) }));

        var ex = Assert.Throws<NotSupportedException>(() => MiniPdf.ConvertToPdf(pptxStream, new[] { "Slide1" }));

        Assert.Contains("Sheet selection is only supported for .xlsx files.", ex.Message);
    }

    [Fact]
    public void ConvertToPdf_UnknownPathExtension_ThrowsHelpfulError()
    {
        var unknownPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.ppt");

        try
        {
            File.WriteAllText(unknownPath, "not a pptx");

            var ex = Assert.Throws<NotSupportedException>(() => MiniPdf.ConvertToPdf(unknownPath));

            Assert.Contains("Supported formats: .xlsx, .docx, .pptx", ex.Message);
        }
        finally
        {
            if (File.Exists(unknownPath)) File.Delete(unknownPath);
        }
    }

    [Fact]
    public void ConvertToPdf_UnknownStream_ThrowsHelpfulError()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not an office document"));

        var ex = Assert.Throws<NotSupportedException>(() => MiniPdf.ConvertToPdf(stream));

        Assert.Contains("Supported formats: .xlsx, .docx, .pptx", ex.Message);
    }

    private static MemoryStream CreatePptx(params PptxSlideSpec[] slides)
    {
        return CreatePptx(9144000, 6858000, slides);
    }

    private static MemoryStream CreatePptx(long slideWidthEmu, long slideHeightEmu, params PptxSlideSpec[] slides)
    {
        var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", CreateContentTypes(slides));
            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "ppt/presentation.xml", CreatePresentationXml(slides.Length, slideWidthEmu, slideHeightEmu));
            AddEntry(archive, "ppt/_rels/presentation.xml.rels", CreatePresentationRelationships(slides.Length));
            AddEntry(archive, "ppt/theme/theme1.xml", ThemeXml);

            for (var i = 0; i < slides.Length; i++)
            {
                AddEntry(archive, $"ppt/slides/slide{i + 1}.xml", CreateSlideXml(slides[i]));
                AddEntry(archive, $"ppt/slides/_rels/slide{i + 1}.xml.rels", CreateSlideRelationships(slides[i]));

                foreach (var image in slides[i].Images)
                {
                    var targetPath = ResolveSlideImageTarget(i + 1, image.Target);
                    AddEntry(archive, targetPath, image.Data);
                }
            }
        }

        ms.Position = 0;
        return ms;
    }

        private static MemoryStream CreatePptxWithOfficeSvgBlip(string sourceRectAttributes = "")
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
                                    <Default Extension="svg" ContentType="image/svg+xml"/>
                                    <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
                                    <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
                                    <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/presentation.xml", CreatePresentationXml(1, 9144000, 6858000));
                        AddEntry(archive, "ppt/_rels/presentation.xml.rels", CreatePresentationRelationships(1));
                        AddEntry(archive, "ppt/theme/theme1.xml", ThemeXml);
                        AddEntry(archive, "ppt/slides/slide1.xml",
                            $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                                             xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                                             xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                                             xmlns:asvg="http://schemas.microsoft.com/office/drawing/2016/SVG/main">
                                    <p:cSld>
                                        <p:bg><p:bgPr><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></p:bgPr></p:bg>
                                        <p:spTree>
                                            <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                                            <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                                            <p:pic>
                                                <p:nvPicPr><p:cNvPr id="2" name="Graphic 1"/><p:cNvPicPr/><p:nvPr/></p:nvPicPr>
                                                <p:blipFill><a:blip><a:extLst><a:ext uri="{96DAC541-7B7A-43D3-8B79-37D633B846F1}"><asvg:svgBlip r:embed="rIdImage1"/></a:ext></a:extLst></a:blip><a:srcRect{{sourceRectAttributes}}/><a:stretch><a:fillRect/></a:stretch></p:blipFill>
                                                <p:spPr><a:xfrm><a:off x="914400" y="914400"/><a:ext cx="1828800" cy="1828800"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></p:spPr>
                                            </p:pic>
                                        </p:spTree>
                                    </p:cSld>
                                </p:sld>
                                """);
                        AddEntry(archive, "ppt/slides/_rels/slide1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rIdImage1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="../media/image1.svg"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/media/image1.svg", Encoding.UTF8.GetBytes("""
                                <svg width="100" height="100" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg"><path d="M0 0 L100 0 L100 100 Z" fill="#E56925"/></svg>
                                """));
                }

                ms.Position = 0;
                return ms;
        }

        private static MemoryStream CreatePptxWithMappedLayoutBackground()
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
                                    <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
                                    <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
                                    <Override PartName="/ppt/slideLayouts/slideLayout1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml"/>
                                    <Override PartName="/ppt/slideMasters/slideMaster1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml"/>
                                    <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/presentation.xml", CreatePresentationXml(1, 9144000, 6858000));
                        AddEntry(archive, "ppt/_rels/presentation.xml.rels", CreatePresentationRelationships(1));
                        AddEntry(archive, "ppt/theme/theme1.xml", ThemeXml);
                        AddEntry(archive, "ppt/slides/slide1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <p:cSld><p:spTree><p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr></p:spTree></p:cSld>
                                    <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
                                </p:sld>
                                """);
                        AddEntry(archive, "ppt/slides/_rels/slide1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/slideLayouts/slideLayout1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sldLayout xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <p:cSld><p:bg><p:bgPr><a:solidFill><a:schemeClr val="tx1"/></a:solidFill></p:bgPr></p:bg><p:spTree><p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr></p:spTree></p:cSld>
                                    <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
                                </p:sldLayout>
                                """);
                        AddEntry(archive, "ppt/slideLayouts/_rels/slideLayout1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster" Target="../slideMasters/slideMaster1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/slideMasters/slideMaster1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sldMaster xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                    <p:cSld><p:spTree><p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr></p:spTree></p:cSld>
                                    <p:clrMap bg1="dk1" tx1="lt1" bg2="dk2" tx2="lt2" accent1="accent1" accent2="accent2" accent3="accent3" accent4="accent4" accent5="accent5" accent6="accent6" hlink="hlink" folHlink="folHlink"/>
                                </p:sldMaster>
                                """);
                }

                ms.Position = 0;
                return ms;
        }

        private static MemoryStream CreatePptxWithInheritedPlaceholderTextLayout()
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
                                    <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
                                    <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
                                    <Override PartName="/ppt/slideLayouts/slideLayout1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml"/>
                                    <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/presentation.xml", CreatePresentationXml(1, 9144000, 6858000));
                        AddEntry(archive, "ppt/_rels/presentation.xml.rels", CreatePresentationRelationships(1));
                        AddEntry(archive, "ppt/theme/theme1.xml", ThemeXml);
                        AddEntry(archive, "ppt/slides/slide1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                    <p:cSld>
                                        <p:bg><p:bgPr><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></p:bgPr></p:bg>
                                        <p:spTree>
                                            <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                                            <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                                            <p:sp>
                                                <p:nvSpPr><p:cNvPr id="2" name="Title"/><p:cNvSpPr/><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr>
                                                <p:spPr/>
                                                <p:txBody><a:bodyPr anchor="ctr"/><a:lstStyle/><a:p><a:r><a:t>Centered Title</a:t></a:r></a:p></p:txBody>
                                            </p:sp>
                                            <p:sp>
                                                <p:nvSpPr><p:cNvPr id="3" name="Body"/><p:cNvSpPr/><p:nvPr><p:ph type="body" idx="1"/></p:nvPr></p:nvSpPr>
                                                <p:spPr/>
                                                <p:txBody>
                                                    <a:bodyPr/>
                                                    <a:lstStyle/>
                                                    <a:p><a:r><a:t>First heading</a:t></a:r></a:p>
                                                    <a:p><a:pPr lvl="1"/><a:r><a:t>Indented bullet</a:t></a:r></a:p>
                                                    <a:p><a:pPr lvl="1"/></a:p>
                                                    <a:p><a:r><a:t>Second heading</a:t></a:r></a:p>
                                                </p:txBody>
                                            </p:sp>
                                        </p:spTree>
                                    </p:cSld>
                                </p:sld>
                                """);
                        AddEntry(archive, "ppt/slides/_rels/slide1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/slideLayouts/slideLayout1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sldLayout xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                    <p:cSld>
                                        <p:spTree>
                                            <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                                            <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                                            <p:sp>
                                                <p:nvSpPr><p:cNvPr id="2" name="Title"/><p:cNvSpPr/><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr>
                                                <p:spPr><a:xfrm><a:off x="914400" y="914400"/><a:ext cx="3657600" cy="914400"/></a:xfrm></p:spPr>
                                                <p:txBody><a:bodyPr anchor="ctr"/><a:lstStyle><a:lvl1pPr algn="ctr"><a:defRPr sz="2400"/></a:lvl1pPr></a:lstStyle></p:txBody>
                                            </p:sp>
                                            <p:sp>
                                                <p:nvSpPr><p:cNvPr id="3" name="Body"/><p:cNvSpPr/><p:nvPr><p:ph type="body" idx="1"/></p:nvPr></p:nvSpPr>
                                                <p:spPr><a:xfrm><a:off x="914400" y="2286000"/><a:ext cx="3657600" cy="2743200"/></a:xfrm></p:spPr>
                                                <p:txBody><a:bodyPr/><a:lstStyle><a:lvl1pPr marL="0" indent="0"><a:buNone/><a:defRPr sz="1800"/></a:lvl1pPr><a:lvl2pPr marL="283464" indent="-283464"><a:defRPr sz="1800"/></a:lvl2pPr></a:lstStyle></p:txBody>
                                            </p:sp>
                                        </p:spTree>
                                    </p:cSld>
                                </p:sldLayout>
                                """);
                }

                ms.Position = 0;
                return ms;
        }

        private static MemoryStream CreatePptxWithDiagramDrawing()
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
                                    <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
                                    <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
                                    <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/presentation.xml", CreatePresentationXml(1, 9144000, 6858000));
                        AddEntry(archive, "ppt/_rels/presentation.xml.rels", CreatePresentationRelationships(1));
                        AddEntry(archive, "ppt/theme/theme1.xml", ThemeXml);
                        AddEntry(archive, "ppt/slides/slide1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:dgm="http://schemas.openxmlformats.org/drawingml/2006/diagram">
                                    <p:cSld>
                                        <p:bg><p:bgPr><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></p:bgPr></p:bg>
                                        <p:spTree>
                                            <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                                            <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
                                            <p:graphicFrame>
                                                <p:nvGraphicFramePr><p:cNvPr id="2" name="SmartArt"/><p:cNvGraphicFramePr/><p:nvPr/></p:nvGraphicFramePr>
                                                <p:xfrm><a:off x="914400" y="914400"/><a:ext cx="3657600" cy="1828800"/></p:xfrm>
                                                <a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/diagram"><dgm:relIds r:dm="rIdData" r:lo="rIdLayout" r:qs="rIdStyle" r:cs="rIdColors"/></a:graphicData></a:graphic>
                                            </p:graphicFrame>
                                        </p:spTree>
                                    </p:cSld>
                                </p:sld>
                                """);
                        AddEntry(archive, "ppt/slides/_rels/slide1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rIdDiagram" Type="http://schemas.microsoft.com/office/2007/relationships/diagramDrawing" Target="../diagrams/drawing1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "ppt/diagrams/drawing1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <dsp:drawing xmlns:dsp="http://schemas.microsoft.com/office/drawing/2008/diagram" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                    <dsp:spTree>
                                        <dsp:nvGrpSpPr><dsp:cNvPr id="0" name=""/><dsp:cNvGrpSpPr/></dsp:nvGrpSpPr><dsp:grpSpPr/>
                                        <dsp:sp><dsp:nvSpPr><dsp:cNvPr id="1" name=""/><dsp:cNvSpPr/></dsp:nvSpPr><dsp:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="3657600" cy="1828800"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom><a:solidFill><a:srgbClr val="2F5597"/></a:solidFill></dsp:spPr><dsp:txBody><a:bodyPr/><a:lstStyle/><a:p><a:r><a:rPr sz="1800"><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></a:rPr><a:t>SmartArt Text</a:t></a:r></a:p></dsp:txBody></dsp:sp>
                                    </dsp:spTree>
                                </dsp:drawing>
                                """);
                }

                ms.Position = 0;
                return ms;
        }

    private static string CreateContentTypes(PptxSlideSpec[] slides)
    {
        var slideOverrides = new StringBuilder();
        for (var i = 0; i < slides.Length; i++)
        {
            slideOverrides.AppendLine($"  <Override PartName=\"/ppt/slides/slide{i + 1}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.slide+xml\"/>");
        }

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="xml" ContentType="application/xml"/>
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="png" ContentType="image/png"/>
              <Default Extension="jpg" ContentType="image/jpeg"/>
              <Default Extension="jpeg" ContentType="image/jpeg"/>
              <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
              <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
            {{slideOverrides}}}</Types>
            """;
    }

    private static string CreatePresentationXml(int slideCount, long slideWidthEmu, long slideHeightEmu)
    {
        var slideIds = new StringBuilder();
        for (var i = 0; i < slideCount; i++)
            slideIds.AppendLine($"    <p:sldId id=\"{256 + i}\" r:id=\"rId{i + 1}\"/>");

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                            xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <p:sldSz cx="{{slideWidthEmu}}" cy="{{slideHeightEmu}}"/>
              <p:sldIdLst>
            {{slideIds}}}  </p:sldIdLst>
            </p:presentation>
            """;
    }

    private static string CreatePresentationRelationships(int slideCount)
    {
        var relationships = new StringBuilder();
        for (var i = 0; i < slideCount; i++)
        {
            relationships.AppendLine($"  <Relationship Id=\"rId{i + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide\" Target=\"slides/slide{i + 1}.xml\"/>");
        }
        relationships.AppendLine($"  <Relationship Id=\"rId{slideCount + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme\" Target=\"theme/theme1.xml\"/>");

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
            {{relationships}}</Relationships>
            """;
    }

    private static string CreateSlideXml(PptxSlideSpec slide)
    {
        var shapeXml = new StringBuilder();
        for (var i = 0; i < slide.Shapes.Length; i++)
            shapeXml.AppendLine(CreateShapeXml(slide.Shapes[i], i + 2));

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                   xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                   xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <p:cSld>
                <p:bg><p:bgPr><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill></p:bgPr></p:bg>
                <p:spTree>
                  <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
                  <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
            {{shapeXml}}}    </p:spTree>
              </p:cSld>
            </p:sld>
            """;
    }

    private static string CreateShapeXml(PptxShapeSpec shape, int id)
    {
        if (shape.PictureRelationshipId != null)
        {
            return $$"""
                    <p:pic>
                      <p:nvPicPr><p:cNvPr id="{{id}}" name="Picture {{id}}"/><p:cNvPicPr/><p:nvPr/></p:nvPicPr>
                      <p:blipFill><a:blip r:embed="{{shape.PictureRelationshipId}}"/><a:stretch><a:fillRect/></a:stretch></p:blipFill>
                      <p:spPr><a:xfrm><a:off x="{{shape.X}}" y="{{shape.Y}}"/><a:ext cx="{{shape.Width}}" cy="{{shape.Height}}"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></p:spPr>
                    </p:pic>
                """;
        }

        var fill = shape.FillXml ?? (shape.FillColor == null
            ? "<a:noFill/>"
            : $"<a:solidFill><a:srgbClr val=\"{shape.FillColor}\"/></a:solidFill>");
        var outline = shape.OutlineColor == null
            ? "<a:ln><a:noFill/></a:ln>"
            : $"<a:ln w=\"12700\"><a:solidFill><a:srgbClr val=\"{shape.OutlineColor}\"/></a:solidFill></a:ln>";
        var textBody = shape.Text == null
            ? string.Empty
            : $$"""
                      <p:txBody>
                        <a:bodyPr/>
                        <a:lstStyle/>
                        <a:p><a:r><a:rPr sz="{{shape.FontSize}}"><a:solidFill><a:srgbClr val="{{shape.Color}}"/></a:solidFill></a:rPr><a:t>{{EscapeXml(shape.Text)}}</a:t></a:r></a:p>
                      </p:txBody>
                """;

        return $$"""
                <p:sp>
                  <p:nvSpPr><p:cNvPr id="{{id}}" name="Shape {{id}}"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr>
                  <p:spPr>
                    <a:xfrm><a:off x="{{shape.X}}" y="{{shape.Y}}"/><a:ext cx="{{shape.Width}}" cy="{{shape.Height}}"/></a:xfrm>
                    <a:prstGeom prst="{{shape.ShapeType}}"><a:avLst/></a:prstGeom>
                    {{fill}}
                    {{outline}}
                  </p:spPr>
            {{textBody}}}  </p:sp>
            """;
    }

    private static string CreateSlideRelationships(PptxSlideSpec slide)
    {
        var relationships = new StringBuilder();
        foreach (var image in slide.Images)
        {
            relationships.AppendLine($"  <Relationship Id=\"{image.RelationshipId}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"{image.Target}\"/>");
        }

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
            {{relationships}}</Relationships>
            """;
    }

    private static string ResolveSlideImageTarget(int slideIndex, string target)
    {
        var basePath = $"ppt/slides/slide{slideIndex}.xml";
        var slashIndex = basePath.LastIndexOf('/');
        var baseDirectory = slashIndex >= 0 ? basePath.Substring(0, slashIndex + 1) : string.Empty;
        var combined = target.StartsWith("/", StringComparison.Ordinal) ? target.TrimStart('/') : baseDirectory + target;
        var normalized = new List<string>();
        foreach (var segment in combined.Split('/'))
        {
            if (segment.Length == 0 || segment == ".")
                continue;
            if (segment == "..")
            {
                if (normalized.Count > 0)
                    normalized.RemoveAt(normalized.Count - 1);
                continue;
            }
            normalized.Add(segment);
        }
        return string.Join("/", normalized);
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static void AddEntry(ZipArchive archive, string path, byte[] content)
    {
        var entry = archive.CreateEntry(path);
        using var stream = entry.Open();
        stream.Write(content, 0, content.Length);
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

    private sealed record PptxSlideSpec(PptxShapeSpec[] Shapes, PptxImageSpec[]? Images = null)
    {
        public PptxImageSpec[] Images { get; } = Images ?? Array.Empty<PptxImageSpec>();
    }

    private sealed record PptxShapeSpec(
        string ShapeType,
        long X,
        long Y,
        long Width,
        long Height,
        string? Text = null,
        int FontSize = 1800,
        string Color = "000000",
        string? FillColor = null,
        string? OutlineColor = null,
        string? PictureRelationshipId = null,
        string? FillXml = null)
    {
        public static PptxShapeSpec TextBox(string text, long x, long y, long width, long height, int fontSize = 1800, string color = "000000")
            => new("rect", x, y, width, height, text, fontSize, color);

        public static PptxShapeSpec Shape(string shapeType, long x, long y, long width, long height, string? fillColor = null, string? outlineColor = null)
            => new(shapeType, x, y, width, height, FillColor: fillColor, OutlineColor: outlineColor);

        public static PptxShapeSpec Picture(string relationshipId, long x, long y, long width, long height)
            => new("rect", x, y, width, height, PictureRelationshipId: relationshipId);
    }

    private sealed record PptxImageSpec(string RelationshipId, string Target, byte[] Data);

    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream _inner;
        public NonSeekableStream(Stream inner) { _inner = inner; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private static readonly byte[] SmallPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/lLQ2VwAAAABJRU5ErkJggg==");

    private const string ThemeXml =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" name="Office Theme">
          <a:themeElements>
            <a:clrScheme name="Office">
              <a:dk1><a:srgbClr val="000000"/></a:dk1>
              <a:lt1><a:srgbClr val="FFFFFF"/></a:lt1>
              <a:dk2><a:srgbClr val="1F497D"/></a:dk2>
              <a:lt2><a:srgbClr val="EEECE1"/></a:lt2>
              <a:accent1><a:srgbClr val="4F81BD"/></a:accent1>
              <a:accent2><a:srgbClr val="C0504D"/></a:accent2>
              <a:accent3><a:srgbClr val="9BBB59"/></a:accent3>
              <a:accent4><a:srgbClr val="8064A2"/></a:accent4>
              <a:accent5><a:srgbClr val="4BACC6"/></a:accent5>
              <a:accent6><a:srgbClr val="F79646"/></a:accent6>
              <a:hlink><a:srgbClr val="0000FF"/></a:hlink>
              <a:folHlink><a:srgbClr val="800080"/></a:folHlink>
            </a:clrScheme>
          </a:themeElements>
        </a:theme>
        """;
}