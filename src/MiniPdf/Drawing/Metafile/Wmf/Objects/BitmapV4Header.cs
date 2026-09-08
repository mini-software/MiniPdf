using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The BitmapV4Header Object contains information about the dimensions and color format of a device-independent bitmap (DIB). 
    ///     It is an extension of the BitmapInfoHeader object.
    /// </summary>
    /// <remarks>
    ///     2.2.2.4 BitmapV4Header Object
    /// </remarks>
    internal class BitmapV4Header
    {
        /// <summary>
        ///     A BitmapInfoHeader object, which defines properties of the DIB.
        /// </summary>
        public BitmapInfoHeader BitmapInfoHeader;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the color mask that specifies the red component of each pixel. 
        ///     If the Compression value in the BitmapInfoHeader object is not BI_BITFIELDS, this value MUST be ignored.
        /// </summary>
        public uint RedMask;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the color mask that specifies the green component of each pixel. 
        ///     If the Compression value in the BitmapInfoHeader object is not BI_BITFIELDS, this value MUST be ignored.
        /// </summary>
        public uint GreenMask;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the color mask that specifies the blue component of each pixel. 
        ///     If the Compression value in the BitmapInfoHeader object is not BI_BITFIELDS, this value MUST be ignored.
        /// </summary>
        public uint BlueMask;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the color mask that specifies the alpha component of each pixel.
        /// </summary>
        public uint AlphaMask;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the color space of the Device Independent Bitmap object (section 2.2.2.9). 
        ///     If this value is LCS_CALIBRATED_RGB from the LogicalColorSpace enumeration (section 2.1.1.14), the color values in the DIB are calibrated RGB values, and the endpoints and gamma values in this structure SHOULD be used to translate the color values before they are passed to the device.
        /// </summary>
        public uint ColorSpaceType;

        /// <summary>
        ///     A CIEXYZTriple object (section 2.2.2.7) that defines the CIE chromaticity x, y, and z coordinates of the three colors that correspond to the red, green, and blue endpoints for the logical color space associated with the DIB. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public CIEXYZTriple Endpoints;

        //TODO Fixed point
        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for red. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaRed;

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for green. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaGreen;

        /// <summary>
        ///     A 32-bit fixed point value that defines the toned response curve for blue. 
        ///     If the ColorSpaceType field does not specify LCS_CALIBRATED_RGB, this field MUST be ignored.
        /// </summary>
        public uint GammaBlue;
    }
}
