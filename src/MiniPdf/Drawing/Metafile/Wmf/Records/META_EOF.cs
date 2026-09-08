using System.IO;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_EOF : Record
    {
        public META_EOF()
        {
            Header.RecordFunction = RecordType.META_EOF;
        }

        public override void Read(BinaryReader reader)
        {
            // no parameters
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
        }
    }
}
