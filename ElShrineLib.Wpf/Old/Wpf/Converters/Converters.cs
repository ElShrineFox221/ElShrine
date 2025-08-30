using ElShrine.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static ElShrine.Wpf.Methods;

namespace ElShrine.Old.Wpf.Converters
{
    [ValueConversion(typeof(Thickness), typeof(Color))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ThicknessToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            return ColorFactor(Color.FromArgb((byte)thickness.Left, (byte)thickness.Top, (byte)thickness.Right, (byte)thickness.Bottom), int.TryParse(parameter.ToString(), out int parameterValue) ? parameterValue : 0, false);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            bool HasValue = int.TryParse(parameter.ToString(), out int parameterValue);
            double rate = 1 - (HasValue ? parameterValue : 0);
            return new Thickness(Math.Min(c.A / rate, 255), Math.Min(c.R / rate, 255), Math.Min(c.G / rate, 255), Math.Min(c.B / rate, 255));
        }
    }

    [ValueConversion(typeof(Color), typeof(Brush))]
    [ValueConversion(typeof(System.Drawing.Color), typeof(Brush))]
    [ValueConversion(typeof(Brush), typeof(Color))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ColorToBrush : IValueConverter
    {
        public readonly static ColorToBrush Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter);
        }
        private static object Convert(object value, Type targetType, object _)
        {
            object? result = null;
            if (targetType == typeof(Brush))
            {
                if (value is Color c) result = new SolidColorBrush(c);
                else if (value is System.Drawing.Color c1) result = new SolidColorBrush(c1.ToMediaColor());
                else result = value;
            } 
            else if (targetType == typeof(Color))
            {
                if (value is Color c) result = c;
                else if (value is System.Drawing.Color c1) result = c1.ToMediaColor();
                else
                {
                    if (value is SolidColorBrush scb) result = scb.Color;
                    else if (value is GradientBrush gcb) result = gcb.GradientStops[^1].Color;
                }
            }
            else if (targetType == typeof(System.Drawing.Color))
            {
                if (value is Color c) result = c.ToDrawingColor();
                else if (value is System.Drawing.Color c1) result = c1;
                else result = ((SolidColorBrush)value).Color;
            }
            return result ?? 0;
        }
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class BrushToColorHeavy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = 0;
            object color = new ColorToBrush().Convert(value, typeof(Color), parameter, culture);
            if (!double.TryParse(parameter.ToString(), out double rate)) rate = 0.4;
            if (color is Color c) result = ColorFactor(c, System.Drawing.Color.White.ToMediaColor(), rate);
            if (targetType == typeof(Brush)) result = new SolidColorBrush((Color)result);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #region Animation RateConverter
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ToOppsiteColorBrush : IMultiValueConverter, IValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1 ||
                values[0] is not Brush && values[0] is not Color && values[0] is not System.Drawing.Color) return 0;
            if (values.Length < 2 || values[1] is not double animeRate) animeRate = 1;
            double alphaRate = 1;
            if (values.Length > 2 && values[2] is double d) alphaRate = d;
            Color color = (Color)new ColorToBrush().Convert(values[0], typeof(Color), parameter, culture);
            string?[] strs = ((string)parameter).Split('_');
            if (strs.Length < 2 || !double.TryParse(strs[1], out double rate)) rate = 0;
            color = ColorFactor(color, ColorFactor(color, rate, false), animeRate);
            if (alphaRate != 1) color.A = (byte)(255 * alphaRate);
            return strs[0] switch {
                "Color" => color,
                "Brush" => new SolidColorBrush(color),
                _ => new SolidColorBrush(color),
            };
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert([value, 1], targetType, parameter, culture);
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ToLightColorBrush : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = 0;
            object color = new ColorToBrush().Convert(value, typeof(Color), parameter, culture);
            if (!double.TryParse(parameter.ToString(), out double rate)) rate = 0.4;
            if (color is Color c) result = ColorFactor(c, System.Drawing.Color.White.ToMediaColor(), rate);
            if (targetType == typeof(Brush)) result = new SolidColorBrush((Color)result);
            return result;
        }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1 || values[0] is not Brush brush) return 0;
            if (values.Length < 2 || values[1] is not double animeRate) animeRate = 1;
            object result = Convert(values[0], typeof(Brush), parameter, CultureInfo.CurrentCulture);
            ((SolidColorBrush)result).Color = ColorFactor(((SolidColorBrush)brush).Color, ((SolidColorBrush)result).Color, animeRate);
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ToTargetColorBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not Brush brush1 || values[1] is not Brush brush2) return targetType == typeof(Brush) ? new SolidColorBrush(System.Drawing.Color.Transparent.ToMediaColor()) : System.Drawing.Color.Transparent.ToMediaColor();
            if (values.Length < 3 || values[2] is not double animeRate) animeRate = 1;
            if (!double.TryParse(parameter.ToString(), out double rate)) rate = 0.2;
            Color endColor = ColorFactor(((SolidColorBrush)brush1).Color, ((SolidColorBrush)brush2).Color, rate);
            Color resultColor = ColorFactor(((SolidColorBrush)brush1).Color, endColor, animeRate);
            object result = targetType == typeof(Brush) ? new SolidColorBrush(resultColor) : resultColor;
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ToRequiredDouble : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 ||
                values[0] is not double based ||
                values[1] is not double d) return 0;
            string?[]? strs = parameter.ToString()?.Split('_');
            if (strs is null || strs.Length < 2 || !double.TryParse(strs[1], out double rate)) rate = 1;
            if (strs is not null && strs.Length == 1 && double.TryParse(strs[0], out double rate1)) rate = rate1;

            double result = strs is null ? 0 : strs[0] switch
            {
                "Extra" => based * (rate * d + 1),
                "DoubleExtra" => based * (rate * (2 * d - 1) + 1),
                _ => based * rate * d,
            };
            return Math.Max(result, 0);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
           => throw new NotImplementedException();
    }

    
    #endregion

    [ValueConversion(typeof(double), typeof(byte))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleToByte : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType);
        }
        private static object Convert(object value, Type targetType)
        {
            object? result;
            string? s;
            if(value is double d) s = Math.Round(d, 0).ToString();
            else s = value.ToString();

            if (targetType == typeof(double)) result = double.Parse(s ?? "0");
            else result = byte.Parse(s ?? "0");
            return result ?? 0;
        }
    }

    #region Factor
    [ValueConversion(typeof(double), typeof(double))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleFactor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, parameter, true);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, parameter, false);
        }
        private static object Convert(object value, object parameter, bool isFactor)
        {
            if (!double.TryParse(parameter.ToString(), out double factor)) factor = 1;
            if (!double.TryParse(value.ToString(), out double v)) v = 0;
            return v * (isFactor ? factor : 1 / factor);
        }
    }
    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ThicknessFactor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            =>throw new Exception();
        private static object Convert(object value, object parameter)
        {
            string[] parameters = parameter.ToString()?.Split('_') ?? ["0",];
            if (value is not Thickness result) result = new(0);
            double? d = parameters[0] switch
            {
                "1" => result.Top,
                "2" => result.Right,
                "3" => result.Bottom,
                "Max" => Math.Max(Math.Max(result.Left, result.Right), Math.Max(result.Top, result.Bottom)),
                "Min" => Math.Min(Math.Min(result.Left, result.Right), Math.Min(result.Top, result.Bottom)),
                "Avg" => (result.Left + result.Right + result.Top + result.Bottom) / 4d,
                "Sum" => result.Left + result.Right + result.Top + result.Bottom,
                _ => null,
            };
            result = d is null ? result : new((double)d);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Contains('L'))
                {
                    result.Left = double.TryParse(parameters[i].Replace("L", string.Empty), out double d0) ? d0 * result.Left : result.Left;
                }
                else if (parameters[i].Contains('T'))
                {
                    result.Top = double.TryParse(parameters[i].Replace("T", string.Empty), out double d0) ? d0 * result.Top : result.Top;
                }
                else if (parameters[i].Contains('R'))
                {
                    result.Right = double.TryParse(parameters[i].Replace("R", string.Empty), out double d0) ? d0 * result.Right : result.Right;
                }
                else if (parameters[i].Contains('B'))
                {
                    result.Bottom = double.TryParse(parameters[i].Replace("B", string.Empty), out double d0) ? d0 * result.Bottom : result.Bottom;
                }
            }
            return result;
        }
    }
    #endregion

    #region Add & Sub
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleAdder : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0d;
            char[]? chars = parameter.ToString()?.ToCharArray();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double || values[i] is CornerRadius radius)
                {
                    if (values[i] is not double d) d = radius.TopLeft;
                    if (chars is not null && chars.Length > i)
                    {
                        result += chars[i] switch
                        {
                            'A' => d,
                            'S' => -d,
                            _ => d
                        };
                    }
                    else result += d;
                }
            }
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion

    [ValueConversion(typeof(int), typeof(string))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class IntToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;
            if (value is double d) result = (int)Math.Round(d, 0);
            else if (value is int i) result = i;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value.ToString() ?? "0");
        }
    }
    [ValueConversion(typeof(double), typeof(string))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString() ?? "0");
        }
    }
    [ValueConversion(typeof(bool), typeof(bool))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class BoolReverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
    }

    [ValueConversion(typeof(Thickness), typeof(double))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ThicknessToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            if (parameter is not null && parameter is not null) 
            {
                string?[]? strs = parameter.ToString()?.Split('_');
                if (strs is null || strs.Length < 1) return thickness.Left;
                if (strs.Length < 2 || !double.TryParse(strs[1], out double rate)) rate = 1;
                return strs[0] switch
                {
                    "1" => thickness.Top * rate,
                    "2" => thickness.Right * rate,
                    "3" => thickness.Bottom * rate,
                    "Max" => Math.Max(Math.Max(thickness.Left, thickness.Right), Math.Max(thickness.Top, thickness.Bottom)) * rate,
                    "Min" => Math.Min(Math.Min(thickness.Left, thickness.Right), Math.Min(thickness.Top, thickness.Bottom)) * rate,
                    "Avg" => (thickness.Left + thickness.Right + thickness.Top + thickness.Bottom) / 4d * rate,
                    "Sum" => (thickness.Left + thickness.Right + thickness.Top + thickness.Bottom) * rate,
                    _ => thickness.Left * rate,
                };
            }
            else return thickness.Left;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Phrase = (string)value;
            string[] strings = Phrase.Split(Const.Sign_SplitPhrase);
            List<int> ints = [];
            foreach (string str in strings)
            {
                if (ints.Count > 4) break;
                if (int.TryParse(str, out int v))
                {
                    ints.Add(v);
                }
                else ints.Add(0);
            }
            while (ints.Count < 4)
            {
                ints.Add(0);
            }
            return new Thickness(ints[0], ints[1], ints[2], ints[3]);
        }
    }
    [ValueConversion(typeof(CornerRadius), typeof(double))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class CornerRadiusToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not CornerRadius cr) cr = new(0);
            if (parameter is not null && parameter is not null)
            {
                string?[]? strs = parameter.ToString()?.Split('_');
                if (strs is null || strs.Length < 1) return cr.TopLeft;
                if (strs.Length < 2 || !double.TryParse(strs[1], out double rate)) rate = 1;
                return strs[0] switch
                {
                    "1" => cr.TopRight * rate,
                    "2" => cr.BottomRight * rate,
                    "3" => cr.BottomLeft * rate,
                    "Max" => Math.Max(Math.Max(cr.TopLeft, cr.TopRight), Math.Max(cr.BottomRight, cr.BottomLeft)) * rate,
                    "Min" => Math.Min(Math.Min(cr.TopLeft, cr.TopRight), Math.Min(cr.BottomRight, cr.BottomLeft)) * rate,
                    "Avg" => (cr.TopLeft + cr.TopRight + cr.BottomRight + cr.BottomLeft) / 4d * rate,
                    "Sum" => (cr.TopLeft + cr.TopRight + cr.BottomRight + cr.BottomLeft) * rate,
                    _ => cr.TopLeft * rate,
                };
            }
            else return cr.TopLeft;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d) d = 0;
            return new CornerRadius(d);
        }
    }
    [ValueConversion(typeof(double), typeof(Thickness))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleToThickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseValue = 0;
            string[] parameters = parameter.ToString()?.Split('_') ?? ["0",];
            if (value is double d) baseValue = d;
            Thickness result = new(baseValue);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Contains('L'))
                {
                    result.Left = double.TryParse(parameters[i].Replace("L", string.Empty), out double rate) ? rate * result.Left : result.Left;
                }
                else if (parameters[i].Contains('T'))
                {
                    result.Top = double.TryParse(parameters[i].Replace("T", string.Empty), out double rate) ? rate * result.Top : result.Top;
                }
                else if (parameters[i].Contains('R'))
                {
                    result.Right = double.TryParse(parameters[i].Replace("R", string.Empty), out double rate) ? rate * result.Right : result.Right;
                }
                else if (parameters[i].Contains('B'))
                {
                    result.Bottom = double.TryParse(parameters[i].Replace("B", string.Empty), out double rate) ? rate * result.Bottom : result.Bottom;
                }
            }
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => 0;
    }
    [ValueConversion(typeof(double), typeof(Thickness))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class DoubleToCornerRadius : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseValue = 0;
            string[] parameters = parameter.ToString()?.Split('_') ?? ["0",];
            if (value is double d) baseValue = d;
            CornerRadius result = new(baseValue);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Contains('L'))
                {
                    result.TopLeft = double.TryParse(parameters[i].Replace("L", string.Empty), out double rate) ? rate * result.TopLeft : result.TopLeft;
                }
                else if (parameters[i].Contains('T'))
                {
                    result.TopRight = double.TryParse(parameters[i].Replace("T", string.Empty), out double rate) ? rate * result.TopRight : result.TopRight;
                }
                else if (parameters[i].Contains('R'))
                {
                    result.BottomRight = double.TryParse(parameters[i].Replace("R", string.Empty), out double rate) ? rate * result.BottomRight : result.BottomRight;
                }
                else if (parameters[i].Contains('B'))
                {
                    result.BottomLeft = double.TryParse(parameters[i].Replace("B", string.Empty), out double rate) ? rate * result.BottomLeft : result.BottomLeft;
                }
            }
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => 0;
    }

    [ValueConversion(typeof(string), typeof(string))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class NameToTitle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            if (name != string.Empty)
            {
                List<char> newChars = [];
                List<char> nameChars = [.. name];
                newChars.Add(nameChars[0]); nameChars.RemoveAt(0);
                foreach (char c in nameChars)
                {
                    if (char.IsUpper(c)) newChars.Add(' ');
                    newChars.Add(c);
                }
                name = new([.. newChars]);
            }
            return name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Replace(" ", string.Empty);
        }
    }
    [ValueConversion(typeof(bool), typeof(Visibility))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility v; bool b = (bool)value;
            bool parameterHasNull = parameter is null || parameter is not string s || s.Contains("null", StringComparison.CurrentCultureIgnoreCase);
            bool parameterReverse = parameter is not null && parameter is string s1 && s1.Contains("reverse", StringComparison.CurrentCultureIgnoreCase);
            if (parameterReverse) b = !b;
            v = b ? Visibility.Visible : parameterHasNull ? Visibility.Hidden : Visibility.Collapsed;
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (Visibility)value == Visibility.Visible;
    }
    [ValueConversion(typeof(object), typeof(Visibility))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class IsNullToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not null ? Visibility.Visible : parameter is null ? Visibility.Hidden : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (Visibility)value == Visibility.Visible;
    }
    [ValueConversion(typeof(object), typeof(bool))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class IsNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new();
    }

    [ValueConversion(typeof(string), typeof(string))]
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ElShrineToStringBase : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? str = value.ToString();
            if (str == null || str.Replace(Const.Sign_Void, string.Empty) == string.Empty) str = Const.Replace_Void;
            else str = str.Replace(Const.Sign_Void, string.Empty);
            return str;
        }
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? result = value.ToString();
            if (result == null || result == Const.Replace_Void || result.Replace(Const.Sign_Void, string.Empty) == string.Empty) result = Const.Sign_Void;
            else result = result.Replace(Const.Sign_Void, string.Empty);
            return result;
        }
    }

    [Obsolete(ObsoleteMsg.OldNamespaceMsg)] public class ColorToARGB : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Color resultColor = System.Drawing.Color.Transparent.ToMediaColor();
            List<byte> ints = [];
            foreach(object v in values)
            {
                if(v is byte b)ints.Insert(0, b);
                else ints.Insert(0, 255);
            }
            if (ints.Count >= 4)
            {
                resultColor = Color.FromArgb(ints[0], ints[1], ints[2], ints[3]);
            }
            return resultColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return [c.A, c.R, c.G, c.B];
        }
    }
}
