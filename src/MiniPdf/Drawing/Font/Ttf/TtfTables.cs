using MiniPdf.Drawing.Font.Ttf.Tables;

namespace MiniPdf.Drawing.Font.Ttf
{
    /// <summary>
    /// Property bag that holds every parsed OpenType table for a TrueType/OTF font.
    /// Required tables are non-nullable; optional tables (absent from some fonts)
    /// are nullable.
    /// </summary>
    public sealed class TtfTables
    {
        // ── Required tables ───────────────────────────────────────────────────

        /// <summary>"head" — font header: unitsPerEm, bounds, indexToLocFormat.</summary>
        public TtfHeadTable Head { get; }

        /// <summary>"hhea" — horizontal header: ascender, descender, lineGap, numberOfHMetrics.</summary>
        public TtfHheaTable Hhea { get; }

        /// <summary>"maxp" — maximum profile: numGlyphs.</summary>
        public TtfMaxpTable Maxp { get; }

        /// <summary>"name" — naming table: family name, full name, PostScript name, etc.</summary>
        public TtfNameTable Name { get; }

        /// <summary>"hmtx" — horizontal metrics: advance widths and left side-bearings.</summary>
        public TtfHmtxTable Hmtx { get; }

        // ── Optional tables ───────────────────────────────────────────────────

        /// <summary>"OS/2" — Windows metrics and license flags. Absent in some old Mac fonts.</summary>
        public TtfOs2Table? Os2Table { get; }

        /// <summary>"post" — PostScript compatibility info: italic angle, isFixedPitch.</summary>
        public TtfPostTable? Post { get; }

        /// <summary>
        /// "loca" — glyph offset index. Present only in TrueType-outline fonts;
        /// null for CFF-flavored OTF.
        /// </summary>
        public TtfLocaTable? Loca { get; }

        /// <summary>
        /// "glyf" — TrueType glyph outline data. Null for CFF-flavored OTF.
        /// </summary>
        public TtfGlyfTable? GlyfTable { get; }

        /// <summary>"cmap" — character-to-glyph-index map. Null for symbol/private-use fonts.</summary>
        public TtfCMapTable? CMapTable { get; }

        /// <summary>"kern" — horizontal kerning pairs (format 0). Null when absent or Apple AAT only.</summary>
        public TtfKernTable? KernTable { get; }

        /// <summary>"GDEF" — glyph classification (Base, Ligature, Mark, Component). Null when absent.</summary>
        public TtfGdefTable? GdefTable { get; }

        /// <summary>"GSUB" — substitution feature tags (e.g. "liga", "calt"). Null when absent.</summary>
        public TtfGsubTable? GsubTable { get; }

        /// <summary>"GPOS" — positioning feature tags (e.g. "kern", "mark"). Null when absent.</summary>
        public TtfGposTable? GposTable { get; }

        /// <summary>"fvar" — variation axes and named instances. Null in non-variable fonts.</summary>
        public TtfFvarTable? FvarTable { get; }

        /// <summary>"gvar" — per-glyph tuple variation data. Null in non-variable fonts.</summary>
        public TtfGvarTable? GvarTable { get; }

        /// <summary>"avar" — axis variation segment maps. Null in non-variable fonts.</summary>
        public TtfAvarTable? AvarTable { get; }

        /// <summary>"CBDT"/"CBLC" — colour bitmap glyph images. Null in non-colour fonts.</summary>
        public TtfCbdtTable? CbdtTable { get; }

        /// <summary>"SVG " — SVG glyph outlines. Null in non-SVG fonts.</summary>
        public TtfSvgTable? SvgTable { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        internal TtfTables(
            TtfHeadTable   head,
            TtfHheaTable   hhea,
            TtfMaxpTable   maxp,
            TtfNameTable   name,
            TtfHmtxTable   hmtx,
            TtfOs2Table?   os2Table,
            TtfPostTable?  post,
            TtfLocaTable?  loca,
            TtfGlyfTable?  glyfTable,
            TtfCMapTable?  cmapTable,
            TtfKernTable?  kernTable  = null,
            TtfGdefTable?  gdefTable  = null,
            TtfGsubTable?  gsubTable  = null,
            TtfGposTable?  gposTable  = null,
            TtfFvarTable?  fvarTable  = null,
            TtfGvarTable?  gvarTable  = null,
            TtfAvarTable?  avarTable  = null,
            TtfCbdtTable?  cbdtTable  = null,
            TtfSvgTable?   svgTable   = null)
        {
            Head      = head;
            Hhea      = hhea;
            Maxp      = maxp;
            Name      = name;
            Hmtx      = hmtx;
            Os2Table  = os2Table;
            Post      = post;
            Loca      = loca;
            GlyfTable = glyfTable;
            CMapTable = cmapTable;
            KernTable = kernTable;
            GdefTable = gdefTable;
            GsubTable = gsubTable;
            GposTable = gposTable;
            FvarTable = fvarTable;
            GvarTable = gvarTable;
            AvarTable = avarTable;
            CbdtTable = cbdtTable;
            SvgTable  = svgTable;
        }
    }
}
