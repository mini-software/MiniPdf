using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SAVEDC : Record
    {
        public META_SAVEDC()
        {
            Header.RecordFunction = RecordType.META_SAVEDC;
        }

        public override void Read(BinaryReader reader)
        {
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
        }
    }
}
