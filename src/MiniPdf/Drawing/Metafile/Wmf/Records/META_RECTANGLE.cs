using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_RECTANGLE record paints a rectangle. The rectangle is outlined by using the pen and filled by using the brush that are defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.17 META_RECTANGLE Record
    /// </remarks>
    internal class META_RECTANGLE : Record
    {
        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the lower-right corner of the rectangle.
        /// </summary>
        public short BottomRect { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the lower-right corner of the rectangle.
        /// </summary>
        public short RightRect { get; set; }

        /// <summary>
        ///     Defines the y-coordinate, in logical units, of the upper-left corner of the rectangle.
        /// </summary>
        public short TopRect { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, of the upper-left corner of the rectangle.
        /// </summary>
        public short LeftRect { get; set; }

        public META_RECTANGLE()
        {
            Header.RecordFunction = RecordType.META_RECTANGLE;
        }

        public override void Read(BinaryReader reader)
        {
            BottomRect = reader.ReadInt16();
            RightRect = reader.ReadInt16();
            TopRect = reader.ReadInt16();
            LeftRect = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(BottomRect);
            writer.Write(RightRect);
            writer.Write(TopRect);
            writer.Write(LeftRect);
        }
    }
}
