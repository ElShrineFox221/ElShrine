using ElShrine.Graphics;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.Wpf.Converters
{
    public sealed class InvalidValueConversionException(object? v) : Exception($"Invalid value: {v ?? "#NULL"}");

    [ValueConversion(typeof(MediaColor), typeof(SolidColorBrush))]
    [ValueConversion(typeof(MediaColor), typeof(DrawingColor))]
    [ValueConversion(typeof(MediaColor), typeof(ColorData))]
    [ValueConversion(typeof(DrawingColor), typeof(SolidColorBrush))]
    [ValueConversion(typeof(DrawingColor), typeof(MediaColor))]
    [ValueConversion(typeof(DrawingColor), typeof(ColorData))]
    [ValueConversion(typeof(SolidColorBrush), typeof(MediaColor))]
    [ValueConversion(typeof(SolidColorBrush), typeof(DrawingColor))]
    [ValueConversion(typeof(SolidColorBrush), typeof(ColorData))]
    [ValueConversion(typeof(ColorData), typeof(SolidColorBrush))]
    [ValueConversion(typeof(ColorData), typeof(MediaColor))]
    [ValueConversion(typeof(ColorData), typeof(DrawingColor))]

    [ValueConversion(typeof(Thickness), typeof(CornerRadius))]
    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(CornerRadius))]
    [ValueConversion(typeof(double), typeof(Thickness))]
    [ValueConversion(typeof(int), typeof(Thickness))]
    [ValueConversion(typeof(bool), typeof(Thickness))]
    [ValueConversion(typeof(double), typeof(CornerRadius))]
    [ValueConversion(typeof(int), typeof(CornerRadius))]
    [ValueConversion(typeof(bool), typeof(CornerRadius))]

    [ValueConversion(typeof(string), typeof(double))]
    [ValueConversion(typeof(int), typeof(double))]
    [ValueConversion(typeof(float), typeof(double))]
    [ValueConversion(typeof(string), typeof(int))]
    [ValueConversion(typeof(double), typeof(int))]
    [ValueConversion(typeof(float), typeof(int))]
    [ValueConversion(typeof(string), typeof(float))]
    [ValueConversion(typeof(double), typeof(float))]
    [ValueConversion(typeof(int), typeof(float))]

    [ValueConversion(typeof(string), typeof(bool))]
    [ValueConversion(typeof(int), typeof(bool))]

    [ValueConversion(typeof(double), typeof(string))]
    [ValueConversion(typeof(int), typeof(string))]
    [ValueConversion(typeof(bool), typeof(string))]
    [ValueConversion(typeof(Thickness), typeof(string))]
    [ValueConversion(typeof(CornerRadius), typeof(string))]
    [ValueConversion(typeof(MediaColor), typeof(string))]
    [ValueConversion(typeof(DrawingColor), typeof(string))]

    [ValueConversion(typeof(object), typeof(string))]
    [ValueConversion(typeof(object), typeof(bool))]
    [ValueConversion(typeof(object), typeof(double))]
    [ValueConversion(typeof(object), typeof(int))]
    [ValueConversion(typeof(object), typeof(float))]
    [ValueConversion(typeof(object), typeof(Thickness))]
    [ValueConversion(typeof(object), typeof(CornerRadius))]
    [ValueConversion(typeof(object), typeof(SolidColorBrush))]
    public sealed class CommonConverter : IValueConverter
    {
        #region Directly converters
        private static bool TryConvertTo<T>(object? value, out T? result)
        {
            if (value is T t)
            {
                result = (T?)t;
                return true;
            }
            try
            {
                result = (T?)System.Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public static double ToNumber(object? value)
        {
            double number;
            if (TryConvertTo<double>(value, out var d)) number = d;
            else
            {
                if (value is not string s) s = value?.ToString() ?? string.Empty;
                if (double.TryParse(s, out var r)) number = r;
                else if (value is null) number = 0;
                else throw new InvalidValueConversionException(s);
            }
            return number;
        }
        public static bool ToBool(object? value)
        {
            bool result;
            if (value is bool b) result = b;
            else
            {
                if (value is not string s) s = value?.ToString() ?? string.Empty;
                if (bool.TryParse(s, out b)) result = b;
                else if (value is null) result = default;
                else throw new InvalidValueConversionException(s);
            }
            return result;
        }
        public static string ToString(object? value) => value?.ToString() ?? string.Empty;
        public static ColorData ToColor(object? value)
        {
            ColorData result;
            if (value is ColorData cd) result = cd;
            else if (value is Brush brush)
            {
                if (brush is SolidColorBrush scb) result = scb.Color.ToColorData();
                else throw new NotImplementedException();
            }
            else if (value is MediaColor mc) result = mc.ToColorData();
            else if (value is DrawingColor dc) result = dc.ToColorData();
            else if(value is null) return ColorData.FromData();
            else throw new InvalidValueConversionException(value);
            return result;
        }
        public static (double V0, double V1, double V2, double V3) ToVector4(object? value)
        {
            (double v0, double v1, double v2, double v3) result;
            if (value is Thickness t) result = (t.Left, t.Top, t.Right, t.Bottom);
            else if (value is CornerRadius c) result = (c.TopLeft, c.TopRight, c.BottomRight, c.BottomLeft);
            else if (!double.IsNaN(ToNumber(value)))
            {
                var num = ToNumber(value);
                result = (num, num, num, num);
            }
            else if (value is null) result = (0, 0, 0, 0);
            else throw new InvalidValueConversionException(value);
            return result;
        }
        #endregion

        public static object? FinalizeConvert(object? temporaryResult, Type targetType)
        {
            object? result;
            if (targetType == typeof(string)) result = ToString(temporaryResult);
            else if (targetType == typeof(bool)) result = ToBool(temporaryResult);
            else if (targetType == typeof(Brush) || targetType == typeof(SolidColorBrush))
            {
                var colorData = ToColor(temporaryResult);
                result = colorData.ToSolidBrush();
            }
            else if (targetType == typeof(MediaColor)) result = ToColor(temporaryResult).ToMediaColor();
            else if (targetType == typeof(DrawingColor)) result = ToColor(temporaryResult).ToDrawingColor();
            else if (targetType == typeof(ColorData)) result = ToColor(temporaryResult);
            else if (targetType == typeof(Thickness) || targetType == typeof(CornerRadius))
            {
                var (V0, V1, V2, V3) = ToVector4(temporaryResult);
                if (targetType == typeof(Thickness)) result = new Thickness(V0, V1, V2, V3);
                else result = new CornerRadius(V0, V1, V2, V3);
            }
            else if (targetType == typeof(double) || targetType == typeof(int) || targetType == typeof(float))
            {
                var num = ToNumber(temporaryResult);
                if (targetType == typeof(double)) result = num;
                else if (targetType == typeof(int)) result = (int)num;
                else result = (float)num;
            }
            else throw new InvalidValueConversionException(temporaryResult);
            return result;
        }

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
            => FinalizeConvert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
