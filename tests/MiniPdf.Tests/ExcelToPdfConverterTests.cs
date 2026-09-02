using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

public class ExcelToPdfConverterTests
{
    [Fact]
    public void Convert_SimpleExcel_ProducesValidPdf()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Name", "Age", "City" },
            new[] { "Alice", "30", "New York" },
            new[] { "Bob", "25", "London" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("Name", content);
        Assert.Contains("Alice", content);
        Assert.Contains("Bob", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void Convert_WorksheetImage_RendersAboveCellText()
    {
        using var excelStream = CreateExcelWithImageOverCell();

        var doc = ExcelToPdfConverter.Convert(excelStream);

        var image = Assert.Single(doc.Pages.SelectMany(page => page.ImageBlocks));
        var overlappedText = Assert.Single(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Covered");
        Assert.True(image.RenderAboveText);
        Assert.True(overlappedText.X < image.X + image.RenderWidth,
            "Default column geometry should keep D1 beneath the A1 drawing edge.");
    }

    [Fact]
    public void Convert_WithOptions_UsesCustomSettings()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Header1", "Header2" },
            new[] { "Value1", "Value2" },
        });

        var options = new ExcelToPdfConverter.ConversionOptions
        {
            FontSize = 14,
            MarginLeft = 72,
            PageWidth = 595, // A4
            PageHeight = 842, // A4
            IncludeSheetName = false,
        };

        var doc = ExcelToPdfConverter.Convert(excelStream, options);
        Assert.True(doc.Pages.Count >= 1);
        var bytes = doc.ToArray();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Convert_ProgrammaticColumnWidthsWithoutCustomWidth_UsesWidths()
    {
        using var excelStream = CreateSimpleExcel(
            new[]
            {
                new[] { "A", "B", "C", "D" },
                new[] { "1", "2", "3", "4" },
            },
            columnWidths: new[] { 24f, 24f, 24f, 24f },
            customWidth: false);

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.True(doc.Pages.Count >= 2, $"Expected wide generated columns to split across pages, got {doc.Pages.Count}");
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == "D");
    }

    [Fact]
    public void Convert_GeneratedWideTextTableWithoutColumnWidths_InfersReadableColumnGroups()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var content = Encoding.ASCII.GetString(doc.ToArray());

