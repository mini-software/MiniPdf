using MiniSoftware.Drawing.Metafile.Wmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal abstract class AbstractBitmapRecord
    {
        public RecordHeader Header;

        /// <summary>
        ///     Defines how the source pixels, the current brush in the playback device context, and the destination pixels are to be combined to form the new image.
        /// </summary>
        public TernaryRasterOperation RasterOperation;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the source rectangle.
        /// </summary>
        public short YSrc;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the source rectangle.
        /// </summary>
        public short XSrc;

        /// <summary>
        ///     Defines the height, in logical units, of the source and destination rectangles.
        /// </summary>
        public short Height;

        /// <summary>
        ///     Defines the width, in logical units, of the source and destination rectangles.
        /// </summary>
        public short Width;

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public short YDest;

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public short XDest;

        /// <summary>
        ///     Defines source image content. This object MUST be specified, even if the raster operation does not require a source.
        /// </summary>
        public Bitmap16? Target;

        public bool HasBitmap
        {
            get
            {
                return this.Header.RecordSize == (((int)this.Header.RecordFunction >> 8) + 3);
            }
        }
    }
}
