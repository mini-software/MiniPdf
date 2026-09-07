namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Describes one image component in a JPEG SOF segment
    /// (e.g. Y, Cb, Cr in a YCbCr image).
    /// </summary>
    internal struct FrameComponent
    {
        /// <summary>Component identifier byte (typically 1=Y, 2=Cb, 3=Cr).</summary>
        public byte Id;

        /// <summary>Horizontal sampling factor (1–4).</summary>
        public byte HSampling;

        /// <summary>Vertical sampling factor (1–4).</summary>
        public byte VSampling;

        /// <summary>Quantization table destination id (0–3).</summary>
        public byte QuantizationTableId;
    }
}
