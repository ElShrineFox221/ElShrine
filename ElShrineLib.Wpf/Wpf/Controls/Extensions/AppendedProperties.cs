using System.Windows;

namespace ElShrine.Wpf.Controls.Extensions
{
    public static class AppendedProperties
    {
        public readonly static DependencyProperty ExtDouble0Property = DependencyProperty.RegisterAttached(nameof(ExtDouble0Property).ToPropRegName(), typeof(double), typeof(AppendedProperties), new(0d));
        public static readonly DependencyProperty ExtDouble1Property = DependencyProperty.RegisterAttached(nameof(ExtDouble1Property).ToPropRegName(), typeof(double), typeof(AppendedProperties), new(0d));
        public static void SetExtDouble0(DependencyObject element, double value) => element.SetValue(ExtDouble0Property, value);
        public static void SetExtDouble1(DependencyObject element, double value) => element.SetValue(ExtDouble1Property, value);
        public static double GetExtDouble0(DependencyObject element) => (double)element.GetValue(ExtDouble0Property);
        public static double GetExtDouble1(DependencyObject element) => (double)element.GetValue(ExtDouble1Property);
    }
}
