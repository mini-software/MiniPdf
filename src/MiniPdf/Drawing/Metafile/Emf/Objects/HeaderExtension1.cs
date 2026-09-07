namespace MiniPdf.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    /// <summary>
    ///     The HeaderExtension1 object defines the first extension to the EMF metafile header. It adds support for a PixelFormatDescriptor object (section 2.2.22) and OpenGL [OPENGL] records (section 2.3.9).
    /// </summary>
    /// <remarks>
    ///     2.2.10 HeaderExtension1 Object
    /// </remarks>
    internal class HeaderExtension1
    {
        /// <summary>
        ///     A 32-bit unsigned integer that specifies the size of the PixelFormatDescriptor object. This MUST be 0x00000000 if no pixel format is set.
        /// </summary>
        public uint cbPixelFormat;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the offset to the PixelFormatDescriptor object. This MUST be 0x00000000 if no pixel format is set.
        /// </summary>
        public uint offPixelFormat;

        /// <summary>
        ///     A 32-bit unsigned integer that indicates whether OpenGL commands are present in the metafile.
        /// </summary>
        public uint bOpenGL;

        public void Read(BinaryReader reader)
        {
            this.cbPixelFormat = reader.ReadUInt32();
            this.offPixelFormat = reader.ReadUInt32();
            this.bOpenGL = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.cbPixelFormat);
            writer.Write(this.offPixelFormat);
            writer.Write(this.bOpenGL);
        }
    }
}
