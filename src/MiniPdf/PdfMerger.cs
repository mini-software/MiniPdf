using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniSoftware;

internal static class PdfMerger
{
    private static readonly Regex ReferenceRegex = new(@"\b(\d+)\s+0\s+R\b", RegexOptions.Compiled);
    private static readonly Regex RootRegex = new(@"/Root\s+(\d+)\s+0\s+R", RegexOptions.Compiled);
    private static readonly Regex PagesRegex = new(@"/Pages\s+(\d+)\s+0\s+R", RegexOptions.Compiled);
    private static readonly Regex KidsRegex = new(@"/Kids\s*\[(.*?)\]", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex PageTypeRegex = new(@"/Type\s*/Page(?!s)\b", RegexOptions.Compiled);
    private static readonly Regex PagesTypeRegex = new(@"/Type\s*/Pages\b", RegexOptions.Compiled);
    private static readonly Regex ParentRegex = new(@"/Parent\s+\d+\s+0\s+R", RegexOptions.Compiled);
    private static readonly string[] InheritablePageEntries = ["/Resources", "/MediaBox", "/CropBox", "/Rotate"];

    internal static byte[] Merge(IReadOnlyList<byte[]> inputPdfs, PdfMergeOptions? options)
    {
        if (inputPdfs.Count == 0)
            throw new ArgumentException("At least one input PDF is required.", nameof(inputPdfs));

        var sources = new List<PdfSourceDocument>(inputPdfs.Count);
        for (var i = 0; i < inputPdfs.Count; i++)
            sources.Add(PdfSourceDocument.Parse(inputPdfs[i], i));

        var cloneOrder = new List<ObjectKey>();
        var objectMap = new Dictionary<ObjectKey, int>();
        var newPageObjectNumbers = new List<int>();
        var nextObjectNumber = 3;

        for (var sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
        {
            var source = sources[sourceIndex];
            foreach (var pageObjectId in source.PageObjectIds)
            {
                CollectObject(source, pageObjectId, cloneOrder, objectMap, ref nextObjectNumber);
                newPageObjectNumbers.Add(objectMap[new ObjectKey(sourceIndex, pageObjectId)]);
            }
        }

        var bookmarks = ResolveBookmarks(options, sources);
        var outlineRootObjectNumber = bookmarks.Count > 0 ? nextObjectNumber++ : 0;
        var bookmarkObjectNumbers = new List<int>(bookmarks.Count);
        for (var i = 0; i < bookmarks.Count; i++)
            bookmarkObjectNumbers.Add(nextObjectNumber++);

        var objectCount = nextObjectNumber - 1;
        var offsets = new long[objectCount + 1];

        using var output = new MemoryStream();
        void WriteRaw(string text)
        {
            var bytes = Compat.Latin1.GetBytes(text);
            output.Write(bytes, 0, bytes.Length);
        }

        WriteRaw("%PDF-1.4\n");
        WriteRaw("%\xe2\xe3\xcf\xd3\n");

        offsets[1] = output.Position;
        WriteRaw("1 0 obj\n");
        if (outlineRootObjectNumber > 0)
            WriteRaw($"<< /Type /Catalog /Pages 2 0 R /Outlines {outlineRootObjectNumber} 0 R /PageMode /UseOutlines >>\n");
        else
            WriteRaw("<< /Type /Catalog /Pages 2 0 R >>\n");
        WriteRaw("endobj\n");

        offsets[2] = output.Position;
        var kids = string.Join(" ", newPageObjectNumbers.Select(n => $"{n} 0 R"));
        WriteRaw($"2 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {newPageObjectNumbers.Count} >>\nendobj\n");

        foreach (var key in cloneOrder)
        {
            var source = sources[key.SourceIndex];
            var newObjectNumber = objectMap[key];
            var body = source.Objects[key.ObjectId];
            var rewrittenBody = RewriteObjectBody(source, key, body, objectMap);

            offsets[newObjectNumber] = output.Position;
            WriteRaw($"{newObjectNumber} 0 obj\n");
            output.Write(rewrittenBody, 0, rewrittenBody.Length);
            WriteRaw("\nendobj\n");
        }

        if (bookmarks.Count > 0)
        {
            offsets[outlineRootObjectNumber] = output.Position;
            WriteRaw($"{outlineRootObjectNumber} 0 obj\n");
            WriteRaw($"<< /Type /Outlines /First {bookmarkObjectNumbers[0]} 0 R /Last {bookmarkObjectNumbers[^1]} 0 R /Count {bookmarks.Count} >>\n");
            WriteRaw("endobj\n");

            for (var i = 0; i < bookmarks.Count; i++)
            {
                var bookmark = bookmarks[i];
                var pageObjectNumber = newPageObjectNumbers[bookmark.PageIndex];
                var previous = i > 0 ? $" /Prev {bookmarkObjectNumbers[i - 1]} 0 R" : "";
                var next = i < bookmarks.Count - 1 ? $" /Next {bookmarkObjectNumbers[i + 1]} 0 R" : "";

                offsets[bookmarkObjectNumbers[i]] = output.Position;
                WriteRaw($"{bookmarkObjectNumbers[i]} 0 obj\n");
                WriteRaw($"<< /Title ({EscapePdfString(bookmark.Title)}) /Parent {outlineRootObjectNumber} 0 R{previous}{next} /Dest [{pageObjectNumber} 0 R /Fit] >>\n");
                WriteRaw("endobj\n");
            }
        }

        var xrefOffset = output.Position;
        WriteRaw("xref\n");
        WriteRaw($"0 {objectCount + 1}\n");
        WriteRaw("0000000000 65535 f \n");
        for (var i = 1; i <= objectCount; i++)
            WriteRaw($"{offsets[i]:D10} 00000 n \n");

        WriteRaw("trailer\n");
        WriteRaw($"<< /Size {objectCount + 1} /Root 1 0 R >>\n");
        WriteRaw("startxref\n");
        WriteRaw($"{xrefOffset}\n");
        WriteRaw("%%EOF\n");

        return output.ToArray();
    }

    private static void CollectObject(PdfSourceDocument source, int objectId, List<ObjectKey> cloneOrder,
        Dictionary<ObjectKey, int> objectMap, ref int nextObjectNumber)
    {
        if (!source.Objects.ContainsKey(objectId))
            throw new InvalidDataException($"PDF object {objectId} was referenced but not found.");

        var key = new ObjectKey(source.SourceIndex, objectId);
        if (objectMap.ContainsKey(key))
            return;

        objectMap[key] = nextObjectNumber++;
        cloneOrder.Add(key);

        var bodyText = Compat.Latin1.GetString(source.Objects[objectId]);
        if (source.PageObjectIds.Contains(objectId))
        {
            bodyText = ParentRegex.Replace(bodyText, "");
            if (source.InheritedPageEntries.TryGetValue(objectId, out var inheritedEntries))
                bodyText = InsertMissingInheritedEntries(bodyText, inheritedEntries);
        }

        var scanText = DictionaryPart(bodyText);
        foreach (Match match in ReferenceRegex.Matches(scanText))
        {
            var referencedObjectId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            if (referencedObjectId == source.CatalogObjectId || source.PageTreeObjectIds.Contains(referencedObjectId))
                continue;

            CollectObject(source, referencedObjectId, cloneOrder, objectMap, ref nextObjectNumber);
        }
    }

    private static byte[] RewriteObjectBody(PdfSourceDocument source, ObjectKey key, byte[] body,
        Dictionary<ObjectKey, int> objectMap)
    {
        var text = Compat.Latin1.GetString(body);

        if (source.PageObjectIds.Contains(key.ObjectId))
        {
            if (ParentRegex.IsMatch(text))
                text = ParentRegex.Replace(text, "/Parent 2 0 R", 1);
            else
                text = InsertIntoDictionary(text, "/Parent 2 0 R");

            if (source.InheritedPageEntries.TryGetValue(key.ObjectId, out var inheritedEntries))
                text = InsertMissingInheritedEntries(text, inheritedEntries);
        }

        text = RewriteDictionaryReferences(text, match =>
        {
            var oldObjectId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            if (oldObjectId == source.CatalogObjectId || source.PageTreeObjectIds.Contains(oldObjectId))
                return match.Value;
            var referenceKey = new ObjectKey(key.SourceIndex, oldObjectId);
            return objectMap.TryGetValue(referenceKey, out var newObjectId)
                ? $"{newObjectId} 0 R"
                : match.Value;
        });

        return Compat.Latin1.GetBytes(text);
    }

    private static string RewriteDictionaryReferences(string objectBody, MatchEvaluator evaluator)
    {
        var streamIndex = objectBody.IndexOf("stream", StringComparison.Ordinal);
        if (streamIndex < 0)
            return ReferenceRegex.Replace(objectBody, evaluator);

        var dictionary = objectBody.Substring(0, streamIndex);
        var streamAndData = objectBody.Substring(streamIndex);
        return ReferenceRegex.Replace(dictionary, evaluator) + streamAndData;
    }

    private static List<PdfBookmark> ResolveBookmarks(PdfMergeOptions? options, List<PdfSourceDocument> sources)
    {
        var result = new List<PdfBookmark>();
        if (options == null)
            return result;

        var sourceStartPages = new int[sources.Count];
        var runningPageCount = 0;
        for (var i = 0; i < sources.Count; i++)
        {
            sourceStartPages[i] = runningPageCount;
            runningPageCount += sources[i].PageObjectIds.Count;
        }

        if (options.BookmarkTitles != null)
        {
            if (options.BookmarkTitles.Count != sources.Count)
                throw new ArgumentException("BookmarkTitles must contain one title per input PDF.", nameof(options));

            for (var i = 0; i < options.BookmarkTitles.Count; i++)
                result.Add(new PdfBookmark(options.BookmarkTitles[i], sourceStartPages[i]));
        }

        if (options.Bookmarks != null)
        {
            foreach (var bookmark in options.Bookmarks)
            {
                if (bookmark.PageIndex >= runningPageCount)
                    throw new ArgumentOutOfRangeException(nameof(options), "Bookmark page index is outside the merged PDF page range.");
                result.Add(bookmark);
            }
        }

        return result;
    }

    private static string DictionaryPart(string objectBody)
    {
        var streamIndex = objectBody.IndexOf("stream", StringComparison.Ordinal);
        return streamIndex >= 0 ? objectBody.Substring(0, streamIndex) : objectBody;
    }

    private static string InsertIntoDictionary(string objectBody, string entry)
    {
        var insertAt = objectBody.LastIndexOf(">>", StringComparison.Ordinal);
        if (insertAt < 0)
            throw new InvalidDataException("PDF page object dictionary is malformed.");
        return objectBody.Insert(insertAt, entry + "\n");
    }

    private static string InsertMissingInheritedEntries(string objectBody, Dictionary<string, string> inheritedEntries)
    {
        if (inheritedEntries.Count == 0)
            return objectBody;

        var additions = new StringBuilder();
        var dictionary = DictionaryPart(objectBody);
        foreach (var entryName in InheritablePageEntries)
        {
            if (inheritedEntries.TryGetValue(entryName, out var entryValue) && !ContainsDictionaryEntry(dictionary, entryName))
                additions.Append(entryName).Append(' ').Append(entryValue).Append('\n');
        }

        return additions.Length == 0
            ? objectBody
            : InsertIntoDictionary(objectBody, additions.ToString());
    }

    private static bool ContainsDictionaryEntry(string dictionary, string entryName)
        => Regex.IsMatch(dictionary, Regex.Escape(entryName) + @"(?![A-Za-z0-9])");

    private static Dictionary<string, string> MergeInheritedEntries(string dictionary, Dictionary<string, string> inherited)
    {
        var merged = new Dictionary<string, string>(inherited, StringComparer.Ordinal);
        foreach (var entryName in InheritablePageEntries)
            if (TryReadDictionaryEntry(dictionary, entryName, out var value))
                merged[entryName] = value;
        return merged;
    }

    private static bool TryReadDictionaryEntry(string dictionary, string entryName, out string value)
    {
        value = "";
        var match = Regex.Match(dictionary, Regex.Escape(entryName) + @"\s+");
        if (!match.Success)
            return false;

        var start = match.Index + match.Length;
        while (start < dictionary.Length && char.IsWhiteSpace(dictionary[start]))
            start++;

        if (start >= dictionary.Length)
            return false;

        int end;
        if (StartsWithAt(dictionary, start, "<<"))
        {
            end = FindBalancedDictionaryEnd(dictionary, start);
        }
        else if (dictionary[start] == '[')
        {
            end = FindBalancedArrayEnd(dictionary, start);
        }
        else
        {
            var indirectMatch = Regex.Match(dictionary.Substring(start), @"^\d+\s+\d+\s+R\b");
            if (indirectMatch.Success)
            {
                value = indirectMatch.Value;
                return true;
            }

            end = start;
            while (end < dictionary.Length && !char.IsWhiteSpace(dictionary[end]) && dictionary[end] != '>')
                end++;
        }

        if (end <= start || end > dictionary.Length)
            return false;

        value = dictionary.Substring(start, end - start).Trim();
        return value.Length > 0;
    }

    private static bool StartsWithAt(string text, int index, string value)
        => index + value.Length <= text.Length && string.CompareOrdinal(text, index, value, 0, value.Length) == 0;

    private static int FindBalancedDictionaryEnd(string text, int start)
    {
        var depth = 0;
        for (var i = start; i < text.Length - 1; i++)
        {
            if (text[i] == '<' && text[i + 1] == '<')
            {
                depth++;
                i++;
            }
            else if (text[i] == '>' && text[i + 1] == '>')
            {
                depth--;
                i++;
                if (depth == 0)
                    return i + 1;
            }
        }

        throw new InvalidDataException("PDF dictionary entry is malformed.");
    }

    private static int FindBalancedArrayEnd(string text, int start)
    {
        var depth = 0;
        for (var i = start; i < text.Length; i++)
        {
            if (text[i] == '[')
                depth++;
            else if (text[i] == ']')
            {
                depth--;
                if (depth == 0)
                    return i + 1;
            }
        }

        throw new InvalidDataException("PDF array entry is malformed.");
    }

    private static string EscapePdfString(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\': sb.Append("\\\\"); break;
                case '(' : sb.Append("\\("); break;
                case ')' : sb.Append("\\)"); break;
                case '\r': sb.Append("\\r"); break;
                case '\n': sb.Append("\\n"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(ch); break;
            }
        }

        return sb.ToString();
    }

    private readonly struct ObjectKey : IEquatable<ObjectKey>
    {
        public ObjectKey(int sourceIndex, int objectId)
        {
            SourceIndex = sourceIndex;
            ObjectId = objectId;
        }

        public int SourceIndex { get; }
        public int ObjectId { get; }

        public bool Equals(ObjectKey other)
            => SourceIndex == other.SourceIndex && ObjectId == other.ObjectId;

        public override bool Equals(object? obj)
            => obj is ObjectKey other && Equals(other);

        public override int GetHashCode()
            => Compat.HashCombine(SourceIndex, ObjectId, 0);
    }

    private sealed class PdfSourceDocument
    {
        private PdfSourceDocument(int sourceIndex, Dictionary<int, byte[]> objects, int catalogObjectId,
            List<int> pageObjectIds, HashSet<int> pageTreeObjectIds, Dictionary<int, Dictionary<string, string>> inheritedPageEntries)
        {
            SourceIndex = sourceIndex;
            Objects = objects;
            CatalogObjectId = catalogObjectId;
            PageObjectIds = pageObjectIds;
            PageTreeObjectIds = pageTreeObjectIds;
            InheritedPageEntries = inheritedPageEntries;
        }

        public int SourceIndex { get; }
        public Dictionary<int, byte[]> Objects { get; }
        public int CatalogObjectId { get; }
        public List<int> PageObjectIds { get; }
        public HashSet<int> PageTreeObjectIds { get; }
        public Dictionary<int, Dictionary<string, string>> InheritedPageEntries { get; }

        public static PdfSourceDocument Parse(byte[] bytes, int sourceIndex)
        {
            var text = Compat.Latin1.GetString(bytes);
            if (text.Contains("/Encrypt", StringComparison.Ordinal))
                throw new NotSupportedException("Encrypted PDF files cannot be merged.");

            var xrefOffsets = ParseXrefOffsets(text);
            var objects = new Dictionary<int, byte[]>();
            foreach (var pair in xrefOffsets)
                objects[pair.Key] = ExtractObjectBody(bytes, text, pair.Key, pair.Value);

            var rootMatch = RootRegex.Match(text);
            if (!rootMatch.Success)
                throw new InvalidDataException("PDF trailer does not contain a root catalog.");
            var catalogObjectId = int.Parse(rootMatch.Groups[1].Value, CultureInfo.InvariantCulture);

            if (!objects.TryGetValue(catalogObjectId, out var catalogBody))
                throw new InvalidDataException("PDF root catalog object was not found.");

            var catalogText = Compat.Latin1.GetString(catalogBody);
            var pagesMatch = PagesRegex.Match(catalogText);
            if (!pagesMatch.Success)
                throw new InvalidDataException("PDF catalog does not contain a page tree.");

            var pageTreeRootId = int.Parse(pagesMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            var pageObjectIds = new List<int>();
            var pageTreeObjectIds = new HashSet<int>();
            var inheritedPageEntries = new Dictionary<int, Dictionary<string, string>>();
            TraversePages(objects, pageTreeRootId, new Dictionary<string, string>(StringComparer.Ordinal),
                pageObjectIds, pageTreeObjectIds, inheritedPageEntries);

            if (pageObjectIds.Count == 0)
                throw new InvalidDataException("PDF contains no pages to merge.");

            return new PdfSourceDocument(sourceIndex, objects, catalogObjectId, pageObjectIds, pageTreeObjectIds, inheritedPageEntries);
        }

        private static Dictionary<int, long> ParseXrefOffsets(string text)
        {
            var startXrefIndex = text.LastIndexOf("startxref", StringComparison.Ordinal);
            if (startXrefIndex < 0)
                throw new NotSupportedException("PDF does not contain a classic xref table.");

            var startXrefMatch = Regex.Match(text.Substring(startXrefIndex), @"startxref\s+(\d+)");
            if (!startXrefMatch.Success)
                throw new InvalidDataException("PDF startxref value is malformed.");

            var xrefOffset = int.Parse(startXrefMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            if (xrefOffset < 0 || xrefOffset >= text.Length || !text.Substring(xrefOffset).StartsWith("xref", StringComparison.Ordinal))
                throw new NotSupportedException("PDF xref streams are not supported by the built-in merger.");

            var result = new Dictionary<int, long>();
            using var reader = new StringReader(text.Substring(xrefOffset));
            var line = reader.ReadLine();
            if (line != "xref")
                throw new InvalidDataException("PDF xref table is malformed.");

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("trailer", StringComparison.Ordinal))
                    break;
                if (line.Length == 0)
                    continue;

                var headerParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (headerParts.Length != 2)
                    throw new InvalidDataException("PDF xref subsection header is malformed.");

                var firstObject = int.Parse(headerParts[0], CultureInfo.InvariantCulture);
                var objectCount = int.Parse(headerParts[1], CultureInfo.InvariantCulture);
                for (var i = 0; i < objectCount; i++)
                {
                    var entry = reader.ReadLine();
                    if (entry == null || entry.Length < 17)
                        throw new InvalidDataException("PDF xref entry is malformed.");

                    var inUse = entry.Length > 17 ? entry[17] : entry[entry.Length - 1];
                    if (inUse == 'n')
                    {
                        var objectId = firstObject + i;
                        var offsetText = entry.Substring(0, 10);
                        result[objectId] = long.Parse(offsetText, CultureInfo.InvariantCulture);
                    }
                }
            }

            return result;
        }

        private static byte[] ExtractObjectBody(byte[] bytes, string text, int objectId, long offset)
        {
            var objectOffset = checked((int)offset);
            var headerEnd = text.IndexOf(" obj", objectOffset, StringComparison.Ordinal);
            if (headerEnd < 0 || headerEnd - objectOffset > 40)
                throw new InvalidDataException($"PDF object {objectId} header was not found at the xref offset.");

            var bodyStart = headerEnd + 4;
            if (bodyStart < text.Length && text[bodyStart] == '\r') bodyStart++;
            if (bodyStart < text.Length && text[bodyStart] == '\n') bodyStart++;

            var bodyEnd = text.IndexOf("endobj", bodyStart, StringComparison.Ordinal);
            if (bodyEnd < 0)
                throw new InvalidDataException($"PDF object {objectId} does not have an endobj marker.");

            while (bodyEnd > bodyStart && (text[bodyEnd - 1] == '\r' || text[bodyEnd - 1] == '\n'))
                bodyEnd--;

            var body = new byte[bodyEnd - bodyStart];
            Array.Copy(bytes, bodyStart, body, 0, body.Length);
            return body;
        }

        private static void TraversePages(Dictionary<int, byte[]> objects, int objectId, Dictionary<string, string> inherited,
            List<int> pageObjectIds, HashSet<int> pageTreeObjectIds, Dictionary<int, Dictionary<string, string>> inheritedPageEntries)
        {
            if (!objects.TryGetValue(objectId, out var body))
                throw new InvalidDataException($"PDF page tree object {objectId} was not found.");

            var text = DictionaryPart(Compat.Latin1.GetString(body));
            if (PagesTypeRegex.IsMatch(text))
            {
                pageTreeObjectIds.Add(objectId);
                var mergedInherited = MergeInheritedEntries(text, inherited);
                var kidsMatch = KidsRegex.Match(text);
                if (!kidsMatch.Success)
                    throw new InvalidDataException("PDF page tree node does not contain /Kids.");

                foreach (Match kidMatch in ReferenceRegex.Matches(kidsMatch.Groups[1].Value))
                {
                    var kidObjectId = int.Parse(kidMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    TraversePages(objects, kidObjectId, mergedInherited, pageObjectIds, pageTreeObjectIds, inheritedPageEntries);
                }
            }
            else if (PageTypeRegex.IsMatch(text))
            {
                pageObjectIds.Add(objectId);
                inheritedPageEntries[objectId] = MergeInheritedEntries(text, inherited);
            }
            else
            {
                throw new InvalidDataException($"PDF page tree kid {objectId} is neither /Page nor /Pages.");
            }
        }
    }
}
