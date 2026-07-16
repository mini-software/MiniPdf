using System.Text;

namespace MiniSoftware.Tests;

public class DocxIssueFileTests
{
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