namespace MiniPdf.Drawing.Rendering
{
    internal struct ActiveEdge
    {
        public int YMaxExclusive;
        public float CurrentX;
        public float DxPerY;
        public int Winding;
    }
}
