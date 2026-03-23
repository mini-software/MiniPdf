using MiniSoftware;

// Register fonts from /home/fonts/ (Azure App Service persistent storage)
// and from <app>/Fonts/ (local development fallback)
foreach (var dir in new[] { "/home/fonts", Path.Combine(AppContext.BaseDirectory, "Fonts") })
{
    if (!Directory.Exists(dir)) continue;
    foreach (var ext in new[] { "*.ttf", "*.ttc", "*.otf" })
        foreach (var file in Directory.GetFiles(dir, ext))
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(file);
                MiniPdf.RegisterFont(name, File.ReadAllBytes(file));
            }
            catch { /* skip fonts that fail to parse */ }
        }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://mini-software.github.io")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

app.MapPost("/api/convert", async (IFormFile file) =>
{
    var ext = Path.GetExtension(file.FileName);
    if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
        !ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Only .xlsx and .docx files are supported.");
    }

    using var stream = file.OpenReadStream();
    byte[] pdfBytes = ext.Equals(".docx", StringComparison.OrdinalIgnoreCase)
        ? MiniPdf.ConvertDocxToPdf(stream)
        : MiniPdf.ConvertToPdf(stream);

    return Results.File(pdfBytes, "application/pdf", "output.pdf");
}).DisableAntiforgery();

app.Run();
