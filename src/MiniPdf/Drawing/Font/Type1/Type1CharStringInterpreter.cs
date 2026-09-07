using System.Collections.Generic;
using MiniPdf.Drawing.Font.Rendering;

namespace MiniPdf.Drawing.Font.Type1
{
    /// <summary>
    /// Executes a decrypted Type 1 charstring, emitting drawing commands to an
    /// <see cref="IGlyphOutlinePainter"/>. Returns the advance width of the glyph.
    /// </summary>
    /// <remarks>
    /// Operators implemented: hsbw, sbw, vmoveto, hmoveto, rmoveto,
    /// rlineto, hlineto, vlineto, rrcurveto, vhcurveto, hvcurveto, closepath, endchar,
    /// hstem, vstem, hstem3, vstem3 (ignored for rendering), seac,
    /// callsubr, return, div, dotsection, callothersubr, pop, setcurrentpoint.
    /// </remarks>
    public sealed class Type1CharStringInterpreter
    {
        // ── Entry point ───────────────────────────────────────────────────────

        /// <summary>
        /// Executes <paramref name="charstring"/> and delivers outline commands
        /// to <paramref name="painter"/>.
        /// </summary>
        /// <param name="charstring">Decrypted Type 1 charstring bytes.</param>
        /// <param name="painter">Receives outline drawing commands.</param>
        /// <param name="subrs">The font's /Subrs array (decrypted), or null.</param>
        /// <param name="charStrings">All font charstrings keyed by glyph name
        /// (needed to implement seac). May be null if seac is not expected.</param>
        /// <returns>The glyph advance width in font units.</returns>
        public double Interpret(
            byte[]                       charstring,
            IGlyphOutlinePainter         painter,
            byte[][]?                    subrs,
            Dictionary<string, byte[]>?  charStrings)
        {
            if (charstring is null || charstring.Length == 0) return 0.0;

            var state = new InterpState(painter, subrs, charStrings);
            Execute(charstring, state, 0);
            return state.AdvanceWidth;
        }

        // ── Core execution ────────────────────────────────────────────────────

