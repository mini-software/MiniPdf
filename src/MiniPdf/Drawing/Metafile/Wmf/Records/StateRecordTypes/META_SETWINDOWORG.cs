using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using System.IO;

    internal class META_SETWINDOWORG : Record
    {
        public short Y;

        public short X;

        public META_SETWINDOWORG()
        {
            Header.RecordFunction = RecordType.META_SETWINDOWORG;
        }

        public override void Read(BinaryReader reader)
        {
            Y = reader.ReadInt16();
            X = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Y);
            writer.Write(X);
        }
    }
}
