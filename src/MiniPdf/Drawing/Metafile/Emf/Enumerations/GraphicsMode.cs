using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The GraphicsMode enumeration is used to specify how to interpret shape data such as rectangle coordinates.
    /// </summary>
    /// <remarks>
    ///     2.1.16 GraphicsMode Enumeration
    /// </remarks>
    internal enum GraphicsMode
    {
        /// <summary>
        ///     TrueType text MUST be written from left to right and right side up, even if the rest of the graphics are rotated about the x-axis or y-axis because of the current world-to-
        ///     device transformation in the playback device context. Only the height of the text SHOULD be scaled.
        /// </summary>
        GM_COMPATIBLE = 0x00000001,
        GM_ADVANCED = 0x00000002
    }
}
