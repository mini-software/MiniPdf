namespace MiniSoftware.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     The 16-bit PenStyle Enumeration is used to specify different types of pens that can be used in graphics operations.
    /// 
    ///     Various styles can be combined by using a logical OR statement, one from each subsection of Style, EndCap, Join, and Type (Cosmetic).
    /// </summary>
    /// <remarks>
    ///     2.1.1.23 PenStyle Enumeration
    /// </remarks>
    internal enum PenStyle
    {
        PS_COSMETIC = 0x0000,
        PS_ENDCAP_ROUND = 0x0000,
        PS_JOIN_ROUND = 0x0000,
        PS_SOLID = 0x0000,
        PS_DASH = 0x0001,
        PS_DOT = 0x0002,
        PS_DASHDOT = 0x0003,
        PS_DASHDOTDOT = 0x0004,
        PS_NULL = 0x0005,
        PS_INSIDEFRAME = 0x0006,
        PS_USERSTYLE = 0x0007,
        PS_ALTERNATE = 0x0008,
        PS_ENDCAP_SQUARE = 0x0100,
        PS_ENDCAP_FLAT = 0x0200,
        PS_JOIN_BEVEL = 0x1000,
        PS_JOIN_MITER = 0x2000
    }
}
