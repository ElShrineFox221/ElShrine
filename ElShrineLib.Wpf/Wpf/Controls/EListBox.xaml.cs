using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class EListBox : ListBox, IThemeControl
    {
        public EListBox? ScrollViewer { get; protected set; }

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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(EListBox), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(EListBox), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(EListBox), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(EListBox), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public double SelectedItemLineWidth
        {
            get => (double)GetValue(SelectedItemLineWidthProperty);
            set => SetValue(SelectedItemLineWidthProperty, value);
        }
        public double SelectedItemLineHeightRate
        {
            get => (double)GetValue(SelectedItemLineHeightRateProperty);
            set => SetValue(SelectedItemLineHeightRateProperty, value);
        }
        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }
        public Thickness ItemPadding
        {
            get => (Thickness)GetValue(ItemPaddingProperty);
            set => SetValue(ItemPaddingProperty, value);
        }
        public Thickness ItemBorderThickness
        {
            get => (Thickness)GetValue(ItemBorderThicknessProperty);
            set => SetValue(ItemBorderThicknessProperty, value);
        }
        public CornerRadius ItemBorderCornerRadius
        {
            get => (CornerRadius)GetValue(ItemBorderCornerRadiusProperty);
            set => SetValue(ItemBorderCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SelectedItemLineWidthProperty = DependencyProperty.Register(nameof(SelectedItemLineWidth), typeof(double), typeof(EListBox), new(4d));
        public static readonly DependencyProperty SelectedItemLineHeightRateProperty = DependencyProperty.Register(nameof(SelectedItemLineHeightRate), typeof(double), typeof(EListBox), new(0.8d));
        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(EListBox), new(new Thickness(2)));
        public static readonly DependencyProperty ItemPaddingProperty = DependencyProperty.Register(nameof(ItemPadding), typeof(Thickness), typeof(EListBox), new(new Thickness(4)));
        public static readonly DependencyProperty ItemBorderThicknessProperty = DependencyProperty.Register(nameof(ItemBorderThickness), typeof(Thickness), typeof(EListBox), new(new Thickness(1)));
        public static readonly DependencyProperty ItemBorderCornerRadiusProperty = DependencyProperty.Register(nameof(ItemBorderCornerRadius), typeof(CornerRadius), typeof(EListBox), new(Theme.Default.CornerRadius));

        #region ScrollBar

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
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
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

        public static readonly DependencyProperty CornerContentProperty = DependencyProperty.Register(nameof(CornerContent), typeof(object), typeof(EListBox), new(null));
        public static readonly DependencyProperty VerticalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarBtnVisibility), typeof(ScrollBarBtnVisibility), typeof(EListBox), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty HorizontalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarBtnVisibility), typeof(ScrollBarBtnVisibility), typeof(EListBox), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(EListBox), new(ScrollBarVisibility.Auto));
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(EListBox), new(ScrollBarVisibility.Auto));
        public static readonly DependencyProperty VerticalScrollBarWidthProperty = DependencyProperty.Register(nameof(VerticalScrollBarWidth), typeof(double), typeof(EListBox), new(10d));
        public static readonly DependencyProperty HorizontalScrollBarHeightProperty = DependencyProperty.Register(nameof(HorizontalScrollBarHeight), typeof(double), typeof(EListBox), new(10d));
        public static readonly DependencyProperty VerticalScrollBarMarginProperty = DependencyProperty.Register(nameof(VerticalScrollBarMargin), typeof(Thickness), typeof(EListBox), new(new Thickness(0)));
        public static readonly DependencyProperty HorizontalScrollBarMarginProperty = DependencyProperty.Register(nameof(HorizontalScrollBarMargin), typeof(Thickness), typeof(EListBox), new(new Thickness(0)));

        #endregion

        #endregion

        static EListBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EListBox), new FrameworkPropertyMetadata(typeof(EListBox)));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ScrollViewer = GetTemplateChild("contentContainer") as EListBox;
        }
    }
}
