namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Abstract base for a parsed OpenType CMap format subtable.
    /// Concrete implementations: <see cref="TtfCMapFormat0Table"/> (format 0),
    /// <see cref="TtfCMapFormat4Table"/> (format 4, BMP),
    /// <see cref="TtfCMapFormat12Table"/> (format 12, full Unicode).
    /// </summary>
    public abstract class TtfCMapFormatBaseTable
    {
        /// <summary>Platform identifier (0=Unicode, 1=Macintosh, 3=Windows).</summary>
        public ushort PlatformId { get; }

        /// <summary>Platform-specific encoding identifier.</summary>
        public ushort PlatformSpecificId { get; }

        /// <summary>CMap subtable format number (0, 4, 12, etc.).</summary>
        public ushort Format { get; }

        /// <summary>
        /// Initializes a new <see cref="TtfCMapFormatBaseTable"/> with the specified parameters.
        /// </summary>
        /// <param name="platformId">Platform identifier (0=Unicode, 1=Macintosh, 3=Windows).</param>
        /// <param name="platformSpecificId">Platform-specific encoding identifier.</param>
        /// <param name="format">CMap subtable format number.</param>
        protected TtfCMapFormatBaseTable(ushort platformId, ushort platformSpecificId, ushort format)
        {
            PlatformId         = platformId;
            PlatformSpecificId = platformSpecificId;
            Format             = format;
        }

        /// <summary>
        /// Maps a Unicode code point to a glyph index.
        /// Returns 0 (.notdef) when no glyph is mapped for the code point.
        /// </summary>
        public abstract uint GetGlyphIndex(uint codePoint);
    }
}
