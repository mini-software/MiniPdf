using MiniSoftware.Drawing.Metafile.Emf;
using MiniSoftware.Drawing.Metafile.Emf.Records;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_SETPALETTEENTRIES : Record
    {
        public uint IhPal;

        public uint Start;

        public uint NumberOfEntries;

        public byte[] PaletteEntries { get; set; } = Array.Empty<byte>();

        public EMR_SETPALETTEENTRIES()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETPALETTEENTRIES };
        }

        public override void Read(BinaryReader reader)
        {
            IhPal = reader.ReadUInt32();
            Start = reader.ReadUInt32();
            NumberOfEntries = reader.ReadUInt32();

            int remaining = Convert.ToInt32(Header.Size) - 20;
            if (remaining > 0)
            {
                PaletteEntries = reader.ReadBytes(remaining);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(IhPal);
            writer.Write(Start);
            uint entryCount = NumberOfEntries == 0 ? (uint)(PaletteEntries.Length / 4) : NumberOfEntries;
            writer.Write(entryCount);
            writer.Write(PaletteEntries);
        }
    }
}
