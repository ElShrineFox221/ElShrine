using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.Controls;

public enum SliderFillStyle
{
    LightLower, LightUpper, LightAll, None
}
[GenerateDPCli]
public partial class ESlider : Slider, IThemeControlBase, IScaleControllerBase
{
    #region DPs

    #region Slider
    public double SliderTrackSize
    {
        get => (double)GetValue(SliderTrackSizeProperty);
        set => SetValue(SliderTrackSizeProperty, value);
    }
    public SliderFillStyle SliderFillStyle
    {
        get => (SliderFillStyle)GetValue(SliderFillStyleProperty);
        set => SetValue(SliderFillStyleProperty, value);
    }
    public Brush FillBack
    {
        get => (Brush)GetValue(FillBackProperty);
        set => SetValue(FillBackProperty, value);
    }
    public Brush FillMiddle
    {
        get => (Brush)GetValue(FillMiddleProperty);
        set => SetValue(FillMiddleProperty, value);
    }
    public Brush FillFore
    {
        get => (Brush)(GetValue(FillForeProperty));
        set => SetValue(FillForeProperty, value);
    }
    public static readonly DependencyProperty SliderTrackSizeProperty = DependencyProperty.Register(nameof(SliderTrackSize), typeof(double), typeof(ESlider), new PropertyMetadata(15d));
    public static readonly DependencyProperty SliderFillStyleProperty = DependencyProperty.Register(nameof(SliderFillStyle), typeof(SliderFillStyle), typeof(ESlider), new(SliderFillStyle.LightLower));
    public static readonly DependencyProperty FillBackProperty = DependencyProperty.Register(nameof(FillBack), typeof(Brush), typeof(ESlider), new(new SolidColorBrush(Color.Transparent.ToMediaColor())));
    public static readonly DependencyProperty FillMiddleProperty = DependencyProperty.Register(nameof(FillMiddle), typeof(Brush), typeof(ESlider), new(new SolidColorBrush(Color.Transparent.ToMediaColor())));
    public static readonly DependencyProperty FillForeProperty = DependencyProperty.Register(nameof(FillFore), typeof(Brush), typeof(ESlider), new(new SolidColorBrush(Color.Transparent.ToMediaColor())));
    #endregion

    #region Thumb
    public object ThumbContent
    {
        get => GetValue(ThumbContentProperty);
        set => SetValue(ThumbContentProperty, value);
    }
    public ControlTemplate ThumbTemplate
    {
        get => (ControlTemplate)GetValue(ThumbTemplateProperty);
        set => SetValue(ThumbTemplateProperty, value);
    }
    public static readonly DependencyProperty ThumbContentProperty = DependencyProperty.Register(nameof(ThumbContent), typeof(object), typeof(ESlider), new(null));
    public static readonly DependencyProperty ThumbTemplateProperty = DependencyProperty.Register(nameof(ThumbTemplate), typeof(ControlTemplate), typeof(ESlider), new(null));
    #endregion

    #endregion

    #region Implements
    static ESlider() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ESlider), new FrameworkPropertyMetadata(typeof(ESlider)));
    public ESlider() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
