using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ElShrine.Wpf.Controls
{
    public class ETabControl : TabControl
    {
        #region UI

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

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(ETabControl), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(ETabControl), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(ETabControl), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(ETabControl), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        #region Header
        public HorizontalAlignment HorizontalHeaderAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalHeaderAlignmentProperty);
            set => SetValue(HorizontalHeaderAlignmentProperty, value);
        }
        public double HorizontalHeaderPannelHeight
        {
            get => (double)GetValue(HorizontalHeaderPannelHeightProperty);
            set => SetValue(HorizontalHeaderAlignmentProperty, value);
        }
        public VerticalAlignment VerticalHeaderAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalHeaderAlignmentProperty);
            set => SetValue(VerticalHeaderAlignmentProperty, value);
        }
        public double VerticalHeaderPannelWidth
        {
            get => (double)GetValue(VerticalHeaderPannelWidthProperty);
            set => SetValue(VerticalHeaderPannelWidthProperty, value);
        }
        public Brush HeaderBackgroundBrush
        {
            get => (Brush)GetValue(HeaderBackgroundBrushProperty);
            set => SetValue(HeaderBackgroundBrushProperty, value);
        }

        public static readonly DependencyProperty HorizontalHeaderAlignmentProperty = DependencyProperty.Register(nameof(HorizontalHeaderAlignment), typeof(HorizontalAlignment), typeof(TabControl), new(HorizontalAlignment.Left));
        public static readonly DependencyProperty HorizontalHeaderPannelHeightProperty = DependencyProperty.Register(nameof(HorizontalHeaderPannelHeight), typeof(double), typeof(TabControl), new(0d));
        public static readonly DependencyProperty VerticalHeaderAlignmentProperty = DependencyProperty.Register(nameof(VerticalHeaderAlignment), typeof(VerticalAlignment), typeof(TabControl), new(VerticalAlignment.Top));
        public static readonly DependencyProperty VerticalHeaderPannelWidthProperty = DependencyProperty.Register(nameof(VerticalHeaderPannelWidth), typeof(double), typeof(TabControl), new(0d));
        public static readonly DependencyProperty HeaderBackgroundBrushProperty = DependencyProperty.Register(nameof(HeaderBackgroundBrush), typeof(Brush), typeof(TabControl), new(Theme.Default.ForeColor.ToSolidBrush()));
        #endregion

        public int OldOutMiliseconds
        {
            get => (int)GetValue(OldOutMilisecondsProperty);
            set => SetValue(OldOutMilisecondsProperty, value);
        }
        public int NewInWaitMiliseconds
        {
            get => (int)GetValue(NewInWaitMilisecondsProperty);
            set => SetValue(NewInWaitMilisecondsProperty, value);
        }
        public int NewInMiliseconds
        {
            get => (int)GetValue(NewInMilisecondsProperty);
            set => SetValue(NewInMilisecondsProperty, value);
        }
        public static readonly DependencyProperty OldOutMilisecondsProperty = DependencyProperty.Register(nameof(OldOutMiliseconds), typeof(int), typeof(TabControl), new(400));
        public static readonly DependencyProperty NewInWaitMilisecondsProperty = DependencyProperty.Register(nameof(NewInWaitMiliseconds), typeof(int), typeof(TabControl), new(0));
        public static readonly DependencyProperty NewInMilisecondsProperty = DependencyProperty.Register(nameof(NewInMiliseconds), typeof(int), typeof(TabControl), new(400));
        #endregion

        static ETabControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ETabControl), new FrameworkPropertyMetadata(typeof(ETabControl)));

        #endregion
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            currentContentContainer = GetTemplateChild("currentContentContainer") as ContentPresenter;
            snapshotContainer = GetTemplateChild("snapshotContainer") as Image;
        }
        private ContentPresenter? currentContentContainer;
        private Image? snapshotContainer;
        private bool animationPlaying;

        public ETabControl() => SelectionChanged += OnSelectionChanged;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != this) return;
            if (currentContentContainer is not null && snapshotContainer is not null && !animationPlaying)
            {
                var removedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                var addedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
                StartAnimatedTransition(removedItem, addedItem);
            }
        }
        public Func<AnimationsInfo<Image, int>?, AnimationsInfo<ContentPresenter, int>?, AnimationInfoTriggerMode>? AnimationBuilder { get; set; }
        private void StartAnimatedTransition(object? oldItem, object? newItem)
        {
            animationPlaying = true;
            AnimationsInfo<Image, int>? oldOutInfoItem = null; AnimationsInfo<ContentPresenter, int>? newInInfoItem = null;
            if (snapshotContainer is not null && oldItem is not null)
            {
                //Create snapshot and set image
                var oldContent = oldItem is TabItem tabItem ? tabItem.Content : oldItem;
                var element = FindVisualForContent(oldContent);
                var snapshot = element.CreateVisualSnapshot(double.IsNaN(element.Width) ? element.ActualWidth : element.Width, double.IsNaN(element.Height) ? element.ActualHeight : element.Height);
                snapshotContainer.Source = snapshot;
                //Update image
                snapshotContainer.Visibility = Visibility.Visible;
                //Create info item
                oldOutInfoItem = new(new(TimeSpan.Zero), new(TimeSpan.FromMilliseconds(OldOutMiliseconds)), snapshotContainer, Items.IndexOf(oldItem));

                FrameworkElement FindVisualForContent(object content)
                {
                    FrameworkElement? result = null;
                    foreach (TabItem item in Items)
                    {
                        if (item.Content == content)
                        {
                            var contentPresenter = item.FindChild<ContentPresenter>();
                            result = contentPresenter?.ContentTemplate?.LoadContent() as FrameworkElement ?? content as FrameworkElement;
                            break;
                        }
                    }
                    result ??= new ContentControl { Content = content };
                    return result;
                }
            }
            if(currentContentContainer is not null && newItem is not null)
            {
                //Update current content view
                currentContentContainer.Visibility = Visibility.Hidden;
                //Create info item
                newInInfoItem = new(new(TimeSpan.FromMilliseconds(NewInWaitMiliseconds)), new(TimeSpan.FromMilliseconds(NewInMiliseconds)), currentContentContainer, Items.IndexOf(newItem));
            }
            //Try builder animations
            var triggerMode = AnimationInfoTriggerMode.None;
            if (AnimationBuilder is not null) triggerMode = AnimationBuilder.Invoke(oldOutInfoItem, newInInfoItem);
            else triggerMode = DefaultAnimationBuilder.Invoke(oldOutInfoItem, newInInfoItem);
            //Prepare to play animations
            List<AnimationsInfo> animations = [];
            if(oldOutInfoItem is not null) animations.Add(oldOutInfoItem);
            if (newInInfoItem is not null) animations.Add(newInInfoItem);
            void completeAction()
            {
                if (snapshotContainer is null) return;
                snapshotContainer.Visibility = Visibility.Collapsed;
                snapshotContainer.Opacity = 1;
                snapshotContainer.Source = null;
                animationPlaying = false;
            }

            Dispatcher.BeginInvoke(() =>
            {
                AnimationsInfo.PlayAnimations([.. animations], triggerMode, completeAction);
            }, DispatcherPriority.Render);
        }
        private readonly static Func<AnimationsInfo<Image, int>?, AnimationsInfo<ContentPresenter, int>?, AnimationInfoTriggerMode> DefaultAnimationBuilder = (oldOutInfoItem, newInInfoItem) =>
        {
            var parent = oldOutInfoItem?.Target.FindParent<ETabControl>(false) ?? newInInfoItem?.Target.FindParent<ETabControl>(false);
            var ef = new SineEase { EasingMode = EasingMode.EaseInOut};
            var newInItemIsAfter = (newInInfoItem?.AppendedData ?? -1) > (oldOutInfoItem?.AppendedData ?? -1);
            var isHorizontal = parent is null || parent.TabStripPlacement == Dock.Top || parent.TabStripPlacement == Dock.Bottom;
            if (oldOutInfoItem is not null)
            {
                var target = oldOutInfoItem.Target;
                target.Opacity = 1;
                target.Margin = new(0);
                var fadeOut = new DoubleAnimation
                {
                    To = 0,
                    Duration = oldOutInfoItem.BaseDuration,
                    EasingFunction = ef
                };
                var toMarginLength = isHorizontal ? (parent?.ActualWidth ?? target.ActualWidth) : (parent?.ActualHeight ?? target.Height);
                if (!newInItemIsAfter) toMarginLength = -toMarginLength;
                var slideOut = new ThicknessAnimation
                {
                    From = new(0),
                    To = isHorizontal ? new(-toMarginLength, 0, toMarginLength, 0): new(0, -toMarginLength, 0, toMarginLength),
                    Duration = oldOutInfoItem.BaseDuration,
                    EasingFunction = ef
                };
                oldOutInfoItem.Animations.AddRange([
                    (fadeOut, OpacityProperty), 
                    (slideOut, MarginProperty)
                    ]);
            }
            if(newInInfoItem is not null)
            {
                var target = newInInfoItem.Target;
                target.Visibility = Visibility.Visible;
                target.Opacity = 0;
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = newInInfoItem.BaseDuration,
                    EasingFunction= ef
                };
                var toMarginLength = isHorizontal ? (parent?.ActualWidth ?? target.ActualWidth) : (parent?.ActualHeight ?? target.Height);
                if (!newInItemIsAfter) toMarginLength = -toMarginLength;
                var slideIn = new ThicknessAnimation
                {
                    From = isHorizontal ? new(toMarginLength, 0, -toMarginLength, 0) : new(0, toMarginLength, 0, -toMarginLength),
                    To = new(0),
                    Duration = newInInfoItem.BaseDuration,
                    EasingFunction = ef
                };
                newInInfoItem.Animations.AddRange([
                    (fadeIn, OpacityProperty), 
                    (slideIn, MarginProperty)
                    ]);
            }

            return AnimationInfoTriggerMode.All;
        };
    }
    public class ETabPannel : TabPanel
    {
        public static readonly DependencyProperty IsHorizontalProperty = DependencyProperty.Register(nameof(IsHorizontal), typeof(bool), typeof(ETabPannel), new(true));
        public bool IsHorizontal
        {
            get => (bool)GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var size = IsHorizontal ? new Size(double.PositiveInfinity, availableSize.Height) : new(availableSize.Width, double.PositiveInfinity);
                child.Measure(size);
            }
            return base.MeasureOverride(availableSize);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = InternalChildren.Count;
            double horStep = double.NaN, vertStep = double.NaN;
            if (HorizontalAlignment == HorizontalAlignment.Stretch && count != 0 && IsHorizontal) horStep = finalSize.Width / count;
            if (VerticalAlignment == VerticalAlignment.Stretch && count != 0 && !IsHorizontal) vertStep = finalSize.Height / count;
            if(double.IsNaN(horStep) ^ double.IsNaN(vertStep))
            {
                for (int i = 0; i < count; i++)
                {
                    Rect rect = IsHorizontal ? new(i * horStep, 0, horStep, finalSize.Height) : new(0, i * vertStep, finalSize.Width, vertStep);
                    InternalChildren[i].Arrange(rect);
                }
            }
            else
            {
                double loc = 0;
                for (int i = 0; i < count; i++)
                {
                    var desired = IsHorizontal ? InternalChildren[i].DesiredSize.Width : InternalChildren[i].DesiredSize.Height;
                    Rect rect = IsHorizontal ? new(loc, 0, desired, finalSize.Height) : new(0, loc, finalSize.Width, desired);
                    loc += desired;
                    InternalChildren[i].Arrange(rect);
                }
            }
            return finalSize;
        }
    }
}
