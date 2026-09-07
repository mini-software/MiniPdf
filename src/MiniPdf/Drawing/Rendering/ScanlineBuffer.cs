using System.Collections.Generic;

namespace MiniSoftware.Drawing.Rendering
{
    internal sealed class ScanlineBuffer
    {
        private readonly List<(int X0, int X1)> _spans = new List<(int X0, int X1)>();

        public void Clear() => _spans.Clear();

        public void AddSpan(int x0, int x1)
        {
            if (x1 < x0)
            {
                int t = x0;
                x0 = x1;
                x1 = t;
            }

            _spans.Add((x0, x1));
        }

        public IReadOnlyList<(int X0, int X1)> Spans => _spans;
    }
}
