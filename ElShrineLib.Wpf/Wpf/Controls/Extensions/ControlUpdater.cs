using System.Windows;

namespace ElShrine.Wpf.Controls.Extensions
{
    public static class ControlUpdater
    {
        public readonly static DependencyProperty InvalidateArrangeSourceProperty = DependencyProperty.RegisterAttached(
            nameof(InvalidateArrangeSourceProperty).ToPropRegName(),
            typeof(object),
            typeof(ControlUpdater),
            new FrameworkPropertyMetadata(null, propertyChangedCallback: (s, e) =>
            {
                if (s is FrameworkElement fe)
                {
                    fe.InvalidateArrange();
                }
            }));
        public static object GetInvalidateArrangeSource(DependencyObject obj) => obj.GetValue(InvalidateArrangeSourceProperty);
        public static void SetInvalidateArrangeSource(DependencyObject obj, object value) => obj.SetValue(InvalidateArrangeSourceProperty, value);
    }
}
