using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ScrollBarControllerProperties))]
    public interface IScrollBarControllerBase
    {
        object CornerContent { get; set; }
        ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        Thickness VerticalScrollBarMargin { get; set; }
        Thickness HorizontalScrollBarMargin { get; set; }
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
        #endregion
    }
}
