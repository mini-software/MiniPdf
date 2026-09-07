using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The BackgroundMode enumeration is used to specify the background mode to be used with text, hatched brushes, and pen styles that are not solid. The background mode determines how to combine the background with foreground text, hatched brushes, and pen styles that are not solid lines.
    /// </summary>
    /// <remarks>
    ///     2.1.4 BackgroundMode Enumeration
    /// </remarks>
    internal enum BackgroundMode
    {
        TRANSPARENT = 0x0001,
        OPAQUE = 0x0002
    }
}
