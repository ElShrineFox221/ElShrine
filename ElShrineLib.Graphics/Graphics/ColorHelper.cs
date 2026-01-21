using System.Drawing;
using System.Globalization;
using System.Reflection;


namespace ElShrine.Graphics
{
    public static class ColorHelper
    {
        static ColorHelper()
        {
            InitializeColorInfos();
        }
        private static (string name, ConsoleColor consoleColor)[] consoleColorTable = [];
        private static (string name, Color color)[] namedDrawingColorTable = [];
        private static (ConsoleColor consoleColor, Color drawingColor)[] consoleColorDrawingColorTable = [];
        private const BindingFlags PulicStaticFlag = BindingFlags.Public | BindingFlags.Static;
        private readonly static Type drawingColorType = typeof(Color);
        private static void InitializeColorInfos()
        {
            var drawingColorPIS = drawingColorType.GetProperties(PulicStaticFlag).Where(pi => pi.PropertyType == drawingColorType);
            namedDrawingColorTable = [.. drawingColorPIS.Select(pi => (pi.Name, (Color)(pi.GetValue(null) ?? Color.White)))];
            var consoleColorFIS = typeof(ConsoleColor).GetFields(PulicStaticFlag).Where(fi => fi.FieldType == typeof(ConsoleColor));
            consoleColorTable = [.. consoleColorFIS.Select(fi => (fi.Name, (ConsoleColor)(fi.GetValue(null) ?? ConsoleColor.Red)))];

            consoleColorDrawingColorTable = [.. consoleColorTable.Select(ti => (ti.consoleColor, Array.Find(namedDrawingColorTable, ti1 => ti1.name == ti.name).color))];
        }

        #region ConsoleColor
        public static Color ToDrawingColor(this ConsoleColor consoleColor)
        {
            var matchedNames = consoleColorTable.Where((ti) => ti.consoleColor == consoleColor);
            string colorName = matchedNames.Any() ? matchedNames.First().name : "White";
            var matchedColors = namedDrawingColorTable.Where((ti) => ti.name == colorName);
            var color = matchedColors.Any() ? matchedColors.First().color : System.Drawing.Color.White;
            return color;
        }
        public static ConsoleColor ToConsoleColor(this Color color)
        {
            var differences = consoleColorDrawingColorTable.Select(ti => ti.drawingColor.Difference(color)).ToList();
            int min = differences.Min();
            int minIndex = differences.Find(num => num == min);
            var (consoleColor, drawingColor) = consoleColorDrawingColorTable[minIndex];
            return consoleColor;
        }
        #endregion

        public static int Difference(this Color color0, Color color1, bool alphaIncluded = false)
            => Math.Abs(color0.R - color1.R) + Math.Abs(color0.G - color1.G) + Math.Abs(color0.B + color1.B) + (alphaIncluded ? Math.Abs(color0.A - color1.A) : 0);
        public static Color Lerp(this Color from, Color to, double rate, bool enableAlpha)
        {
            var newColor = Color.FromArgb(enableAlpha ? from.A.Lerp(to.A, rate) : from.A, from.R.Lerp(to.R, rate), from.G.Lerp(to.G, rate), from.B.Lerp(to.B, rate));
            return newColor;
        }
        private static byte ToOpposite(this byte value)
            => (byte)(byte.MaxValue - value);
        public static Color ToOppositeColor(this Color color)
            => Color.FromArgb(color.R.ToOpposite(), color.G.ToOpposite(), color.B.ToOpposite());
        
        private static byte Shift(this byte value, byte offset)
            => (byte)((value + offset) % 256);
        public static Color ToContractGreyColor(this Color color, double rate = 1d, ColorContractMode mode = ColorContractMode.Auto)
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
            Color result = Color.FromArgb(color.A, contractGrey, contractGrey, contractGrey);
            return result;
        }

