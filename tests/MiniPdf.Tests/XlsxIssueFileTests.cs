using System.Text;
using System.Text.RegularExpressions;

namespace MiniSoftware.Tests;

public class XlsxIssueFileTests
{
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