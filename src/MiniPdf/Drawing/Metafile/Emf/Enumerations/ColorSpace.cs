using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The ColorSpace enumeration is used to specify when to turn color proofing on and off, and when to delete transforms.
    /// </summary>
    /// <remarks>
    ///     2.1.7 ColorSpace Enumeration
    /// </remarks>
    internal enum ColorSpace
    {
        /// <summary>
        ///     Maps colors to the target device's color gamut. This enables color proofing. All subsequent draw commands to the playback device context will render colors as they would appear on the target device.
        /// </summary>
        CsEnable = 0x00000001,

        /// <summary>
        ///     Disables color proofing.
        /// </summary>
        CsDisable = 0x00000002,

        /// <summary>
        ///     If color management is enabled for the target profile, disables it and deletes the concatenated transform.
        /// </summary>
        CsDeleteTransform = 0x00000003
    }
}
