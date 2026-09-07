using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System;
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;

    internal class EMR_HEADER : Record
    {
        public Header HeaderObject { get; set; } = new();

        public HeaderExtension1? Extension1 { get; set; }

        public HeaderExtension2? Extension2 { get; set; }

        public byte[] DescriptionData { get; set; } = Array.Empty<byte>();

        public EMR_HEADER()
        {
            Header = new RecordHeader { Type = RecordType.EMR_HEADER };
        }

        public override void Read(BinaryReader reader)
        {
            HeaderObject.Read(reader);

            long bytesRead = 72;
            long remaining = Header.Size - 8 - bytesRead;

            if (remaining >= 12)
            {
                Extension1 = new HeaderExtension1();
                Extension1.Read(reader);
                remaining -= 12;
            }

            if (remaining >= 8)
            {
                Extension2 = new HeaderExtension2();
                Extension2.Read(reader);
                remaining -= 8;
            }

            if (remaining > 0)
            {
                DescriptionData = reader.ReadBytes(Convert.ToInt32(remaining));
            }
        }

        public override void Write(BinaryWriter writer)
        {
            HeaderObject.Write(writer);
            Extension1?.Write(writer);
            Extension2?.Write(writer);
            writer.Write(DescriptionData);
        }
    }
}
