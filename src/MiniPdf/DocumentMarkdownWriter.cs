using System.Text;

namespace MiniSoftware;

internal static class DocumentMarkdownWriter
{
    internal static string Write(MiniPdfDocumentContent document)
    {
        var output = new StringBuilder();
        foreach (var section in document.Sections)
        {
            if (output.Length > 0)
                output.Append('\n');
            WriteSectionHeading(output, section);
            foreach (var block in section.Blocks)
                WriteBlock(output, block);
        }
        return output.ToString();
    }

    private static void WriteSectionHeading(StringBuilder output, MiniPdfContentSection section)
    {
        if (section.Kind == "worksheet")
            output.Append("# Worksheet: ").Append(EscapeInline(section.Title ?? section.Index.ToString())).Append("\n\n");
        else if (section.Kind == "slide")
        {
            output.Append("# Slide ").Append(section.Index);
            if (!string.IsNullOrWhiteSpace(section.Title))
                output.Append(": ").Append(EscapeInline(section.Title!));
            output.Append("\n\n");
        }
    }

    private static void WriteBlock(StringBuilder output, MiniPdfContentBlock block)
    {
        switch (block)
        {
            case MiniPdfHeadingBlock heading:
                output.Append(new string('#', Math.Max(1, Math.Min(6, heading.Level))))
                    .Append(' ');
                WriteRuns(output, heading.Runs);
                output.Append("\n\n");
                break;
            case MiniPdfParagraphBlock paragraph:
                WriteRuns(output, paragraph.Runs);
                output.Append("\n\n");
                break;
            case MiniPdfListBlock list:
                foreach (var item in list.Items)
                {
                    output.Append(new string(' ', Math.Max(0, item.Level) * 2));
                    output.Append(item.Ordered ? "1. " : "- ");
                    WriteRuns(output, item.Runs);
                    output.Append('\n');
                }
                output.Append('\n');
                break;
            case MiniPdfTableBlock table:
                WriteTable(output, table);
                break;
            case MiniPdfImageBlock image:
                output.Append("> Image");
                if (!string.IsNullOrWhiteSpace(image.Name))
                    output.Append(": ").Append(EscapeInline(image.Name!));
                output.Append(" (format: ").Append(EscapeInline(image.Format));
                if (!string.IsNullOrWhiteSpace(image.AlternativeText))
                    output.Append(", alt: ").Append(EscapeInline(image.AlternativeText!));
                output.Append(")\n\n");
                break;
            case MiniPdfChartBlock chart:
                output.Append("> Chart: ").Append(EscapeInline(chart.Title ?? chart.ChartType));
                if (chart.SeriesNames.Count > 0)
                    output.Append(" (series: ").Append(string.Join(", ", chart.SeriesNames.Select(EscapeInline))).Append(')');
                output.Append("\n\n");
                break;
            case MiniPdfNoteBlock note:
                output.Append("> ").Append(EscapeInline(note.NoteType));
                if (!string.IsNullOrWhiteSpace(note.Label))
                    output.Append(' ').Append(EscapeInline(note.Label!));
                if (!string.IsNullOrWhiteSpace(note.Author))
                    output.Append(" by ").Append(EscapeInline(note.Author!));
                output.Append(": ").Append(EscapeInline(note.Text)).Append("\n\n");
                break;
        }
    }

    private static void WriteTable(StringBuilder output, MiniPdfTableBlock table)
    {
        var columnCount = table.Rows.Count == 0 ? 0 : table.Rows.Max(row => row.Cells.Count);
        if (columnCount == 0)
            return;
        var header = table.Rows.FirstOrDefault(row => row.IsHeader);
        WriteTableRow(output, header, columnCount);
        output.Append('|');
        for (var column = 0; column < columnCount; column++)
            output.Append(" --- |");
        output.Append('\n');
        foreach (var row in table.Rows)
        {
            if (ReferenceEquals(row, header))
                continue;
            WriteTableRow(output, row, columnCount);
        }
        output.Append('\n');
    }

    private static void WriteTableRow(StringBuilder output, MiniPdfContentTableRow? row, int columnCount)
    {
        output.Append('|');
        for (var column = 0; column < columnCount; column++)
        {
            var cell = row != null && column < row.Cells.Count ? row.Cells[column] : null;
            var text = cell?.Text ?? string.Empty;
            var rendered = EscapeTableCell(text);
            if (!string.IsNullOrWhiteSpace(cell?.Link))
                rendered = "[" + rendered + "](" + cell!.Link!.Replace(" ", "%20") + ")";
            output.Append(' ').Append(rendered).Append(" |");
        }
        output.Append('\n');
    }

    private static void WriteRuns(StringBuilder output, IEnumerable<MiniPdfContentRun> runs)
    {
        foreach (var run in runs)
        {
            var text = EscapeInline(run.Text);
            if (run.Bold)
                text = "**" + text + "**";
            if (run.Italic)
                text = "*" + text + "*";
            if (!string.IsNullOrWhiteSpace(run.Link))
                text = "[" + text + "](" + run.Link!.Replace(" ", "%20") + ")";
            output.Append(text);
        }
    }

    private static string EscapeInline(string value)
        => value.Replace("\\", "\\\\")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("`", "\\`")
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

    private static string EscapeTableCell(string value)
        => EscapeInline(value).Replace("|", "\\|").Replace("\n", "<br>");
}
