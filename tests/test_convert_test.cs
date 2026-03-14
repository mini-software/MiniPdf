#!/usr/bin/env dotnet run
#:project ../src/MiniPdf/MiniPdf.csproj
#:property LangVersion=latest

using MiniSoftware;

var docxPath = @"C:\Users\Wei\Downloads\SA8000 ch sample.docx";
var pdfPath = Path.ChangeExtension(docxPath, ".pdf");

Console.WriteLine($"Input:  {docxPath}");
Console.WriteLine($"Output: {pdfPath}");

if (!File.Exists(docxPath))
{
    Console.WriteLine("Error: DOCX file not found.");
    return;
}

var sw = System.Diagnostics.Stopwatch.StartNew();
MiniPdf.ConvertToPdf(docxPath, pdfPath);
sw.Stop();

var info = new FileInfo(pdfPath);
Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms, PDF size: {info.Length / 1024.0:F1} KB");
Console.WriteLine($"Opening PDF...");
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
