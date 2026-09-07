using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.ObjectRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_EXTCREATEFONTINDIRECTW : Record
    {
        public uint IhFont;

        public Font Font { get; set; } = new();

        public EMR_EXTCREATEFONTINDIRECTW()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXTCREATEFONTINDIRECTW };
        }

        public override void Read(BinaryReader reader)
        {
            IhFont = reader.ReadUInt32();
            Font.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhFont);
            Font.Write(writer);
        }
    }
}
