using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(Thickness))]
    [ValueConversion(typeof(double), typeof(Thickness))]
    [ValueConversion(typeof(int), typeof(Thickness))]
    [ValueConversion(typeof(CornerRadius), typeof(CornerRadius))]
    [ValueConversion(typeof(Thickness), typeof(CornerRadius))]
    [ValueConversion(typeof(double), typeof(CornerRadius))]
    [ValueConversion(typeof(int), typeof(CornerRadius))]
    public class QuadruplesConverter : IValueConverter
    {
        private enum QuadModiSigns
        {
            None = 0, L = 1, T = 2, R = 4, B = 8,
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            (double v0, double v1, double v2, double v3) values = ToValues(value);

            (double v0, double v1, double v2, double v3) = (0, 0, 0, 0);
            var parameters = parameter?.ToString()?.Split('|');//LR=R1+1
            if (parameters is not null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    var sp = param.Split('_');
                    if (sp.Length == 2)
                    {
                        double num = 0;
                        var ps = sp[1].Split('+');
                        var chars = "LTRBMSA";
                        foreach (var s in ps)
                        {
                            var str = s; double d = 0;
                            var index = chars.IndexOfIngoreCase(str[0]);
                            if (index != -1) str = str[1..];
                            else
                            {
                                chars.IndexOfIngoreCase(str[^1]);
                                if (index != -1) str = str[..^2];
                            }
                            d = double.Parse(str);
                            d *= index switch
                            {
                                0 => values.v0,
                                1 => values.v1,
                                2 => values.v2,
                                3 => values.v3,
                                4 => new double[] { v0, v1, v2, v3 }.Max(),
                                5 => new double[] { v0, v1, v2, v3 }.Min(),
                                6 => new double[] { v0, v1, v2, v3 }.Average(),
                                _ => 1
                            };
                            num += d;
                        }
                        if (sp[0].ContainsIgnoreCase("L")) v0 += num;
                        if (sp[0].ContainsIgnoreCase("T")) v1 += num;
                        if (sp[0].ContainsIgnoreCase("R")) v2 += num;
                        if (sp[0].ContainsIgnoreCase("B")) v3 += num;
                    }
                }
            }

            return ToQuadruple((v0, v1, v2, v3), targetType);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == targetType) return value;

            (double v0, double v1, double v2, double v3) values = ToValues(value);
            return ToQuadruple(values, targetType);
        }
        public static (double v0, double v1, double v2, double v3) ToValues(object quadruple)
        {
            (double v0, double v1, double v2, double v3) values;
            if (quadruple is double d) values = (d, d, d, d);
            else if(quadruple is int i) values = (i, i, i, i);
            else if (quadruple is Thickness thickness) values = (thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
            else if (quadruple is CornerRadius cornerRadius) values = (cornerRadius.TopLeft, cornerRadius.BottomLeft, cornerRadius.TopRight, cornerRadius.BottomRight);
            else throw new ArgumentException(null, nameof(quadruple));
            return values;
        }
        public static object ToQuadruple((double v0, double v1, double v2, double v3) values, Type targetType)
        {
            object result;
            if (targetType == typeof(Thickness)) result = new Thickness(values.v0, values.v1, values.v2, values.v3);
            else if (targetType == typeof(CornerRadius)) result = new CornerRadius(values.v0, values.v1, values.v2, values.v3);
            else throw new NotImplementedException();
            return result;
        }
    }
}
