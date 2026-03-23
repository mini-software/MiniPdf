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

if (app.Environment.IsDevelopment())
{
    app.MapGet("/test", () => Results.Content("""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>MiniPdf Convert Test</title>
        <style>
            body { font-family: sans-serif; max-width: 600px; margin: 40px auto; padding: 0 20px; }
            h1 { color: #333; }
            #dropZone { border: 2px dashed #aaa; border-radius: 8px; padding: 40px; text-align: center; color: #666; cursor: pointer; transition: border-color .2s; }
            #dropZone.hover { border-color: #4a90d9; background: #f0f7ff; }
            #status { margin-top: 16px; }
            .error { color: red; } .success { color: green; }
            button { margin-top: 12px; padding: 8px 20px; font-size: 14px; cursor: pointer; }
        </style>
    </head>
    <body>
        <h1>MiniPdf Convert Test</h1>
        <p>Upload a <strong>.xlsx</strong> or <strong>.docx</strong> file to convert to PDF.</p>
        <div id="dropZone">Drag &amp; drop file here, or click to select</div>
        <input type="file" id="fileInput" accept=".xlsx,.docx" hidden />
        <button id="convertBtn" disabled>Convert to PDF</button>
        <div id="status"></div>
        <script>
            const dropZone = document.getElementById('dropZone');
            const fileInput = document.getElementById('fileInput');
            const convertBtn = document.getElementById('convertBtn');
            const status = document.getElementById('status');
            let selectedFile = null;

            dropZone.addEventListener('click', () => fileInput.click());
            dropZone.addEventListener('dragover', e => { e.preventDefault(); dropZone.classList.add('hover'); });
            dropZone.addEventListener('dragleave', () => dropZone.classList.remove('hover'));
            dropZone.addEventListener('drop', e => {
                e.preventDefault(); dropZone.classList.remove('hover');
                if (e.dataTransfer.files.length) pick(e.dataTransfer.files[0]);
            });
            fileInput.addEventListener('change', () => { if (fileInput.files.length) pick(fileInput.files[0]); });

            function pick(f) { selectedFile = f; dropZone.textContent = f.name; convertBtn.disabled = false; status.textContent = ''; }

            convertBtn.addEventListener('click', async () => {
                if (!selectedFile) return;
                convertBtn.disabled = true;
                status.textContent = 'Converting...'; status.className = '';
                try {
                    const fd = new FormData(); fd.append('file', selectedFile);
                    const res = await fetch('/api/convert', { method: 'POST', body: fd });
                    if (!res.ok) { status.textContent = 'Error: ' + await res.text(); status.className = 'error'; return; }
                    const blob = await res.blob();
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a'); a.href = url;
                    a.download = selectedFile.name.replace(/\.\w+$/, '.pdf');
                    a.click(); URL.revokeObjectURL(url);
                    status.textContent = 'Done!'; status.className = 'success';
                } catch (e) { status.textContent = 'Error: ' + e.message; status.className = 'error'; }
                finally { convertBtn.disabled = false; }
            });
        </script>
    </body>
    </html>
    """, "text/html"));
}

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
