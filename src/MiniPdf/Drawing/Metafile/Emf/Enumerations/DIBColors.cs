using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The DIBColors enumeration defines how to interpret the values in the color table of a DIB.
    /// </summary>
    /// <remarks>
    ///     2.1.9 DIBColors Enumeration
    /// </remarks>
    internal enum DIBColors
    {
        /// <summary>
        ///     The color table contains literal RGB values.
        /// </summary>
        DibRgbColors = 0x00,

        /// <summary>
        ///     The color table consists of an array of 16-bit indexes into the LogPalette object (section 2.2.17) that is currently defined in the playback device context.
        /// </summary>
        DibPalColors = 0x01,

        /// <summary>
        ///     No color table exists. The pixels in the DIB are indices into the current logical palette in the playback device context.
        /// </summary>
        DibPalIndices = 0x02
    }
}
