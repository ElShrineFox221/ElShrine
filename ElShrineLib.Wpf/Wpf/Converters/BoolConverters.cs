using System;
using System.Globalization;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !(bool)value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => !(bool)value;
    }
}
