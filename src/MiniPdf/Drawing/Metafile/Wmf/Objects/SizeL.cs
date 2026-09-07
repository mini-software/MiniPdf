namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The SizeL Object defines the x- and y-extents of a rectangle.
    /// </summary>
    /// <remarks>
    ///     2.2.2.22 SizeL Object
    /// </remarks>
    internal struct SizeL
    {
        /// <summary>
        ///     A 32-bit unsigned integer that defines the x-coordinate of the point.
        /// </summary>
        public uint cx;

        /// <summary>
        ///     A 32-bit unsigned integer that defines the y-coordinate of the point.
        /// </summary>
        public uint cy;

        public void Read(BinaryReader reader)
        {
            this.cx = reader.ReadUInt32();
            this.cy = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.cx);
            writer.Write(this.cy);
        }
    }
}
