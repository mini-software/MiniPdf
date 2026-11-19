namespace MiniSoftware.Tests;

/// <summary>
/// Runs registered-font tests apart from other collections. Registrations are process-wide,
/// so conversions running in parallel would otherwise read the fonts these tests register.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class RegisteredFontCollection
{
    public const string Name = "Registered fonts";
}

[Collection(RegisteredFontCollection.Name)]
public class RegisteredFontTests : IDisposable
{
    /// <summary>
    /// Starts each test with no registered fonts.
    /// </summary>
    public RegisteredFontTests() => MiniPdf.ClearRegisteredFonts();

    /// <summary>
    /// Clears the fonts a test registered so the next test starts from an empty list.
    /// </summary>
    public void Dispose() => MiniPdf.ClearRegisteredFonts();

    /// <summary>
    /// Clearing removes every registration, including repeated registrations of the same name.
    /// </summary>
    [Fact]
    public void ClearRegisteredFonts_RemovesAllRegistrations()
    {
        MiniPdf.RegisterFont("First", new byte[] { 1 });
        MiniPdf.RegisterFont("Second", new byte[] { 2 });
        MiniPdf.RegisterFont("First", new byte[] { 3 });
        Assert.Equal(3, MiniPdf.GetRegisteredFonts().Count);

        MiniPdf.ClearRegisteredFonts();

        Assert.Empty(MiniPdf.GetRegisteredFonts());
    }

    /// <summary>
    /// A host can swap font sets between conversions by clearing and registering again.
    /// </summary>
    [Fact]
    public void ClearRegisteredFonts_AllowsRegisteringADifferentFontSet()
    {
        MiniPdf.RegisterFont("NotoSansTC", new byte[] { 1 });

        MiniPdf.ClearRegisteredFonts();
        MiniPdf.RegisterFont("NotoSansJP", new byte[] { 2 });

        var registered = Assert.Single(MiniPdf.GetRegisteredFonts());
        Assert.Equal("NotoSansJP", registered.Name);
    }

    /// <summary>
    /// A snapshot taken before clearing, such as the one a conversion in progress reads, is not changed.
    /// </summary>
    [Fact]
    public void ClearRegisteredFonts_DoesNotChangeEarlierSnapshot()
    {
        MiniPdf.RegisterFont("NotoSansTC", new byte[] { 1 });
        var snapshot = MiniPdf.GetRegisteredFonts();

        MiniPdf.ClearRegisteredFonts();

        Assert.Equal("NotoSansTC", Assert.Single(snapshot).Name);
    }

    [Fact]
    public void MetafileResolverUsesTheRegisteredFamilyWithoutSubstitution()
    {
        var data = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        MiniPdf.RegisterFont("MiniPdfRegisteredMetafileFace", data);

        Assert.Same(data, PdfWriter.ResolveMetafileFont("MiniPdfRegisteredMetafileFace", false, false));
    }

    [Fact]
    public void MetafileResolverUsesSameFamilyOutlinesForSyntheticBold()
    {
        var data = TrueTypeGlyphReaderTests.CreateFont(TrueTypeGlyphReaderTests.Box());
        TrueTypeGlyphReaderTests.Write16(data, TrueTypeGlyphReaderTests.TableOffset(data, "OS/2") + 4, 400);
        MiniPdf.RegisterFont("MiniPdfRegisteredMetafileFace", data);

        Assert.Same(data, PdfWriter.ResolveMetafileFont("MiniPdfRegisteredMetafileFace", true, false));
    }

    [Fact]
    public void MetafileResolverRejectsAnUnavailableFamily()
    {
        Assert.Throws<NotSupportedException>(() =>
            PdfWriter.ResolveMetafileFont("MiniPdfNoSuchMetafileFamilyF6DD51", false, false));
    }
}
