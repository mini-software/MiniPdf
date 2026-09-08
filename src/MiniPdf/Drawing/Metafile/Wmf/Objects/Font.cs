using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    using System;
    using System.IO;
    using System.Text;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Flags;

    /// <summary>
    ///     The Font object specifies the attributes of a logical font.
    /// </summary>
    /// <remarks>
    ///     2.2.1.2 Font Object
    /// </remarks>
    internal class Font
    {
        /// <summary>
        ///     A 16-bit signed integer that specifies the height, in logical units, of the font's character cell. The character height is computed as the character cell height minus the internal leading. The font mapper SHOULD interpret the height as follows.
        /// </summary>
        public short Height { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the average width, in logical units, of characters in the font. 
        ///     If Width is 0x0000, the aspect ratio of the device SHOULD be matched against the digitization aspect ratio of the available fonts to find the closest match, determined by the absolute value of the difference.
        /// </summary>
        public short Width { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the angle, in tenths of degrees, between the escapement vector and the x-axis of the device. 
        ///     The escapement vector is parallel to the base line of a row of text.
        /// </summary>
        public short Escapement { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the angle, in tenths of degrees, between each character's base line and the x-axis of the device.
        /// </summary>
        public short Orientation { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the weight of the font in the range 0 through 1000. 
        ///     For example, 400 is normal and 700 is bold. If this value is 0x0000, a default weight SHOULD be used.
        /// </summary>
        public short Weight { get; set; }

        /// <summary>
        ///     A 8-bit Boolean value that specifies the italic attribute of the font.
        /// </summary>
        public byte Italic { get; set; }

        /// <summary>
        ///     An 8-bit Boolean value that specifies the underline attribute of the font.
        /// </summary>
        public byte Underline { get; set; }

        /// <summary>
        ///     An 8-bit Boolean value that specifies the strikeout attribute of the font.
        /// </summary>
        public byte StrikeOut { get; set; }

        /// <summary>
        ///     An 8-bit unsigned integer that defines the character set. It SHOULD be set to a value in the CharacterSet Enumeration (section 2.1.1.5).
        /// </summary>
        public CharacterSet CharSet { get; set; }

        /// <summary>
        ///     An 8-bit unsigned integer that defines the output precision. The output precision defines how closely the output must match the requested font's height, width, character orientation, escapement, pitch, and font type. It MUST be one of the values from the OutPrecision Enumeration (section 2.1.1.21).
        /// </summary>
        public OutPrecision OutPrecision { get; set; }

        /// <summary>
        ///     An 8-bit unsigned integer that defines the clipping precision. The clipping precision defines how to clip characters that are partially outside the clipping region. 
        ///     It MUST be a combination of one or more of the bit settings in the ClipPrecision Flags.
        /// </summary>
        public ClipPrecision ClipPrecision { get; set; }

        /// <summary>
        ///     A PitchAndFamily object (section 2.2.2.14) that defines the pitch and the family of the font. Font families specify the look of fonts in a general way and are intended for specifying fonts when the exact typeface wanted is not available.
        /// </summary>
        public byte PitchAndFamily { get; set; }

        public string Facename { get; set; } = string.Empty;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(Escapement);
            writer.Write(Orientation);
            writer.Write(Weight);
            writer.Write(Italic);
            writer.Write(Underline);
            writer.Write(StrikeOut);
            writer.Write((byte)CharSet);
            writer.Write((byte)OutPrecision);
            writer.Write((byte)ClipPrecision);
            writer.Write(PitchAndFamily);
            // Facename is a null-terminated ANSI string, max 32 bytes
            var facenameBytes = new byte[32];
            if (!string.IsNullOrEmpty(Facename))
            {
                var nameBytes = Encoding.ASCII.GetBytes(Facename);
                int len = nameBytes.Length > 31 ? 31 : nameBytes.Length;
                Array.Copy(nameBytes, facenameBytes, len);
            }
            writer.Write(facenameBytes);
        }

        public void Read(BinaryReader reader)
        {
            Height = reader.ReadInt16();
            Width = reader.ReadInt16();
            Escapement = reader.ReadInt16();
            Orientation = reader.ReadInt16();
            Weight = reader.ReadInt16();
            Italic = reader.ReadByte();
            Underline = reader.ReadByte();
            StrikeOut = reader.ReadByte();
            CharSet = (CharacterSet)reader.ReadByte();
            OutPrecision = (OutPrecision)reader.ReadByte();
            ClipPrecision = (ClipPrecision)reader.ReadByte();
            PitchAndFamily = reader.ReadByte();
            var facenameBytes = reader.ReadBytes(32);
            int facenameLen = Array.IndexOf(facenameBytes, (byte)0);
            if (facenameLen < 0) facenameLen = 32;
            Facename = Encoding.ASCII.GetString(facenameBytes, 0, facenameLen);
        }
    }
}
