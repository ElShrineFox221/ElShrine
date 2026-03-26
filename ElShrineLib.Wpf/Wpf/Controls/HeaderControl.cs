using System.Windows;

namespace ElShrine.Wpf.Controls
{
    public enum HeaderPlacement
    {
        R0C0, R0C1, R0C2,
        R1C0, R1C2,
        R2C0, R2C1, R2C2,
        Left = R1C0,
        Right = R1C2,
        Top = R0C1,
        Bottom = R2C1,
        LeftTop = R0C0,
        RightTop = R0C2,
        LeftBottom = R2C0,
        RightBottom = R2C2
    }
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(HeaderControlProperties))]
    public interface IHeaderControlBase
    {
        public HeaderPlacement HeaderPlacement { get; set; }
        public object Header { get; set; }
        public DataTemplate HeaderTemplate { get; set; }
    }
    public static class HeaderControlProperties
    {
        #region DPs
        public static readonly DependencyProperty HeaderPlacementProperty = DependencyProperty.RegisterAttached(
            nameof(HeaderPlacementProperty).ToPropRegName(),
            typeof(HeaderPlacement),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: HeaderPlacement.Left,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.RegisterAttached(
            nameof(HeaderTemplateProperty).ToPropRegName(),
            typeof(DataTemplate),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(HeaderProperty).ToPropRegName(),
            typeof(object),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                flags: FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure
        ));
        #endregion
    }
}
