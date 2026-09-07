using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
using MiniSoftware.Drawing.Metafile.Wmf.Objects;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{

    /// <summary>
    ///     The META_CREATEFONTINDIRECT record creates a Font Object (section 2.2.1.2).
    /// </summary>
    /// <remarks>
    ///     2.3.4.2 META_CREATEFONTINDIRECT Record
    /// </remarks>
    internal class META_CREATEFONTINDIRECT : Record
    {
        /// <summary>
        ///     Font Object data that defines the font to create.
        /// </summary>
        public Objects.Font Font = new Objects.Font();

        public META_CREATEFONTINDIRECT()
        {
            Header.RecordFunction = RecordType.META_CREATEFONTINDIRECT;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Font.Read(reader);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            base.Write(writer);
            Font.Write(writer);
        }
    }
}
