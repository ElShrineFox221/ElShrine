using ElShrine.Wpf.UITheme;
using System.Windows;

namespace ElShrine.Wpf.Controls.Extensions
{
    public static class TransHelper
    {
        public static void GetThemeControlParent(object? sender, out FrameworkElement? element, out IThemeControlBase? themeControl) 
        {
            element = sender as FrameworkElement;
            themeControl = element as IThemeControlBase;
            if(themeControl is null && element is not null)
            {
                themeControl = element.FindVisualParent(p => p is IThemeControlBase) as IThemeControlBase;
            }
        }
    }
}
