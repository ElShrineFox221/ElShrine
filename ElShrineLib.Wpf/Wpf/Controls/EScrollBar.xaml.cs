using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class EScrollBar : ScrollBar, IThemeControlOld
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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register("BorderCornerRadius", typeof(CornerRadius), typeof(EScrollBar), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register("FontBrush", typeof(Brush), typeof(EScrollBar), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(EScrollBar), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register("ClickBrush", typeof(Brush), typeof(EScrollBar), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public bool IsStartEndButtonVisibile
        {
            get => (bool)GetValue(IsStartEndButtonVisibileProperty);
            set => SetValue(IsStartEndButtonVisibileProperty, value);
        }
        public bool IsMoveButtonVisibile
        {
            get => (bool)GetValue(IsMoveButtonVisibileProperty);
            set => SetValue(IsMoveButtonVisibileProperty, value);
        }

        public static readonly DependencyProperty IsStartEndButtonVisibileProperty = DependencyProperty.Register("IsStartEndButtonVisibile", typeof(bool), typeof(EScrollBar), new(true));
        public static readonly DependencyProperty IsMoveButtonVisibileProperty = DependencyProperty.Register("IsMoveButtonVisibile", typeof(bool), typeof(EScrollBar), new(true));
        #endregion

        static EScrollBar() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollBar), new FrameworkPropertyMetadata(typeof(EScrollBar)));
    }
}
