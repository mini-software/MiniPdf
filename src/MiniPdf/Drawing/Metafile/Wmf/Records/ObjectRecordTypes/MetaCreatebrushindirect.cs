using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.ObjectRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_CREATEBRUSHINDIRECT record creates a Brush Object (section 2.2.1.1) from a LogBrush Object (section 2.2.2.10).
    /// </summary>
    /// <remarks>
    ///     2.3.4.1 META_CREATEBRUSHINDIRECT Record
    /// </remarks>
    internal class MetaCreatebrushindirect : Record
    {
        /// <summary>
        ///     LogBrush Object data that defines the brush to create. 
        ///     The BrushStyle field specified in the LogBrush Object SHOULD be BS_SOLID, BS_NULL, or BS_HATCHED; otherwise, a default Brush Object MAY be created. 
        ///     See the following table for details.
        /// </summary>
        public LogBrush LogBrush { get; set; } = new LogBrush();

        public MetaCreatebrushindirect()
        {
            Header.RecordFunction = RecordType.META_CREATEBRUSHINDIRECT;
        }

        public override void Read(BinaryReader reader)
        {
            LogBrush.BrushStyle = (BrushStyle)reader.ReadUInt16();
            var color = new ColorRef();
            color.Read(reader);
            LogBrush.ColorRef = color;
            LogBrush.BrushHatch = (HatchStyle)reader.ReadUInt16();
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)LogBrush.BrushStyle);
            LogBrush.ColorRef.Write(writer);
            writer.Write((ushort)(LogBrush.BrushHatch ?? 0));
        }
    }
}
