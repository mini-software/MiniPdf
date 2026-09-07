using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_EXCLUDECLIPRECT : Record
    {
        public RectL Clip;

        public EMR_EXCLUDECLIPRECT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXCLUDECLIPRECT };
        }

        public override void Read(BinaryReader reader)
        {
            Clip = new RectL();
            Clip.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Clip.Write(writer);
        }
    }
}
