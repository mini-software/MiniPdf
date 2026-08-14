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

        var labelRightEdges = labels.Select(block =>
        {
            Assert.True(PdfWriter.TryMeasurePreferredFontWidth(block.PreferredFontName, block.Text,
                block.FontSize, block.Bold, block.Italic, block.CharSpacing, out var width));
            return block.X + width;
        }).ToArray();
        Assert.InRange(labelRightEdges.Max() - labelRightEdges.Min(), 0, 0.1f);

        Assert.True(PdfWriter.TryMeasurePreferredFontWidth(dateValue.PreferredFontName, dateValue.Text,
            dateValue.FontSize, dateValue.Bold, dateValue.Italic, dateValue.CharSpacing, out var dateWidth));
        Assert.InRange(dateValue.X + dateWidth / 2, 380f, 385f);
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

    [Fact]
    public void Issue90_RunHighlightsAndWingdingsCheckMark_ArePreserved()
    {
        var issuePath = FindIssueDocx("TestIssue90.docx");

        using var stream = File.OpenRead(issuePath);
        var document = DocxReader.Read(stream);
        var paragraphs = document.Elements
            .OfType<DocxParagraph>()
            .ToList();
        var runs = paragraphs
            .SelectMany(paragraph => paragraph.Runs)
            .Concat(paragraphs
                .SelectMany(paragraph => paragraph.FloatingTextBoxes ?? [])
                .SelectMany(textBox => textBox.Paragraphs)
                .SelectMany(paragraph => paragraph.Runs))
            .ToList();

        var highlightedName = Assert.Single(runs,
            run => run.Text == "Họ tên khách hàng:");
        Assert.Equal(PdfColor.FromHex("FFFF00"), highlightedName.Shading);
        Assert.Contains(runs, run => run.Text.Contains('✓'));

        stream.Position = 0;
        var pdf = DocxToPdfConverter.Convert(stream);
        var page = Assert.Single(pdf.Pages);
        var checkedBox = Assert.Single(page.RectBlocks, rectangle =>
            rectangle.X is > 332f and < 334f
            && rectangle.Width is > 20f and < 21f
            && rectangle.Height is > 17f and < 19f);
        Assert.InRange(checkedBox.Y, 481.5f, 482.5f);

        var superscript = Assert.Single(page.TextBlocks, block =>
            block.Text == "1" && block.FontSize is > 7.9f and < 8.1f);
        Assert.InRange(superscript.FontSize, 7.9f, 8.1f);

        var yesLabel = Assert.Single(page.TextBlocks, block => block.Text == "Có");
        Assert.InRange(yesLabel.X, 359.9f, 360.1f);
        Assert.Contains(page.TextBlocks, block =>
            block.Text.EndsWith("cho Tài khoản", StringComparison.Ordinal));

        var footnoteSeparator = Assert.Single(page.LineBlocks, line =>
            line.X1 is > 71.9f and < 72.1f
            && line.X2 is > 215.9f and < 216.1f
            && line.Y1 is > 88.9f and < 89.1f
            && Math.Abs(line.Y1 - line.Y2) < 0.01f);
        Assert.Equal(144f, footnoteSeparator.X2 - footnoteSeparator.X1, 1);
        var footnoteText = Assert.Single(page.TextBlocks, block =>
            block.Text.Contains("Giấy phép tương đương", StringComparison.Ordinal));
        Assert.InRange(footnoteText.Y, 74.9f, 75.1f);
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