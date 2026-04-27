using System;
using System.IO;
using MiniPdf;
class P {
    static void Main() {
        var doc = new PdfDocument();
        var page = doc.AddPage(612,792);
        var s = "Slifka M.K., Whitton J.L. (2000), Clinical implications of dysregulated cytokine production, J Mol Med,";
        // Use AddText to measure indirectly: we can't easily. Instead just emit a test PDF.
        page.AddText(s, 50, 700, 12, "#000000", preferredFontName: "Times New Roman");
        File.WriteAllBytes("_measure_test.pdf", doc.GetBytes());
        Console.WriteLine("done");
    }
}
