using ElShrine.Graphics;
using System.Windows.Media;
using Color = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.Wpf
{
    public static class ColorsExtensions
    {
        #region Color tranfer
        public static Color ToDrawingColor(this MediaColor color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);
        public static Color ToDrawingColor(this ColorData colorData)
        {
            var argbColorData = colorData.ToARGB();
            var color = Color.FromArgb(argbColorData.A, argbColorData.V1, argbColorData.V2, argbColorData.V3);
            return color;
        }
        public static MediaColor ToMediaColor(this Color color)
            => MediaColor.FromArgb(color.A, color.R, color.G, color.B);
        public static MediaColor ToMediaColor(this ColorData colorData)
        {
            var argbColorData = colorData.ToARGB();
            var color = MediaColor.FromArgb(argbColorData.A, argbColorData.V1, argbColorData.V2, argbColorData.V3);
            return color;
        }
        public static ColorData ToColorData(this MediaColor color)
            => ColorData.FromComp(color.A, color.R, color.G, color.B);
        public static ColorData ToColorData(this Color color)
            => ColorData.FromComp(color.A, color.R, color.G, color.B);
        #endregion

        #region SolidBrush
        public static SolidColorBrush ToSolidBrush(this ColorData colorData)
        {
            var color = colorData.ToMediaColor();
            var brush = new SolidColorBrush(color);
            return brush;
        }
        public static SolidColorBrush ToSolidBrush(this MediaColor color)
        {
            var brush = new SolidColorBrush(color);
            return brush;
        }
        public static SolidColorBrush ToSolidBrush(this Color color)
        {
            var mediaColor = color.ToMediaColor();
            var brush = new SolidColorBrush(mediaColor);
            return brush;
        }
        #endregion
    }
}