        private static void Execute(byte[] data, InterpState s, int depth)
        {
            if (depth > 10) return; // guard against runaway recursion

            int pos = 0, len = data.Length;

            while (pos < len)
            {
                byte b = data[pos++];

                // ── Number encoding ───────────────────────────────────────────
                if (b >= 32 && b <= 246)
                {
                    s.Push(b - 139);
                    continue;
                }
                if (b >= 247 && b <= 250)
                {
                    if (pos >= len) return;
                    s.Push((b - 247) * 256.0 + data[pos++] + 108);
                    continue;
                }
                if (b >= 251 && b <= 254)
                {
                    if (pos >= len) return;
                    s.Push(-((b - 251) * 256.0 + data[pos++] + 108));
                    continue;
                }
                if (b == 255) // 4-byte signed integer
                {
                    if (pos + 3 >= len) return;
                    int v = (data[pos] << 24) | (data[pos + 1] << 16)
                          | (data[pos + 2] << 8) | data[pos + 3];
                    pos += 4;
                    s.Push(v);
                    continue;
                }

                // ── Two-byte escape (12) ──────────────────────────────────────
                if (b == 12)
                {
                    if (pos >= len) return;
                    byte b2 = data[pos++];
                    ExecuteEscape(b2, s, depth);
                    continue;
                }

                // ── One-byte operators ────────────────────────────────────────
                switch (b)
                {
                    case 1:  // hstem – horizontal stem hint, ignore for rendering
                    case 3:  // vstem – vertical stem hint, ignore for rendering
                        s.ClearStack();
                        break;

                    case 4: // vmoveto dy
                    {
                        double dy = s.PopOrZero();
                        s.MoveBy(0, dy);
                        break;
                    }

                    case 5: // rlineto dx dy
                    {
                        double dy = s.PopOrZero();
                        double dx = s.PopOrZero();
                        s.LineBy(dx, dy);
                        break;
                    }

                    case 6: // hlineto dx
                    {
                        double dx = s.PopOrZero();
                        s.LineBy(dx, 0);
                        break;
                    }

                    case 7: // vlineto dy
                    {
                        double dy = s.PopOrZero();
                        s.LineBy(0, dy);
                        break;
                    }

                    case 8: // rrcurveto dx1 dy1 dx2 dy2 dx3 dy3
                    {
                        double dy3 = s.PopOrZero(), dx3 = s.PopOrZero();
                        double dy2 = s.PopOrZero(), dx2 = s.PopOrZero();
                        double dy1 = s.PopOrZero(), dx1 = s.PopOrZero();
                        s.CurveBy(dx1, dy1, dx2, dy2, dx3, dy3);
                        break;
                    }

                    case 9: // closepath
                        s.ClosePath();
                        break;

                    case 10: // callsubr index
                    {
                        int idx = (int)s.PopOrZero();
                        if (s.Subrs != null && idx >= 0 && idx < s.Subrs.Length
                            && s.Subrs[idx] != null)
                        {
                            Execute(s.Subrs[idx]!, s, depth + 1);
                        }
                        break;
                    }

                    case 11: // return (from subr)
                        return;

                    case 13: // hsbw sbx wx
                    {
                        double wx  = s.PopOrZero();
                        double sbx = s.PopOrZero();
                        s.SetOriginAndWidth(sbx, 0.0, wx);
                        break;
                    }

                    case 14: // endchar
                        s.ClosePath();
                        return;

                    case 21: // rmoveto dx dy  (later Type 1 revision)
                    {
                        double dy = s.PopOrZero();
                        double dx = s.PopOrZero();
                        s.MoveBy(dx, dy);
                        break;
                    }

                    case 22: // hmoveto dx
                    {
                        double dx = s.PopOrZero();
                        s.MoveBy(dx, 0);
                        break;
                    }

                    case 30: // vhcurveto dy1 dx2 dy2 dx3
                    {
                        double dx3 = s.PopOrZero(), dy2 = s.PopOrZero();
                        double dx2 = s.PopOrZero(), dy1 = s.PopOrZero();
                        s.CurveBy(0, dy1, dx2, dy2, dx3, 0);
                        break;
                    }

                    case 31: // hvcurveto dx1 dx2 dy2 dy3
                    {
                        double dy3 = s.PopOrZero(), dy2 = s.PopOrZero();
                        double dx2 = s.PopOrZero(), dx1 = s.PopOrZero();
                        s.CurveBy(dx1, 0, dx2, dy2, 0, dy3);
                        break;
                    }

                    // Anything else: skip (unknown/undefined operators)
                    default:
                        s.ClearStack();
                        break;
                }
            }
        }

        // ── Escape operator dispatch ──────────────────────────────────────────

        private static void ExecuteEscape(byte op, InterpState s, int depth)
        {
            switch (op)
            {
                case 0: // dotsection – rendering hint, ignore
                    break;

                case 1: // vstem3 – 3 vertical stems, ignore
                case 2: // hstem3 – 3 horizontal stems, ignore
                    s.ClearStack();
                    break;

                case 6: // seac asb adx ady bchar achar
                {
                    // Standard Encoding Accented Character
                    // Operand order on stack (bottom→top): asb adx ady bchar achar
                    int  achar = (int)s.PopOrZero();
                    int  bchar = (int)s.PopOrZero();
                    double ady = s.PopOrZero();
                    double adx = s.PopOrZero();
                    // asb = sidebearing of accent (ignored here)
                    s.PopOrZero();

                    if (s.CharStrings != null)
                    {
                        string? baseName   = GetStdEncName(bchar);
                        string? accentName = GetStdEncName(achar);

                        if (baseName != null && s.CharStrings.TryGetValue(baseName, out byte[]? baseCs))
                            Execute(baseCs, s, depth + 1);

                        if (accentName != null && s.CharStrings.TryGetValue(accentName, out byte[]? accCs))
                        {
                            // Render accent displaced by (adx, ady) from glyph origin
                            double savedX = s.CX, savedY = s.CY;
                            s.CX = adx;
                            s.CY = ady;
                            Execute(accCs, s, depth + 1);
                            s.CX = savedX;
                            s.CY = savedY;
                        }
                    }
                    break;
                }

                case 7: // sbw sbx sby wx wy
                {
                    double wy  = s.PopOrZero(), wx  = s.PopOrZero();
                    double sby = s.PopOrZero(), sbx = s.PopOrZero();
                    s.SetOriginAndWidth(sbx, sby, wx);
                    break;
                }

                case 12: // div num den → num/den
                {
                    double den = s.PopOrZero();
                    double num = s.PopOrZero();
                    s.Push(den != 0.0 ? num / den : 0.0);
                    break;
                }

                case 16: // callothersubr proc_num count args...
                {
                    // OtherSubrs are PostScript procedures; we cannot execute them,
                    // but we must consume the arguments.
                    int procNum = (int)s.PopOrZero();
                    int count   = (int)s.PopOrZero();
                    // Push the right number of dummy results for subsequent "pop" ops
                    s.OtherSubrResultCount = count;
                    for (int i = 0; i < count; i++) s.Pop(); // consume args
                    break;
                }

                case 17: // pop – retrieve result from OtherSubr
                    // Push 0 as a placeholder for any OtherSubr result
                    if (s.OtherSubrResultCount > 0)
                    {
                        s.OtherSubrResultCount--;
                        s.Push(0);
                    }
                    break;

                case 33: // setcurrentpoint x y
                {
                    double y = s.PopOrZero(), x = s.PopOrZero();
                    s.CX = x;
                    s.CY = y;
                    break;
                }

                default:
                    s.ClearStack();
                    break;
            }
        }

