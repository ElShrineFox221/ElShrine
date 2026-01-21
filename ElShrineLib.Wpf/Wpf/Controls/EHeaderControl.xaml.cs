using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EHeaderControl : ContentControl, IThemeControlBase, IHeaderControlBase
    {
        #region Implements
        static EHeaderControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EHeaderControl), new FrameworkPropertyMetadata(typeof(EHeaderControl)));
        public EHeaderControl() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
