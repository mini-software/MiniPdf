using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_EXTFLOODFILL record fills an area with the brush that is defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.4 META_EXTFLOODFILL Record
    /// </remarks>
    internal class META_EXTFLOODFILL : Record
    {
        /// <summary>
        ///     Defines the fill operation to be performed.
        /// </summary>
        public FloodFill Mode;

        /// <summary>
        ///     A 32-bit ColorRef Object that defines the color value.
        /// </summary>
        public ColorRef ColorRef;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the point to be set.
        /// </summary>
        public short Y;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the point to be set.
        /// </summary>
        public short X;
    }
}
