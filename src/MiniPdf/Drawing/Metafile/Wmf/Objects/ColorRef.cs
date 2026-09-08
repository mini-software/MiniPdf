namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;

    /// <summary>
    ///     The ColorRef Object defines the RGB color.
    /// </summary>
    /// <remarks>
    ///     2.2.2.8 ColorRef Object
    /// </remarks>
    internal struct ColorRef
    {
        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of red.
        /// </summary>
        public byte Red;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of green.
        /// </summary>
        public byte Green;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the relative intensity of blue.
        /// </summary>
        public byte Blue;

        /// <summary>
        ///     This byte MUST be 0x00.
        /// </summary>
        public byte Reserved;

        public void Read(BinaryReader reader)
        {
            this.Red = reader.ReadByte();
            this.Green = reader.ReadByte();
            this.Blue = reader.ReadByte();
            this.Reserved = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Red);
            writer.Write(this.Green);
            writer.Write(this.Blue);
            writer.Write(this.Reserved);
        }
    }
}
