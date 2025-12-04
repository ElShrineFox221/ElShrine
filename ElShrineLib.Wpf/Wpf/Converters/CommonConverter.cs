using System;
using System.Globalization;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    public class CommonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }

        public static TValue? Convert<TValue>(object? o, TValue? defaultValue = default(TValue))
        {
            var valueType = ValueType.Other;
            if(typeof(double).IsAssignableFrom(typeof(TValue))) valueType = ValueType.Number;
            return defaultValue;
        }
        private enum ValueType
        {
            Other,
            Number,
            String,
            Color,
            Boolean,
            Quadruple,
            Flags,
        }
    }
}
