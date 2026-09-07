using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.ObjectRecordTypes
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_CREATEBRUSHINDIRECT : Record
    {
        public uint IhBrush;

        public LogBrush LogBrush { get; set; } = new();

        public EMR_CREATEBRUSHINDIRECT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CREATEBRUSHINDIRECT };
        }

        public override void Read(BinaryReader reader)
        {
            IhBrush = reader.ReadUInt32();
            LogBrush.BrushStyle = (Wmf.Enumerations.BrushStyle)reader.ReadUInt32();
            LogBrush.ColorRef.Read(reader);
            LogBrush.BrushHatch = (Wmf.Enumerations.HatchStyle)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhBrush);
            writer.Write((uint)LogBrush.BrushStyle);
            LogBrush.ColorRef.Write(writer);
            writer.Write((uint)(LogBrush.BrushHatch ?? Metafile.Wmf.Enumerations.HatchStyle.HS_HORIZONTAL));
        }
    }
}
