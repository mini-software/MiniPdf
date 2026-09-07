namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The PointL Object defines the coordinates of a point.
    /// </summary>
    /// <remarks>
    ///     2.2.2.15 PointL Object
    /// </remarks>
    internal struct PointL
    {
        /// <summary>
        ///     A 32-bit signed integer that defines the horizontal (x) coordinate of the point.
        /// </summary>
        public int x;

        /// <summary>
        ///     A 32-bit signed integer that defines the vertical (y) coordinate of the point.
        /// </summary>
        public int y;

        public void Read(BinaryReader reader)
        {
            this.x = reader.ReadInt32();
            this.y = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.x);
            writer.Write(this.y);
        }
    }
}
