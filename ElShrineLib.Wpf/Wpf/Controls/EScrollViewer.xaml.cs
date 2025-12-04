using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public enum ScrollBarBtnVisibility
    {
        None = 0,
        End = 1,
        Page = 2,
        All = 3,
    }
    public class EScrollViewer : ScrollViewer, IThemeControlOld
    {
        public EScrollBar? VerticalScrollBar { get; protected set; }
        public EScrollBar? HorizontalScrollBar { get; protected set; }

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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius),  typeof(CornerRadius), typeof(EScrollViewer), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush),  typeof(Brush), typeof(EScrollViewer), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush),  typeof(Brush), typeof(EScrollViewer), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush),  typeof(Brush), typeof(EScrollViewer), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public object CornerContent
        {
            get => GetValue(CornerContentProperty);
            set => SetValue(CornerContentProperty, value);
        }
        public ScrollBarBtnVisibility VerticalScrollBarBtnVisibility
        {
            get => (ScrollBarBtnVisibility)GetValue(VerticalScrollBarBtnVisibilityProperty);
            set => SetValue(VerticalScrollBarBtnVisibilityProperty, value);
        }
        public ScrollBarBtnVisibility HorizontalScrollBarBtnVisibility
        {
            get => (ScrollBarBtnVisibility)GetValue(HorizontalScrollBarBtnVisibilityProperty);
            set => SetValue(HorizontalScrollBarBtnVisibilityProperty, value);
        }
        public double VerticalScrollBarWidth
        {
            get => (double)GetValue(VerticalScrollBarWidthProperty);
            set => SetValue(VerticalScrollBarWidthProperty, value);
        }
        public double HorizontalScrollBarHeight
        {
            get => (double)GetValue(HorizontalScrollBarHeightProperty);
            set => SetValue(HorizontalScrollBarHeightProperty, value);
        }
        public Thickness VerticalScrollBarMargin
        {
            get => (Thickness)GetValue(VerticalScrollBarMarginProperty);
            set => SetValue(VerticalScrollBarMarginProperty, value);
        }
        public Thickness HorizontalScrollBarMargin
        {
            get => (Thickness)GetValue(HorizontalScrollBarMarginProperty);
            set => SetValue(HorizontalScrollBarMarginProperty, value);
        }

        public static readonly DependencyProperty CornerContentProperty = DependencyProperty.Register(nameof(CornerContent),  typeof(object), typeof(EScrollViewer), new(null));
        public static readonly DependencyProperty VerticalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarBtnVisibility),  typeof(ScrollBarBtnVisibility), typeof(EScrollViewer), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty HorizontalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarBtnVisibility),  typeof(ScrollBarBtnVisibility), typeof(EScrollViewer), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty VerticalScrollBarWidthProperty = DependencyProperty.Register(nameof(VerticalScrollBarWidth),  typeof(double), typeof(EScrollViewer), new(10d));
        public static readonly DependencyProperty HorizontalScrollBarHeightProperty = DependencyProperty.Register(nameof(HorizontalScrollBarHeight),  typeof(double), typeof(EScrollViewer), new(10d));
        public static readonly DependencyProperty VerticalScrollBarMarginProperty = DependencyProperty.Register(nameof(VerticalScrollBarMargin),  typeof(Thickness), typeof(EScrollViewer), new(new Thickness(0)));
        public static readonly DependencyProperty HorizontalScrollBarMarginProperty = DependencyProperty.Register(nameof(HorizontalScrollBarMargin),  typeof(Thickness), typeof(EScrollViewer), new(new Thickness(0)));
        #endregion

        static EScrollViewer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollViewer), new FrameworkPropertyMetadata(typeof(EScrollViewer)));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            HorizontalScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as EScrollBar;
            VerticalScrollBar = GetTemplateChild("PART_VerticalScrollBar") as EScrollBar;
        }
    }
}
