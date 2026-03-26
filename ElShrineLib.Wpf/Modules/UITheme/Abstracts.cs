using ElShrine.Wpf.UITheme;
using System.Windows;

namespace ElShrine.Modules.UITheme;

public interface IUIThemeManager
{
    bool RegisterCoerceThemeDPs<T>(T control) where T : DependencyObject, IThemeControlBase;
    Theme CurrentTheme { get; }
}
