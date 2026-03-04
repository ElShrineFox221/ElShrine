using System.Windows;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ArrowControllerProperties))]
    public interface IArrowControllerBase
    {
        double ArrowSize { get; set; }
        Thickness ArrowMargin { get; set; }
        double ArrowBasicAngle { get; set; }
        double ArrowTargetAngle { get; set; }
    }
    public static class ArrowControllerProperties
    {
        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.RegisterAttached(
            nameof(ArrowSizeProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 16d,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.RegisterAttached(
            nameof(ArrowMarginProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(0),
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty ArrowBasicAngleProperty = DependencyProperty.RegisterAttached(
            nameof(ArrowBasicAngleProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 0d,
                flags: FrameworkPropertyMetadataOptions.Inherits
        ));
        public static readonly DependencyProperty ArrowTargetAngleProperty = DependencyProperty.RegisterAttached(
            nameof(ArrowTargetAngleProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 90d,
                flags: FrameworkPropertyMetadataOptions.Inherits
        ));
    }
}
