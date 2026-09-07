using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Identifies an image encoder parameter category.
    /// </summary>
    public sealed class Encoder
    {
        /// <summary>
        /// Gets the GUID of this encoder.
        /// </summary>
        public Guid Guid { get; }

        private Encoder(string guid) { Guid = new Guid(guid); }

        /// <summary>
        /// Gets the compression encoder.
        /// </summary>
        public static readonly Encoder Compression      = new Encoder("e09d739d-ccd4-44ee-8eba-3fbef14a6159");

        /// <summary>
        /// Gets the color depth encoder.
        /// </summary>
        public static readonly Encoder ColorDepth       = new Encoder("66087055-ad66-4c7c-9a18-38a2310b8337");

        /// <summary>
        /// Gets the scan method encoder.
        /// </summary>
        public static readonly Encoder ScanMethod       = new Encoder("3a4e2661-3109-4e56-8536-42c156e7dcfa");

        /// <summary>
        /// Gets the version encoder.
        /// </summary>
        public static readonly Encoder Version          = new Encoder("24d18c76-814a-41a4-bf53-1c219cccf797");

        /// <summary>
        /// Gets the render method encoder.
        /// </summary>
        public static readonly Encoder RenderMethod     = new Encoder("6d42c53a-229a-4825-8bb7-5c99e2b9a8b8");

        /// <summary>
        /// Gets the quality encoder.
        /// </summary>
        public static readonly Encoder Quality          = new Encoder("1d5be4b5-fa4a-452d-9cdd-5db35105e7eb");

        /// <summary>
        /// Gets the transformation encoder.
        /// </summary>
        public static readonly Encoder Transformation   = new Encoder("8d0eb2d1-a58e-4ea8-aa14-108074b7b6f9");

        /// <summary>
        /// Gets the luminance table encoder.
        /// </summary>
        public static readonly Encoder LuminanceTable   = new Encoder("edb33bce-0266-4a77-b904-27216099e717");

        /// <summary>
        /// Gets the chrominance table encoder.
        /// </summary>
        public static readonly Encoder ChrominanceTable = new Encoder("f2e455dc-09b3-4316-8260-676ada32481c");

        /// <summary>
        /// Gets the save flag encoder.
        /// </summary>
        public static readonly Encoder SaveFlag         = new Encoder("292266fc-ac40-47bf-8cfc-a85b89a655de");
    }
}
