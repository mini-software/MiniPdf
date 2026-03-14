#!/usr/bin/env dotnet run
#:project ../src/MiniPdf/MiniPdf.csproj
#:property LangVersion=latest

using MiniSoftware;

var docxPath = @"C:\Users\Wei\Downloads\SA8000 ch sample.docx";
var pdfPath = Path.Combine(Path.GetDirectoryName(docxPath)!, "SA8000_test_registerfont.pdf");
var fontPath = @"d:\git\MiniPdf\MiniPdf.Web\MiniPdf.Web.Client\wwwroot\fonts\NotoSansSC-Regular.ttf";

Console.WriteLine($"Input:  {docxPath}");
Console.WriteLine($"Output: {pdfPath}");
Console.WriteLine($"Font:   {fontPath}");

if (!File.Exists(docxPath)) { Console.WriteLine("Error: DOCX not found."); return; }
if (!File.Exists(fontPath)) { Console.WriteLine("Error: Font not found."); return; }

// Simulate WASM: register font from bytes (not system path)
Console.WriteLine("Registering font...");
var fontBytes = File.ReadAllBytes(fontPath);
MiniPdf.RegisterFont("NotoSansSC", fontBytes);
Console.WriteLine($"Font registered ({fontBytes.Length / 1024} KB)");

var sw = System.Diagnostics.Stopwatch.StartNew();
MiniPdf.ConvertToPdf(docxPath, pdfPath);
sw.Stop();

var info = new FileInfo(pdfPath);
Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms, PDF size: {info.Length / 1024.0:F1} KB");
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
