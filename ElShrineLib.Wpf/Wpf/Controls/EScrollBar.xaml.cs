using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls;

public partial class EScrollBar : ScrollBar, IThemeControlBase, IScrollBarControlBase
{
    #region Implements
    static EScrollBar() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollBar), new FrameworkPropertyMetadata(typeof(EScrollBar)));
    public EScrollBar() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
