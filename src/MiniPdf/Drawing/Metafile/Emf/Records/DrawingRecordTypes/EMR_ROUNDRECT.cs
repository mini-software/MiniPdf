using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_ROUNDRECT : Record
    {
        public RectL Box;

        public SizeL Corner;

        public EMR_ROUNDRECT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_ROUNDRECT };
        }

        public override void Read(BinaryReader reader)
        {
            Box = new RectL();
            Box.Read(reader);
            Corner = new SizeL();
            Corner.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Box.Write(writer);
            Corner.Write(writer);
        }
    }
}
