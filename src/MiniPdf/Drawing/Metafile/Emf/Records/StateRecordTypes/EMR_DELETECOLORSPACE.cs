using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_DELETECOLORSPACE : Record
    {
        public uint IhCS;

        public EMR_DELETECOLORSPACE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_DELETECOLORSPACE };
        }

        public override void Read(BinaryReader reader)
        {
            IhCS = reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhCS);
        }
    }
}
