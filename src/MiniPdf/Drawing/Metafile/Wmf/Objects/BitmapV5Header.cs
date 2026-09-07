namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The BitmapV5Header Object contains information about the dimensions and color format of a device-independent bitmap (DIB). 
    ///     It is an extension of the BitmapV4Header object (section 2.2.2.4).
    /// </summary>
    /// <remarks>
    ///     2.2.2.5 BitmapV5Header Object
    /// </remarks>
    internal class BitmapV5Header
    {
        /// <summary>
        ///     A BitmapV4Header object, which defines properties of the DIB.
        /// </summary>
        public BitmapV4Header BitmapV4Header { get; set; } = new();

        /// <summary>
        ///     A 32-bit unsigned integer that defines the rendering intent for the DIB. 
        ///     This MUST be defined in the LogicalColorSpace enumeration (section 2.1.1.14).
        /// </summary>
        public uint Intent { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that defines the offset, in bytes, from the beginning of this structure to the start of the color profile data.
        /// </summary>
        public uint ProfileData { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that defines the size, in bytes, of embedded color profile data.
        /// </summary>
        public uint ProfileSize { get; set; }
    }
}
