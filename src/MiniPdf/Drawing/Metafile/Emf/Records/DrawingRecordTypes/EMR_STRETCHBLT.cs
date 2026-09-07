using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Emf.Objects;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_STRETCHBLT : Record
    {
        public RectL Bounds = new();

        public int XDest;

        public int YDest;

        public int CxDest;

        public int CyDest;

        public Metafile.Wmf.Enumerations.TernaryRasterOperation BitBltRasterOperation;

        public int XSrc;

        public int YSrc;

        public XForm XFormSrc { get; set; } = new();

        public ColorRef BkColorSrc;

        public DIBColors UsageSrc;

        public uint OffBmiSrc;

        public uint CbBmiSrc;

        public uint OffBitsSrc;

        public uint CbBitsSrc;

        public int CxSrc;

        public int CySrc;

        public byte[] BmiSrc { get; set; } = Array.Empty<byte>();

        public byte[] BitsSrc { get; set; } = Array.Empty<byte>();

        public EMR_STRETCHBLT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_STRETCHBLT };
        }

        public override void Read(BinaryReader reader)
        {
            long recordStart = reader.BaseStream.Position - 8;

            Bounds.Read(reader);
            XDest = reader.ReadInt32();
            YDest = reader.ReadInt32();
            CxDest = reader.ReadInt32();
            CyDest = reader.ReadInt32();
            BitBltRasterOperation = (Metafile.Wmf.Enumerations.TernaryRasterOperation)reader.ReadUInt32();
            XSrc = reader.ReadInt32();
            YSrc = reader.ReadInt32();
            XFormSrc.Read(reader);
            BkColorSrc.Read(reader);
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

            const uint fixedSizeAfterHeader = 108;
            uint offBmi = cbBmi > 0 ? fixedSizeAfterHeader : 0;
            uint offBits = cbBits > 0 ? fixedSizeAfterHeader + cbBmi : 0;

            Bounds.Write(writer);
            writer.Write(XDest);
            writer.Write(YDest);
            writer.Write(CxDest);
            writer.Write(CyDest);
            writer.Write((uint)BitBltRasterOperation);
            writer.Write(XSrc);
            writer.Write(YSrc);
            XFormSrc.Write(writer);
            BkColorSrc.Write(writer);
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
