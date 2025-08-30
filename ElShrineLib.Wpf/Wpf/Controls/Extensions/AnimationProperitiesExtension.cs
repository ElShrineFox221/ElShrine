using System.Windows;

namespace ElShrine.Wpf.Controls.Extensions
{
    public class AnimationProperitiesExtension : DependencyObject
    {
        //Fore rate -> borderBrush, foreground
        public static double GetForeColorRate(DependencyObject obj)
            => (double)obj.GetValue(ForeColorRateProperty);
        public static void SetForeColorRate(DependencyObject obj, double value)
            => obj.SetValue(ForeColorRateProperty, value);
        public static readonly DependencyProperty ForeColorRateProperty =
            DependencyProperty.RegisterAttached("ForeColorRate", typeof(double), typeof(AnimationProperitiesExtension), new PropertyMetadata(0d));

        public static double GetBackColorRate(DependencyObject obj)
            => (double)obj.GetValue(BackColorRateProperty);
        public static void SetBackColorRate(DependencyObject obj, double value)
            => obj.SetValue(BackColorRateProperty, value);
        public static readonly DependencyProperty BackColorRateProperty =
            DependencyProperty.RegisterAttached("BackColorRate", typeof(double), typeof(AnimationProperitiesExtension), new PropertyMetadata(0d));

        public static double GetFontColorRate(DependencyObject obj)
            => (double)obj.GetValue(FontColorRateProperty);
        public static void SetFontColorRate(DependencyObject obj, double value)
            => obj.SetValue(FontColorRateProperty, value);
        public static readonly DependencyProperty FontColorRateProperty =
            DependencyProperty.RegisterAttached("FontColorRate", typeof(double), typeof(AnimationProperitiesExtension), new PropertyMetadata(0d));
    }
}
