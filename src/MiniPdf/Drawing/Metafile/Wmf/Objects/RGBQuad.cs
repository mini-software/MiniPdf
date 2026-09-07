namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The RGBQuad Object defines the pixel color values in an uncompressed DIB.
    /// </summary>
    /// <remarks>
    ///     2.2.2.20 RGBQuad Object
    /// </remarks>
    internal struct RGBQuad
    {
        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of blue.
        /// </summary>
        public byte Blue;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of green.
        /// </summary>
        public byte Green;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of red.
        /// </summary>
        public byte Red;

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Blue);
            writer.Write(this.Green);
            writer.Write(this.Red);
            writer.Write((byte)0); // Reserved
        }

        public void Read(BinaryReader reader)
        {
            this.Blue = reader.ReadByte();
            this.Green = reader.ReadByte();
            this.Red = reader.ReadByte();
            reader.ReadByte();
        }

    }
}
