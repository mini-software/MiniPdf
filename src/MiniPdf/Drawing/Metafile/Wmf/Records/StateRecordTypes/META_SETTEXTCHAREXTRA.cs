using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SETTEXTCHAREXTRA : Record
    {
        public short CharacterExtra;

        public META_SETTEXTCHAREXTRA()
        {
            Header.RecordFunction = RecordType.META_SETTEXTCHAREXTRA;
        }

        public override void Read(BinaryReader reader)
        {
            CharacterExtra = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(CharacterExtra);
        }
    }
}
