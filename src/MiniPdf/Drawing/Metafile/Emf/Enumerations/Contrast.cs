using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The Contrast enumeration defines values for one of the characteristics in the PANOSE system for classifying typefaces.
    /// </summary>
    /// <remarks>
    ///     2.1.8 Contrast Enumeration
    /// </remarks>
    internal enum Contrast
    {
        PAN_ANY = 0x00,
        PAN_NO_FIT = 0x01,
        PAN_CONTRAST_NONE = 0x02,
        PAN_CONTRAST_VERY_LOW = 0x03,
        PAN_CONTRAST_LOW = 0x04,
        PAN_CONTRAST_MEDIUM_LOW = 0x05,
        PAN_CONTRAST_MEDIUM = 0x06,
        PAN_CONTRAST_MEDIUM_HIGH = 0x07,
        PAN_CONTRAST_HIGH = 0x08,
        PAN_CONTRAST_VERY_HIGH = 0x09
    }
}
