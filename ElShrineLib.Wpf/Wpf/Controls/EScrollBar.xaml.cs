using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls
{
    public partial class EScrollBar : ScrollBar, IThemeControlBase, IScrollBarControlBase
    {
        #region Implements
        static EScrollBar() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollBar), new FrameworkPropertyMetadata(typeof(EScrollBar)));
        public EScrollBar() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
