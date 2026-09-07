using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_SETDIBITSTODEVICE : Record
    {
        public RectL Bounds = new();

        public int XDest;

        public int YDest;

        public int XSrc;

        public int YSrc;

        public int CxSrc;

        public int CySrc;

        public uint OffBmiSrc;

        public uint CbBmiSrc;

        public uint OffBitsSrc;

        public uint CbBitsSrc;

        public DIBColors UsageSrc;

        public uint IStartScan;

        public uint CScans;

        public byte[] BmiSrc { get; set; } = Array.Empty<byte>();

        public byte[] BitsSrc { get; set; } = Array.Empty<byte>();

        public EMR_SETDIBITSTODEVICE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETDIBITSTODEVICE };
        }

        public override void Read(BinaryReader reader)
        {
            long recordStart = reader.BaseStream.Position - 8;

            Bounds.Read(reader);
            XDest = reader.ReadInt32();
            YDest = reader.ReadInt32();
            XSrc = reader.ReadInt32();
            YSrc = reader.ReadInt32();
            CxSrc = reader.ReadInt32();
            CySrc = reader.ReadInt32();
            OffBmiSrc = reader.ReadUInt32();
            CbBmiSrc = reader.ReadUInt32();
            OffBitsSrc = reader.ReadUInt32();
            CbBitsSrc = reader.ReadUInt32();
            UsageSrc = (DIBColors)reader.ReadUInt32();
            IStartScan = reader.ReadUInt32();
            CScans = reader.ReadUInt32();

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

            const uint fixedSizeAfterHeader = 76;
            uint offBmi = cbBmi > 0 ? fixedSizeAfterHeader : 0;
            uint offBits = cbBits > 0 ? fixedSizeAfterHeader + cbBmi : 0;

            Bounds.Write(writer);
            writer.Write(XDest);
            writer.Write(YDest);
            writer.Write(XSrc);
            writer.Write(YSrc);
            writer.Write(CxSrc);
            writer.Write(CySrc);
            writer.Write(offBmi);
            writer.Write(cbBmi);
            writer.Write(offBits);
            writer.Write(cbBits);
            writer.Write((uint)UsageSrc);
            writer.Write(IStartScan);
            writer.Write(CScans);
            writer.Write(BmiSrc);
            writer.Write(BitsSrc);
        }
    }
}
