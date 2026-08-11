using System.Globalization;

namespace MiniSoftware;

internal static class DocumentContentExtractor
{
    internal static MiniPdfDocumentContent ExtractDocx(Stream stream, string? sourceName, MiniPdfContentOptions options)
    {
        var document = DocxReader.Read(stream);
        var blocks = new List<MiniPdfContentBlock>();
        List<MiniPdfContentListItem>? listItems = null;
        var listSourceIndex = 0;

        void FlushList()
        {
            if (listItems == null)
                return;
            blocks.Add(new MiniPdfListBlock(listSourceIndex, listItems));
            listItems = null;
        }

        for (var elementIndex = 0; elementIndex < document.Elements.Count; elementIndex++)
        {
            switch (document.Elements[elementIndex])
            {
                case DocxParagraph paragraph:
                {
                    var runs = ConvertRuns(paragraph.Runs);
                    if (paragraph.IsBulletList || paragraph.IsNumberedList)
                    {
                        if (listItems == null)
                        {
                            listItems = new List<MiniPdfContentListItem>();
                            listSourceIndex = elementIndex;
                        }
                        listItems.Add(new MiniPdfContentListItem(
                            Math.Max(0, paragraph.ListLevel),
                            paragraph.IsNumberedList,
                            paragraph.ListText,
                            runs));
                    }
                    else
                    {
                        FlushList();
                        if (runs.Count > 0)
                        {
                            var headingLevel = GetHeadingLevel(paragraph);
                            blocks.Add(headingLevel > 0
                                ? new MiniPdfHeadingBlock(elementIndex, headingLevel, runs)
                                : new MiniPdfParagraphBlock(elementIndex, runs));
                        }
                    }

                    if (options.IncludeMediaMetadata)
                    {
                        foreach (var image in paragraph.Images)
                        {
                            blocks.Add(new MiniPdfImageBlock(
                                elementIndex,
                                image.Extension,
                                image.Name,
                                image.AlternativeText,
                                image.WidthEmu,
                                image.HeightEmu));
                        }
                    }
                    break;
                }
                case DocxTable table:
                    FlushList();
                    blocks.Add(ConvertDocxTable(table, elementIndex));
                    break;
                default:
                    FlushList();
                    break;
            }
        }
        FlushList();

        if (document.Footnotes != null)
        {
            foreach (var footnote in document.Footnotes.Values.OrderBy(note => ParseSortableInt(note.Id)))
            {
                blocks.Add(new MiniPdfNoteBlock(
                    blocks.Count,
                    "footnote",
                    footnote.Id,
                    null,
                    string.Concat(footnote.Runs.Select(run => run.Text))));
            }
        }

        var title = blocks.OfType<MiniPdfHeadingBlock>()
            .FirstOrDefault(block => block.Level == 1) is { } heading
            ? NormalizeWhitespace(PlainText(heading.Runs))
            : null;
        var section = new MiniPdfContentSection("document", 1, title, blocks);
        return new MiniPdfDocumentContent("docx", sourceName, new[] { section });
    }

