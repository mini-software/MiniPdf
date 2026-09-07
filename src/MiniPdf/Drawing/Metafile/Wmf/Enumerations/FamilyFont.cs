namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the font family. Font families describe the look of a font in a general way. 
    ///     They are intended for specifying fonts when the exact typeface desired is not available.
    /// </summary>
    /// <remarks>
    ///     2.1.1.8 FamilyFont Enumeration
    /// </remarks>
    internal enum FamilyFont
    {
        /// <summary>
        ///     The default font is specified, which is implementation-dependent.
        /// </summary>
        FF_DONTCARE = 0x00,

        /// <summary>
        ///     Fonts with variable stroke widths, which are proportional to the actual widths of the glyphs, and which have serifs. "MS Serif" is an example.
        /// </summary>
        FF_ROMAN = 0x01,

        /// <summary>
        ///     Fonts with variable stroke widths, which are proportional to the actual widths of the glyphs, and which do not have serifs. "MS Sans Serif" is an example.
        /// </summary>
        FF_SWISS = 0x02,

        /// <summary>
        ///     Fonts with constant stroke width, with or without serifs. Fixed-width fonts are usually modern. "Pica", "Elite", and "Courier New" are examples.
        /// </summary>
        FF_MODERN = 0x03,

        /// <summary>
        ///     Fonts designed to look like handwriting. "Script" and "Cursive" are examples.
        /// </summary>
        FF_SCRIPT = 0x04,

        /// <summary>
        ///     Novelty fonts. "Old English" is an example.
        /// </summary>
        FF_DECORATIVE = 0x05
    }
}
