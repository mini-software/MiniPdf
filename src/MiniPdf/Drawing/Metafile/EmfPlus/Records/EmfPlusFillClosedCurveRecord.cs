namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusFillClosedCurveRecord : Record
    {
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
