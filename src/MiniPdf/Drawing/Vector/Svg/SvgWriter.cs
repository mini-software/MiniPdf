using System;
using System.IO;
using System.Xml;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Top-level SVG writer: serializes a <see cref="VectorScene"/> to an SVG
    /// document using a two-pass pipeline:
    /// <list type="number">
    ///   <item>Def-collection pass: replay onto <see cref="SvgDefCollector"/>
    ///     to gather unique brushes/clips.</item>
    ///   <item>Emit pass: write <c>&lt;svg&gt;</c> root, <c>&lt;defs&gt;</c>,
    ///     then replay onto <see cref="SvgDrawingContext"/> for the body.</item>
    /// </list>
    /// </summary>
    internal static class SvgWriter
    {
        /// <summary>
        /// Writes the specified scene as an SVG document to the given stream.
        /// </summary>
        public static void Write(Stream stream, VectorScene scene, SvgEncoderParameters? parameters = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (scene == null) throw new ArgumentNullException(nameof(scene));

            var p = parameters ?? SvgEncoderParameters.Default;

            var settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = p.Indent,
                IndentChars = "  ",
                OmitXmlDeclaration = false,
            };

            using var xmlWriter = XmlWriter.Create(stream, settings);
            Write(xmlWriter, scene, p);
        }

        /// <summary>
        /// Writes the specified scene as an SVG document to the given
        /// <see cref="XmlWriter"/>.
        /// </summary>
        public static void Write(XmlWriter xmlWriter, VectorScene scene, SvgEncoderParameters? parameters = null)
        {
            if (xmlWriter == null) throw new ArgumentNullException(nameof(xmlWriter));
            if (scene == null) throw new ArgumentNullException(nameof(scene));

            var p = parameters ?? SvgEncoderParameters.Default;

            // ── Pass 1: def collection ──────────────────────────────────────
            var defs = new SvgDefTable();
            using (var collector = new SvgDefCollector(defs, scene.Width, scene.Height))
            {
                VectorScenePlayer.Play(scene, collector);
            }

            // ── Pass 2: emit ────────────────────────────────────────────────
            using var w = new SvgXmlWriter(xmlWriter, p.CoordinatePrecision);
            w.WriteStartDocument();

            w.WriteStartElement("svg");
            w.WriteAttribute("width", scene.Width);
            w.WriteAttribute("height", scene.Height);
            w.WriteAttribute("viewBox", "0 0 " + scene.Width + " " + scene.Height);

            // Write defs (gradients, patterns, clip paths)
            SvgBrushWriter.WriteDefs(w, defs);

            // Write body: replay scene onto SvgDrawingContext
            using (var ctx = new SvgDrawingContext(w, defs, p, scene.Width, scene.Height))
            {
                VectorScenePlayer.Play(scene, ctx);
            }

            w.WriteEndElement(); // </svg>
            w.WriteEndDocument();
            xmlWriter.Flush();
        }

        /// <summary>
        /// Returns the SVG document as a string.
        /// </summary>
        public static string WriteToString(VectorScene scene, SvgEncoderParameters? parameters = null)
        {
            using var ms = new MemoryStream();
            Write(ms, scene, parameters);
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }
    }
}