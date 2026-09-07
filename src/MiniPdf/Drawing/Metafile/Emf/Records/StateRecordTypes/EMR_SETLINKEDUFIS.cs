namespace MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using System;
    using System.IO;

    internal class EMR_SETLINKEDUFIS : Record
    {
        public uint NumberOfLinkedUfis;

        public byte[] LinkedUfisData { get; set; } = Array.Empty<byte>();

        public EMR_SETLINKEDUFIS()
        {
            Header = new RecordHeader { Type = RecordType.EMR_SETLINKEDUFIS };
        }

        public override void Read(BinaryReader reader)
        {
            NumberOfLinkedUfis = reader.ReadUInt32();
            int remaining = Convert.ToInt32(Header.Size) - 12;
            if (remaining > 0)
            {
                LinkedUfisData = reader.ReadBytes(remaining);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(NumberOfLinkedUfis);
            writer.Write(LinkedUfisData);
        }
    }
}