    internal static MiniPdfDocumentContent ExtractXlsx(Stream stream, string? sourceName, MiniPdfContentOptions options)
    {
        var sheets = ExcelReader.ReadSheets(stream);
        var sections = new List<MiniPdfContentSection>();

        for (var sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
        {
            var sheet = sheets[sheetIndex];
            if (!IsSelectedSheet(sheet, sheetIndex, options))
                continue;

            var rowCount = options.MaxRows.HasValue
                ? Math.Min(sheet.Rows.Count, options.MaxRows.Value)
                : sheet.Rows.Count;
            var naturalColumnCount = rowCount == 0 ? 0 : sheet.Rows.Take(rowCount).Max(row => row.Count);
            var columnCount = options.MaxColumns.HasValue
                ? Math.Min(naturalColumnCount, options.MaxColumns.Value)
                : naturalColumnCount;
            var rows = new List<MiniPdfContentTableRow>(rowCount);

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var cells = new List<MiniPdfContentTableCell>(columnCount);
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var text = columnIndex < sheet.Rows[rowIndex].Count
                        ? sheet.Rows[rowIndex][columnIndex].Text
                        : string.Empty;
                    var merge = sheet.MergedCells.FirstOrDefault(region =>
                        region.StartRow == rowIndex && region.StartCol == columnIndex);
                    var rowSpan = merge == default ? 1 : merge.EndRow - merge.StartRow + 1;
                    var columnSpan = merge == default ? 1 : merge.EndCol - merge.StartCol + 1;
                    cells.Add(new MiniPdfContentTableCell(
                        rowIndex,
                        columnIndex,
                        GetCellAddress(rowIndex, columnIndex),
                        rowSpan,
                        columnSpan,
                        text,
                        columnIndex < sheet.Rows[rowIndex].Count ? sheet.Rows[rowIndex][columnIndex].Link : null));
                }
                rows.Add(new MiniPdfContentTableRow(rowIndex, false, cells));
            }

            var blocks = new List<MiniPdfContentBlock>
            {
                new MiniPdfTableBlock(0, rows),
            };
            if (options.IncludeMediaMetadata)
            {
                for (var imageIndex = 0; imageIndex < sheet.Images.Count; imageIndex++)
                {
                    var image = sheet.Images[imageIndex];
                    blocks.Add(new MiniPdfImageBlock(
                        imageIndex + 1,
                        image.Extension,
                        null,
                        null,
                        image.WidthEmu,
                        image.HeightEmu));
                }
                for (var chartIndex = 0; chartIndex < sheet.Charts.Count; chartIndex++)
                {
                    var chart = sheet.Charts[chartIndex];
                    blocks.Add(new MiniPdfChartBlock(
                        sheet.Images.Count + chartIndex + 1,
                        chart.ChartType,
                        string.IsNullOrWhiteSpace(chart.Title) ? null : chart.Title,
                        chart.Series.Select(series => series.Name).ToArray()));
                }
            }

            sections.Add(new MiniPdfContentSection("worksheet", sheetIndex + 1, sheet.Name, blocks));
        }

