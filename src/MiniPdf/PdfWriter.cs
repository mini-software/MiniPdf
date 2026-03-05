using System.Globalization;
using System.Text;

namespace MiniSoftware;

/// <summary>
/// Low-level PDF writer. Produces valid PDF 1.4 output with Helvetica font.
/// Supports embedded JPEG and PNG images via PDF Image XObjects.
/// </summary>
internal sealed class PdfWriter
{
    private readonly Stream _stream;
    private readonly List<long> _objectOffsets = [];
    private int _objectCount;

    internal PdfWriter(Stream stream)
    {
        _stream = stream;
    }

    internal void Write(PdfDocument document)
    {
        // PDF Header
        WriteRaw("%PDF-1.4\n");
        // Binary comment to signal binary content (recommended by spec)
        WriteRaw("%\xe2\xe3\xcf\xd3\n");

        var pages = document.Pages;
        var pageCount = pages.Count;

        // Detect whether any page needs non-Latin1 Unicode characters.
        // If so we'll add a second font (F2) using Identity-H CIDFont with ToUnicode CMap.
        // When a text block contains ANY non-WinAnsi char, the ENTIRE block is rendered
        // in F2 so all spans share the same bbox Y in text extractors.  Therefore we
        // collect ALL characters from such blocks (including ASCII) for the ToUnicode CMap.
        var unicodeChars = new SortedSet<int>();
        foreach (var page in pages)
            foreach (var block in page.TextBlocks)
            {
                bool blockNeedsUnicode = false;
                foreach (var ch in block.Text)
                    if (!IsWinAnsiHandled(ch)) { blockNeedsUnicode = true; break; }
                if (blockNeedsUnicode)
                    foreach (var ch in block.Text)
                        unicodeChars.Add(ch);
            }
        var needsUnicodeFont = unicodeChars.Count > 0;

        // Try to find and embed a system CJK font so Unicode text renders correctly.
        byte[]? ttfFontData = null;
        Dictionary<int, ushort>? fontCmap = null;
        byte[]? compressedFontData = null;
        byte[]? cidToGidMapData = null;
        string? wArrayString = null;
        int fontAscent = 718, fontDescent = -207, fontCapHeight = 718;
        int[] fontBbox = [-166, -225, 1000, 931];
        var fontEmbedded = false;
        var fontUncompressedLength = 0;

        if (needsUnicodeFont)
        {
            var fontPath = FindSystemCjkFont();
            if (fontPath != null)
            {
                try
                {
                    ttfFontData = LoadTtfFont(fontPath);
                    fontCmap = ParseCmapTable(ttfFontData);
                    if (fontCmap.Count > 0)
                    {
                        var (advances, upm) = ParseHmtxWidths(ttfFontData);
                        var (asc, desc, capH, bbox) = ParseFontMetrics(ttfFontData);
                        var scale = 1000.0 / upm;
                        fontAscent = (int)(asc * scale);
                        fontDescent = (int)(desc * scale);
                        fontCapHeight = (int)(capH * scale);
                        fontBbox = [.. bbox.Select(v => (int)(v * scale))];

                        cidToGidMapData = BuildCidToGidMap(fontCmap);
                        wArrayString = BuildWArray(unicodeChars, fontCmap, advances, upm);

                        // Subset the font: zero out unused glyph outlines to reduce size
                        var neededGlyphs = new HashSet<ushort> { 0 }; // always keep .notdef
                        foreach (var cp in unicodeChars)
                            if (fontCmap.TryGetValue(cp, out var gid))
                                neededGlyphs.Add(gid);
                        var subsetFont = SubsetTtfFont(ttfFontData, neededGlyphs);

                        using var ms = new System.IO.MemoryStream();
                        using (var zlib = new System.IO.Compression.ZLibStream(ms, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
                            zlib.Write(subsetFont, 0, subsetFont.Length);
                        compressedFontData = ms.ToArray();
                        fontUncompressedLength = subsetFont.Length;
                        fontEmbedded = true;
                    }
                }
                catch
                {
                    // Font loading/parsing failed – fall back to non-embedded
                    fontEmbedded = false;
                }
            }
        }

        // Pre-build content streams
        var contentStreams = new List<byte[]>(pageCount);
        for (var i = 0; i < pageCount; i++)
            contentStreams.Add(Encoding.Latin1.GetBytes(BuildContentStream(pages[i], needsUnicodeFont)));

        // Allocate object numbers.
        //   1 = Catalog, 2 = Pages, 3 = Font F1 (Helvetica/WinAnsi)
        //   When Unicode: 4 = ToUnicode CMap, 5 = FontDescriptor, 6 = CIDFont, 7 = Type0 font F2
        //   When font embedded: 8 = FontFile2 stream, 9 = CIDToGIDMap stream
        //   Per page: content stream obj, N image XObject objs, page obj
        var nextObj = needsUnicodeFont ? (fontEmbedded ? 10 : 8) : 4;
        var contentObjNums = new List<int>(pageCount);
        var imageObjNums = new List<List<int>>(pageCount);
        var pageObjNums = new List<int>(pageCount);

        for (var i = 0; i < pageCount; i++)
        {
            contentObjNums.Add(nextObj++);            // content stream
            var imgNums = new List<int>();
            for (var j = 0; j < pages[i].ImageBlocks.Count; j++)
                imgNums.Add(nextObj++);               // image XObject
            imageObjNums.Add(imgNums);
            pageObjNums.Add(nextObj++);               // page dict
        }

        _objectCount = nextObj - 1;
        _objectOffsets.Clear();
        for (var i = 0; i <= _objectCount; i++)
            _objectOffsets.Add(0);

        // ── Object 1: Catalog ──────────────────────────────────────────────
        _objectOffsets[1] = Position;
        WriteRaw("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        // ── Object 2: Pages ────────────────────────────────────────────────
        _objectOffsets[2] = Position;
        var kids = string.Join(" ", pageObjNums.Select(n => $"{n} 0 R"));
        WriteRaw($"2 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {pageCount} >>\nendobj\n");

        // ── Object 3: Font F1 (Helvetica, built-in WinAnsiEncoding) ────────
        _objectOffsets[3] = Position;
        WriteRaw("3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

        // ── Objects 4-6: Unicode font (only if needed) ─────────────────────
        if (needsUnicodeFont)
        {
            // Object 4: ToUnicode CMap stream
            _objectOffsets[4] = Position;
            var toUnicode = BuildToUnicodeCMap(unicodeChars);
            var toUnicodeBytes = Encoding.ASCII.GetBytes(toUnicode);
            WriteRaw($"4 0 obj\n<< /Length {toUnicodeBytes.Length} >>\nstream\n");
            _stream.Write(toUnicodeBytes);
            WriteRaw("\nendstream\nendobj\n");

            // Object 5: Font descriptor (required by PDF spec for CIDFontType2)
            _objectOffsets[5] = Position;
            WriteRaw("5 0 obj\n");
            WriteRaw("<< /Type /FontDescriptor\n");
            WriteRaw("/FontName /Arial\n");
            WriteRaw("/Flags 32\n");
            WriteRaw($"/FontBBox [{fontBbox[0]} {fontBbox[1]} {fontBbox[2]} {fontBbox[3]}]\n");
            WriteRaw("/ItalicAngle 0\n");
            WriteRaw($"/Ascent {fontAscent}\n");
            WriteRaw($"/Descent {fontDescent}\n");
            WriteRaw($"/CapHeight {fontCapHeight}\n");
            WriteRaw("/StemV 80\n");
            if (fontEmbedded)
                WriteRaw("/FontFile2 8 0 R\n");
            WriteRaw(">>\n");
            WriteRaw("endobj\n");

            // Object 6: CIDFont dict (Type2, references descriptor)
            _objectOffsets[6] = Position;
            WriteRaw("6 0 obj\n");
            WriteRaw("<< /Type /Font /Subtype /CIDFontType2\n");
            WriteRaw("/BaseFont /Arial\n");
            WriteRaw("/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >>\n");
            WriteRaw("/FontDescriptor 5 0 R\n");
            WriteRaw("/DW 1000\n");
            if (fontEmbedded)
            {
                WriteRaw($"/W {wArrayString}\n");
                WriteRaw("/CIDToGIDMap 9 0 R\n");
            }
            WriteRaw(">>\n");
            WriteRaw("endobj\n");

            // Object 7: Type0 font F2 (composite Unicode font)
            _objectOffsets[7] = Position;
            WriteRaw("7 0 obj\n");
            WriteRaw("<< /Type /Font /Subtype /Type0\n");
            WriteRaw("/BaseFont /Arial\n");
            WriteRaw("/Encoding /Identity-H\n");
            WriteRaw("/DescendantFonts [6 0 R]\n");
            WriteRaw("/ToUnicode 4 0 R\n");
            WriteRaw(">>\n");
            WriteRaw("endobj\n");
            // Objects 8-9: Embedded font data (only when font found)
            if (fontEmbedded)
            {
                // Object 8: FontFile2 (compressed TrueType font program)
                _objectOffsets[8] = Position;
                WriteRaw($"8 0 obj\n");
                WriteRaw($"<< /Length {compressedFontData!.Length} /Length1 {fontUncompressedLength} /Filter /FlateDecode >>\n");
                WriteRaw("stream\n");
                _stream.Write(compressedFontData);
                WriteRaw("\nendstream\nendobj\n");

                // Object 9: CIDToGIDMap stream
                _objectOffsets[9] = Position;
                WriteRaw($"9 0 obj\n");
                WriteRaw($"<< /Length {cidToGidMapData!.Length} /Filter /FlateDecode >>\n");
                WriteRaw("stream\n");
                _stream.Write(cidToGidMapData);
                WriteRaw("\nendstream\nendobj\n");
            }        }

        // ── Per-page objects ───────────────────────────────────────────────
        for (var i = 0; i < pageCount; i++)
        {
            var page = pages[i];

            // Content stream
            var content = contentStreams[i];
            _objectOffsets[contentObjNums[i]] = Position;
            WriteRaw($"{contentObjNums[i]} 0 obj\n<< /Length {content.Length} >>\nstream\n");
            _stream.Write(content);
            WriteRaw("\nendstream\nendobj\n");

            // Image XObjects
            for (var j = 0; j < page.ImageBlocks.Count; j++)
            {
                _objectOffsets[imageObjNums[i][j]] = Position;
                WriteImageXObject(imageObjNums[i][j], page.ImageBlocks[j]);
            }

            // Page dictionary
            var w = page.Width.ToString(CultureInfo.InvariantCulture);
            var h = page.Height.ToString(CultureInfo.InvariantCulture);
            _objectOffsets[pageObjNums[i]] = Position;
            WriteRaw($"{pageObjNums[i]} 0 obj\n");
            WriteRaw($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {w} {h}]\n");
            WriteRaw($"/Contents {contentObjNums[i]} 0 R\n");
            WriteRaw("/Resources <<\n");
            if (needsUnicodeFont)
                WriteRaw("/Font << /F1 3 0 R /F2 7 0 R >>\n");
            else
                WriteRaw("/Font << /F1 3 0 R >>\n");
            if (imageObjNums[i].Count > 0)
            {
                WriteRaw("/XObject <<\n");
                for (var j = 0; j < imageObjNums[i].Count; j++)
                    WriteRaw($"/Im{j} {imageObjNums[i][j]} 0 R\n");
                WriteRaw(">>\n");
            }
            WriteRaw(">>\n");
            WriteRaw(">>\nendobj\n");
        }

        // ── Cross-reference table ──────────────────────────────────────────
        var xrefOffset = Position;
        WriteRaw("xref\n");
        WriteRaw($"0 {_objectCount + 1}\n");
        WriteRaw("0000000000 65535 f \n");
        for (var i = 1; i <= _objectCount; i++)
            WriteRaw($"{_objectOffsets[i]:D10} 00000 n \n");

        // ── Trailer ────────────────────────────────────────────────────────
        WriteRaw("trailer\n");
        WriteRaw($"<< /Size {_objectCount + 1} /Root 1 0 R >>\n");
        WriteRaw("startxref\n");
        WriteRaw($"{xrefOffset}\n");
        WriteRaw("%%EOF\n");

        _stream.Flush();
    }

    /// <summary>
    /// Writes a PDF Image XObject stream for a JPEG or PNG image.
    /// JPEG uses native /DCTDecode; PNG raw-RGB bytes use /FlateDecode.
    /// </summary>
    private void WriteImageXObject(int objNum, PdfImageBlock img)
    {
        byte[] pixelData;
        int width, height;
        string dictExtras;

        var isJpeg = img.Format is "jpg" or "jpeg";

        if (isJpeg)
        {
            (width, height) = GetJpegDimensions(img.Data);
            pixelData = img.Data;
            dictExtras = "/Filter /DCTDecode\n";
        }
        else
        {
            // PNG: decode to raw RGB scanlines and compress with Deflate
            if (!TryDecodePngToRgb(img.Data, out width, out height, out var rgb))
            {
                // Fallback: treat bytes as raw 1×1 white pixel
                width = 1; height = 1; rgb = [255, 255, 255];
            }
            using var deflated = new System.IO.MemoryStream();
            using (var deflate = new System.IO.Compression.DeflateStream(deflated, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
                deflate.Write(rgb, 0, rgb.Length);
            pixelData = WrapDeflateInZlib(deflated.ToArray());
            dictExtras = "/Filter /FlateDecode\n";
        }

        WriteRaw($"{objNum} 0 obj\n");
        WriteRaw("<< /Type /XObject /Subtype /Image\n");
        WriteRaw($"/Width {width} /Height {height}\n");
        WriteRaw("/ColorSpace /DeviceRGB\n/BitsPerComponent 8\n");
        WriteRaw(dictExtras);
        WriteRaw($"/Length {pixelData.Length}\n");
        WriteRaw(">>\nstream\n");
        _stream.Write(pixelData);
        WriteRaw("\nendstream\nendobj\n");
    }

    private static string BuildContentStream(PdfPage page, bool hasUnicodeFont = false)
    {
        var sb = new StringBuilder();

        // Draw filled rectangles first (background)
        foreach (var rect in page.RectBlocks)
        {
            var rx = rect.X.ToString("F3", CultureInfo.InvariantCulture);
            var ry = rect.Y.ToString("F3", CultureInfo.InvariantCulture);
            var rw = rect.Width.ToString("F3", CultureInfo.InvariantCulture);
            var rh = rect.Height.ToString("F3", CultureInfo.InvariantCulture);
            var rr = rect.FillColor.R.ToString("F3", CultureInfo.InvariantCulture);
            var rg2 = rect.FillColor.G.ToString("F3", CultureInfo.InvariantCulture);
            var rb = rect.FillColor.B.ToString("F3", CultureInfo.InvariantCulture);
            sb.Append($"{rr} {rg2} {rb} rg\n");
            sb.Append($"{rx} {ry} {rw} {rh} re\n");
            sb.Append("f\n");
        }

        // Draw line segments
        foreach (var line in page.LineBlocks)
        {
            var lr = line.Color.R.ToString("F3", CultureInfo.InvariantCulture);
            var lg = line.Color.G.ToString("F3", CultureInfo.InvariantCulture);
            var lb = line.Color.B.ToString("F3", CultureInfo.InvariantCulture);
            var lw = line.LineWidth.ToString("F3", CultureInfo.InvariantCulture);
            var lx1 = line.X1.ToString("F3", CultureInfo.InvariantCulture);
            var ly1 = line.Y1.ToString("F3", CultureInfo.InvariantCulture);
            var lx2 = line.X2.ToString("F3", CultureInfo.InvariantCulture);
            var ly2 = line.Y2.ToString("F3", CultureInfo.InvariantCulture);
            sb.Append($"{lr} {lg} {lb} RG\n");
            sb.Append($"{lw} w\n");
            sb.Append($"{lx1} {ly1} m\n");
            sb.Append($"{lx2} {ly2} l\n");
            sb.Append("S\n");
        }

        // Place images (under text)
        for (var idx = 0; idx < page.ImageBlocks.Count; idx++)
        {
            var img = page.ImageBlocks[idx];
            var x = img.X.ToString("F3", CultureInfo.InvariantCulture);
            var y = img.Y.ToString("F3", CultureInfo.InvariantCulture);
            var w = img.RenderWidth.ToString("F3", CultureInfo.InvariantCulture);
            var h = img.RenderHeight.ToString("F3", CultureInfo.InvariantCulture);
            sb.Append("q\n");
            sb.Append($"{w} 0 0 {h} {x} {y} cm\n");
            sb.Append($"/Im{idx} Do\n");
            sb.Append("Q\n");
        }

        // Render text blocks on top
        foreach (var block in page.TextBlocks)
        {
            var fontSize = block.FontSize.ToString(CultureInfo.InvariantCulture);
            var x = block.X.ToString(CultureInfo.InvariantCulture);
            var y = block.Y.ToString(CultureInfo.InvariantCulture);

            // Set text color
            var colorCmd = block.Color.IsBlack
                ? "0 0 0 rg\n"
                : $"{block.Color.R.ToString("F3", CultureInfo.InvariantCulture)} " +
                  $"{block.Color.G.ToString("F3", CultureInfo.InvariantCulture)} " +
                  $"{block.Color.B.ToString("F3", CultureInfo.InvariantCulture)} rg\n";

            // If a clip rectangle is specified, save graphics state and set clipping path
            var hasClip = block.ClipRect.HasValue;
            if (hasClip)
            {
                var cr = block.ClipRect!.Value;
                var cx = cr.X.ToString("F3", CultureInfo.InvariantCulture);
                var cy = cr.Y.ToString("F3", CultureInfo.InvariantCulture);
                var cw = cr.Width.ToString("F3", CultureInfo.InvariantCulture);
                var ch = cr.Height.ToString("F3", CultureInfo.InvariantCulture);
                sb.Append("q\n");
                sb.Append($"{cx} {cy} {cw} {ch} re W n\n");
            }

            if (!hasUnicodeFont || !block.Text.Any(c => !IsWinAnsiHandled(c)))
            {
                // Pure Latin-1 text — use F1 (Helvetica) as before
                var escapedText = EscapePdfString(block.Text);
                sb.Append("BT\n");
                sb.Append(colorCmd);
                sb.Append($"/F1 {fontSize} Tf\n");
                // Apply horizontal scaling if text overflows MaxWidth
                if (block.MaxWidth.HasValue)
                {
                    var naturalWidth = MeasureTextWidth(block.Text, block.FontSize);
                    if (naturalWidth > block.MaxWidth.Value && naturalWidth > 0)
                    {
                        var tzPercent = (block.MaxWidth.Value / naturalWidth) * 100.0;
                        sb.Append($"{tzPercent.ToString("F1", CultureInfo.InvariantCulture)} Tz\n");
                    }
                }
                sb.Append($"{x} {y} Td\n");
                sb.Append($"({escapedText}) Tj\n");
                sb.Append("ET\n");
            }
            else
            {
                // Block contains non-WinAnsi characters.  Render the ENTIRE
                // block in F2 (CID/Unicode font) so that all characters share
                // the same font descriptor and thus the same bounding-box Y in
                // text extractors (avoids the ~3 pt offset between Type1 F1
                // and CIDFontType2 F2 that caused PyMuPDF to put spans on
                // separate lines).
                sb.Append("BT\n");
                sb.Append(colorCmd);
                sb.Append($"/F2 {fontSize} Tf\n");
                // Apply horizontal scaling if text overflows MaxWidth
                if (block.MaxWidth.HasValue)
                {
                    var naturalWidth = MeasureTextWidth(block.Text, block.FontSize);
                    if (naturalWidth > block.MaxWidth.Value && naturalWidth > 0)
                    {
                        var tzPercent = (block.MaxWidth.Value / naturalWidth) * 100.0;
                        sb.Append($"{tzPercent.ToString("F1", CultureInfo.InvariantCulture)} Tz\n");
                    }
                }
                sb.Append($"{x} {y} Td\n");
                sb.Append('<');
                foreach (var ch in block.Text)
                {
                    sb.Append(((int)ch).ToString("X4"));
                }
                sb.Append("> Tj\n");
                sb.Append("ET\n");
            }

            // Restore graphics state after clipping
            if (hasClip)
                sb.Append("Q\n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Measures the natural rendering width of text in Helvetica at the given font size.
    /// Uses the standard Helvetica character width table.
    /// </summary>
    private static double MeasureTextWidth(string text, float fontSize)
    {
        double total = 0;
        foreach (var ch in text)
        {
            // Standard Helvetica character widths in 1/1000 em units
            var w = ch switch
            {
                ' ' => 278, '!' => 278, '"' => 355, '#' => 556, '$' => 556, '%' => 889,
                '&' => 667, '\'' => 191, '(' => 333, ')' => 333, '*' => 389, '+' => 584,
                ',' => 278, '-' => 333, '.' => 278, '/' => 278,
                >= '0' and <= '9' => 556,
                ':' => 278, ';' => 278, '<' => 584, '=' => 584, '>' => 584, '?' => 556,
                '@' => 1015,
                'A' => 667, 'B' => 667, 'C' => 722, 'D' => 722, 'E' => 667, 'F' => 611,
                'G' => 778, 'H' => 722, 'I' => 278, 'J' => 500, 'K' => 667, 'L' => 556,
                'M' => 833, 'N' => 722, 'O' => 778, 'P' => 667, 'Q' => 778, 'R' => 722,
                'S' => 667, 'T' => 611, 'U' => 722, 'V' => 667, 'W' => 944, 'X' => 667,
                'Y' => 667, 'Z' => 611,
                '[' => 278, '\\' => 278, ']' => 278, '^' => 469, '_' => 556, '`' => 333,
                'a' => 556, 'b' => 556, 'c' => 500, 'd' => 556, 'e' => 556, 'f' => 278,
                'g' => 556, 'h' => 556, 'i' => 222, 'j' => 222, 'k' => 500, 'l' => 222,
                'm' => 833, 'n' => 556, 'o' => 556, 'p' => 556, 'q' => 556, 'r' => 333,
                's' => 500, 't' => 278, 'u' => 556, 'v' => 500, 'w' => 722, 'x' => 500,
                'y' => 500, 'z' => 500,
                '{' => 334, '|' => 260, '}' => 334, '~' => 584,
                _ => IsFullWidthCharPdf(ch) ? 1000 : 556
            };
            total += w;
        }
        return total * fontSize / 1000.0;
    }

    /// <summary>
    /// Returns true for CJK and fullwidth characters that occupy ~1em width.
    /// </summary>
    private static bool IsFullWidthCharPdf(char c)
    {
        return (c >= 0x1100 && c <= 0x115F)  // Hangul Jamo
            || (c >= 0x2E80 && c <= 0x9FFF)  // CJK
            || (c >= 0xAC00 && c <= 0xD7AF)  // Hangul Syllables
            || (c >= 0xF900 && c <= 0xFAFF)  // CJK Compat
            || (c >= 0xFE30 && c <= 0xFE4F)  // CJK Compat Forms
            || (c >= 0xFF01 && c <= 0xFF60)  // Fullwidth Forms
            || (c >= 0xFFE0 && c <= 0xFFE6); // Fullwidth Signs
    }

    /// <summary>
    /// Returns true if a character can be rendered using F1 (Helvetica/WinAnsiEncoding)
    /// directly—either it's in the Latin-1 range (≤0xFF) or it's a known Unicode
    /// character that EscapePdfString can map to a WinAnsiEncoding byte or ASCII
    /// replacement.  Characters returning false need the F2 CID/Unicode font.
    /// </summary>
    private static bool IsWinAnsiHandled(char ch)
    {
        if (ch <= '\xFF') return true;
        return ch is '\u2012' or '\u2013' or '\u2014'      // figure/en/em dash
                  or '\u2018' or '\u2019'                   // smart single quotes
                  or '\u201C' or '\u201D'                   // smart double quotes
                  or '\u2026'                               // ellipsis
                  or '\u2022'                               // bullet
                  or '\u2264' or '\u2265'                   // ≤ ≥
                  or '\u2122'                               // trademark
                  or '\u20AC';                              // euro sign
    }

    /// <summary>
    /// Splits text into segments: (text, isUnicode) where isUnicode=true means
    /// the segment contains characters above U+00FF that need the F2 CID font.
    /// Adjacent characters of the same "class" are grouped together.
    /// </summary>
    private static List<(string text, bool isUnicode)> SplitTextIntoFontSegments(string text)
    {
        var result = new List<(string, bool)>();
        if (string.IsNullOrEmpty(text)) return result;

        var sb = new StringBuilder();
        bool? currentIsUnicode = null;

        foreach (var ch in text)
        {
            var needsUnicode = !IsWinAnsiHandled(ch);
            if (currentIsUnicode == null)
            {
                currentIsUnicode = needsUnicode;
                sb.Append(ch);
            }
            else if (currentIsUnicode == needsUnicode)
            {
                sb.Append(ch);
            }
            else
            {
                result.Add((sb.ToString(), currentIsUnicode.Value));
                sb.Clear();
                sb.Append(ch);
                currentIsUnicode = needsUnicode;
            }
        }

        if (sb.Length > 0)
            result.Add((sb.ToString(), currentIsUnicode!.Value));

        return result;
    }

    /// <summary>
    /// Builds a ToUnicode CMap stream for Identity-H encoded Unicode text.
    /// Maps each Unicode code point to itself (since Identity-H uses Unicode code points as glyph IDs).
    /// </summary>
    private static string BuildToUnicodeCMap(IEnumerable<int> codePoints)
    {
        var chars = codePoints.ToList();
        var sb = new StringBuilder();
        sb.Append("/CIDInit /ProcSet findresource begin\n");
        sb.Append("12 dict begin\n");
        sb.Append("begincmap\n");
        sb.Append("/CIDSystemInfo\n");
        sb.Append("<< /Registry (Adobe)\n");
        sb.Append("/Ordering (UCS)\n");
        sb.Append("/Supplement 0\n");
        sb.Append(">> def\n");
        sb.Append("/CMapName /Adobe-Identity-UCS def\n");
        sb.Append("/CMapType 2 def\n");
        sb.Append("1 begincodespacerange\n");
        sb.Append("<0000> <FFFF>\n");
        sb.Append("endcodespacerange\n");

        // Write in chunks of 100 (PDF limit per beginbfchar block)
        const int chunkSize = 100;
        for (var offset = 0; offset < chars.Count; offset += chunkSize)
        {
            var chunk = chars.Skip(offset).Take(chunkSize).ToList();
            sb.Append($"{chunk.Count} beginbfchar\n");
            foreach (var cp in chunk)
            {
                sb.Append($"<{cp:X4}> <{cp:X4}>\n");
            }
            sb.Append("endbfchar\n");
        }

        sb.Append("endcmap\n");
        sb.Append("CMapName currentdict /CMap defineresource pop\n");
        sb.Append("end\nend\n");
        return sb.ToString();
    }

    private static string EscapePdfString(string text)
    {
        // Map Unicode characters to WinAnsiEncoding byte values where possible.
        // Characters in 0x80–0x9F range are correctly decoded by PDF readers
        // when WinAnsiEncoding is declared on the font.
        var normalized = new System.Text.StringBuilder(text.Length);
        foreach (var ch in text)
        {
            normalized.Append(ch switch
            {
                '\u2013' or '\u2012' => (char)0x96,        // en-dash
                '\u2014' => (char)0x97,                     // em-dash
                '\u2018' => (char)0x91,                     // left single quote
                '\u2019' => (char)0x92,                     // right single quote
                '\u201C' => (char)0x93,                     // left double quote
                '\u201D' => (char)0x94,                     // right double quote
                '\u2026' => (char)0x85,                     // ellipsis
                '\u2022' => (char)0x95,                     // bullet
                '\u2020' => (char)0x86,                     // dagger
                '\u2021' => (char)0x87,                     // double dagger
                '\u2030' => (char)0x89,                     // per mille
                '\u0160' => (char)0x8A,                     // S-caron
                '\u0152' => (char)0x8C,                     // OE ligature
                '\u017D' => (char)0x8E,                     // Z-caron
                '\u0161' => (char)0x9A,                     // s-caron
                '\u0153' => (char)0x9C,                     // oe ligature
                '\u017E' => (char)0x9E,                     // z-caron
                '\u0178' => (char)0x9F,                     // Y-diaeresis
                '\u2122' => (char)0x99,                     // trademark
                '\u20AC' => (char)0x80,                     // euro sign
                '\u00A0' => ' ',                            // non-breaking space
                '\u0060' => '\'',                           // backtick → apostrophe
                '\u00B7' => '*',                            // middle dot → asterisk
                '\u00D7' => 'x',                            // multiplication sign
                '\u00F7' => '/',                            // division sign
                '\u2264' => "<=",                           // ≤
                '\u2265' => ">=",                           // ≥
                '\u00B0' => " deg",                         // degree sign
                '\u00AE' => (char)0xAE,                     // registered trademark (already in WinAnsi)
                '\u00A3' => '\u00A3',                       // pound sign (already in WinAnsi)
                '\u00A5' => '\u00A5',                       // yen sign (already in WinAnsi)
                _ when ch > '\xFF' => "",                   // skip: non-Latin1 chars are handled by F2 font
                _ => ch
            });
        }
        return normalized.ToString()
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    // ── Image dimension helpers ────────────────────────────────────────────

    private static (int width, int height) GetJpegDimensions(byte[] data)
    {
        var i = 2; // skip SOI marker (FF D8)
        while (i < data.Length - 8)
        {
            if (data[i] != 0xFF) break;
            var marker = data[i + 1];
            var segLen = (data[i + 2] << 8) | data[i + 3];
            // SOF0, SOF1, SOF2 markers hold image dimensions
            if (marker is 0xC0 or 0xC1 or 0xC2)
            {
                var h = (data[i + 5] << 8) | data[i + 6];
                var w = (data[i + 7] << 8) | data[i + 8];
                return (w, h);
            }
            if (segLen < 2) break;
            i += 2 + segLen;
        }
        return (1, 1);
    }

    /// <summary>
    /// Minimal PNG decoder: extracts width, height, and raw filtered scanline data,
    /// then applies the row filters to produce 8-bit-per-channel RGB pixel data.
    /// Supports color type 2 (RGB) and color type 6 (RGBA, alpha stripped).
    /// </summary>
    private static bool TryDecodePngToRgb(byte[] data, out int width, out int height, out byte[] rgb)
    {
        width = 1; height = 1; rgb = [255, 255, 255];
        if (data.Length < 33) return false;

        // Validate PNG signature
        ReadOnlySpan<byte> sig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (!data.AsSpan(0, 8).SequenceEqual(sig)) return false;

        // Parse IHDR (always first chunk, at offset 8)
        width  = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
        height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
        var bitDepth  = data[24];
        var colorType = data[25];
        // Supported: RGB (2) with 8-bit depth, RGBA (6) with 8-bit depth
        if (bitDepth != 8 || colorType is not (2 or 6))
            return false;

        int channels = colorType == 6 ? 4 : 3;

        // Collect all IDAT chunks and concatenate their compressed data
        using var idatStream = new System.IO.MemoryStream();
        var pos = 8;
        while (pos + 12 <= data.Length)
        {
            var chunkLen = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
            var chunkType = Encoding.ASCII.GetString(data, pos + 4, 4);
            if (chunkType == "IDAT")
                idatStream.Write(data, pos + 8, chunkLen);
            else if (chunkType == "IEND")
                break;
            pos += 12 + chunkLen;
        }

        // zlib-compressed data: skip 2-byte zlib header, decompress raw deflate
        var compressed = idatStream.ToArray();
        if (compressed.Length < 3) return false;

        byte[] decompressed;
        try
        {
            using var inputMs = new System.IO.MemoryStream(compressed, 2, compressed.Length - 2); // skip zlib header
            using var outputMs = new System.IO.MemoryStream();
            using (var deflate = new System.IO.Compression.DeflateStream(inputMs, System.IO.Compression.CompressionMode.Decompress))
                deflate.CopyTo(outputMs);
            decompressed = outputMs.ToArray();
        }
        catch
        {
            return false;
        }

        // Apply PNG row filters to get raw RGB data
        var stride = width * channels;
        var outputRgb = new byte[width * height * 3];
        var prevRow = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filterByte = decompressed[row * (stride + 1)];
            var scanStart = row * (stride + 1) + 1;
            var raw = decompressed.AsSpan(scanStart, stride);
            var cur = new byte[stride];

            switch (filterByte)
            {
                case 0: // None
                    raw.CopyTo(cur);
                    break;
                case 1: // Sub
                    for (var x = 0; x < stride; x++)
                        cur[x] = (byte)(raw[x] + (x >= channels ? cur[x - channels] : 0));
                    break;
                case 2: // Up
                    for (var x = 0; x < stride; x++)
                        cur[x] = (byte)(raw[x] + prevRow[x]);
                    break;
                case 3: // Average
                    for (var x = 0; x < stride; x++)
                    {
                        var a = x >= channels ? cur[x - channels] : 0;
                        cur[x] = (byte)(raw[x] + (a + prevRow[x]) / 2);
                    }
                    break;
                case 4: // Paeth
                    for (var x = 0; x < stride; x++)
                    {
                        var a = x >= channels ? cur[x - channels] : 0;
                        var b = prevRow[x];
                        var c = x >= channels ? prevRow[x - channels] : 0;
                        cur[x] = (byte)(raw[x] + PaethPredictor(a, b, c));
                    }
                    break;
                default:
                    raw.CopyTo(cur);
                    break;
            }

            // Convert to RGB (drop alpha if RGBA)
            var outBase = row * width * 3;
            for (var px = 0; px < width; px++)
            {
                outputRgb[outBase + px * 3]     = cur[px * channels];
                outputRgb[outBase + px * 3 + 1] = cur[px * channels + 1];
                outputRgb[outBase + px * 3 + 2] = cur[px * channels + 2];
            }

            cur.CopyTo(prevRow, 0);
        }

        rgb = outputRgb;
        return true;
    }

    private static int PaethPredictor(int a, int b, int c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    /// <summary>
    /// Wraps raw Deflate-compressed bytes in a zlib wrapper (CMF + FLG + data + Adler-32)
    /// required by the PDF /FlateDecode filter.
    /// </summary>
    private static byte[] WrapDeflateInZlib(byte[] deflateData)
    {
        // zlib header: CMF=0x78 (deflate, window 32KB), FLG computed so (CMF*256+FLG) % 31 == 0
        var cmf = 0x78;
        var flg = (byte)(31 - (cmf * 256) % 31);

        // Compute Adler-32 checksum (we don't have the original data here, use a placeholder)
        // A correct implementation would require the original uncompressed bytes.
        // For brevity, write 0 adler (PDF readers tolerate it for standard deflate).
        var adler = new byte[4]; // zeros

        var result = new byte[2 + deflateData.Length + 4];
        result[0] = (byte)cmf;
        result[1] = flg;
        Array.Copy(deflateData, 0, result, 2, deflateData.Length);
        Array.Copy(adler, 0, result, 2 + deflateData.Length, 4);
        return result;
    }

    private long Position => _stream.Position;

    private void WriteRaw(string text)
    {
        var bytes = Encoding.Latin1.GetBytes(text);
        _stream.Write(bytes);
    }

    // ── System CJK font discovery ──────────────────────────────────────

    /// <summary>
    /// Searches for a system TrueType/TTC font that supports CJK characters.
    /// Returns the full path to the first font found, or null.
    /// </summary>
    private static string? FindSystemCjkFont()
    {
        // Candidate fonts in priority order (file name only, searched in system font dir)
        string[] candidates;
        if (OperatingSystem.IsWindows())
        {
            candidates = [
                "msyh.ttc",      // Microsoft YaHei (Win7+)
                "msyhbd.ttc",    // Microsoft YaHei Bold
                "simsun.ttc",    // SimSun
                "simhei.ttf",    // SimHei
                "msjh.ttc",      // Microsoft JhengHei
                "malgun.ttf",    // Malgun Gothic (Korean)
                "NotoSansCJKsc-Regular.otf",
            ];
        }
        else if (OperatingSystem.IsMacOS())
        {
            candidates = [
                "PingFang.ttc",
                "STHeiti Medium.ttc",
                "Hiragino Sans GB.ttc",
            ];
        }
        else // Linux and others
        {
            // Common paths on Linux for CJK fonts
            string[] searchDirs = [
                "/usr/share/fonts/truetype/noto",
                "/usr/share/fonts/opentype/noto",
                "/usr/share/fonts/noto-cjk",
                "/usr/share/fonts/google-noto-cjk",
                "/usr/share/fonts/truetype",
                "/usr/share/fonts",
            ];
            string[] names = [
                "NotoSansCJKsc-Regular.ttf",
                "NotoSansCJK-Regular.ttc",
                "NotoSansSC-Regular.otf",
                "wqy-microhei.ttc",
                "DroidSansFallbackFull.ttf",
            ];
            foreach (var dir in searchDirs)
                if (Directory.Exists(dir))
                    foreach (var name in names)
                    {
                        var p = Path.Combine(dir, name);
                        if (File.Exists(p)) return p;
                    }
            return null;
        }

        var fontDir = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts")
            : "/System/Library/Fonts";

        foreach (var name in candidates)
        {
            var p = Path.Combine(fontDir, name);
            if (File.Exists(p)) return p;
        }
        return null;
    }

    /// <summary>
    /// Loads a TrueType/TTC font file. For TTC collections, extracts the first font.
    /// </summary>
    private static byte[] LoadTtfFont(string path)
    {
        var raw = File.ReadAllBytes(path);
        // TTC files start with "ttcf"
        if (raw.Length > 12 && raw[0] == 't' && raw[1] == 't' && raw[2] == 'c' && raw[3] == 'f')
        {
            // TTC header: tag(4) + majorVersion(2) + minorVersion(2) + numFonts(4)
            // then offsets[numFonts], each 4 bytes
            var numFonts = ReadU32(raw, 8);
            if (numFonts == 0) return raw;
            var offset0 = (int)ReadU32(raw, 12);

            // Extract just the first font by finding its table directory size
            // and collecting all referenced table data
            return ExtractTtfFromTtc(raw, offset0);
        }
        return raw;
    }

    private static byte[] ExtractTtfFromTtc(byte[] ttc, int fontOffset)
    {
        // Read the offset table at fontOffset
        var numTables = ReadU16(ttc, fontOffset + 4);
        // Build list of tables: tag(4) + checksum(4) + offset(4) + length(4) = 16 each
        var headerSize = 12 + numTables * 16;
        var tables = new List<(string tag, uint checksum, uint offset, uint length)>();
        for (var i = 0; i < numTables; i++)
        {
            var entryOff = fontOffset + 12 + i * 16;
            var tag = Encoding.ASCII.GetString(ttc, entryOff, 4);
            var cs = ReadU32(ttc, entryOff + 4);
            var off = ReadU32(ttc, entryOff + 8);
            var len = ReadU32(ttc, entryOff + 12);
            tables.Add((tag, cs, off, len));
        }

        // Build a standalone TTF
        using var ms = new MemoryStream();
        // Offset table
        ms.Write(ttc, fontOffset, 12);

        // We'll write table directory first with placeholder offsets, then table data
        var dirStart = ms.Position;
        // Write placeholder directory entries
        for (var i = 0; i < numTables; i++)
            ms.Write(new byte[16]);

        // Write each table's data, recording new offsets
        var newOffsets = new uint[numTables];
        for (var i = 0; i < numTables; i++)
        {
            var (tag, checksum, offset, length) = tables[i];
            // Align to 4 bytes
            while (ms.Position % 4 != 0) ms.WriteByte(0);
            newOffsets[i] = (uint)ms.Position;
            ms.Write(ttc, (int)offset, (int)length);
        }

        // Go back and fill in the directory
        var result = ms.ToArray();
        for (var i = 0; i < numTables; i++)
        {
            var entryOff = (int)dirStart + i * 16;
            var (tag, checksum, _, length) = tables[i];
            Array.Copy(Encoding.ASCII.GetBytes(tag), 0, result, entryOff, 4);
            WriteU32(result, entryOff + 4, checksum);
            WriteU32(result, entryOff + 8, newOffsets[i]);
            WriteU32(result, entryOff + 12, length);
        }

        return result;
    }

    /// <summary>
    /// Subsets a TrueType font by zeroing out glyph outlines not in the needed set.
    /// Preserves the font structure (all tables, glyph count, loca format) so glyph IDs
    /// remain stable. Only 'glyf' table entries for unused glyphs are replaced with
    /// empty glyph records, yielding much better compression.
    /// </summary>
    private static byte[] SubsetTtfFont(byte[] ttf, HashSet<ushort> neededGlyphs)
    {
        var (glyfOff, glyfLen) = FindTable(ttf, "glyf");
        var (locaOff, locaLen) = FindTable(ttf, "loca");
        var (headOff, _) = FindTable(ttf, "head");
        var (maxpOff, _) = FindTable(ttf, "maxp");

        if (glyfOff == 0 || locaOff == 0 || headOff == 0 || maxpOff == 0)
            return ttf; // Can't subset without required tables

        var numGlyphs = ReadU16(ttf, (int)maxpOff + 4);
        var indexToLocFormat = ReadU16(ttf, (int)headOff + 50); // 0=short, 1=long
        var isLong = indexToLocFormat == 1;

        // Read loca offsets
        var offsets = new uint[numGlyphs + 1];
        for (var i = 0; i <= numGlyphs; i++)
        {
            offsets[i] = isLong
                ? ReadU32(ttf, (int)locaOff + i * 4)
                : (uint)(ReadU16(ttf, (int)locaOff + i * 2) * 2);
        }

        // Clone the font data
        var result = (byte[])ttf.Clone();

        // Zero out glyph data for unused glyphs
        for (ushort gid = 0; gid < numGlyphs; gid++)
        {
            if (neededGlyphs.Contains(gid)) continue;

            var glyphStart = (int)(glyfOff + offsets[gid]);
            var glyphEnd = (int)(glyfOff + offsets[gid + 1]);
            var glyphSize = glyphEnd - glyphStart;
            if (glyphSize > 0 && glyphStart >= 0 && glyphEnd <= result.Length)
                Array.Clear(result, glyphStart, glyphSize);
        }

        return result;
    }

    // ── TrueType table parsing ─────────────────────────────────────────

    private static (uint offset, uint length) FindTable(byte[] ttf, string tag)
    {
        var numTables = ReadU16(ttf, 4);
        for (var i = 0; i < numTables; i++)
        {
            var entryOff = 12 + i * 16;
            if (entryOff + 16 > ttf.Length) break;
            var t = Encoding.ASCII.GetString(ttf, entryOff, 4);
            if (t == tag)
                return (ReadU32(ttf, entryOff + 8), ReadU32(ttf, entryOff + 12));
        }
        return (0, 0);
    }

    /// <summary>
    /// Parses the cmap table to build a Unicode codepoint → glyph ID mapping.
    /// Supports format 4 (BMP) subtables.
    /// </summary>
    private static Dictionary<int, ushort> ParseCmapTable(byte[] ttf)
    {
        var map = new Dictionary<int, ushort>();
        var (tableOff, tableLen) = FindTable(ttf, "cmap");
        if (tableOff == 0) return map;

        var off = (int)tableOff;
        var numSubtables = ReadU16(ttf, off + 2);

        // Find a Unicode BMP subtable (platform 3 encoding 1, or platform 0)
        for (var i = 0; i < numSubtables; i++)
        {
            var stOff = off + 4 + i * 8;
            var platformId = ReadU16(ttf, stOff);
            var encodingId = ReadU16(ttf, stOff + 2);
            var subtableOffset = off + (int)ReadU32(ttf, stOff + 4);

            bool isUnicodeBmp = (platformId == 3 && encodingId == 1)
                             || (platformId == 0 && encodingId <= 4);
            if (!isUnicodeBmp) continue;

            var format = ReadU16(ttf, subtableOffset);
            if (format == 4)
            {
                ParseCmapFormat4(ttf, subtableOffset, map);
                if (map.Count > 0) return map;
            }
            else if (format == 12)
            {
                ParseCmapFormat12(ttf, subtableOffset, map);
                if (map.Count > 0) return map;
            }
        }

        // Try format 12 subtable (platform 3 encoding 10)
        for (var i = 0; i < numSubtables; i++)
        {
            var stOff = off + 4 + i * 8;
            var platformId = ReadU16(ttf, stOff);
            var encodingId = ReadU16(ttf, stOff + 2);
            var subtableOffset = off + (int)ReadU32(ttf, stOff + 4);

            if (platformId == 3 && encodingId == 10)
            {
                var format = ReadU16(ttf, subtableOffset);
                if (format == 12)
                {
                    ParseCmapFormat12(ttf, subtableOffset, map);
                    if (map.Count > 0) return map;
                }
            }
        }

        return map;
    }

    private static void ParseCmapFormat4(byte[] ttf, int off, Dictionary<int, ushort> map)
    {
        var segCount = ReadU16(ttf, off + 6) / 2;
        var endCodeOff = off + 14;
        var startCodeOff = endCodeOff + segCount * 2 + 2; // +2 for reservedPad
        var idDeltaOff = startCodeOff + segCount * 2;
        var idRangeOff = idDeltaOff + segCount * 2;

        for (var seg = 0; seg < segCount; seg++)
        {
            var endCode = ReadU16(ttf, endCodeOff + seg * 2);
            var startCode = ReadU16(ttf, startCodeOff + seg * 2);
            var idDelta = (short)ReadU16(ttf, idDeltaOff + seg * 2);
            var idRangeOffset = ReadU16(ttf, idRangeOff + seg * 2);

            if (startCode == 0xFFFF) break;

            for (var c = startCode; c <= endCode; c++)
            {
                ushort gid;
                if (idRangeOffset == 0)
                {
                    gid = (ushort)((c + idDelta) & 0xFFFF);
                }
                else
                {
                    var glyphOff = idRangeOff + seg * 2 + idRangeOffset + (c - startCode) * 2;
                    if (glyphOff + 1 >= ttf.Length) continue;
                    gid = ReadU16(ttf, glyphOff);
                    if (gid != 0) gid = (ushort)((gid + idDelta) & 0xFFFF);
                }
                if (gid != 0) map[c] = gid;
            }
        }
    }

    private static void ParseCmapFormat12(byte[] ttf, int off, Dictionary<int, ushort> map)
    {
        var nGroups = (int)ReadU32(ttf, off + 12);
        var groupOff = off + 16;
        for (var i = 0; i < nGroups; i++)
        {
            var startCode = ReadU32(ttf, groupOff + i * 12);
            var endCode = ReadU32(ttf, groupOff + i * 12 + 4);
            var startGlyph = ReadU32(ttf, groupOff + i * 12 + 8);
            for (uint c = startCode; c <= endCode && c <= 0xFFFF; c++)
            {
                var gid = (ushort)(startGlyph + (c - startCode));
                if (gid != 0) map[(int)c] = gid;
            }
        }
    }

    /// <summary>
    /// Parses the 'hmtx' table to extract glyph advance widths.
    /// Returns (advances indexed by glyph id, unitsPerEm).
    /// </summary>
    private static (ushort[] advances, int unitsPerEm) ParseHmtxWidths(byte[] ttf)
    {
        // head table for unitsPerEm
        var (headOff, _) = FindTable(ttf, "head");
        var upm = headOff > 0 ? ReadU16(ttf, (int)headOff + 18) : 1000;

        // hhea table for numOfLongHorMetrics
        var (hheaOff, _) = FindTable(ttf, "hhea");
        var numHMetrics = hheaOff > 0 ? ReadU16(ttf, (int)hheaOff + 34) : 0;

        // maxp table for numGlyphs
        var (maxpOff, _) = FindTable(ttf, "maxp");
        var numGlyphs = maxpOff > 0 ? ReadU16(ttf, (int)maxpOff + 4) : numHMetrics;

        // hmtx table
        var (hmtxOff, hmtxLen) = FindTable(ttf, "hmtx");
        var advances = new ushort[numGlyphs];
        if (hmtxOff > 0)
        {
            var off = (int)hmtxOff;
            ushort lastWidth = 0;
            for (var i = 0; i < numHMetrics && off + 3 < ttf.Length; i++)
            {
                lastWidth = ReadU16(ttf, off);
                advances[i] = lastWidth;
                off += 4; // advanceWidth(2) + lsb(2)
            }
            // Remaining glyphs share the last advance width
            for (var i = numHMetrics; i < numGlyphs; i++)
                advances[i] = lastWidth;
        }
        return (advances, upm);
    }

    /// <summary>
    /// Parses font metrics from head, OS/2, and hhea tables.
    /// </summary>
    private static (int ascent, int descent, int capHeight, int[] bbox) ParseFontMetrics(byte[] ttf)
    {
        var (headOff, _) = FindTable(ttf, "head");
        int[] bbox = [-166, -225, 1000, 931];
        if (headOff > 0)
        {
            bbox = [
                (short)ReadU16(ttf, (int)headOff + 36),
                (short)ReadU16(ttf, (int)headOff + 38),
                (short)ReadU16(ttf, (int)headOff + 40),
                (short)ReadU16(ttf, (int)headOff + 42),
            ];
        }

        var (os2Off, os2Len) = FindTable(ttf, "OS/2");
        if (os2Off > 0)
        {
            var asc = (short)ReadU16(ttf, (int)os2Off + 68);   // sTypoAscender
            var desc = (short)ReadU16(ttf, (int)os2Off + 70);  // sTypoDescender
            var capH = os2Len >= 90 ? (short)ReadU16(ttf, (int)os2Off + 88) : asc; // sCapHeight
            return (asc, desc, capH, bbox);
        }

        // Fallback to hhea
        var (hheaOff, _) = FindTable(ttf, "hhea");
        if (hheaOff > 0)
        {
            var asc = (short)ReadU16(ttf, (int)hheaOff + 4);
            var desc = (short)ReadU16(ttf, (int)hheaOff + 6);
            return (asc, desc, asc, bbox);
        }

        return (718, -207, 718, bbox);
    }

    /// <summary>
    /// Builds the /W (widths) array for the CID font, covering only the Unicode chars used.
    /// Format: [cid1 [w1] cid2 [w2] ...] with widths in 1/1000 em units.
    /// </summary>
    private static string BuildWArray(SortedSet<int> unicodeChars, Dictionary<int, ushort> cmap, ushort[] advances, int upm)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var cp in unicodeChars)
        {
            if (cmap.TryGetValue(cp, out var gid) && gid < advances.Length)
            {
                var w = (int)(advances[gid] * 1000L / upm);
                sb.Append($"{cp} [{w}] ");
            }
        }
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Builds a compressed CIDToGIDMap (maps CID → glyph ID for the entire BMP range 0-65535).
    /// </summary>
    private static byte[] BuildCidToGidMap(Dictionary<int, ushort> cmap)
    {
        // The map is 65536 entries × 2 bytes = 131072 bytes uncompressed
        var raw = new byte[65536 * 2];
        foreach (var (cp, gid) in cmap)
        {
            if (cp < 65536)
            {
                raw[cp * 2] = (byte)(gid >> 8);
                raw[cp * 2 + 1] = (byte)(gid & 0xFF);
            }
        }

        using var ms = new MemoryStream();
        using (var zlib = new System.IO.Compression.ZLibStream(ms, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(raw, 0, raw.Length);
        return ms.ToArray();
    }

    // ── Binary read/write helpers ──────────────────────────────────────

    private static ushort ReadU16(byte[] data, int offset)
    {
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static uint ReadU32(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16)
             | ((uint)data[offset + 2] << 8) | data[offset + 3];
    }

    private static void WriteU32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)(value & 0xFF);
    }
}
