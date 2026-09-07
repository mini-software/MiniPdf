namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The Rect Object defines a rectangle.
    /// </summary>
    /// <remarks>
    ///     2.2.2.18 Rect Object
    /// </remarks>
    internal struct Rect
    {
        /// <summary>
        ///     A 16-bit signed integer that defines the x-coordinate, in logical coordinates, of the upper-left corner of the rectangle
        /// </summary>
        public short Left;

        /// <summary>
        ///     A 16-bit signed integer that defines the y-coordinate, in logical coordinates, of the upper-left corner of the rectangle.
        /// </summary>
        public short Top;

        /// <summary>
        ///     A 16-bit signed integer that defines the x-coordinate, in logical coordinates, of the lower-right corner of the rectangle.
        /// </summary>
        public short Right;

        /// <summary>
        ///     A 16-bit signed integer that defines the y-coordinate, in logical coordinates, of the lower-right corner of the rectangle.
        /// </summary>
        public short Bottom;

        public void Read(BinaryReader reader)
        {
            this.Left = reader.ReadInt16();
            this.Top = reader.ReadInt16();
            this.Right = reader.ReadInt16();
            this.Bottom = reader.ReadInt16();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Left);
            writer.Write(this.Top);
            writer.Write(this.Right);
            writer.Write(this.Bottom);
        }
    }
}
