namespace MiniSoftware.Drawing.Metafile.Wmf.Flags
{
    using System;

    /// <summary>
    ///     Specify the relationship between a reference point and a bounding rectangle, for text alignment. 
    ///     These flags can be combined to specify multiple options, with the restriction that only one flag can be chosen that alters the drawing position in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.1.2.3 TextAlignmentMode Flags
    /// </remarks>
    [Flags]
    internal enum TextAlignmentMode
    {
        /// <summary>
        ///     The drawing position in the playback device context MUST NOT be updated after each text output call. 
        ///     The reference point MUST be passed to the text output function.
        /// </summary>
        TA_NOUPDATECP = 0x0000,

        /// <summary>
        ///     The drawing position in the playback device context MUST be updated after each text output call. 
        ///     It MUST be used as the reference point.
        /// </summary>
        TA_UPDATECP = 0x0001,

        /// <summary>
        ///     The reference point MUST be on the right edge of the bounding rectangle.
        /// </summary>
        TA_RIGHT = 0x0002,

        /// <summary>
        ///     The reference point MUST be aligned horizontally with the center of the bounding rectangle.
        /// </summary>
        TA_CENTER = 0x0006,

        /// <summary>
        ///     The reference point MUST be on the bottom edge of the bounding rectangle.
        /// </summary>
        TA_BOTTOM = 0x0008,

        /// <summary>
        ///     The reference point MUST be on the baseline of the text.
        /// </summary>
        TA_BASELINE = 0x0018,

        /// <summary>
        ///     The text MUST be laid out in right-to-left reading order, instead of the default left-to-right order. This SHOULD be applied only when the font that is defined in the playback device context is either Hebrew or Arabic.
        /// </summary>
        TA_RTLREADING = 0x0100
    }
}
