using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_PATBLT record paints a specified rectangle using the brush that is defined in the playback device context. 
    ///     The brush color and the surface color or colors are combined using the specified raster operation.
    /// </summary>
    internal class META_PATBLT : Record
    {
        /// <summary>
        ///     Defines the raster operation code.
        /// </summary>
        public TernaryRasterOperation RasterOperation { get; set; }

        /// <summary>
        ///     Defines the height, in logical units, of the rectangle.
        /// </summary>
        public short Height { get; set; }

        /// <summary>
        ///     Defines the width, in logical units, of the rectangle.
        /// </summary>
        public short Width { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the rectangle to be filled.
        /// </summary>
        public short YLeft { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the rectangle to be filled.
        /// </summary>
        public short XLeft { get; set; }

        public META_PATBLT()
        {
            Header.RecordFunction = RecordType.META_PATBLT;
        }

        public override void Read(BinaryReader reader)
        {
            RasterOperation = (TernaryRasterOperation)reader.ReadUInt32();
            Height = reader.ReadInt16();
            Width = reader.ReadInt16();
            YLeft = reader.ReadInt16();
            XLeft = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((uint)RasterOperation);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(YLeft);
            writer.Write(XLeft);
        }
    }
}
