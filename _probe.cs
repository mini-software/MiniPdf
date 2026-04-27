using System;
using System.Reflection;
using MiniPdf;

var asm = Assembly.LoadFrom(@"src/MiniPdf/bin/Debug/net8.0/MiniPdf.dll");
var t = asm.GetType("MiniPdf.PdfFont", true);
// Try TNR
Console.WriteLine("Loaded type " + t.FullName);
