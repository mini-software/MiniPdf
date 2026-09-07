using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETSTRETCHBLTMODE : Record
    {
        public Wmf.Enumerations.StretchMode StretchMode;

        public EMR_SETSTRETCHBLTMODE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETSTRETCHBLTMODE };
        }

        public override void Read(BinaryReader reader)
        {
            StretchMode = (Wmf.Enumerations.StretchMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)StretchMode);
        }
    }
}

