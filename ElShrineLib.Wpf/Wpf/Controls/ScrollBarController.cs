using System;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [Flags]
    public enum ScrollToEndMode
    {
        Disabled = 0,
        EnabledFlag = 1,
        AutoFlag = 2,
        Auto = AutoFlag | EnabledFlag,
        Always = EnabledFlag,
        ToLeftOrTop = 4,
        ToRightOrBottom = 8,
        ToAll = ToLeftOrTop | ToRightOrBottom,

        AutoToEnd = Auto | ToRightOrBottom,
        AutoToStart = Auto | ToLeftOrTop,
        AutoToAll = Auto | ToAll,
        AlwaysToEnd = Always | ToRightOrBottom,
        AlwaysToStart = Always | ToLeftOrTop,
        AlwaysToAll = Always | ToAll,
    }
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ScrollBarControllerProperties))]
    public interface IScrollBarControllerBase
    {
        object CornerContent { get; set; }
        ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        Thickness VerticalScrollBarMargin { get; set; }
        Thickness HorizontalScrollBarMargin { get; set; }
        ScrollToEndMode VerticalScrollToEnd { get; set;}
        ScrollToEndMode HorizontalScrollToEnd { get; set;}
    }
    public static class ScrollBarControllerProperties
    {
        #region DPs
        public static readonly DependencyProperty CornerContentProperty = DependencyProperty.RegisterAttached(
            nameof(CornerContentProperty).ToPropRegName(),
            typeof(object),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.RegisterAttached(
            nameof(VerticalScrollBarVisibilityProperty).ToPropRegName(),
            typeof(ScrollBarVisibility),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: ScrollBarVisibility.Auto,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.RegisterAttached(
            nameof(HorizontalScrollBarVisibilityProperty).ToPropRegName(),
            typeof(ScrollBarVisibility),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: ScrollBarVisibility.Auto,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty VerticalScrollBarMarginProperty = DependencyProperty.RegisterAttached(
            nameof(VerticalScrollBarMarginProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(0),
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty HorizontalScrollBarMarginProperty = DependencyProperty.RegisterAttached(
            nameof(HorizontalScrollBarMarginProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(0),
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty VerticalScrollToEndProperty = DependencyProperty.RegisterAttached(
            nameof(VerticalScrollToEndProperty).ToPropRegName(),
            typeof(ScrollToEndMode),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: ScrollToEndMode.Auto | ScrollToEndMode.ToRightOrBottom,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty HorizontalScrollToEndProperty = DependencyProperty.RegisterAttached(
            nameof(HorizontalScrollToEndProperty).ToPropRegName(),
            typeof(ScrollToEndMode),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: ScrollToEndMode.Disabled,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        #endregion
    }
}
