namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The PointS Object defines the x- and y-coordinates of a point.
    /// </summary>
    /// <remarks>
    ///     2.2.2.16 PointS Object
    /// </remarks>
    internal struct PointS
    {
        /// <summary>
        ///     A 16-bit signed integer that defines the horizontal (x) coordinate of the point.
        /// </summary>
        public int x;

        /// <summary>
        ///     A 16-bit signed integer that defines the vertical (y) coordinate of the point.
        /// </summary>
        public int y;

        public void Read(BinaryReader reader)
        {
            this.x = reader.ReadInt16();
            this.y = reader.ReadInt16();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((short)this.x);
            writer.Write((short)this.y);
        }
    }
}
