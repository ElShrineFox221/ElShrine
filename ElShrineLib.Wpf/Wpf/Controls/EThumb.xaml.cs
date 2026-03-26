using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EThumb : Thumb, IThemeControlBase
{
    #region Implements
    static EThumb() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EThumb), new FrameworkPropertyMetadata(typeof(EThumb)));
    public EThumb() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
