using System;
using System.IO;
using System.Text;
using MiniSoftware;

var xlsxPath = @"tests\MiniPdf.Scripts\output\classic23_unicode_text.xlsx";
var pdfPath = @"tests\MiniPdf.Scripts\output\classic23_test_output.pdf";
MiniExcel.SaveAs(pdfPath, xlsxPath);
var bytes = File.ReadAllBytes(pdfPath);
var text = Encoding.Latin1.GetString(bytes);
// Find content streams: look for "stream\n" ... "endstream"
var idx = 0;
while ((idx = text.IndexOf("/F2 ", idx)) >= 0)
{
    var start = Math.Max(0, idx - 20);
    var end = Math.Min(text.Length, idx + 200);
    Console.WriteLine($"--- F2 usage at offset {idx} ---");
    Console.WriteLine(text.Substring(start, end - start));
    Console.WriteLine();
    idx += 4;
}
