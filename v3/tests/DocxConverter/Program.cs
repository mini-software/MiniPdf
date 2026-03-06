using MiniSoftware;

var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var docxDir = Path.Combine(baseDir, "tests", "MiniPdf.Scripts", "output_docx");
var pdfDir = Path.Combine(baseDir, "tests", "MiniPdf.Scripts", "pdf_output_docx");

if (args.Length >= 2)
{
    docxDir = Path.GetFullPath(args[0]);
    pdfDir = Path.GetFullPath(args[1]);
}

Directory.CreateDirectory(pdfDir);

var files = Directory.GetFiles(docxDir, "*.docx").OrderBy(f => f).ToArray();
Console.WriteLine($"Converting {files.Length} .docx files (v3)...");

int passed = 0, failed = 0;
foreach (var docx in files)
{
    var name = Path.GetFileNameWithoutExtension(docx);
    var pdf = Path.Combine(pdfDir, name + ".pdf");
    try
    {
        MiniPdf.ConvertToPdf(docx, pdf);
        var sz = new FileInfo(pdf).Length;
        Console.WriteLine($"  OK  {name}.pdf ({sz / 1024.0:F1} KB)");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERR {name}: {ex.Message}");
        failed++;
    }
}
Console.WriteLine($"\nDone! Passed: {passed}, Failed: {failed}, Total: {files.Length}");
return failed > 0 ? 1 : 0;
