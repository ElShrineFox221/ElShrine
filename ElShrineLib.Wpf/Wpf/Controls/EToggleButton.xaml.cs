using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EToggleButton : ToggleButton, IThemeControlBase
    {
        #region DPs
        public double ToggleSignSize
        {
            get => (double)GetValue(ToggleSignSizeProperty);
            set => SetValue(ToggleSignSizeProperty, value);
        }
        public Thickness ToggleSignMargin
        {
            get => (Thickness)GetValue(ToggleSignMarginProperty);
            set => SetValue(ToggleSignMarginProperty, value);
        }
        public ExpandDirection ContentPlacement
        {
            get => (ExpandDirection)GetValue(ContentPlacementProperty);
            set => SetValue(ContentPlacementProperty, value);
        }

        public static readonly DependencyProperty ToggleSignSizeProperty = DependencyProperty.Register(nameof(ToggleSignSize), typeof(double), typeof(EToggleButton), new(14d));
        public static readonly DependencyProperty ToggleSignMarginProperty = DependencyProperty.Register(nameof(ToggleSignMargin), typeof(Thickness), typeof(EToggleButton), new(new Thickness(0)));
        public static readonly DependencyProperty ContentPlacementProperty = DependencyProperty.Register(nameof(ContentPlacement), typeof(ExpandDirection), typeof(EToggleButton), new(ExpandDirection.Right));
        #endregion

        public EToggleButton() => UIThemesManager.RegisterCoerceThemeDPs(this);
        static EToggleButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EToggleButton), new FrameworkPropertyMetadata(typeof(EToggleButton)));

        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
    }
}
