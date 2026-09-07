using System;
using System.IO;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.Encoding;
using MiniPdf.Drawing.Font.Glyphs;

namespace MiniPdf.Drawing.Font.Ttf
{
    /// <summary>
    /// Represents a loaded TrueType or OpenType/TT font face.
    /// Implements <see cref="IFont"/>; exposes the full parsed table set via
    /// <see cref="TtfTables"/>.
    /// </summary>
    public sealed class TtfFont : IFont
    {
        // ── Private state ─────────────────────────────────────────────────────

        private readonly FontMetrics _metrics;
        private readonly IFontEncoding _encoding;
        private readonly TtfGlyphAccessor _glyphAccessor;

        // ── Constructor ───────────────────────────────────────────────────────

        internal TtfFont(TtfTables tables)
        {
            TtfTables = tables ?? throw new ArgumentNullException(nameof(tables));

            // Build metrics from hhea + optional OS/2.
            _metrics = BuildMetrics(tables);

            // Build encoding from best Unicode cmap subtable.
            _encoding = BuildEncoding(tables);

            // Build glyph accessor.
            _glyphAccessor = new TtfGlyphAccessor(tables);
        }

        // ── TtfFont-specific property ─────────────────────────────────────────

        /// <summary>All parsed OpenType tables for this font.</summary>
        public TtfTables TtfTables { get; }

        // ── IFont ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string FontName =>
            TtfTables.Name.FullName
            ?? TtfTables.Name.PostScriptName
            ?? TtfTables.Name.FamilyName
            ?? "(unknown)";

        /// <inheritdoc/>
        public string FontFamily =>
            TtfTables.Name.FamilyName ?? "(unknown)";

        /// <inheritdoc/>
        public int NumGlyphs => TtfTables.Maxp.NumGlyphs;

        /// <inheritdoc/>
        public FontFaceStyle Style => ResolveStyle(TtfTables.Head.MacStyle);

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
            TtfFontWriter.Write(this, stream);
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
        /// Returns a string representation of this TrueType font.
        /// </summary>
        public override string ToString() =>
            $"TtfFont(\"{FontName}\", {NumGlyphs} glyphs)";

        // ── Helpers ───────────────────────────────────────────────────────────

        private static FontMetrics BuildMetrics(TtfTables t)
        {
            var m = new FontMetrics(gid =>
            {
                if (gid is GlyphUInt32Id uid)
                    return t.Hmtx.GetAdvanceWidth((int)uid.Value);
                return 0.0;
            });

            m.Ascender    = t.Hhea.Ascender;
            m.Descender   = t.Hhea.Descender;
            m.LineGap     = t.Hhea.LineGap;
            m.UnitsPerEM  = t.Head.UnitsPerEm;

            // OS/2 typographic values are preferred when available.
            if (t.Os2Table != null)
            {
                m.TypoAscender    = t.Os2Table.TypoAscender;
                m.TypoDescender   = t.Os2Table.TypoDescender;
                m.StrikeoutPosition = t.Os2Table.StrikeoutPosition;
                m.StrikeoutSize     = t.Os2Table.StrikeoutSize;
            }
            else
            {
                m.TypoAscender  = t.Hhea.Ascender;
                m.TypoDescender = t.Hhea.Descender;
            }

            // post table underline metrics.
            if (t.Post != null)
            {
                m.UnderlinePosition  = t.Post.UnderlinePosition;
                m.UnderlineThickness = t.Post.UnderlineThickness;
            }

            return m;
        }

        private static IFontEncoding BuildEncoding(TtfTables t)
        {
            if (t.CMapTable != null)
            {
                var best = t.CMapTable.FindUnicodeTable();
                if (best != null)
                    return new TtfUnicodeEncoding(best);
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
    }
}
