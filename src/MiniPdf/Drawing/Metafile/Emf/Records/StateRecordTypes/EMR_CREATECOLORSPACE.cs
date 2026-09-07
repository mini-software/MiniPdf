using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_CREATECOLORSPACE : Record
    {
        public uint IhCS;

        public byte[] LogColorSpaceData { get; set; } = Array.Empty<byte>();

        public EMR_CREATECOLORSPACE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CREATECOLORSPACE };
        }

        public override void Read(BinaryReader reader)
        {
            IhCS = reader.ReadUInt32();
            int remaining = Convert.ToInt32(Header.Size) - 12;
            if (remaining > 0)
            {
                LogColorSpaceData = reader.ReadBytes(remaining);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhCS);
            writer.Write(LogColorSpaceData);
        }
    }
}
