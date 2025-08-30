using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;

namespace ElShrine.EGraphic
{
    [DataContract]
    public sealed class ColorData(int data = 0x7FFFFFFF, ColorSpace colorSpace = ColorSpace.ARGB)
    {
        private int data = data;
        [DataMember] public int Data
        {
            get => data;
            set
            {
                if (data != value)
                {
                    data = value;
                    hex6 = null;
                    hex8 = null;
                }
            }
        }
        [DataMember] public readonly ColorSpace Space = colorSpace;

        #region string & hex code
        private static bool TryParseHex(string input, int length, out int result)
        {
            result = 0;
            if (input.Length != length || !input.All(Uri.IsHexDigit)) return false;
            return int.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }
        private string? hex8;
        private string? hex6;
        public string Hex8
        {
            get => hex8 ??= data.ToString("X8");
            set
            {
                if (hex8 != value && TryParseHex(value, 8, out var r))
                    data = r;
            }
        }
        public string Hex6
        {
            get => hex6 ??= (data & 0x00FFFFFF).ToString("X6");
            set
            {
                if (hex6 != value && TryParseHex(value, 6, out var rgb))
                {
                    int alpha = data & unchecked((int)0xFF000000);
                    data = alpha | (rgb & 0x00FFFFFF);
                }
            }
        }
        #endregion

        public byte A
        {
            get => (byte)((data >> 24) & 0xFF);
            set => data = (data & 0x00FFFFFF) | (value << 24);
        }
        public byte V1
        {
            get => (byte)((data >> 16) & 0xFF);
            set => data = (data & unchecked((int)0xFF00FFFF)) | (value << 16);
        }
        public byte V2
        {
            get => (byte)((data >> 8) & 0xFF);
            set => data = (data & unchecked((int)0xFFFF00FF)) | (value << 8);
        }
        public byte V3
        {
            get => (byte)(data & 0xFF);
            set => data = (data & unchecked((int)0xFFFFFF00)) | value;
        }


        public (byte A, byte R, byte G, byte B) ARGB
        {
            get
            {
                var colorData = ToARGB();
                return (colorData.A, colorData.V1, colorData.V2, colorData.V3);
            }
            set
            {
                var colorData = new ColorData((value.A << 24) | (value.R << 16) | (value.G << 8) | value.B, ColorSpace.ARGB);
                Data = colorData.To(Space).Data;
            }
        }
        public ColorData ToARGB() => To(ColorSpace.ARGB);
        public (byte A, byte H, byte S, byte V) AHSV
        {
            get
            {
                var colorData = ToAHSV();
                return (colorData.A, colorData.V1, colorData.V2, colorData.V3);
            }
            set
            {
                var colorData = new ColorData((value.A << 24) | (value.H << 16) | (value.S << 8) | value.V, ColorSpace.AHSV);
                Data = colorData.To(Space).Data;
            }
        }
        public ColorData ToAHSV() => To(ColorSpace.AHSV);
        public (byte A, byte H, byte S, byte L) AHSL
        {
            get
            {
                var colorData = ToAHSL();
                return (colorData.A, colorData.V1, colorData.V2, colorData.V3);
            }
            set
            {
                var colorData = new ColorData((value.A << 24) | (value.H << 16) | (value.S << 8) | value.L, ColorSpace.AHSL);
                Data = colorData.To(Space).Data;
            }
        }
        public ColorData ToAHSL() => To(ColorSpace.AHSL);
        public ColorData To(ColorSpace colorSpace) => Convert(this, colorSpace);
        private static ColorData Convert(ColorData source, ColorSpace targetSpace)
        {
            if (source.Space == targetSpace) return new(source.data, targetSpace);
            (byte a, double r, double g, double b) argb = source.Space switch
            {
                ColorSpace.ARGB => (source.A, source.V1 / 255d, source.V2 / 255d, source.V3 / 255d),
                ColorSpace.AHSV => ahsvToArgb(source),
                ColorSpace.AHSL => ahslToArgb(source),
                _ => (0, 0, 0, 0)
            };
            return targetSpace switch
            {
                ColorSpace.ARGB => new ColorData((argb.a << 24) | ((byte)(argb.r * 255) << 16) | ((byte)(argb.g * 255) << 8) | (byte)(argb.b * 255), ColorSpace.ARGB),
                ColorSpace.AHSV => argbToAhsv(argb.a, argb.r, argb.g, argb.b),
                ColorSpace.AHSL => argbToAhsl(argb.a, argb.r, argb.g, argb.b),
                _ => new(source.data, targetSpace)
            };

            static (byte a, double r, double g, double b) ahsvToArgb(ColorData colorData)
            {
                double h = colorData.V1 * 360 / 255d, s = colorData.V2 / 255d, v = colorData.V3 / 255d;
                double cVal = v * s, x = cVal * (1 - Math.Abs((h / 60) % 2 - 1));
                double m = v - cVal;
                return (colorData.A,
                    (h < 60 ? cVal : h < 120 ? x : h < 240 ? 0 : h < 300 ? x : cVal) + m,
                    (h < 60 ? x : h < 120 ? cVal : h < 180 ? cVal : h < 240 ? x : 0) + m,
                    (h < 120 ? 0 : h < 180 ? x : h < 240 ? cVal : h < 300 ? cVal : x) + m);
            }
            static (byte a, double r, double g, double b) ahslToArgb(ColorData colorData)
            {
                double h = colorData.V1 * 360 / 255d, s = colorData.V2 / 255d, l = colorData.V3 / 255d;
                double cVal = (1 - Math.Abs(2 * l - 1)) * s;
                double x = cVal * (1 - Math.Abs((h / 60) % 2 - 1));
                double m = l - cVal / 2;
                return (colorData.A,
                    (h < 60 ? cVal : h < 120 ? x : h < 240 ? 0 : h < 300 ? x : cVal) + m,
                    (h < 60 ? x : h < 120 ? cVal : h < 180 ? cVal : h < 240 ? x : 0) + m,
                    (h < 120 ? 0 : h < 180 ? x : h < 240 ? cVal : h < 300 ? cVal : x) + m);
            }
            static ColorData argbToAhsv(byte a, double r, double g, double b)
            {
                double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
                double delta = max - min, h = 0, s = max == 0 ? 0 : delta / max;
                if (delta != 0)
                {
                    if (max == r) h = (g - b) / delta + (g < b ? 6 : 0);
                    else if (max == g) h = (b - r) / delta + 2;
                    else h = (r - g) / delta + 4;
                    h *= 60;
                }
                return new ColorData((a << 24) | ((byte)(h * 255 / 360) << 16) | ((byte)(s * 255) << 8) | (byte)(max * 255), ColorSpace.AHSV);
            }
            static ColorData argbToAhsl(byte a, double r, double g, double b)
            {
                double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
                double delta = max - min, h = 0, l = (max + min) / 2;
                double s = l == 0 || l == 1 ? 0 : delta / (1 - Math.Abs(2 * l - 1));
                if (delta != 0)
                {
                    if (max == r) h = (g - b) / delta + (g < b ? 6 : 0);
                    else if (max == g) h = (b - r) / delta + 2;
                    else h = (r - g) / delta + 4;
                    h *= 60;
                }
                return new ColorData((a << 24) | ((byte)(h * 255 / 360) << 16) | ((byte)(s * 255) << 8) | (byte)(l * 255), ColorSpace.AHSL);
            }
        }

