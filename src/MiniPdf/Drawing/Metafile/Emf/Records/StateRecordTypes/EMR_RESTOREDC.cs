using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_RESTOREDC : Record
    {
        public int SavedDC;

        public EMR_RESTOREDC()
        {
            Header = new RecordHeader { Type = RecordType.EMR_RESTOREDC };
        }

        public override void Read(BinaryReader reader)
        {
            SavedDC = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(SavedDC);
        }
    }
}

