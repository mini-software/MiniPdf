namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusHeaderRecord : Record
    {
        public uint Version;
        public EmfPlusHeaderFlags EmfPlusFlags;
        public uint LogicalDpiX;
        public uint LogicalDpiY;

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize >= 16)
            {
                Version = reader.ReadUInt32();
                EmfPlusFlags = (EmfPlusHeaderFlags)reader.ReadUInt32();
                LogicalDpiX = reader.ReadUInt32();
                LogicalDpiY = reader.ReadUInt32();

                int remaining = Convert.ToInt32(Header.DataSize) - 16;
                if (remaining > 0)
                {
                    Data = reader.ReadBytes(remaining);
                }
            }
            else
            {
                base.Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((uint)EmfPlusFlags);
            writer.Write(LogicalDpiX);
            writer.Write(LogicalDpiY);
            writer.Write(Data);
        }
    }
}
