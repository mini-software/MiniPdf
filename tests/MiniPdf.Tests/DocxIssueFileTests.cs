using System.Text;

namespace MiniSoftware.Tests;

public class DocxIssueFileTests
{
    [Fact]
    public void Issue78_HeaderTable_UsesPreferredFontWidthsForAlignment()
    {
        var issuePath = FindIssueDocx("TestIssue78.docx");

        using var stream = File.OpenRead(issuePath);
        var document = DocxToPdfConverter.Convert(stream);
        var firstPage = Assert.Single(document.Pages);
        var labels = new[]
        {
            "使 用 单 位 名 称：",
            "单  位  内  编 号：",
            "设   备   代  码 ：",
            "设   备   类  别 ：",
            "检   测   日  期 ：",
        }.Select(text => Assert.Single(firstPage.TextBlocks, block => block.Text == text)).ToArray();
        var dateValue = Assert.Single(firstPage.TextBlocks,
            block => block.Text == "#[checkStart]# 至 #[checkEnd]#");

        Assert.InRange(labels.Max(block => block.X) - labels.Min(block => block.X), 0, 0.1f);
        Assert.InRange(dateValue.X, 275f, 280f);
    }

    [Fact]
    public void Issue78_ParagraphTab_DoesNotMoveNumberedBodyText()
    {
        var issuePath = FindIssueDocx("TestIssue78.docx");

        using var stream = File.OpenRead(issuePath);
        var document = DocxToPdfConverter.Convert(stream);
        var firstPage = Assert.Single(document.Pages);
        var label = Assert.Single(firstPage.TextBlocks, block => block.Text == "一、");
        var body = Assert.Single(firstPage.TextBlocks,
            block => block.Text.StartsWith("本原始记录适用于", StringComparison.Ordinal));

        Assert.InRange(Math.Abs(body.Y - label.Y), 0, 0.1f);
        Assert.InRange(label.X, 85f, 105f);
        Assert.InRange(body.X - label.X, 0, 60f);
    }

    [Theory]
    [InlineData("Issue79_FilledContract.docx")]
    [InlineData("Issue79_TemplateContract.docx")]
    public void Issue79_EmbeddedCjkFonts_ProducesCompactPdf(string fileName)
    {
        var issuePath = FindIssueDocx(fileName);

        var pdf = MiniPdf.ConvertToPdf(issuePath);
        var content = Encoding.ASCII.GetString(pdf);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("/FontFile2", content);
        Assert.True(pdf.Length < 1_000_000,
            $"Expected a compact PDF below 1 MB, got {pdf.Length:N0} bytes.");
    }

    private static string FindIssueDocx(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "tests", "Issue_Files", "docx", fileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find issue DOCX file '{fileName}' from '{AppContext.BaseDirectory}'.");
    }
}