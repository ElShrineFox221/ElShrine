using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class EToggleButton : ToggleButton, IThemeControlOld
    {

        #region DPs

        #region Theme
        public CornerRadius BorderCornerRadius
        {
            get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
            set => SetValue(BorderCornerRadiusProperty, value);
        }
        public Brush FontBrush
        {
            get => (Brush)GetValue(FontBrushProperty);
            set => SetValue(FontBrushProperty, value);
        }
        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }
        public Brush ClickBrush
        {
            get => (Brush)GetValue(ClickBrushProperty);
            set => SetValue(ClickBrushProperty, value);
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(EToggleButton), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(EToggleButton), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(EToggleButton), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(EToggleButton), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

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

        static EToggleButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EToggleButton), new FrameworkPropertyMetadata(typeof(EToggleButton)));
    }
}
