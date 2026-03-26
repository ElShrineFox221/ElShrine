using System.Windows;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ItemRenderControlProperties))]
    public interface IItemRenderControlBase
    {
        Thickness ItemMargin { get; set; }
        Thickness ItemPadding { get; set; }
        Thickness ItemBorderThickness { get; set; }
        CornerRadius ItemBorderCornerRadius { get; set; }
    }
    public static class ItemRenderControlProperties
    {
        #region DPs
        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.RegisterAttached(
            nameof(ItemMarginProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(2),
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));

        public static readonly DependencyProperty ItemPaddingProperty = DependencyProperty.RegisterAttached(
            nameof(ItemPaddingProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(4),
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));

        public static readonly DependencyProperty ItemBorderThicknessProperty = DependencyProperty.RegisterAttached(
            nameof(ItemBorderThicknessProperty).ToPropRegName(),
            typeof(Thickness),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new Thickness(1),
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));

        public static readonly DependencyProperty ItemBorderCornerRadiusProperty = DependencyProperty.RegisterAttached(
            nameof(ItemBorderCornerRadiusProperty).ToPropRegName(),
            typeof(CornerRadius),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: new CornerRadius(2),
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        #endregion
    }
}
