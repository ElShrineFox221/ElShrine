using ElShrine.Graphics;
using Microsoft.Win32;

namespace ElShrine;

public static class ColorDataExtensions
{
    private static byte Clamp(int value) => (byte)Math.Max(0, Math.Min(255, value));
    private static byte Clamp(double value) => (byte)Math.Max(0, Math.Min(255, value));

    #region 基础信息与取值
    public static byte GetGrayValue(this ColorData color)
    {
        var argb = color.Space == ColorSpace.ARGB ? color : color.ToSpace(ColorSpace.ARGB);
        return Clamp(argb.V1 * 0.299 + argb.V2 * 0.587 + argb.V3 * 0.114);
    }
    public static bool IsDark(this ColorData color) => color.GetGrayValue() < 128;
    #endregion

    #region 颜色转换与滤镜

    /// <summary>
    /// 转为灰度颜色 (ARGB 格式)
    /// </summary>
    public static ColorData ToGrayColor(this ColorData color)
    {
        byte gray = color.GetGrayValue();
        return ColorData.FromComp(color.A, gray, gray, gray, ColorSpace.ARGB);
    }

    /// <summary>
    /// 获取反色 (Inverted)，保持 Alpha 不变
    /// </summary>
    public static ColorData ToOppositeColor(this ColorData color)
    {
        var c = color.Space == ColorSpace.ARGB ? color : color.ToSpace(ColorSpace.ARGB);
        return ColorData.FromComp(
            c.A,
            (byte)(255 - c.V1),
            (byte)(255 - c.V2),
            (byte)(255 - c.V3),
            ColorSpace.ARGB
        );
    }

    /// <summary>
    /// 修改透明度 (直接设置 0-255)
    /// </summary>
    public static ColorData WithAlpha(this ColorData color, byte alpha)
    {
        return ColorData.FromComp(alpha, color.V1, color.V2, color.V3, color.Space);
    }

    /// <summary>
    /// 修改透明度 (比例 0.0 - 1.0)
    /// </summary>
    public static ColorData MultiplyAlpha(this ColorData color, double amount)
    {
        byte newAlpha = Clamp(color.A * amount);
        return ColorData.FromComp(newAlpha, color.V1, color.V2, color.V3, color.Space);
    }

    #endregion

    #region 混合运算 (Blending)

    /// <summary>
    /// 线性插值 (Linear Interpolation)
    /// </summary>
    public static ColorData Lerp(this ColorData from, ColorData to, double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        var c1 = from.Space == ColorSpace.ARGB ? from : from.ToSpace(ColorSpace.ARGB);
        var c2 = to.Space == ColorSpace.ARGB ? to : to.ToSpace(ColorSpace.ARGB);
        return ColorData.FromComp(
            Clamp(c1.A + (c2.A - c1.A) * t),
            Clamp(c1.V1 + (c2.V1 - c1.V1) * t),
            Clamp(c1.V2 + (c2.V2 - c1.V2) * t),
            Clamp(c1.V3 + (c2.V3 - c1.V3) * t),
            ColorSpace.ARGB
        );
    }

    #endregion

    #region HSL 设置 (直接设值)

    /// <summary>
    /// 设置 HSL 色相
    /// </summary>
    /// <param name="hue">色相 (0.0 - 360.0)</param>
    public static ColorData WithHslHue(this ColorData color, double hue)
    {
        var c = color.Space == ColorSpace.AHSL ? color : color.ToSpace(ColorSpace.AHSL);
        hue %= 360;
        if (hue < 0) hue += 360;
        byte v1 = Clamp(hue / 360d * 255);
        return ColorData.FromComp(c.A, v1, c.V2, c.V3, ColorSpace.AHSL);
    }

    /// <summary>
    /// 设置 HSL 饱和度
    /// </summary>
    /// <param name="saturation">饱和度 (0.0 - 1.0)</param>
    public static ColorData WithHslSaturation(this ColorData color, double saturation)
    {
        var c = color.Space == ColorSpace.AHSL ? color : color.ToSpace(ColorSpace.AHSL);
        byte v2 = Clamp(saturation * 255);
        return ColorData.FromComp(c.A, c.V1, v2, c.V3, ColorSpace.AHSL);
    }

    /// <summary>
    /// 设置 HSL 亮度
    /// </summary>
    /// <param name="lightness">亮度 (0.0 - 1.0)</param>
    public static ColorData WithHslLightness(this ColorData color, double lightness)
    {
        var c = color.Space == ColorSpace.AHSL ? color : color.ToSpace(ColorSpace.AHSL);
        byte v3 = Clamp(lightness * 255);
        return ColorData.FromComp(c.A, c.V1, c.V2, v3, ColorSpace.AHSL);
    }

    #endregion

    #region HSV 设置 (直接设值)

    /// <summary>
    /// 设置 HSV 色相
    /// </summary>
    /// <param name="hue">色相 (0.0 - 360.0)</param>
    public static ColorData WithHsvHue(this ColorData color, double hue)
    {
        var c = color.Space == ColorSpace.AHSV ? color : color.ToSpace(ColorSpace.AHSV);

        hue %= 360;
        if (hue < 0) hue += 360;

        byte v1 = Clamp(hue / 360d * 255);
        return ColorData.FromComp(c.A, v1, c.V2, c.V3, ColorSpace.AHSV);
    }

    /// <summary>
    /// 设置 HSV 饱和度
    /// </summary>
    /// <param name="saturation">饱和度 (0.0 - 1.0)</param>
    public static ColorData WithHsvSaturation(this ColorData color, double saturation)
    {
        var c = color.Space == ColorSpace.AHSV ? color : color.ToSpace(ColorSpace.AHSV);
        byte v2 = Clamp(saturation * 255);
        return ColorData.FromComp(c.A, c.V1, v2, c.V3, ColorSpace.AHSV);
    }

    /// <summary>
    /// 设置 HSV 明度 (ValueText)
    /// </summary>
    /// <param name="value">明度 (0.0 - 1.0)</param>
    public static ColorData WithHsvValue(this ColorData color, double value)
    {
        var c = color.Space == ColorSpace.AHSV ? color : color.ToSpace(ColorSpace.AHSV);
        byte v3 = Clamp(value * 255);
        return ColorData.FromComp(c.A, c.V1, c.V2, v3, ColorSpace.AHSV);
    }

    #endregion

    #region Build
    public static ColorData ToColorData(this uint data, ColorSpace colorSpace = ColorSpace.ARGB)
        => ColorData.FromData(data, colorSpace);
    public static ColorData ToAHSV (this ColorData color)
        => color.ToSpace(ColorSpace.AHSV);
    public static ColorData ToAHSL (this ColorData color)
        => color.ToSpace(ColorSpace.AHSL);
    public static ColorData ToARGB(this ColorData color)
        => color.ToSpace(ColorSpace.ARGB);
    #endregion

    public static ColorData GetAccentColor()
    {
        string registryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM";

        int colorValue = (int)(Registry.GetValue(registryPath, "AccentColor", 0) ?? -1);
        byte r = (byte)(colorValue & 0xFF);
        byte g = (byte)((colorValue >> 8) & 0xFF);
        byte b = (byte)((colorValue >> 16) & 0xFF);
        byte a = (byte)((colorValue >> 24) & 0xFF);
        return ColorData.FromComp(a, r, g, b, ColorSpace.ARGB);
    }
}
