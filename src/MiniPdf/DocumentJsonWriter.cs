using System.Globalization;
using System.Text;

namespace MiniSoftware;

internal static class DocumentJsonWriter
{
    internal static string Write(MiniPdfDocumentContent document)
    {
        var writer = new JsonTextWriter();
        writer.BeginObject();
        writer.Number("schemaVersion", document.SchemaVersion);
        writer.String("sourceFormat", document.SourceFormat);
        writer.NullableString("sourceName", document.SourceName);
        writer.Name("sections");
        writer.BeginArray();
        foreach (var section in document.Sections)
        {
            writer.BeginObject();
            writer.String("kind", section.Kind);
            writer.Number("index", section.Index);
            writer.NullableString("title", section.Title);
            writer.Name("blocks");
            writer.BeginArray();
            foreach (var block in section.Blocks)
                WriteBlock(writer, block);
            writer.EndArray();
            writer.EndObject();
        }
        writer.EndArray();
        writer.EndObject();
        return writer.ToString();
    }

    private static void WriteBlock(JsonTextWriter writer, MiniPdfContentBlock block)
    {
        writer.BeginObject();
        writer.String("kind", block.Kind);
        writer.Number("sourceIndex", block.SourceIndex);
        switch (block)
        {
            case MiniPdfHeadingBlock heading:
                writer.Number("level", heading.Level);
                WriteRuns(writer, heading.Runs);
                break;
            case MiniPdfParagraphBlock paragraph:
                WriteRuns(writer, paragraph.Runs);
                break;
            case MiniPdfListBlock list:
                writer.Name("items");
                writer.BeginArray();
                foreach (var item in list.Items)
                {
                    writer.BeginObject();
                    writer.Number("level", item.Level);
                    writer.Boolean("ordered", item.Ordered);
                    writer.NullableString("marker", item.Marker);
                    WriteRuns(writer, item.Runs);
                    writer.EndObject();
                }
                writer.EndArray();
                break;
            case MiniPdfTableBlock table:
                writer.Name("rows");
                writer.BeginArray();
                foreach (var row in table.Rows)
                {
                    writer.BeginObject();
                    writer.Number("index", row.Index);
                    writer.Boolean("isHeader", row.IsHeader);
                    writer.Name("cells");
                    writer.BeginArray();
                    foreach (var cell in row.Cells)
                    {
                        writer.BeginObject();
                        writer.Number("rowIndex", cell.RowIndex);
                        writer.Number("columnIndex", cell.ColumnIndex);
                        writer.NullableString("address", cell.Address);
                        writer.Number("rowSpan", cell.RowSpan);
                        writer.Number("columnSpan", cell.ColumnSpan);
                        writer.String("text", cell.Text);
                        writer.NullableString("link", cell.Link);
                        writer.EndObject();
                    }
                    writer.EndArray();
                    writer.EndObject();
                }
                writer.EndArray();
                break;
            case MiniPdfImageBlock image:
                writer.String("format", image.Format);
                writer.NullableString("name", image.Name);
                writer.NullableString("alternativeText", image.AlternativeText);
                writer.Number("width", image.Width);
                writer.Number("height", image.Height);
                break;
            case MiniPdfChartBlock chart:
                writer.String("chartType", chart.ChartType);
                writer.NullableString("title", chart.Title);
                writer.Name("seriesNames");
                writer.BeginArray();
                foreach (var name in chart.SeriesNames)
                    writer.Value(name);
                writer.EndArray();
                break;
            case MiniPdfNoteBlock note:
                writer.String("noteType", note.NoteType);
                writer.NullableString("label", note.Label);
                writer.NullableString("author", note.Author);
                writer.String("text", note.Text);
                break;
        }
        writer.EndObject();
    }

    private static void WriteRuns(JsonTextWriter writer, IReadOnlyList<MiniPdfContentRun> runs)
    {
        writer.Name("runs");
        writer.BeginArray();
        foreach (var run in runs)
        {
            writer.BeginObject();
            writer.String("text", run.Text);
            writer.Boolean("bold", run.Bold);
            writer.Boolean("italic", run.Italic);
            writer.NullableString("link", run.Link);
            writer.EndObject();
        }
        writer.EndArray();
    }

    private sealed class JsonTextWriter
    {
        private readonly StringBuilder _output = new();
        private readonly Stack<bool> _firstValues = new();
        private bool _afterName;

        internal void BeginObject() => BeginContainer('{');
        internal void EndObject() => EndContainer('}');
        internal void BeginArray() => BeginContainer('[');
        internal void EndArray() => EndContainer(']');

        internal void Name(string name)
        {
            BeforeValue();
            AppendQuoted(name);
            _output.Append(':');
            _afterName = true;
        }

        internal void String(string name, string value)
        {
            Name(name);
            Value(value);
        }

        internal void NullableString(string name, string? value)
        {
            Name(name);
            if (value == null)
                Null();
            else
                Value(value);
        }

        internal void Number(string name, long value)
        {
            Name(name);
            Raw(value.ToString(CultureInfo.InvariantCulture));
        }

        internal void Boolean(string name, bool value)
        {
            Name(name);
            Raw(value ? "true" : "false");
        }

        internal void Value(string value)
        {
            BeforeValue();
            AppendQuoted(value);
        }

        private void Null() => Raw("null");

        private void Raw(string value)
        {
            BeforeValue();
            _output.Append(value);
        }

        private void BeginContainer(char token)
        {
            BeforeValue();
            _output.Append(token);
            _firstValues.Push(true);
        }

        private void EndContainer(char token)
        {
            _output.Append(token);
            _firstValues.Pop();
        }

        private void BeforeValue()
        {
            if (_afterName)
            {
                _afterName = false;
                return;
            }
            if (_firstValues.Count == 0)
                return;
            if (_firstValues.Peek())
            {
                _firstValues.Pop();
                _firstValues.Push(false);
            }
            else
            {
                _output.Append(',');
            }
        }

        private void AppendQuoted(string value)
        {
            _output.Append('"');
            foreach (var character in value)
            {
                switch (character)
                {
                    case '"': _output.Append("\\\""); break;
                    case '\\': _output.Append("\\\\"); break;
                    case '\b': _output.Append("\\b"); break;
                    case '\f': _output.Append("\\f"); break;
                    case '\n': _output.Append("\\n"); break;
                    case '\r': _output.Append("\\r"); break;
                    case '\t': _output.Append("\\t"); break;
                    default:
                        if (character < ' ')
                            _output.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            _output.Append(character);
                        break;
                }
            }
            _output.Append('"');
        }

        public override string ToString() => _output.ToString();
    }
}
