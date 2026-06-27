#:project ../../src/MiniPdf/MiniPdf.csproj

using Mp = MiniSoftware.MiniPdf;

var baseDir = Directory.GetCurrentDirectory();

var pptxDir = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(baseDir, "output_pptx");

var pdfDir = args.Length > 1
    ? Path.GetFullPath(args[1])
    : Path.Combine(baseDir, "pdf_output_pptx");

var filterPattern = args.Length > 2 ? args[2] : null;

Directory.CreateDirectory(pdfDir);

var pptxFiles = Directory.GetFiles(pptxDir, "*.pptx")
                         .Where(f => !Path.GetFileName(f).StartsWith("~$", StringComparison.Ordinal))
                         .OrderBy(f => f)
                         .ToArray();

if (!string.IsNullOrEmpty(filterPattern))
    pptxFiles = pptxFiles.Where(f => Path.GetFileNameWithoutExtension(f)
        .Contains(filterPattern, StringComparison.OrdinalIgnoreCase)).ToArray();

if (pptxFiles.Length == 0)
{
    Console.WriteLine($"No .pptx files found in: {pptxDir}");
    return 1;
}

Console.WriteLine($"Converting {pptxFiles.Length} .pptx files to PDF...");
Console.WriteLine($"  Input : {pptxDir}");
Console.WriteLine($"  Output: {pdfDir}");
Console.WriteLine();

var passed = 0;
var failed = 0;

foreach (var pptxPath in pptxFiles)
{
    var name = Path.GetFileNameWithoutExtension(pptxPath);
    var pdfPath = Path.Combine(pdfDir, name + ".pdf");

    try
    {
        Mp.ConvertToPdf(pptxPath, pdfPath);
        var pdfSize = new FileInfo(pdfPath).Length;
        Console.WriteLine($"  OK  {name}.pdf ({pdfSize / 1024.0:F1} KB)");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERR {name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine();
Console.WriteLine($"Done! Passed: {passed}, Failed: {failed}, Total: {pptxFiles.Length}");

return failed > 0 ? 1 : 0;