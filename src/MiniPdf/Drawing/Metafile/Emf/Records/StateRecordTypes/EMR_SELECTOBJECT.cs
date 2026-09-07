using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SELECTOBJECT : Record
    {
        public uint ObjectIndex;

        public EMR_SELECTOBJECT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SELECTOBJECT };
        }

        public override void Read(BinaryReader reader)
        {
            ObjectIndex = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(ObjectIndex);
        }
    }
}

