using System;

namespace MiniSoftware.Drawing.Font.Cff
{
    /// <summary>
    /// Internal data holder produced by <see cref="CffParser"/>.
    /// Stores the parsed CFF table in a font-type-agnostic form.
    /// </summary>
    internal sealed class CffData
    {
        /// <summary>PostScript name (first Name INDEX entry).</summary>
        public string FontName = string.Empty;

        /// <summary>Full name resolved from Top DICT FullName SID.</summary>
        public string? FullName;

        /// <summary>Family name resolved from Top DICT FamilyName SID.</summary>
        public string? FamilyName;

        /// <summary>FontBBox array [xMin, yMin, xMax, yMax] in font units.</summary>
        public double[] FontBBox = new double[4];

        /// <summary>Raw charstring bytes, one entry per glyph (indexed by GID).</summary>
        public byte[][] CharStrings = Array.Empty<byte[]>();

        /// <summary>
        /// Glyph names resolved from the charset table, indexed by GID.
        /// Entry 0 is always ".notdef". Empty when no charset is present.
        /// </summary>
        public string[] GlyphNames = Array.Empty<string>();

        /// <summary>Private DICT for simple (non-CID) fonts.</summary>
        public CffPrivateDict PrivateDict = new CffPrivateDict();

        /// <summary>Global subroutines shared across all glyphs.</summary>
        public byte[][]? GlobalSubrs;

        // ── CID font support ──────────────────────────────────────────────────

        /// <summary><see langword="true"/> when ROS / FDArray / FDSelect are present.</summary>
        public bool IsCIDFont;

        /// <summary>Per-FD Private DICTs for CID fonts; null for simple fonts.</summary>
        public CffCIDFontDict[]? CIDFontDicts;

        /// <summary>Maps each GID to an FD index into <see cref="CIDFontDicts"/>.</summary>
        public int[]? GlyphFDIndex;
    }

    /// <summary>
    /// Per-Font-Dict data within a CID-keyed CFF font.
    /// Each FD has its own Private DICT (and thus its own default/nominal widths and local subrs).
    /// </summary>
    internal sealed class CffCIDFontDict
    {
        public CffPrivateDict PrivateDict = new CffPrivateDict();
    }
}
