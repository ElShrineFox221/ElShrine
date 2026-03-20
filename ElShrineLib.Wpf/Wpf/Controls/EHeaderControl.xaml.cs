using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EHeaderControl : ContentControl, IThemeControlBase, IHeaderControlBase
{
    #region Implements
    static EHeaderControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EHeaderControl), new FrameworkPropertyMetadata(typeof(EHeaderControl)));
    public EHeaderControl() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
