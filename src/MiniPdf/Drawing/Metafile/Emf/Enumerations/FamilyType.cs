using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The FamilyType enumeration defines values for one of the characteristics in the PANOSE system for classifying typefaces.
    /// </summary>
    /// <remarks>
    ///     2.1.12 FamilyType Enumeration
    /// </remarks>
    internal enum FamilyType
    {
        /// <summary>
        ///     Any.
        /// </summary>
        PanAny = 0x00,

        /// <summary>
        ///     No fit.
        /// </summary>
        PanNoFit = 0x01,
        PanFamilyTextDisplay = 0x02,
        PanFamilyScript = 0x03,
        PanFamilyDecorative = 0x04,
        PanFamilyPictorial = 0x05
    }
}
