using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.ObjectRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_CREATEDIBPATTERNBRUSHPT : Record
    {
        public uint IhBrush;

        public DIBColors Usage;

        public uint OffBmi;

        public uint CbBmi;

        public uint OffBits;

        public uint CbBits;

        public byte[] Bmi { get; set; } = [];

        public byte[] Bits { get; set; } = [];

        public EMR_CREATEDIBPATTERNBRUSHPT()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CREATEDIBPATTERNBRUSHPT };
        }

        public override void Read(BinaryReader reader)
        {
            long recordStart = reader.BaseStream.Position - 8;

            IhBrush = reader.ReadUInt32();
            Usage = (DIBColors)reader.ReadUInt32();
            OffBmi = reader.ReadUInt32();
            CbBmi = reader.ReadUInt32();
            OffBits = reader.ReadUInt32();
            CbBits = reader.ReadUInt32();

            long restorePosition = reader.BaseStream.Position;

            if (CbBmi > 0 && OffBmi > 0)
            {
                reader.BaseStream.Position = recordStart + OffBmi;
                Bmi = reader.ReadBytes((int)CbBmi);
            }

            if (CbBits > 0 && OffBits > 0)
            {
                reader.BaseStream.Position = recordStart + OffBits;
                Bits = reader.ReadBytes((int)CbBits);
            }

            reader.BaseStream.Position = restorePosition;
        }

        public override void Write(BinaryWriter writer)
        {
            uint cbBmi = CbBmi == 0 ? (uint)Bmi.Length : CbBmi;
            uint cbBits = CbBits == 0 ? (uint)Bits.Length : CbBits;

            const uint fixedSizeAfterHeader = 32;
            uint offBmi = cbBmi > 0 ? fixedSizeAfterHeader : 0;
            uint offBits = cbBits > 0 ? fixedSizeAfterHeader + cbBmi : 0;

            writer.Write(IhBrush);
            writer.Write((uint)Usage);
            writer.Write(offBmi);
            writer.Write(cbBmi);
            writer.Write(offBits);
            writer.Write(cbBits);
            writer.Write(Bmi);
            writer.Write(Bits);
        }
    }
}
