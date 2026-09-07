using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Represents a single parameter for an image encoder.
    /// </summary>
    public sealed class EncoderParameter : IDisposable
    {
        /// <summary>
        /// Gets or sets the encoder GUID for this parameter.
        /// </summary>
        public Guid Encoder       { get; set; }

        /// <summary>
        /// Gets or sets the number of values in this parameter.
        /// </summary>
        public int  NumberOfValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter value.
        /// </summary>
        public int  Type          { get; set; }

        // Internal storage for common value types
        internal long   _valueLong;
        internal byte[] _valueBytes = Array.Empty<byte>();

        /// <summary>
        /// Initializes a new <see cref="EncoderParameter"/> with a long value.
        /// </summary>
        /// <param name="encoder">The encoder for this parameter.</param>
        /// <param name="value">The long value.</param>
        public EncoderParameter(Encoder encoder, long value)
        {
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            Encoder        = encoder.Guid;
            NumberOfValues = 1;
            Type           = 4;   // ValueTypeLong
            _valueLong     = value;
        }

        /// <summary>
        /// Initializes a new <see cref="EncoderParameter"/> with a byte array value.
        /// </summary>
        /// <param name="encoder">The encoder for this parameter.</param>
        /// <param name="value">The byte array value.</param>
        public EncoderParameter(Encoder encoder, byte[] value)
        {
            if (encoder == null) throw new ArgumentNullException(nameof(encoder));
            Encoder        = encoder.Guid;
            NumberOfValues = value?.Length ?? 0;
            Type           = 1;   // ValueTypeByte
            _valueBytes    = value ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Releases resources used by the <see cref="EncoderParameter"/>.
        /// </summary>
        public void Dispose() { }
    }
}
