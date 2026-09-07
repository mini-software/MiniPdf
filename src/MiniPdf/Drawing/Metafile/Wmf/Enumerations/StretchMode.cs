namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies the bitmap stretching mode, which defines how the system combines rows or columns of a bitmap with existing pixels.
    /// </summary>
    /// <remarks>
    ///     2.1.1.30 StretchMode Enumeration
    /// </remarks>
    internal enum StretchMode
    {
        /// <summary>
        ///     Performs a Boolean AND operation by using the color values for the eliminated and existing pixels. 
        ///     If the bitmap is a monochrome bitmap, this mode preserves black pixels at the expense of white pixels.
        /// </summary>
        BLACKONWHITE = 0x0001,

        /// <summary>
        ///     Performs a Boolean OR operation by using the color values for the eliminated and existing pixels. 
        ///     If the bitmap is a monochrome bitmap, this mode preserves white pixels at the expense of black pixels.
        /// </summary>
        WHITEONBLACK = 0x0002,

        /// <summary>
        ///     Deletes the pixels. This mode deletes all eliminated lines of pixels without trying to preserve their information.
        /// </summary>
        COLORONCOLOR = 0x0003,

        /// <summary>
        ///     Maps pixels from the source rectangle into blocks of pixels in the destination rectangle. The average color over the destination block of pixels approximates the color of the source pixels.
        /// </summary>
        HALFTONE = 0x0004
    }
}
