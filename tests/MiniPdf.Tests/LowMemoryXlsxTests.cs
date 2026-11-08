using System.IO.Compression;
using System.Text;
using System.Xml;

namespace MiniSoftware.Tests;

public class LowMemoryXlsxTests
{
    public static IEnumerable<object[]> SupportedClassicFixtures()
    {
        var directory = GetCorpusDirectory();
        var numbers = Enumerable.Range(1, 60).Except([35, 36, 38])
            .Concat(Enumerable.Range(121, 7))
            .Concat(Enumerable.Range(131, 6))
            .Concat([139])
            .Concat(Enumerable.Range(143, 6))
            .Concat(Enumerable.Range(151, 18))
            .Concat(Enumerable.Range(170, 11))
            .Concat([182, 184]);
        foreach (var number in numbers)
            yield return [Directory.GetFiles(directory, $"classic{number:D2}_*.xlsx").Single()];
    }

    [ClassicCorpusTheory]
    [MemberData(nameof(SupportedClassicFixtures))]
    public void LowMemory_RealClassicFixture_ProducesIdenticalPdf(string path)
    {
        using var input = File.OpenRead(path);
        var expected = MiniPdf.ConvertToPdf(input);
        input.Position = 0;
        Assert.Equal(expected, MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    private static string GetCorpusDirectory()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "MiniPdf.sln"))) root = root.Parent;
        if (root == null) throw new DirectoryNotFoundException("MiniPdf repository root was not found.");
        return Path.Combine(root.FullName, "tests", "MiniPdf.Scripts", "output");
    }

    public sealed class ClassicCorpusTheoryAttribute : TheoryAttribute
    {
        public ClassicCorpusTheoryAttribute()
        {
            if (!File.Exists(Path.Combine(GetCorpusDirectory(), "classic01_basic_table_with_headers.xlsx")))
                Skip = "Generate the classic XLSX corpus with tests/MiniPdf.Scripts/generate_classic_xlsx.py.";
        }
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(200, false, false)]
    [InlineData(1500, true, false)]
    [InlineData(80, false, true)]
    [InlineData(80, true, true)]
    public void LowMemory_ProducesByteIdenticalPdf(int rows, bool compress, bool wide)
    {
        using var input = CreateWorkbook(rows, wide);
        var expected = MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { Compress = compress });
        input.Position = 0;
        var actual = MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true, Compress = compress });
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LowMemory_NonSeekableStreams_ProduceIdenticalPdfAndStayOpen()
    {
        using var workbook = CreateWorkbook(100);
        var expected = MiniPdf.ConvertToPdf(workbook);
        workbook.Position = 0;
        using var input = new NonSeekableStream(workbook);
        using var destination = new MemoryStream();
        using var output = new NonSeekableStream(destination);
        MiniPdf.ConvertToPdf(input, output, new MiniPdfConversionOptions { LowMemory = true });
        Assert.Equal(expected, destination.ToArray());
        Assert.True(input.CanRead);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void LowMemory_FileOverloads_ProduceIdenticalPdf()
    {
        var inputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
        var outputPath = Path.ChangeExtension(inputPath, ".pdf");
        try
        {
            using (var workbook = CreateWorkbook(150))
            using (var file = File.Create(inputPath))
                workbook.CopyTo(file);
            var expected = MiniPdf.ConvertToPdf(inputPath);
            Assert.Equal(expected, MiniPdf.ConvertToPdf(inputPath, new MiniPdfConversionOptions { LowMemory = true }));
            MiniPdf.ConvertToPdf(inputPath, outputPath, new MiniPdfConversionOptions { LowMemory = true });
            Assert.Equal(expected, File.ReadAllBytes(outputPath));
            ExcelToPdfConverter.ConvertToFile(inputPath, outputPath,
                new ExcelToPdfConverter.ConversionOptions { LowMemory = true });
            Assert.Equal(expected, File.ReadAllBytes(outputPath));
        }
        finally
        {
            File.Delete(inputPath);
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void LowMemory_LayoutOverridesAndCulture_PreserveOutput()
    {
        using var input = CreateWorkbook(200);
        var options = new MiniPdfConversionOptions
        {
            Landscape = true,
            FitToWidth = 1,
            FitToHeight = 0,
            PrintScale = 80,
            Culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR"),
            Sheets = ["Data"],
        };
        var expected = MiniPdf.ConvertToPdf(input, options);
        input.Position = 0;
        options.LowMemory = true;
        Assert.Equal(expected, MiniPdf.ConvertToPdf(input, options));
    }

    [Theory]
    [InlineData("<mergeCells><mergeCell ref=\"A1:B1\"/></mergeCells>", "mergeCells")]
    [InlineData("<conditionalFormatting sqref=\"A1\"/>", "conditionalFormatting")]
    [InlineData("<drawing/>", "drawing")]
    [InlineData("<legacyDrawing/>", "legacyDrawing")]
    [InlineData("<tableParts/>", "tableParts")]
    [InlineData("<hyperlinks/>", "hyperlinks")]
    [InlineData("<headerFooter/>", "headerFooter")]
    [InlineData("<printOptions verticalCentered=\"1\"/>", "vertical centering")]
    [InlineData("<extLst/>", "extLst")]
    public void LowMemory_UnsupportedFeatures_ThrowBeforeWritingOutput(string feature, string message)
    {
        using var input = CreateWorkbook(1, extraWorksheetXml: feature);
        using var output = new MemoryStream();
        var exception = Assert.Throws<NotSupportedException>(() =>
            MiniPdf.ConvertToPdf(input, output, new MiniPdfConversionOptions { LowMemory = true }));
        Assert.Contains(message, exception.Message);
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData("<row r=\"1\" ht=\"20\"><c r=\"A1\"><v>1</v></c></row>", "row heights")]
    [InlineData("<row r=\"1\" hidden=\"true\"><c r=\"A1\"><v>1</v></c></row>", "row heights")]
    [InlineData("<row r=\"1\" customHeight=\"1\"><c r=\"A1\"><v>1</v></c></row>", "row heights")]
    [InlineData("<row r=\"1\"><c r=\"A1\"><f>1+1</f><v>2</v></c></row>", "formulas")]
    public void LowMemory_UnsupportedRows_Throw(string row, string message)
    {
        using var input = CreateWorkbook(0, customRows: row);
        var exception = Assert.Throws<NotSupportedException>(() =>
            MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
        Assert.Contains(message, exception.Message);
    }

    [Theory]
    [InlineData("<row r=\"2\"/><row r=\"1\"/>")]
    [InlineData("<row r=\"1048577\"/>")]
    [InlineData("<row r=\"1\"><c r=\"XFE1\"><v>1</v></c></row>")]
    public void LowMemory_InvalidRowOrderOrLimit_Throws(string rows)
    {
        using var input = CreateWorkbook(0, customRows: rows);
        Assert.Throws<InvalidDataException>(() =>
            MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    [Fact]
    public void LowMemory_SparseRowsColumnsAndTrailingEmptyRows_PreserveOutput()
    {
        using var input = CreateWorkbook(0, customRows:
            """
            <row r="2"><c r="A2" t="inlineStr"><is><t>First</t></is></c></row>
            <row r="55"><c r="D55" t="inlineStr"><is><t>Last</t></is></c></row>
            <row r="100"/>
            """);
        var expected = MiniPdf.ConvertToPdf(input);
        input.Position = 0;
        Assert.Equal(expected, MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    [Fact]
    public void LowMemory_UnsupportedOptions_Throw()
    {
        var options = new[]
        {
            new MiniPdfConversionOptions { LowMemory = true, MaxRows = 10 },
            new MiniPdfConversionOptions { LowMemory = true, MaxColumns = 2 },
            new MiniPdfConversionOptions { LowMemory = true, FitToHeight = 1 },
            new MiniPdfConversionOptions { LowMemory = true, RowsPerPage = 40 },
        };
        foreach (var option in options)
        {
            using var input = CreateWorkbook(1);
            Assert.Throws<NotSupportedException>(() => MiniPdf.ConvertToPdf(input, option));
        }
    }

    [Theory]
    [InlineData("_xlnm.Print_Area")]
    [InlineData("_xlnm.Print_Titles")]
    public void LowMemory_PrintDefinitions_Throw(string name)
    {
        using var input = CreateWorkbook(1);
        using (var archive = new ZipArchive(input, ZipArchiveMode.Update, leaveOpen: true))
        {
            archive.GetEntry("xl/workbook.xml")!.Delete();
            WriteEntry(archive, "xl/workbook.xml",
                $"""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheets><sheet name="Data" sheetId="1"/></sheets><definedNames><definedName name="{name}">Data!$A$1:$B$2</definedName></definedNames></workbook>""");
        }
        input.Position = 0;
        Assert.Throws<NotSupportedException>(() =>
            MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    [Fact]
    public void LowMemory_WorksheetFitToHeight_Throws()
    {
        using var input = CreateWorkbook(1, fitToHeight: 1,
            extraWorksheetXml: "<sheetPr><pageSetUpPr fitToPage=\"1\"/></sheetPr>");
        Assert.Throws<NotSupportedException>(() =>
            MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    [Fact]
    public void LowMemory_NonXlsxInput_Throws()
    {
        using var input = new MemoryStream();
        using (var zip = new ZipArchive(input, ZipArchiveMode.Create, leaveOpen: true))
            WriteEntry(zip, "word/document.xml", "<document/>");
        input.Position = 0;
        Assert.Throws<NotSupportedException>(() =>
            MiniPdf.ConvertToPdf(input, new MiniPdfConversionOptions { LowMemory = true }));
    }

    [Fact]
    public void FileBackedPages_RoundTripAndPersistChanges()
    {
        using var memory = new PdfDocument();
        using var disk = new PdfDocument(lowMemory: true);
        foreach (var document in new[] { memory, disk })
        {
            for (var i = 0; i < 4; i++)
            {
                var page = document.AddPage();
                page.AddText("Text " + i, 20, 50, clipRect: (1, 2, 3, 4), maxWidth: 80,
                    bold: true, italic: true, underline: true, strikethrough: true, hidden: i == 3,
                    charSpacing: 0.5f, wordSpacing: 1.5f, underlineWidth: 23);
                page.AddRectangle(1, 2, 3, 4, PdfColor.Red, 0.5f);
                page.AddLine(10, 20, 30, 40, PdfColor.Blue, 2, [2, 3]);
            }
            document.Pages[0].AddText("Added later", 10, 20);
        }
        Assert.Equal(memory.ToArray(), disk.ToArray());
    }

    [Fact]
    public void TemporaryStorage_IsDeletedOnDispose()
    {
        string path;
        using (var stream = FileBackedList<string>.CreateTemporaryStream())
        {
            path = stream.Name;
            Assert.True(File.Exists(path));
        }
        Assert.False(File.Exists(path));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LowMemoryWorkbook_DeletesOwnedFilesOnSuccessAndFailure(bool fail)
    {
        var paths = new List<string>();
        using (var input = CreateWorkbook(10, extraWorksheetXml: fail ? "<drawing/>" : ""))
        using (var storage = new LowMemoryWorkbook())
        {
            paths.Add(storage.Package.Name);
            paths.Add(storage.SharedStrings.FilePath);
            if (fail) Assert.Throws<NotSupportedException>(() => storage.Prepare(input));
            else
            {
                storage.Prepare(input);
                var sheets = ExcelReader.ReadSheets(storage.Package, lowMemory: storage);
                paths.Add(Assert.IsType<FileBackedList<List<ExcelCell>>>(Assert.Single(sheets).Rows).FilePath);
            }
            paths.Add(Assert.IsType<FileBackedList<string>>(storage.GetRows("xl/worksheets/sheet1.xml")).FilePath);
            Assert.All(paths, path => Assert.True(File.Exists(path)));
        }
        Assert.All(paths, path => Assert.False(File.Exists(path)));
    }

    [Fact]
    public void LowMemory_OutputFailure_Propagates()
    {
        using var input = CreateWorkbook(100);
        using var output = new FailingOutput();
        var exception = Assert.Throws<IOException>(() =>
            MiniPdf.ConvertToPdf(input, output, new MiniPdfConversionOptions { LowMemory = true }));
        Assert.Equal("Output failure", exception.Message);
    }

    internal static MemoryStream CreateWorkbook(int rows, bool wide = false, string extraWorksheetXml = "",
        string? customRows = null, int fitToHeight = 0)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var result = new MemoryStream();
        using (var zip = new ZipArchive(result, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "xl/workbook.xml",
                $"""<workbook xmlns="{ns}"><sheets><sheet name="Data" sheetId="1"/></sheets></workbook>""");
            WriteEntry(zip, "xl/sharedStrings.xml",
                $"""<sst xmlns="{ns}"><si><r><rPr><b/></rPr><t>Header</t></r><r><t> value</t></r></si><si><t>&#x4E2D;&#x6587;</t></si></sst>""");
            WriteEntry(zip, "xl/styles.xml",
                $"""
                <styleSheet xmlns="{ns}">
                  <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><i/><u/><strike/><sz val="12"/><color rgb="FF008800"/><name val="Calibri"/></font></fonts>
                  <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FFFFEEAA"/></patternFill></fill></fills>
                  <borders count="2"><border/><border><left style="dashed"><color rgb="FF0000FF"/></left><right style="thin"/><top style="thin"/><bottom style="thin"/></border></borders>
                  <cellXfs count="2"><xf fontId="0"/><xf fontId="1" fillId="1" borderId="1" numFmtId="4"><alignment wrapText="1" horizontal="center" vertical="center"/></xf></cellXfs>
                </styleSheet>
                """);
            var entry = zip.CreateEntry("xl/worksheets/sheet1.xml");
            using var stream = entry.Open();
            using var writer = XmlWriter.Create(stream);
            writer.WriteStartElement("worksheet", ns);
            writer.WriteRaw($"<sheetFormatPr defaultRowHeight=\"15\" defaultColWidth=\"10\"/><cols><col min=\"1\" max=\"4\" width=\"{(wide ? 35 : 12)}\" customWidth=\"1\"/></cols>");
            writer.WriteStartElement("sheetData", ns);
            if (customRows != null) writer.WriteRaw(customRows);
            else
            {
                for (var i = 1; i <= rows; i++)
                {
                    writer.WriteStartElement("row", ns);
                    writer.WriteAttributeString("r", i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteRaw($"<c r=\"A{i}\" t=\"s\"><v>0</v></c><c r=\"B{i}\" t=\"s\"><v>1</v></c><c r=\"C{i}\" s=\"1\"><v>{i}.25</v></c>");
                    writer.WriteStartElement("c", ns);
                    writer.WriteAttributeString("r", $"D{i}");
                    writer.WriteAttributeString("t", "inlineStr");
                    writer.WriteStartElement("is", ns);
                    writer.WriteElementString("t", ns, $"Row {i} & value");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteRaw($"<pageSetup paperSize=\"9\" fitToHeight=\"{fitToHeight}\"/>" + extraWorksheetXml);
            writer.WriteEndElement();
        }
        result.Position = 0;
        return result;
    }

    private static void WriteEntry(ZipArchive zip, string path, string text)
    {
        using var writer = new StreamWriter(zip.CreateEntry(path).Open(), Encoding.UTF8);
        writer.Write(text);
    }

    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    private sealed class FailingOutput : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override void Write(byte[] buffer, int offset, int count) => throw new IOException("Output failure");
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
