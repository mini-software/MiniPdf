using System.Threading;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Enums;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Provides a set of pre-defined system icons as stub implementations.
    /// Each icon is a 16×16 solid-color bitmap; the instances are cached after first access.
    /// </summary>
    public static class SystemIcons
    {
        /// <summary>
        /// Gets the application system icon.
        /// </summary>
        public static Icon Application => GetOrCreate(ref s_application, Color.DarkGray);

        /// <summary>
        /// Gets the error system icon.
        /// </summary>
        public static Icon Error       => GetOrCreate(ref s_error,       Color.Red);

        /// <summary>
        /// Gets the hand system icon.
        /// </summary>
        public static Icon Hand        => GetOrCreate(ref s_hand,        Color.DarkRed);

        /// <summary>
        /// Gets the exclamation system icon.
        /// </summary>
        public static Icon Exclamation => GetOrCreate(ref s_exclamation, Color.Gold);

        /// <summary>
        /// Gets the warning system icon.
        /// </summary>
        public static Icon Warning     => GetOrCreate(ref s_warning,     Color.Orange);

        /// <summary>
        /// Gets the information system icon.
        /// </summary>
        public static Icon Information => GetOrCreate(ref s_information, Color.DodgerBlue);

        /// <summary>
        /// Gets the asterisk system icon.
        /// </summary>
        public static Icon Asterisk    => GetOrCreate(ref s_asterisk,    Color.SteelBlue);

        /// <summary>
        /// Gets the question system icon.
        /// </summary>
        public static Icon Question    => GetOrCreate(ref s_question,    Color.RoyalBlue);

        /// <summary>
        /// Gets the Windows logo system icon.
        /// </summary>
        public static Icon WinLogo     => GetOrCreate(ref s_winLogo,     Color.CornflowerBlue);

        /// <summary>
        /// Gets the shield system icon.
        /// </summary>
        public static Icon Shield      => GetOrCreate(ref s_shield,      Color.SlateBlue);

        // ── Backing fields ───────────────────────────────────────────────────────

        private static Icon? s_application;
        private static Icon? s_error;
        private static Icon? s_hand;
        private static Icon? s_exclamation;
        private static Icon? s_warning;
        private static Icon? s_information;
        private static Icon? s_asterisk;
        private static Icon? s_question;
        private static Icon? s_winLogo;
        private static Icon? s_shield;

        // ── Helper ───────────────────────────────────────────────────────────────

        private static Icon GetOrCreate(ref Icon? field, Color fill)
        {
            if (field == null)
            {
                var icon = new Icon(MakeStub(fill));
                Interlocked.CompareExchange(ref field, icon, null);
            }
            return field!;
        }

        private static Bitmap MakeStub(Color fill)
        {
            const int Size = 16;
            var bmp = new Bitmap(Size, Size, PixelFormat.Format32bppArgb);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    bmp.SetPixel(x, y, fill);
            return bmp;
        }
    }
}
