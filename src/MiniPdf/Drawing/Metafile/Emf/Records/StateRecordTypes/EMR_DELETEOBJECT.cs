using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_DELETEOBJECT : Record
    {
        public uint ObjectIndex;

        public EMR_DELETEOBJECT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_DELETEOBJECT };
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

