using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The ColorMatchToTarget enumeration is used to determine whether a color profile has been embedded in the metafile.
    /// </summary>
    /// <remarks>
    ///     2.1.6 ColorMatchToTarget Enumeration
    /// </remarks>
    internal enum ColorMatchToTarget
    {
        /// <summary>
        ///     Indicates that a color profile has not been embedded in the metafile.
        /// </summary>
        ColorMatchToTargetNotembedded = 0x00000000,

        /// <summary>
        ///     Indicates that a color profile has been embedded in the metafile.
        /// </summary>
        ColorMatchToTargetEmbedded = 0x00000001
    }
}
