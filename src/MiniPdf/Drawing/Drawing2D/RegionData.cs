namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Encapsulates the data that describes a Region.
    /// </summary>
    public sealed class RegionData
    {
        /// <summary>The raw binary data for this region.</summary>
        public byte[] Data { get; set; }

        internal RegionData(byte[] data) { Data = data; }
    }
}
