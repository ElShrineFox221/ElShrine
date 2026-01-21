using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EThumb : Thumb, IThemeControlBase
    {
        #region Implements
        static EThumb() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EThumb), new FrameworkPropertyMetadata(typeof(EThumb)));
        public EThumb() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
