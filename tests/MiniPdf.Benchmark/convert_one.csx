// Quick test: convert classic128 only
#r "../../src/MiniPdf/bin/Release/net8.0/MiniPdf.dll"
using MiniPdf;

var inputFile = @"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic128_font_sizes.xlsx";
var outputFile = @"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf";

var converter = new ExcelToPdfConverter();
converter.Convert(inputFile, outputFile);
Console.WriteLine("Done: " + outputFile);
