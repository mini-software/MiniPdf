using System.Text;
using System.Text.RegularExpressions;

namespace MiniSoftware.Tests;

public class XlsxIssueFileTests
{
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