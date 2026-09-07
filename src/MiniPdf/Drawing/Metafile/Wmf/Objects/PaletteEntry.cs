using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;
    /// <summary>
    ///     The PaletteEntry Object defines the color and usage of an entry in a palette.
    /// </summary>
    /// <remarks>
    ///     2.2.2.13 PaletteEntry Object
    /// </remarks>
    internal class PaletteEntry
    {
        /// <summary>
        ///     An 8-bit unsigned integer that defines how the palette entry is to be used. 
        ///     The Values field MUST be 0x00 or one of the values in the PaletteEntryFlag Enumeration table.
        /// </summary>
        public PaletteEntryFlag Values;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the blue intensity value for the palette entry.
        /// </summary>
        public byte Blue;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the green intensity value for the palette entry.
        /// </summary>
        public byte Green;

        /// <summary>
        ///     An 8-bit unsigned integer that defines the red intensity value for the palette entry.
        /// </summary>
        public byte Red;

        public void Read(BinaryReader reader)
        {
            Values = (PaletteEntryFlag)reader.ReadByte();
            Blue = reader.ReadByte();
            Green = reader.ReadByte();
            Red = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)Values);
            writer.Write(Blue);
            writer.Write(Green);
            writer.Write(Red);
        }
    }
}
