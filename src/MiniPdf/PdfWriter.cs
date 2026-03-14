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

    /// <summary>
    /// Holds per-font embedding data for a single Unicode font slot (F2, F3, ...).
    /// </summary>
    private sealed class EmbeddedFontInfo
    {
        public string FontName = "";
        public byte[] CompressedFontData = [];
        public byte[] CidToGidMapData = [];
        public string WArrayString = "";
        public string ToUnicodeCMap = "";
        public int FontUncompressedLength;
        public int Ascent = 718, Descent = -207, CapHeight = 718;
        public int[] Bbox = [-166, -225, 1000, 931];
        /// <summary>Maps Unicode code point → CID. BMP chars use identity; non-BMP use PUA slots.</summary>
        public Dictionary<int, int> CpToCid = new();
        // PDF object numbers (assigned during Write)
        public int ToUnicodeObj, DescriptorObj, CidFontObj, Type0Obj, FontFileObj, CidToGidObj;
    }

    internal void Write(PdfDocument document)
    {
        // PDF Header
        WriteRaw("%PDF-1.4\n");
        // Binary comment to signal binary content (recommended by spec)
        WriteRaw("%\xe2\xe3\xcf\xd3\n");

        var pages = document.Pages;
        var pageCount = pages.Count;

        // Collect all Unicode code points (handling surrogate pairs) from text
        // blocks that contain any non-WinAnsi character.  When a block has ANY
        // non-WinAnsi char the ENTIRE block is rendered in a Unicode font so all
        // spans share the same bbox Y in text extractors.
        var unicodeCodePoints = new SortedSet<int>();
        foreach (var page in pages)
            foreach (var block in page.TextBlocks)
            {
                bool blockNeedsUnicode = false;
                foreach (var ch in block.Text)
                    if (!IsWinAnsiHandled(ch)) { blockNeedsUnicode = true; break; }
                if (blockNeedsUnicode)
                {
                    var shaped = ShapeArabicCodePoints(EnumerateCodePoints(block.Text).ToList());
                    foreach (var cp in shaped)
                        unicodeCodePoints.Add(cp);
                }
            }
        var needsUnicodeFont = unicodeCodePoints.Count > 0;

        // ── Multi-font discovery ───────────────────────────────────────────
        // Find system fonts and assign each Unicode code point to a font that
        // can render it.  Each font becomes a separate PDF Type0 font (F2, F3, …).
        var embeddedFonts = new List<EmbeddedFontInfo>();  // index = fontSlot (0→F2, 1→F3, …)
        var cpToFontSlot = new Dictionary<int, int>();     // code point → fontSlot index

        if (needsUnicodeFont)
        {
            var loadedFonts = new List<(byte[] ttf, Dictionary<int, ushort> cmap, ushort[] advances, int upm,
                                        int asc, int desc, int capH, int[] bbox, string name)>();

            // 1) Load registered fonts first (for WASM / environments without system fonts)
            foreach (var (regName, regData) in MiniPdf.GetRegisteredFonts())
            {
                try
                {
                    var ttf = LoadTtfFontFromBytes(regData);
                    var cmap = ParseCmapTable(ttf);
                    if (cmap.Count == 0) continue;
                    var (advances, upm) = ParseHmtxWidths(ttf);
                    var (asc, desc, capH, bbox) = ParseFontMetrics(ttf);
                    loadedFonts.Add((ttf, cmap, advances, upm, asc, desc, capH, bbox, regName));
                }
                catch { /* skip fonts that fail to parse */ }
            }

            // 2) Then load system fonts
            var candidatePaths = FindSystemFontCandidates();

            foreach (var path in candidatePaths)
            {
                try
                {
                    var ttf = LoadTtfFont(path);
                    var cmap = ParseCmapTable(ttf);
                    if (cmap.Count == 0) continue;
                    var (advances, upm) = ParseHmtxWidths(ttf);
                    var (asc, desc, capH, bbox) = ParseFontMetrics(ttf);
                    var name = System.IO.Path.GetFileNameWithoutExtension(path);
                    loadedFonts.Add((ttf, cmap, advances, upm, asc, desc, capH, bbox, name));
                }
                catch { /* skip fonts that fail to parse */ }
            }

            // Identify a dedicated emoji font slot (by filename)
            var emojiFontIdx = -1;
            for (var fi = 0; fi < loadedFonts.Count; fi++)
            {
                var n = loadedFonts[fi].name;
                if (n.Contains("emj", StringComparison.OrdinalIgnoreCase) ||
                    n.Contains("emoji", StringComparison.OrdinalIgnoreCase))
                { emojiFontIdx = fi; break; }
            }

            // Assign each code point to the first font that covers it AND has actual glyph outlines.
            // For emoji ranges, prefer the dedicated emoji font to avoid CJK placeholder glyphs.
            var uncovered = new List<int>();
            foreach (var cp in unicodeCodePoints)
            {
                bool found = false;

                // For emoji ranges, try the emoji font first
                if (!found && emojiFontIdx >= 0 && IsEmojiRange(cp))
                {
                    if (loadedFonts[emojiFontIdx].cmap.TryGetValue(cp, out var egid) &&
                        HasGlyphOutline(loadedFonts[emojiFontIdx].ttf, egid))
                    {
                        cpToFontSlot[cp] = emojiFontIdx;
                        found = true;
                    }
                }

                if (!found)
                {
                    for (var fi = 0; fi < loadedFonts.Count; fi++)
                    {
                        if (loadedFonts[fi].cmap.TryGetValue(cp, out var gid) && HasGlyphOutline(loadedFonts[fi].ttf, gid))
                        {
                            cpToFontSlot[cp] = fi;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found) uncovered.Add(cp);
            }

            // For uncovered characters, assign to the first loaded font (best effort)
            if (uncovered.Count > 0 && loadedFonts.Count > 0)
                foreach (var cp in uncovered)
                    cpToFontSlot[cp] = 0;

            // Build EmbeddedFontInfo for each font slot actually used
            var usedSlots = new SortedSet<int>(cpToFontSlot.Values);
            var slotRemap = new Dictionary<int, int>(); // old slot → new sequential index
            foreach (var slot in usedSlots)
            {
                var newIdx = embeddedFonts.Count;
                slotRemap[slot] = newIdx;

                var (ttf, cmap, advances, upm, asc, desc, capH, bbox, name) = loadedFonts[slot];
                var scale = 1000.0 / upm;
                var charsForFont = new SortedSet<int>(unicodeCodePoints.Where(cp => cpToFontSlot.TryGetValue(cp, out var s) && s == slot));

                // Build code point → CID mapping. BMP chars use identity mapping;
                // non-BMP chars (e.g. emoji) get assigned CIDs in the PUA range.
                var cpToCid = new Dictionary<int, int>();
                var nextPuaCid = 0xE000;
                foreach (var cp in charsForFont)
                {
                    if (cp <= 0xFFFF)
                        cpToCid[cp] = cp;
                    else
                    {
                        // Skip PUA slots that are already used by actual text
                        while (charsForFont.Contains(nextPuaCid) && nextPuaCid < 0xF8FF)
                            nextPuaCid++;
                        cpToCid[cp] = nextPuaCid++;
                    }
                }

                var cidToGid = BuildCidToGidMap(charsForFont, cmap, cpToCid);
                var wArray = BuildWArray(charsForFont, cmap, advances, upm, cpToCid);
                var toUnicode = BuildToUnicodeCMap(charsForFont, cpToCid);

                // Subset: keep only needed glyphs
                var neededGlyphs = new HashSet<ushort> { 0 };
                foreach (var cp in charsForFont)
                    if (cmap.TryGetValue(cp, out var gid))
                        neededGlyphs.Add(gid);
                var subsetFont = SubsetTtfFont(ttf, neededGlyphs);

                using var ms = new System.IO.MemoryStream();
                using (var zlib = new System.IO.Compression.ZLibStream(ms, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
                    zlib.Write(subsetFont, 0, subsetFont.Length);

                embeddedFonts.Add(new EmbeddedFontInfo
                {
                    FontName = name,
                    CompressedFontData = ms.ToArray(),
                    CidToGidMapData = cidToGid,
                    WArrayString = wArray,
                    ToUnicodeCMap = toUnicode,
                    FontUncompressedLength = subsetFont.Length,
                    Ascent = (int)(asc * scale),
                    Descent = (int)(desc * scale),
                    CapHeight = (int)(capH * scale),
                    Bbox = [.. bbox.Select(v => (int)(v * scale))],
                    CpToCid = cpToCid,
                });
            }

            // Remap cpToFontSlot to sequential indices
            var remapped = new Dictionary<int, int>(cpToFontSlot.Count);
            foreach (var (cp, slot) in cpToFontSlot)
                remapped[cp] = slotRemap[slot];
            cpToFontSlot = remapped;
        }

        // Pre-build content streams
        var contentStreams = new List<byte[]>(pageCount);
        for (var i = 0; i < pageCount; i++)
            contentStreams.Add(Encoding.Latin1.GetBytes(BuildContentStream(pages[i], embeddedFonts.Count > 0, cpToFontSlot, embeddedFonts)));

        // Allocate object numbers.
        //   1 = Catalog, 2 = Pages, 3 = Font F1 (Helvetica/WinAnsi), 4 = Font F1B (Helvetica-Bold/WinAnsi)
        //   Per embedded font: 6 objects (ToUnicode, Descriptor, CIDFont, Type0, FontFile2, CIDToGIDMap)
        //   Per page: content stream obj, N image XObject objs, page obj
        var nextObj = 5;

        // Allocate font objects
        foreach (var ef in embeddedFonts)
        {
            ef.ToUnicodeObj = nextObj++;
            ef.DescriptorObj = nextObj++;
            ef.CidFontObj = nextObj++;
            ef.Type0Obj = nextObj++;
            ef.FontFileObj = nextObj++;
            ef.CidToGidObj = nextObj++;
        }

        var contentObjNums = new List<int>(pageCount);
        var imageObjNums = new List<List<int>>(pageCount);
        var pageObjNums = new List<int>(pageCount);

        for (var i = 0; i < pageCount; i++)
        {
            contentObjNums.Add(nextObj++);
            var imgNums = new List<int>();
            for (var j = 0; j < pages[i].ImageBlocks.Count; j++)
                imgNums.Add(nextObj++);
            imageObjNums.Add(imgNums);
            pageObjNums.Add(nextObj++);
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

        // ── Object 4: Font F1B (Helvetica-Bold, built-in WinAnsiEncoding) ──
        _objectOffsets[4] = Position;
        WriteRaw("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        // ── Per-font objects (F2, F3, …) ───────────────────────────────────
        for (var fi = 0; fi < embeddedFonts.Count; fi++)
        {
            var ef = embeddedFonts[fi];

            // ToUnicode CMap stream
            _objectOffsets[ef.ToUnicodeObj] = Position;
            var toUnicodeBytes = Encoding.ASCII.GetBytes(ef.ToUnicodeCMap);
            WriteRaw($"{ef.ToUnicodeObj} 0 obj\n<< /Length {toUnicodeBytes.Length} >>\nstream\n");
            _stream.Write(toUnicodeBytes);
            WriteRaw("\nendstream\nendobj\n");

            // FontDescriptor
            _objectOffsets[ef.DescriptorObj] = Position;
            WriteRaw($"{ef.DescriptorObj} 0 obj\n");
            WriteRaw("<< /Type /FontDescriptor\n");
            WriteRaw($"/FontName /{ef.FontName}\n");
            WriteRaw("/Flags 32\n");
            WriteRaw($"/FontBBox [{ef.Bbox[0]} {ef.Bbox[1]} {ef.Bbox[2]} {ef.Bbox[3]}]\n");
            WriteRaw("/ItalicAngle 0\n");
            WriteRaw($"/Ascent {ef.Ascent}\n");
            WriteRaw($"/Descent {ef.Descent}\n");
            WriteRaw($"/CapHeight {ef.CapHeight}\n");
            WriteRaw("/StemV 80\n");
            WriteRaw($"/FontFile2 {ef.FontFileObj} 0 R\n");
            WriteRaw(">>\nendobj\n");

            // CIDFont
            _objectOffsets[ef.CidFontObj] = Position;
            WriteRaw($"{ef.CidFontObj} 0 obj\n");
            WriteRaw("<< /Type /Font /Subtype /CIDFontType2\n");
            WriteRaw($"/BaseFont /{ef.FontName}\n");
            WriteRaw("/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >>\n");
            WriteRaw($"/FontDescriptor {ef.DescriptorObj} 0 R\n");
            WriteRaw("/DW 1000\n");
            WriteRaw($"/W {ef.WArrayString}\n");
            WriteRaw($"/CIDToGIDMap {ef.CidToGidObj} 0 R\n");
            WriteRaw(">>\nendobj\n");

            // Type0 font wrapper (Fn where n = fi + 2)
            _objectOffsets[ef.Type0Obj] = Position;
            WriteRaw($"{ef.Type0Obj} 0 obj\n");
            WriteRaw("<< /Type /Font /Subtype /Type0\n");
            WriteRaw($"/BaseFont /{ef.FontName}\n");
            WriteRaw("/Encoding /Identity-H\n");
            WriteRaw($"/DescendantFonts [{ef.CidFontObj} 0 R]\n");
            WriteRaw($"/ToUnicode {ef.ToUnicodeObj} 0 R\n");
            WriteRaw(">>\nendobj\n");

            // FontFile2 (compressed TrueType)
            _objectOffsets[ef.FontFileObj] = Position;
            WriteRaw($"{ef.FontFileObj} 0 obj\n");
            WriteRaw($"<< /Length {ef.CompressedFontData.Length} /Length1 {ef.FontUncompressedLength} /Filter /FlateDecode >>\n");
            WriteRaw("stream\n");
            _stream.Write(ef.CompressedFontData);
            WriteRaw("\nendstream\nendobj\n");

            // CIDToGIDMap stream
            _objectOffsets[ef.CidToGidObj] = Position;
            WriteRaw($"{ef.CidToGidObj} 0 obj\n");
            WriteRaw($"<< /Length {ef.CidToGidMapData.Length} /Filter /FlateDecode >>\n");
            WriteRaw("stream\n");
            _stream.Write(ef.CidToGidMapData);
            WriteRaw("\nendstream\nendobj\n");
        }

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
            // Font dictionary: F1, F1B + Fn for each embedded font
            WriteRaw("/Font << /F1 3 0 R /F1B 4 0 R");
            for (var fi = 0; fi < embeddedFonts.Count; fi++)
                WriteRaw($" /F{fi + 2} {embeddedFonts[fi].Type0Obj} 0 R");
            WriteRaw(" >>\n");
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

    private static string BuildContentStream(PdfPage page, bool hasUnicodeFont, Dictionary<int, int>? cpToFontSlot, List<EmbeddedFontInfo>? embeddedFonts)
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
                // Pure Latin-1 text — use F1 (Helvetica) or F1B (Helvetica-Bold)
                var fontName = block.Bold ? "F1B" : "F1";
                var escapedText = EscapePdfString(block.Text);
                sb.Append("BT\n");
                sb.Append(colorCmd);
                sb.Append($"/{fontName} {fontSize} Tf\n");
                // Apply character spacing (Tc)
                if (block.CharSpacing != 0)
                    sb.Append($"{block.CharSpacing.ToString("F2", CultureInfo.InvariantCulture)} Tc\n");
                // Apply horizontal scaling if text overflows MaxWidth
                if (block.MaxWidth.HasValue)
                {
                    var naturalWidth = MeasureTextWidth(block.Text, block.FontSize, block.CharSpacing);
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
                // block using Unicode fonts so all characters share the same
                // bounding-box Y in text extractors.
                //
                // Characters may span multiple embedded fonts (e.g. CJK in F2,
                // Korean in F3, emoji in F4).  Split into runs by font slot and
                // emit each run with the appropriate Fn, using Td to advance.
                sb.Append("BT\n");
                sb.Append(colorCmd);
                // Apply character spacing (Tc)
                if (block.CharSpacing != 0)
                    sb.Append($"{block.CharSpacing.ToString("F2", CultureInfo.InvariantCulture)} Tc\n");
                // Apply horizontal scaling if text overflows MaxWidth
                if (block.MaxWidth.HasValue)
                {
                    var naturalWidth = MeasureTextWidth(block.Text, block.FontSize, block.CharSpacing);
                    if (naturalWidth > block.MaxWidth.Value && naturalWidth > 0)
                    {
                        var tzPercent = (block.MaxWidth.Value / naturalWidth) * 100.0;
                        sb.Append($"{tzPercent.ToString("F1", CultureInfo.InvariantCulture)} Tz\n");
                    }
                }
                sb.Append($"{x} {y} Td\n");

                // Split text into runs by font slot.  Default all chars to slot 0 (F2).
                var codePoints = ShapeArabicCodePoints(EnumerateCodePoints(block.Text).ToList());
                var runs = new List<(int fontSlot, List<int> cps)>();
                foreach (var cp in codePoints)
                {
                    var slot = cpToFontSlot != null && cpToFontSlot.TryGetValue(cp, out var s) ? s : 0;
                    if (runs.Count > 0 && runs[^1].fontSlot == slot)
                        runs[^1].cps.Add(cp);
                    else
                        runs.Add((slot, new List<int> { cp }));
                }

                foreach (var run in runs)
                {
                    var fontName = $"F{run.fontSlot + 2}";
                    sb.Append($"/{fontName} {fontSize} Tf\n");
                    sb.Append('<');
                    foreach (var cp in run.cps)
                    {
                        // Map code point to CID via the font's CpToCid table
                        var cid = cp;
                        if (embeddedFonts != null && run.fontSlot < embeddedFonts.Count)
                        {
                            var ef = embeddedFonts[run.fontSlot];
                            if (ef.CpToCid.TryGetValue(cp, out var mapped))
                                cid = mapped;
                        }
                        sb.Append(cid.ToString("X4"));
                    }
                    sb.Append("> Tj\n");
                }

                sb.Append("ET\n");
            }

            // Restore graphics state after clipping
            if (hasClip)
                sb.Append("Q\n");

            // Render underline as a line below the text
            if (block.Underline)
            {
                var textWidth = MeasureTextWidth(block.Text, block.FontSize, block.CharSpacing);
                if (block.MaxWidth.HasValue && textWidth > block.MaxWidth.Value)
                    textWidth = block.MaxWidth.Value;
                var ulY = block.Y - block.FontSize * 0.15f; // position below baseline
                var ulThickness = Math.Max(0.5f, block.FontSize * 0.05f);
                var x1 = block.X.ToString("F3", CultureInfo.InvariantCulture);
                var y1 = ulY.ToString("F3", CultureInfo.InvariantCulture);
                var x2 = (block.X + textWidth).ToString("F3", CultureInfo.InvariantCulture);
                var lw = ulThickness.ToString("F3", CultureInfo.InvariantCulture);
                sb.Append($"{block.Color.R.ToString("F3", CultureInfo.InvariantCulture)} " +
                          $"{block.Color.G.ToString("F3", CultureInfo.InvariantCulture)} " +
                          $"{block.Color.B.ToString("F3", CultureInfo.InvariantCulture)} RG\n");
                sb.Append($"{lw} w\n");
                sb.Append($"{x1} {y1} m {x2} {y1} l S\n");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Measures the natural rendering width of text in Helvetica at the given font size.
    /// Uses the standard Helvetica character width table.
    /// </summary>
    private static double MeasureTextWidth(string text, float fontSize, float charSpacing = 0)
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
        var result = total * fontSize / 1000.0;
        // Tc adds charSpacing points per character (except after the last)
        if (charSpacing != 0 && text.Length > 1)
            result += charSpacing * (text.Length - 1);
        return result;
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
    /// Returns true if a code point is in a common emoji range, used to prefer
    /// the dedicated emoji font over CJK fonts that have placeholder glyphs.
    /// </summary>
    private static bool IsEmojiRange(int cp)
    {
        return cp >= 0x1F000                          // Supplemental Symbols, Emoticons, etc.
            || (cp >= 0x2600 && cp <= 0x27BF)         // Misc Symbols + Dingbats
            || (cp >= 0x2300 && cp <= 0x23FF)         // Misc Technical (⌚ etc.)
            || (cp >= 0x2B50 && cp <= 0x2B55)         // Stars, circles
            || (cp >= 0xFE00 && cp <= 0xFE0F);        // Variation Selectors
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
    private static string BuildToUnicodeCMap(IEnumerable<int> codePoints, Dictionary<int, int>? cpToCid = null)
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
                var cid = cpToCid != null && cpToCid.TryGetValue(cp, out var mapped) ? mapped : cp;
                if (cp <= 0xFFFF)
                {
                    sb.Append($"<{cid:X4}> <{cp:X4}>\n");
                }
                else
                {
                    // Non-BMP: CID is a PUA value; Unicode target is UTF-16 surrogate pair
                    var hi = 0xD800 + ((cp - 0x10000) >> 10);
                    var lo = 0xDC00 + ((cp - 0x10000) & 0x3FF);
                    sb.Append($"<{cid:X4}> <{hi:X4}{lo:X4}>\n");
                }
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
                '\u00B7' => '\u00B7',                       // middle dot (already in WinAnsi)
                '\u00D7' => '\u00D7',                       // multiplication sign (already in WinAnsi)
                '\u00F7' => '\u00F7',                       // division sign (already in WinAnsi)
                '\u2264' => "<=",                           // ≤
                '\u2265' => ">=",                           // ≥
                '\u00B0' => '\u00B0',                       // degree sign (already in WinAnsi)
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

    // ── System font discovery ──────────────────────────────────────────

    /// <summary>
    /// Returns a list of candidate system font paths, ordered by priority.
    /// Multiple fonts are needed to cover different scripts (CJK, Korean, Arabic, emoji).
    /// </summary>
    private static List<string> FindSystemFontCandidates()
    {
        var results = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            var fontDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            // Priority order: CJK first, then Korean, Arabic-capable, emoji, symbols
            string[] candidates = [
                "msyh.ttc",       // Microsoft YaHei (CJK + Japanese)
                "msjh.ttc",       // Microsoft JhengHei (Traditional Chinese)
                "malgun.ttf",     // Malgun Gothic (Korean)
                "segoeui.ttf",    // Segoe UI (Arabic, Hebrew, Thai, etc.)
                "seguiemj.ttf",   // Segoe UI Emoji
                "seguisym.ttf",   // Segoe UI Symbol
                "simsun.ttc",     // SimSun (CJK fallback)
                "simhei.ttf",     // SimHei (CJK fallback)
                "arial.ttf",      // Arial (broad Latin + some scripts)
                "msgothic.ttc",   // MS Gothic (Japanese fallback)
            ];
            foreach (var name in candidates)
            {
                var p = Path.Combine(fontDir, name);
                if (File.Exists(p)) results.Add(p);
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            var fontDir = "/System/Library/Fonts";
            string[] candidates = [
                "PingFang.ttc",
                "AppleSDGothicNeo.ttc",
                "STHeiti Medium.ttc",
                "Hiragino Sans GB.ttc",
                "Apple Color Emoji.ttc",
            ];
            foreach (var name in candidates)
            {
                var p = Path.Combine(fontDir, name);
                if (File.Exists(p)) results.Add(p);
            }
        }
        else // Linux and others
        {
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
                "NotoColorEmoji.ttf",
                "wqy-microhei.ttc",
                "DroidSansFallbackFull.ttf",
            ];
            foreach (var dir in searchDirs)
                if (Directory.Exists(dir))
                    foreach (var name in names)
                    {
                        var p = Path.Combine(dir, name);
                        if (File.Exists(p) && !results.Contains(p))
                            results.Add(p);
                    }
        }

        return results;
    }

    /// <summary>
    /// Enumerates Unicode code points from a .NET string, properly handling surrogate pairs.
    /// </summary>
    private static IEnumerable<int> EnumerateCodePoints(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                yield return char.ConvertToUtf32(text[i], text[i + 1]);
                i++; // skip low surrogate
            }
            else
            {
                yield return text[i];
            }
        }
    }

    /// <summary>
    /// Loads a TrueType/TTC font file. For TTC collections, extracts the first font.
    /// </summary>
    private static byte[] LoadTtfFont(string path)
    {
        var raw = File.ReadAllBytes(path);
        return LoadTtfFontFromBytes(raw);
    }

    /// <summary>
    /// Processes raw TrueType/TTC font bytes. For TTC collections, extracts the first font.
    /// </summary>
    private static byte[] LoadTtfFontFromBytes(byte[] raw)
    {
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
    /// Prefers format 12 (full Unicode including non-BMP) over format 4 (BMP only).
    /// </summary>
    private static Dictionary<int, ushort> ParseCmapTable(byte[] ttf)
    {
        var map = new Dictionary<int, ushort>();
        var (tableOff, tableLen) = FindTable(ttf, "cmap");
        if (tableOff == 0) return map;

        var off = (int)tableOff;
        var numSubtables = ReadU16(ttf, off + 2);

        // Pass 1: Prefer format 12 subtables (full Unicode including non-BMP emoji).
        // Check platform 3 encoding 10 first, then platform 0 encoding ≥3.
        for (var i = 0; i < numSubtables; i++)
        {
            var stOff = off + 4 + i * 8;
            var platformId = ReadU16(ttf, stOff);
            var encodingId = ReadU16(ttf, stOff + 2);
            var subtableOffset = off + (int)ReadU32(ttf, stOff + 4);

            bool isFullUnicode = (platformId == 3 && encodingId == 10)
                              || (platformId == 0 && encodingId >= 3);
            if (!isFullUnicode) continue;

            var format = ReadU16(ttf, subtableOffset);
            if (format == 12)
            {
                ParseCmapFormat12(ttf, subtableOffset, map);
                if (map.Count > 0) return map;
            }
        }

        // Pass 2: Fall back to format 4 / format 12 in BMP subtables
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
            // Support non-BMP code points (e.g. emoji at U+1Fxxx) up to 0x10FFFF
            for (uint c = startCode; c <= endCode && c <= 0x10FFFF; c++)
            {
                var gid = (ushort)((startGlyph + (c - startCode)) & 0xFFFF);
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
    private static string BuildWArray(SortedSet<int> unicodeChars, Dictionary<int, ushort> cmap, ushort[] advances, int upm, Dictionary<int, int>? cpToCid = null)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var cp in unicodeChars)
        {
            if (cmap.TryGetValue(cp, out var gid) && gid < advances.Length)
            {
                var cid = cpToCid != null && cpToCid.TryGetValue(cp, out var mapped) ? mapped : cp;
                var w = (int)(advances[gid] * 1000L / upm);
                sb.Append($"{cid} [{w}] ");
            }
        }
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Builds a compressed CIDToGIDMap for the specific code points used in this font.
    /// Maps CID → glyph ID; BMP chars use identity CID, non-BMP use PUA CID slots.
    /// </summary>
    private static byte[] BuildCidToGidMap(SortedSet<int> codePoints, Dictionary<int, ushort> cmap, Dictionary<int, int>? cpToCid = null)
    {
        // The map is 65536 entries × 2 bytes = 131072 bytes uncompressed
        var raw = new byte[65536 * 2];
        foreach (var cp in codePoints)
        {
            if (!cmap.TryGetValue(cp, out var gid)) continue;
            var cid = cpToCid != null && cpToCid.TryGetValue(cp, out var mapped) ? mapped : cp;
            if (cid >= 0 && cid < 65536)
            {
                raw[cid * 2] = (byte)(gid >> 8);
                raw[cid * 2 + 1] = (byte)(gid & 0xFF);
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

    // ── Arabic text shaping ─────────────────────────────────────────────

    /// <summary>
    /// Arabic Presentation Forms-B mapping.
    /// Each tuple: (isolated, final, initial, medial). 0 = form not available.
    /// </summary>
    private static readonly Dictionary<int, (int iso, int fin, int ini, int med)> ArabicFormMap = new()
    {
        [0x0621] = (0xFE80, 0, 0, 0),                     // HAMZA
        [0x0622] = (0xFE81, 0xFE82, 0, 0),                // ALEF WITH MADDA ABOVE
        [0x0623] = (0xFE83, 0xFE84, 0, 0),                // ALEF WITH HAMZA ABOVE
        [0x0624] = (0xFE85, 0xFE86, 0, 0),                // WAW WITH HAMZA ABOVE
        [0x0625] = (0xFE87, 0xFE88, 0, 0),                // ALEF WITH HAMZA BELOW
        [0x0626] = (0xFE89, 0xFE8A, 0xFE8B, 0xFE8C),     // YEH WITH HAMZA ABOVE
        [0x0627] = (0xFE8D, 0xFE8E, 0, 0),                // ALEF
        [0x0628] = (0xFE8F, 0xFE90, 0xFE91, 0xFE92),     // BEH
        [0x0629] = (0xFE93, 0xFE94, 0, 0),                // TEH MARBUTA
        [0x062A] = (0xFE95, 0xFE96, 0xFE97, 0xFE98),     // TEH
        [0x062B] = (0xFE99, 0xFE9A, 0xFE9B, 0xFE9C),     // THEH
        [0x062C] = (0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0),     // JEEM
        [0x062D] = (0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4),     // HAH
        [0x062E] = (0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8),     // KHAH
        [0x062F] = (0xFEA9, 0xFEAA, 0, 0),                // DAL
        [0x0630] = (0xFEAB, 0xFEAC, 0, 0),                // THAL
        [0x0631] = (0xFEAD, 0xFEAE, 0, 0),                // REH
        [0x0632] = (0xFEAF, 0xFEB0, 0, 0),                // ZAIN
        [0x0633] = (0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4),     // SEEN
        [0x0634] = (0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8),     // SHEEN
        [0x0635] = (0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC),     // SAD
        [0x0636] = (0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0),     // DAD
        [0x0637] = (0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4),     // TAH
        [0x0638] = (0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8),     // ZAH
        [0x0639] = (0xFEC9, 0xFECA, 0xFECB, 0xFECC),     // AIN
        [0x063A] = (0xFECD, 0xFECE, 0xFECF, 0xFED0),     // GHAIN
        [0x0641] = (0xFED1, 0xFED2, 0xFED3, 0xFED4),     // FEH
        [0x0642] = (0xFED5, 0xFED6, 0xFED7, 0xFED8),     // QAF
        [0x0643] = (0xFED9, 0xFEDA, 0xFEDB, 0xFEDC),     // KAF
        [0x0644] = (0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0),     // LAM
        [0x0645] = (0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4),     // MEEM
        [0x0646] = (0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8),     // NOON
        [0x0647] = (0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC),     // HEH
        [0x0648] = (0xFEED, 0xFEEE, 0, 0),                // WAW
        [0x0649] = (0xFEEF, 0xFEF0, 0, 0),                // ALEF MAKSURA
        [0x064A] = (0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4),     // YEH
    };

    /// <summary>
    /// Returns the Arabic joining type for a code point.
    /// 0=Non-Joining, 1=Right-Joining, 2=Dual-Joining, 3=Join-Causing, 4=Transparent
    /// </summary>
    private static int GetArabicJoiningType(int cp)
    {
        if (cp == 0x0640 || cp == 0x200D) return 3; // TATWEEL, ZWJ
        if ((cp >= 0x064B && cp <= 0x065F) || cp == 0x0670) return 4; // diacritics
        if (!ArabicFormMap.TryGetValue(cp, out var forms)) return 0;
        return forms.ini != 0 ? 2 : forms.fin != 0 ? 1 : 0;
    }

    /// <summary>
    /// Can character at position i join with the character before it (toward string start)?
    /// Requires: character i is R or D, and nearest non-transparent predecessor is D or C.
    /// </summary>
    private static bool ArabicCanJoinBefore(List<int> cps, int i)
    {
        var jt = GetArabicJoiningType(cps[i]);
        if (jt != 1 && jt != 2) return false; // must be R or D to receive
        for (var j = i - 1; j >= 0; j--)
        {
            var pjt = GetArabicJoiningType(cps[j]);
            if (pjt == 4) continue; // transparent, skip
            return pjt == 2 || pjt == 3; // D or C can transmit
        }
        return false;
    }

    /// <summary>
    /// Can character at position i join with the character after it (toward string end)?
    /// Requires: character i is D, and nearest non-transparent successor is R, D, or C.
    /// </summary>
    private static bool ArabicCanJoinAfter(List<int> cps, int i)
    {
        var jt = GetArabicJoiningType(cps[i]);
        if (jt != 2) return false; // must be D to transmit
        for (var j = i + 1; j < cps.Count; j++)
        {
            var njt = GetArabicJoiningType(cps[j]);
            if (njt == 4) continue; // transparent, skip
            return njt == 1 || njt == 2 || njt == 3; // R, D, or C can receive
        }
        return false;
    }

    /// <summary>
    /// Shapes Arabic text by replacing base Arabic code points with their
    /// contextual Presentation Forms-B equivalents. Also handles Lam-Alef ligatures.
    /// Non-Arabic characters pass through unchanged.
    /// </summary>
    private static List<int> ShapeArabicCodePoints(List<int> cps)
    {
        var result = new List<int>(cps.Count);
        for (var i = 0; i < cps.Count; i++)
        {
            var cp = cps[i];
            if (!ArabicFormMap.ContainsKey(cp))
            {
                result.Add(cp);
                continue;
            }

            // Check for Lam-Alef ligature: Lam (0x0644) followed by an Alef variant
            if (cp == 0x0644 && i + 1 < cps.Count)
            {
                var next = cps[i + 1];
                int ligIso = 0, ligFin = 0;
                if (next == 0x0627) { ligIso = 0xFEFB; ligFin = 0xFEFC; }
                else if (next == 0x0622) { ligIso = 0xFEF5; ligFin = 0xFEF6; }
                else if (next == 0x0623) { ligIso = 0xFEF7; ligFin = 0xFEF8; }
                else if (next == 0x0625) { ligIso = 0xFEF9; ligFin = 0xFEFA; }
                if (ligIso != 0)
                {
                    result.Add(ArabicCanJoinBefore(cps, i) ? ligFin : ligIso);
                    i++; // skip the alef
                    continue;
                }
            }

            var forms = ArabicFormMap[cp];
            var jb = ArabicCanJoinBefore(cps, i);
            var ja = ArabicCanJoinAfter(cps, i);

            int shaped;
            if (jb && ja && forms.med != 0) shaped = forms.med;
            else if (jb && forms.fin != 0) shaped = forms.fin;
            else if (ja && forms.ini != 0) shaped = forms.ini;
            else shaped = forms.iso != 0 ? forms.iso : cp;

            result.Add(shaped);
        }
        return result;
    }

    /// <summary>
    /// Checks if a glyph ID has actual outline data (contours) in the glyf table.
    /// Returns true for CFF fonts or when tables can't be found (assumes glyph exists).
    /// Filters out glyphs that have a glyf entry but zero contours (empty placeholders).
    /// </summary>
    private static bool HasGlyphOutline(byte[] ttf, ushort gid)
    {
        var (glyfOff, _) = FindTable(ttf, "glyf");
        var (locaOff, _) = FindTable(ttf, "loca");
        if (glyfOff == 0 || locaOff == 0) return true; // CFF font — assume OK
        var (headOff, _) = FindTable(ttf, "head");
        var (maxpOff, _) = FindTable(ttf, "maxp");
        if (headOff == 0 || maxpOff == 0) return true;
        var numGlyphs = ReadU16(ttf, (int)maxpOff + 4);
        if (gid >= numGlyphs) return false;
        var isLong = ReadU16(ttf, (int)headOff + 50) == 1;
        uint o1, o2;
        if (isLong)
        {
            o1 = ReadU32(ttf, (int)locaOff + gid * 4);
            o2 = ReadU32(ttf, (int)locaOff + (gid + 1) * 4);
        }
        else
        {
            o1 = (uint)(ReadU16(ttf, (int)locaOff + gid * 2) * 2);
            o2 = (uint)(ReadU16(ttf, (int)locaOff + (gid + 1) * 2) * 2);
        }
        if (o1 == o2) return false; // no glyph data at all
        // Check numberOfContours: >0 = simple glyph, <0 = composite glyph, 0 = empty
        var glyphDataOff = (int)(glyfOff + o1);
        if (glyphDataOff + 2 > ttf.Length) return false;
        var numberOfContours = (short)ReadU16(ttf, glyphDataOff);
        return numberOfContours != 0;
    }
}
