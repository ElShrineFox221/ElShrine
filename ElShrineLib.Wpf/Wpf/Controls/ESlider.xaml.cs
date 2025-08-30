using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.Controls
{
    public enum SliderFillStyle
    {
        LightLower, LightUpper, LightAll, None
    }
    public class ESlider : Slider, IThemeControl
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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(ESlider), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(ESlider), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(ESlider), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(ESlider), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public object ThumbContent
        {
            get => GetValue(ThumbContentProperty);
            set => SetValue(ThumbContentProperty, value);
        }
        public DataTemplate ThumbTemplate
        {
            get => (DataTemplate)GetValue(ThumbTemplateProperty);
            set => SetValue(ThumbTemplateProperty, value);
        }
        public bool IsClickShakable
        {
            get => (bool)GetValue(IsClickShakableProperty);
            set => SetValue(IsClickShakableProperty, value);
        }
        public SliderFillStyle SliderFillStyle
        {
            get => (SliderFillStyle)GetValue(SliderFillStyleProperty);
            set => SetValue(SliderFillStyleProperty, value);
        }
        public Brush FillBack
        {
            get => (Brush)GetValue(FillBackProperty);
            set => SetValue(FillBackProperty, value);
        }
        public Brush FillFore
        {
            get => (Brush)(GetValue(FillForeProperty));
            set => SetValue(FillForeProperty, value);
        }

        public static readonly DependencyProperty ThumbContentProperty = DependencyProperty.Register(nameof(ThumbContent), typeof(object), typeof(ESlider), new(null));
        public static readonly DependencyProperty ThumbTemplateProperty = DependencyProperty.Register(nameof(ThumbTemplate), typeof(DataTemplate), typeof(ESlider), new(null));
        public static readonly DependencyProperty IsClickShakableProperty = DependencyProperty.Register(nameof(IsClickShakable), typeof(bool), typeof(ESlider), new(true));
        public static readonly DependencyProperty SliderFillStyleProperty = DependencyProperty.Register(nameof(SliderFillStyle), typeof(SliderFillStyle), typeof(ESlider), new(SliderFillStyle.LightLower));

        public static readonly DependencyProperty FillBackProperty = DependencyProperty.Register(nameof(FillBack), typeof(Brush), typeof(ESlider), new(new SolidColorBrush(Color.Transparent.ToMediaColor())));
        public static readonly DependencyProperty FillForeProperty = DependencyProperty.Register(nameof(FillFore), typeof(Brush), typeof(ESlider), new(new SolidColorBrush(Color.Transparent.ToMediaColor())));
        #endregion

        static ESlider() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ESlider), new FrameworkPropertyMetadata(typeof(ESlider)));

        private Track? track;
        private Thumb? thumb;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            track = GetTemplateChild("PART_Track") as Track;
            thumb = GetTemplateChild("trackThumb") as Thumb;
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (track is not null && thumb is not null && e.OriginalSource is FrameworkElement fe && !fe.IsChildOf(thumb))
            {
                Point pos = e.GetPosition(track);
                var value = Orientation == Orientation.Horizontal ? pos.X / track.ActualWidth * (Maximum - Minimum) + Minimum : (Maximum - ((1 - pos.Y / track.ActualHeight) * (Maximum - Minimum) + Minimum));
                Value = snapToTick(value);
                e.Handled = true;
            }
            if (!e.Handled) base.OnPreviewMouseLeftButtonDown(e);
            double snapToTick(double value)
            {
                var result = value;
                if (TickFrequency > 0 && IsSnapToTickEnabled)
                {
                    double snapped = Math.Round(value / TickFrequency) * TickFrequency;
                    result = Math.Max(Minimum, Math.Min(Maximum, snapped));
                }
                return result;
            }
        }
    }
}
