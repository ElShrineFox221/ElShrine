using ElShrine.Old.Wpf.Converters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility v; bool b = (bool)value;
            bool parameterHasNull = parameter is null || parameter is not string s || (!s.ContainsIgnoreCase("collapsed") && s.ContainsIgnoreCase("null"));
            bool parameterReverse = parameter is not null && parameter is string s1 && s1.ContainsIgnoreCase("reverse");
            if (parameterReverse) b = !b;
            v = b ? Visibility.Visible : parameterHasNull ? Visibility.Hidden : Visibility.Collapsed;
            return v;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool parameterReverse = parameter is not null && parameter is string s1 && s1.ContainsIgnoreCase("reverse");
            bool result = ((Visibility)value != Visibility.Visible) ^ parameterReverse;
            return result;
        }
    }
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class IsNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not null ? Visibility.Visible : parameter is null ? Visibility.Hidden : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new($"{nameof(IsNullToVisibilityConverter)} cannot convert visibility to unkonwn object.");
    }
}
