using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls;

public partial class ETabControl : TabControl, IThemeControlBase
{
    #region DPs

    #region Header
    public HorizontalAlignment HorizontalHeaderAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalHeaderAlignmentProperty);
        set => SetValue(HorizontalHeaderAlignmentProperty, value);
    }
    public double HorizontalHeaderPannelHeight
    {
        get => (double)GetValue(HorizontalHeaderPannelHeightProperty);
        set => SetValue(HorizontalHeaderPannelHeightProperty, value);
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

    public static readonly DependencyProperty HorizontalHeaderAlignmentProperty = DependencyProperty.Register(nameof(HorizontalHeaderAlignment), typeof(HorizontalAlignment), typeof(TabControl), new(HorizontalAlignment.Left));
    public static readonly DependencyProperty HorizontalHeaderPannelHeightProperty = DependencyProperty.Register(nameof(HorizontalHeaderPannelHeight), typeof(double), typeof(TabControl), new(0d));
    public static readonly DependencyProperty VerticalHeaderAlignmentProperty = DependencyProperty.Register(nameof(VerticalHeaderAlignment), typeof(VerticalAlignment), typeof(TabControl), new(VerticalAlignment.Top));
    public static readonly DependencyProperty VerticalHeaderPannelWidthProperty = DependencyProperty.Register(nameof(VerticalHeaderPannelWidth), typeof(double), typeof(TabControl), new(0d));
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

    #region Implements
    static ETabControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ETabControl), new FrameworkPropertyMetadata(typeof(ETabControl)));
    public ETabControl()
    {
        WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        SelectionChanged += OnSelectionChanged;
    }
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_CurrentContentContainer = GetTemplateChild(nameof(PART_CurrentContentContainer)) as ContentPresenter;
        PART_SnapshotContainer = GetTemplateChild(nameof(PART_SnapshotContainer)) as Image;
    }
    private ContentPresenter? PART_CurrentContentContainer;
    private Image? PART_SnapshotContainer;
    public bool AnimationPlaying { get; protected set; }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != this) return;
        if (PART_CurrentContentContainer is not null && PART_SnapshotContainer is not null)
        {
            var removedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
            var addedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            StartAnimatedTransition(removedItem, addedItem);
        }
    }
    
    private void StartAnimatedTransition(object? oldItem, object? newItem)
    {
        AnimationPlaying = true;
        var anims = new List<AnimInfo>();
        var builder = AnimationBuilder ?? DefaultBuilder;
        var oldIndex = Items.IndexOf(oldItem);
        var newIndex = Items.IndexOf(newItem);
        var oldTBI = ItemContainerGenerator.ContainerFromItem(oldItem) as TabItem;
        if (oldTBI is not null && PART_SnapshotContainer is not null)
        {
            PART_SnapshotContainer.Visibility = Visibility.Visible;
            var oldContent = FindVisualForContent(oldTBI);
            var snapShot = oldContent.CreateVisualSnapshot(
                double.IsNaN(oldContent.Width) ? oldContent.ActualWidth : oldContent.Width,
                double.IsNaN(oldContent.Height) ? oldContent.ActualHeight : oldContent.Height);
            PART_SnapshotContainer.Source = snapShot;
            anims.AddRange(builder.Invoke(new(this, PART_SnapshotContainer, oldTBI, false, oldIndex, newIndex)));
        }
        var newTBI = ItemContainerGenerator.ContainerFromItem(newItem) as TabItem;
        if (newTBI is not null && PART_CurrentContentContainer is not null)
            anims.AddRange(builder.Invoke(new(this, PART_CurrentContentContainer, newTBI, true, oldIndex, newIndex)));

        foreach (var anim in anims) anim.BeginAnimation();

        static FrameworkElement FindVisualForContent(TabItem item)
        {
            var content = item.Content;
            FrameworkElement? result = content as FrameworkElement ?? item.Content as FrameworkElement;
            result ??= new ContentControl { Content = item.Content };
            return result;
        }
    }
    public sealed record AnimInfo(AnimationTimeline Animation, TimeSpan WaitTime, DependencyProperty TargetProp, FrameworkElement Target, Action<FrameworkElement>? Completion = null)
    {
        public void BeginAnimation()
        {
            Animation.Completed += (s, e) => Completion?.Invoke(Target);
            Animation.BeginTime = WaitTime;
            Target.BeginAnimation(TargetProp, Animation);
        }
    }
    public sealed record ETabControlSelectionTransInfo(ETabControl Source, FrameworkElement Container, TabItem TargetItem, bool IsNewIn, int OldIndex, int NewIndex);
    public Func<ETabControlSelectionTransInfo, AnimInfo[]>? AnimationBuilder { get; set; }
    private readonly static Func<ETabControlSelectionTransInfo, AnimInfo[]> DefaultBuilder = transInfo =>
    {
        transInfo.Container.Visibility = Visibility.Visible;
        var isHorizontal = transInfo.Source.TabStripPlacement == Dock.Top || transInfo.Source.TabStripPlacement == Dock.Bottom;
        var toMarginLength = isHorizontal ? transInfo.Source.ActualWidth : transInfo.Source.ActualHeight;
        var fromOpacity = transInfo.IsNewIn ? 0d : 1d;
        var toOpacity = transInfo.IsNewIn ? 1d : 0d;
        transInfo.Container.Opacity = fromOpacity;
        var baseMargin = isHorizontal ? new Thickness(toMarginLength, 0, -toMarginLength, 0) : new Thickness(0, toMarginLength, 0, -toMarginLength);
        var newIsAfter = transInfo.NewIndex > transInfo.OldIndex;
        var fromMargin = transInfo.IsNewIn ? (newIsAfter ? baseMargin : reverseThickness(baseMargin)) : new(0);
        var toMargin = transInfo.IsNewIn ? new(0) : newIsAfter ? reverseThickness(baseMargin) : baseMargin;
        
        var duration = TimeSpan.FromMilliseconds(transInfo.IsNewIn ? transInfo.Source.NewInMiliseconds : transInfo.Source.OldOutMiliseconds);
        var waitTime = transInfo.IsNewIn ? TimeSpan.FromMilliseconds(transInfo.Source.NewInWaitMiliseconds) : TimeSpan.Zero;

        var opacityAnimation = new DoubleAnimation(fromOpacity, toOpacity, duration, FillBehavior.Stop) { EasingFunction = transInfo.Source.AnimaEaseFunc };
        opacityAnimation.Completed += (s, e) => transInfo.Container.SetValue(OpacityProperty, toOpacity);
        var slideAnimation = new ThicknessAnimation(fromMargin, toMargin, duration, FillBehavior.Stop) { EasingFunction = transInfo.Source.AnimaEaseFunc };
        slideAnimation.Completed += (s, e) => transInfo.Container.SetValue(MarginProperty, toMargin);

        var opacityInfo = new AnimInfo(opacityAnimation, waitTime, OpacityProperty, transInfo.Container, opacityAnimation.To == 0 ? t => t.Visibility = Visibility.Hidden : null);
        var slideInfo = new AnimInfo(slideAnimation, waitTime, MarginProperty, transInfo.Container, t =>
        {
            transInfo.Source.AnimationPlaying = false;
            //if (t is Image img) img.Source = null;
        });
        return [opacityInfo, slideInfo];
        static Thickness reverseThickness(Thickness t) => new(-t.Left, -t.Top, -t.Right, -t.Bottom);
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
