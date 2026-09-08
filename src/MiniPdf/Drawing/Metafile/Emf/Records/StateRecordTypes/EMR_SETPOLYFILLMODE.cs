using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETPOLYFILLMODE : Record
    {
        public Wmf.Enumerations.PolyFillMode PolyFillMode;

        public EMR_SETPOLYFILLMODE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETPOLYFILLMODE };
        }

        public override void Read(BinaryReader reader)
        {
            PolyFillMode = (Wmf.Enumerations.PolyFillMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((uint)PolyFillMode);
        }
    }
}

