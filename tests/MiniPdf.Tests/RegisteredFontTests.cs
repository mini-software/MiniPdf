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
    public RegisteredFontTests() => MiniPdf.ClearRegisteredFonts();

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
}
