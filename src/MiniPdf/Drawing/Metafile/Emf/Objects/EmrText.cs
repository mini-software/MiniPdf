namespace MiniPdf.Drawing.Metafile.Emf.Objects
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EmrText object contains values for text output.
    /// </summary>
    /// <remarks>
    ///     2.2.5 EmrText Object - MS-EMF Page 56
    /// </remarks>
    internal class EmrText
    {
        /// <summary>
        ///     A WMF PointL object ([MS-WMF] section 2.2.2.15) that specifies the coordinates of the reference point used to position the string. The reference point is defined by the last EMR_SETTEXTALIGN record (section 2.3.11.25). 
        ///     If no such record has been set, the default alignment is TA_LEFT,TA_TOP.
        /// </summary>
        public PointL Reference { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the number of characters in the string.
        /// </summary>
        public uint Chars { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the offset to the output string, in bytes, from the start of the record in which this object is contained. 
        ///     This value MUST be 8- or 16-bit aligned, according to the character format.
        /// </summary>
        public uint offString { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that specifies how to use the rectangle specified in the Rectangle field. 
        ///     This field can be a combination of more than one ExtTextOutOptions enumeration (section 2.1.11) values.
        /// </summary>
        public ExtTextOutOptions Options { get; set; }

        /// <summary>
        ///     An optional WMF RectL object ([MS-WMF] section 2.2.2.19) that defines a clipping and/or opaquing rectangle in logical units. 
        ///     This rectangle is applied to the text output performed by the containing record.
        /// </summary>
        public RectL Rectangle { get; set; }

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the offset to an intercharacter spacing array, in bytes, from the start of the record in which this object is contained. This value MUST be 32-bit aligned.
        /// </summary>
        public uint offDx { get; set; }

        /// <summary>
        ///     An optional number of unused bytes. The OutputString field is not required to follow immediately the preceding portion of this structure.
        /// </summary>
        public byte[] UndefinedSpace1 { get; set; } = Array.Empty<byte>();

        /// <summary>
        ///     An array of characters that specify the string to output. The location of this field is specified by the value of offString in bytes from the start of this record. The number of characters is specified by the value of Chars.
        /// </summary>
        public byte[] OutputString { get; set; } = Array.Empty<byte>();

        /// <summary>
        ///     An optional number of unused bytes. The OutputDx field is not required to follow immediately the preceding portion of this structure.
        /// </summary>
        public byte[] UndefinedSpace2 { get; set; } = Array.Empty<byte>();

        /// <summary>
        ///     An array of 32-bit unsigned integers that specify the output spacing between the origins of adjacent character cells in logical units.
        ///     The location of this field is specified by the value of offDx in bytes from the start of this record. 
        ///     If spacing is defined, this field contains the same number of values as characters in the output string.
        ///     If the Options field of the EmrText object contains the ETO_PDY flag, then this buffer contains twice as many values as there are characters in the output string, one horizontal and one vertical offset for each, in that order.
        ///     If ETO_RTLREADING is specified, characters are laid right to left instead of left to right. No other options affect the interpretation of this field.
        /// </summary>
        public byte[] OutputDx { get; set; } = Array.Empty<byte>();

        public void Read(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position;

            var reference = new PointL();
            reference.Read(reader);
            this.Reference = reference;
            Chars = reader.ReadUInt32();
            offString = reader.ReadUInt32();
            Options = (ExtTextOutOptions)reader.ReadUInt32();
            var rectangle = new RectL();
            rectangle.Read(reader);
            this.Rectangle = rectangle;
            offDx = reader.ReadUInt32();

            long headerSize = reader.BaseStream.Position - startPosition;

            int undefinedSpace1Length = (int)(offString - headerSize);
            if (undefinedSpace1Length < 0) throw new InvalidDataException("Invalid offString value.");
            if (undefinedSpace1Length > 0)
            {
                UndefinedSpace1 = reader.ReadBytes(undefinedSpace1Length);
            }

            OutputString = reader.ReadBytes((int)Chars);

            long currentOffset = headerSize + undefinedSpace1Length + Chars;
            int undefinedSpace2Length = (int)(offDx - currentOffset);
            if (undefinedSpace2Length < 0) throw new InvalidDataException("Invalid offDx value.");
            if (undefinedSpace2Length > 0)
            {
                UndefinedSpace2 = reader.ReadBytes(undefinedSpace2Length);
            }

            int outputDxLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            if (outputDxLength > 0)
            {
                OutputDx = reader.ReadBytes(outputDxLength);
            }
        }

        public void Write(BinaryWriter writer)
        {
            const uint headerSize = 40;
            uint chars = Chars == 0 ? (uint)OutputString.Length : Chars;
            uint offStringValue = offString == 0 ? headerSize + (uint)UndefinedSpace1.Length : offString;
            uint offDxValue = offDx == 0 ? offStringValue + chars + (uint)UndefinedSpace2.Length : offDx;

            int undefinedSpace1Length = (int)Math.Max(0, offStringValue - headerSize);
            int undefinedSpace2Length = (int)Math.Max(0, offDxValue - (offStringValue + chars));

            Reference.Write(writer);
            writer.Write(chars);
            writer.Write(offStringValue);
            writer.Write((uint)Options);
            Rectangle.Write(writer);
            writer.Write(offDxValue);

            if (undefinedSpace1Length > 0)
            {
                if (UndefinedSpace1.Length >= undefinedSpace1Length)
                {
                    writer.Write(UndefinedSpace1, 0, undefinedSpace1Length);
                }
                else
                {
                    writer.Write(UndefinedSpace1);
                    writer.Write(new byte[undefinedSpace1Length - UndefinedSpace1.Length]);
                }
            }

            writer.Write(OutputString, 0, (int)Math.Min(chars, (uint)OutputString.Length));
            if (chars > OutputString.Length)
            {
                writer.Write(new byte[chars - OutputString.Length]);
            }

            if (undefinedSpace2Length > 0)
            {
                if (UndefinedSpace2.Length >= undefinedSpace2Length)
                {
                    writer.Write(UndefinedSpace2, 0, undefinedSpace2Length);
                }
                else
                {
                    writer.Write(UndefinedSpace2);
                    writer.Write(new byte[undefinedSpace2Length - UndefinedSpace2.Length]);
                }
            }

            writer.Write(OutputDx);
        }
    }
}
