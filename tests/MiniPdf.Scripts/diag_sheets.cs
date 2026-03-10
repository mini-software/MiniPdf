#:project ../../src/MiniPdf/MiniPdf.csproj
using MiniSoftware;

// Quick diagnostic: check sheet properties
var path = Path.GetFullPath(@"..\..\tests\Issue_Files\xlsx\payroll-calculator_f.xlsx");
Console.WriteLine($"File: {path}");

using var stream = File.OpenRead(path);
// Use reflection to call internal ReadSheets
var readerType = typeof(MiniPdf).Assembly.GetType("MiniSoftware.ExcelReader");
var method = readerType!.GetMethod("ReadSheets", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
var sheets = (System.Collections.IList)method!.Invoke(null, new object[] { stream })!;

Console.WriteLine($"Sheets: {sheets.Count}");
var sheetType = typeof(MiniPdf).Assembly.GetType("MiniSoftware.ExcelSheet");
var nameProp = sheetType!.GetProperty("Name");
var landscapeProp = sheetType.GetProperty("IsLandscape");
var scaleProp = sheetType.GetProperty("PrintScale");
var paperProp = sheetType.GetProperty("PaperSize");
var printAreaProp = sheetType.GetProperty("PrintArea");

foreach (var sheet in sheets)
{
    var name = nameProp!.GetValue(sheet);
    var landscape = landscapeProp!.GetValue(sheet);
    var scale = scaleProp!.GetValue(sheet);
    var paper = paperProp?.GetValue(sheet) ?? "N/A";
    var printArea = printAreaProp?.GetValue(sheet) ?? "null";
    Console.WriteLine($"  {name}: landscape={landscape}, scale={scale}, paper={paper}, printArea={printArea}");
}
