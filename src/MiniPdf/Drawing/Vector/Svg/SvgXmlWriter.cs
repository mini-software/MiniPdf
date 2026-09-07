using System;
using System.Globalization;
using System.Xml;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Thin wrapper over <see cref="System.Xml.XmlWriter"/> providing SVG-friendly
    /// helpers: invariant number formatting with bounded precision, attribute
    /// escaping, and element emission shortcuts.
    /// </summary>
    internal sealed class SvgXmlWriter : IDisposable
    {
        internal const string SvgNamespace = "http://www.w3.org/2000/svg";
        internal const string XLinkNamespace = "http://www.w3.org/1999/xlink";

        private readonly XmlWriter _writer;
        private readonly int _precision;
        private bool _rootWritten;

        public SvgXmlWriter(XmlWriter writer, int precision = 4)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _precision = Math.Max(0, precision);
        }

        public XmlWriter Underlying => _writer;

        // ── Element shortcuts ──────────────────────────────────────────────────

        /// <summary>
        /// Writes a start element in the SVG namespace. On the first call
        /// (the root &lt;svg&gt;), also emits the xmlns:xlink declaration.
        /// </summary>
        public void WriteStartElement(string name)
        {
            _writer.WriteStartElement(name, SvgNamespace);
            if (!_rootWritten)
            {
                _writer.WriteAttributeString("xmlns", "xlink", null, XLinkNamespace);
                _rootWritten = true;
            }
        }

        public void WriteEndElement()              => _writer.WriteEndElement();
        public void WriteStartDocument()           => _writer.WriteStartDocument(true);
        public void WriteEndDocument()             => _writer.WriteEndDocument();
        public void WriteComment(string text)      => _writer.WriteComment(text);

        public void WriteElementString(string name, string value)
            => _writer.WriteElementString(name, value, SvgNamespace);

        // ── Attribute helpers ───────────────────────────────────────────────────

        public void WriteAttribute(string name, string value)
            => _writer.WriteAttributeString(name, value);

        /// <summary>
        /// Writes an xlink:href attribute (e.g. for &lt;image&gt; elements).
        /// </summary>
        public void WriteXLinkHref(string value)
            => _writer.WriteAttributeString("xlink", "href", XLinkNamespace, value);

        public void WriteAttribute(string name, float value)
            => _writer.WriteAttributeString(name, FormatNumber(value));

        public void WriteAttribute(string name, double value)
            => _writer.WriteAttributeString(name, FormatNumber(value));

        public void WriteAttribute(string name, int value)
            => _writer.WriteAttributeString(name, value.ToString(CultureInfo.InvariantCulture));

        public void WriteAttribute(string name, bool value)
            => _writer.WriteAttributeString(name, value ? "true" : "false");

        public void WriteAttributeOptional(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                _writer.WriteAttributeString(name, value);
        }

        public void WriteAttributeOptional(string name, string? value, string? omitIfEqual)
        {
            if (!string.IsNullOrEmpty(value) && value != omitIfEqual)
                _writer.WriteAttributeString(name, value);
        }

        public void WriteAttributeOptional(string name, float value, float omitIfEqual)
        {
            if (value != omitIfEqual)
                WriteAttribute(name, value);
        }

        // ── Number formatting ───────────────────────────────────────────────────

        /// <summary>
        /// Formats a number for SVG output: up to <see cref="_precision"/> decimal
        /// places, trailing zeros stripped, invariant culture. Infinity and NaN
        /// are clamped to 0.
        /// </summary>
        public string FormatNumber(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) value = 0f;
            return FormatNumber((double)value);
        }

        public string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) value = 0d;
            string s = value.ToString("F" + _precision, CultureInfo.InvariantCulture);
            if (s.IndexOf('.') >= 0)
            {
                s = s.TrimEnd('0').TrimEnd('.');
                if (s.Length == 0 || s == "-") s = "0";
            }
            return s;
        }

        public void Dispose() => _writer.Dispose();
    }
}