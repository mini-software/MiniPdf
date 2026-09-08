using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.ObjectRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_CREATEPEN : Record
    {
        internal uint IhPen;

        internal Pen Pen { get; set; } = new();

        public EMR_CREATEPEN()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CREATEPEN };
        }

        public override void Read(BinaryReader reader)
        {
            IhPen = reader.ReadUInt32();
            Pen.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhPen);
            Pen.Write(writer);
        }
    }
}
