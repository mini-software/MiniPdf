using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_SETBKMODE : Record
    {
        public BackgroundMode BackgroundMode;

        public EMR_SETBKMODE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETBKMODE };
        }

        public override void Read(BinaryReader reader)
        {
            BackgroundMode = (BackgroundMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)BackgroundMode);
        }
    }
}

