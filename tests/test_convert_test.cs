#!/usr/bin/env dotnet run
#:project ../src/MiniPdf/MiniPdf.csproj
#:property LangVersion=latest

using MiniSoftware;
using System.Diagnostics;

var inputPath = @"C:\Users\Wei\Downloads\HCT FIR-(2)YL6520AS1 HCT PO_anonymized.xlsx";
var dir = Path.GetDirectoryName(inputPath)!;
var baseName = Path.GetFileNameWithoutExtension(inputPath);

Console.WriteLine($"Input: {inputPath}");
if (!File.Exists(inputPath))
{
    Console.WriteLine("Error: file not found.");
    return;
}

// === 1. MiniPdf ===
var miniPdfOut = Path.Combine(dir, baseName + "_minipdf.pdf");
Console.WriteLine($"\n[MiniPdf] Converting...");
var sw = Stopwatch.StartNew();
try
{
    MiniPdf.ConvertToPdf(inputPath, miniPdfOut);
    sw.Stop();
    var info = new FileInfo(miniPdfOut);
    Console.WriteLine($"[MiniPdf] Done in {sw.ElapsedMilliseconds} ms, PDF size: {info.Length / 1024.0:F1} KB");
}
catch (Exception ex)
{
    sw.Stop();
    Console.WriteLine($"[MiniPdf] Error: {ex.Message}");
}

// === 2. LibreOffice ===
var libreOut = Path.Combine(dir, baseName + "_libreoffice.pdf");
Console.WriteLine($"\n[LibreOffice] Converting...");
sw = Stopwatch.StartNew();
try
{
    var soffice = @"C:\Program Files\LibreOffice\program\soffice.exe";
    if (!File.Exists(soffice)) soffice = "soffice";

    using var tmpDir = new TempDir();
    var psi = new ProcessStartInfo
    {
        FileName = soffice,
        Arguments = $"--headless --norestore \"-env:UserInstallation=file:///{tmpDir.Path.Replace('\\', '/')}\" --convert-to pdf --outdir \"{tmpDir.Path}\" \"{inputPath}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    var proc = Process.Start(psi)!;
    proc.WaitForExit(120_000);
    sw.Stop();

    // LibreOffice outputs to tmpDir with original base name
    var librePdf = Path.Combine(tmpDir.Path, baseName + ".pdf");
    if (File.Exists(librePdf))
    {
        File.Copy(librePdf, libreOut, true);
        var info = new FileInfo(libreOut);
        Console.WriteLine($"[LibreOffice] Done in {sw.ElapsedMilliseconds} ms, PDF size: {info.Length / 1024.0:F1} KB");
    }
    else
    {
        Console.WriteLine($"[LibreOffice] PDF not found. stderr: {proc.StandardError.ReadToEnd()}");
    }
}
catch (Exception ex)
{
    sw.Stop();
    Console.WriteLine($"[LibreOffice] Error: {ex.Message}");
}

// === 3. Office (Excel COM) ===
var officeOut = Path.Combine(dir, baseName + "_office.pdf");
Console.WriteLine($"\n[Office] Converting...");
sw = Stopwatch.StartNew();
try
{
    var excelType = Type.GetTypeFromProgID("Excel.Application");
    if (excelType == null) throw new Exception("Excel not installed or COM not available");
    dynamic excel = Activator.CreateInstance(excelType)!;
    excel.Visible = false;
    excel.DisplayAlerts = false;
    dynamic wb = excel.Workbooks.Open(inputPath);
    // ExportAsFixedFormat: Type=0 (PDF)
    wb.ExportAsFixedFormat(0, officeOut);
    wb.Close(false);
    excel.Quit();
    sw.Stop();
    var info = new FileInfo(officeOut);
    Console.WriteLine($"[Office] Done in {sw.ElapsedMilliseconds} ms, PDF size: {info.Length / 1024.0:F1} KB");
}
catch (Exception ex)
{
    sw.Stop();
    Console.WriteLine($"[Office] Error: {ex.Message}");
}

// === Open all PDFs ===
Console.WriteLine("\nOpening PDFs...");
foreach (var f in new[] { miniPdfOut, libreOut, officeOut })
{
    if (File.Exists(f))
    {
        Console.WriteLine($"  {Path.GetFileName(f)}");
        Process.Start(new ProcessStartInfo(f) { UseShellExecute = true });
    }
}

class TempDir : IDisposable
{
    public string Path { get; }
    public TempDir() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(Path); }
    public void Dispose() { try { Directory.Delete(Path, true); } catch { } }
}
