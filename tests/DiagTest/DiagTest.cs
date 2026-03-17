// Quick check: convert the invoice XLSX and compare output size
using MiniSoftware;

var inputPath = @"tests\Issue_Files\xlsx\Simple invoice1.xlsx";
var pdfBytes = MiniPdf.ConvertToPdf(inputPath);
Console.WriteLine($"Output PDF size: {pdfBytes.Length} bytes");

// Check PDF content for border line operators (l = lineTo)
var pdfText = System.Text.Encoding.Latin1.GetString(pdfBytes);
var lineCount = 0;
var idx = 0;
while ((idx = pdfText.IndexOf(" l\n", idx)) >= 0) { lineCount++; idx++; }
Console.WriteLine($"Line operators (l): {lineCount}");
