using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls.Extensions;

public static class TransHelper
{
    public static void GetThemeControlParent(
        object? sender,
        out FrameworkElement? element,
        out IThemeControlBase? themeControl) 
    {
        element = sender as FrameworkElement;
        themeControl = element as IThemeControlBase;
        if(themeControl is null && element is not null)
        {
            themeControl = element.FindParent(p => p is IThemeControlBase) as IThemeControlBase;
        }
    }
    public static DoubleAnimation ToDoubleAnimation(
        this IThemeControlBase? animParaSource, double tarValue, bool isIn,
        double defaultSeconds = Constants.DefaultAnimationDuration, IEasingFunction? defaultEaseFunc = null)
        => new(tarValue, TimeSpan.FromSeconds((isIn? animParaSource?.AnimaDurationIn:animParaSource?.AnimaDurationOut)??defaultSeconds))
        {
            EasingFunction = animParaSource?.AnimaEaseFunc ?? defaultEaseFunc,
        };
    public static void CoerceValue<T>(T control) where T : DependencyObject, IThemeControlBase
    {
        control.CoerceValue(ThemeProperties.SecondaryBrushProperty);
        control.CoerceValue(ThemeProperties.PrimaryBrushProperty);
        control.CoerceValue(ThemeProperties.FontBrushProperty);
        control.CoerceValue(ThemeProperties.BackBrushProperty);
        control.CoerceValue(ThemeProperties.AnimaDurationInProperty);
        control.CoerceValue(ThemeProperties.AnimaDurationOutProperty);
    }
}
