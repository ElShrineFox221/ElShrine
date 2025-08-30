using System.Windows;

namespace ElShrine.Old.Wpf.Controls.Extensions
{
    public class RateExtension : DependencyObject
    {
        public static double GetForeColorRate(DependencyObject obj)
            =>(double) obj.GetValue(ForeColorRateProperty);
        public static void SetForeColorRate(DependencyObject obj, double value)
            => obj.SetValue(ForeColorRateProperty, value);
        public static readonly DependencyProperty ForeColorRateProperty =
            DependencyProperty.RegisterAttached("ForeColorRate", typeof(double), typeof(RateExtension), new PropertyMetadata(0d));

        public static double GetBackColorRate(DependencyObject obj)
            => (double)obj.GetValue(BackColorRateProperty);
        public static void SetBackColorRate(DependencyObject obj, double value)
            => obj.SetValue(BackColorRateProperty, value);
        public static readonly DependencyProperty BackColorRateProperty =
            DependencyProperty.RegisterAttached("BackColorRate", typeof(double), typeof(RateExtension), new PropertyMetadata(0d));

        public static double GetUnderLineRate(DependencyObject obj)
            => (double)obj.GetValue(UnderLineRateProperty);
        public static void SetUnderLineRate(DependencyObject obj, double value)
            => obj.SetValue(UnderLineRateProperty, value);
        public static readonly DependencyProperty UnderLineRateProperty =
            DependencyProperty.RegisterAttached("UnderLineRate", typeof(double), typeof(RateExtension), new PropertyMetadata(0d));
    }
    public class TexxExtension : DependencyObject
    {
        public static string GetTitleText(DependencyObject obj)
            => (string)obj.GetValue(TitleTextProperty);
        public static void SetTitleText(DependencyObject obj, string value)
            => obj.SetValue(TitleTextProperty, value);
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.RegisterAttached("TitleText", typeof(string), typeof(TexxExtension), new PropertyMetadata(string.Empty));
    }
}
