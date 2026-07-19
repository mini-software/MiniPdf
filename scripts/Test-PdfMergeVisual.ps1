param(
    [string]$OutputDir = "artifacts/issue62-merge-visual",
    [switch]$Open
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$resolvedOutputDir = Join-Path $repoRoot $OutputDir
$runnerDir = Join-Path $resolvedOutputDir "_visual-runner"
$miniPdfProject = Join-Path $repoRoot "src/MiniPdf/MiniPdf.csproj"

New-Item -ItemType Directory -Force -Path $runnerDir | Out-Null

$escapedProjectPath = [System.Security.SecurityElement]::Escape($miniPdfProject)

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$escapedProjectPath" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path (Join-Path $runnerDir "PdfMergeVisual.csproj") -Encoding UTF8

$escapedOutputDir = $resolvedOutputDir.Replace('"', '""')

@"
using System.Globalization;
using System.Text;
using MiniSoftware;

var outputDir = @"$escapedOutputDir";
Directory.CreateDirectory(outputDir);

var first = Path.Combine(outputDir, "source-a.pdf");
var second = Path.Combine(outputDir, "source-b.pdf");
var merged = Path.Combine(outputDir, "merged-bookmarks.pdf");

File.WriteAllBytes(first, CreateSamplePdf("Source A", "This PDF contributes pages 1 and 2.", 2));
File.WriteAllBytes(second, CreateSamplePdf("Source B", "This PDF contributes pages 3 and 4.", 2));

MiniPdf.MergePdf(new[] { first, second }, merged, new PdfMergeOptions
{
    BookmarkTitles = new[] { "Source A", "Source B" },
    Bookmarks = new[] { new PdfBookmark("Source B - page 2", 3) }
});

Console.WriteLine($"Created: {merged}");
Console.WriteLine("Open the PDF and check that the bookmark pane contains Source A, Source B, and Source B - page 2.");

static byte[] CreateSamplePdf(string title, string body, int pageCount)
{
    using var stream = new MemoryStream();
    var offsets = new List<long> { 0 };

    void WriteRaw(string text)
    {
        var bytes = Encoding.Latin1.GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    void BeginObject(int objectNumber)
    {
        while (offsets.Count <= objectNumber)
            offsets.Add(0);
        offsets[objectNumber] = stream.Position;
        WriteRaw($"{objectNumber} 0 obj\n");
    }

    WriteRaw("%PDF-1.4\n%\xe2\xe3\xcf\xd3\n");

    BeginObject(1);
    WriteRaw("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

    var pageObjects = Enumerable.Range(0, pageCount).Select(i => 5 + i * 2).ToArray();
    BeginObject(2);
    WriteRaw($"<< /Type /Pages /Kids [{string.Join(" ", pageObjects.Select(n => $"{n} 0 R"))}] /Count {pageCount} >>\nendobj\n");

    BeginObject(3);
    WriteRaw("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

    for (var i = 0; i < pageCount; i++)
    {
        var contentObject = 4 + i * 2;
        var pageObject = 5 + i * 2;
        var content = $"BT /F1 18 Tf 50 730 Td ({EscapePdfString(title)}) Tj /F1 12 Tf 0 -32 Td ({EscapePdfString(body)}) Tj 0 -20 Td (Page {i + 1}) Tj ET\n";
        var contentBytes = Encoding.ASCII.GetBytes(content);

        BeginObject(contentObject);
        WriteRaw($"<< /Length {contentBytes.Length} >>\nstream\n");
        stream.Write(contentBytes, 0, contentBytes.Length);
        WriteRaw("endstream\nendobj\n");

        BeginObject(pageObject);
        WriteRaw($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObject} 0 R >>\nendobj\n");
    }

    var xrefOffset = stream.Position;
    WriteRaw("xref\n");
    WriteRaw($"0 {offsets.Count}\n");
    WriteRaw("0000000000 65535 f \n");
    for (var i = 1; i < offsets.Count; i++)
        WriteRaw($"{offsets[i].ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");

    WriteRaw("trailer\n");
    WriteRaw($"<< /Size {offsets.Count} /Root 1 0 R >>\n");
    WriteRaw("startxref\n");
    WriteRaw($"{xrefOffset}\n");
    WriteRaw("%%EOF\n");

    return stream.ToArray();
}

static string EscapePdfString(string value)
    => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
"@ | Set-Content -Path (Join-Path $runnerDir "Program.cs") -Encoding UTF8

dotnet run --project (Join-Path $runnerDir "PdfMergeVisual.csproj")

$mergedPdf = Join-Path $resolvedOutputDir "merged-bookmarks.pdf"
if ($Open)
{
    Start-Process $mergedPdf
}
