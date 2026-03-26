using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class ETreeViewItem : TreeViewItem, IThemeControlBase
{
    #region DPs
    public bool IsScopeEndLine
    {
        get => (bool)GetValue(IsScopeEndLineProperty);
        set => SetValue(IsScopeEndLineProperty, value);
    }
    public double OverrideIndentPixels
    {
        get => (double)GetValue(OverrideIntentPixelsProperty);
        set => SetValue(OverrideIntentPixelsProperty, value);
    }
    public double IndentUnitLength
    {
        get => (double)GetValue(IndentUnitLengthProperty);
        set => SetValue(IndentUnitLengthProperty, value);
    }
    public int IndentLevels
    {
        get => (int)GetValue(IntentLevelsProperty);
        set => SetValue(IntentLevelsProperty, value);
    }
    public DataTemplate LeftColumnTemplate
    {
        get => (DataTemplate)GetValue(LeftColumnTemplateProperty);
        set => SetValue(LeftColumnTemplateProperty, value);
    }
    public Thickness ArrowMargin
    {
        get => (Thickness)GetValue(ArrowMarginProperty);
        set => SetValue(ArrowMarginProperty, value);
    }
    
    public double IndentWidth
    {
        get => (double)GetValue(IndentWidthProperty);
        protected set => SetValue(IndentWidthProperty, value);
    }
    public Visibility ItemsBtnVisibility
    {
        get => (Visibility)GetValue(ItemsBtnVisibilityProperty);
        protected set => SetValue(ItemsBtnVisibilityProperty, value);
    }

    public static readonly DependencyProperty IsScopeEndLineProperty = DependencyProperty.Register(
        nameof(IsScopeEndLine), 
        typeof(bool), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, propertyChangedCallback: UpdateIndentCallback));
    public static readonly DependencyProperty OverrideIntentPixelsProperty = DependencyProperty.Register(
        nameof(OverrideIndentPixels), 
        typeof(double), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender, UpdateIndentCallback));
    public static readonly DependencyProperty IndentUnitLengthProperty = DependencyProperty.Register(
        nameof(IndentUnitLength), 
        typeof(double), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender, UpdateIndentCallback));
    public static readonly DependencyProperty IntentLevelsProperty = DependencyProperty.Register(
        nameof(IndentLevels), 
        typeof(int), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, UpdateIndentCallback));
    public static readonly DependencyProperty LeftColumnTemplateProperty = DependencyProperty.Register(
        nameof(LeftColumnTemplate), 
        typeof(DataTemplate), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register(
        nameof(ArrowMargin), 
        typeof(Thickness), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(new Thickness(0)));
    public static readonly DependencyProperty IndentWidthProperty = DependencyProperty.Register(
        nameof(IndentWidth), 
        typeof(double), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ItemsBtnVisibilityProperty = DependencyProperty.Register(
        nameof(ItemsBtnVisibility), 
        typeof(Visibility), 
        typeof(ETreeViewItem), 
        new FrameworkPropertyMetadata(Visibility.Hidden, FrameworkPropertyMetadataOptions.AffectsRender));
    #endregion

    #region Implements
    static ETreeViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ETreeViewItem), new FrameworkPropertyMetadata(typeof(ETreeViewItem)));
        
    }
    public ETreeViewItem() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    #region Items
    protected override DependencyObject GetContainerForItemOverride() => new ETreeViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is ETreeViewItem;
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        ItemsBtnVisibility = Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
    }
    #endregion

    #region Indent
    private Rectangle? PART_IndentSpacer;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_IndentSpacer = GetTemplateChild(nameof(PART_IndentSpacer)) as Rectangle;
        UpdateLevel();
    }
    private static void UpdateIndentCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as ETreeViewItem)?.UpdateIndent();
    public void UpdateIndent()
    {
        if (PART_IndentSpacer is null) return;
        var indentPixels = 0d;
        if (!double.IsNaN(OverrideIndentPixels) && OverrideIndentPixels > 0) indentPixels = OverrideIndentPixels; 
        else indentPixels = IndentLevels * IndentUnitLength;
        if(IndentWidth != indentPixels) IndentWidth = indentPixels;
    }
    public void UpdateLevel()
    {
        int level = 0;
        DependencyObject parent = VisualTreeHelper.GetParent(this);

        while (parent is not null && parent is not ETreeView)
        {
            if (parent is ETreeViewItem) level++;
            parent = VisualTreeHelper.GetParent(parent);
        }
        level = Math.Max(0, IsScopeEndLine ? level - 1 : level);
        if(parent is null) level = 0;
        IndentLevels = level;
    }
    #endregion
}
