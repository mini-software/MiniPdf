using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The HatchStyle enumeration is an extension to the WMF HatchStyle enumeration ([MS-WMF] section 2.1.1.12).
    /// </summary>
    /// <remarks>
    ///     2.1.17 HatchStyle Enumeration
    /// </remarks>
    internal enum HatchStyle
    {
        HS_SOLIDCLR = 0x0006,
        HS_DITHEREDCLR = 0x0007,
        HS_SOLIDTEXTCLR = 0x0008,
        HS_DITHEREDTEXTCLR = 0x0009,
        HS_SOLIDBKCLR = 0x000A,
        HS_DITHEREDBKCLR = 0x000B
    }
}
