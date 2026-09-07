using MiniSoftware.Drawing.Metafile.Wmf;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    internal class META_SETPALENTRIES : Record
    {
        public ushort Start;

        public ushort NumberOfEntries;

        public PaletteEntry[] PaletteEntries { get; set; } = Array.Empty<PaletteEntry>();

        public META_SETPALENTRIES()
        {
            Header.RecordFunction = RecordType.META_SETPALENTRIES;
        }

        public override void Read(BinaryReader reader)
        {
            Start = reader.ReadUInt16();
            NumberOfEntries = reader.ReadUInt16();
            PaletteEntries = new PaletteEntry[NumberOfEntries];
            for (int i = 0; i < NumberOfEntries; i++)
            {
                var entry = new PaletteEntry();
                entry.Read(reader);
                PaletteEntries[i] = entry;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            ushort entryCount = NumberOfEntries == 0 ? (ushort)PaletteEntries.Length : NumberOfEntries;
            writer.Write(Start);
            writer.Write(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                PaletteEntries[i].Write(writer);
            }
        }
    }
}
