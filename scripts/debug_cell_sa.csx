// Quick test to check cell paragraph SA values after table style override
using System;
using System.IO;
using System.Reflection;

// Add MiniPdf reference
var docPath = @"D:\git\MiniPdf\tests\Issue_Files\docx\20260318_issue.docx";
var bytes = File.ReadAllBytes(docPath);

// Use reflection to call DocxReader.Read
var asm = Assembly.LoadFrom(@"D:\git\MiniPdf\src\MiniPdf\bin\Debug\net8.0\MiniPdf.dll");
var readerType = asm.GetType("MiniPdf.DocxReader");
var readMethod = readerType.GetMethod("Read", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
var doc = readMethod.Invoke(null, new object[] { bytes });

// Get elements
var elemsProp = doc.GetType().GetProperty("Elements");
var elems = (System.Collections.IList)elemsProp.GetValue(doc);

int tableIdx = 0;
foreach (var elem in elems)
{
    if (elem.GetType().Name == "DocxTable")
    {
        var rowsProp = elem.GetType().GetProperty("Rows");
        var rows = (System.Collections.IList)rowsProp.GetValue(elem);
        
        var styleSpacingAfterProp = elem.GetType().GetProperty("StyleSpacingAfter");
        var styleSA = (float)styleSpacingAfterProp.GetValue(elem);
        
        Console.WriteLine($"Table {tableIdx}: StyleSpacingAfter={styleSA}");
        
        int rowIdx = 0;
        foreach (var row in rows)
        {
            var cellsProp = row.GetType().GetProperty("Cells");
            var cells = (System.Collections.IList)cellsProp.GetValue(row);
            
            foreach (var cell in cells)
            {
                var parasProp = cell.GetType().GetProperty("Paragraphs");
                var paras = (System.Collections.IList)parasProp.GetValue(cell);
                
                foreach (var para in paras)
                {
                    var saProp = para.GetType().GetProperty("SpacingAfter");
                    var sa = (float)saProp.GetValue(para);
                    var styleIdProp = para.GetType().GetProperty("StyleId");
                    var styleId = (string)styleIdProp.GetValue(para);
                    
                    Console.WriteLine($"  Row{rowIdx}: StyleId={styleId ?? "null"}, SA={sa}");
                    break; // just first paragraph per cell
                }
                break; // just first cell per row
            }
            rowIdx++;
            if (rowIdx > 3) break;
        }
        tableIdx++;
        if (tableIdx > 2) break;
    }
}
