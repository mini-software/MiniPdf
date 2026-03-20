using MiniSoftware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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
});

app.Run();
