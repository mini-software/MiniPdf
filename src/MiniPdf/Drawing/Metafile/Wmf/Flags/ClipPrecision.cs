namespace MiniSoftware.Drawing.Metafile.Wmf.Flags
{
    using System;

    /// <summary>
    /// Specify clipping precision, which defines how to clip characters that are partially outside a clipping region.
    /// </summary>
    [Flags]
    internal enum ClipPrecision
    {
        /// <summary>
        ///     Specifies that default clipping MUST be used.
        /// </summary>
        CLIP_DEFAULT_PRECIS = 0x00000000,

        /// <summary>
        ///     This value SHOULD NOT be used.
        /// </summary>
        CLIP_CHARACTER_PRECIS = 0x00000001,

        /// <summary>
        ///     This value MAY be returned when enumerating rasterized, TrueType and vector fonts.
        /// </summary>
        CLIP_STROKE_PRECIS = 0x00000002,

        /// <summary>
        ///     This value is used to control font rotation.
        /// </summary>
        CLIP_LH_ANGLES = 0x00000010,

        /// <summary>
        /// This value should not be used.
        /// </summary>
        CLIP_TT_ALWAYS = 0x00000020,

        /// <summary>
        /// This value specifies that font association should be turned off.
        /// </summary>
        CLIP_DFA_DISABLE = 0x00000040,

        /// <summary>
        ///     This value specifies that font embedding MUST be used to render document content; embedded fonts are read-only.
        /// </summary>
        CLIP_EMBEDDED = 0x00000080
    }
}
