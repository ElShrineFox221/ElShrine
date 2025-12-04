using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls
{
    public class EInWindowDialog : ContentControl
    {
        #region DPs
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        public bool CloseOnClickOutside
        {
            get => (bool)GetValue(CloseOnClickOutsideProperty);
            set => SetValue(CloseOnClickOutsideProperty, value);
        }
        public Type OverlayParentType
        {
            get => (Type)GetValue(OverlayParentTypeProperty);
            set => SetValue(OverlayParentTypeProperty, value);
        }
        public string? OverlayParentName
        {
            get => (string?)GetValue(OverlayParentNameProperty);
            set => SetValue(OverlayParentNameProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(EInWindowDialog), new(false, OnIsOpenChanged));
        public static readonly DependencyProperty CloseOnClickOutsideProperty =
            DependencyProperty.Register(nameof(CloseOnClickOutside), typeof(bool), typeof(EInWindowDialog), new(true));
        public static readonly DependencyProperty OverlayParentTypeProperty =
            DependencyProperty.Register(nameof(OverlayParentType), typeof(Type), typeof(EInWindowDialog), new(typeof(Window)));
        public static readonly DependencyProperty OverlayParentNameProperty =
            DependencyProperty.Register(nameof(OverlayParentName), typeof(string), typeof(EInWindowDialog), new(null));
        #endregion

        private FrameworkElement? hostElement;
        private FrameworkElement? overlayRoot;
        private FrameworkElement? overlayMask;
        private ScaleTransform? contentScaleTransform;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            overlayRoot = GetTemplateChild(nameof(overlayRoot)) as FrameworkElement;
            OverlayMask = GetTemplateChild(nameof(overlayMask)) as FrameworkElement;
            contentScaleTransform = GetTemplateChild(nameof(contentScaleTransform)) as ScaleTransform;
        }

        private bool isAnimating;

        static EInWindowDialog() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EInWindowDialog), new FrameworkPropertyMetadata(typeof(EInWindowDialog)));
        public EInWindowDialog() { }
        
        private FrameworkElement? OverlayMask
        {
            get => overlayMask;
            set
            {
                if(overlayMask != value)
                {
                    if (overlayMask is not null) overlayMask.MouseDown -= OnOverlayMaskMouseDown;
                    if (value is not null) value.MouseDown += OnOverlayMaskMouseDown;
                    overlayMask = value;
                }
            }
        }
        private FrameworkElement? HostElement
        {
            get => hostElement;
            set
            {
                if(value != hostElement)
                {
                    if (hostElement is not null)
                    {
                        hostElement.SizeChanged -= HostWindowSizeChanged;
                        if (hostElement is Window olds)
                        {
                            olds.LocationChanged -= HostWindowLocationChanged;
                            olds.StateChanged -= HostWindowStateChanged;
                        }
                    }
                    if (value is not null)
                    {
                        value.SizeChanged += HostWindowSizeChanged;
                        if(value is Window news)
                        {
                            news.LocationChanged += HostWindowLocationChanged;
                            news.StateChanged += HostWindowStateChanged;
                        }
                    }
                    hostElement = value;
                }
            }
        }
        private void Update()
        {
            var root = this.GetRootDependencyObject();
            
            HostElement = root.FindVisualChildRecursive(dobj =>
            {
                var parentType = OverlayParentType;
                var parentName = OverlayParentName;
                var typeMatched = parentType.IsAssignableFrom(dobj.GetType());
                var nameMatched = parentName is null || (dobj is FrameworkElement ele && ele.Name == parentName);
                var r = typeMatched && nameMatched;
                return r;
            });
            HostElement ??= root as FrameworkElement;
        }

        private void UpdatePositionAndSize(bool forceUpdate = false)
        {
            if (forceUpdate || IsOpen)
            {
                if (HostElement is null || overlayRoot is null) return;
                Point relativePoint = this.GetPositionRelativeTo(HostElement);
                overlayRoot.Width = HostElement.ActualWidth;
                overlayRoot.Height = HostElement.ActualHeight;
                Canvas.SetLeft(overlayRoot, -relativePoint.X);
                Canvas.SetTop(overlayRoot, -relativePoint.Y);
            } 
        }

        #region Events
        private void HostWindowSizeChanged(object sender, SizeChangedEventArgs e) => UpdatePositionAndSize();
        private void HostWindowLocationChanged(object? sender, EventArgs e) => UpdatePositionAndSize();
        private void HostWindowStateChanged(object? sender, EventArgs e) => UpdatePositionAndSize();
        private void OnOverlayMaskMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CloseOnClickOutside && IsOpen) SetCurrentValue(IsOpenProperty, false);
        }
        #endregion


        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dialogControl = d as EInWindowDialog;
            if(dialogControl is not null)
            {
                dialogControl.Update();
                if ((bool)e.NewValue)
                {
                    dialogControl.ShowOverlay();
                }
                else
                {
                    dialogControl.HideOverlay();
                }
            }
        }
        private void ShowOverlay()
        {
            if (OverlayMask is null || contentScaleTransform is null || overlayRoot is null || isAnimating) return;
            isAnimating = true;
            UpdatePositionAndSize();
            overlayRoot.Visibility = Visibility.Visible;
            isAnimating = true;

            var maskFadeIn = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var contentScale = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new ElasticEase
                {
                    Oscillations = 1,
                    Springiness = 5,
                    EasingMode = EasingMode.EaseOut
                }
            };

            maskFadeIn.Completed += (s, e) => isAnimating = false;

            OverlayMask.BeginAnimation(OpacityProperty, maskFadeIn);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, contentScale);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, contentScale);
        }
        private void HideOverlay()
        {
            if (OverlayMask is null || contentScaleTransform is null || isAnimating) return;
            isAnimating = true;
            var maskFadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            var contentScale = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            maskFadeOut.Completed += (s, e) =>
            {
                isAnimating = false;
                if (overlayRoot is not null) overlayRoot.Visibility = Visibility.Collapsed;
            };

            OverlayMask.BeginAnimation(OpacityProperty, maskFadeOut);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, contentScale);
            contentScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, contentScale);
        }

        private record EDialogResult<T>(T Data, ControlDataUpdates Result, bool IsInnerClose) : IEDialogResult<T>;
        public static IEDialogResult<T> Show<T>(T dataSource, EInWindowDialog? dialog = null)
        {
            dialog ??= new EInWindowDialog();
            dialog.DataContext = dataSource;
            var result = new EDialogResult<T>(dataSource, ControlDataUpdates.Confrim, false);
            return result;
        }

        public interface IEDialogResult<T>
        {
            T Data { get; }
            ControlDataUpdates Result { get; }
            bool IsInnerClose { get; }
        }
    }
}
