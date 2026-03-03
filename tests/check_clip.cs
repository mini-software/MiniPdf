using System;
using System.IO;
using System.Text;
using MiniSoftware;

// Simulate Classic44 test
var ms = new MemoryStream();
using (var pkg = new System.IO.Packaging.Package(ms, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
{
    // This won't work without XlsxBuilder
}

// Just print what the default col width formula gives
var charUnits = 8.43f;
var pts = ExcelSheet.CharUnitsToPoints(charUnits);
var avgCharWidth = 5.0f; // 10pt * 0.5
var maxChars = (int)(pts / avgCharWidth);
Console.WriteLine($"CharUnitsToPoints({charUnits}) = {pts}pt");
Console.WriteLine($"maxChars = {maxChars}");
Console.WriteLine($"'Engineering'[..{maxChars}] = '{("Engineering".Length > maxChars ? "Engineering"[..maxChars] : "Engineering")}'");
Console.WriteLine($"'alice@example.com'[..{maxChars}] = '{("alice@example.com".Length > maxChars ? "alice@example.com"[..maxChars] : "alice@example.com")}'");
Console.WriteLine($"'StrongAgree'[..{maxChars}] = '{("StrongAgree".Length > maxChars ? "StrongAgree"[..maxChars] : "StrongAgree")}'");
Console.WriteLine($"'Tall Header'[..{maxChars}] = '{("Tall Header".Length > maxChars ? "Tall Header"[..maxChars] : "Tall Header")}'");
Console.WriteLine($"'Basic Widget'[..{maxChars}] = '{("Basic Widget".Length > maxChars ? "Basic Widget"[..maxChars] : "Basic Widget")}'");
Console.WriteLine($"'Electronics'[..{maxChars}] = '{("Electronics".Length > maxChars ? "Electronics"[..maxChars] : "Electronics")}'");
Console.WriteLine($"'Consulting Services'[..{maxChars}] = '{("Consulting Services".Length > maxChars ? "Consulting Services"[..maxChars] : "Consulting Services")}'");
Console.WriteLine($"'Product 10'[..{maxChars}] = '{("Product 10".Length > maxChars ? "Product 10"[..maxChars] : "Product 10")}'");
Console.WriteLine($"'2025-01-15': {("2025-01-15".Length > maxChars ? "2025-01-15"[..maxChars] : "2025-01-15")}");
