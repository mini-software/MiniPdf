using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal class META_SETBKMODE : Record
    {
        public MixMode BackgroundMode;

        public META_SETBKMODE()
        {
            Header.RecordFunction = RecordType.META_SETBKMODE;
        }

        public override void Read(BinaryReader reader)
        {
            BackgroundMode = (MixMode)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)BackgroundMode);
        }
    }
}