        public bool DataEqual(ColorData other) => (Data == other.Data && Space == other.Space) || To(other.Space).Data == other.data;
    }
    public static class ColorDataExtension
    {
        public static ColorData ToColorData(this int dataSource, ColorSpace colorSpace = ColorSpace.ARGB) => new(dataSource, colorSpace);
        public static ColorData ToColorData(this (byte a, byte r, byte g, byte b) dataSource, ColorSpace colorSpace = ColorSpace.ARGB)
            => new((dataSource.a << 24) | (dataSource.r << 16) | (dataSource.g << 8) | (dataSource.b), colorSpace);
        public static ColorData ToColorData(this Color dataSource)
            => new((dataSource.A << 24) | (dataSource.R << 16) | (dataSource.G << 8) | (dataSource.B));

        public static Color ToDrawingColor(this ColorData colorData)
        {
            var (A, R, G, B) = colorData.ARGB;
            var color = Color.FromArgb(A, R, G, B);
            return color;
        }
        public static ColorData ToGray(this ColorData colorData)
        {
            var (A, R, G, B) = colorData.ARGB;
            var gray = (byte)(R * 0.299d + G * 0.587d + B * 0.114d);
            return (A, gray, gray, gray).ToColorData();
        }
        public static ColorData ToOpposite(this ColorData colorData)
        {
            var (A, R, G, B) = colorData.ARGB;
            return (A, (byte)(byte.MaxValue - R), (byte)(byte.MaxValue - G), (byte)(byte.MaxValue - B)).ToColorData();
        }

        public static ColorData Clone(this ColorData colorData) => colorData.Data.ToColorData(colorData.Space);
        public static ColorData Lerp(this ColorData from, ColorData to, double rate, LerpState spaceState, LerpState alphaSatae)
        {
            var alpha = alphaSatae switch
            {
                LerpState.Stay => from.A,
                LerpState.To => to.A,
                LerpState.Lerp => from.A.Lerp(to.A, rate),
                _ => byte.MaxValue,
            };
            if(from.Space != to.Space)
            {
                switch (spaceState)
                {
                    case LerpState.Stay:
                        to = to.To(from.Space);
                        break;
                    case LerpState.To:
                        from = from.To(to.Space);
                        break;
                    case LerpState.Lerp:
                        if (rate > 0.5) to = to.To(from.Space);
                        else from = from.To(to.Space);
                        break;
                }
            }
            var newColor = new ColorData((alpha, from.V1.Lerp(to.V1, rate), from.V2.Lerp(to.V2, rate), from.V3.Lerp(to.V3, rate)).ToInt32());
            return newColor;
        }
    }
    public enum LerpState { Stay, To, Lerp, Full }
    public enum ColorSpace { ARGB, AHSV, AHSL }
}
