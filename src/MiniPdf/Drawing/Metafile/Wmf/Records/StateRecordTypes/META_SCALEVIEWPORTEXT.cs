using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Records;
    using System.IO;

    internal class META_SCALEVIEWPORTEXT : Record
    {
        public short YNumerator;

        public short YDenominator;

        public short XNumerator;

        public short XDenominator;

        public META_SCALEVIEWPORTEXT()
        {
            Header.RecordFunction = RecordType.META_SCALEVIEWPORTEXT;
        }

        public override void Read(BinaryReader reader)
        {
            YNumerator = reader.ReadInt16();
            YDenominator = reader.ReadInt16();
            XNumerator = reader.ReadInt16();
            XDenominator = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(YNumerator);
            writer.Write(YDenominator);
            writer.Write(XNumerator);
            writer.Write(XDenominator);
        }
    }
}
