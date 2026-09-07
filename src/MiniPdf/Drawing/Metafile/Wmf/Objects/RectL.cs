namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The RectL Object defines a rectangle.
    /// </summary>
    /// <remarks>
    ///     2.2.2.19 RectL Object
    /// </remarks>
    internal struct RectL
    {
        /// <summary>
        ///     A 32-bit signed integer that defines the x-coordinate, in logical coordinates, of the upper-left corner of the rectangle
        /// </summary>
        public int Left;

        /// <summary>
        ///     A 32-bit signed integer that defines the y-coordinate, in logical coordinates, of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        ///     A 32-bit signed integer that defines the x-coordinate, in logical coordinates, of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        ///     A 32-bit signed integer that defines the y-coordinate, in logical coordinates, of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        public void Read(BinaryReader reader)
        {
            this.Left = reader.ReadInt32();
            this.Top = reader.ReadInt32();
            this.Right = reader.ReadInt32();
            this.Bottom = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Left);
            writer.Write(this.Top);
            writer.Write(this.Right);
            writer.Write(this.Bottom);
        }
    }
}
