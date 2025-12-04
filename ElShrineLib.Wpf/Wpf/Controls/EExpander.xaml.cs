using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Brush = System.Windows.Media.Brush;

namespace ElShrine.Wpf.Controls
{
    public class EExpander : Expander, IThemeControlOld
    {
        private ContentPresenter? contentHost;
        private RotateTransform? rotateTransform;

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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius),  typeof(CornerRadius), typeof(EExpander), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush),  typeof(Brush), typeof(EExpander), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush),  typeof(Brush), typeof(EExpander), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush),  typeof(Brush), typeof(EExpander), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public Thickness ArrowMargin
        {
            get => (Thickness)GetValue(ArrowMarginProperty); 
            set => SetValue(ArrowMarginProperty, value);
        }
        public double ArrowAngle
        {
            get => (double)GetValue(ArrowAngleProperty);
            set => SetValue(ArrowAngleProperty, value);
        }
        public ExpandDirection HeaderPlacement
        {
            get => (ExpandDirection)GetValue(HeaderPlacementProperty);
            set => SetValue(HeaderPlacementProperty, value);
        }
        public int SlideDuration
        {
            get => (int)GetValue(SlideDurationProperty);
            set => SetValue(SlideDurationProperty, value);
        }
        public double SlideDistance
        {
            get => (double)GetValue(SlideDistanceProperty);
            set => SetValue(SlideDistanceProperty, value);
        }
        public int FadeDuration
        {
            get => (int)GetValue(FadeDurationProperty);
            set => SetValue(FadeDurationProperty, value);
        }
        public bool IsCollapseContrary
        {
            get => (bool)GetValue(IsCollapseContraryProperty);
            set => SetValue(IsCollapseContraryProperty, value);
        }

        public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register(nameof(ArrowMargin),  typeof(Thickness), typeof(EExpander), new(new Thickness(0)));
        public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register(nameof(ArrowAngle),  typeof(double), typeof(EExpander), new(0d));
        public static readonly DependencyProperty HeaderPlacementProperty = DependencyProperty.Register(nameof(HeaderPlacement),  typeof(ExpandDirection), typeof(EExpander), new(ExpandDirection.Left));
        public static readonly DependencyProperty SlideDistanceProperty = DependencyProperty.Register(nameof(SlideDistance),  typeof(double), typeof(EExpander), new (20d));
        public static readonly DependencyProperty SlideDurationProperty = DependencyProperty.Register(nameof(SlideDuration),  typeof(int), typeof(EExpander), new (300));
        public static readonly DependencyProperty FadeDurationProperty = DependencyProperty.Register(nameof(FadeDuration),  typeof(int), typeof(EExpander), new (200));
        public static readonly DependencyProperty IsCollapseContraryProperty = DependencyProperty.Register(nameof(IsCollapseContrary),  typeof(bool), typeof(EExpander), new(false));
        #endregion

        static EExpander() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EExpander), new FrameworkPropertyMetadata(typeof(EExpander)));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            contentHost = GetTemplateChild("PART_ContentHost") as ContentPresenter;
            rotateTransform = GetTemplateChild("AnimationArrowRotateTransform") as RotateTransform;
            if (contentHost is not null)
            {
                TranslateTransform transform;
                if (contentHost.RenderTransform is not null and TranslateTransform tf) transform = tf;
                else
                {
                    transform = new TranslateTransform();
                    contentHost.RenderTransform = transform;
                }
                if (IsExpanded)
                {
                    contentHost.Visibility = Visibility.Visible;
                    contentHost.Opacity = 1;
                }
                else
                {
                    contentHost.Visibility = Visibility.Collapsed;
                    contentHost.Opacity = 0;
                    switch (ExpandDirection)
                    {
                        case ExpandDirection.Down:
                            transform.Y = -SlideDistance;
                            break;
                        case ExpandDirection.Up:
                            transform.Y = SlideDistance;
                            break;
                        case ExpandDirection.Left:
                            transform.X = SlideDistance;
                            break;
                        case ExpandDirection.Right:
                            transform.X = -SlideDistance;
                            break;
                    }
                }
            }
        }
        protected override void OnExpanded()
        {
            base.OnExpanded();
            StartAnimation(true);
        }
        protected override void OnCollapsed()
        {
            base.OnCollapsed();
            StartAnimation(false);
        }
        private void StartAnimation(bool expand)
        {
            if (contentHost == null) return;
            if (contentHost.RenderTransform is not TranslateTransform transform)
            {
                transform = new TranslateTransform();
                contentHost.RenderTransform = transform;
            }
            var isXProperty = ExpandDirection == ExpandDirection.Left || ExpandDirection == ExpandDirection.Right;
            var propdp = isXProperty ? TranslateTransform.XProperty : TranslateTransform.YProperty;
            var toValue = getValue(expand);

            var slide = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(SlideDuration),
                EasingFunction = new CubicEase { EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            var fade = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(FadeDuration),
                EasingFunction = new CubicEase { EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            var rotate = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(Math.Max(FadeDuration, SlideDuration)),
                EasingFunction = new CubicEase { EasingMode = expand ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            int rotatedNow = getDirectionAngle(HeaderPlacement), rotateTo = 0;
            if (expand)
            {
                transform.X = isXProperty ? toValue : 0;
                transform.Y = isXProperty ? 0 : toValue;
                contentHost.Visibility = Visibility.Visible;
                slide.To = 0;
                fade.To = 1;
                rotateTo = getDirectionAngle(ExpandDirection) - rotatedNow;
            }
            else
            {
                slide.To = toValue;
                fade.To = 0;
                var compeleteSource = SlideDuration > FadeDuration ? slide : fade;
                compeleteSource.Completed += (_, _) => contentHost.Visibility = Visibility.Collapsed;
            }
            transform.BeginAnimation(propdp, slide);
            contentHost.BeginAnimation(OpacityProperty, fade);
            if (Math.Abs(rotateTo) > 180) rotateTo = rotateTo.Shift(0, 360, 0);
            rotate.To = rotateTo;
            rotateTransform?.BeginAnimation(RotateTransform.AngleProperty, rotate);

            double getValue(bool expand)
            {
                bool contrary = !expand && IsCollapseContrary;
                double result = ExpandDirection switch
                {
                    ExpandDirection.Up => contrary ? -SlideDistance : SlideDistance,
                    ExpandDirection.Left => contrary ? -SlideDistance : SlideDistance,
                    ExpandDirection.Right => contrary ? SlideDistance : -SlideDistance,
                    _ => contrary ? SlideDistance : -SlideDistance, //Down
                };
                return result;
            }
            int getDirectionAngle(ExpandDirection direction)
                => direction switch
                {
                    ExpandDirection.Down => 0,
                    ExpandDirection.Up => 180,
                    ExpandDirection.Left => 90,
                    _ => 270, //Right
                };
        }
    }
}