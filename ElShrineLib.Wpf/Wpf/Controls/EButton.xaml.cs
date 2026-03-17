using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EButton : Button, IThemeControlBase
{
    #region DPs
    public bool IsClickShakable
    {
        get => (bool)GetValue(IsClickShakableProperty);
        set => SetValue(IsClickShakableProperty, value);
    }
    public double ClickShakeScale
    {
        get => (double)GetValue(ClickShakeScaleProperty);
        set => SetValue(ClickShakeScaleProperty, value);
    }

    public static readonly DependencyProperty IsClickShakableProperty = DependencyProperty.Register(nameof(IsClickShakable), typeof(bool), typeof(EButton), new(true));
    public static readonly DependencyProperty ClickShakeScaleProperty = DependencyProperty.Register(nameof(ClickShakeScale), typeof(double), typeof(EButton), new(0.9d));
    #endregion

    #region Implements
    static EButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EButton), new FrameworkPropertyMetadata(typeof(EButton)));
    public EButton() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
