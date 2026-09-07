using System;

namespace MiniSoftware.Drawing.Metafile.Wmf.Flags
{
    /// <summary>
    ///     Specify various characteristics of the output of text. These flags can be combined to specify multiple options.
    /// </summary>
    /// <remarks>
    ///     2.1.2.2 ExtTextOutOptions Flags
    /// </remarks>
    [Flags]
    internal enum ExtTextOutOptions
    {
        /// <summary>
        ///     Indicates that the background color that is defined in the playback device context SHOULD be used to fill the rectangle.
        /// </summary>
        ETO_OPAQUE = 0x0002,

        /// <summary>
        ///     Indicates that the text SHOULD be clipped to the rectangle.
        /// </summary>
        ETO_CLIPPED = 0x0004,

        /// <summary>
        ///     Indicates that the string to be output SHOULD NOT require further processing with respect to the placement of the characters, 
        ///     and an array of character placement values SHOULD be provided. This character placement process is
        ///     useful for fonts in which diacritical characters affect character spacing.
        /// </summary>
        ETO_GLYPH_INDEX = 0x0010,

        /// <summary>
        ///     Indicates that the text MUST be laid out in right-to-left reading order, instead of the default left-to-right order. 
        ///     This SHOULD be applied only when the font that is defined in the playback device context is either Hebrew or Arabic.
        /// </summary>
        ETO_RTLREADING = 0x0080,

        /// <summary>
        ///     Indicates that to display numbers, digits appropriate to the locale SHOULD be used.
        /// </summary>
        ETO_NUMERICSLOCAL = 0x0400,

        /// <summary>
        ///     Indicates that to display numbers, European digits SHOULD be used.
        /// </summary>
        ETO_NUMERICSLATIN = 0x0800,

        /// <summary>
        ///     Indicates that both horizontal and vertical character displacement values SHOULD be provided.
        /// </summary>
        ETO_PDY = 0x2000
    }
}
