using ElShrine.Common.DataStructure;
using System.Globalization;
using System.Runtime.Serialization;

namespace ElShrine.Graphics;

[DataContract]
public sealed class ColorData : ICloneable<ColorData>, IEquatable<ColorData>
{
    private uint data;
    [DataMember]
    public uint Data
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
    [DataMember] public readonly ColorSpace Space;

    #region builder
    public static ColorData FromComp(byte a, byte v1, byte v2, byte v3, ColorSpace colorSpace = ColorSpace.ARGB)
        => new((uint)a << 24 | (uint)v1 << 16 | (uint)v2 << 8 | v3, colorSpace);
    public static ColorData FromCompRate(double aRate, double v1Rate, double v2Rate, double v3Rate, ColorSpace colorSpace = ColorSpace.ARGB)
        => new((uint)Math.Round(aRate * 255) << 24 | (uint)Math.Round(v1Rate * 255) << 16 | (uint)Math.Round(v2Rate * 255) << 8 | (uint)Math.Round(v3Rate * 255), colorSpace);
    public static ColorData FromData(uint data = 0xFFFFFFFF, ColorSpace colorSpace = ColorSpace.ARGB)
        => new(data, colorSpace);
    private ColorData(uint data = 0xFFFFFFFF, ColorSpace colorSpace = ColorSpace.ARGB)
    {
        this.data = data;
        Space = colorSpace;
    }
    #endregion

    #region string & hex code
    private static bool TryParseHex(string input, int length, out uint result)
    {
        result = 0;
        if (input.Length != length || !input.All(Uri.IsHexDigit)) return false;
        return uint.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
    }
    private string? hex8;
    private string? hex6;

    public string Hex8
    {
        get => hex8 ??= data.ToString("X8");
        set
        {
            if (hex8 != value && TryParseHex(value, 8, out var r))
                Data = r;
        }
    }
    public string Hex6
    {
        get => hex6 ??= (data & 0x00FFFFFFu).ToString("X6");
        set
        {
            if (hex6 != value && TryParseHex(value, 6, out var rgb))
            {
                var alpha = data & 0xFF000000u;
                Data = alpha | (rgb & 0x00FFFFFFu);
            }
        }
    }
    #endregion

    public byte A
    {
        get => (byte)((data >> 24) & 0xFFu);
        set => Data = (data & 0x00FFFFFFu) | (((uint)value) << 24);
    }
    public byte V1
    {
        get => (byte)((data >> 16) & 0xFFu);
        set => Data = (data & 0xFF00FFFFu) | (((uint)value) << 16);
    }
    public byte V2
    {
        get => (byte)((data >> 8) & 0xFFu);
        set => Data = (data & 0xFFFF00FFu) | (((uint)value) << 8);
    }
    public byte V3
    {
        get => (byte)(data & 0xFF);
        set => Data = (data & 0xFFFFFF00u) | value;
    }


    private static ColorData ConvertInternal(ColorData source, ColorSpace targetSpace)
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
            ColorSpace.ARGB => FromCompRate(argb.a / 255d, argb.r, argb.g, argb.b, ColorSpace.ARGB),
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
            return FromCompRate(a / 255d, h / 360, s, max, ColorSpace.AHSV);
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
            return FromCompRate(a / 255d, h / 360, s, l, ColorSpace.AHSL);
        }
    }
    public ColorData ToSpace(ColorSpace colorSpace) => ConvertInternal(this, colorSpace);
    object ICloneable.Clone() => new ColorData(data, Space);


    #region Equality
    public bool Equals(ColorData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return data == other.data && Space == other.Space;
    }
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ColorData other && Equals(other));
    }
    public override int GetHashCode() => HashCode.Combine(data, Space);
    public static bool operator ==(ColorData? left, ColorData? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
    public static bool operator !=(ColorData? left, ColorData? right)
    {
        return !(left == right);
    }
    public override string ToString()
    {
        return $"{Space}: {Hex8}";
    }
    #endregion
}
public enum ColorSpace { ARGB, AHSV, AHSL }
