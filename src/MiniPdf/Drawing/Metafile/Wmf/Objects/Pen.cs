using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    /// <summary>
    ///     The Pen Object specifies the style, width, and color of a pen.
    /// </summary>
    /// <remarks>
    ///     2.2.1.4 Pen Object
    /// </remarks>
    internal class Pen
    {
        /// <summary>
        ///     A 16-bit unsigned integer that specifies the pen style. The value MUST be defined from the PenStyle Enumeration table.
        /// </summary>
        public PenStyle PenStyle;

        /// <summary>
        ///     A 32-bit PointS Object that specifies a point for the object dimensions. The x-coordinate is the pen width. The y-coordinate is ignored.
        /// </summary>
        public PointS Width;

        /// <summary>
        ///     A 32-bit ColorRef Object that specifies the pen color value.
        /// </summary>
        public ColorRef ColorRef;

        public void Read(BinaryReader reader)
        {
            this.PenStyle = (PenStyle)reader.ReadUInt16();
            this.Width.Read(reader);
            this.ColorRef.Read(reader);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((ushort)this.PenStyle);
            this.Width.Write(writer);
            this.ColorRef.Write(writer);
        }
    }
}
