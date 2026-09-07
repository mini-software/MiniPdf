namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusObjectRecord : Record
    {
        public bool Continued
        {
            get
            {
                return Convert.ToBoolean(BitHelper.GetBits(this.Header.Flags, 0, 1, false));
            }
        }

        public ObjectType ObjectType
        {
            get
            {
                return (ObjectType)BitHelper.GetBits(this.Header.Flags, 1, 7, false);
            }
        }

        public byte ObjectId
        {
            get
            {
                return Convert.ToByte(BitHelper.GetBits(this.Header.Flags, 8, 8, false));
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}",base.ToString(), this.ObjectType.ToString());
        }

        public override void Read(BinaryReader reader)
        {
            Data = reader.ReadBytes(Convert.ToInt32(Header.DataSize));
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}
