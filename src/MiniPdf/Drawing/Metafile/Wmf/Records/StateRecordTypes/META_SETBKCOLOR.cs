using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class META_SETBKCOLOR : Record
    {
        public ColorRef Color;

        public META_SETBKCOLOR()
        {
            Header.RecordFunction = RecordType.META_SETBKCOLOR;
        }

        public override void Read(BinaryReader reader)
        {
            Color.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            Color.Write(writer);
        }
    }
}
