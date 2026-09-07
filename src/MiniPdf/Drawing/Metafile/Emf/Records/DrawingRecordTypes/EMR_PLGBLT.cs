using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class EMR_PLGBLT : Record
    {
        public RectL Bounds = new();

        public PointL[] AptlDest { get; set; } = new[] { new PointL(), new PointL(), new PointL() };

        public int XSrc;

        public int YSrc;

        public int CxSrc;

        public int CySrc;

        public XForm XFormSrc { get; set; } = new();

        public ColorRef BkColorSrc;

        public DIBColors UsageSrc;

        public uint OffBmiSrc;

        public uint CbBmiSrc;

        public uint OffBitsSrc;

        public uint CbBitsSrc;

        public int XMask;

        public int YMask;

        public DIBColors UsageMask;

        public uint OffBmiMask;

        public uint CbBmiMask;

        public uint OffBitsMask;

        public uint CbBitsMask;

        public byte[] BmiSrc { get; set; } = Array.Empty<byte>();

        public byte[] BitsSrc { get; set; } = Array.Empty<byte>();

        public byte[] BmiMask { get; set; } = Array.Empty<byte>();

        public byte[] BitsMask { get; set; } = Array.Empty<byte>();

        public EMR_PLGBLT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_PLGBLT };
        }

        public override void Read(BinaryReader reader)
        {
            long recordStart = reader.BaseStream.Position - 8;

            Bounds.Read(reader);
            AptlDest[0].Read(reader);
            AptlDest[1].Read(reader);
            AptlDest[2].Read(reader);
            XSrc = reader.ReadInt32();
            YSrc = reader.ReadInt32();
            CxSrc = reader.ReadInt32();
            CySrc = reader.ReadInt32();
            XFormSrc.Read(reader);
            BkColorSrc.Read(reader);
            UsageSrc = (DIBColors)reader.ReadUInt32();
            OffBmiSrc = reader.ReadUInt32();
            CbBmiSrc = reader.ReadUInt32();
            OffBitsSrc = reader.ReadUInt32();
            CbBitsSrc = reader.ReadUInt32();
            XMask = reader.ReadInt32();
            YMask = reader.ReadInt32();
            UsageMask = (DIBColors)reader.ReadUInt32();
            OffBmiMask = reader.ReadUInt32();
            CbBmiMask = reader.ReadUInt32();
            OffBitsMask = reader.ReadUInt32();
            CbBitsMask = reader.ReadUInt32();

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

            if (CbBmiMask > 0 && OffBmiMask > 0)
            {
                reader.BaseStream.Position = recordStart + OffBmiMask;
                BmiMask = reader.ReadBytes(Convert.ToInt32(CbBmiMask));
            }

            if (CbBitsMask > 0 && OffBitsMask > 0)
            {
                reader.BaseStream.Position = recordStart + OffBitsMask;
                BitsMask = reader.ReadBytes(Convert.ToInt32(CbBitsMask));
            }

            reader.BaseStream.Position = restorePosition;
        }

        public override void Write(BinaryWriter writer)
        {
            uint cbBmiSrc = CbBmiSrc == 0 ? (uint)BmiSrc.Length : CbBmiSrc;
            uint cbBitsSrc = CbBitsSrc == 0 ? (uint)BitsSrc.Length : CbBitsSrc;
            uint cbBmiMask = CbBmiMask == 0 ? (uint)BmiMask.Length : CbBmiMask;
            uint cbBitsMask = CbBitsMask == 0 ? (uint)BitsMask.Length : CbBitsMask;

            const uint fixedSizeAfterHeader = 140;
            uint cursor = fixedSizeAfterHeader;

            uint offBmiSrc = cbBmiSrc > 0 ? cursor : 0;
            cursor += cbBmiSrc;

            uint offBitsSrc = cbBitsSrc > 0 ? cursor : 0;
            cursor += cbBitsSrc;

            uint offBmiMask = cbBmiMask > 0 ? cursor : 0;
            cursor += cbBmiMask;

            uint offBitsMask = cbBitsMask > 0 ? cursor : 0;

            Bounds.Write(writer);
            AptlDest[0].Write(writer);
            AptlDest[1].Write(writer);
            AptlDest[2].Write(writer);
            writer.Write(XSrc);
            writer.Write(YSrc);
            writer.Write(CxSrc);
            writer.Write(CySrc);
            XFormSrc.Write(writer);
            BkColorSrc.Write(writer);
            writer.Write((uint)UsageSrc);
            writer.Write(offBmiSrc);
            writer.Write(cbBmiSrc);
            writer.Write(offBitsSrc);
            writer.Write(cbBitsSrc);
            writer.Write(XMask);
            writer.Write(YMask);
            writer.Write((uint)UsageMask);
            writer.Write(offBmiMask);
            writer.Write(cbBmiMask);
            writer.Write(offBitsMask);
            writer.Write(cbBitsMask);
            writer.Write(BmiSrc);
            writer.Write(BitsSrc);
            writer.Write(BmiMask);
            writer.Write(BitsMask);
        }
    }
}
