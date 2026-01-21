using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EScrollViewer : ScrollViewer, IThemeControlBase, IScrollBarControlBase, IScrollBarControllerBase
    {
        #region Implements
        static EScrollViewer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollViewer), new FrameworkPropertyMetadata(typeof(EScrollViewer)));
        public EScrollViewer() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
