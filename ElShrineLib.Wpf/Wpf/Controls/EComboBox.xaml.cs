using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class EComboBox : ComboBox, IThemeControl
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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius),  typeof(CornerRadius), typeof(EComboBox), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush),  typeof(Brush), typeof(EComboBox), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush),  typeof(Brush), typeof(EComboBox), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush),  typeof(Brush), typeof(EComboBox), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        #region Items

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

        public static readonly DependencyProperty SelectedItemLineWidthProperty = DependencyProperty.Register(nameof(SelectedItemLineWidth), typeof(double), typeof(EComboBox), new(4d));
        public static readonly DependencyProperty SelectedItemLineHeightRateProperty = DependencyProperty.Register(nameof(SelectedItemLineHeightRate), typeof(double), typeof(EComboBox), new(0.8d));
        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(EComboBox), new(new Thickness(2)));
        public static readonly DependencyProperty ItemPaddingProperty = DependencyProperty.Register(nameof(ItemPadding), typeof(Thickness), typeof(EComboBox), new(new Thickness(4)));
        public static readonly DependencyProperty ItemBorderThicknessProperty = DependencyProperty.Register(nameof(ItemBorderThickness), typeof(Thickness), typeof(EComboBox), new(new Thickness(1)));
        public static readonly DependencyProperty ItemBorderCornerRadiusProperty = DependencyProperty.Register(nameof(ItemBorderCornerRadius), typeof(CornerRadius), typeof(EComboBox), new(Theme.Default.CornerRadius));
        #endregion

        public Thickness ArrowMargin
        {
            get => (Thickness)GetValue(ArrowMarginProperty);
            set => SetValue(ArrowMarginProperty, value);
        }

        public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register(nameof(ArrowMargin),  typeof(Thickness), typeof(EComboBox), new(new Thickness(0)));
        #endregion

        static EComboBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EComboBox), new FrameworkPropertyMetadata(typeof(EComboBox)));
        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
        }
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
        }
    }
}
