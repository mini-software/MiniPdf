using System;
using System.IO;
using System.Text;

var xlsxPath = Path.GetFullPath(@"..\..\tests\MiniPdf.Scripts\output\classic23_unicode_text.xlsx");
var pdfPath = Path.GetFullPath(@"classic23_diag.pdf");

Console.WriteLine($"Input: {xlsxPath} exists={File.Exists(xlsxPath)}");

MiniSoftware.MiniPdf.ConvertToPdf(xlsxPath, pdfPath);

var bytes = File.ReadAllBytes(pdfPath);
Console.WriteLine($"Output PDF size: {bytes.Length} bytes");

// Search for /F2 font references in content streams
var text = Encoding.Latin1.GetString(bytes);

// Check if F2 font is defined
if (text.Contains("/F2 "))
    Console.WriteLine("F2 font IS referenced in PDF");
else
    Console.WriteLine("F2 font is NOT referenced - Unicode font missing!");

// Check for hex-encoded CJK (你 = 4F60, 好 = 597D)
if (text.Contains("4F60"))
    Console.WriteLine("CJK hex '4F60' (你) found in PDF");
else
    Console.WriteLine("CJK hex '4F60' (你) NOT found in PDF");

// Check for FontFile2 (embedded font)
if (text.Contains("/FontFile2"))
    Console.WriteLine("Font IS embedded (FontFile2 present)");
else
    Console.WriteLine("Font is NOT embedded (FontFile2 missing)");

// Check for CIDToGIDMap
if (text.Contains("/CIDToGIDMap"))
    Console.WriteLine("CIDToGIDMap IS present");
else
    Console.WriteLine("CIDToGIDMap is NOT present");

// Check for /W array
if (text.Contains("/W ["))
    Console.WriteLine("/W widths array IS present");
else
    Console.WriteLine("/W widths array is NOT present");

// Print all content stream text near F2 references
var idx = 0;
var count = 0;
while ((idx = text.IndexOf("/F2 ", idx)) >= 0 && count < 5)
{
    var start = Math.Max(0, idx - 50);
    var end = Math.Min(text.Length, idx + 300);
    var snippet = text.Substring(start, end - start);
    // Only show printable ASCII portion
    var sb = new StringBuilder();
    foreach (var c in snippet)
        sb.Append(c >= 32 && c < 127 ? c : '.');
    Console.WriteLine($"\n--- F2 usage {count + 1} at offset {idx} ---");
    Console.WriteLine(sb.ToString());
    idx += 4;
    count++;
}

// Dump first 5000 chars (font object area) as printable ASCII
Console.WriteLine("\n=== PDF header objects ===");
var headerSnip = text.Substring(0, Math.Min(5000, text.Length));
var hsb = new StringBuilder();
foreach (var c in headerSnip)
    hsb.Append(c >= 32 && c < 127 ? c : '.');
Console.WriteLine(hsb.ToString());

// Check for emoji (U+1F600 = D83D DE00 in surrogates)
if (text.Contains("D83D"))
    Console.WriteLine("\nSurrogate D83D found (emoji as surrogates)");
if (text.Contains("1F600"))
    Console.WriteLine("U+1F600 found (emoji as code point)");

Console.WriteLine($"\nDone. PDF saved to: {pdfPath}");
