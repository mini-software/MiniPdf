namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines values that are used for specifying characteristics of a font. The values are used to indicate whether the characters in a font have a fixed or variable width, or pitch.
    /// </summary>
    /// <remarks>
    ///     2.1.1.24 PitchFont Enumeration
    /// </remarks>
    internal enum PitchFont
    {
        /// <summary>
        ///     The default pitch, which is implementation-dependent.
        /// </summary>
        DEFAULT_PITCH = 0,

        /// <summary>
        ///     A fixed pitch, which means that all the characters in the font occupy the same width when output in a string.
        /// </summary>
        FIXED_PITCH = 1,

        /// <summary>
        ///     A variable pitch, which means that the characters in the font occupy widths that are proportional to the actual widths of the glyphs when output in a string. For example, the "i" and space characters usually have much smaller widths than a "W" or "O" character.
        /// </summary>
        VARIABLE_PITCH = 2
    }
}
