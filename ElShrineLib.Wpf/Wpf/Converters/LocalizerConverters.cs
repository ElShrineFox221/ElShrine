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
            var text = string.Empty;
            var paramStr = parameter?.ToString(); 
            if (value is Enum enumValue)
            {
                string key = enumValue.ToString();
                text = string.IsNullOrWhiteSpace(paramStr) ? key.Translate() : key.Translate(paramStr);
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
        {
            var paramStr = parameter?.ToString();
            var key = value?.ToString() ?? string.Empty;
            return string.IsNullOrWhiteSpace(paramStr) ? key.Translate() : key.Translate(paramStr);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