        Assert.True(doc.Pages.Count >= 2, $"Expected inferred wide columns to create column groups, got {doc.Pages.Count}");
        Assert.Contains("1234 North Evergreen Avenue", content);
        Assert.Contains("QA Automation Specialist", content);
    }

    [Fact]
    public void Convert_LargeGeneratedWideTable_UsesReadableA4ColumnGroups()
    {
        var rows = new List<string[]>
        {
            Enumerable.Range(1, 10).Select(index => $"Header{index}").ToArray(),
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row =>
            Enumerable.Range(1, 10).Select(column => $"Long generated value {row}-{column}").ToArray()));
        using var excelStream = CreateSimpleExcel(rows.ToArray());

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var firstPageText = doc.Pages[0].TextBlocks.Select(block => block.Text).ToHashSet();
        var headerY = doc.Pages[0].TextBlocks.First(block => block.Text == "Header1").Y;
        var firstRowY = doc.Pages[0].TextBlocks.Where(block => block.Y < headerY).Max(block => block.Y);

        Assert.Equal(595.2756f, doc.Pages[0].Width, 3);
        Assert.Equal(841.8898f, doc.Pages[0].Height, 3);
        Assert.Equal(14.52f, headerY - firstRowY, 3);
        for (var column = 1; column <= 4; column++)
            Assert.Contains($"Header{column}", firstPageText);
        Assert.DoesNotContain("Header5", firstPageText);
        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Header10");
    }

    [Fact]
    public void Convert_FitAttributesWithoutFitFlag_DoesNotCompressAllRowsToOnePageHigh()
    {
        var rows = new List<string[]>
        {
            Enumerable.Range(1, 8).Select(index => $"Header{index}").ToArray(),
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row =>
            Enumerable.Range(1, 8).Select(column => $"Value {row}-{column}").ToArray()));
        using var excelStream = CreateSimpleExcel(rows.ToArray(), includeUnflaggedFitAttributes: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var minimumFontSize = doc.Pages.SelectMany(page => page.TextBlocks).Min(block => block.FontSize);
        var firstPageText = doc.Pages[0].TextBlocks.Select(block => block.Text).ToHashSet();

        Assert.True(doc.Pages.Count > 1, $"Expected rows to paginate naturally, got {doc.Pages.Count} page.");
        Assert.Equal(595.2756f, doc.Pages[0].Width, 3);
        Assert.Equal(841.8898f, doc.Pages[0].Height, 3);
        Assert.True(minimumFontSize >= 10f, $"Expected readable text, got {minimumFontSize}pt.");
        for (var column = 1; column <= 6; column++)
            Assert.Contains($"Header{column}", firstPageText);
        Assert.DoesNotContain("Header7", firstPageText);
        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "Header8");
    }

    [Fact]
    public void Convert_LargeGeneratedTextTableWithUnflaggedFitAttributes_KeepsReadableColumnGroups()
    {
        var rows = new List<string[]>
        {
            new[] { "ID", "Name", "Description", "Phone", "Email", "Company", "Title", "Notes" },
        };
        rows.AddRange(Enumerable.Range(1, 1000).Select(row => new[]
        {
            $"ID-{row:00000}",
            $"Customer {row:00000}",
            $"Detailed service description for customer account {row:00000}",
            "555-0123",
            $"customer{row:00000}@example.test",
            "Contoso Operations",
            "QA Automation Specialist",
            "Priority customer account",
        }));
        using var excelStream = CreateSimpleExcel(rows.ToArray(), includeUnflaggedFitAttributes: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var renderedBlocks = doc.Pages.SelectMany(page => page.TextBlocks).ToArray();
        var renderedText = renderedBlocks.Select(block => block.Text).ToArray();

        Assert.Contains("Description", renderedText);
        Assert.Contains(renderedText, text => text.StartsWith("QA Automation", StringComparison.Ordinal));
        Assert.DoesNotContain("Descriptio", renderedText);
        Assert.Contains(renderedBlocks, block => block.Text.StartsWith("Detailed service", StringComparison.Ordinal) && block.MaxWidth.HasValue);
    }

    [Fact]
    public void Convert_PrintTitleRowsStartingAtFirstRow_RepeatsHeaderAfterPageBreak()
    {
        var rows = new List<string[]> { new[] { "Record ID", "Name", "City" } };
        for (var i = 1; i <= 90; i++)
            rows.Add(new[] { $"ID-{i:00000}", $"Name {i}", $"City {i}" });

        using var excelStream = CreateSimpleExcel(rows.ToArray(), printTitleFirstRow: true);

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.True(doc.Pages.Count >= 2, $"Expected multiple pages, got {doc.Pages.Count}");
        Assert.Contains(doc.Pages[1].TextBlocks, block => block.Text == "Record ID");
    }

    [Fact]
    public void Convert_AdjacentContentColumns_ConstrainFullTextToCellWidth()
    {
        using var excelStream = CreateSimpleExcel(
            new[]
            {
                new[] { "Street Address", "City" },
                new[] { "1035 North Evergreen Avenue Suite 235", "Berlin" },
            },
            columnWidths: new[] { 34f, 18f },
            customWidth: false);

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var addressBlock = doc.Pages.SelectMany(page => page.TextBlocks)
            .FirstOrDefault(block => block.Text == "1035 North Evergreen Avenue Suite 235");

        Assert.NotNull(addressBlock);
        Assert.True(addressBlock.MaxWidth.HasValue, "Adjacent populated cells should constrain text to the cell width.");
    }

    [Fact]
    public void Convert_EmbeddedNewlineWithoutWrapText_RendersOnOneLine()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "Name", "Address" },
            new[] { "Alice", "Line 1\nLine 2" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var renderedText = doc.Pages.SelectMany(page => page.TextBlocks).Select(block => block.Text).ToArray();

        Assert.Contains("Line 1 Line 2", renderedText);
        Assert.DoesNotContain("Line 1", renderedText);
        Assert.DoesNotContain("Line 2", renderedText);
    }

    [Fact]
    public void Convert_WithRenderLimits_RendersOnlyLeadingRowsAndColumns()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "H1", "H2", "H3" },
            new[] { "R1C1", "R1C2", "R1C3" },
            new[] { "R2C1", "R2C2", "R2C3" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            MaxRows = 2,
            MaxColumns = 2,
        });
        var renderedText = string.Join("\n", doc.Pages.SelectMany(page => page.TextBlocks).Select(block => block.Text));

        Assert.Contains("H1", renderedText);
        Assert.Contains("H2", renderedText);
        Assert.Contains("R1C1", renderedText);
        Assert.Contains("R1C2", renderedText);
        Assert.DoesNotContain("H3", renderedText);
        Assert.DoesNotContain("R1C3", renderedText);
        Assert.DoesNotContain("R2C1", renderedText);
    }

    [Fact]
    public void Convert_WithLandscapeOption_UsesLandscapePageDimensions()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "A", "B" },
            new[] { "1", "2" },
        });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Landscape = true,
        });

        Assert.True(doc.Pages[0].Width > doc.Pages[0].Height,
            $"Expected landscape dimensions, got {doc.Pages[0].Width}x{doc.Pages[0].Height}.");
    }

    [Fact]
    public void Convert_WithFitToPageOption_FitsWideColumnsOnSinglePage()
    {
        var rows = new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        };

        using var defaultStream = CreateSimpleExcel(rows);
        using var fitStream = CreateSimpleExcel(rows);

        var defaultDoc = ExcelToPdfConverter.Convert(defaultStream);
        var fitDoc = ExcelToPdfConverter.Convert(fitStream, new ExcelToPdfConverter.ConversionOptions
        {
            FitToPage = true,
        });

        Assert.True(defaultDoc.Pages.Count >= 2, $"Expected default wide sheet to split, got {defaultDoc.Pages.Count} pages.");
        Assert.Single(fitDoc.Pages);
    }

    [Fact]
    public void Convert_WithFitToWidthZero_DoesNotForceWideColumnsOntoOnePage()
    {
        var rows = new[]
        {
            new[] { "First", "Last", "Address", "Phone", "Email", "Company", "Title", "Notes" },
            new[]
            {
                "Naveen",
                "Adhikari",
                "1234 North Evergreen Avenue",
                "555-0123",
                "naveen.adhikari@example.test",
                "Contoso Operations",
                "QA Automation Specialist",
                "Priority customer account",
            },
        };

        using var excelStream = CreateSimpleExcel(rows);

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            FitToPage = true,
            FitToWidth = 0,
        });

        Assert.True(doc.Pages.Count >= 2, $"Expected unlimited fit-to-width to keep column groups, got {doc.Pages.Count} pages.");
    }

    [Fact]
    public void Convert_WithRowsPerPageOption_ReducesTallSheetPageCount()
    {
        var rows = Enumerable.Range(0, 80)
            .Select(i => new[] { $"Row{i}", $"Value{i}" })
            .ToArray();

        using var defaultStream = CreateSimpleExcel(rows);
        using var compactStream = CreateSimpleExcel(rows);

        var defaultDoc = ExcelToPdfConverter.Convert(defaultStream);
        var compactDoc = ExcelToPdfConverter.Convert(compactStream, new ExcelToPdfConverter.ConversionOptions
        {
            RowsPerPage = 80,
        });

        Assert.True(defaultDoc.Pages.Count >= 2, $"Expected default tall sheet to need multiple pages, got {defaultDoc.Pages.Count}.");
        Assert.True(compactDoc.Pages.Count < defaultDoc.Pages.Count,
            $"Expected rows-per-page layout to reduce pages from {defaultDoc.Pages.Count}, got {compactDoc.Pages.Count}.");
    }

    [Fact]
    public void Convert_EmptyExcel_CreatesAtLeastOnePage()
    {
        using var excelStream = CreateSimpleExcel(Array.Empty<string[]>());

        var doc = ExcelToPdfConverter.Convert(excelStream);
        Assert.True(doc.Pages.Count >= 1);
    }

    [Fact]
    public void Convert_WithSelectedSheets_RendersOnlyMatchingSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Sheets = new[] { "Details" },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("DetailsOnly", content);
        Assert.DoesNotContain("SummaryOnly", content);
        Assert.DoesNotContain("ArchiveOnly", content);
    }

    [Fact]
    public void Convert_WithSelectedSheetIndexes_RendersOnlyMatchingSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            SheetIndexes = new[] { 2 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("DetailsOnly", content);
        Assert.DoesNotContain("SummaryOnly", content);
        Assert.DoesNotContain("ArchiveOnly", content);
    }

    [Fact]
    public void Convert_WorkbookRelationshipTargetOutsideWorksheetsFolder_RendersSelectedSheetIndex()
    {
        using var excelStream = CreateExcelWithWorksheetTarget("sheets/report.xml", "RelationshipTargetOnly");

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            SheetIndexes = new[] { 1 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("RelationshipTargetOnly", content);
    }

    [Fact]
    public void Convert_WithSelectedSheetNamesAndIndexes_RendersUnion()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
            ("Archive", new[] { new[] { "ArchiveOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Sheets = new[] { "Archive" },
            SheetIndexes = new[] { 1 },
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("SummaryOnly", content);
        Assert.Contains("ArchiveOnly", content);
        Assert.DoesNotContain("DetailsOnly", content);
    }

    [Fact]
    public void Convert_WithNullSheets_RendersAllSheets()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions { Sheets = null });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("SummaryOnly", content);
        Assert.Contains("DetailsOnly", content);
    }

    [Fact]
    public void Convert_WithUnknownSheet_ThrowsHelpfulError()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var ex = Assert.Throws<ArgumentException>(() => MiniPdf.ConvertToPdf(excelStream,
            new MiniPdfConversionOptions { Sheets = new[] { "Missing" } }));

        Assert.Contains("Sheet(s) not found: Missing", ex.Message);
        Assert.Contains("Available sheets: Summary, Details", ex.Message);
    }

    [Fact]
    public void Convert_WithUnknownSheetIndex_ThrowsHelpfulError()
    {
        using var excelStream = CreateMultiSheetExcel(new[]
        {
            ("Summary", new[] { new[] { "SummaryOnly" } }),
            ("Details", new[] { new[] { "DetailsOnly" } }),
        });

        var ex = Assert.Throws<ArgumentException>(() => MiniPdf.ConvertToPdf(excelStream,
            new MiniPdfConversionOptions { SheetIndexes = new[] { 3 } }));

        Assert.Contains("Sheet index(es) out of range: 3", ex.Message);
        Assert.Contains("Available index range: 1-2", ex.Message);
    }

    [Fact]
    public void ConvertToFile_CreatesOutputFile()
    {
        var excelPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.xlsx");
        var pdfPath = Path.Combine(Path.GetTempPath(), $"minipdf_test_{Guid.NewGuid()}.pdf");

        try
        {
            using (var fs = File.Create(excelPath))
            using (var excelStream = CreateSimpleExcel(new[]
            {
                new[] { "Test", "Data" },
                new[] { "1", "2" },
            }))
            {
                excelStream.CopyTo(fs);
            }

            ExcelToPdfConverter.ConvertToFile(excelPath, pdfPath);

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(excelPath)) File.Delete(excelPath);
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public void Convert_ManyRows_CreatesMultiplePages()
    {
        var rows = new List<string[]>();
        for (var i = 0; i < 100; i++)
        {
            rows.Add(new[] { $"Row{i}", $"Value{i}", $"Data{i}" });
        }

        using var excelStream = CreateSimpleExcel(rows.ToArray());
        var doc = ExcelToPdfConverter.Convert(excelStream);

        // 100 rows at ~14pt line height should require multiple pages
        Assert.True(doc.Pages.Count >= 2, $"Expected at least 2 pages, got {doc.Pages.Count}");
    }

    [Fact]
    public void Convert_WithTextColor_PreservesColorInPdf()
    {
        // Create an xlsx with red text in cell A1
        using var excelStream = CreateColoredExcel(
            new[] { ("Red Text", "FFFF0000"), ("Normal", "") },
            new[] { ("Blue Val", "FF0000FF"), ("Green", "FF00FF00") }
        );

        var doc = ExcelToPdfConverter.Convert(excelStream);
        var bytes = doc.ToArray();
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("Red Text", content);
        Assert.Contains("Blue Val", content);
        // Verify that non-black color operators appear (red = 1.000 0.000 0.000 rg)
        Assert.Contains("1.000 0.000 0.000 rg", content);
        // Blue = 0.000 0.000 1.000 rg
        Assert.Contains("0.000 0.000 1.000 rg", content);
    }

    [Fact]
    public void ConvertToPdf_StreamApi_AutoDetectsXlsx()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "AutoDetect", "Xlsx" },
            new[] { "Cell1", "Cell2" },
        });

        var bytes = MiniPdf.ConvertToPdf(excelStream);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("AutoDetect", content);
        Assert.Contains("Cell1", content);
        Assert.Contains("%%EOF", content);
    }

    [Fact]
    public void ConvertToPdf_StreamApi_WorksWithNonSeekableXlsxStream()
    {
        using var excelStream = CreateSimpleExcel(new[]
        {
            new[] { "NonSeekable", "Xlsx" },
            new[] { "Row2A", "Row2B" },
        });
        using var nonSeekable = new NonSeekableStream(excelStream);

        var bytes = MiniPdf.ConvertToPdf(nonSeekable);
        var content = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", content);
        Assert.Contains("%%EOF", content);
        // Sanity check: the produced PDF should be larger than an empty one.
        Assert.True(bytes.Length > 500, $"Expected non-trivial PDF, got {bytes.Length} bytes");
    }

    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream _inner;
        public NonSeekableStream(Stream inner) { _inner = inner; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30.596,37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30,596.37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_LargeValueWithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 1234567.89 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1.234.567,89");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30,596.37");
    }

    [Fact]
    public void Convert_BuiltInNumFmtId4_MultipleValuesWithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(
            numFmtId: 4,
            values: new[] { 0, 1, 100, 1234.5, 30596.37, 1234567.89, -30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        var expected = new[]
        {
            "0,00",
            "1,00",
            "100,00",
            "1.234,50",
            "30.596,37",
            "1.234.567,89",
            "-30.596,37",
        };

        foreach (var text in expected)
        {
            Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == text);
        }
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596,37");
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596.37");
    }

    [Fact]
    public void Convert_BuiltInGeneralFormat_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 0, values: new[] { 30596.37 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "30596.37");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_WithEsArCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1-ene-23");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_WithEnUsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream, new ExcelToPdfConverter.ConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"),
        });

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1-Jan-23");
    }

    [Fact]
    public void Convert_BuiltInDateFormat_NullCulturePreservesInvariantBehavior()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 15, values: new[] { 44927.0 });

        var doc = ExcelToPdfConverter.Convert(excelStream);

        Assert.Contains(doc.Pages.SelectMany(page => page.TextBlocks), block => block.Text == "1-Jan-23");
    }

    [Fact]
    public void ConvertToPdf_PublicApi_BuiltInNumFmtId4_HonorsCulture()
    {
        using var excelStream = CreateExcelWithBuiltInNumberFormat(numFmtId: 4, values: new[] { 30596.37 });

        var bytes = MiniPdf.ConvertToPdf(excelStream, new MiniPdfConversionOptions
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
        });
        var content = Encoding.ASCII.GetString(bytes);

        Assert.Contains("30.596,37", content);
    }

    /// <summary>
    /// Creates a minimal .xlsx with a built-in number format (numFmtId only, no custom
    /// &lt;numFmts&gt; entry) and a single column of numeric cells using that format. This
    /// reproduces the real-world XLSX where cells carry only a built-in numFmtId.
    /// </summary>
    private static MemoryStream CreateExcelWithBuiltInNumberFormat(int numFmtId, params double[] values)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            // styles.xml with a built-in numFmtId and no custom <numFmts> section:
            // the cell style references the built-in format directly.
            AddEntry(archive, "xl/styles.xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="1">
                    <font><sz val="11"/><name val="Calibri"/></font>
                  </fonts>
                  <fills count="1">
                    <fill><patternFill patternType="none"/></fill>
                  </fills>
                  <borders count="1">
                    <border><left/><right/><top/><bottom/><diagonal/></border>
                  </borders>
                  <cellXfs count="1">
                    <xf numFmtId="{{numFmtId}}" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>
                  </cellXfs>
                </styleSheet>
                """);

            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            sheetSb.AppendLine("<sheetData>");
            for (var i = 0; i < values.Length; i++)
            {
                sheetSb.AppendLine($"  <row r=\"{i + 1}\">");
                sheetSb.AppendLine($"    <c r=\"A{i + 1}\" s=\"0\"><v>{values[i].ToString(System.Globalization.CultureInfo.InvariantCulture)}</v></c>");
                sheetSb.AppendLine("  </row>");
            }
            sheetSb.AppendLine("</sheetData>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Creates a minimal valid .xlsx file in memory with the given data.
    /// </summary>
    private static MemoryStream CreateSimpleExcel(string[][] rows, float[]? columnWidths = null, bool customWidth = true,
        bool printTitleFirstRow = false, bool includeUnflaggedFitAttributes = false)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // [Content_Types].xml
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            // _rels/.rels
            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            // xl/_rels/workbook.xml.rels
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);

                        var definedNames = printTitleFirstRow
                                ? """
                                        <definedNames>
                                            <definedName name="_xlnm.Print_Titles" localSheetId="0">'Sheet1'!$1:$1</definedName>
                                        </definedNames>
                                    """
                                : "";

                        // xl/workbook.xml
            AddEntry(archive, "xl/workbook.xml",
                                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                                {{definedNames}}
                </workbook>
                """);

            // Build shared strings and sheet data
            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            // Build sheet XML
            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            if (columnWidths is { Length: > 0 })
            {
                sheetSb.AppendLine("<cols>");
                for (var c = 0; c < columnWidths.Length; c++)
                {
                    var width = columnWidths[c].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var customWidthAttr = customWidth ? " customWidth=\"1\"" : "";
                    sheetSb.AppendLine($"  <col min=\"{c + 1}\" max=\"{c + 1}\" width=\"{width}\"{customWidthAttr}/>");
                }
                sheetSb.AppendLine("</cols>");
            }
            sheetSb.AppendLine("<sheetData>");

            for (var r = 0; r < rows.Length; r++)
            {
                sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                for (var c = 0; c < rows[r].Length; c++)
                {
                    var colLetter = (char)('A' + c);
                    var cellRef = $"{colLetter}{r + 1}";
                    var idx = GetStringIndex(rows[r][c]);
                    sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\"><v>{idx}</v></c>");
                }
                sheetSb.AppendLine("  </row>");
            }

            sheetSb.AppendLine("</sheetData>");
            if (includeUnflaggedFitAttributes)
                sheetSb.AppendLine("<pageSetup paperSize=\"1\" scale=\"100\" fitToWidth=\"1\" fitToHeight=\"1\"/>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());

            // Build shared strings XML
            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");

            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

        private static MemoryStream CreateExcelWithImageOverCell()
        {
                var stream = new MemoryStream();
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                {
                        AddEntry(archive, "[Content_Types].xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                                    <Default Extension="xml" ContentType="application/xml"/>
                                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                                    <Default Extension="png" ContentType="image/png"/>
                                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                                    <Override PartName="/xl/drawings/drawing1.xml" ContentType="application/vnd.openxmlformats-officedocument.drawing+xml"/>
                                </Types>
                                """);
                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/_rels/workbook.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/workbook.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <sheets><sheet name="Sheet1" sheetId="1" r:id="rId1"/></sheets>
                                </workbook>
                                """);
                        AddEntry(archive, "xl/worksheets/sheet1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                     xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                      <sheetData><row r="1"><c r="D1" t="inlineStr"><is><t>Covered</t></is></c></row></sheetData>
                                    <drawing r:id="rId1"/>
                                </worksheet>
                                """);
                        AddEntry(archive, "xl/worksheets/_rels/sheet1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing" Target="../drawings/drawing1.xml"/>
                                </Relationships>
                                """);
                        AddEntry(archive, "xl/drawings/drawing1.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
                                                    xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <xdr:oneCellAnchor>
                                        <xdr:from><xdr:col>0</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>0</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
                                        <xdr:ext cx="1905000" cy="914400"/>
                                        <xdr:pic><xdr:blipFill><a:blip r:embed="rId1"/></xdr:blipFill></xdr:pic>
                                        <xdr:clientData/>
                                    </xdr:oneCellAnchor>
                                </xdr:wsDr>
                                """);
                        AddEntry(archive, "xl/drawings/_rels/drawing1.xml.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="../media/image1.png"/>
                                </Relationships>
                                """);

                        var imageEntry = archive.CreateEntry("xl/media/image1.png");
                        using var imageStream = imageEntry.Open();
                        var image = Convert.FromBase64String(
                                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/lLQ2VwAAAABJRU5ErkJggg==");
                        imageStream.Write(image, 0, image.Length);
                }

                stream.Position = 0;
                return stream;
        }

    private static MemoryStream CreateMultiSheetExcel((string Name, string[][] Rows)[] sheets)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var worksheetOverrides = string.Join(Environment.NewLine, sheets.Select((_, index) =>
                $"  <Override PartName=\"/xl/worksheets/sheet{index + 1}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"));
            AddEntry(archive, "[Content_Types].xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                {{worksheetOverrides}}
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            var rels = new StringBuilder();
            for (var i = 0; i < sheets.Length; i++)
            {
                rels.AppendLine($"  <Relationship Id=\"rId{i + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i + 1}.xml\"/>");
            }
            rels.AppendLine($"  <Relationship Id=\"rId{sheets.Length + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>");
            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                {{rels}}</Relationships>
                """);

            var workbookSheets = new StringBuilder();
            for (var i = 0; i < sheets.Length; i++)
            {
                workbookSheets.AppendLine($"    <sheet name=\"{EscapeXml(sheets[i].Name)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
            }
            AddEntry(archive, "xl/workbook.xml",
                $$"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                {{workbookSheets}}  </sheets>
                </workbook>
                """);

            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            for (var sheetIndex = 0; sheetIndex < sheets.Length; sheetIndex++)
            {
                var sheetSb = new StringBuilder();
                sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
                sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
                sheetSb.AppendLine("<sheetData>");
                for (var r = 0; r < sheets[sheetIndex].Rows.Length; r++)
                {
                    sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                    for (var c = 0; c < sheets[sheetIndex].Rows[r].Length; c++)
                    {
                        var colLetter = (char)('A' + c);
                        var cellRef = $"{colLetter}{r + 1}";
                        var idx = GetStringIndex(sheets[sheetIndex].Rows[r][c]);
                        sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\"><v>{idx}</v></c>");
                    }
                    sheetSb.AppendLine("  </row>");
                }
                sheetSb.AppendLine("</sheetData>");
                sheetSb.AppendLine("</worksheet>");
                AddEntry(archive, $"xl/worksheets/sheet{sheetIndex + 1}.xml", sheetSb.ToString());
            }

            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");
            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }

        private static MemoryStream CreateExcelWithWorksheetTarget(string worksheetTarget, string cellText)
        {
                var ms = new MemoryStream();
                var worksheetPart = "xl/" + worksheetTarget.Replace('\\', '/').TrimStart('/');

                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                        AddEntry(archive, "[Content_Types].xml",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                                    <Default Extension="xml" ContentType="application/xml"/>
                                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                                    <Override PartName="/{{worksheetPart}}" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                                    <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                                </Types>
                                """);

                        AddEntry(archive, "_rels/.rels",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                                </Relationships>
                                """);

                        AddEntry(archive, "xl/_rels/workbook.xml.rels",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="{{worksheetTarget}}"/>
                                    <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                                </Relationships>
                                """);

                        AddEntry(archive, "xl/workbook.xml",
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                                                    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                                    <sheets>
                                        <sheet name="Report" sheetId="1" r:id="rId1"/>
                                    </sheets>
                                </workbook>
                                """);

                        AddEntry(archive, worksheetPart,
                                """
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                                    <sheetData>
                                        <row r="1">
                                            <c r="A1" t="s"><v>0</v></c>
                                        </row>
                                    </sheetData>
                                </worksheet>
                                """);

                        AddEntry(archive, "xl/sharedStrings.xml",
                                $$"""
                                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="1" uniqueCount="1">
                                    <si><t>{{EscapeXml(cellText)}}</t></si>
                                </sst>
                                """);
                }

                ms.Position = 0;
                return ms;
        }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    /// <summary>
    /// Creates a minimal .xlsx with per-cell font colors.
    /// Each row is an array of (text, argbHex) tuples. Empty argb = default/black.
    /// </summary>
    private static MemoryStream CreateColoredExcel(params (string text, string argb)[][] rows)
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            // Collect unique colors and build fonts + cellXfs
            var colorToFontIndex = new Dictionary<string, int>();
            var fontsSb = new StringBuilder();
            var xfsSb = new StringBuilder();

            // Font 0 / xf 0 = default (black, no color element)
            fontsSb.AppendLine("  <font><sz val=\"11\"/><name val=\"Calibri\"/></font>");
            xfsSb.AppendLine("  <xf fontId=\"0\"/>");
            var nextFontId = 1;
            var nextXfId = 1;

            // Map (fontIndex -> xfIndex) for non-default colors
            var colorToXfIndex = new Dictionary<string, int>();

            foreach (var row in rows)
            {
                foreach (var (_, argb) in row)
                {
                    if (string.IsNullOrEmpty(argb) || colorToXfIndex.ContainsKey(argb))
                        continue;

                    fontsSb.AppendLine($"  <font><color rgb=\"{argb}\"/><sz val=\"11\"/><name val=\"Calibri\"/></font>");
                    colorToFontIndex[argb] = nextFontId;

                    xfsSb.AppendLine($"  <xf fontId=\"{nextFontId}\"/>");
                    colorToXfIndex[argb] = nextXfId;

                    nextFontId++;
                    nextXfId++;
                }
            }

            var stylesSb = new StringBuilder();
            stylesSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            stylesSb.AppendLine("""<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            stylesSb.AppendLine($"<fonts count=\"{nextFontId}\">");
            stylesSb.Append(fontsSb);
            stylesSb.AppendLine("</fonts>");
            stylesSb.AppendLine($"<cellXfs count=\"{nextXfId}\">");
            stylesSb.Append(xfsSb);
            stylesSb.AppendLine("</cellXfs>");
            stylesSb.AppendLine("</styleSheet>");

            AddEntry(archive, "xl/styles.xml", stylesSb.ToString());

            // Shared strings
            var sharedStrings = new List<string>();
            var sharedStringIndex = new Dictionary<string, int>();

            int GetStringIndex(string value)
            {
                if (!sharedStringIndex.TryGetValue(value, out var idx))
                {
                    idx = sharedStrings.Count;
                    sharedStrings.Add(value);
                    sharedStringIndex[value] = idx;
                }
                return idx;
            }

            // Sheet data
            var sheetSb = new StringBuilder();
            sheetSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            sheetSb.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
            sheetSb.AppendLine("<sheetData>");

            for (var r = 0; r < rows.Length; r++)
            {
                sheetSb.AppendLine($"  <row r=\"{r + 1}\">");
                for (var c = 0; c < rows[r].Length; c++)
                {
                    var colLetter = (char)('A' + c);
                    var cellRef = $"{colLetter}{r + 1}";
                    var idx = GetStringIndex(rows[r][c].text);
                    var styleIdx = !string.IsNullOrEmpty(rows[r][c].argb) && colorToXfIndex.TryGetValue(rows[r][c].argb, out var si) ? si : 0;
                    sheetSb.AppendLine($"    <c r=\"{cellRef}\" t=\"s\" s=\"{styleIdx}\"><v>{idx}</v></c>");
                }
                sheetSb.AppendLine("  </row>");
            }

            sheetSb.AppendLine("</sheetData>");
            sheetSb.AppendLine("</worksheet>");

            AddEntry(archive, "xl/worksheets/sheet1.xml", sheetSb.ToString());

            var ssSb = new StringBuilder();
            ssSb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
            ssSb.AppendLine($"""<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="{sharedStrings.Count}" uniqueCount="{sharedStrings.Count}">""");
            foreach (var s in sharedStrings)
            {
                ssSb.AppendLine($"  <si><t>{EscapeXml(s)}</t></si>");
            }
            ssSb.AppendLine("</sst>");

            AddEntry(archive, "xl/sharedStrings.xml", ssSb.ToString());
        }

        ms.Position = 0;
        return ms;
    }
}
