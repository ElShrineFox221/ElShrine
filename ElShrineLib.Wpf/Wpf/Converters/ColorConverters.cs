using ElShrine.EGraphic;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(MediaColor), typeof(SolidColorBrush))]
    [ValueConversion(typeof(MediaColor), typeof(Color))]
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    [ValueConversion(typeof(Color), typeof(MediaColor))]
    [ValueConversion(typeof(SolidColorBrush), typeof(MediaColor))]
    [ValueConversion(typeof(SolidColorBrush), typeof(Color))]
    public class ColorToSolidBrushConverter : IValueConverter
    {
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
            ColorData colorData = ToColor(value);
            object result;
            if (targetType == typeof(ColorData)) result = colorData;
            else if(targetType == typeof(MediaColor)) result = colorData.ToMediaColor();
            else if (targetType == typeof(Color)) result = colorData.ToDrawingColor();
            else if (targetType == typeof(Brush)) result = colorData.ToSolidBrush();
            else throw new("Invalid target type.");
            return result;
        }
        public static ColorData ToColor(object? value)
        {
            ColorData colorData;
            if (value is ColorData cd) colorData = cd; 
            else if (value is Color c) colorData = c.ToColorData();
            else if (value is MediaColor mc) colorData = mc.ToColorData();
            else if (value is SolidColorBrush scb) colorData = scb.Color.ToColorData();
            else colorData = Color.Red.ToColorData();
            return colorData;
        }
    }
    public class ColorLerpToSolidBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var str = parameter?.ToString();
            bool hasReverse = str?.ContainsIgnoreCase("reverse") ?? false;
            bool hasMax = str?.ContainsIgnoreCase("max") ?? false;
            bool hasMin = str?.ContainsIgnoreCase("min") ?? false;
            var alphaState = LerpState.Full;
            if(str?.ContainsIgnoreCase("alpha") ?? false)
            {
                if (str.ContainsIgnoreCase("alpha.to")) alphaState = LerpState.To;
                else if (str.ContainsIgnoreCase("alpha.stay")) alphaState = LerpState.Stay;
                else if (str.ContainsIgnoreCase("alpha.lerp")) alphaState = LerpState.Lerp;
            }
            ColorData? resultColor = null; double rate = double.NaN;
            for (int i = 0; i< values.Length; i++)
            {
                var value = values[i];
                if (double.TryParse(value?.ToString(), out var v))
                {
                    v = hasReverse ? (1 - v) : v;
                    if (double.IsNaN(rate)) rate = v;
                    else if (hasMax) rate = Math.Max(rate, v);
                    else if (hasMin) rate = Math.Min(rate, v);
                    else rate *= v;
                }
                else if (resultColor is null)
                {
                    var color = ColorToSolidBrushConverter.ToColor(value);
                    resultColor = color;
                }
                else if (!double.IsNaN(rate))
                {
                    var color = ColorToSolidBrushConverter.ToColor(value);
                    resultColor = resultColor.Lerp(color, rate, LerpState.Stay, alphaState);
                    rate = double.NaN;
                }
            }
            return new SolidColorBrush(resultColor?.ToMediaColor() ?? throw new());
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
