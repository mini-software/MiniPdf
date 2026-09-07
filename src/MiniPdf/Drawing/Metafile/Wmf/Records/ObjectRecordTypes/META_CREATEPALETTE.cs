using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_CREATEPALETTE record creates a Palette Object (section 2.2.1.3).
    /// </summary>
    /// <remarks>
    ///     2.3.4.3 META_CREATEPALETTE Record
    /// </remarks>
    internal class META_CREATEPALETTE : Record
    {
        /// <summary>
        ///     Palette Object data that defines the palette to create. The Start field in the Palette Object MUST be set to 0x0300.
        /// </summary>
        public Palette Palette = new Palette();

        public META_CREATEPALETTE()
        {
            Header.RecordFunction = RecordType.META_CREATEPALETTE;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Palette.Read(reader);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            base.Write(writer);
            Palette.Write(writer);
        }
    }
}
