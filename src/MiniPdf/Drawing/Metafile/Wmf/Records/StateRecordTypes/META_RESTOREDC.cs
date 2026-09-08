using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal class META_RESTOREDC : Record
    {
        public short SavedDC;

        public META_RESTOREDC()
        {
            Header.RecordFunction = RecordType.META_RESTOREDC;
        }

        public override void Read(BinaryReader reader)
        {
            SavedDC = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(SavedDC);
        }
    }
}
