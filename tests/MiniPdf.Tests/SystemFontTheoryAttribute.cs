namespace MiniSoftware.Tests;

public sealed class SystemFontTheoryAttribute : TheoryAttribute
{
    public SystemFontTheoryAttribute(params string[] fontNames)
    {
        foreach (var fontName in fontNames)
        {
            if (!PdfWriter.TryMeasurePreferredFontWidth(fontName, "A", 11f,
                bold: false, italic: false, charSpacing: 0f, out _))
            {
                Skip = $"Required system font is unavailable: {fontName}";
                return;
            }
        }
    }
}

public class SystemFontTheoryAttributeTests
{
    [Fact]
    public void MissingFont_IsReportedAsSkipped()
    {
        const string missingFont = "MiniPdf Missing Test Font";
        var attribute = new SystemFontTheoryAttribute(missingFont);

        Assert.Equal($"Required system font is unavailable: {missingFont}", attribute.Skip);
    }
}
