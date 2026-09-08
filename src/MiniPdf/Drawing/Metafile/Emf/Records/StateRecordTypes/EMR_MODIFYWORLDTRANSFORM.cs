using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;

    internal class EMR_MODIFYWORLDTRANSFORM : Record
    {
        public float M11;

        public float M12;

        public float M21;

        public float M22;

        public float Dx;

        public float Dy;

        public ModifyWorldTransformMode ModifyWorldTransformMode;

        public EMR_MODIFYWORLDTRANSFORM()
        {
            Header = new RecordHeader { Type = RecordType.EMR_MODIFYWORLDTRANSFORM };
        }

        public override void Read(BinaryReader reader)
        {
            M11 = reader.ReadSingle();
            M12 = reader.ReadSingle();
            M21 = reader.ReadSingle();
            M22 = reader.ReadSingle();
            Dx = reader.ReadSingle();
            Dy = reader.ReadSingle();
            ModifyWorldTransformMode = (ModifyWorldTransformMode)reader.ReadUInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(M11);
            writer.Write(M12);
            writer.Write(M21);
            writer.Write(M22);
            writer.Write(Dx);
            writer.Write(Dy);
            writer.Write((uint)ModifyWorldTransformMode);
        }
    }
}
