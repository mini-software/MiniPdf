using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
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

// --- SQLite usage stats ---
var dbPath = Path.Combine(AppContext.BaseDirectory, "usage.db");
var connStr = $"Data Source={dbPath}";
using (var initConn = new SqliteConnection(connStr))
{
    initConn.Open();
    using var cmd = initConn.CreateCommand();
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS request_log (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            timestamp TEXT NOT NULL,
            ip_hash TEXT NOT NULL,
            file_type TEXT NOT NULL,
            file_size INTEGER NOT NULL,
            duration_ms INTEGER NOT NULL,
            success INTEGER NOT NULL
        )
        """;
    cmd.ExecuteNonQuery();
}

void LogRequest(string ipHash, string fileType, long fileSize, long durationMs, bool success)
{
    try
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO request_log (timestamp, ip_hash, file_type, file_size, duration_ms, success) VALUES (@ts, @ip, @ft, @fs, @d, @s)";
        cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("@ip", ipHash);
        cmd.Parameters.AddWithValue("@ft", fileType);
        cmd.Parameters.AddWithValue("@fs", fileSize);
        cmd.Parameters.AddWithValue("@d", durationMs);
        cmd.Parameters.AddWithValue("@s", success ? 1 : 0);
        cmd.ExecuteNonQuery();
    }
    catch { /* non-critical */ }
}

string HashIp(string ip)
{
    var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ip + "minipdf-salt"));
    return Convert.ToHexString(bytes)[..16];
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

// --- IP-based rate limiting: 10 requests per day per IP, 10 MB max ---
const int MaxRequestsPerDay = 10;
const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
var ipRequestCounts = new ConcurrentDictionary<string, (int Count, DateTime ResetTime)>();

string GetClientIp(HttpContext ctx)
{
    // Prefer X-Forwarded-For header (Azure / reverse proxy)
    var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrEmpty(forwarded))
        return forwarded.Split(',', StringSplitOptions.TrimEntries)[0];
    return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

bool TryConsumeRateLimit(string ip)
{
    var now = DateTime.UtcNow;
    var resetTime = now.Date.AddDays(1); // midnight UTC

    var entry = ipRequestCounts.AddOrUpdate(
        ip,
        _ => (1, resetTime),
        (_, existing) =>
        {
            if (now >= existing.ResetTime)
                return (1, resetTime); // new day, reset
            return (existing.Count + 1, existing.ResetTime);
        });

    return entry.Count <= MaxRequestsPerDay;
}

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

app.MapPost("/api/convert", async (HttpContext ctx, IFormFile file) =>
{
    // Enforce file size limit (10 MB)
    if (file.Length > MaxFileSizeBytes)
    {
        return Results.BadRequest("File size exceeds the 10 MB limit.");
    }

    // Enforce IP-based rate limit (10 per day)
    var clientIp = GetClientIp(ctx);
    if (!TryConsumeRateLimit(clientIp))
    {
        return Results.Json(
            new { error = "Rate limit exceeded. Maximum 10 conversions per day per IP." },
            statusCode: 429);
    }

    var ext = Path.GetExtension(file.FileName);
    if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
        !ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Only .xlsx and .docx files are supported.");
    }

    var sw = Stopwatch.StartNew();
    var ipHash = HashIp(clientIp);
    try
    {
        using var stream = file.OpenReadStream();
        byte[] pdfBytes = ext.Equals(".docx", StringComparison.OrdinalIgnoreCase)
            ? MiniPdf.ConvertDocxToPdf(stream)
            : MiniPdf.ConvertToPdf(stream);

        sw.Stop();
        LogRequest(ipHash, ext.ToLowerInvariant(), file.Length, sw.ElapsedMilliseconds, true);

        return Results.File(pdfBytes, "application/pdf", "output.pdf");
    }
    catch (Exception)
    {
        sw.Stop();
        LogRequest(ipHash, ext.ToLowerInvariant(), file.Length, sw.ElapsedMilliseconds, false);
        throw;
    }
}).DisableAntiforgery();

app.MapGet("/api/stats", () =>
{
    try
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();

        // Daily summary for last 30 days
        using var dailyCmd = conn.CreateCommand();
        dailyCmd.CommandText = """
            SELECT date(timestamp) as day, COUNT(*) as count,
                   CAST(AVG(duration_ms) AS INTEGER) as avg_ms,
                   CAST(MIN(duration_ms) AS INTEGER) as min_ms,
                   CAST(MAX(duration_ms) AS INTEGER) as max_ms,
                   SUM(CASE WHEN success = 1 THEN 1 ELSE 0 END) as success_count
            FROM request_log
            WHERE timestamp >= datetime('now', '-30 days')
            GROUP BY date(timestamp)
            ORDER BY day DESC
            """;
        var daily = new List<object>();
        using (var reader = dailyCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                daily.Add(new
                {
                    date = reader.GetString(0),
                    count = reader.GetInt32(1),
                    avgMs = reader.GetInt32(2),
                    minMs = reader.GetInt32(3),
                    maxMs = reader.GetInt32(4),
                    successCount = reader.GetInt32(5),
                });
            }
        }

        // File type breakdown
        using var typeCmd = conn.CreateCommand();
        typeCmd.CommandText = """
            SELECT file_type, COUNT(*) as count, CAST(AVG(duration_ms) AS INTEGER) as avg_ms
            FROM request_log
            WHERE timestamp >= datetime('now', '-30 days')
            GROUP BY file_type
            """;
        var byType = new List<object>();
        using (var reader2 = typeCmd.ExecuteReader())
        {
            while (reader2.Read())
            {
                byType.Add(new
                {
                    fileType = reader2.GetString(0),
                    count = reader2.GetInt32(1),
                    avgMs = reader2.GetInt32(2),
                });
            }
        }

        // Total stats
        using var totalCmd = conn.CreateCommand();
        totalCmd.CommandText = "SELECT COUNT(*), CAST(AVG(duration_ms) AS INTEGER) FROM request_log";
        int totalCount = 0, totalAvgMs = 0;
        using (var reader3 = totalCmd.ExecuteReader())
        {
            if (reader3.Read())
            {
                totalCount = reader3.GetInt32(0);
                totalAvgMs = reader3.IsDBNull(1) ? 0 : reader3.GetInt32(1);
            }
        }

        return Results.Ok(new { totalCount, totalAvgMs, daily, byType });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();
