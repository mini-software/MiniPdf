using MiniSoftware.Drawing.Metafile.Wmf;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;

namespace MiniSoftware.Drawing.Metafile.Wmf.Objects
{
    /// <summary>
    ///     The LogBrush Object defines the style, color, and pattern of a brush. This object is used only in the META_CREATEBRUSHINDIRECT Record (section 2.3.4.1) to create a Brush Object (section 2.2.1.1).
    /// </summary>
    /// <remarks>
    ///     2.2.2.10 LogBrush Object
    /// </remarks>
    internal class LogBrush
    {
        /// <summary>
        ///     A 16-bit unsigned integer that defines the brush style. This MUST be a value from the BrushStyle Enumeration (section 2.1.1.4). For the meanings of different values, see the following table. The BS_NULL style specifies a brush that has no effect.
        /// </summary>
        public BrushStyle BrushStyle;

        /// <summary>
        ///     A 32-bit ColorRef Object (section 2.2.2.8) that specifies a color. 
        ///     Its interpretation depends on the value of BrushStyle, as explained in the following.
        /// </summary>
        public ColorRef ColorRef;

        /// <summary>
        ///     A 16-bit field that specifies the brush hatch type. Its interpretation depends on the value of BrushStyle, as explained in the following.
        /// </summary>
        public HatchStyle? BrushHatch;
    }
}
