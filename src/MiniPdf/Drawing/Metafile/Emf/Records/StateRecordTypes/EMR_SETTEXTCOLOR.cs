using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SETTEXTCOLOR : Record
    {
        public ColorRef Color;

        public EMR_SETTEXTCOLOR()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETTEXTCOLOR };
        }

        public override void Read(BinaryReader reader)
        {
            Color.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Color.Write(writer);
        }
    }
}

