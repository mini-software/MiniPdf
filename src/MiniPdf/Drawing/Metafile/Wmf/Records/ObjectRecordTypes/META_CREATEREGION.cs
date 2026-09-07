using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_CREATEREGION record creates a Region Object (section 2.2.1.5).
    /// </summary>
    /// <remarks>
    ///     2.3.4.6 META_CREATEREGION Record
    /// </remarks>
    internal class META_CREATEREGION : Record
    {
        /// <summary>
        ///     Region Object data that defines the region to create.
        /// </summary>
        public Region Region = new Region();

        public META_CREATEREGION()
        {
            Header.RecordFunction = RecordType.META_CREATEREGION;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Region.Read(reader);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            base.Write(writer);
            Region.Write(writer);
        }
    }
}
