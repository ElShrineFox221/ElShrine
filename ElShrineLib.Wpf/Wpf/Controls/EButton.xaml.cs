using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class EButton : Button, IThemeControl
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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(EButton), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(EButton), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(EButton), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(EButton), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public bool IsClickShakable
        {
            get => (bool)GetValue(IsClickShakableProperty);
            set => SetValue(IsClickShakableProperty, value);
        }

        public static readonly DependencyProperty IsClickShakableProperty = DependencyProperty.Register(nameof(IsClickShakable), typeof(bool), typeof(EButton), new(true));
        #endregion

        static EButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EButton), new FrameworkPropertyMetadata(typeof(EButton)));
    }
}
