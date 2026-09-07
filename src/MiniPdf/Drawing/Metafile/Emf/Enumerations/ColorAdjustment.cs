using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The ColorAdjustment enumeration is used to specify how the output image should be prepared when the stretch mode is HALFTONE.
    /// </summary>
    /// <remarks>
    ///     2.1.5 ColorAdjustment Enumeration
    /// </remarks>
    internal enum ColorAdjustment
    {
        /// <summary>
        ///     Specifies that the negative of the original image SHOULD be displayed.
        /// </summary>
        CA_NEGATIVE = 0x0001,

        /// <summary>
        ///     Specifies that a logarithmic process SHOULD be applied to the final density of the output colors. This will increase the color contrast when the luminance is low.
        /// </summary>
        CA_LOG_FILTER = 0x0002
    }
}
