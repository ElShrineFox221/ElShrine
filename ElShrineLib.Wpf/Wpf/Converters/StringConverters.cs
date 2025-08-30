using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    [ValueConversion(typeof(int), typeof(string))]
    [ValueConversion(typeof(float), typeof(string))]
    [ValueConversion(typeof(double), typeof(string))]

    [ValueConversion(typeof(int), typeof(int))]
    [ValueConversion(typeof(float), typeof(int))]
    [ValueConversion(typeof(double), typeof(int))]
    [ValueConversion(typeof(string), typeof(int))]

    [ValueConversion(typeof(int), typeof(float))]
    [ValueConversion(typeof(float), typeof(float))]
    [ValueConversion(typeof(double), typeof(float))]
    [ValueConversion(typeof(string), typeof(float))]

    [ValueConversion(typeof(int), typeof(double))]
    [ValueConversion(typeof(float), typeof(double))]
    [ValueConversion(typeof(double), typeof(double))]
    [ValueConversion(typeof(string), typeof(double))]
    public class NumToStringConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double numValue = ToDouble(value);
            numValue *= ToDouble(parameter, 1);
            return ToObj(numValue, targetType);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double numValue = ToDouble(value);
            numValue /= ToDouble(parameter, 1);
            return ToObj(numValue, targetType);
        }
        private static double ToDouble(object value, double? defalut = null)
        {
            double numValue;
            if (value is null) numValue = defalut ?? 0;
            else if (value is int i) numValue = i;
            else if (value is float f) numValue = f;
            else if (value is double d) numValue = d;
            else if (value is string s && double.TryParse(s, out var r)) numValue = r;
            else numValue = defalut is not null ? defalut.Value : throw new NotImplementedException();
            return numValue;
        }
        private static object ToObj(double value, Type targetType)
        {
            object obj;
            if (targetType == typeof(int)) obj = (int)value;
            else if (targetType == typeof(float)) obj = (float)value;
            else if (targetType == typeof(double)) obj = value;
            else if (targetType == typeof(string)) obj = value.ToString();
            else throw new NotImplementedException();
            return obj;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dvs = values.Select(v => ToDouble(v));
            var d = ConvertWithInterpolation([.. dvs], parameter);
            return ToObj(d, targetType);
        }
        public static double ConvertWithInterpolation(double[] values, object parameter)
        {
            if (values is null || values.Length == 0) throw new ArgumentException("Values array cannot be null or empty");
            string[] groups = parameter?.ToString()?.Split('_') ?? [];
            if (groups.Length == 0) return values[0];
            double current = values[0];
            int valueIndex = 1;
            foreach (string group in groups)
            {
                string groupStr = group.Trim();
                if (groupStr.IsEmpty()) continue;
                bool reverse = groupStr.TryReplaceIgnoreCase("reverse", out groupStr);
                bool isMax = groupStr.TryReplaceIgnoreCase("max", out groupStr);
                bool isMin = !isMax && groupStr.TryReplaceIgnoreCase("min", out groupStr);
                groupStr = groupStr.Trim();
                if (!int.TryParse(groupStr, out int paramCount) || paramCount <= 0) break;
                if (valueIndex + paramCount >= values.Length) break;
                double[] parameters = new double[paramCount];
                for (int i = 0; i < paramCount; i++)
                {
                    parameters[i] = values[valueIndex + i];
                    if (reverse) parameters[i] = 1 - parameters[i];
                }
                double targetValue = values[valueIndex + paramCount];
                double rate = isMax ? parameters.Max() : isMin ? parameters.Min() : parameters.Aggregate(1.0, (acc, p) => acc * p);
                current = current.Lerp(targetValue, rate);
                valueIndex += paramCount + 1;
                if (valueIndex >= values.Length) break;
            }
            return current;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
