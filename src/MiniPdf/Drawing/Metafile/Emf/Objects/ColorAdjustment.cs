namespace MiniSoftware.Drawing.Metafile.Emf.Objects
{
    using System.IO;

    /// <summary>
    ///     The ColorAdjustment object defines values for adjusting the colors in source bitmaps in bit-block transfers.
    /// </summary>
    /// <remarks>
    ///     2.2.2 ColorAdjustment Object
    /// </remarks>
    internal class ColorAdjustment
    {
        /// <summary>
        ///     A 16-bit unsigned integer that specifies the size in bytes of this object. This MUST be 0x0018.
        /// </summary>
        public ushort Size { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies how to prepare the output image. This field can be set to NULL or to any combination of values in the ColorAdjustment enumeration (section 2.1.5).
        /// </summary>
        public ushort Values { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the type of standard light source under which the image is viewed, from the Illuminant enumeration (section 2.1.19).
        /// </summary>
        public ushort IlluminantIndex { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the nth power gamma correction value for the red primary of the source colors. This value SHOULD be in the range from 2,500 to 65,000. A value of 10,000 means gamma correction MUST NOT be performed.
        /// </summary>
        public ushort RedGamma { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the nth power gamma correction value for the green primary of the source colors. This value SHOULD be in the range from 2,500 to 65,000. A value of 10,000 means gamma correction MUST NOT be performed.
        /// </summary>
        public ushort GreenGamma { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the nth power gamma correction value for the blue primary of the source colors. This value SHOULD be in the range from 2,500 to 65,000. A value of 10,000 means gamma correction MUST NOT be performed.
        /// </summary>
        public ushort BlueGamma { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the black reference for the source colors. Any colors that are darker than this are treated as black. This value SHOULD be in the range from zero to 4,000.
        /// </summary>
        public ushort ReferenceBlack { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the white reference for the source colors. Any colors that are lighter than this are treated as white. This value SHOULD be in the range from 6,000 to 10,000.
        /// </summary>
        public ushort ReferenceWhite { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that specifies the amount of contrast to be applied to the source object. This value SHOULD be in the range from –100 to 100. A value of zero means contrast adjustment MUST NOT be performed.
        /// </summary>
        public short Contrast { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that specifies the amount of brightness to be applied to the source object. This value SHOULD be in the range from –100 to 100. A value of zero means brightness adjustment MUST NOT be performed.
        /// </summary>
        public short Brightness { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that specifies the amount of colorfulness to be applied to the source object. This value SHOULD be in the range from –100 to 100. A value of zero means colorfulness adjustment MUST NOT be performed.
        /// </summary>
        public short Colorfulness { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that specifies the amount of red or green tint adjustment to be applied to the source object. This value SHOULD be in the range from –100 to 100. Positive numbers adjust towards red and negative numbers adjust towards green. A value of zero means tint adjustment MUST NOT be performed.
        /// </summary>
        public short RedGreenTint { get; set; }

        public ColorAdjustment()
        {
            Size = 0x0018;
        }

        public void Read(BinaryReader reader)
        {
            Size = reader.ReadUInt16();
            Values = reader.ReadUInt16();
            IlluminantIndex = reader.ReadUInt16();
            RedGamma = reader.ReadUInt16();
            GreenGamma = reader.ReadUInt16();
            BlueGamma = reader.ReadUInt16();
            ReferenceBlack = reader.ReadUInt16();
            ReferenceWhite = reader.ReadUInt16();
            Contrast = reader.ReadInt16();
            Brightness = reader.ReadInt16();
            Colorfulness = reader.ReadInt16();
            RedGreenTint = reader.ReadInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Size);
            writer.Write(Values);
            writer.Write(IlluminantIndex);
            writer.Write(RedGamma);
            writer.Write(GreenGamma);
            writer.Write(BlueGamma);
            writer.Write(ReferenceBlack);
            writer.Write(ReferenceWhite);
            writer.Write(Contrast);
            writer.Write(Brightness);
            writer.Write(Colorfulness);
            writer.Write(RedGreenTint);
        }
    }
}
