using ElShrine.Graphics;
using ElShrine.Modules;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.UITheme;

public static class ThemeProperties
{
    #region Border base

    public static readonly DependencyProperty EBorderCornerRadiusProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.EBorderCornerRadius),
        typeof(CornerRadius),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: new CornerRadius(0),
            flags: FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnThemePropertyChanged
        )
    );

    public static readonly DependencyProperty EBorderThicknessProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.EBorderThickness),
        typeof(Thickness),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: new Thickness(0),
            flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
            propertyChangedCallback: OnThemePropertyChanged
        )
    );

    #endregion

    #region Color base

    public static readonly DependencyProperty PrimaryBrushProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.PrimaryBrush),
        typeof(Brush),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: null,
            flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeBrushValue(d, v, ThemeProperty.PrimaryBrush)
        )
    );

    public static readonly DependencyProperty FontBrushProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.FontBrush),
        typeof(Brush),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: null,
            flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeBrushValue(d, v, ThemeProperty.FontBrush)
        )
    );

    public static readonly DependencyProperty BackBrushProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.BackBrush),
        typeof(Brush),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: null,
            flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeBrushValue(d, v, ThemeProperty.BackBrush)
        )
    );

    public static readonly DependencyProperty SecondaryBrushProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.SecondaryBrush),
        typeof(Brush),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            null,
            flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeBrushValue(d, v, ThemeProperty.SecondaryBrush)
        )
    );
    private static object? CoerceThemeBrushValue(DependencyObject _, object baseValue, ThemeProperty tp)
    {
        if (baseValue is Brush brush && brush is not null) return baseValue;
        var theme = WpfModuleAccessor.UITheme.CurrentTheme;
        var color = tp switch
        {
            ThemeProperty.PrimaryBrush => theme.PrimaryColor,
            ThemeProperty.BackBrush => theme.BackColor,
            ThemeProperty.FontBrush => theme.FontColor,
            ThemeProperty.SecondaryBrush => theme.SecondaryColor,
            _ => ColorData.FromData(0x00000000)
        };
        return new SolidColorBrush(color.ToMediaColor());
    }

    public static object GetPrimaryBrush(DependencyObject obj) => (Brush)obj.GetValue(PrimaryBrushProperty);
    public static void SetPrimaryBrush(DependencyObject obj, Brush value) => obj.SetValue(PrimaryBrushProperty, value);
    public static object GetFontBrush(DependencyObject obj) => (Brush)obj.GetValue(FontBrushProperty);
    public static void SetFontBrush(DependencyObject obj, Brush value) => obj.SetValue(FontBrushProperty, value);
    public static object GetBackBrush(DependencyObject obj) => (Brush)obj.GetValue(BackBrushProperty);
    public static void SetBackBrush(DependencyObject obj, Brush value) => obj.SetValue(BackBrushProperty, value);
    public static object GetSecondaryBrush(DependencyObject obj) => (Brush)obj.GetValue(SecondaryBrushProperty);
    public static void SetSecondaryBrush(DependencyObject obj, Brush value) => obj.SetValue(SecondaryBrushProperty, value);
    #endregion

    #region Anima base

    public static readonly DependencyProperty AnimaDurationInProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.AnimaDurationIn),
        typeof(double),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: double.NaN,
            flags: FrameworkPropertyMetadataOptions.Inherits,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeAnimParamValue(d, v, ThemeProperty.AnimaDurationIn)
        )
    );

    public static readonly DependencyProperty AnimaDurationOutProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.AnimaDurationOut),
        typeof(double),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: double.NaN,
            flags: FrameworkPropertyMetadataOptions.Inherits,
            propertyChangedCallback: OnThemePropertyChanged,
            coerceValueCallback: (d, v) => CoerceThemeAnimParamValue(d, v, ThemeProperty.AnimaDurationOut)
        )
    );

    public static readonly DependencyProperty AnimaEaseFuncProperty = DependencyProperty.RegisterAttached(
        nameof(IThemeControlBase.AnimaEaseFunc),
        typeof(EasingFunctionBase),
        typeof(UIElement),
        new FrameworkPropertyMetadata(
            defaultValue: new SineEase() { EasingMode = EasingMode.EaseInOut },
            flags: FrameworkPropertyMetadataOptions.Inherits,
            propertyChangedCallback: OnThemePropertyChanged
        )
    );

    private static object? CoerceThemeAnimParamValue(DependencyObject _, object baseValue, ThemeProperty tp)
    {
        if (baseValue is double d && !double.IsNaN(d)) return baseValue;
        var theme = WpfModuleAccessor.UITheme.CurrentTheme;
        var value = tp switch
        {
            ThemeProperty.AnimaDurationIn => theme.AnimDurationIn,
            ThemeProperty.AnimaDurationOut => theme.AnimDurationOut,
            _ => Theme.ANIMA_DEFAULTDURA
        };
        return value;
    }
    #endregion

    private static void OnThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IThemeControlBase themeControl) themeControl.LocalThemePorpertyChanged(e);
    }
}