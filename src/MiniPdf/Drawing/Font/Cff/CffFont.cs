using System;
using System.IO;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.Encoding;
using MiniPdf.Drawing.Font.Glyphs;
using MiniPdf.Drawing.Font.Rendering;
using MiniPdf.Drawing.Font.Ttf;

namespace MiniPdf.Drawing.Font.Cff
{
    /// <summary>
    /// Represents a loaded CFF (Compact Font Format) or OTF/CFF font face.
    /// Charstrings are decoded via <see cref="Type2Interpreter"/>.
    /// </summary>
    public sealed class CffFont : IFont
    {
        // ── Private state ─────────────────────────────────────────────────────

        private readonly FontMetrics _metrics;
        private readonly IFontEncoding _encoding;
        private readonly CffGlyphAccessor _glyphAccessor;

        // ── Internal access for rendering / serialisation ─────────────────────

        internal CffData CffData { get; }

        /// <summary>
        /// The sfnt table set when this font was loaded from an OTF container;
        /// <see langword="null"/> for standalone raw CFF files.
        /// </summary>
        internal TtfTables? SfntTables { get; }

        /// <summary>
        /// Raw bytes of the "CFF " table when loaded from an OTF container;
        /// <see langword="null"/> for standalone raw CFF files.
        /// </summary>
        internal byte[]? RawCffBytes { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        /// <param name="cffData">Parsed CFF binary data.</param>
        /// <param name="sfntTables">
        /// OpenType sfnt table set (for OTF/CFF), or <see langword="null"/> for
        /// standalone CFF files.
        /// </param>
        /// <param name="rawCffBytes">
        /// Raw bytes of the "CFF " table for round-trip serialisation,
        /// or <see langword="null"/> for standalone CFF files.
        /// </param>
        internal CffFont(CffData cffData, TtfTables? sfntTables, byte[]? rawCffBytes = null)
        {
            CffData      = cffData ?? throw new ArgumentNullException(nameof(cffData));
            SfntTables   = sfntTables;
            RawCffBytes  = rawCffBytes;

            _metrics      = BuildMetrics(cffData, sfntTables);
            _encoding     = BuildEncoding(sfntTables);
            _glyphAccessor = new CffGlyphAccessor(cffData);

            if (sfntTables != null)
                NumGlyphs = sfntTables.Maxp.NumGlyphs;
            else
                NumGlyphs = cffData.CharStrings.Length;

            Style = sfntTables != null
                ? ResolveStyle(sfntTables.Head.MacStyle)
                : FontFaceStyle.Regular;
        }

        // ── IFont ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string FontName =>
            CffData.FullName ?? CffData.FontName;

        /// <inheritdoc/>
        public string FontFamily =>
            CffData.FamilyName ?? CffData.FontName;

        /// <inheritdoc/>
        public int NumGlyphs { get; }

        /// <inheritdoc/>
        public FontFaceStyle Style { get; }

        /// <inheritdoc/>
        public IFontMetrics Metrics => _metrics;

        /// <inheritdoc/>
        public IFontEncoding Encoding => _encoding;

        /// <inheritdoc/>
        public IGlyphAccessor GlyphAccessor => _glyphAccessor;

        /// <inheritdoc/>
        public void Save(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            CffFontWriter.Write(this, stream);
        }

        /// <inheritdoc/>
        public void Save(string filePath)
        {
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                                          FileShare.None, bufferSize: 4096);
            Save(fs);
        }

        /// <summary>
        /// Returns a string representation of this CFF font, including its name and glyph count.
        /// </summary>
        public override string ToString() =>
            $"CffFont(\"{FontName}\", {NumGlyphs} glyphs)";

        // ── Helpers ───────────────────────────────────────────────────────────

        private static FontMetrics BuildMetrics(CffData cff, TtfTables? sfnt)
        {
            var m = new FontMetrics(gid =>
            {
                if (!(gid is GlyphUInt32Id uid)) return 0.0;
                int idx = (int)uid.Value;
                if (idx < 0 || idx >= cff.CharStrings.Length) return 0.0;

                var priv = GetPrivateDictStatic(cff, idx);
                var interp = new Type2Interpreter();
                // Use a no-op painter to extract width only
                return interp.Interpret(cff.CharStrings[idx], NopPainter.Instance, priv, cff.GlobalSubrs);
            });

            if (sfnt != null)
            {
                m.Ascender   = sfnt.Hhea.Ascender;
                m.Descender  = sfnt.Hhea.Descender;
                m.LineGap    = sfnt.Hhea.LineGap;
                m.UnitsPerEM = sfnt.Head.UnitsPerEm;

                if (sfnt.Os2Table != null)
                {
                    m.TypoAscender  = sfnt.Os2Table.TypoAscender;
                    m.TypoDescender = sfnt.Os2Table.TypoDescender;
                }
                else
                {
                    m.TypoAscender  = sfnt.Hhea.Ascender;
                    m.TypoDescender = sfnt.Hhea.Descender;
                }
            }
            else
            {
                // Standalone CFF: derive metrics from FontBBox
                // FontBBox = [xMin, yMin, xMax, yMax]
                m.Ascender   = cff.FontBBox[3];
                m.Descender  = cff.FontBBox[1];
                m.UnitsPerEM = cff.FontBBox[3] - cff.FontBBox[1];
                if (m.UnitsPerEM <= 0) m.UnitsPerEM = 1000; // typical CFF default
                m.TypoAscender  = m.Ascender;
                m.TypoDescender = m.Descender;
            }

            return m;
        }

        private static IFontEncoding BuildEncoding(TtfTables? sfnt)
        {
            if (sfnt?.CMapTable != null)
            {
                var best = sfnt.CMapTable.FindUnicodeTable();
                if (best != null) return new TtfUnicodeEncoding(best);
            }
            return NullEncoding.Instance;
        }

        private static FontFaceStyle ResolveStyle(ushort macStyle)
        {
            bool bold   = (macStyle & 0x01) != 0;
            bool italic = (macStyle & 0x02) != 0;
            if (bold && italic) return FontFaceStyle.BoldItalic;
            if (bold)           return FontFaceStyle.Bold;
            if (italic)         return FontFaceStyle.Italic;
            return FontFaceStyle.Regular;
        }

        private static CffPrivateDict GetPrivateDictStatic(CffData cff, int gid)
        {
            if (cff.IsCIDFont && cff.GlyphFDIndex != null && cff.CIDFontDicts != null)
            {
                int fdIdx = gid < cff.GlyphFDIndex.Length ? cff.GlyphFDIndex[gid] : 0;
                if (fdIdx >= 0 && fdIdx < cff.CIDFontDicts.Length)
                    return cff.CIDFontDicts[fdIdx].PrivateDict;
            }
            return cff.PrivateDict;
        }

        // ── No-op painter for width-only measurement ──────────────────────────

        private sealed class NopPainter : IGlyphOutlinePainter
        {
            public static readonly NopPainter Instance = new NopPainter();
            public void MoveTo(MoveTo cmd)   { }
            public void LineTo(LineTo cmd)   { }
            public void CurveTo(CurveTo cmd) { }
            public void ClosePath() { }
        }
    }
}
