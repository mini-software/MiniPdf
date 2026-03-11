#:project ../../src/MiniPdf/MiniPdf.csproj

using Mp = MiniSoftware.MiniPdf;

// Test: convert a single cell to see date format
var testXlsx = @"D:\git\MiniPdf\tests\Issue_Files\xlsx\payroll-calculator_f.xlsx";
var testPdf = @"D:\git\MiniPdf\tests\Issue_Files\minipdf_xlsx\test_output.pdf";
Mp.ConvertToPdf(testXlsx, testPdf);
Console.WriteLine("Done. Size: " + new System.IO.FileInfo(testPdf).Length / 1024 + "KB");
