namespace MiniSoftware.Drawing.Metafile.Wmf.Flags
{
    using System;

    /// <summary>
    ///     Specify the relationship between a reference point and a bounding rectangle, for text alignment. These flags can be combined to specify multiple options, with the restriction that only one flag can be chosen that alters the drawing position in the playback device context.
    /// </summary>
    /// <remarks>
    ///     Vertical text alignment is performed when the font has a vertical default baseline, such as Kanji.
    /// 
    ///     2.1.2.4 VerticalTextAlignmentMode Flags
    /// </remarks>
    [Flags]
    internal enum VerticalTextAlignmentMode
    {
        /// <summary>
        ///     The reference point MUST be on the top edge of the bounding rectangle.
        /// </summary>
        VTA_TOP = 0x0000,

        /// <summary>
        ///     The reference point MUST be on the bottom edge of the bounding rectangle.
        /// </summary>
        VTA_BOTTOM = 0x0002,

        /// <summary>
        ///     The reference point MUST be aligned vertically with the center of the bounding rectangle.
        /// </summary>
        VTA_CENTER = 0x0006,

        /// <summary>
        ///     The reference point MUST be on the left edge of the bounding rectangle.
        /// </summary>
        VTA_LEFT = 0x0008,

        /// <summary>
        ///     The reference point MUST be on the baseline of the text.
        /// </summary>
        VTA_BASELINE = 0x0018
    }
}
