namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// Identifies the format of a font file.
    /// </summary>
    public enum FontType
    {
        /// <summary>TrueType font (.ttf).</summary>
        TTF,

        /// <summary>OpenType font with TrueType outlines (.otf treated as TTF internally).</summary>
        OTF,

        /// <summary>OpenType font with CFF/Type 2 outlines (.otf with "CFF " table).</summary>
        CFF,

        /// <summary>Adobe Type 1 font (.pfb / .pfa).</summary>
        Type1,

        /// <summary>Web Open Font Format 1 (.woff) — zlib-compressed sfnt tables.</summary>
        WOFF,

        /// <summary>Web Open Font Format 2 (.woff2) — Brotli-compressed sfnt tables.</summary>
        WOFF2,
    }
}
