using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_SETARCDIRECTION : Record
    {
        public ArcDirection Direction;

        public EMR_SETARCDIRECTION()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETARCDIRECTION };
        }

        public override void Read(BinaryReader reader)
        {
            Direction = (ArcDirection)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)Direction);
        }
    }
}

