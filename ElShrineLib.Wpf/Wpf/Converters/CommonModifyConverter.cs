using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    [ValueConversion(typeof(double), typeof(double))]

    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(Thickness))]
    [ValueConversion(typeof(double), typeof(Thickness))]
    [ValueConversion(typeof(int), typeof(Thickness))]
    [ValueConversion(typeof(float), typeof(Thickness))]
    [ValueConversion(typeof(object), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(CornerRadius))]
    [ValueConversion(typeof(Thickness), typeof(CornerRadius))]
    [ValueConversion(typeof(double), typeof(CornerRadius))]
    [ValueConversion(typeof(int), typeof(CornerRadius))]
    [ValueConversion(typeof(float), typeof(CornerRadius))]
    [ValueConversion(typeof(object), typeof(CornerRadius))]
    public sealed class CommonModifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, false);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, true);
        private static object Convert(object value, Type targetType, object parameter, bool isConvertBack)
        {
            if (targetType == typeof(object) && value.GetType() != targetType) targetType = value.GetType();
            if (targetType == typeof(bool)) return !CommonConverter.ToBool(value);
            if (targetType == typeof(double))
            {
                var v = CommonConverter.ToNumber(value);
                var paraStr = parameter?.ToString()?.Trim();
                return DoParameterCalculation(v, paraStr);
            }
            if (targetType == typeof(Thickness) || targetType == typeof(CornerRadius))
            {
                var values = CommonConverter.ToVector4(value);
                var (V0, V1, V2, V3) = (0d, 0d, 0d, 0d);
                var paraStr = parameter?.ToString()?.Trim() ?? string.Empty;
                var splits = paraStr.Split('|');
                foreach (var param in splits)
                {
                    var sp = param.Split('_');
                    if (sp.Length == 2)
                    {
                        double num = 0;
                        var ps = sp[1].Split('+');
                        const string chars = "LTRBMSA";
                        foreach (var s in ps)
                        {
                            var str = s; double d = 0;
                            var index = chars.IndexOfIngoreCase(str[0]);
                            if (index != -1) str = str[1..];
                            else
                            {
                                index = chars.IndexOfIngoreCase(str[^1]);
                                if (index != -1) str = str[..^1];
                            }
                            d = double.Parse(str);
                            d *= index switch
                            {
                                0 => values.V0,
                                1 => values.V1,
                                2 => values.V2,
                                3 => values.V3,
                                4 => new double[] { values.V0, values.V1, values.V2, values.V3 }.Max(),
                                5 => new double[] { values.V0, values.V1, values.V2, values.V3 }.Min(),
                                6 => new double[] { values.V0, values.V1, values.V2, values.V3 }.Average(),
                                _ => 1
                            };
                            num += d;
                        }
                        if (sp[0].ContainsIgnoreCase("L")) V0 += num;
                        if (sp[0].ContainsIgnoreCase("T")) V1 += num;
                        if (sp[0].ContainsIgnoreCase("R")) V2 += num;
                        if (sp[0].ContainsIgnoreCase("B")) V3 += num;
                    }
                }
                if (targetType == typeof(Thickness)) return new Thickness(V0, V1, V2, V3);
                return new CornerRadius(V0, V1, V2, V3);
            } 
            return value;
        }

        private static double DoParameterCalculation(double v, string? paraStr)
        {
            if (!string.IsNullOrWhiteSpace(paraStr))
            {
                const double PI = Math.PI;
                const double HPI = Math.PI / 2;
                const double PI2 = Math.PI * 2;
                if (paraStr == "l2") v = 1 - 2 * Math.Abs(Math.Clamp(v, 0, 1) - 0.5);
                else if (paraStr == "cl2") v = Math.Abs(Math.Clamp(v, 0, 1) - 0.5d) * 2;
                else if (paraStr.EqualIgnoreCase("sin")) v = (Math.Sin(PI * (Math.Clamp(v, 0, 1) - 0.5d)) + 1) / 2d;
                else if (paraStr.EqualIgnoreCase("cos")) v = (Math.Cos(Math.PI * Math.Clamp(v, 0, 1)) + 1) / 2d;
                else if (paraStr.EqualIgnoreCase("sin2")) v = (Math.Sin(PI2 * Math.Clamp(v, 0, 1) - HPI) + 1) / 2d;
                else if (paraStr.EqualIgnoreCase("cos2")) v = (Math.Cos(PI2 * Math.Clamp(v, 0, 1)) + 1) / 2d;
                else if (paraStr.Length > 1 && double.TryParse(paraStr[1..].Trim(), out var num))
                {
                    v = paraStr[0] switch
                    {
                        '+' => v + num,
                        '-' => v - num,
                        '*' => v * num,
                        '/' => v / num,
                        '%' => v % num,
                        '^' => Math.Pow(v, num),
                        _ => v
                    };
                }
            }
            return v;
        }
    }
}