        #region hex code
        public static Color ToDrawingColor(this string hexCode)
        {
            hexCode = hexCode.ToUpper();
            if (hexCode.Length < 6) hexCode = hexCode.PadLeft(6, 'F');
            if (hexCode.Length < 8) hexCode = hexCode.PadRight(8, 'F');
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) bytes[i] = byte.Parse(hexCode.Substring(2 * i, 2), NumberStyles.HexNumber);
            return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
        public static string ToHexARGB(this Color color)
        {
            byte[] bytes = [color.A, color.R, color.G, color.B];
            string[] strs = [.. bytes.Select(x => x.ToString("X2"))];
            return $"{strs[0]}{strs[1]}{strs[2]}{strs[3]}";
        }
        #endregion

        #region HSV space transfer
        public static Color FromAhsv(byte a, byte h, byte s, byte v)
        {
            (byte r, byte g, byte b) = HSVToRGB(h, s, v);
            return Color.FromArgb(a, r, g, b);
        }
        public static Color FromAhsv(double a, double h, double s, double v)
            => FromAhsv(byte.MinValue * a, byte.MaxValue * h, byte.MinValue * s, byte.MaxValue * v);
        public static Color FromHSV(byte h, byte s, byte v) => FromAhsv(byte.MaxValue, h, s, v);
        public static Color FromHSV(double h, double s, double v)
            => FromAhsv(1d, h, s, v);
        
        private const double spectrumOffset = 1d / 6d;
        public static (byte? h, byte s, byte v) RGBToHSV(byte r, byte g, byte b)
        {
            (double? h, double s, double v) = RGBToHSV(r / (double)byte.MaxValue, g / (double)byte.MaxValue, b / (double)byte.MaxValue);
            return (h is null ? null : (byte.MaxValue * h.Value).ToByte(), (byte.MaxValue * s).ToByte(), (byte.MaxValue * v).ToByte());
        }
        public static (byte r, byte g, byte b) HSVToRGB(byte h, byte s, byte v)
        {
            (double r, double g, double b) = HSVToRGB(h / (double)byte.MaxValue, s / (double)byte.MaxValue, v / (double)byte.MaxValue);
            return ((byte.MaxValue * r).ToByte(), (byte.MaxValue * g).ToByte(), (byte.MaxValue * b).ToByte());
        }
        public static (double? h, double s, double v) RGBToHSV(double r, double g, double b)
        {
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double deltaSub = max - min;
            if (deltaSub == 0)
            {
                if(max == 1) return (null, 0, 1);
                else return (null, 1, 0);
            }
            double h, s, v;
            const double factor = 1d / 6d;
            if (max == r) h = (factor * ((g - b) / (max - min) + 1)).Shift(-factor, 1, 0);
            else if (max == g) h = (factor * ((b - r) / (max - min) + 3)).Shift(-factor, 1, 0);
            else h = (factor * ((r - g) / (max - min) + 5)).Shift(-factor, 1, 0);
            s = deltaSub / max;
            v = max;
            return (h, s, v);
        }
        public static (double r, double g, double b) HSVToRGB(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / spectrumOffset) % 2 - 1));
            double m = v - c;
            double num1 = Math.Floor(h / spectrumOffset);
            if (num1 == 6) num1 = 0;

            double r, g, b;
            switch (num1)
            {
                default:
                case 0: r = c; g = x; b = 0; break;
                case 1: r = x; g = c; b = 0; break;
                case 2: r = 0; g = c; b = x; break;
                case 3: r = 0; g = x; b = c; break;
                case 4: r = x; g = 0; b = c; break;
                case 5: r = c; g = 0; b = x; break;
            }
            return (r + m, g + m, b + m);
        }
        #endregion

        public static byte ToGray(this Color color) 
            => (byte)(color.R * 0.299d + color.G * 0.587d + color.B * 0.114d);
        public static Color ToGrayColor(this Color color)
        {
            byte grey = color.ToGray();
            return Color.FromArgb(255, grey, grey, grey);
        }
    }
    public enum ColorContractMode
    {
        Auto, Light, Dark
    }
}
