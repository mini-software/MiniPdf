using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The ExtTextOutOptions enumeration specifies parameters that control various aspects of the output of text by EMR_SMALLTEXTOUT (section 2.3.5.37) records and in EmrText objects.
    /// </summary>
    /// <remarks>
    ///     2.1.11 ExtTextOutOptions Enumeration
    /// </remarks>
    internal enum ExtTextOutOptions
    {
        /// <summary>
        ///     This bit indicates that the current background color SHOULD be used to fill the rectangle.
        /// </summary>
        ETO_OPAQUE = 0x00000002,
        /// <summary>
        ///     This bit indicates that the text SHOULD be clipped to the rectangle.
        /// </summary>
        ETO_CLIPPED = 0x00000004,

        /// <summary>
        ///     This bit indicates that the codes for characters in an output text string are actually indexes of the character glyphs in a TrueType font. Glyph indexes are font-specific, so to display the correct characters on playback, the font that is used MUST be identical to the font used to generate the indexes.
        /// </summary>
        ETO_GLYPH_INDEX = 0x00000010,

        ETO_RTLREADING = 0x00000080,
        /// <summary>
        ///     This bit indicates that the record does not specify a bounding rectangle for the text output.
        /// </summary>
        ETO_NO_RECT = 0x00000100,
        ETO_SMALL_CHARS = 0x00000200,
        ETO_NUMERICSLOCAL = 0x00000400,
        ETO_NUMERICSLATIN = 0x00000800,
        ETO_IGNORELANGUAGE = 0x00001000,
        ETO_PDY = 0x00002000,
        ETO_REVERSE_INDEX_MAP = 0x00010000
    }
}
