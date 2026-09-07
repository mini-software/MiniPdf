using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     specifies the transfer of a block of pixels according to a raster operation, with possible expansion or contraction.
    /// </summary>
    /// <remarks>
    ///     2.3.1.5 META_STRETCHBLT Record
    /// </remarks>
    internal class META_STRETCHBLT : Record
    {
        /// <summary>
        ///     Defines how the source pixels, the current brush in the playback device context, and the destination pixels are to be combined to form the new image.
        /// </summary>
        public TernaryRasterOperation RasterOperation { get; set; }

        /// <summary>
        ///     Defines the height, in logical units, of the source rectangle.
        /// </summary>
        public short SrcHeight { get; set; }

        /// <summary>
        ///     Defines the width, in logical units, of the source rectangle.
        /// </summary>
        public short SrcWidth { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the source rectangle.
        /// </summary>
        public short YSrc { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the source rectangle.
        /// </summary>
        public short XSrc { get; set; }

        /// <summary>
        ///     Defines the height, in logical units, of the destination rectangle.
        /// </summary>
        public short DestHeight { get; set; }

        /// <summary>
        ///     Defines the width, in logical units, of the destination rectangle.
        /// </summary>
        public short DestWidth { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public short YDest { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the destination rectangle.
        /// </summary>
        public short XDest { get; set; }

        /// <summary>
        ///     Defines source image content. This object MUST be specified, even if the raster operation does not require a source.
        /// </summary>
        public Bitmap16? Target { get; set; }

        public META_STRETCHBLT()
        {
            Header.RecordFunction = RecordType.META_STRETCHBLT;
        }

        public override void Read(BinaryReader reader)
        {
            long endPosition = reader.BaseStream.Position + (Header.RecordSize * 2) - 6;

            RasterOperation = (TernaryRasterOperation)reader.ReadUInt32();
            SrcHeight = reader.ReadInt16();
            SrcWidth = reader.ReadInt16();
            YSrc = reader.ReadInt16();
            XSrc = reader.ReadInt16();
            DestHeight = reader.ReadInt16();
            DestWidth = reader.ReadInt16();
                        YDest = reader.ReadInt16();
            XDest = reader.ReadInt16();

            if (reader.BaseStream.Position < endPosition)
            {
                var target = new Bitmap16();
                target.Read(reader);
                Target = target;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((uint)RasterOperation);
            writer.Write(SrcHeight);
            writer.Write(SrcWidth);
            writer.Write(YSrc);
            writer.Write(XSrc);
            writer.Write(DestHeight);
            writer.Write(DestWidth);
            writer.Write(YDest);
            writer.Write(XDest);

            if (Target.HasValue)
            {
                Target.Value.Write(writer);
            }
        }
    }
}
