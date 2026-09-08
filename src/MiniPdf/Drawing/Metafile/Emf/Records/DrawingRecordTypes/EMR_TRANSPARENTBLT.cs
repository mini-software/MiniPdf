using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_TRANSPARENTBLT : Record
    {
        public RectL Bounds = new();

        public int XDest;

        public int YDest;

        public int CxDest;

        public int CyDest;

        public int XSrc;

        public int YSrc;

        public XForm XFormSrc { get; set; } = new();

        public uint ColorTransparent;

        public DIBColors UsageSrc;

        public uint OffBmiSrc;

        public uint CbBmiSrc;

        public uint OffBitsSrc;

        public uint CbBitsSrc;

        public int CxSrc;

        public int CySrc;

        public byte[] BmiSrc { get; set; } = Array.Empty<byte>();

        public byte[] BitsSrc { get; set; } = Array.Empty<byte>();

        public EMR_TRANSPARENTBLT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_TRANSPARENTBLT };
        }

        public override void Read(BinaryReader reader)
        {
            long recordStart = reader.BaseStream.Position - 8;

            Bounds.Read(reader);
            XDest = reader.ReadInt32();
            YDest = reader.ReadInt32();
            CxDest = reader.ReadInt32();
            CyDest = reader.ReadInt32();
            XSrc = reader.ReadInt32();
            YSrc = reader.ReadInt32();
            XFormSrc.Read(reader);
            ColorTransparent = reader.ReadUInt32();
            UsageSrc = (DIBColors)reader.ReadUInt32();
            OffBmiSrc = reader.ReadUInt32();
            CbBmiSrc = reader.ReadUInt32();
            OffBitsSrc = reader.ReadUInt32();
            CbBitsSrc = reader.ReadUInt32();
            CxSrc = reader.ReadInt32();
            CySrc = reader.ReadInt32();

            long restorePosition = reader.BaseStream.Position;

            if (CbBmiSrc > 0 && OffBmiSrc > 0)
            {
                reader.BaseStream.Position = recordStart + OffBmiSrc;
                BmiSrc = reader.ReadBytes(Convert.ToInt32(CbBmiSrc));
            }

            if (CbBitsSrc > 0 && OffBitsSrc > 0)
            {
                reader.BaseStream.Position = recordStart + OffBitsSrc;
                BitsSrc = reader.ReadBytes(Convert.ToInt32(CbBitsSrc));
            }

            reader.BaseStream.Position = restorePosition;
        }

        public override void Write(BinaryWriter writer)
        {
            uint cbBmi = CbBmiSrc == 0 ? (uint)BmiSrc.Length : CbBmiSrc;
            uint cbBits = CbBitsSrc == 0 ? (uint)BitsSrc.Length : CbBitsSrc;

            const uint fixedSizeAfterHeader = 104;
            uint offBmi = cbBmi > 0 ? fixedSizeAfterHeader : 0;
            uint offBits = cbBits > 0 ? fixedSizeAfterHeader + cbBmi : 0;

            Bounds.Write(writer);
            writer.Write(XDest);
            writer.Write(YDest);
            writer.Write(CxDest);
            writer.Write(CyDest);
            writer.Write(XSrc);
            writer.Write(YSrc);
            XFormSrc.Write(writer);
            writer.Write(ColorTransparent);
            writer.Write((uint)UsageSrc);
            writer.Write(offBmi);
            writer.Write(cbBmi);
            writer.Write(offBits);
            writer.Write(cbBits);
            writer.Write(CxSrc);
            writer.Write(CySrc);
            writer.Write(BmiSrc);
            writer.Write(BitsSrc);
        }
    }
}
