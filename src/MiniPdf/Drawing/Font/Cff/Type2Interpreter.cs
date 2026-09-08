using System;
using MiniSoftware.Drawing.Font.Rendering;

namespace MiniSoftware.Drawing.Font.Cff
{
    /// <summary>
    /// Directly executes a Type 2 charstring, emitting drawing commands to an
    /// <see cref="IGlyphOutlinePainter"/>. The interpreter is reentrant: recursive
    /// calls for <c>callsubr</c> / <c>callgsubr</c> share the same
    /// <see cref="InterpState"/> instance.
    /// </summary>
    internal sealed class Type2Interpreter
    {
        // ── Subroutine bias ───────────────────────────────────────────────────

        internal static int CalcBias(int nSubrs)
        {
            if (nSubrs < 1240)  return 107;
            if (nSubrs < 33900) return 1131;
            return 32768;
        }

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Executes <paramref name="charstring"/> and delivers outline commands
        /// to <paramref name="painter"/>.
        /// </summary>
        /// <returns>The advance width of the glyph in font units.</returns>
        public double Interpret(
            byte[]           charstring,
            IGlyphOutlinePainter painter,
            CffPrivateDict   priv,
            byte[][]?        globalSubrs)
        {
            if (charstring is null || charstring.Length == 0)
                return priv.DefaultWidthX;

            var state = new InterpState(painter, priv, globalSubrs);
            Execute(charstring, state);
            if (state.InPath) { painter.ClosePath(); state.InPath = false; }
            return state.Width;
        }

        // ── Core execution ────────────────────────────────────────────────────

