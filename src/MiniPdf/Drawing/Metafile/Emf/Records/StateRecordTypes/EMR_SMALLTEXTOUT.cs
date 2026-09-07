using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SMALLTEXTOUT : Record
    {
        public PointL Reference;

        public uint Chars;

        public ExtTextOutOptions FuOptions;

        public RectL? Bounds;

        public byte[] TextBytes { get; set; } = [];

        public EMR_SMALLTEXTOUT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SMALLTEXTOUT };
        }

        public override void Read(BinaryReader reader)
        {
            Reference = new PointL();
            Reference.Read(reader);
            Chars = reader.ReadUInt32();
            FuOptions = (ExtTextOutOptions)reader.ReadUInt32();

            if ((FuOptions & (ExtTextOutOptions.ETO_OPAQUE | ExtTextOutOptions.ETO_CLIPPED)) != 0)
            {
                var bounds = new RectL();
                bounds.Read(reader);
                Bounds = bounds;
            }

            TextBytes = reader.ReadBytes((int)Chars);
        }

        public override void Write(BinaryWriter writer)
        {
            uint chars = Chars == 0 ? (uint)TextBytes.Length : Chars;
            Reference.Write(writer);
            writer.Write(chars);
            writer.Write((uint)FuOptions);
            if (Bounds.HasValue)
            {
                Bounds.Value.Write(writer);
            }
            writer.Write(TextBytes);
        }
    }
}
