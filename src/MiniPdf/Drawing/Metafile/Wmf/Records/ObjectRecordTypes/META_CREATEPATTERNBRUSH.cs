using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_CREATEPATTERNBRUSH record creates a brush object with a pattern specified by a bitmap.
    /// </summary>
    /// <remarks>
    ///     2.3.4.4 META_CREATEPATTERNBRUSH Record
    /// </remarks>
    internal class META_CREATEPATTERNBRUSH : Record
    {
        /// <summary>
        ///     A partial Bitmap16 object (section 2.2.2.1), which defines parameters for the bitmap that specifies the pattern for the brush. 
        ///     Fields not described below are specified in section 2.2.2.1.
        /// </summary>
        public Bitmap16 Bitmap16 = new Bitmap16();

        public int PatternLength
        {
            get
            {
                return (((this.Bitmap16.Width * this.Bitmap16.BitsPixel + 15) >> 4) << 1) * this.Bitmap16.Height;
            }
        }

        /// <summary>
        ///     A variable-length array of bytes that defines the bitmap pixel data that composes the brush pattern. 
        ///     The length of this field, in bytes, can be computed from bitmap parameters as follows.
        /// </summary>
        public byte[] Pattern = System.Array.Empty<byte>();

        public META_CREATEPATTERNBRUSH()
        {
            Header.RecordFunction = RecordType.META_CREATEPATTERNBRUSH;
        }

        public override void Read(BinaryReader reader)
        {
            this.Bitmap16.Read(reader);
            reader.ReadBytes(18);
            this.Pattern = reader.ReadBytes(this.PatternLength);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            this.Bitmap16.Write(writer);
            writer.Write(new byte[18]); // Write 18 reserved bytes
            writer.Write(this.Pattern);
        }
    }
}