        return new MiniPdfDocumentContent("xlsx", sourceName, sections);
    }

    internal static MiniPdfDocumentContent ExtractPptx(Stream stream, string? sourceName, MiniPdfContentOptions options)
    {
        var presentation = PptxReader.Read(stream);
        var sections = new List<MiniPdfContentSection>(presentation.Slides.Count);

        for (var slideIndex = 0; slideIndex < presentation.Slides.Count; slideIndex++)
        {
            var slide = presentation.Slides[slideIndex];
            var ordered = slide.Elements
                .Select((element, index) => new { Element = element, Index = index, Bounds = GetBounds(element) })
                .OrderBy(item => item.Bounds.Y)
                .ThenBy(item => item.Bounds.X)
                .ThenBy(item => item.Index)
                .ToList();
            var title = ordered
                .Where(item => item.Element is PptxShape)
                .Select(item => ((PptxShape)item.Element).Paragraphs.FirstOrDefault())
                .Where(paragraph => paragraph != null)
                .Select(paragraph => NormalizeWhitespace(string.Concat(paragraph!.Runs.Select(run => run.Text))))
                .FirstOrDefault(text => text.Length > 0);
            var blocks = new List<MiniPdfContentBlock>();

            foreach (var item in ordered)
            {
                if (item.Element is PptxShape shape)
                {
                    List<MiniPdfContentListItem>? listItems = null;
                    for (var paragraphIndex = 0; paragraphIndex < shape.Paragraphs.Count; paragraphIndex++)
                    {
                        var paragraph = shape.Paragraphs[paragraphIndex];
                        var runs = paragraph.Runs
                            .Where(run => run.Text.Length > 0)
                            .Select((run, runIndex) => new MiniPdfContentRun(
                                paragraph.IsBullet && runIndex == 0 && run.Text.StartsWith("\u2022 ", StringComparison.Ordinal)
                                    ? run.Text.Substring(2)
                                    : run.Text,
                                run.Bold,
                                run.Italic,
                                run.Link))
                            .ToArray();
                        if (runs.Length == 0)
                            continue;

                        if (paragraph.IsBullet)
                        {
                            listItems ??= new List<MiniPdfContentListItem>();
                            listItems.Add(new MiniPdfContentListItem(paragraph.Level, false, null, runs));
                        }
                        else
                        {
                            if (listItems != null)
                            {
                                blocks.Add(new MiniPdfListBlock(item.Index, listItems));
                                listItems = null;
                            }
                            var text = PlainText(runs);
                            blocks.Add(text == title && blocks.Count == 0
                                ? new MiniPdfHeadingBlock(item.Index, 1, runs)
                                : new MiniPdfParagraphBlock(item.Index, runs));
                        }
                    }
                    if (listItems != null)
                        blocks.Add(new MiniPdfListBlock(item.Index, listItems));
                }
                else if (options.IncludeMediaMetadata && item.Element is PptxPicture picture)
                {
                    blocks.Add(new MiniPdfImageBlock(
                        item.Index,
                        picture.Format,
                        picture.Name,
                        picture.AlternativeText,
                        (long)Math.Round(picture.Bounds.Width * 12700d),
                        (long)Math.Round(picture.Bounds.Height * 12700d)));
                }
            }

            sections.Add(new MiniPdfContentSection("slide", slideIndex + 1, title, blocks));
        }

        return new MiniPdfDocumentContent("pptx", sourceName, sections);
    }

    private static MiniPdfTableBlock ConvertDocxTable(DocxTable table, int sourceIndex)
    {
        var rows = new List<MiniPdfContentTableRow>(table.Rows.Count);
        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var sourceRow = table.Rows[rowIndex];
            var cells = new List<MiniPdfContentTableCell>(sourceRow.Cells.Count);
            var columnIndex = sourceRow.GridBefore;
            foreach (var sourceCell in sourceRow.Cells)
            {
                var text = string.Join("\n", sourceCell.Paragraphs.Select(paragraph =>
                    string.Concat(paragraph.Runs.Select(run => run.Text))));
                cells.Add(new MiniPdfContentTableCell(
                    rowIndex,
                    columnIndex,
                    null,
                    1,
                    Math.Max(1, sourceCell.GridSpan),
                    text));
                columnIndex += Math.Max(1, sourceCell.GridSpan);
            }
            rows.Add(new MiniPdfContentTableRow(rowIndex, sourceRow.IsHeader, cells));
        }
        return new MiniPdfTableBlock(sourceIndex, rows);
    }

    private static IReadOnlyList<MiniPdfContentRun> ConvertRuns(IEnumerable<DocxRun> runs)
        => runs
            .Where(run => run.Text.Length > 0)
            .Select(run => new MiniPdfContentRun(run.Text, run.Bold, run.Italic, run.Link))
            .ToArray();

    private static int GetHeadingLevel(DocxParagraph paragraph)
    {
        if (paragraph.OutlineLevel >= 0)
            return Math.Min(6, paragraph.OutlineLevel + 1);
        var style = paragraph.StyleId;
        if (style != null && style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(style.Substring("Heading".Length).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var level))
            return Math.Max(1, Math.Min(6, level));
        return 0;
    }

    private static bool IsSelectedSheet(ExcelSheet sheet, int index, MiniPdfContentOptions options)
    {
        var hasNames = options.Sheets is { Length: > 0 };
        var hasIndexes = options.SheetIndexes is { Length: > 0 };
        if (!hasNames && !hasIndexes)
            return true;
        return (hasNames && options.Sheets!.Contains(sheet.Name, StringComparer.OrdinalIgnoreCase)) ||
               (hasIndexes && options.SheetIndexes!.Contains(index + 1));
    }

    private static string GetCellAddress(int rowIndex, int columnIndex)
    {
        var column = string.Empty;
        for (var value = columnIndex + 1; value > 0; value = (value - 1) / 26)
            column = (char)('A' + ((value - 1) % 26)) + column;
        return column + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
    }

    private static PptxRect GetBounds(PptxElement element)
        => element switch
        {
            PptxShape shape => shape.Bounds,
            PptxPicture picture => picture.Bounds,
            _ => new PptxRect(float.MaxValue, float.MaxValue, 0, 0),
        };

    private static int ParseSortableInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : int.MaxValue;

    private static string PlainText(IEnumerable<MiniPdfContentRun> runs)
        => string.Concat(runs.Select(run => run.Text)).Trim();

    private static string NormalizeWhitespace(string value)
        => string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
