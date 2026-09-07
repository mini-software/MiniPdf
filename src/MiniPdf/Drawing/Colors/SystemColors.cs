namespace MiniPdf.Drawing.Colors
{
    /// <summary>
    /// Provides system-defined colors, mirroring System.Drawing.SystemColors.
    /// Values are static defaults; they do not track the live OS theme.
    /// </summary>
    public static class SystemColors
    {
        /// <summary>
        /// Gets the color of the active window border.
        /// </summary>
        public static Color ActiveBorder          => Color.FromKnownColor(KnownColor.ActiveBorder);

        /// <summary>
        /// Gets the color of the active window title bar.
        /// </summary>
        public static Color ActiveCaption         => Color.FromKnownColor(KnownColor.ActiveCaption);

        /// <summary>
        /// Gets the color of the text in the active window title bar.
        /// </summary>
        public static Color ActiveCaptionText     => Color.FromKnownColor(KnownColor.ActiveCaptionText);

        /// <summary>
        /// Gets the color of the application workspace.
        /// </summary>
        public static Color AppWorkspace          => Color.FromKnownColor(KnownColor.AppWorkspace);

        /// <summary>
        /// Gets the color of the face of a 3-D element.
        /// </summary>
        public static Color ButtonFace            => Color.FromKnownColor(KnownColor.ButtonFace);

        /// <summary>
        /// Gets the color of the highlight of a 3-D element.
        /// </summary>
        public static Color ButtonHighlight       => Color.FromKnownColor(KnownColor.ButtonHighlight);

        /// <summary>
        /// Gets the color of the shadow of a 3-D element.
        /// </summary>
        public static Color ButtonShadow          => Color.FromKnownColor(KnownColor.ButtonShadow);

        /// <summary>
        /// Gets the color of the control background.
        /// </summary>
        public static Color Control               => Color.FromKnownColor(KnownColor.Control);

        /// <summary>
        /// Gets the color of the shadow of a 3-D control.
        /// </summary>
        public static Color ControlDark           => Color.FromKnownColor(KnownColor.ControlDark);

        /// <summary>
        /// Gets the color of the dark shadow of a 3-D control.
        /// </summary>
        public static Color ControlDarkDark       => Color.FromKnownColor(KnownColor.ControlDarkDark);

        /// <summary>
        /// Gets the color of the light of a 3-D control.
        /// </summary>
        public static Color ControlLight          => Color.FromKnownColor(KnownColor.ControlLight);

        /// <summary>
        /// Gets the color of the highlight of a 3-D control.
        /// </summary>
        public static Color ControlLightLight     => Color.FromKnownColor(KnownColor.ControlLightLight);

        /// <summary>
        /// Gets the color of the text in a control.
        /// </summary>
        public static Color ControlText           => Color.FromKnownColor(KnownColor.ControlText);

        /// <summary>
        /// Gets the color of the desktop.
        /// </summary>
        public static Color Desktop               => Color.FromKnownColor(KnownColor.Desktop);

        /// <summary>
        /// Gets the color of the gradient in the active window title bar.
        /// </summary>
        public static Color GradientActiveCaption => Color.FromKnownColor(KnownColor.GradientActiveCaption);

        /// <summary>
        /// Gets the color of the gradient in the inactive window title bar.
        /// </summary>
        public static Color GradientInactiveCaption => Color.FromKnownColor(KnownColor.GradientInactiveCaption);

        /// <summary>
        /// Gets the color of the text that is dimmed.
        /// </summary>
        public static Color GrayText              => Color.FromKnownColor(KnownColor.GrayText);

        /// <summary>
        /// Gets the color of the highlighted item.
        /// </summary>
        public static Color Highlight             => Color.FromKnownColor(KnownColor.Highlight);

        /// <summary>
        /// Gets the color of the text in a highlighted item.
        /// </summary>
        public static Color HighlightText         => Color.FromKnownColor(KnownColor.HighlightText);

        /// <summary>
        /// Gets the color used to identify hot-tracked items.
        /// </summary>
        public static Color HotTrack              => Color.FromKnownColor(KnownColor.HotTrack);

        /// <summary>
        /// Gets the color of the inactive window border.
        /// </summary>
        public static Color InactiveBorder        => Color.FromKnownColor(KnownColor.InactiveBorder);

        /// <summary>
        /// Gets the color of the inactive window title bar.
        /// </summary>
        public static Color InactiveCaption       => Color.FromKnownColor(KnownColor.InactiveCaption);

        /// <summary>
        /// Gets the color of the text in the inactive window title bar.
        /// </summary>
        public static Color InactiveCaptionText   => Color.FromKnownColor(KnownColor.InactiveCaptionText);

        /// <summary>
        /// Gets the color of the tooltip background.
        /// </summary>
        public static Color Info                  => Color.FromKnownColor(KnownColor.Info);

        /// <summary>
        /// Gets the color of the tooltip text.
        /// </summary>
        public static Color InfoText              => Color.FromKnownColor(KnownColor.InfoText);

        /// <summary>
        /// Gets the color of the menu background.
        /// </summary>
        public static Color Menu                  => Color.FromKnownColor(KnownColor.Menu);

        /// <summary>
        /// Gets the color of the menu bar.
        /// </summary>
        public static Color MenuBar               => Color.FromKnownColor(KnownColor.MenuBar);

        /// <summary>
        /// Gets the color of the highlighted menu item.
        /// </summary>
        public static Color MenuHighlight         => Color.FromKnownColor(KnownColor.MenuHighlight);

        /// <summary>
        /// Gets the color of the text in a menu.
        /// </summary>
        public static Color MenuText              => Color.FromKnownColor(KnownColor.MenuText);

        /// <summary>
        /// Gets the color of the scroll bar.
        /// </summary>
        public static Color ScrollBar             => Color.FromKnownColor(KnownColor.ScrollBar);

        /// <summary>
        /// Gets the color of the window background.
        /// </summary>
        public static Color Window                => Color.FromKnownColor(KnownColor.Window);

        /// <summary>
        /// Gets the color of the window frame.
        /// </summary>
        public static Color WindowFrame           => Color.FromKnownColor(KnownColor.WindowFrame);

        /// <summary>
        /// Gets the color of the text in a window.
        /// </summary>
        public static Color WindowText            => Color.FromKnownColor(KnownColor.WindowText);
    }
}
