using ElShrine.Common;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    public sealed class EnumTextLocalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = string.Empty;
            if (value is Enum enumValue)
            {
                string key = enumValue.ToString();
                text = key.Translate();
            }
            return text;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enums = Enum.GetValues(targetType);
            var valueStr = value?.ToString();
            foreach (var enumValue in enums)
            {
                if (enumValue?.ToString() == valueStr && enumValue is not null) return enumValue;
            }
            return Binding.DoNothing;
        }
    }
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class LocalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString()?.Translate() ?? string.Empty;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