        // ── Standard encoding name lookup ─────────────────────────────────────

        private static string? GetStdEncName(int charCode)
        {
            if (charCode < 0 || charCode >= 256) return null;
            string[] std = Type1Parser.GetStandardEncoding();
            string name = std[charCode];
            return (name == ".notdef") ? null : name;
        }

        // ── Interpreter state ─────────────────────────────────────────────────

        private sealed class InterpState
        {
            private readonly IGlyphOutlinePainter     _painter;
            private readonly Stack<double>            _stack = new Stack<double>();
            private bool                              _pathOpen;

            internal byte[][]?                        Subrs;
            internal Dictionary<string, byte[]>?      CharStrings;

            internal double CX;
            internal double CY;
            internal double AdvanceWidth;
            internal int    OtherSubrResultCount;

            internal InterpState(
                IGlyphOutlinePainter        painter,
                byte[][]?                   subrs,
                Dictionary<string, byte[]>? charStrings)
            {
                _painter    = painter;
                Subrs       = subrs;
                CharStrings = charStrings;
            }

            // ── Stack helpers ─────────────────────────────────────────────────

            internal void Push(double v) => _stack.Push(v);

            internal double Pop()       => _stack.Count > 0 ? _stack.Pop() : 0.0;
            internal double PopOrZero() => _stack.Count > 0 ? _stack.Pop() : 0.0;

            internal void ClearStack() => _stack.Clear();

            // ── Movement / drawing ────────────────────────────────────────────

            internal void SetOriginAndWidth(double sbx, double sby, double wx)
            {
                CX            = sbx;
                CY            = sby;
                AdvanceWidth  = wx;
            }

            internal void MoveBy(double dx, double dy)
            {
                if (_pathOpen) { _painter.ClosePath(); _pathOpen = false; }
                CX += dx;
                CY += dy;
                _painter.MoveTo(new MoveTo(CX, CY));
                _pathOpen = true;
            }

            internal void LineBy(double dx, double dy)
            {
                CX += dx;
                CY += dy;
                _painter.LineTo(new LineTo(CX, CY));
            }

            internal void CurveBy(
                double dx1, double dy1,
                double dx2, double dy2,
                double dx3, double dy3)
            {
                double x1 = CX + dx1, y1 = CY + dy1;
                double x2 = x1 + dx2, y2 = y1 + dy2;
                double x3 = x2 + dx3, y3 = y2 + dy3;
                _painter.CurveTo(new CurveTo(x1, y1, x2, y2, x3, y3));
                CX = x3;
                CY = y3;
            }

            internal void ClosePath()
            {
                if (_pathOpen)
                {
                    _painter.ClosePath();
                    _pathOpen = false;
                }
            }
        }
    }
}
