using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace MiniSoftware;

internal sealed class LowMemoryWorkbook : IDisposable
{
    private readonly List<IDisposable> _resources = [];
    private readonly Dictionary<string, FileBackedList<string>> _rows = new(StringComparer.Ordinal);
    internal FileStream Package { get; }
    internal FileBackedList<string> SharedStrings { get; }
    internal Dictionary<int, int> BoldPrefixLengths { get; } = [];

    internal LowMemoryWorkbook()
    {
        Package = Own(FileBackedList<string>.CreateTemporaryStream());
        var initialized = false;
        try
        {
            SharedStrings = Own(new FileBackedList<string>(reader => reader.ReadString(), (writer, text) => writer.Write(text)));
            initialized = true;
        }
        finally
        {
            if (!initialized) Package.Dispose();
        }
    }

    private T Own<T>(T resource) where T : IDisposable
    {
        _resources.Add(resource);
        return resource;
    }

    internal FileBackedList<List<ExcelCell>> CreateRows()
        => Own(new FileBackedList<List<ExcelCell>>(LowMemorySerialization.ReadRow, LowMemorySerialization.WriteRow));

    internal IEnumerable<string> GetRows(string path) => _rows[path];

    internal void Prepare(Stream input)
    {
        using var source = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
        using (var target = new ZipArchive(Package, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var entry in source.Entries)
            {
                using var entryStream = entry.Open();
                if (entry.FullName == "xl/sharedStrings.xml")
                {
                    ReadSharedStrings(entryStream);
                    continue;
                }
                var outputEntry = target.CreateEntry(entry.FullName);
                using var output = outputEntry.Open();
                if (entry.FullName.StartsWith("xl/worksheets/", StringComparison.Ordinal) &&
                    entry.FullName.EndsWith(".xml", StringComparison.Ordinal) &&
                    !entry.FullName.Contains("/_rels/"))
                {
                    var rows = Own(new FileBackedList<string>(reader => reader.ReadString(), (writer, text) => writer.Write(text)));
                    _rows.Add(entry.FullName, rows);
                    CopyWorksheet(entryStream, output, rows);
                }
                else if (entry.FullName == "xl/workbook.xml")
                {
                    var workbook = XDocument.Load(entryStream);
                    foreach (var name in workbook.Descendants().Where(element => element.Name.LocalName == "definedName"))
                    {
                        var value = name.Attribute("name")?.Value;
                        if (value is "_xlnm.Print_Area" or "_xlnm.Print_Titles")
                            Unsupported(value);
                    }
                    workbook.Save(output);
                }
                else
                {
                    entryStream.CopyTo(output);
                }
            }
        }
        Package.Position = 0;
    }

    private void ReadSharedStrings(Stream input)
    {
        using var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        reader.Read();
        while (!reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "si")
            {
                var item = (XElement)XNode.ReadFrom(reader);
                var (text, boldPrefix) = ExcelReader.ReadSharedString(item);
                if (boldPrefix > 0) BoldPrefixLengths[SharedStrings.Count] = boldPrefix;
                SharedStrings.Add(text);
            }
            else reader.Read();
        }
    }

    private static void CopyWorksheet(Stream input, Stream output, FileBackedList<string> rows)
    {
        using var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        using var writer = XmlWriter.Create(output, new XmlWriterSettings { CloseOutput = false });
        reader.Read();
        var lastRow = 0;
        while (!reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "mergeCells":
                    case "conditionalFormatting":
                    case "drawing":
                    case "legacyDrawing":
                    case "legacyDrawingHF":
                    case "tableParts":
                    case "hyperlinks":
                    case "headerFooter":
                    case "extLst":
                        Unsupported(reader.LocalName);
                        break;
                    case "printOptions":
                        if (reader.GetAttribute("verticalCentered") is "1" or "true")
                            Unsupported("vertical centering");
                        break;
                    case "row":
                        var row = (XElement)XNode.ReadFrom(reader);
                        if (row.Attribute("ht") != null || row.Attribute("hidden") is { Value: "1" or "true" } ||
                            row.Attribute("customHeight") is { Value: "1" or "true" })
                            Unsupported("custom or hidden row heights");
                        if (row.Descendants().Any(element => element.Name.LocalName == "f"))
                            Unsupported("formulas");
                        var rowNumber = row.Attribute("r") == null ? lastRow + 1 : (int)row.Attribute("r")!;
                        if (rowNumber <= lastRow || rowNumber > 1048576)
                            throw new InvalidDataException("Worksheet rows must be ordered and within the XLSX row limit.");
                        while (++lastRow < rowNumber) rows.Add(string.Empty);
                        // Each row is parsed independently; its original position is kept by the disk index.
                        row.SetAttributeValue("r", 1);
                        rows.Add(row.ToString(SaveOptions.DisableFormatting));
                        continue;
                }
                writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                writer.WriteAttributes(reader, defattr: false);
                if (reader.IsEmptyElement) writer.WriteEndElement();
            }
            else if (reader.NodeType == XmlNodeType.EndElement) writer.WriteFullEndElement();
            else if (reader.NodeType is XmlNodeType.Text or XmlNodeType.SignificantWhitespace or XmlNodeType.Whitespace)
                writer.WriteString(reader.Value);
            else if (reader.NodeType == XmlNodeType.CDATA) writer.WriteCData(reader.Value);
            else if (reader.NodeType == XmlNodeType.Comment) writer.WriteComment(reader.Value);
            else if (reader.NodeType == XmlNodeType.ProcessingInstruction)
                writer.WriteProcessingInstruction(reader.Name, reader.Value);
            reader.Read();
        }
    }

    internal static void Unsupported(string feature)
        => throw new NotSupportedException($"XLSX LowMemory mode does not support {feature}. Disable LowMemory to use the existing converter.");

    public void Dispose()
    {
        for (var i = _resources.Count - 1; i >= 0; i--) _resources[i].Dispose();
    }
}
