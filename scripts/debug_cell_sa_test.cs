using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;

var docPath = @"D:\git\MiniPdf\tests\Issue_Files\docx\20260318_issue.docx";
var bytes = File.ReadAllBytes(docPath);

var asm = Assembly.LoadFrom(@"D:\git\MiniPdf\src\MiniPdf\bin\Debug\net8.0\MiniPdf.dll");
var readerType = asm.GetType("MiniPdf.DocxReader");
var readMethod = readerType.GetMethod("Read", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
var doc = readMethod!.Invoke(null, new object[] { new MemoryStream(bytes) });

var elemsProp = doc!.GetType().GetProperty("Elements");
var elems = (IList)elemsProp!.GetValue(doc)!;

int tableIdx = 0;
foreach (var elem in elems)
{
    if (elem.GetType().Name != "DocxTable") continue;
    
    var styleSA = (float)elem.GetType().GetProperty("StyleSpacingAfter")!.GetValue(elem)!;
    var rows = (IList)elem.GetType().GetProperty("Rows")!.GetValue(elem)!;
    
    Console.WriteLine($"Table {tableIdx}: StyleSpacingAfter={styleSA}");
    
    int rowIdx = 0;
    foreach (var row in rows)
    {
        var cells = (IList)row.GetType().GetProperty("Cells")!.GetValue(row)!;
        var cell = cells[0]!;
        var paras = (IList)cell.GetType().GetProperty("Paragraphs")!.GetValue(cell)!;
        if (paras.Count > 0)
        {
            var para = paras[0]!;
            var sa = (float)para.GetType().GetProperty("SpacingAfter")!.GetValue(para)!;
            var styleId = (string)para.GetType().GetProperty("StyleId")!.GetValue(para);
            Console.WriteLine($"  Row{rowIdx}: StyleId={styleId ?? "null"}, SA={sa}");
        }
        rowIdx++;
        if (rowIdx > 3) break;
    }
    tableIdx++;
    if (tableIdx > 2) break;
}
