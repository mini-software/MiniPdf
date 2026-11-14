using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace MiniSoftware.Tests;

public class ExcelToPdfConverterTests
{
    [Theory]
    [InlineData("Calibri", false)]
    [InlineData("Arial", false)]
    [InlineData("Calibri", true)]
    public void Convert_FontWithoutFamily_UsesWorkbookNormalFont(string normalFont, bool includeTheme)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Company", "Explicit", "Normal" } },
            columnWidths: new[] { 20f, 20f, 20f });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", $$"""
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="3">
                    <font><name val="{{normalFont}}"/><sz val="11"/></font>
                    <font><b/><sz val="10"/></font>
                    <font><name val="Verdana"/><sz val="12"/></font>
                  </fonts>
                  <cellXfs count="3">
                    <xf fontId="0"/><xf fontId="1"><alignment horizontal="right"/></xf><xf fontId="2"/>
                  </cellXfs>
                </styleSheet>
                """);
            if (includeTheme)
                AddEntry(archive, "xl/theme/theme1.xml", $$"""
                    <a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                      <a:themeElements><a:fontScheme name="Test">
                        <a:minorFont><a:latin typeface="{{normalFont}}"/></a:minorFont>
                      </a:fontScheme></a:themeElements>
                    </a:theme>
                    """);
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", """
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <cols><col min="1" max="3" width="20" customWidth="1"/></cols>
                  <sheetData><row r="1">
                    <c r="A1" s="1" t="inlineStr"><is><t>Company</t></is></c>
                    <c r="B1" s="2" t="inlineStr"><is><t>Explicit</t></is></c>
                    <c r="C1" t="inlineStr"><is><t>Normal</t></is></c>
                  </row></sheetData>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var blocks = Assert.Single(doc.Pages).TextBlocks;
        var company = Assert.Single(blocks, block => block.Text == "Company");
        Assert.Equal(normalFont, company.PreferredFontName);
        Assert.Equal(10f, company.FontSize);
        Assert.True(company.Bold);
        var explicitFont = Assert.Single(blocks, block => block.Text == "Explicit");
        Assert.Equal("Verdana", explicitFont.PreferredFontName);
        var normal = Assert.Single(blocks, block => block.Text == "Normal");
        Assert.Equal(normalFont, normal.PreferredFontName);
        Assert.False(normal.Bold);
        Assert.Equal(11f, normal.FontSize);
        if (normalFont == "Calibri" &&
            PdfWriter.TryMeasurePreferredFontWidth(normalFont, company.Text, company.FontSize, true, false, 0, out var width))
            Assert.Equal(explicitFont.X - 3.96f, company.X + width, 2);
    }

    [Theory]
    [InlineData("thin", 1f)]
    [InlineData("medium", 2f)]
    [InlineData("thick", 3f)]
    public void Convert_SolidBorder_PreservesLineWeight(string style, float width)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Border" } });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", $$"""
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="1"><border><bottom style="{{style}}"/></border></borders>
                  <cellXfs count="1"><xf borderId="0"/></cellXfs>
                </styleSheet>
                """);
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", """
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData><row r="1">
                    <c r="A1" s="0" t="inlineStr"><is><t>Border</t></is></c>
                  </row></sheetData>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        Assert.Equal(width, Assert.Single(Assert.Single(doc.Pages).LineBlocks).LineWidth);
    }

    [Fact]
    public void Convert_MergedHeading_DoesNotInflateIndividualColumnWidths()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "A long heading spanning both columns", "" },
            new[] { "Left", "Right" },
        });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml")!;
            XDocument worksheet;
            using (var input = entry.Open())
                worksheet = XDocument.Load(input);
            var ns = worksheet.Root!.Name.Namespace;
            worksheet.Root.Add(new XElement(ns + "mergeCells",
                new XElement(ns + "mergeCell", new XAttribute("ref", "A1:B1"))));
            entry.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", worksheet.ToString());
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var right = Assert.Single(Assert.Single(doc.Pages).TextBlocks, block => block.Text == "Right");
        Assert.InRange(right.X, 100f, 115f);
    }

    [Theory]
    [InlineData("top", 100)]
    [InlineData("bottom", 100)]
    [InlineData("left", 100)]
    [InlineData("right", 100)]
    [InlineData("top", 50)]
    [InlineData("left", 50)]
    public void Convert_DoubleBorder_RendersTwoScaledSolidStrokes(string side, int printScale)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Total" } });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", $$"""
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="1"><border><{{side}} style="double"><color rgb="FFFF0000"/></{{side}}></border></borders>
                  <cellXfs count="1"><xf borderId="0"/></cellXfs>
                </styleSheet>
                """);
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", $$"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData><row r="1"><c r="A1" s="0" t="inlineStr"><is><t>Total</t></is></c></row></sheetData>
                  <pageSetup scale="{{printScale}}" paperSize="9"/>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var lines = Assert.Single(doc.Pages).LineBlocks;
        Assert.Equal(2, lines.Count);
        var scale = printScale / 100f;
        Assert.All(lines, line =>
        {
            Assert.Equal(scale, line.LineWidth, 3);
            Assert.Equal(new PdfColor(1f, 0f, 0f), line.Color);
            Assert.Null(line.DashPattern);
        });
        if (side is "top" or "bottom")
        {
            Assert.All(lines, line => Assert.Equal(line.Y1, line.Y2));
            Assert.Equal(lines[0].X1, lines[1].X1);
            Assert.Equal(lines[0].X2, lines[1].X2);
            Assert.Equal(2f * scale, Math.Abs(lines[0].Y1 - lines[1].Y1), 3);
        }
        else
        {
            Assert.All(lines, line => Assert.Equal(line.X1, line.X2));
            Assert.Equal(lines[0].Y1, lines[1].Y1);
            Assert.Equal(lines[0].Y2, lines[1].Y2);
            Assert.Equal(2f * scale, Math.Abs(lines[0].X1 - lines[1].X1), 3);
        }
    }

    [Theory]
    [InlineData("top", "bottom")]
    [InlineData("bottom", "top")]
    [InlineData("left", "right")]
    [InlineData("right", "left")]
    public void Convert_DoubleBorder_TakesPrecedenceOverAdjacentThinBorder(string side, string opposite)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Total", "Other" } }, columnWidths: new[] { 20f, 20f });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", $$"""
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="2">
                    <border><{{opposite}} style="thin"/></border>
                    <border><{{side}} style="double"/></border>
                  </borders>
                  <cellXfs count="2"><xf borderId="0"/><xf borderId="1"/></cellXfs>
                </styleSheet>
                """);
            var firstStyle = side is "bottom" or "right" ? 1 : 0;
            var secondStyle = 1 - firstStyle;
            var cells = side is "top" or "bottom"
                ? $"""<row r="1"><c r="A1" s="{firstStyle}" t="inlineStr"><is><t>First</t></is></c></row><row r="2"><c r="A2" s="{secondStyle}" t="inlineStr"><is><t>Second</t></is></c></row>"""
                : $"""<row r="1"><c r="A1" s="{firstStyle}" t="inlineStr"><is><t>First</t></is></c><c r="B1" s="{secondStyle}" t="inlineStr"><is><t>Second</t></is></c></row>""";
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", $$"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <cols><col min="1" max="2" width="20" customWidth="1"/></cols>
                  <sheetData>{{cells}}</sheetData>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var lines = Assert.Single(doc.Pages).LineBlocks;
        Assert.Equal(2, lines.Count);
        Assert.All(lines, line => Assert.Equal(1f, line.LineWidth));
        var separation = side is "top" or "bottom"
            ? Math.Abs(lines[0].Y1 - lines[1].Y1)
            : Math.Abs(lines[0].X1 - lines[1].X1);
        Assert.Equal(2f, separation, 3);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Convert_DoubleBordersAcrossPages_PreserveSharedEdges(bool lowMemory, bool manualBreaks)
    {
        using var excelStream = CreateSimpleExcel(
            Enumerable.Range(1, 120).Select(row => new[] { $"Detail {row}" }).ToArray(),
            columnWidths: new[] { 20f });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="1"><border><top style="double"/><bottom style="thin"/></border></borders>
                  <cellXfs count="1"><xf borderId="0"/></cellXfs>
                </styleSheet>
                """);
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml")!;
            XDocument worksheet;
            using (var input = entry.Open())
                worksheet = XDocument.Load(input);
            var ns = worksheet.Root!.Name.Namespace;
            foreach (var cell in worksheet.Descendants(ns + "c"))
                cell.SetAttributeValue("s", 0);
            if (manualBreaks)
                worksheet.Root.Add(new XElement(ns + "rowBreaks",
                    new XAttribute("count", 2), new XAttribute("manualBreakCount", 2),
                    new XElement(ns + "brk", new XAttribute("id", 25), new XAttribute("man", 1)),
                    new XElement(ns + "brk", new XAttribute("id", 75), new XAttribute("man", 1))));
            entry.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", worksheet.ToString());
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream,
            new ExcelToPdfConverter.ConversionOptions { LowMemory = lowMemory });
        Assert.True(doc.Pages.Count > 1);
        Assert.Equal(120, doc.Pages.Sum(page => page.TextBlocks.Count));
        Assert.All(doc.Pages, page =>
        {
            Assert.NotEmpty(page.TextBlocks);
            Assert.Equal(2 * page.TextBlocks.Count + 1, page.LineBlocks.Count);
            Assert.All(page.LineBlocks, line =>
            {
                Assert.Equal(line.Y1, line.Y2);
                Assert.Equal(1f, line.LineWidth);
                Assert.Null(line.DashPattern);
            });
        });
        using var actual = new MemoryStream();
        doc.Save(actual);
        excelStream.Position = 0;
        using var expected = ExcelToPdfConverter.Convert(excelStream);
        using var expectedPdf = new MemoryStream();
        expected.Save(expectedPdf);
        Assert.Equal(expectedPdf.ToArray(), actual.ToArray());
    }

    [Theory]
    [InlineData("Calibri", false, 15f, 14.52f)]
    [InlineData("Calibri", true, 15f, 15f)]
    [InlineData("Calibri", false, 20f, 20f)]
    [InlineData("Arial", false, 15f, 15f)]
    public void Convert_DefaultRowHeight_PreservesCustomDefaults(
        string fontName, bool customHeight, float defaultHeight, float expectedHeight)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "First" }, new[] { "Second" } });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", $$"""
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="1"><font><name val="{{fontName}}"/><sz val="11"/></font></fonts>
                  <cellXfs count="1"><xf fontId="0"/></cellXfs>
                </styleSheet>
                """);
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            var height = defaultHeight.ToString(System.Globalization.CultureInfo.InvariantCulture);
            AddEntry(archive, "xl/worksheets/sheet1.xml", $$"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetFormatPr defaultRowHeight="{{height}}" customHeight="{{(customHeight ? 1 : 0)}}"/>
                  <sheetData>
                    <row r="1"><c r="A1" t="inlineStr"><is><t>First</t></is></c></row>
                    <row r="2"><c r="A2" t="inlineStr"><is><t>Second</t></is></c></row>
                  </sheetData>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var blocks = Assert.Single(doc.Pages).TextBlocks;
        var first = Assert.Single(blocks, block => block.Text == "First");
        var second = Assert.Single(blocks, block => block.Text == "Second");
        Assert.Equal(expectedHeight, first.Y - second.Y, 3);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    public void Convert_DoubleBorder_ResolvesOverlappingMergedEdges(bool mergeDouble, int expectedLines)
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "First", "Second" } });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="2">
                    <border><bottom style="thin"/></border>
                    <border><top style="double"/></border>
                  </borders>
                  <cellXfs count="2"><xf borderId="0"/><xf borderId="1"/></cellXfs>
                </styleSheet>
                """);
            var mergeRef = mergeDouble ? "A2:B2" : "A1:B1";
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", $$"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <cols><col min="1" max="2" width="20" customWidth="1"/></cols>
                  <sheetData>
                    <row r="1"><c r="A1" s="0" t="inlineStr"><is><t>First</t></is></c><c r="B1" s="0"/></row>
                    <row r="2"><c r="A2" s="1" t="inlineStr"><is><t>Total</t></is></c><c r="B2" t="inlineStr"/></row>
                  </sheetData>
                  <mergeCells count="1"><mergeCell ref="{{mergeRef}}"/></mergeCells>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var lines = Assert.Single(doc.Pages).LineBlocks;
        Assert.Equal(expectedLines, lines.Count);
        var doubleLines = lines.OrderBy(line => line.Y1).ToArray();
        Assert.Equal(2f, doubleLines[^1].Y1 - doubleLines[0].Y1, 3);
        if (!mergeDouble)
        {
            var middle = doubleLines[1];
            Assert.Equal(doubleLines[0].X2, middle.X1, 3);
            Assert.True(middle.X2 > middle.X1);
        }
    }

    [Fact]
    public void Convert_CalibriColumnWidths_MatchExcelPrintMetrics()
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "A", "B", "C" } },
            columnWidths: new[] { 25f, 15f, 15f });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="1"><font><name val="Calibri"/><sz val="11"/></font></fonts>
                  <cellXfs count="1"><xf fontId="0"/></cellXfs>
                </styleSheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var blocks = Assert.Single(doc.Pages).TextBlocks;
        var a = Assert.Single(blocks, block => block.Text == "A");
        var b = Assert.Single(blocks, block => block.Text == "B");
        var c = Assert.Single(blocks, block => block.Text == "C");
        Assert.InRange(b.X - a.X, 140.65f, 141.45f);
        Assert.InRange(c.X - b.X, 84.22f, 85.02f);
    }

    [Fact]
    public void Convert_DoubleBorder_VerticalMergeContinuesAcrossPages()
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Total", "Detail" } });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <borders count="2"><border/><border><left style="double"/><right style="double"/></border></borders>
                  <cellXfs count="2"><xf borderId="0"/><xf borderId="1"/></cellXfs>
                </styleSheet>
                """);
            var rows = new StringBuilder();
            for (var row = 1; row <= 80; row++)
            {
                rows.Append($"""<row r="{row}">""");
                if (row == 1)
                    rows.Append("""<c r="A1" s="1" t="inlineStr"><is><t>Total</t></is></c>""");
                rows.Append($"""<c r="B{row}" t="inlineStr"><is><t>Detail {row}</t></is></c></row>""");
            }
            archive.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", $$"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <cols><col min="1" max="2" width="20" customWidth="1"/></cols>
                  <sheetData>{{rows}}</sheetData>
                  <mergeCells count="1"><mergeCell ref="A1:A80"/></mergeCells>
                </worksheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        Assert.Equal(2, doc.Pages.Count);
        Assert.All(doc.Pages, page =>
        {
            var lines = page.LineBlocks.OrderBy(line => line.X1).ToArray();
            Assert.Equal(4, lines.Length);
            Assert.All(lines, line =>
            {
                Assert.Equal(line.X1, line.X2);
                Assert.Equal(1f, line.LineWidth);
                Assert.Null(line.DashPattern);
            });
            Assert.Equal(2f, lines[1].X1 - lines[0].X1, 3);
            Assert.Equal(2f, lines[3].X1 - lines[2].X1, 3);
        });
    }

    [Fact]
    public void Convert_DefaultCalibriFont_PreservesUnsupportedScriptFallback()
    {
        const string sample = "\u0E20\u0E32\u0E29\u0E32\u0E44\u0E17\u0E22";
        using var excelStream = CreateSimpleExcel(new[] { new[] { "Label", sample } },
            columnWidths: new[] { 20f, 30f });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="1"><font><name val="Calibri"/><sz val="11"/></font></fonts>
                  <cellXfs count="1"><xf fontId="0"/></cellXfs>
                </styleSheet>
                """);
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var page = Assert.Single(doc.Pages);
        var label = Assert.Single(page.TextBlocks, block => block.Text == "Label");
        var script = Assert.Single(page.TextBlocks, block => block.Text == sample);
        Assert.Equal("Calibri", label.PreferredFontName);
        var supported = PdfWriter.TryMeasurePreferredFontWidth("Calibri", sample, 11f, false, false, 0f, out _);
        Assert.Equal(supported ? "Calibri" : null, script.PreferredFontName);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(3, true)]
    public void Convert_ChartAnchorsOutsideData_UseDefaultWidths(int anchorCol, bool useDefaultWidths)
    {
        using var excelStream = CreateExcelWithBarChart(new[] { "Category" }, new[] { 140.0 });
        using (var archive = new ZipArchive(excelStream, ZipArchiveMode.Update, leaveOpen: true))
        {
            AddEntry(archive, "xl/styles.xml", """
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="2"><font><sz val="11"/></font><font><sz val="14"/></font></fonts>
                  <cellXfs count="2"><xf fontId="0"/><xf fontId="1"/></cellXfs>
                </styleSheet>
                """);
            var entry = archive.GetEntry("xl/worksheets/sheet1.xml")!;
            XDocument worksheet;
            using (var input = entry.Open())
                worksheet = XDocument.Load(input);
            var ns = worksheet.Root!.Name.Namespace;
            worksheet.Descendants(ns + "sheetData").Single().Add(
                new XElement(ns + "row", new XAttribute("r", 1),
                    new XElement(ns + "c", new XAttribute("r", "A1"), new XAttribute("s", 1), new XAttribute("t", "inlineStr"),
                        new XElement(ns + "is", new XElement(ns + "t", "A long overflowing worksheet heading")))),
                new XElement(ns + "row", new XAttribute("r", 2),
                    new XElement(ns + "c", new XAttribute("r", "B2"), new XAttribute("t", "inlineStr"),
                        new XElement(ns + "is", new XElement(ns + "t", "Cell B2")))));
            entry.Delete();
            AddEntry(archive, "xl/worksheets/sheet1.xml", worksheet.ToString());
            var drawingEntry = archive.GetEntry("xl/drawings/drawing1.xml")!;
            XDocument drawing;
            using (var input = drawingEntry.Open())
                drawing = XDocument.Load(input);
            var drawingNs = drawing.Root!.Name.Namespace;
            drawing.Descendants(drawingNs + "from").Single().Element(drawingNs + "col")!.Value =
                anchorCol.ToString(System.Globalization.CultureInfo.InvariantCulture);
            drawingEntry.Delete();
            AddEntry(archive, "xl/drawings/drawing1.xml", drawing.ToString());
        }
        excelStream.Position = 0;
        using var doc = ExcelToPdfConverter.Convert(excelStream);
        var cell = Assert.Single(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Cell B2");
        if (useDefaultWidths)
            Assert.InRange(cell.X, 103f, 104f);
        else
            Assert.True(cell.X > 150f);
    }

    [Fact]
    public void Convert_SimpleExcel_ProducesValidPdf()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Name", "Age", "City" },
            new[] { "Alice", "30", "New York" },
            new[] { "Bob", "25", "London" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Name", content);
        Assert.Contains("Alice", content);
        Assert.Contains("Bob", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void Convert_WithoutPaperSize_UsesA4Page()
    {
        using var excelStream = CreateSimpleExcel(new[] { new[] { "A4" } });

        var page = Assert.Single(ExcelToPdfConverter.Convert(excelStream).Pages);

        Assert.Equal(595.2756f, page.Width, 3);
        Assert.Equal(841.8898f, page.Height, 3);
    }

    [Fact]
    public void Convert_WithExplicitLetterPaperSize_UsesLetterPage()
    {
        using var excelStream = CreateSimpleExcel(
            new[] { new[] { "Letter" } },
            includeUnflaggedFitAttributes: true);

        var page = Assert.Single(ExcelToPdfConverter.Convert(excelStream).Pages);

        Assert.Equal(612f, page.Width, 3);
        Assert.Equal(792f, page.Height, 3);
    }

    [Fact]
    public void Convert_WorksheetImage_RendersAboveCellText()
    {
        using var excelStream = CreateExcelWithImageOverCell();

        var doc = ExcelToPdfConverter.Convert(excelStream);

        var image = Assert.Single(doc.Pages.SelectMany(page => page.ImageBlocks));
        var overlappedText = Assert.Single(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Covered");
        Assert.True(image.RenderAboveText);
        Assert.True(overlappedText.X < image.X + image.RenderWidth,
            "Default column geometry should keep D1 beneath the A1 drawing edge.");
    }

    [Fact]
    public void Convert_BarChartWithClusteredNonZeroData_KeepsBarsAndAxisWithinPlot()
    {
        // Values clustered far above zero (like stock prices) should auto-scale
        // the axis close to the data instead of forcing a zero baseline; bars
        // must still be drawn as bounded rectangles within the chart area.
        using var excelStream = CreateExcelWithBarChart(
            categories: new[] { "Day 1", "Day 2", "Day 3", "Day 4", "Day 5" },
            values: new[] { 140.0, 145.0, 150.0, 155.0, 150.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        var page = Assert.Single(doc.Pages);
        var yAxis = Assert.Single(page.LineBlocks, line =>
            Math.Abs(line.X1 - line.X2) < 0.01f && line.Color == PdfColor.Black);
        var plotBottom = Math.Min(yAxis.Y1, yAxis.Y2);
        var plotTop = Math.Max(yAxis.Y1, yAxis.Y2);
        var bars = page.RectBlocks.Where(rectangle => rectangle.Height > 7f)
            .OrderBy(rectangle => rectangle.X)
            .ToArray();

        Assert.Equal(5, bars.Length);
        Assert.All(bars, bar =>
        {
            Assert.Equal(plotBottom, bar.Y, 3);
            Assert.InRange(bar.Y + bar.Height, plotBottom, plotTop + 0.01f);
        });
        var firstGap = bars[1].X - bars[0].X - bars[0].Width;
        Assert.True(firstGap > bars[0].Width, "A 150% gapWidth should leave more gap than bar width.");

        var axisLabels = page.TextBlocks.Select(block => block.Text);
        Assert.DoesNotContain("0", axisLabels);
        Assert.Contains("130", axisLabels);
        Assert.Contains("160", axisLabels);
        Assert.Contains("Price", axisLabels);
        Assert.Contains(page.TextBlocks, block => block.Text == "Clustered Non-Zero Prices" && block.Bold);
    }

    [Fact]
    public void Convert_WithOptions_UsesCustomSettings()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Header1", "Header2" },
            new[] { "Value1", "Value2" },
        });

        var options = new ExcelToPdfConverter.ConversionOptions
        {
            FontSize = 14,
            MarginLeft = 72,
            PageWidth = 595, // A4
            PageHeight = 842, // A4
            IncludeSheetName = false,
        };

        var doc = ExcelToPdfConverter.Convert(excelStream, options);
        Assert.True(doc.Pages.Count >= 1);
        var bytes = doc.ToArray();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Convert_ProgrammaticColumnWidthsWithoutCustomWidth_UsesWidths()
    {
        using var excelStream = CreateSimpleExcel(
            new[]
            {
                new[] { "A", "B", "C", "D" },
                new[] { "1", "2", "3", "4" },
            },
            columnWidths: new[] { 24f, 24f, 24f, 24f },
            customWidth: false);

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.True(doc.Pages.Count >= 2, $"Expected wide generated columns to split across pages, got {doc.Pages.Count}");
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == "D");
    }

    [Fact]
    public void Convert_GeneratedWideTextTableWithoutColumnWidths_InfersReadableColumnGroups()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var content = Encoding.ASCII.GetString(doc.ToArray());

        Assert.True(doc.Pages.Count >= 2, $"Expected inferred wide columns to create column groups, got {doc.Pages.Count}");
        Assert.Contains("1234 North Evergreen Avenue", content);
        Assert.Contains("QA Automation Specialist", content);
    }

    [Fact]
    public void Convert_LargeGeneratedWideTable_UsesReadableA4ColumnGroups()
    {
        var rows = new List<string[]>
        {
            Enumerable.Range(1, 10).Select(index => $"Header{index}").ToArray(),
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row =>
            Enumerable.Range(1, 10).Select(column => $"Long generated value {row}-{column}").ToArray()));
        using var excelStream = CreateSimpleExcel(rows.ToArray());

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var firstPageText = doc.Pages[0].TextBlocks.Select(block => block.Text).ToHashSet();
        var headerY = doc.Pages[0].TextBlocks.First(block => block.Text == "Header1").Y;
        var firstRowY = doc.Pages[0].TextBlocks.Where(block => block.Y < headerY).Max(block => block.Y);

        Assert.Equal(595.2756f, doc.Pages[0].Width, 3);
        Assert.Equal(841.8898f, doc.Pages[0].Height, 3);
        Assert.Equal(14.52f, headerY - firstRowY, 3);
        for (var column = 1; column <= 4; column++)
            Assert.Contains($"Header{column}", firstPageText);
        Assert.DoesNotContain("Header5", firstPageText);
        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Header10");
    }

    [Fact]
    public void Convert_FitAttributesWithoutFitFlag_DoesNotCompressAllRowsToOnePageHigh()
    {
        var rows = new List<string[]>
        {
            Enumerable.Range(1, 8).Select(index => $"Header{index}").ToArray(),
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row =>
            Enumerable.Range(1, 8).Select(column => $"Value {row}-{column}").ToArray()));
        using var excelStream = CreateSimpleExcel(rows.ToArray(), includeUnflaggedFitAttributes: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var minimumFontSize = doc.Pages.SelectMany(page => page.TextBlocks).Min(block => block.FontSize);
        var firstPageText = doc.Pages[0].TextBlocks.Select(block => block.Text).ToHashSet();

        Assert.True(doc.Pages.Count > 1, $"Expected rows to paginate naturally, got {doc.Pages.Count} page.");
        Assert.Equal(595.2756f, doc.Pages[0].Width, 3);
        Assert.Equal(841.8898f, doc.Pages[0].Height, 3);
        Assert.True(minimumFontSize >= 10f, $"Expected readable text, got {minimumFontSize}pt.");
        for (var column = 1; column <= 6; column++)
            Assert.Contains($"Header{column}", firstPageText);
        Assert.DoesNotContain("Header7", firstPageText);
        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Header8");
    }

    [Fact]
    public void Convert_LargeGeneratedTextTableWithUnflaggedFitAttributes_KeepsReadableColumnGroups()
    {
        var rows = new List<string[]>
        {
            new[] { "ID", "Name", "Description", "Phone", "Email", "Company", "Title", "Notes" },
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row => new[]
        {
            $"ID-{row:00000}",
            $"Customer {row:00000}",
            $"Detailed service description for customer account {row:00000}",
            "555-0123",
            $"customer{row:00000}@example.test",
            "Contoso Operations",
            "QA Automation Specialist",
            "Priority customer account",
        }));
        using var excelStream = CreateSimpleExcel(rows.ToArray(), includeUnflaggedFitAttributes: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var renderedBlocks = doc.Pages.SelectMany(page => page.TextBlocks).ToArray();
        var renderedText = renderedBlocks.Select(block => block.Text).ToArray();

        Assert.Contains("Description", renderedText);
        Assert.Contains(renderedText, text => text.StartsWith("QA Automation", StringComparison.Ordinal));
        Assert.DoesNotContain("Descriptio", renderedText);
        Assert.Contains(renderedBlocks, block => block.Text.StartsWith("Detailed service", StringComparison.Ordinal) && block.MaxWidth.HasValue);
    }

    [Fact]
    public void Convert_PrintTitleRowsStartingAtFirstRow_RepeatsHeaderAfterPageBreak()
    {
        var rows = new List<string[]> { new[] { "Record ID", "Name", "City" } };
        for (var i = 1; i <= 90; i++)
            rows.Add(new[] { $"ID-{i:00000}", $"Name {i}", $"City {i}" });

        using var excelStream = CreateSimpleExcel(rows.ToArray(), printTitleFirstRow: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.True(doc.Pages.Count >= 2, $"Expected multiple pages, got {doc.Pages.Count}");
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == "Record ID");
    }

    [Fact]
    public void Convert_AdjacentContentColumns_ConstrainFullTextToCellWidth()
    {
        using var excelStream = CreateSimpleExcel(
            new[]
            {
                new[] { "Street Address", "City" },
                new[] { "1035 North Evergreen Avenue Suite 235", "Berlin" },
            },
            columnWidths: new[] { 34f, 18f },
            customWidth: false);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var addressBlock = doc.Pages.SelectMany(page => page.TextBlocks)
            .FirstOrDefault(block => block.Text == "1035 North Evergreen Avenue Suite 235");

        Assert.NotNull(addressBlock);
        Assert.True(addressBlock.MaxWidth.HasValue, "Adjacent populated cells should constrain text to the cell width.");
    }

    [Fact]
    public void Convert_EmbeddedNewlineWithoutWrapText_RendersOnOneLine()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Name", "Address" },
            new[] { "Alice", "Line 1\nLine 2" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var renderedText = doc.Pages.SelectMany(page => page.TextBlocks).Select(block => block.Text).ToArray();

        Assert.Contains("Line 1 Line 2", renderedText);
        Assert.DoesNotContain("Line 1", renderedText);
        Assert.DoesNotContain("Line 2", renderedText);
    }

    [Fact]
    public void Convert_WithRenderLimits_RendersOnlyLeadingRowsAndColumns()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "H1", "H2", "H3" },
            new[] { "R1C1", "R1C2", "R1C3" },
            new[] { "R2C1", "R2C2", "R2C3" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            MaxRows = 2,
            MaxColumns = 2,
        });
        var renderedText = string.Join("\n", doc.Pages.SelectMany(page => page.TextBlocks).Select(block => block.Text));

        Assert.Contains("H1", renderedText);
        Assert.Contains("H2", renderedText);
        Assert.Contains("R1C1", renderedText);
        Assert.Contains("R1C2", renderedText);
        Assert.DoesNotContain("H3", renderedText);
        Assert.DoesNotContain("R1C3", renderedText);
        Assert.DoesNotContain("R2C1", renderedText);
    }

    [Fact]
    public void Convert_WithLandscapeOption_UsesLandscapePageDimensions()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "A", "B" },
            new[] { "1", "2" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Landscape = true,
        });

        Assert.True(doc.Pages[0].Width > doc.Pages[0].Height,
            $"Expected landscape dimensions, got {doc.Pages[0].Width}x{doc.Pages[0].Height}.");
    }

    [Fact]
    public void Convert_WithFitToPageOption_FitsWideColumnsOnSinglePage()
    {
        var rows = new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        };

        using var defaultStream = CreateSimpleExcel(rows);
        using var fitStream = CreateSimpleExcel(rows);

        var defaultDoc = ExcelToPdfConverter.Convert(defaultStream);
        var fitDoc = ExcelToPdfConverter.Convert(fitStream, new ExcelToPdfConverter.ConversionOptions
        {
            FitToPage = true,
        });

        Assert.True(defaultDoc.Pages.Count >= 2, $"Expected default wide sheet to split, got {defaultDoc.Pages.Count} pages.");
        Assert.Single(fitDoc.Pages);
    }

    [Fact]
    public void Convert_WithFitToWidthZero_DoesNotForceWideColumnsOntoOnePage()
    {
        var rows = new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        };

        using var excelStream = CreateSimpleExcel(rows);

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            FitToPage = true,
            FitToWidth = 0,
        });

        Assert.True(doc.Pages.Count >= 2, $"Expected unlimited fit-to-width to keep column groups, got {doc.Pages.Count} pages.");
    }

    [Fact]
    public void Convert_WithRowsPerPageOption_ReducesTallSheetPageCount()
    {
        var rows = Enumerable.Range(0, 80)
            .Select(i => new[] { $"Row{i}", $"Value{i}" })
            .ToArray();

        using var defaultStream = CreateSimpleExcel(rows);
        using var compactStream = CreateSimpleExcel(rows);

        var defaultDoc = ExcelToPdfConverter.Convert(defaultStream);
        var compactDoc = ExcelToPdfConverter.Convert(compactStream, new ExcelToPdfConverter.ConversionOptions
        {
            RowsPerPage = 80,
        });

        Assert.True(defaultDoc.Pages.Count >= 2, $"Expected default tall sheet to need multiple pages, got {defaultDoc.Pages.Count}.");
        Assert.True(compactDoc.Pages.Count < defaultDoc.Pages.Count,
            $"Expected rows-per-page layout to reduce pages from {defaultDoc.Pages.Count}, got {compactDoc.Pages.Count}.");
    }

    [Fact]
    public void Convert_EmptyExcel_CreatesAtLeastOnePage()
    {
        using var excelStream = CreateSimpleExcel(Array.Empty<string[]>());

        var doc = ExcelToPdfConverter.Convert(excelStream);
        Assert.True(doc.Pages.Count >= 1);
    }

    [Fact]
    public void Convert_WithSelectedSheets_RendersOnlyMatchingSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Sheets = new[] { "Details" },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("DetailsOnly", content);
        Assert.DoesNotContain("SummaryOnly", content);
        Assert.DoesNotContain("ArchiveOnly", content);
    }

    [Fact]
    public void Convert_WithSelectedSheetIndexes_RendersOnlyMatchingSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            SheetIndexes = new[] { 2 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("DetailsOnly", content);
        Assert.DoesNotContain("SummaryOnly", content);
        Assert.DoesNotContain("ArchiveOnly", content);
    }

    [Fact]
    public void Convert_WorkbookRelationshipTargetOutsideWorksheetsFolder_RendersSelectedSheetIndex()
    {
        using var excelStream = CreateExcelWithWorksheetTarget("sheets/report.xml", "RelationshipTargetOnly");

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            SheetIndexes = new[] { 1 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("RelationshipTargetOnly", content);
    }

    [Fact]
    public void Convert_WithSelectedSheetNamesAndIndexes_RendersUnion()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Sheets = new[] { "Archive" },
            SheetIndexes = new[] { 1 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("SummaryOnly", content);
        Assert.Contains("ArchiveOnly", content);
        Assert.DoesNotContain("DetailsOnly", content);
    }

    [Fact]
    public void Convert_WithNullSheets_RendersAllSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions { Sheets = null });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("SummaryOnly", content);
        Assert.Contains("DetailsOnly", content);
    }

    [Fact]
    public void Convert_WithUnknownSheet_ThrowsHelpfulError()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var ex = Assert.Throws<ArgumentException>(() => MiniPdf.ConvertToPdf(excelStream,
            new MiniPdfConversionOptions { Sheets = new[] { "Missing" } }));

        Assert.Contains("Sheet(s) not found: Missing", ex.Message);
        Assert.Contains("Available sheets: Summary, Details", ex.Message);
    }

    [Fact]
    public void Convert_WithUnknownSheetIndex_ThrowsHelpfulError()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var ex = Assert.Throws<ArgumentException>(() => MiniPdf.ConvertToPdf(excelStream,
            new MiniPdfConversionOptions { SheetIndexes = new[] { 3 } }));

        Assert.Contains("Sheet index(es) out of range: 3", ex.Message);
        Assert.Contains("Available index range: 1-2", ex.Message);
    }

    [Fact]
    public void ConvertToFile_CreatesOutputFile()
    {
        var excelPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.xlsx");
        var pdfPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");

        try
        {
            using (var fs = File.Create(excelPath))
            using (var excelStream = CreateSimpleExcel(new[]
            {
                new[] { "Test", "Data" },
                new[] { "1", "2" },
            }))
            {
                excelStream.CopyTo(fs);
            }

            ExcelToPdfConverter.ConvertToFile(excelPath, pdfPath);

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(excelPath)) File.Delete(excelPath);
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public void Convert_ManyRows_CreatesMultiplePages()
    {
        var rows = new List<string[]>();
        for (var i = 0; i < 100; i++)
        {
            rows.Add(new[] { $"Row{i}", $"Value{i}", $"Data{i}" });
        }

        using var excelStream = CreateSimpleExcel(rows.ToArray());
        var doc = ExcelToPdfConverter.Convert(excelStream);

        // 100 rows at ~14pt line height should require multiple pages
        Assert.True(doc.Pages.Count >= 2, $"Expected at least 2 pages, got {doc.Pages.Count}");
    }

    [Fact]
    public void Convert_WithTextColor_PreservesColorInPdf()
    {
        // Create an xlsx with red text in cell A1
        using var excelStream = CreateColoredExcel(
            new[] { ("Red Text", "FFFF0000"), ("Normal", "") },
            new[] { ("Blue Val", "FF0000FF"), ("Green", "FF00FF00") }
        );

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("Red Text", content);
        Assert.Contains("Blue Val", content);
        // Verify that non-black color operators appear (red = 1.000 0.000 0.000 rg)
        Assert.Contains("1.000 0.000 0.000 rg", content);
        // Blue = 0.000 0.000 1.000 rg
        Assert.Contains("0.000 0.000 1.000 rg", content);
    }

    [Fact]
    public void ConvertToPdf_StreamApi_AutoDetectsXlsx()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "AutoDetect", "Xlsx" },
            new[] { "Cell1", "Cell2" },
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("AutoDetect", content);
        Assert.Contains("Cell1", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void ConvertToPdf_StreamApi_WorksWithNonSeekableXlsxStream()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "NonSeekable", "Xlsx" },
            new[] { "Row2A", "Row2B" },
        });
        using var nonSeekable = new NonSeekableStream(excelStream);

        var bytes = MiniPdf.ConvertToPdf(nonSeekable);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("%%EOF", content);
        // Sanity check: the produced PDF should be larger than an empty one.
        Assert.True(bytes.Length > 500, $"Expected non-trivial PDF, got {bytes.Length} bytes");
    }

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

    [Fact]
    public void Convert_BuiltInNumFmtId4_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30.596,37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30,596.37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_LargeValueWithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 1234567.89 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1.234.567,89");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30,596.37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_MultipleValuesWithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(
            numFmtId: 4,
            values: new[] { 0, 1, 100, 1234.5, 30596.37, 1234567.89, -30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        var expected = new[]
        {
            "0,00",
            "1,00",
            "100,00",
            "1.234,50",
            "30.596,37",
            "1.234.567,89",
            "-30.596,37",
        };

        foreach (var text in expected)
        {
            Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == text);
        }
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596,37");
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596.37");
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596.37");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });
        var culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR");

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = culture,
        });

        var expected = new DateTime(2023, 1, 1).ToString("d-MMM-yy", culture);
        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == expected);
    }

    [Fact]
    public void Convert_BuiltInShortDateFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 14, values: new[] { 45092.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "15/6/2023");
    }

    [Fact]
    public void Convert_BuiltInShortDateTimeFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 22, values: new[] { 45092.5 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "15/6/2023 12:00");
    }

    [Fact]
    public void Convert_BuiltInShortDateFormat_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 14, values: new[] { 45092.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "6/15/2023");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1-Jan-23");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1-Jan-23");
    }

    [Fact]
    public void ConvertToPdf_PublicApi_BuiltInNumFmtId4_HonorsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("30.596,37", content);
    }

    /// <summary>
    /// Creates a minimal .xlsx with a built-in number format (numFmtId only, no custom
    /// &lt;numFmts&gt; entry) and a single column of numeric cells using that format. This
    /// reproduces the real-world XLSX where cells carry only a built-in numFmtId.
    /// </summary>
    private static MemoryStream CreateExcelWithBuiltInNumberFormat(int numFmtId, params double[] values)
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
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            // styles.xml with a built-in numFmtId and no custom <numFmts> section:
            // the cell style references the built-in format directly.
            AddEntry(archive, "xl/styles.xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="1">
                    <font><sz val="11"/><name val="Calibri"/></font>
                  </fonts>
                  <fills count="1">
                    <fill><patternFill patternType="none"/></fill>
                  </fills>
                  <borders count="1">
                    <border><left/><right/><top/><bottom/><diagonal/></border>
                  </borders>
                  <cellXfs count="1">
                    <xf numFmtId="{{numFmtId}}" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>
                  </cellXfs>
                </styleSheet>
                """);

            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            sheetSb.AppendLine("<sheetData>");
            for (var i = 0; i < values.Length; i++)
            {
                sheetSb.AppendLine($"  <row r=\"{i + 1}\">");
                sheetSb.AppendLine($"    <c r=\"A{i + 1}\" s=\"0\"><v>{values[i].ToString(System.Globalization.CultureInfo.InvariantCulture)}</v></c>");
                sheetSb.AppendLine("  </row>");
            }
            sheetSb.AppendLine("</sheetData>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Creates a minimal valid .xlsx file in memory with the given data.
    /// </summary>
    private static MemoryStream CreateSimpleExcel(string[][] rows, float[]? columnWidths = null, bool customWidth = true,
        bool printTitleFirstRow = false, bool includeUnflaggedFitAttributes = false)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // [Content_Types].xml
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            // _rels/.rels
            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            // xl/_rels/workbook.xml.rels
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);

                        var definedNames = printTitleFirstRow
                                ? """
                                        <definedNames>
                                            <definedName name="_xlnm.Print_Titles" localSheetId="0">'Sheet1'!$1:$1</definedName>
                                        </definedNames>
                                    """
                                : "";

                        // xl/workbook.xml
            AddEntry(archive, "xl/workbook.xml",
                                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                                {{definedNames}}
                </workbook>
                """);

            // Build shared strings and sheet data
            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            // Build sheet XML
            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            if (columnWidths is { Length: > 0 })
            {
                sheetSb.AppendLine("<cols>");
                for (var c = 0; c < columnWidths.Length; c++)
                {
                    var width = columnWidths[c].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var customWidthAttr = customWidth ? " customWidth=\"1\"" : "";
                    sheetSb.AppendLine($"  <col min=\"{c + 1}\" max=\"{c + 1}\" width=\"{width}\"{customWidthAttr}/>");
                }
                sheetSb.AppendLine("</cols>");
            }
            sheetSb.AppendLine("<sheetData>");

            for (var r = 0; r < rows.Length; r++)
            {
                sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                for (var c = 0; c < rows[r].Length; c++)
                {
                    var colLetter = (char)('A' + c);
                    var cellRef = $"{colLetter}{r + 1}";
                    var idx = GetStringIndex(rows[r][c]);
                    sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\"><v>{idx}</v></c>");
                }
                sheetSb.AppendLine("  </row>");
            }

            sheetSb.AppendLine("</sheetData>");
            if (includeUnflaggedFitAttributes)
                sheetSb.AppendLine("<pageSetup paperSize=\"1\" scale=\"100\" fitToWidth=\"1\" fitToHeight=\"1\"/>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());

            // Build shared strings XML
            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");

            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

        private static MemoryStream CreateExcelWithImageOverCell()
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
                                    <Default Extension="png" ContentType="image/png"/>
                                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                                    <Override PartName="/xl/drawings/drawing1.xml" ContentType="application/vnd.openxmlformats-officedocument.drawing+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/_rels/workbook.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/workbook.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <sheets><sheet name="Sheet1" sheetId="1" r:id="rId1"/></sheets>
                                </workbook>
                                """);
                        AddEntry(archive, "xl/worksheets/sheet1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                     xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                      <sheetData><row r="1"><c r="D1" t="inlineStr"><is><t>Covered</t></is></c></row></sheetData>
                                    <drawing r:id="rId1"/>
                                </worksheet>
                                """);
                        AddEntry(archive, "xl/worksheets/_rels/sheet1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing" Target="../drawings/drawing1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/drawings/drawing1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
                                                    xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <xdr:oneCellAnchor>
                                        <xdr:from><xdr:col>0</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>0</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
                                        <xdr:ext cx="1905000" cy="914400"/>
                                        <xdr:pic><xdr:blipFill><a:blip r:embed="rId1"/></xdr:blipFill></xdr:pic>
                                        <xdr:clientData/>
                                    </xdr:oneCellAnchor>
                                </xdr:wsDr>
                                """);
                        AddEntry(archive, "xl/drawings/_rels/drawing1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="../media/image1.png"/>
                                </Relationships>
                                """);

                        var imageEntry = archive.CreateEntry("xl/media/image1.png");
                        using var imageStream = imageEntry.Open();
                        var image = Convert.FromBase64String(
                                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/lLQ2VwAAAABJRU5ErkJggg==");
                        imageStream.Write(image, 0, image.Length);
                }

                stream.Position = 0;
                return stream;
        }

    /// <summary>
    /// Creates a minimal .xlsx containing a single clustered-column bar chart
    /// with the given category labels and values embedded as chart cache data
    /// (no dependency on resolving worksheet cell references).
    /// </summary>
    private static MemoryStream CreateExcelWithBarChart(string[] categories, double[] values)
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
                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                    <Override PartName="/xl/drawings/drawing1.xml" ContentType="application/vnd.openxmlformats-officedocument.drawing+xml"/>
                    <Override PartName="/xl/charts/chart1.xml" ContentType="application/vnd.openxmlformats-officedocument.drawingml.chart+xml"/>
                </Types>
                """);
            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <sheets><sheet name="Sheet1" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddEntry(archive, "xl/worksheets/sheet1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                     xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                      <sheetData/>
                    <drawing r:id="rId1"/>
                </worksheet>
                """);
            AddEntry(archive, "xl/worksheets/_rels/sheet1.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing" Target="../drawings/drawing1.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/drawings/drawing1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
                                    xmlns:c="http://schemas.openxmlformats.org/drawingml/2006/chart"
                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <xdr:twoCellAnchor>
                        <xdr:from><xdr:col>0</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>0</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
                        <xdr:to><xdr:col>10</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>20</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:to>
                        <xdr:graphicFrame>
                            <xdr:nvGraphicFramePr>
                                <xdr:cNvPr id="2" name="Chart 1"/>
                                <xdr:cNvGraphicFramePr/>
                            </xdr:nvGraphicFramePr>
                            <xdr:xfrm><a:off x="0" y="0" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"/><a:ext cx="5486400" cy="2971800" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"/></xdr:xfrm>
                            <a:graphic xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                <a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/chart">
                                    <c:chart r:id="rId1"/>
                                </a:graphicData>
                            </a:graphic>
                        </xdr:graphicFrame>
                        <xdr:clientData/>
                    </xdr:twoCellAnchor>
                </xdr:wsDr>
                """);
            AddEntry(archive, "xl/drawings/_rels/drawing1.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart" Target="../charts/chart1.xml"/>
                </Relationships>
                """);

            var catPts = string.Join("", categories.Select((cat, i) =>
                $"""<c:pt idx="{i}"><c:v>{EscapeXml(cat)}</c:v></c:pt>"""));
            var valPts = string.Join("", values.Select((val, i) =>
                $"""<c:pt idx="{i}"><c:v>{val.ToString(System.Globalization.CultureInfo.InvariantCulture)}</c:v></c:pt>"""));

            AddEntry(archive, "xl/charts/chart1.xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <c:chartSpace xmlns:c="http://schemas.openxmlformats.org/drawingml/2006/chart"
                                            xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
                                    <c:style val="10"/>
                    <c:chart>
                                        <c:title><c:tx><c:rich><a:bodyPr/><a:p><a:r><a:t>Clustered Non-Zero Prices</a:t></a:r></a:p></c:rich></c:tx></c:title>
                        <c:plotArea>
                            <c:barChart>
                                <c:barDir val="col"/>
                                <c:grouping val="clustered"/>
                                <c:ser>
                                    <c:idx val="0"/>
                                    <c:order val="0"/>
                                                        <c:tx><c:v>Price</c:v></c:tx>
                                    <c:cat>
                                        <c:strRef>
                                            <c:f>Sheet1!$A$2:$A${{categories.Length + 1}}</c:f>
                                            <c:strCache>{{catPts}}</c:strCache>
                                        </c:strRef>
                                    </c:cat>
                                    <c:val>
                                        <c:numRef>
                                            <c:f>Sheet1!$B$2:$B${{values.Length + 1}}</c:f>
                                            <c:numCache>{{valPts}}</c:numCache>
                                        </c:numRef>
                                    </c:val>
                                </c:ser>
                                                    <c:gapWidth val="150"/>
                            </c:barChart>
                        </c:plotArea>
                                        <c:legend><c:legendPos val="r"/></c:legend>
                    </c:chart>
                </c:chartSpace>
                """);
        }

        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateMultiSheetExcel((string Name, string[][] Rows)[] sheets)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var worksheetOverrides = string.Join(Environment.NewLine, sheets.Select((_, index) =>
                $"  <Override PartName=\"/xl/worksheets/sheet{index + 1}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"));
            AddEntry(archive, "[Content_Types].xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                {{worksheetOverrides}}
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            var rels = new StringBuilder();
            for (var i = 0; i < sheets.Length; i++)
            {
                rels.AppendLine($"  <Relationship Id=\"rId{i + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i + 1}.xml\"/>");
            }
            rels.AppendLine($"  <Relationship Id=\"rId{sheets.Length + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>");
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                {{rels}}</Relationships>
                """);

            var workbookSheets = new StringBuilder();
            for (var i = 0; i < sheets.Length; i++)
            {
                workbookSheets.AppendLine($"    <sheet name=\"{EscapeXml(sheets[i].Name)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
            }
            AddEntry(archive, "xl/workbook.xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                {{workbookSheets}}  </sheets>
                </workbook>
                """);

            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            for (var sheetIndex = 0; sheetIndex < sheets.Length; sheetIndex++)
            {
                var sheetSb = new StringBuilder();
                sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
                sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
                sheetSb.AppendLine("<sheetData>");
                for (var r = 0; r < sheets[sheetIndex].Rows.Length; r++)
                {
                    sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                    for (var c = 0; c < sheets[sheetIndex].Rows[r].Length; c++)
                    {
                        var colLetter = (char)('A' + c);
                        var cellRef = $"{colLetter}{r + 1}";
                        var idx = GetStringIndex(sheets[sheetIndex].Rows[r][c]);
                        sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\"><v>{idx}</v></c>");
                    }
                    sheetSb.AppendLine("  </row>");
                }
                sheetSb.AppendLine("</sheetData>");
                sheetSb.AppendLine("</worksheet>");
                AddEntry(archive, $"xl/worksheets/sheet{sheetIndex + 1}.xml", sheetSb.ToString());
            }

            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");
            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

        private static MemoryStream CreateExcelWithWorksheetTarget(string worksheetTarget, string cellText)
        {
                var ms = new MemoryStream();
                var worksheetPart = "xl/" + worksheetTarget.Replace('\\', '/').TrimStart('/');

                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                        AddEntry(archive, "[Content_Types].xml",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                                    <Default Extension="xml" ContentType="application/xml"/>
                                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                                    <Override PartName="/{{worksheetPart}}" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                                    <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                                </Types>
                                """);

                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                                </Relationships>
                                """);

                        AddEntry(archive, "xl/_rels/workbook.xml.rels",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="{{worksheetTarget}}"/>
                                    <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                                </Relationships>
                                """);

                        AddEntry(archive, "xl/workbook.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <sheets>
                                        <sheet name="Report" sheetId="1" r:id="rId1"/>
                                    </sheets>
                                </workbook>
                                """);

                        AddEntry(archive, worksheetPart,
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                                    <sheetData>
                                        <row r="1">
                                            <c r="A1" t="s"><v>0</v></c>
                                        </row>
                                    </sheetData>
                                </worksheet>
                                """);

                        AddEntry(archive, "xl/sharedStrings.xml",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="1" uniqueCount="1">
                                    <si><t>{{EscapeXml(cellText)}}</t></si>
                                </sst>
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

    /// <summary>
    /// Creates a minimal .xlsx with per-cell font colors.
    /// Each row is an array of (text, argbHex) tuples. Empty argb = default/black.
    /// </summary>
    private static MemoryStream CreateColoredExcel(params (string text, string argb)[][] rows)
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
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            // Collect unique colors and build fonts + cellXfs
            var colorToFontIndex = new Dictionary<string, int>();
            var fontsSb = new StringBuilder();
            var xfsSb = new StringBuilder();

            // Font 0 / xf 0 = default (black, no color element)
            fontsSb.AppendLine("  <font><sz val=\"11\"/><name val=\"Calibri\"/></font>");
            xfsSb.AppendLine("  <xf fontId=\"0\"/>");
            var nextFontId = 1;
            var nextXfId = 1;

            // Map (fontIndex -> xfIndex) for non-default colors
            var colorToXfIndex = new Dictionary<string, int>();

            foreach (var row in rows)
            {
                foreach (var (_, argb) in row)
                {
                    if (string.IsNullOrEmpty(argb) || colorToXfIndex.ContainsKey(argb))
                        continue;

                    fontsSb.AppendLine($"  <font><color rgb=\"{argb}\"/><sz val=\"11\"/><name val=\"Calibri\"/></font>");
                    colorToFontIndex[argb] = nextFontId;

                    xfsSb.AppendLine($"  <xf fontId=\"{nextFontId}\"/>");
                    colorToXfIndex[argb] = nextXfId;

                    nextFontId++;
                    nextXfId++;
                }
            }

            var stylesSb = new StringBuilder();
            stylesSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            stylesSb.AppendLine("""<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            stylesSb.AppendLine($"<fonts count=\"{nextFontId}\">");
            stylesSb.Append(fontsSb);
            stylesSb.AppendLine("</fonts>");
            stylesSb.AppendLine($"<cellXfs count=\"{nextXfId}\">");
            stylesSb.Append(xfsSb);
            stylesSb.AppendLine("</cellXfs>");
            stylesSb.AppendLine("</styleSheet>");

            AddEntry(archive, "xl/styles.xml", stylesSb.ToString());

            // Shared strings
            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            // Sheet data
            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            sheetSb.AppendLine("<sheetData>");

            for (var r = 0; r < rows.Length; r++)
            {
                sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                for (var c = 0; c < rows[r].Length; c++)
                {
                    var colLetter = (char)('A' + c);
                    var cellRef = $"{colLetter}{r + 1}";
                    var idx = GetStringIndex(rows[r][c].text);
                    var styleIdx = !string.IsNullOrEmpty(rows[r][c].argb) && colorToXfIndex.TryGetValue(rows[r][c].argb, out var si) ? si : 0;
                    sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\" s=\"{styleIdx}\"><v>{idx}</v></c>");
                }
                sheetSb.AppendLine("  </row>");
            }

            sheetSb.AppendLine("</sheetData>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());

            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");

            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }
}
