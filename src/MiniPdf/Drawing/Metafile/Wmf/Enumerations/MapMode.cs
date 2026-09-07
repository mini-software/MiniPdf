using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines how logical units are mapped to physical units; that is, assuming that the origins in both the logical and physical coordinate systems are at the same point on the drawing surface, what is the physical coordinate (x',y') that corresponds to logical coordinate (x,y).
    /// </summary>
    internal enum MapMode
    {
        /// <summary>
        ///     Each logical unit is mapped to one device pixel. Positive x is to the right; positive y is down.
        /// </summary>
        MM_TEXT = 0x0001,

        /// <summary>
        ///     Each logical unit is mapped to 0.1 millimeter. Positive x is to the right; positive y is up.
        /// </summary>
        MM_LOMETRIC = 0x0002,

        /// <summary>
        ///     Each logical unit is mapped to 0.01 millimeter. Positive x is to the right; positive y is up.
        /// </summary>
        MM_HIMETRIC = 0x0003,

        /// <summary>
        ///     Each logical unit is mapped to 0.01 inch. Positive x is to the right; positive y is up.
        /// </summary>
        MM_LOENGLISH = 0x0004,

        /// <summary>
        ///     Each logical unit is mapped to 0.001 inch. Positive x is to the right; positive y is up.
        /// </summary>
        MM_HIENGLISH = 0x0005,

        /// <summary>
        ///     Each logical unit is mapped to one twentieth (1/20) of a point. In printing, a point is 1/72 of an inch; therefore, 1/20 of a point is 1/1440 of an inch. This unit is also known as a "twip".
        /// </summary>
        MM_TWIPS = 0x0006,

        /// <summary>
        ///     Logical units are mapped to arbitrary device units with equally scaled axes; that is, one unit along the x-axis is equal to one unit along the y-axis. The META_SETWINDOWEXT and META_SETVIEWPORTEXT records specify the units and the orientation of the axes.
        /// </summary>
        MM_ISOTROPIC = 0x0007,

        /// <summary>
        ///     Logical units are mapped to arbitrary units with arbitrarily scaled axes.
        /// </summary>
        MM_ANISOTROPIC = 0x0008
    }
}
