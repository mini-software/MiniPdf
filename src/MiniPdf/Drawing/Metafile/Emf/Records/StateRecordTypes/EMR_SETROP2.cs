namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETROP2 : Record
    {
        public Metafile.Wmf.Enumerations.BinaryRasterOperation DrawMode;

        public EMR_SETROP2()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETROP2 };
        }

        public override void Read(BinaryReader reader)
        {
            DrawMode = (Metafile.Wmf.Enumerations.BinaryRasterOperation)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)DrawMode);
        }
    }
}

