using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System.IO;

    internal class EMR_SETCOLORSPACE : Record
    {
        public uint IhCS;

        public EMR_SETCOLORSPACE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETCOLORSPACE };
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
