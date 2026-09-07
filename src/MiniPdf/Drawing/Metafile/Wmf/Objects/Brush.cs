using MiniPdf.Drawing.Metafile.Wmf;
using MiniPdf.Drawing.Metafile.Wmf.Enumerations;

namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The Brush Object defines the style, color, and pattern of a brush. Brush Objects are created by the META_CREATEBRUSHINDIRECT, META_CREATEPATTERNBRUSH and META_DIBCREATEPATTERNBRUSH records.
    /// </summary>
    /// <remarks>
    ///     2.2.1.1 Brush Object
    /// </remarks>
    internal class Brush
    {
        /// <summary>
        ///     A 16-bit unsigned integer that defines the brush style. The value MUST be an enumeration from the BrushStyle Enumeration table. For the meanings of the different values, see the following table.
        /// </summary>
        public BrushStyle BrushStyle;

        /// <summary>
        ///     A 32-bit field that specifies how to interpret color values in the object defined in the BrushHatch field. 
        ///     Its interpretation depends on the value of BrushStyle, as explained in the following table.
        /// </summary>
        public uint ColorRef;

        //TODO BrushStyle specific reading
    }
}
