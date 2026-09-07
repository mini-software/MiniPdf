using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Contains a collection of <see cref="EncoderParameter"/> objects for image encoding.
    /// </summary>
    public sealed class EncoderParameters : IDisposable
    {
        /// <summary>
        /// Gets or sets the array of encoder parameters.
        /// </summary>
        public EncoderParameter[] Param { get; set; }

        /// <summary>
        /// Initializes a new <see cref="EncoderParameters"/> with an empty parameter array.
        /// </summary>
        public EncoderParameters()                { Param = Array.Empty<EncoderParameter>(); }

        /// <summary>
        /// Initializes a new <see cref="EncoderParameters"/> with the specified capacity.
        /// </summary>
        /// <param name="count">The number of parameters to allocate space for.</param>
        public EncoderParameters(int count)       { Param = new EncoderParameter[count]; }

        /// <summary>
        /// Releases resources used by the <see cref="EncoderParameters"/>.
        /// </summary>
        public void Dispose() { }
    }
}
