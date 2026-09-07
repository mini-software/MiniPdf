using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SETBKCOLOR : Record
    {
        public ColorRef Color;

        public EMR_SETBKCOLOR()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETBKCOLOR };
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

