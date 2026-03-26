using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public sealed class ImageSourceCoverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(str))
            {
                var url = new Uri(str);
                return new BitmapImage(url);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