        private static void Execute(byte[] data, InterpState s)
        {
            int pos = 0;
            int len = data.Length;

            while (pos < len)
            {
                byte b0 = data[pos++];

                // ── Number operands ───────────────────────────────────────────
                if (b0 >= 32 && b0 <= 246)
                {
                    s.Push(b0 - 139);
                    continue;
                }
                if (b0 >= 247 && b0 <= 250)
                {
                    if (pos >= len) return;
                    s.Push((b0 - 247) * 256.0 + data[pos++] + 108);
                    continue;
                }
                if (b0 >= 251 && b0 <= 254)
                {
                    if (pos >= len) return;
                    s.Push(-((b0 - 251) * 256.0 + data[pos++] + 108));
                    continue;
                }
                if (b0 == 28) // shortint
                {
                    if (pos + 1 >= len) return;
                    int v = (sbyte)data[pos] << 8 | data[pos + 1];
                    pos += 2;
                    s.Push(v);
                    continue;
                }
                if (b0 == 255) // fixed 16.16
                {
                    if (pos + 3 >= len) return;
                    int fixed1616 = (int)((uint)(data[pos] << 24) | (uint)(data[pos + 1] << 16) |
                                         (uint)(data[pos + 2] << 8) | data[pos + 3]);
                    pos += 4;
                    s.Push(fixed1616 / 65536.0);
                    continue;
                }

                // ── Two-byte operators (escape 12) ────────────────────────────
                if (b0 == 12)
                {
                    if (pos >= len) return;
                    byte b1 = data[pos++];
                    switch (b1)
                    {
                        case 3:  // and
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push((v1 != 0 && v2 != 0) ? 1.0 : 0.0);
                            break;
                        }
                        case 4:  // or
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push((v1 != 0 || v2 != 0) ? 1.0 : 0.0);
                            break;
                        }
                        case 5:  // not
                            s.Push(s.Pop() == 0 ? 1.0 : 0.0);
                            break;
                        case 9:  // abs
                            s.Push(Math.Abs(s.Pop()));
                            break;
                        case 10: // add
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v1 + v2);
                            break;
                        }
                        case 11: // sub
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v1 - v2);
                            break;
                        }
                        case 12: // div
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v2 == 0 ? 0 : v1 / v2);
                            break;
                        }
                        case 14: // neg
                            s.Push(-s.Pop());
                            break;
                        case 15: // eq
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v1 == v2 ? 1.0 : 0.0);
                            break;
                        }
                        case 18: // drop
                            if (s.Top > 0) s.Pop();
                            break;
                        case 20: // put
                        {
                            int idx = (int)s.Pop();
                            double val = s.Pop();
                            if (idx >= 0 && idx < s.Storage.Length) s.Storage[idx] = val;
                            break;
                        }
                        case 21: // get
                        {
                            int idx = (int)s.Pop();
                            s.Push(idx >= 0 && idx < s.Storage.Length ? s.Storage[idx] : 0);
                            break;
                        }
                        case 22: // ifelse
                        {
                            double s2 = s.Pop(), s1 = s.Pop();
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(s1 <= s2 ? v1 : v2);
                            break;
                        }
                        case 23: // random — push deterministic 0.5
                            s.Push(0.5);
                            break;
                        case 24: // mul
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v1 * v2);
                            break;
                        }
                        case 26: // sqrt
                            s.Push(Math.Sqrt(Math.Abs(s.Pop())));
                            break;
                        case 27: // dup
                        {
                            double v = s.Peek();
                            s.Push(v);
                            break;
                        }
                        case 28: // exch
                        {
                            double v2 = s.Pop(), v1 = s.Pop();
                            s.Push(v2); s.Push(v1);
                            break;
                        }
                        case 29: // index
                        {
                            int idx = (int)s.Pop();
                            s.Push(idx < 0 ? s.Peek() : s.PeekAt(idx));
                            break;
                        }
                        case 30: // roll
                        {
                            int j = (int)s.Pop();
                            int n = (int)s.Pop();
                            s.Roll(n, j);
                            break;
                        }
                        case 34: // hflex
                            ExecHFlex(s);
                            break;
                        case 35: // flex
                            ExecFlex(s);
                            break;
                        case 36: // hflex1
                            ExecHFlex1(s);
                            break;
                        case 37: // flex1
                            ExecFlex1(s);
                            break;
                        default:
                            s.ClearStack();
                            break;
                    }
                    continue;
                }

                // ── Single-byte operators ─────────────────────────────────────
                switch (b0)
                {
                    case 1:  // hstem
                    case 3:  // vstem
                    case 18: // hstemhm
                    case 23: // vstemhm
                        DoStem(s);
                        break;

                    case 4:  // vmoveto
                    {
                        ExtractWidthIfFirst(s, 2);
                        double dy = s.Pop();
                        s.ClearStack();
                        DoMoveTo(s, 0, dy);
                        break;
                    }

                    case 5:  // rlineto
                    {
                        int i = 0;
                        while (i + 1 < s.Top)
                        {
                            s.CX += s.Stack[i]; s.CY += s.Stack[i + 1]; i += 2;
                            s.Painter.LineTo(new LineTo(s.CX, s.CY));
                        }
                        s.ClearStack();
                        break;
                    }

                    case 6:  // hlineto
                        DoHLineTo(s);
                        break;

                    case 7:  // vlineto
                        DoVLineTo(s);
                        break;

                    case 8:  // rrcurveto
                        DoRRCurveTo(s);
                        break;

                    case 10: // callsubr
                    {
                        int idx = (int)s.Pop() + s.LocalBias;
                        if (s.LocalSubrs != null && idx >= 0 && idx < s.LocalSubrs.Length)
                            Execute(s.LocalSubrs[idx], s);
                        break;
                    }

                    case 11: // return
                        return; // return from current Execute() call

                    case 14: // endchar
                        ExtractWidthIfFirst(s, 1);
                        if (s.InPath) { s.Painter.ClosePath(); s.InPath = false; }
                        return;

                    case 15: // (unused in Type2) — ignore
                    case 16: // blend — variable fonts; ignore for now
                        s.ClearStack();
                        break;

                    case 19: // hintmask
                    case 20: // cntrmask
                        DoStem(s); // implicit stems from remaining stack
                        // Skip hint mask bytes
                        int maskBytes = (s.StemCount + 7) / 8;
                        pos += maskBytes;
                        if (pos > len) pos = len;
                        break;

                    case 21: // rmoveto
                    {
                        ExtractWidthIfFirst(s, 3);
                        double dy = s.Pop(), dx = s.Pop();
                        s.ClearStack();
                        DoMoveTo(s, dx, dy);
                        break;
                    }

                    case 22: // hmoveto
                    {
                        ExtractWidthIfFirst(s, 2);
                        double dx = s.Pop();
                        s.ClearStack();
                        DoMoveTo(s, dx, 0);
                        break;
                    }

                    case 24: // rcurveline
                        DoRCurveLine(s);
                        break;

                    case 25: // rlinecurve
                        DoRLineCurve(s);
                        break;

                    case 26: // vvcurveto
                        DoVVCurveTo(s);
                        break;

                    case 27: // hhcurveto
                        DoHHCurveTo(s);
                        break;

                    case 29: // callgsubr
                    {
                        int idx = (int)s.Pop() + s.GlobalBias;
                        if (s.GlobalSubrs != null && idx >= 0 && idx < s.GlobalSubrs.Length)
                            Execute(s.GlobalSubrs[idx], s);
                        break;
                    }

                    case 30: // vhcurveto
                        DoVHCurveTo(s);
                        break;

                    case 31: // hvcurveto
                        DoHVCurveTo(s);
                        break;

                    default:
                        // Unknown operator — clear stack and continue
                        s.ClearStack();
                        break;
                }
            }
        }

        // ── Stem helpers ──────────────────────────────────────────────────────

        private static void DoStem(InterpState s)
        {
            if (!s.FirstOpSeen && s.Top % 2 != 0)
            {
                // Odd stack count → first value is the width delta
                s.Width = s.NominalWidth + s.Stack[0];
                // Shift stack left by 1
                for (int i = 0; i + 1 < s.Top; i++) s.Stack[i] = s.Stack[i + 1];
                s.Top--;
                s.HasWidth = true;
            }
            s.StemCount += s.Top / 2;
            s.FirstOpSeen = true;
            s.ClearStack();
        }

        // ── MoveTo helpers ────────────────────────────────────────────────────

        private static void ExtractWidthIfFirst(InterpState s, int expectedArgs)
        {
            if (!s.FirstOpSeen && s.Top == expectedArgs)
            {
                s.Width = s.NominalWidth + s.Stack[0];
                for (int i = 0; i + 1 < s.Top; i++) s.Stack[i] = s.Stack[i + 1];
                s.Top--;
                s.HasWidth = true;
            }
            s.FirstOpSeen = true;
        }

        private static void DoMoveTo(InterpState s, double dx, double dy)
        {
            if (s.InPath) { s.Painter.ClosePath(); s.InPath = false; }
            s.CX += dx;
            s.CY += dy;
            s.SX = s.CX;
            s.SY = s.CY;
            s.Painter.MoveTo(new MoveTo(s.CX, s.CY));
            s.InPath = true;
        }

        // ── Line operators ────────────────────────────────────────────────────

        private static void DoHLineTo(InterpState s)
        {
            // hlineto: dx {dy dx}*  OR  {dx dy}+
            // Alternating: first segment horizontal (dx), then vertical (dy), ...
            // If odd count: first is horizontal; if even: first pair h/v
            int i = 0, count = s.Top;
            bool isH = true;
            while (i < count)
            {
                if (isH) s.CX += s.Stack[i];
                else     s.CY += s.Stack[i];
                s.Painter.LineTo(new LineTo(s.CX, s.CY));
                i++;
                isH = !isH;
            }
            s.ClearStack();
        }

        private static void DoVLineTo(InterpState s)
        {
            // vlineto: dy {dx dy}*  — first segment vertical
            int i = 0, count = s.Top;
            bool isV = true;
            while (i < count)
            {
                if (isV) s.CY += s.Stack[i];
                else     s.CX += s.Stack[i];
                s.Painter.LineTo(new LineTo(s.CX, s.CY));
                i++;
                isV = !isV;
            }
            s.ClearStack();
        }

        // ── Curve operators ───────────────────────────────────────────────────

        private static void DoRRCurveTo(InterpState s)
        {
            int i = 0;
            while (i + 5 < s.Top)
            {
                double x1 = s.CX + s.Stack[i];   double y1 = s.CY + s.Stack[i + 1];
                double x2 = x1  + s.Stack[i + 2]; double y2 = y1  + s.Stack[i + 3];
                double x3 = x2  + s.Stack[i + 4]; double y3 = y2  + s.Stack[i + 5];
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                s.CX = x3; s.CY = y3;
                i += 6;
            }
            s.ClearStack();
        }

        private static void DoHHCurveTo(InterpState s)
        {
            // |- dy1? {dxa dxb dyb dxc}+  hhcurveto |-
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;

            if (count % 2 != 0)
            {
                // Odd: first arg is dy1
                double dy1 = s.Stack[i];
                double x1 = curX + s.Stack[i + 1]; double y1 = curY + dy1;
                double x2 = x1   + s.Stack[i + 2]; double y2 = y1   + s.Stack[i + 3];
                double x3 = x2   + s.Stack[i + 4]; double y3 = y2;
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                curX = x3; curY = y3;
                i += 5; count -= 5;
            }

            while (count >= 4)
            {
                double x1 = curX + s.Stack[i];     double y1 = curY;
                double x2 = x1   + s.Stack[i + 1]; double y2 = y1 + s.Stack[i + 2];
                double x3 = x2   + s.Stack[i + 3]; double y3 = y2;
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                curX = x3; curY = y3;
                i += 4; count -= 4;
            }

            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        private static void DoVVCurveTo(InterpState s)
        {
            // |- dx1? {dya dxb dyb dyc}+  vvcurveto |-
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;

            if (count % 2 != 0)
            {
                // Odd: first arg is dx1
                double dx1 = s.Stack[i];
                double x1 = curX + dx1;            double y1 = curY + s.Stack[i + 1];
                double x2 = x1   + s.Stack[i + 2]; double y2 = y1   + s.Stack[i + 3];
                double x3 = x2;                    double y3 = y2   + s.Stack[i + 4];
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                curX = x3; curY = y3;
                i += 5; count -= 5;
            }

            while (count >= 4)
            {
                double x1 = curX;                  double y1 = curY + s.Stack[i];
                double x2 = x1   + s.Stack[i + 1]; double y2 = y1   + s.Stack[i + 2];
                double x3 = x2;                    double y3 = y2   + s.Stack[i + 3];
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                curX = x3; curY = y3;
                i += 4; count -= 4;
            }

            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        private static void DoHVCurveTo(InterpState s)
        {
            // |- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf?  (rem 0/1)
            // |- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf?  (rem 4/5)
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;
            int rem = count % 8;

            if (rem == 4 || rem == 5)
            {
                // First curve: h → v  (dx1 dx2 dy2 dy3)
                if (count == 5)
                {
                    double x1 = curX + s.Stack[0]; double y1 = curY;
                    double x2 = x1   + s.Stack[1]; double y2 = y1 + s.Stack[2];
                    double x3 = x2   + s.Stack[4]; double y3 = y2 + s.Stack[3];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 5; count -= 5;
                }
                else
                {
                    double x1 = curX + s.Stack[0]; double y1 = curY;
                    double x2 = x1   + s.Stack[1]; double y2 = y1 + s.Stack[2];
                    double x3 = x2;                double y3 = y2 + s.Stack[3];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 4; count -= 4;
                }
                // Remaining curves alternate v→h then h→v
                while (count >= 8)
                {
                    // v → h
                    double x1 = curX;                   double y1 = curY + s.Stack[i];
                    double x2 = x1   + s.Stack[i + 1];  double y2 = y1   + s.Stack[i + 2];
                    double x3 = x2   + s.Stack[i + 3];  double y3 = y2;
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3;
                    // h → v
                    if (count == 9)
                    {
                        x1 = curX + s.Stack[i + 4]; y1 = curY;
                        x2 = x1   + s.Stack[i + 5]; y2 = y1 + s.Stack[i + 6];
                        x3 = x2   + s.Stack[i + 8]; y3 = y2 + s.Stack[i + 7];
                    }
                    else
                    {
                        x1 = curX + s.Stack[i + 4]; y1 = curY;
                        x2 = x1   + s.Stack[i + 5]; y2 = y1 + s.Stack[i + 6];
                        x3 = x2;                    y3 = y2 + s.Stack[i + 7];
                    }
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 8; count -= 8;
                }
            }
            else // rem == 0 or 1
            {
                while (count >= 8)
                {
                    // h → v
                    double x1 = curX + s.Stack[i];      double y1 = curY;
                    double x2 = x1   + s.Stack[i + 1];  double y2 = y1   + s.Stack[i + 2];
                    double x3 = x2;                     double y3 = y2   + s.Stack[i + 3];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3;
                    // v → h
                    if (count == 9)
                    {
                        x1 = curX;                   y1 = curY + s.Stack[i + 4];
                        x2 = x1 + s.Stack[i + 5];   y2 = y1   + s.Stack[i + 6];
                        x3 = x2 + s.Stack[i + 7];   y3 = y2   + s.Stack[i + 8];
                    }
                    else
                    {
                        x1 = curX;                   y1 = curY + s.Stack[i + 4];
                        x2 = x1 + s.Stack[i + 5];   y2 = y1   + s.Stack[i + 6];
                        x3 = x2 + s.Stack[i + 7];   y3 = y2;
                    }
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 8; count -= 8;
                }
            }

            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        private static void DoVHCurveTo(InterpState s)
        {
            // |- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf?  (rem 0/1)
            // |- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf?  (rem 4/5)
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;
            int rem = count % 8;

            if (rem == 4 || rem == 5)
            {
                // First curve: v → h  (dy1 dx2 dy2 dx3)
                if (count == 5)
                {
                    double x1 = curX;              double y1 = curY + s.Stack[0];
                    double x2 = x1 + s.Stack[1];   double y2 = y1   + s.Stack[2];
                    double x3 = x2 + s.Stack[3];   double y3 = y2   + s.Stack[4];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 5; count -= 5;
                }
                else
                {
                    double x1 = curX;              double y1 = curY + s.Stack[0];
                    double x2 = x1 + s.Stack[1];   double y2 = y1   + s.Stack[2];
                    double x3 = x2 + s.Stack[3];   double y3 = y2;
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 4; count -= 4;
                }
                // Remaining alternate h→v then v→h
                while (count >= 8)
                {
                    // h → v
                    double x1 = curX + s.Stack[i];      double y1 = curY;
                    double x2 = x1   + s.Stack[i + 1];  double y2 = y1   + s.Stack[i + 2];
                    double x3 = x2;                     double y3 = y2   + s.Stack[i + 3];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3;
                    // v → h
                    if (count == 9)
                    {
                        x1 = curX;                   y1 = curY + s.Stack[i + 4];
                        x2 = x1 + s.Stack[i + 5];   y2 = y1   + s.Stack[i + 6];
                        x3 = x2 + s.Stack[i + 7];   y3 = y2   + s.Stack[i + 8];
                    }
                    else
                    {
                        x1 = curX;                   y1 = curY + s.Stack[i + 4];
                        x2 = x1 + s.Stack[i + 5];   y2 = y1   + s.Stack[i + 6];
                        x3 = x2 + s.Stack[i + 7];   y3 = y2;
                    }
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 8; count -= 8;
                }
            }
            else // rem == 0 or 1
            {
                while (count >= 8)
                {
                    // v → h
                    double x1 = curX;                    double y1 = curY + s.Stack[i];
                    double x2 = x1   + s.Stack[i + 1];   double y2 = y1   + s.Stack[i + 2];
                    double x3 = x2   + s.Stack[i + 3];   double y3 = y2;
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3;
                    // h → v
                    if (count == 9)
                    {
                        x1 = curX + s.Stack[i + 4]; y1 = curY;
                        x2 = x1   + s.Stack[i + 5]; y2 = y1 + s.Stack[i + 6];
                        x3 = x2   + s.Stack[i + 8]; y3 = y2 + s.Stack[i + 7];
                    }
                    else
                    {
                        x1 = curX + s.Stack[i + 4]; y1 = curY;
                        x2 = x1   + s.Stack[i + 5]; y2 = y1 + s.Stack[i + 6];
                        x3 = x2;                    y3 = y2 + s.Stack[i + 7];
                    }
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 8; count -= 8;
                }
            }

            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        private static void DoRCurveLine(InterpState s)
        {
            // |- {dxa dya dxb dyb dxc dyc}+ dxd dyd rcurveline |-
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;
            while (count > 2)
            {
                if (count == 8) // last group is the lineto
                {
                    double x1 = curX + s.Stack[i];     double y1 = curY + s.Stack[i + 1];
                    double x2 = x1   + s.Stack[i + 2]; double y2 = y1   + s.Stack[i + 3];
                    double x3 = x2   + s.Stack[i + 4]; double y3 = y2   + s.Stack[i + 5];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 6; count -= 6;
                    break;
                }
                if (count < 8)
                {
                    // Remaining 6 are the last curve + the final 2 are the line
                    double x1 = curX + s.Stack[i];     double y1 = curY + s.Stack[i + 1];
                    double x2 = x1   + s.Stack[i + 2]; double y2 = y1   + s.Stack[i + 3];
                    double x3 = x2   + s.Stack[i + 4]; double y3 = y2   + s.Stack[i + 5];
                    s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                    curX = x3; curY = y3; i += 6; count -= 6;
                    break;
                }
                double ax1 = curX + s.Stack[i];       double ay1 = curY + s.Stack[i + 1];
                double ax2 = ax1  + s.Stack[i + 2];   double ay2 = ay1  + s.Stack[i + 3];
                double ax3 = ax2  + s.Stack[i + 4];   double ay3 = ay2  + s.Stack[i + 5];
                s.Painter.CurveTo(new CurveTo(ax1, ay1, ax2, ay2, ax3, ay3));
                curX = ax3; curY = ay3; i += 6; count -= 6;
            }
            // final lineto
            if (i + 1 < s.Top)
            {
                curX += s.Stack[i]; curY += s.Stack[i + 1];
                s.Painter.LineTo(new LineTo(curX, curY));
            }
            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        private static void DoRLineCurve(InterpState s)
        {
            // |- {dxa dya}+ dxb dyb dxc dyc dxd dyd rlinecurve |-
            int i = 0, count = s.Top;
            double curX = s.CX, curY = s.CY;
            while (count > 6)
            {
                curX += s.Stack[i]; curY += s.Stack[i + 1];
                s.Painter.LineTo(new LineTo(curX, curY));
                i += 2; count -= 2;
            }
            // Final rrcurveto
            if (i + 5 < s.Top)
            {
                double x1 = curX + s.Stack[i];     double y1 = curY + s.Stack[i + 1];
                double x2 = x1   + s.Stack[i + 2]; double y2 = y1   + s.Stack[i + 3];
                double x3 = x2   + s.Stack[i + 4]; double y3 = y2   + s.Stack[i + 5];
                s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                curX = x3; curY = y3;
            }
            s.CX = curX; s.CY = curY;
            s.ClearStack();
        }

        // ── Flex operators ────────────────────────────────────────────────────

        private static void ExecFlex(InterpState s)
        {
            // dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd
            // Two rrcurveto segments; fd (flex depth) ignored.
            if (s.Top < 13) { s.ClearStack(); return; }
            double curX = s.CX, curY = s.CY;
            double x1 = curX + s.Stack[0]; double y1 = curY + s.Stack[1];
            double x2 = x1   + s.Stack[2]; double y2 = y1   + s.Stack[3];
            double x3 = x2   + s.Stack[4]; double y3 = y2   + s.Stack[5];
            s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
            double x4 = x3   + s.Stack[6]; double y4 = y3   + s.Stack[7];
            double x5 = x4   + s.Stack[8]; double y5 = y4   + s.Stack[9];
            double x6 = x5   + s.Stack[10]; double y6 = y5  + s.Stack[11];
            s.Painter.CurveTo(new CurveTo(x4, y4, x5, y5, x6, y6));
            s.CX = x6; s.CY = y6;
            s.ClearStack();
        }

        private static void ExecHFlex(InterpState s)
        {
            // dx1 dx2 dy2 dx3 dx4 dx5 dx6
            // All y-deltas for first/last control and endpoint are zero or implied.
            if (s.Top < 7) { s.ClearStack(); return; }
            double curX = s.CX, curY = s.CY;
            double x1 = curX + s.Stack[0]; double y1 = curY;
            double x2 = x1   + s.Stack[1]; double y2 = y1   + s.Stack[2];
            double x3 = x2   + s.Stack[3]; double y3 = y2;
            s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
            double x4 = x3   + s.Stack[4]; double y4 = y3;
            double x5 = x4   + s.Stack[5]; double y5 = curY; // same y as start
            double x6 = x5   + s.Stack[6]; double y6 = curY;
            s.Painter.CurveTo(new CurveTo(x4, y4, x5, y5, x6, y6));
            s.CX = x6; s.CY = y6;
            s.ClearStack();
        }

        private static void ExecHFlex1(InterpState s)
        {
            // dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6
            if (s.Top < 9) { s.ClearStack(); return; }
            double curX = s.CX, curY = s.CY;
            double x1 = curX + s.Stack[0]; double y1 = curY + s.Stack[1];
            double x2 = x1   + s.Stack[2]; double y2 = y1   + s.Stack[3];
            double x3 = x2   + s.Stack[4]; double y3 = y2;
            s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
            double x4 = x3   + s.Stack[5]; double y4 = y3;
            double x5 = x4   + s.Stack[6]; double y5 = y4   + s.Stack[7];
            double x6 = x5   + s.Stack[8]; double y6 = curY;
            s.Painter.CurveTo(new CurveTo(x4, y4, x5, y5, x6, y6));
            s.CX = x6; s.CY = y6;
            s.ClearStack();
        }

        private static void ExecFlex1(InterpState s)
        {
            // dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 d6
            // d6 is dx or dy depending on which direction moved more overall.
            if (s.Top < 11) { s.ClearStack(); return; }
            double curX = s.CX, curY = s.CY;
            double x1 = curX + s.Stack[0]; double y1 = curY + s.Stack[1];
            double x2 = x1   + s.Stack[2]; double y2 = y1   + s.Stack[3];
            double x3 = x2   + s.Stack[4]; double y3 = y2   + s.Stack[5];
            s.Painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
            double x4 = x3   + s.Stack[6]; double y4 = y3   + s.Stack[7];
            double x5 = x4   + s.Stack[8]; double y5 = y4   + s.Stack[9];
            // Determine d6 direction
            double dx = x5 - curX, dy = y5 - curY;
            double x6, y6;
            if (Math.Abs(dx) > Math.Abs(dy))
            { x6 = x5 + s.Stack[10]; y6 = curY; }
            else
            { x6 = curX; y6 = y5 + s.Stack[10]; }
            s.Painter.CurveTo(new CurveTo(x4, y4, x5, y5, x6, y6));
            s.CX = x6; s.CY = y6;
            s.ClearStack();
        }

        // ── Interpreter state ─────────────────────────────────────────────────

        private sealed class InterpState
        {
            public readonly IGlyphOutlinePainter Painter;
            public readonly double[] Stack  = new double[48];
            public readonly double[] Storage = new double[32];
            public int  Top;           // stack top index (Stack[0..Top-1] are valid)
            public double CX, CY;      // current point
            public double SX, SY;      // start of current subpath (for ClosePath)
            public int    StemCount;
            public bool   FirstOpSeen; // true after first stem/moveto
            public bool   HasWidth;
            public double Width;
            public double NominalWidth;
            public bool   InPath;
            public readonly byte[][]? LocalSubrs;
            public readonly byte[][]? GlobalSubrs;
            public readonly int LocalBias;
            public readonly int GlobalBias;

            public InterpState(IGlyphOutlinePainter painter, CffPrivateDict priv, byte[][]? globalSubrs)
            {
                Painter      = painter;
                NominalWidth = priv.NominalWidthX;
                Width        = priv.DefaultWidthX;
                LocalSubrs   = priv.LocalSubrs;
                GlobalSubrs  = globalSubrs;
                LocalBias    = CalcBias(priv.LocalSubrs?.Length ?? 0);
                GlobalBias   = CalcBias(globalSubrs?.Length ?? 0);
            }

            public void Push(double v)
            {
                if (Top < Stack.Length) Stack[Top++] = v;
            }

            public double Pop()
            {
                if (Top == 0) return 0;
                return Stack[--Top];
            }

            public double Peek()              => Top > 0 ? Stack[Top - 1] : 0;
            public double PeekAt(int fromTop) => (Top - 1 - fromTop) >= 0 ? Stack[Top - 1 - fromTop] : 0;

            public void ClearStack() => Top = 0;

            public void Roll(int n, int j)
            {
                if (n <= 0 || Top < n) return;
                int start = Top - n;
                j = ((j % n) + n) % n; // normalize
                Reverse(start, Top - 1);
                Reverse(start, start + j - 1);
                Reverse(start + j, Top - 1);
            }

            private void Reverse(int lo, int hi)
            {
                while (lo < hi) { double tmp = Stack[lo]; Stack[lo] = Stack[hi]; Stack[hi] = tmp; lo++; hi--; }
            }
        }
    }
}
