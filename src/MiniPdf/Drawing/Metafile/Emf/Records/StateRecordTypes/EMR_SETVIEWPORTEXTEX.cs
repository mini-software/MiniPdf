using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETVIEWPORTEXTEX : Record
    {
        public int XExtent;

        public int YExtent;

        public EMR_SETVIEWPORTEXTEX()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETVIEWPORTEXTEX };
        }

        public override void Read(BinaryReader reader)
        {
            XExtent = reader.ReadInt32();
            YExtent = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(XExtent);
            writer.Write(YExtent);
        }
    }
}

