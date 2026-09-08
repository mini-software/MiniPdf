using MiniSoftware.Drawing.Metafile.Emf;

namespace MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System.IO;
    using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
    using MiniSoftware.Drawing.Metafile.Emf.Objects;
    using MiniSoftware.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The EMR_EXTTEXTOUTA record draws an ASCII text string using the current font and text colors.
    /// </summary>
    /// <remarks>
    ///     2.3.5.7 EMR_EXTTEXTOUTA Record - MS-EMF Page 149
    /// </remarks>
    internal class EMR_EXTTEXTOUTA : Record
    {
        /// <summary>
        ///     A WMF RectL object ([MS-WMF] section 2.2.2.19). It is not used and MUST be ignored on receipt.
        /// </summary>
        public RectL Bounds;

        /// <summary>
        ///     A 32-bit unsigned integer that specifies the graphics mode from the GraphicsMode enumeration (section 2.1.16).
        /// </summary>
        public GraphicsMode IGraphicsMode;

        /// <summary>
        ///     A 32-bit floating-point value that specifies the scale factor to apply along the X axis to convert from page space units to .01mm units. 
        ///     This SHOULD be used only if the graphics mode specified by iGraphicsMode is GM_COMPATIBLE.
        /// </summary>
        public float ExScale;

        /// <summary>
        ///     A 32-bit floating-point value that specifies the scale factor to apply along the Y axis to convert from page space units to .01mm units. 
        ///     This SHOULD be used only if the graphics mode specified by iGraphicsMode is GM_COMPATIBLE.
        /// </summary>
        public float EyScale;

        public EmrText EmrText { get; set; } = new();

        public EMR_EXTTEXTOUTA()
        {
            Header = new RecordHeader { Type = RecordType.EMR_EXTTEXTOUTA };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds = new RectL();
            Bounds.Read(reader);
            IGraphicsMode = (GraphicsMode)reader.ReadUInt32();
            ExScale = reader.ReadSingle();
            EyScale = reader.ReadSingle();
            EmrText.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            this.Bounds.Write(writer);
            writer.Write((uint)this.IGraphicsMode);
            writer.Write(this.ExScale);
            writer.Write(this.EyScale);
            EmrText.Write(writer);
        }
    }
}
