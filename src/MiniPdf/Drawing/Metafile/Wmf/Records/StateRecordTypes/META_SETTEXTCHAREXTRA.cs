using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Records;
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
