using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The ArmStyle enumeration defines values for one of the characteristics in the PANOSE system for classifying typefaces.
    /// </summary>
    internal enum ArmStyle
    {
        PAN_ANY = 0x00,
        PAN_NO_FIT = 0x01,
        PAN_STRAIGHT_ARMS_HORZ = 0x02,
        PAN_STRAIGHT_ARMS_WEDGE = 0x03,
        PAN_STRAIGHT_ARMS_VERT = 0x04,
        /// <summary>
        /// Straight arms/single-serif.
        /// </summary>
        PAN_STRAIGHT_ARMS_SINGLE_SERIF = 0x05,
        PAN_STRAIGHT_ARMS_DOUBLE_SERIF = 0x06,
        PAN_BENT_ARMS_HORZ = 0x07,
        PAN_BENT_ARMS_WEDGE = 0x08,
        PAN_BENT_ARMS_VERT = 0x09,
        PAN_BENT_ARMS_SINGLE_SERIF = 0x0A,
        PAN_BENT_ARMS_DOUBLE_SERIF = 0x0B
    }
}
