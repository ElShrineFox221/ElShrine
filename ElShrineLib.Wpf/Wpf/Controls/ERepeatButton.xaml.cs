using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls
{
    public partial class ERepeatButton : RepeatButton, IThemeControlBase
    {
        #region Implements
        static ERepeatButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ERepeatButton), new FrameworkPropertyMetadata(typeof(ERepeatButton)));
        public ERepeatButton() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
