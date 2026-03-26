using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EComboBox : ComboBox, IThemeControlBase, IItemRenderControlBase, ISelectionRenderControlBase
{
    #region DPs

    public Thickness ArrowMargin
    {
        get => (Thickness)GetValue(ArrowMarginProperty);
        set => SetValue(ArrowMarginProperty, value);
    }

    public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register(nameof(ArrowMargin),  typeof(Thickness), typeof(EComboBox), new(new Thickness(0)));
    #endregion

    #region Implements
    static EComboBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EComboBox), new FrameworkPropertyMetadata(typeof(EComboBox)));
    public EComboBox() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion
}
