namespace MiniSoftware.Tests;

public class OfficeCloudFontTests
{
    [Fact]
    public void PreferredFontLookup_ResolvesNumericOfficeCloudFontFiles()
    {
        if (!OperatingSystem.IsWindows()) return;

        var windowsFonts = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        var regularSource = Path.Combine(windowsFonts, "arial.ttf");
        var boldSource = Path.Combine(windowsFonts, "arialbd.ttf");
        if (!File.Exists(regularSource) || !File.Exists(boldSource)) return;

        var cacheRoot = Path.Combine(Path.GetTempPath(), $"minipdf-font-cache-{Guid.NewGuid():N}");
        var cloudFamily = Path.Combine(cacheRoot, "4", "CloudFonts", "Arial");
        Directory.CreateDirectory(cloudFamily);
        var regularTarget = Path.Combine(cloudFamily, "123456789.ttf");
        var boldTarget = Path.Combine(cloudFamily, "987654321.ttf");
        File.Copy(regularSource, regularTarget);
        File.Copy(boldSource, boldTarget);

        try
        {
            Assert.Equal(regularTarget,
                PdfWriter.FindOfficeCloudFontByPreferredName("Arial", cacheRoot));
            Assert.Equal(boldTarget,
                PdfWriter.FindOfficeCloudFontByPreferredName("Arial Bold", cacheRoot));
        }
        finally
        {
            Directory.Delete(cacheRoot, recursive: true);
        }
    }
}