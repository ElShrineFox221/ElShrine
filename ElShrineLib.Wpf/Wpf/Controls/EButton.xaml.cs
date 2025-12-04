using ElShrine.Common;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    public partial class EButton : Button, IThemeControlBase
    {
        #region DPs
        public bool IsClickShakable
        {
            get => (bool)GetValue(IsClickShakableProperty);
            set => SetValue(IsClickShakableProperty, value);
        }

        public static readonly DependencyProperty IsClickShakableProperty = DependencyProperty.Register(nameof(IsClickShakable), typeof(bool), typeof(EButton), new(true));
        #endregion

        static EButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EButton), new FrameworkPropertyMetadata(typeof(EButton)));
        public EButton() => ThemeManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => ThemeManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) { }
    }
}
