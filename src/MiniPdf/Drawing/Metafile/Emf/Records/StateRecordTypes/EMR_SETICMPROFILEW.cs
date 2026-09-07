using MiniPdf.Drawing.Metafile.Emf;
using MiniPdf.Drawing.Metafile.Emf.Records;

namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_SETICMPROFILEW : Record
    {
        public IcmProfileFlags DwFlags;

        public uint CbName;

        public uint CbData;

        public byte[] NameData { get; set; } = Array.Empty<byte>();

        public byte[] ProfileData { get; set; } = Array.Empty<byte>();

        public EMR_SETICMPROFILEW()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETICMPROFILEW };
        }

        public override void Read(BinaryReader reader)
        {
            DwFlags = (IcmProfileFlags)reader.ReadUInt32();
            CbName = reader.ReadUInt32();
            CbData = reader.ReadUInt32();
            NameData = reader.ReadBytes(Convert.ToInt32(CbName));
            ProfileData = reader.ReadBytes(Convert.ToInt32(CbData));
        }

        public override void Write(BinaryWriter writer)
        {
            uint nameLength = CbName == 0 ? (uint)NameData.Length : CbName;
            uint dataLength = CbData == 0 ? (uint)ProfileData.Length : CbData;
            writer.Write((uint)DwFlags);
            writer.Write(nameLength);
            writer.Write(dataLength);
            writer.Write(NameData);
            writer.Write(ProfileData);
        }
    }
}
