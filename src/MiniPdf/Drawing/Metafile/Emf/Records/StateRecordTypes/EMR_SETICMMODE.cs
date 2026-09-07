using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_SETICMMODE : Record
    {
        public IcmMode IcmMode;

        public EMR_SETICMMODE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETICMMODE };
        }

        public override void Read(BinaryReader reader)
        {
            IcmMode = (IcmMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)IcmMode);
        }
    }
}
