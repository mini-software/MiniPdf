using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_CREATEPALETTE : Record
    {
        public uint IhPal;

        public byte[] LogPaletteData { get; set; } = Array.Empty<byte>();

        public EMR_CREATEPALETTE()
        {
            Header = new RecordHeader { Type = RecordType.EMR_CREATEPALETTE };
        }

        public override void Read(BinaryReader reader)
        {
            IhPal = reader.ReadUInt32();
            int remaining = Convert.ToInt32(Header.Size) - 12;
            if (remaining > 0)
            {
                LogPaletteData = reader.ReadBytes(remaining);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhPal);
            writer.Write(LogPaletteData);
        }
    }
}
