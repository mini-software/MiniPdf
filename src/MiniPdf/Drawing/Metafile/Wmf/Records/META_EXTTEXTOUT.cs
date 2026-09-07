using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    using System;
    using System.IO;
    using System.Text;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_EXTTEXTOUT record outputs text by using the font, background color, and text color that are defined in the playback device context. Optionally, dimensions can be provided for clipping, opaquing, or both.
    /// </summary>
    /// <remarks>
    ///     2.3.3.5 META_EXTTEXTOUT Record
    /// </remarks>
    internal class META_EXTTEXTOUT : Record
    {
        /// <summary>
        ///     Defines the y-coordinate, in logical units, where the text string is to be located.
        /// </summary>
        public short Y { get; set; }

        /// <summary>
        ///     Defines the x-coordinate, in logical units, where the text string is to be located.
        /// </summary>
        public short X { get; set; }

        /// <summary>
        ///     Defines the length of the string.
        /// </summary>
        public short StringLength { get; set; }

        /// <summary>
        ///     Defines the use of the application-defined rectangle.
        /// </summary>
        public ExtTextOutOptions fwOpts { get; set; }

        /// <summary>
        ///     An optional 8-byte Rect Object (section 2.2.2.18) that defines the dimensions, in logical coordinates, of a rectangle that is used for clipping, opaquing, or both.
        /// </summary>
        public Rect Rectangle { get; set; }

        /// <summary>
        ///     A variable-length string that specifies the text to be drawn. The string does not need to be null-terminated, because StringLength specifies the length of the string. If the length is odd, an extra byte is placed after it so that the following member (optional Dx) is aligned on a 16-bit boundary.
        /// </summary>
        public string String { get; set; } = string.Empty;

        /// <summary>
        ///     An optional array of 16-bit signed integers that indicate the distance between origins of adjacent character cells. For example, Dx[i] logical units separate the origins of character cell i and character cell i + 1. 
        ///     If this field is present, there MUST be the same number of values as there are characters in the string.
        /// </summary>
        public short[] Dx { get; set; } = Array.Empty<short>();

        public META_EXTTEXTOUT()
        {
            Header.RecordFunction = Enumerations.RecordType.META_EXTTEXTOUT;
        }

        public override void Read(BinaryReader reader)
        {
            Y = reader.ReadInt16();
            X = reader.ReadInt16();
            StringLength = reader.ReadInt16();
            fwOpts = (ExtTextOutOptions)reader.ReadUInt16();

            if ((fwOpts & (ExtTextOutOptions.ETO_CLIPPED | ExtTextOutOptions.ETO_OPAQUE)) != 0)
            {
                var rect = new Rect();
                rect.Read(reader);
                Rectangle = rect;
            }

            byte[] stringBytes = reader.ReadBytes(StringLength);
            String = Encoding.Default.GetString(stringBytes);

            if (StringLength > 0 && StringLength % 2 != 0)
            {
                reader.ReadByte(); // consume padding byte
            }

            if ((fwOpts & ExtTextOutOptions.ETO_PDY) != 0)
            {
                Dx = new short[StringLength];
                for (int i = 0; i < StringLength; i++)
                {
                    Dx[i] = reader.ReadInt16();
                }
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            writer.Write(Y);
            writer.Write(X);
            writer.Write(StringLength);
            writer.Write((ushort)fwOpts);

            if ((fwOpts & (ExtTextOutOptions.ETO_CLIPPED | ExtTextOutOptions.ETO_OPAQUE)) != 0)
            {
                Rectangle.Write(writer);
            }

            byte[] stringBytes = Encoding.Default.GetBytes(String);
            writer.Write(stringBytes);

            if (StringLength > 0 && StringLength % 2 != 0)
            {
                writer.Write((byte)0);
            }

            if ((fwOpts & ExtTextOutOptions.ETO_PDY) != 0)
            {
                foreach (short val in Dx)
                {
                    writer.Write(val);
                }
            }
        }
    }
}
