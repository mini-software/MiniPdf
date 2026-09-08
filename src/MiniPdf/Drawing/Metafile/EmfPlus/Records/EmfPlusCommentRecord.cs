namespace MiniSoftware.Drawing.Metafile.EmfPlus.Records
{
    using System.IO;

    internal class EmfPlusCommentRecord : Record
    {
        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}
