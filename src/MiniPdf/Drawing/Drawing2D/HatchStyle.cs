namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies the hatch style for <see cref="HatchBrush"/> fill patterns.
    /// </summary>
    public enum HatchStyle
    {
        /// <summary>
        /// Horizontal hatch pattern.
        /// </summary>
        Horizontal              = 0,

        /// <summary>
        /// Vertical hatch pattern.
        /// </summary>
        Vertical                = 1,

        /// <summary>
        /// Forward diagonal hatch pattern (top-left to bottom-right).
        /// </summary>
        ForwardDiagonal         = 2,

        /// <summary>
        /// Backward diagonal hatch pattern (top-right to bottom-left).
        /// </summary>
        BackwardDiagonal        = 3,

        /// <summary>
        /// Cross hatch pattern.
        /// </summary>
        Cross                   = 4,

        /// <summary>
        /// Large grid hatch pattern (alias for Cross).
        /// </summary>
        LargeGrid               = 4,   // alias

        /// <summary>
        /// Diagonal cross hatch pattern.
        /// </summary>
        DiagonalCross           = 5,

        /// <summary>
        /// 5% hatch pattern.
        /// </summary>
        Percent05               = 6,

        /// <summary>
        /// 10% hatch pattern.
        /// </summary>
        Percent10               = 7,

        /// <summary>
        /// 20% hatch pattern.
        /// </summary>
        Percent20               = 8,

        /// <summary>
        /// 25% hatch pattern.
        /// </summary>
        Percent25               = 9,

        /// <summary>
        /// 30% hatch pattern.
        /// </summary>
        Percent30               = 10,

        /// <summary>
        /// 40% hatch pattern.
        /// </summary>
        Percent40               = 11,

        /// <summary>
        /// 50% hatch pattern.
        /// </summary>
        Percent50               = 12,

        /// <summary>
        /// 60% hatch pattern.
        /// </summary>
        Percent60               = 13,

        /// <summary>
        /// 70% hatch pattern.
        /// </summary>
        Percent70               = 14,

        /// <summary>
        /// 75% hatch pattern.
        /// </summary>
        Percent75               = 15,

        /// <summary>
        /// 80% hatch pattern.
        /// </summary>
        Percent80               = 16,

        /// <summary>
        /// 90% hatch pattern.
        /// </summary>
        Percent90               = 17,

        /// <summary>
        /// Light downward diagonal hatch pattern.
        /// </summary>
        LightDownwardDiagonal   = 18,

        /// <summary>
        /// Light upward diagonal hatch pattern.
        /// </summary>
        LightUpwardDiagonal     = 19,

        /// <summary>
        /// Dark downward diagonal hatch pattern.
        /// </summary>
        DarkDownwardDiagonal    = 20,

        /// <summary>
        /// Dark upward diagonal hatch pattern.
        /// </summary>
        DarkUpwardDiagonal      = 21,

        /// <summary>
        /// Wide downward diagonal hatch pattern.
        /// </summary>
        WideDownwardDiagonal    = 22,

        /// <summary>
        /// Wide upward diagonal hatch pattern.
        /// </summary>
        WideUpwardDiagonal      = 23,

        /// <summary>
        /// Light vertical hatch pattern.
        /// </summary>
        LightVertical           = 24,

        /// <summary>
        /// Light horizontal hatch pattern.
        /// </summary>
        LightHorizontal         = 25,

        /// <summary>
        /// Narrow vertical hatch pattern.
        /// </summary>
        NarrowVertical          = 26,

        /// <summary>
        /// Narrow horizontal hatch pattern.
        /// </summary>
        NarrowHorizontal        = 27,

        /// <summary>
        /// Dark vertical hatch pattern.
        /// </summary>
        DarkVertical            = 28,

        /// <summary>
        /// Dark horizontal hatch pattern.
        /// </summary>
        DarkHorizontal          = 29,

        /// <summary>
        /// Dashed downward diagonal hatch pattern.
        /// </summary>
        DashedDownwardDiagonal  = 30,

        /// <summary>
        /// Dashed upward diagonal hatch pattern.
        /// </summary>
        DashedUpwardDiagonal    = 31,

        /// <summary>
        /// Dashed horizontal hatch pattern.
        /// </summary>
        DashedHorizontal        = 32,

        /// <summary>
        /// Dashed vertical hatch pattern.
        /// </summary>
        DashedVertical          = 33,

        /// <summary>
        /// Small confetti hatch pattern.
        /// </summary>
        SmallConfetti           = 34,

        /// <summary>
        /// Large confetti hatch pattern.
        /// </summary>
        LargeConfetti           = 35,

        /// <summary>
        /// Zigzag hatch pattern.
        /// </summary>
        ZigZag                  = 36,

        /// <summary>
        /// Wave hatch pattern.
        /// </summary>
        Wave                    = 37,

        /// <summary>
        /// Diagonal brick hatch pattern.
        /// </summary>
        DiagonalBrick           = 38,

        /// <summary>
        /// Horizontal brick hatch pattern.
        /// </summary>
        HorizontalBrick         = 39,

        /// <summary>
        /// Weave hatch pattern.
        /// </summary>
        Weave                   = 40,

        /// <summary>
        /// Plaid hatch pattern.
        /// </summary>
        Plaid                   = 41,

        /// <summary>
        /// Divot hatch pattern.
        /// </summary>
        Divot                   = 42,

        /// <summary>
        /// Dotted grid hatch pattern.
        /// </summary>
        DottedGrid              = 43,

        /// <summary>
        /// Dotted diamond hatch pattern.
        /// </summary>
        DottedDiamond           = 44,

        /// <summary>
        /// Shingle hatch pattern.
        /// </summary>
        Shingle                 = 45,

        /// <summary>
        /// Trellis hatch pattern.
        /// </summary>
        Trellis                 = 46,

        /// <summary>
        /// Sphere hatch pattern.
        /// </summary>
        Sphere                  = 47,

        /// <summary>
        /// Small grid hatch pattern.
        /// </summary>
        SmallGrid               = 48,

        /// <summary>
        /// Small checkerboard hatch pattern.
        /// </summary>
        SmallCheckerBoard       = 49,

        /// <summary>
        /// Large checkerboard hatch pattern.
        /// </summary>
        LargeCheckerBoard       = 50,

        /// <summary>
        /// Outlined diamond hatch pattern.
        /// </summary>
        OutlinedDiamond         = 51,

        /// <summary>
        /// Solid diamond hatch pattern.
        /// </summary>
        SolidDiamond            = 52,

        /// <summary>
        /// Minimum hatch style value.
        /// </summary>
        Min                     = Horizontal,

        /// <summary>
        /// Maximum hatch style value.
        /// </summary>
        Max                     = SolidDiamond,
    }
}
