using System.Windows;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ScrollBarControlProperties))]
    public interface IScrollBarControlBase
    {
        bool IsStartEndButtonVisibile { get; set; }
        bool IsMoveButtonVisibile { get; set; }
        double BarSize { get; set; }
    }
    public static class ScrollBarControlProperties
    {
        #region DPs
        public static readonly DependencyProperty IsStartEndButtonVisibileProperty = DependencyProperty.RegisterAttached(
            nameof(IsStartEndButtonVisibileProperty).ToPropRegName(),
            typeof(bool),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: true,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty IsMoveButtonVisibileProperty = DependencyProperty.RegisterAttached(
            nameof(IsMoveButtonVisibileProperty).ToPropRegName(),
            typeof(bool),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: true,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty BarSizeProperty = DependencyProperty.RegisterAttached(
            nameof(BarSizeProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 10d,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        #endregion
    }
}
