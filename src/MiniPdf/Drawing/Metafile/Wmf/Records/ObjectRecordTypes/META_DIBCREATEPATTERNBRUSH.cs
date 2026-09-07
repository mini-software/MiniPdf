using MiniPdf.Drawing.Metafile.Wmf;
using MiniPdf.Drawing.Metafile.Wmf.Enumerations;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System;
    using System.IO;

    /// <summary>
    ///     The META_DIBCREATEPATTERNBRUSH record creates a Brush Object (section 2.2.1.1) with a pattern specified by a DeviceIndependentBitmap (DIB) Object (section 2.2.2.9).
    /// </summary>
    /// <remarks>
    ///     2.3.4.8 META_DIBCREATEPATTERNBRUSH Record
    /// </remarks>
    internal class META_DIBCREATEPATTERNBRUSH : Record
    {
        /// <summary>
        ///     Defines the brush style. The legal values for this field are defined as follows: if the value is not BS_PATTERN, BS_DIBPATTERNPT MUST be assumed.
        /// </summary>
        public BrushStyle Style;

        /// <summary>
        ///     Defines whether the Colors field of a DIB Object contains explicit RGB values, or indexes into a palette.
        /// </summary>
        public ColorUsage ColorUsage;

        public byte[] Target = Array.Empty<byte>();

        public META_DIBCREATEPATTERNBRUSH()
        {
            Header.RecordFunction = RecordType.META_DIBCREATEPATTERNBRUSH;
        }

        public override void Read(BinaryReader reader)
        {
            this.Style = (BrushStyle)reader.ReadUInt16();
            this.ColorUsage = (ColorUsage)reader.ReadUInt16();

            this.Target = reader.ReadBytes(Convert.ToInt32(this.Header.RecordSize * 2) - 10);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)this.Style);
            writer.Write((ushort)this.ColorUsage);
            writer.Write(this.Target);
        }
    }
}
