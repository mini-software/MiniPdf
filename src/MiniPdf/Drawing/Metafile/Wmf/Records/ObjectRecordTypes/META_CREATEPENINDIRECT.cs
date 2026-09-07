using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;

    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_CREATEPENINDIRECT record creates a Pen Object (section 2.2.1.4).
    /// </summary>
    /// <remarks>
    ///     2.3.4.5 META_CREATEPENINDIRECT Record
    /// </remarks>
    internal class META_CREATEPENINDIRECT : Record
    {
        /// <summary>
        ///     Pen Object data that defines the pen to create.
        /// </summary>
        public Pen Pen = new Pen();

        public META_CREATEPENINDIRECT()
        {
            Header.RecordFunction = RecordType.META_CREATEPENINDIRECT;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Pen.Read(reader);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            base.Write(writer);
            Pen.Write(writer);
        }
    }
}
