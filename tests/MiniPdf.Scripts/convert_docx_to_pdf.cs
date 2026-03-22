// force-rebuild: 2026-03-19T12:00:00+08:00
#:project ../../src/MiniPdf/MiniPdf.csproj

using Mp = MiniSoftware.MiniPdf;

// Resolve directories relative to CWD (when run via `dotnet run`)
var baseDir = Directory.GetCurrentDirectory();

var docxDir = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(baseDir, "output_docx");

var pdfDir = args.Length > 1
    ? Path.GetFullPath(args[1])
    : Path.Combine(baseDir, "pdf_output_docx");

var filterPattern = args.Length > 2 ? args[2] : null;

Directory.CreateDirectory(pdfDir);

var docxFiles = Directory.GetFiles(docxDir, "*.docx")
                         .OrderBy(f => f)
                         .ToArray();

if (!string.IsNullOrEmpty(filterPattern))
    docxFiles = docxFiles.Where(f => Path.GetFileNameWithoutExtension(f)
        .Contains(filterPattern, StringComparison.OrdinalIgnoreCase)).ToArray();

if (docxFiles.Length == 0)
{
    Console.WriteLine($"No .docx files found in: {docxDir}");
    return 1;
}

Console.WriteLine($"Converting {docxFiles.Length} .docx files to PDF... (v2)");
Console.WriteLine($"  Input : {docxDir}");
Console.WriteLine($"  Output: {pdfDir}");
Console.WriteLine();

var passed = 0;
var failed = 0;

foreach (var docxPath in docxFiles)
{
    var name = Path.GetFileNameWithoutExtension(docxPath);
    var pdfPath = Path.Combine(pdfDir, name + ".pdf");

    try
    {
        Mp.ConvertToPdf(docxPath, pdfPath);
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
Console.WriteLine($"Done! Passed: {passed}, Failed: {failed}, Total: {docxFiles.Length}");

return failed > 0 ? 1 : 0;

