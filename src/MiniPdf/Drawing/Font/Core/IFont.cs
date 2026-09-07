using System;
using System.IO;
using MiniPdf.Drawing.Font.Encoding;
using MiniPdf.Drawing.Font.Glyphs;

namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// Represents a loaded font face and provides access to its metadata, metrics,
    /// encoding map, and glyph data.
    /// </summary>
    public interface IFont
    {
        /// <summary>Full name of the font, e.g. "Montserrat Regular".</summary>
        string FontName { get; }

        /// <summary>Family name of the font, e.g. "Montserrat".</summary>
        string FontFamily { get; }

        /// <summary>Total number of glyphs in the font.</summary>
        int NumGlyphs { get; }

        /// <summary>Typographic style of this font face.</summary>
        FontFaceStyle Style { get; }

        /// <summary>Font-level metrics (ascender, descender, units per em, etc.).</summary>
        IFontMetrics Metrics { get; }

        /// <summary>Encoding map that translates Unicode code points to glyph identifiers.</summary>
        IFontEncoding Encoding { get; }

        /// <summary>Provides access to individual glyphs by identifier or index.</summary>
        IGlyphAccessor GlyphAccessor { get; }

        /// <summary>Serialises the (possibly modified) font to <paramref name="stream"/>.</summary>
        void Save(Stream stream);

        /// <summary>Serialises the (possibly modified) font to the file at <paramref name="filePath"/>.</summary>
        void Save(string filePath);
    }
}
