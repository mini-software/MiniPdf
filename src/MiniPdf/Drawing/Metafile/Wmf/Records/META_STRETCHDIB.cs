using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     Specifies the transfer of color data from a block of pixels in device-independent format according to a raster operation, with possible expansion or contraction.
    /// </summary>
    /// <remarks>
    ///     2.3.1.6 META_STRETCHDIB Record
    /// </remarks>
    internal class META_STRETCHDIB : Record
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
        public DeviceIndependentBitmap Target { get; set; }

        public META_STRETCHDIB()
        {
            Header.RecordFunction = RecordType.META_STRETCHDIB;
            Target = new DeviceIndependentBitmap();
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
                Target.Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            using var payloadStream = new MemoryStream();
            using var payloadWriter = new BinaryWriter(payloadStream);

            payloadWriter.Write((uint)RasterOperation);
            payloadWriter.Write(SrcHeight);
            payloadWriter.Write(SrcWidth);
            payloadWriter.Write(YSrc);
            payloadWriter.Write(XSrc);
            payloadWriter.Write(DestHeight);
            payloadWriter.Write(DestWidth);
            payloadWriter.Write(YDest);
            payloadWriter.Write(XDest);
            Target.Write(payloadWriter);

            Header.RecordSize = (uint)((payloadStream.Length + 6) / 2);
            Header.Write(writer);
            writer.Write(payloadStream.ToArray());
        }
    }
}
