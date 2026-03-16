using System;
using System.IO;

var xlsxPath = Path.GetFullPath(@"..\..\tests\Issue_Files\xlsx\classic23_unicode_text.xlsx");
var pdfPath = Path.GetFullPath(@"..\..\tests\Issue_Files\minipdf_xlsx\classic23_unicode_text.pdf");

Console.WriteLine($"Input: {xlsxPath} exists={File.Exists(xlsxPath)}");

MiniSoftware.MiniPdf.ConvertToPdf(xlsxPath, pdfPath);

var fi = new FileInfo(pdfPath);
Console.WriteLine($"Output PDF size: {fi.Length} bytes");
Console.WriteLine("Done.");
