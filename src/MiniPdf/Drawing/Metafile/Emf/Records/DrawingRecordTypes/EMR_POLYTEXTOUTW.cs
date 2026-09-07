using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_POLYTEXTOUTW : Record
    {
        public RectL Bounds;

        public GraphicsMode IGraphicsMode;

        public float ExScale;

        public float EyScale;

        public uint CStrings;

        public EmrText[] EmrTexts { get; set; } = [];

        public EMR_POLYTEXTOUTW()
        {
            Header = new RecordHeader { Type = RecordType.EMR_POLYTEXTOUTW };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            IGraphicsMode = (GraphicsMode)reader.ReadUInt32();
            ExScale = reader.ReadSingle();
            EyScale = reader.ReadSingle();
            CStrings = reader.ReadUInt32();

            EmrTexts = new EmrText[CStrings];
            for (int i = 0; i < CStrings; i++)
            {
                var emrText = new EmrText();
                emrText.Read(reader);
                EmrTexts[i] = emrText;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            uint stringsCount = CStrings == 0 ? (uint)EmrTexts.Length : CStrings;

            Bounds.Write(writer);
            writer.Write((uint)IGraphicsMode);
            writer.Write(ExScale);
            writer.Write(EyScale);
            writer.Write(stringsCount);
            for (int i = 0; i < stringsCount; i++)
            {
                EmrTexts[i].Write(writer);
            }
        }
    }
}
