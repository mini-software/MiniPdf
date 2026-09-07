using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The GradientFill enumeration defines the modes for gradient fill operations.
    /// </summary>
    /// <remarks>
    ///     2.1.15 GradientFill Enumeration
    /// </remarks>
    internal enum GradientFill
    {
        /// <summary>
        ///     A mode in which color interpolation is performed along a gradient from the left to the right edges of a rectangle.
        /// </summary>
        GradientFillRectH = 0x00000000,

        /// <summary>
        ///     A mode in which color interpolation is performed along a gradient from the top to the bottom edges of a rectangle.
        /// </summary>
        GradientFillRectV = 0x00000001,

        /// <summary>
        ///     A mode in which color interpolation is performed between vertexes of a triangle.
        /// </summary>
        GradientFillTriangle = 0x00000002
    }
}
