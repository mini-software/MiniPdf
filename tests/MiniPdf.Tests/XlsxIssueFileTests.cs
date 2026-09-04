using System.Text;
using System.Text.RegularExpressions;

namespace MiniSoftware.Tests;

public class XlsxIssueFileTests
{
    [Fact]
    public void Issue202609031340_PreservesLayoutAndOlePreviews()
    {
        var issuePath = FindIssueXlsx("Issue202609031340.xlsx");

        using var stream = File.OpenRead(issuePath);
        var sheets = ExcelReader.ReadSheets(stream);
        Assert.All(sheets, sheet => Assert.True(sheet.VerticalCentered));
        Assert.Equal(255, sheets[0].Rows[45][0].TextRotation);
        if (Compat.IsWindows())
        {
            Assert.Equal(2, sheets[0].Images.Count);
            Assert.All(sheets[0].Images, image =>
            {
                Assert.Equal("png", image.Extension);
                Assert.NotNull(image.AbsoluteLeftPt);
                Assert.NotNull(image.AbsoluteTopPt);
                Assert.NotNull(image.VmlFromColOffset);
                Assert.NotNull(image.VmlFromRowOffset);
                Assert.NotNull(image.VmlToColOffset);
                Assert.NotNull(image.VmlToRowOffset);
            });
        }

        var doc = ExcelToPdfConverter.Convert(issuePath);

        Assert.Equal(4, doc.Pages.Count);
        Assert.All(new[] { doc.Pages[1], doc.Pages[3] }, page =>
        {
            Assert.NotEmpty(page.LineBlocks);
            Assert.True(page.LineBlocks.Max(line => Math.Max(line.Y1, line.Y2)) < page.Height * 0.7f);
            Assert.True(page.LineBlocks.Min(line => Math.Min(line.Y1, line.Y2)) > page.Height * 0.3f);
        });
        Assert.All("修订记录", character =>
            Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == character.ToString()));
        if (Compat.IsWindows())
        {
            Assert.Equal(2, doc.Pages[0].ImageBlocks.Count);
            var imageTops = doc.Pages[0].ImageBlocks
                .Select(image => doc.Pages[0].Height - image.Y - image.RenderHeight)
                .OrderBy(top => top)
                .ToArray();
            Assert.InRange(imageTops[0], 215f, 218f);
            Assert.InRange(imageTops[1], 528f, 530f);
        }
    }

    [Fact]
    public void AcademicAchievement_ManualPageBreak_UsesIntegerScaleForFixedRows()
    {
        var issuePath = FindIssueXlsx("Academic Achievement Summary Table.xlsx");

        var doc = ExcelToPdfConverter.Convert(issuePath);

        Assert.Equal(2, doc.Pages.Count);
        var horizontalGridLines = doc.Pages[1].LineBlocks
            .Where(line => Math.Abs(line.Y1 - line.Y2) < 0.001f)
            .Select(line => line.Y1)
            .Distinct()
            .OrderByDescending(y => y)
            .ToArray();
        var repeatedFixedRows = horizontalGridLines
            .Zip(horizontalGridLines.Skip(1), (top, bottom) => top - bottom)
            .Count(height => Math.Abs(height - 28.4625f) < 0.001f);

        Assert.True(repeatedFixedRows >= 8,
            $"Expected repeated 37.95pt rows at 75% scale, found {repeatedFixedRows} matching rows.");
    }

    [Fact]
    public void Issue81_LayoutOptions_CompactLargeWorksheetOutput()
    {
        var issuePath = FindIssueXlsx("XlsxIssue81_LayoutOptions.xlsx");

        var defaultDoc = ExcelToPdfConverter.Convert(issuePath);
        var compactOptions = new ExcelToPdfConverter.ConversionOptions
        {
            FitToPage = true,
            Landscape = true,
            PrintScale = 70,
            RowsPerPage = 80,
        };
        var compactDoc = ExcelToPdfConverter.Convert(issuePath, compactOptions);

        Assert.True(defaultDoc.Pages.Count > compactDoc.Pages.Count,
            $"Expected layout options to reduce page count from {defaultDoc.Pages.Count}, got {compactDoc.Pages.Count}.");
        Assert.True(compactDoc.Pages[0].Width > compactDoc.Pages[0].Height,
            $"Expected landscape output, got {compactDoc.Pages[0].Width}x{compactDoc.Pages[0].Height}.");

        var compactBytes = MiniPdf.ConvertToPdf(issuePath, new MiniPdfConversionOptions
        {
            Compress = true,
            FitToPage = true,
            Landscape = true,
            PrintScale = 70,
            RowsPerPage = 80,
        });
        var compactPdf = Encoding.ASCII.GetString(compactBytes);

        Assert.Contains("/FlateDecode", compactPdf);
        Assert.Equal(compactDoc.Pages.Count, CountPdfPages(compactPdf));
    }

    [Fact]
    public void Issue82_WideTable_NoColumnMetadataFitsOnFinalPage()
    {
        var issuePath = FindIssueXlsx("XlsxIssue82_WideTable.xlsx");

        var doc = ExcelToPdfConverter.Convert(issuePath);

        Assert.Equal(13, doc.Pages.Count);
        Assert.Contains(doc.Pages[12].TextBlocks, block => block.Text == "Phone");
        Assert.Contains(doc.Pages[12].TextBlocks, block => block.Text.Contains("QA Automation Specialist"));
    }

    [Fact]
    public void BusinessExpenseBudget_ExplicitPageSetupPreservesHorizontalChartSlices()
    {
        var issuePath = FindIssueXlsx("Business expense budget1.xlsx");

        var doc = ExcelToPdfConverter.Convert(issuePath);

        Assert.Equal(4, doc.Pages.Count);
        Assert.DoesNotContain(doc.Pages[0].TextBlocks, block => block.Text == "Q2 ACTUAL");
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == "Budget vs Actual by Category");
        Assert.Contains(doc.Pages[2].TextBlocks, block => block.Text == "Q2 ACTUAL");
        Assert.Contains(doc.Pages[3].TextBlocks, block => block.Text.Contains("Professional Services"));
    }

    [Fact]
    public void BusinessExpensesBudget2_GroupedDrawingUsesPrintScale()
    {
        var issuePath = FindIssueXlsx("Business expenses budget2.xlsx");

        using var stream = File.OpenRead(issuePath);
        var sheet = Assert.Single(ExcelReader.ReadSheets(stream).Take(1));
        Assert.Equal("Wages", sheet.Rows[8][1].Text);
        var firstStripeFill = Assert.IsType<PdfColor>(sheet.Rows[8][1].FillColor);
        Assert.InRange(firstStripeFill.R, 0.84f, 0.86f);
        Assert.InRange(firstStripeFill.G, 0.84f, 0.86f);
        Assert.InRange(firstStripeFill.B, 0.84f, 0.86f);
        Assert.Equal("Benefits", sheet.Rows[9][1].Text);
        Assert.Null(sheet.Rows[9][1].FillColor);

        var doc = ExcelToPdfConverter.Convert(issuePath);
        var page = doc.Pages[0];

        Assert.Equal(2, page.ImageBlocks.Count);
        Assert.All(page.ImageBlocks, image =>
        {
            Assert.InRange(image.RenderWidth, 105f, 107f);
            Assert.InRange(image.RenderHeight, 67f, 69f);
        });

        var decoration = Assert.Single(page.PolygonBlocks);
        Assert.Equal(new PdfColor(0f, 0f, 0f), decoration.FillColor);
        Assert.Equal(0.5f, decoration.Alpha);
        var decorationWidth = decoration.Points.Max(point => point.X) - decoration.Points.Min(point => point.X);
        var decorationHeight = decoration.Points.Max(point => point.Y) - decoration.Points.Min(point => point.Y);
        Assert.InRange(decorationWidth, 89f, 91f);
        Assert.InRange(decorationHeight, 49f, 51f);

        using var output = new MemoryStream();
        doc.Save(output);
        var pdf = System.Text.Encoding.Latin1.GetString(output.ToArray());
        Assert.Contains("/GS_P0 << /Type /ExtGState /ca 0.500 >>", pdf);
        Assert.Contains("/GS_P0 gs", pdf);
    }

    private static string FindIssueXlsx(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "tests", "Issue_Files", "xlsx", fileName);
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not find issue XLSX file '{fileName}' from '{AppContext.BaseDirectory}'.");
    }

    private static int CountPdfPages(string pdf)
        => Regex.Matches(pdf, @"/Type /Page\b").Count;
}