using System.Collections;
using System.Collections.Generic;

namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// An ordered collection of font faces loaded from a font collection file (e.g. TTC).
    /// </summary>
    public sealed class FontFaceCollection : IReadOnlyList<IFont>
    {
        private readonly IReadOnlyList<IFont> _fonts;

        /// <summary>
        /// Initializes a new <see cref="FontFaceCollection"/> from the specified list of fonts.
        /// </summary>
        /// <param name="fonts">The list of fonts in the collection.</param>
        public FontFaceCollection(IReadOnlyList<IFont> fonts)
        {
            _fonts = fonts ?? throw new System.ArgumentNullException(nameof(fonts));
        }

        /// <inheritdoc/>
        public IFont this[int index] => _fonts[index];

        /// <inheritdoc/>
        public int Count => _fonts.Count;

        /// <inheritdoc/>
        public IEnumerator<IFont> GetEnumerator() => _fonts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_fonts).GetEnumerator();
    }
}
