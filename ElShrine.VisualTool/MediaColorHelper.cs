using ElShrine.EGraphic;
using System.Windows.Media;


namespace ElShrine.VisualTool
{
    public static partial class MediaColorHelper
    {
        #region WPF
        public static int Difference(this Color color0, Color color1, bool alphaIncluded = false)
            => Math.Abs(color0.R - color1.R) + Math.Abs(color0.G - color1.G) + Math.Abs(color0.B + color1.B) + (alphaIncluded ? Math.Abs(color0.A - color1.A) : 0);
        public static System.Drawing.Color ToDrawingColor(this Color color)
            => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        public static Color ToMediaColor(this System.Drawing.Color color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);
        public static Color ToMediaColor(this ConsoleColor consoleColor)
           => consoleColor.ToDrawingColor().ToMediaColor();
        public static ConsoleColor ToConsoleColor(this Color color)
            => color.ToDrawingColor().ToConsoleColor();
        #endregion
    }
}
