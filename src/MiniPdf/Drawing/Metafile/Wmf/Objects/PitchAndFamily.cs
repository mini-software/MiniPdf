namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

    /// <summary>
    ///     The PitchAndFamily object specifies the pitch and family properties of a Font object (section 2.2.1.2). 
    ///     Pitch refers to the width of the characters, and family refers to the general appearance of a font.
    /// </summary>
    /// <remarks>
    ///     2.2.2.14 PitchAndFamily Object
    /// </remarks>
    internal struct PitchAndFamily
    {
        /// <summary>
        ///     A property of a font that describes its general appearance. This MUST be a value in the FamilyFont enumeration (section 2.1.1.8).
        /// </summary>
        public FamilyFont Family;

        /// <summary>
        ///     A property of a font that describes the pitch, of the characters. This MUST be a value in the PitchFont enumeration (section 2.1.1.24).
        /// </summary>
        public PitchFont Pitch;

        public void Read(BinaryReader reader)
        {
            var value = reader.ReadByte();
            this.Family = (FamilyFont)BitManipulation.GetBits(value, 0, 4);
            this.Pitch = (PitchFont)BitManipulation.GetBits(value, 6, 2);
        }

        public void Write(BinaryWriter writer)
        {
            byte value = (byte)((byte)this.Family | ((byte)this.Pitch << 6));
            writer.Write(value);
        }
    }
}
