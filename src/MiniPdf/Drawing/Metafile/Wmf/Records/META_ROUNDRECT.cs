using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The META_ROUNDRECT record paints a rectangle with rounded corners. 
    ///     The rectangle is outlined using the pen and filled using the brush, as defined in the playback device context.
    /// </summary>
    /// <remarks>
    ///     2.3.3.18 META_ROUNDRECT Record
    /// </remarks>
    internal class META_ROUNDRECT : Record
    {
        /// <summary>
        ///     Defines the height, in logical coordinates, of the ellipse used to draw the rounded corners.
        /// </summary>
        public short Height { get; set; }

        /// <summary>
        ///     Defines the width, in logical coordinates, of the ellipse used to draw the rounded corners.
        /// </summary>
        public short Width { get; set; }

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

        public META_ROUNDRECT()
        {
            Header.RecordFunction = RecordType.META_ROUNDRECT;
        }

        public override void Read(BinaryReader reader)
        {
            Height = reader.ReadInt16();
            Width = reader.ReadInt16();
            BottomRect = reader.ReadInt16();
            RightRect = reader.ReadInt16();
            TopRect = reader.ReadInt16();
            LeftRect = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(BottomRect);
            writer.Write(RightRect);
            writer.Write(TopRect);
            writer.Write(LeftRect);
        }
    }
}
