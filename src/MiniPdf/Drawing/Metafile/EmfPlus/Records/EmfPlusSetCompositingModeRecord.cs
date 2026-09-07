namespace MiniPdf.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;
    using MiniPdf.Drawing.Metafile.EmfPlus.Enumerations;

    internal class EmfPlusSetCompositingModeRecord : Record
    {
        public EmfPlusCompositingMode CompositingMode => (EmfPlusCompositingMode)(Header.Flags & 0x00FF);

        public override void Read(BinaryReader reader)
        {
            if (Header.DataSize > 0)
            {
                Data = reader.ReadBytes((int)Header.DataSize);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}
