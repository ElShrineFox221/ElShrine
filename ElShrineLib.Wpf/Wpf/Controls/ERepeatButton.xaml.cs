using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls;

public partial class ERepeatButton : RepeatButton, IThemeControlBase
{
    #region Implements
    static ERepeatButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ERepeatButton), new FrameworkPropertyMetadata(typeof(ERepeatButton)));
    public ERepeatButton() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
