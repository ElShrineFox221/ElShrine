using ElShrine.EGraphic;
using System;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using Color = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.Wpf
{
    public static class MediaColorHelper
    {
        #region WPF

        #region Color tranfer
        public static Color ToDrawingColor(this MediaColor color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);
        public static MediaColor ToMediaColor(this Color color)
            => MediaColor.FromArgb(color.A, color.R, color.G, color.B);
        public static MediaColor ToMediaColor(this ColorData colorData)
        {
            var (A, R, G, B) = colorData.ARGB;
            var color = MediaColor.FromArgb(A, R, G, B);
            return color;
        }
        public static SolidColorBrush ToSolidBrush(this ColorData colorData)
        {
            var (A, R, G, B) = colorData.ARGB;
            var color = MediaColor.FromArgb(A, R, G, B);
            return new SolidColorBrush(color);
        }
        public static ColorData ToColorData(this MediaColor color)
            => (color.A, color.R, color.G, color.B).ToColorData();
        #endregion

        #region Console color
        public static MediaColor ToMediaColor(this ConsoleColor consoleColor)
            => consoleColor.ToDrawingColor().ToMediaColor();
        public static ConsoleColor ToConsoleColor(this MediaColor color)
            => color.ToDrawingColor().ToConsoleColor();
        #endregion

        public static int Difference(this MediaColor color0, MediaColor color1, bool alphaIncluded = false)
            => Math.Abs(color0.R - color1.R) + Math.Abs(color0.G - color1.G) + Math.Abs(color0.B + color1.B) + (alphaIncluded ? Math.Abs(color0.A - color1.A) : 0);
        private static byte ToOpposite(this byte value)
            => (byte)(byte.MaxValue - value);
        public static MediaColor ToOppositeColor(this MediaColor color)
            => MediaColor.FromRgb(color.R.ToOpposite(), color.G.ToOpposite(), color.B.ToOpposite());

        private static byte Shift(this byte value, byte offset)
            => (byte)((value + offset) % 256);
        public static MediaColor ToContractGreyColor(this MediaColor color, double rate = 1d, ColorContractMode mode = ColorContractMode.Auto)
        {
            byte grey = color.ToGray();
            byte contractGrey = 0;

            switch (mode)
            {
                case ColorContractMode.Auto:
                    if (grey >= 128) contractGrey = contractGrey.Shift((byte)(128 * rate));
                    else contractGrey = contractGrey.Shift((byte)(-128 * rate));
                    break;
                case ColorContractMode.Light:
                    contractGrey = contractGrey.Shift((byte)((byte.MaxValue - grey) * rate));
                    break;
                case ColorContractMode.Dark:
                    contractGrey = contractGrey.Shift((byte)(-grey * rate));
                    break;
            }
            MediaColor result = MediaColor.FromArgb(color.A, contractGrey, contractGrey, contractGrey);
            return result;
        }
        #region Hex code
        public static MediaColor ToMediaColor(this string hexCode)
        {
            hexCode = hexCode.ToUpper();
            if (hexCode.Length < 6) hexCode = hexCode.PadLeft(6, 'F');
            if (hexCode.Length < 8) hexCode = hexCode.PadRight(8, 'F');
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) bytes[i] = byte.Parse(hexCode.Substring(2 * i, 2), NumberStyles.HexNumber);
            return MediaColor.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
        public static string ToHexARGB(this MediaColor color)
        {
            byte[] bytes = [color.A, color.R, color.G, color.B];
            string[] strs = [.. bytes.Select(x => x.ToString("X2"))];
            return $"{strs[0]}{strs[1]}{strs[2]}{strs[3]}";
        }
        #endregion

        #region HSV space transfer
        public static MediaColor FromAhsv(byte a, byte h, byte s, byte v)
        {
            (byte r, byte g, byte b) = ColorHelper.HSVToRGB(h, s, v);
            return MediaColor.FromArgb(a, r, g, b);
        }
        public static MediaColor FromAhsv(double a, double h, double s, double v)
            => FromAhsv(byte.MinValue * a, byte.MaxValue * h, byte.MinValue * s, byte.MaxValue * v);
        public static MediaColor FromHSV(byte h, byte s, byte v) => FromAhsv(byte.MaxValue, h, s, v);
        public static MediaColor FromHSV(double h, double s, double v)
            => FromAhsv(1d, h, s, v);
        #endregion

        public static byte ToGray(this MediaColor color)
            => (byte)(color.R * 0.299d + color.G * 0.587d + color.B * 0.114d);
        public static MediaColor ToGrayColor(this MediaColor color)
        {
            byte grey = color.ToGray();
            return MediaColor.FromArgb(255, grey, grey, grey);
        }
        #endregion
    }
}
