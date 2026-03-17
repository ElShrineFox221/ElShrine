using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public partial class EExpander : Expander, IThemeControlBase
    {
        #region Implements
        static EExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EExpander), new FrameworkPropertyMetadata(typeof(EExpander)));
            IsExpandedProperty.OverrideMetadata(typeof(EExpander), new FrameworkPropertyMetadata(defaultValue:false, propertyChangedCallback: IsExpandedChanged));
        }
        public EExpander() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
        #endregion

        #region DPs

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
        public double SlideDistance
        {
            get => (double)GetValue(SlideDistanceProperty);
            set => SetValue(SlideDistanceProperty, value);
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
        public static readonly DependencyProperty IsCollapseContraryProperty = DependencyProperty.Register(nameof(IsCollapseContrary),  typeof(bool), typeof(EExpander), new(false));
        #endregion

        private ContentPresenter? PART_ContentHost;
        private RotateTransform? PART_ArrowAnimRotateTransform;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_ContentHost = GetTemplateChild(nameof(PART_ContentHost)) as ContentPresenter;
            PART_ArrowAnimRotateTransform = GetTemplateChild(nameof(PART_ArrowAnimRotateTransform)) as RotateTransform;
            if (PART_ContentHost is not null)
            {
                TranslateTransform transform;
                if (PART_ContentHost.RenderTransform is not null and TranslateTransform tf) transform = tf;
                else
                {
                    transform = new TranslateTransform();
                    PART_ContentHost.RenderTransform = transform;
                }
                if (IsExpanded)
                {
                    PART_ContentHost.Visibility = Visibility.Visible;
                    PART_ContentHost.Opacity = 1;
                }
                else
                {
                    PART_ContentHost.Visibility = Visibility.Collapsed;
                    PART_ContentHost.Opacity = 0;
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
        private static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EExpander expander && ((bool)e.NewValue ^ (bool)e.OldValue)) expander.StartAnimation((bool)e.NewValue);
        }
        private void StartAnimation(bool expand)
        {
            if (PART_ContentHost == null) return;
            if (PART_ContentHost.RenderTransform is not TranslateTransform transform)
            {
                transform = new TranslateTransform();
                PART_ContentHost.RenderTransform = transform;
            }
            var isXProperty = ExpandDirection == ExpandDirection.Left || ExpandDirection == ExpandDirection.Right;
            var toValue = getValue(expand);
            var tc = this as IThemeControlBase;

            var slideX = tc.ToDoubleAnimation(toValue, expand);
            var slideY = tc.ToDoubleAnimation(toValue, expand);
            var fade = tc.ToDoubleAnimation(toValue, expand);
            var rotate = tc.ToDoubleAnimation(toValue, expand);
            int rotatedNow = getDirectionAngle(HeaderPlacement), rotateTo = 0;
            if (expand)
            {
                transform.X = isXProperty ? toValue : 0;
                transform.Y = isXProperty ? 0 : toValue;
                PART_ContentHost.Visibility = Visibility.Visible;
                slideX.To = 0;
                slideY.To = 0;
                fade.To = 1;
                rotateTo = getDirectionAngle(ExpandDirection) - rotatedNow;
            }
            else
            {
                slideX.To = isXProperty ? toValue : 0;
                slideY.To = isXProperty ? 0 : toValue;
                fade.To = 0;
                var compeleteSource = fade;
                compeleteSource.Completed += (_, _) => PART_ContentHost.Visibility = Visibility.Collapsed;
            }
            transform.BeginAnimation(TranslateTransform.XProperty, slideX);
            transform.BeginAnimation(TranslateTransform.YProperty, slideY);
            PART_ContentHost.BeginAnimation(OpacityProperty, fade);
            if (Math.Abs(rotateTo) > 180) rotateTo = rotateTo.Shift(0, 360, 0);
            rotate.To = rotateTo;
            PART_ArrowAnimRotateTransform?.BeginAnimation(RotateTransform.AngleProperty, rotate);

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