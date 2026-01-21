using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(SelectionRenderControlProperties))]
    public interface ISelectionRenderControlBase
    {
        double SelectedItemLineWidth { get; set; }
        double SelectedItemLineLengthRate { get; set; }
        ExpandDirection IndicatorPlacement { get; set; }
    }
    public static class SelectionRenderControlProperties
    {
        #region DPs
        public static readonly DependencyProperty SelectedItemLineWidthProperty = DependencyProperty.RegisterAttached(
            nameof(SelectedItemLineWidthProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 4d,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty SelectedItemLineLengthRateProperty = DependencyProperty.RegisterAttached(
            nameof(SelectedItemLineLengthRateProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 1d,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty IndicatorPlacementProperty = DependencyProperty.RegisterAttached(
            nameof(IndicatorPlacementProperty).ToPropRegName(),
            typeof(ExpandDirection),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: ExpandDirection.Left,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        #endregion
    }
}
