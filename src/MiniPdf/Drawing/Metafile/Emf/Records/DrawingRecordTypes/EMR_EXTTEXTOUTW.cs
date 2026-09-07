using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_EXTTEXTOUTW : Record
    {
        public RectL Bounds;

        public GraphicsMode IGraphicsMode;

        public float ExScale;

        public float EyScale;

        public EmrText EmrText { get; set; } = new();

        public EMR_EXTTEXTOUTW()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXTTEXTOUTW };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            IGraphicsMode = (GraphicsMode)reader.ReadUInt32();
            ExScale = reader.ReadSingle();
            EyScale = reader.ReadSingle();
            EmrText.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Bounds.Write(writer);
            writer.Write((uint)IGraphicsMode);
            writer.Write(ExScale);
            writer.Write(EyScale);
            EmrText.Write(writer);
        }
    }
}
