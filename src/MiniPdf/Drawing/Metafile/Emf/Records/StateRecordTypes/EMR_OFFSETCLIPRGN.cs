using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_OFFSETCLIPRGN : Record
    {
        public int XOffset;

        public int YOffset;

        public EMR_OFFSETCLIPRGN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_OFFSETCLIPRGN };
        }

        public override void Read(BinaryReader reader)
        {
            XOffset = reader.ReadInt32();
            YOffset = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(XOffset);
            writer.Write(YOffset);
        }
    }
}

