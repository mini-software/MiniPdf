using System.IO;
using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
using MiniSoftware.Drawing.Metafile.Wmf.Objects;

namespace MiniSoftware.Drawing.Metafile.Wmf.Records
{
    internal class META_BITBLT : Record
    {
        public TernaryRasterOperation RasterOperation { get; set; }
        public short YSrc { get; set; }
        public short XSrc { get; set; }
        public short Height { get; set; }
        public short Width { get; set; }
        public short YDest { get; set; }
        public short XDest { get; set; }
        public DeviceIndependentBitmap? Bitmap { get; set; }

        public META_BITBLT()
        {
            Header.RecordFunction = RecordType.META_BITBLT;
        }

        public override void Read(BinaryReader reader)
        {
            RasterOperation = (TernaryRasterOperation)reader.ReadUInt32();
            YSrc = reader.ReadInt16();
            XSrc = reader.ReadInt16();
            Height = reader.ReadInt16();
            Width = reader.ReadInt16();
            YDest = reader.ReadInt16();
            XDest = reader.ReadInt16();

            if (Header.RecordSize > 11) // 11 words for the record, excluding the bitmap
            {
                Bitmap = new DeviceIndependentBitmap();
                Bitmap.Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((uint)RasterOperation);
            writer.Write(YSrc);
            writer.Write(XSrc);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(YDest);
            writer.Write(XDest);

            if (Bitmap != null)
            {
                Bitmap.Write(writer);
            }
        }
    }
}
