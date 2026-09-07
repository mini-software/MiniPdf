using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.Sources;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Represents a collection of <see cref="FontFamily"/> objects.
    /// </summary>
    public abstract class FontCollection : IDisposable
    {
        private readonly Dictionary<string, FontFamily> _familiesByName = new Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the array of font families in this collection.
        /// </summary>
        public FontFamily[] Families
        {
            get
            {
                var result = new FontFamily[_familiesByName.Count];
                _familiesByName.Values.CopyTo(result, 0);
                return result;
            }
        }

        /// <summary>
        /// Adds or merges a font family into this collection.
        /// </summary>
        /// <param name="family">The font family to add or merge.</param>
        protected void AddOrMergeFamily(FontFamily family)
        {
            if (family == null)
                throw new ArgumentNullException(nameof(family));

            if (_familiesByName.TryGetValue(family.Name, out var existing))
            {
                existing.MergeFrom(family.CloneData());
                return;
            }

            _familiesByName[family.Name] = new FontFamily(family.CloneData());
        }

        /// <summary>
        /// Tries to find a font family by name in this collection.
        /// </summary>
        /// <param name="name">The case-insensitive family name to look up.</param>
        /// <returns>
        /// The matching <see cref="FontFamily"/> cloned from the collection, or
        /// <c>null</c> when no family with that name is present.
        /// </returns>
        public FontFamily? FindFamily(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (_familiesByName.TryGetValue(name, out var existing))
                return new FontFamily(existing.CloneData());

            return null;
        }

        /// <summary>
        /// Loads every face from a font collection file (single font or TTC) and merges
        /// the resulting families into this collection.
        /// </summary>
        /// <param name="source">The stream source pointing at the font file.</param>
        protected void AddFacesFromSource(IStreamSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var collection = MiniSoftware.Drawing.Font.FontFactory.OpenCollection(source);
            AddFaces(collection);
        }

        /// <summary>
        /// Merges every face in a loaded font collection into this
        /// <see cref="FontCollection"/> as <see cref="FontFamily"/> entries.
        /// </summary>
        /// <param name="collection">The loaded font faces.</param>
        protected void AddFaces(MiniSoftware.Drawing.Font.Core.FontFaceCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var face in collection)
            {
                var data = new FontFamilyData(face.FontFamily);
                var drawingStyle = MapStyle(face.Style);
                data.AddStyle(drawingStyle, FontMetricsData.FromMetrics(face.Metrics));
                data.AddFontFace(drawingStyle, face);
                AddOrMergeFamily(new FontFamily(data));
            }
        }

        /// <summary>
        /// Maps the integrated font engine <see cref="MiniSoftware.Drawing.Font.Core.FontFaceStyle"/>
        /// to the GDI+-compatible <see cref="Enums.FontStyle"/> flags.
        /// </summary>
        private static Enums.FontStyle MapStyle(MiniSoftware.Drawing.Font.Core.FontFaceStyle style)
        {
            switch (style)
            {
                case MiniSoftware.Drawing.Font.Core.FontFaceStyle.Bold:
                    return Enums.FontStyle.Bold;
                case MiniSoftware.Drawing.Font.Core.FontFaceStyle.Italic:
                    return Enums.FontStyle.Italic;
                case MiniSoftware.Drawing.Font.Core.FontFaceStyle.BoldItalic:
                    return Enums.FontStyle.Bold | Enums.FontStyle.Italic;
                default:
                    return Enums.FontStyle.Regular;
            }
        }

        /// <summary>
        /// Releases resources used by the <see cref="FontCollection"/>.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}