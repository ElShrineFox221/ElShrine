using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EListView : ListView, IThemeControlBase, IScrollBarControlBase, IScrollBarControllerBase, ISelectionRenderControlBase, IItemRenderControlBase
{
    private record DragData(object Item, EListViewItem Container, ListView Source);

    #region Normal Properties
    private bool isDraggable = false;
    public bool IsDraggable
    {
        get => isDraggable;
        set
        {
            isDraggable = value;
            AllowDrop = value;
        }
    }
    #endregion

    private object? draggedItem;
    private static InsertionAdorner? insertionAdorner;
    private EListViewItem? draggedContainer;
    private bool somethingDragging;
    public bool Dragging => somethingDragging;

    public delegate void EListViewDroppedDragItemHandler(object sender, object item, ListView source);
    [Category("Behavior")] public event EListViewDroppedDragItemHandler? DragItemDropped;

    public EListView()
    {
        WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        AllowDrop = IsDraggable;
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseMove += OnPreviewMouseMove;
        Drop += OnDrop;
        
        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        
        Unloaded += clearIndicatorDelegate;
        Loaded += clearIndicatorDelegate;
        void clearIndicatorDelegate(object sender, RoutedEventArgs e) => UpdateInsertionIndicator();
    }

    #region Implements
    static EListView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EListView), new FrameworkPropertyMetadata(typeof(EListView)));
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new EListViewItem();
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
        somethingDragging = true;
    }
    private Size spaceSize = default;
    private static int insertionIndex = -1;
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        var itemContainer = ((DependencyObject)e.OriginalSource).FindParent<EListViewItem>();
        var item = itemContainer?.DataContext;
        var ePosY = e.GetPosition(this).Y;
        insertionIndex = 0;
        if (item is not null && itemContainer is not null) insertionIndex = Items.IndexOf(item) + (e.GetPosition(itemContainer).Y >= itemContainer.ActualHeight / 2 ? 1 : 0);
        else if (Items.Count == 0) insertionIndex = 0; 
        else if (ItemContainerGenerator.ContainerFromIndex(Items.Count - 1) is EListViewItem lastContainer && ePosY > (lastContainer.TransformToVisual(this).Transform(new Point(0, lastContainer.ActualHeight)).Y)) insertionIndex = Items.Count;
        else
        {
            for (var i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is not EListViewItem container) continue;
                var itemTop = container.TransformToVisual(this).Transform(default).Y;
                var itemMidpoint = itemTop + container.ActualHeight / 2;
                if (ePosY < itemMidpoint)
                {
                    insertionIndex = i;
                    break;
                }
                insertionIndex = i + 1;
            }
        }
        //space position.
        Point relativePos = default;
        if (Items.Count != 0)
        {
            EListViewItem? relaItem0, relaItem1;
            double spaceHeight = 0;
            if (insertionIndex == 0)
            {
                relaItem0 = ItemContainerGenerator.ContainerFromIndex(0) as EListViewItem;
                relativePos = relaItem0?.GetPositionRelativeTo(PART_ItemsHandler!) ?? default;
            }
            else if (insertionIndex == Items.Count)
            {
                relaItem0 = ItemContainerGenerator.ContainerFromIndex(Items.Count - 1) as EListViewItem;
                var pos = relaItem0?.GetPositionRelativeTo(PART_ItemsHandler!) ?? default;
                relativePos = new(pos.X, pos.Y + relaItem0?.RenderSize.Height ?? 0);
            }
            else
            {
                relaItem0 = ItemContainerGenerator.ContainerFromIndex(insertionIndex - 1) as EListViewItem;
                relaItem1 = ItemContainerGenerator.ContainerFromIndex(insertionIndex) as EListViewItem;
                var pos0 = relaItem0?.GetPositionRelativeTo(PART_ItemsHandler!) ?? default;
                var pos1 = relaItem1?.GetPositionRelativeTo(PART_ItemsHandler!) ?? default;
                spaceHeight = pos1.Y - (pos0.Y + relaItem0?.RenderSize.Height ?? 0);
                relativePos = new((pos0.X + pos1.X) / 2, pos1.Y - spaceHeight / 2);
            }
            spaceSize = new(Math.Min(relaItem0?.RenderSize.Width ?? RenderSize.Width, RenderSize.Width) - relativePos.X * 2, Math.Max(spaceHeight, 0));
        }
        UpdateInsertionIndicator(relativePos);
    }
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        dragStartPoint = e.GetPosition(null);
        draggedContainer = ((DependencyObject)e.OriginalSource).FindParent<EListViewItem>();
    }
    private Point dragStartPoint = default;
    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && dragStartPoint != default && draggedContainer is not null)
        {
            Point currentPosition = e.GetPosition(null);
            Vector diff = dragStartPoint - currentPosition;
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                draggedItem = ItemContainerGenerator.ItemFromContainer(draggedContainer);
                if (draggedItem != null)
                {
                    var data = new DragData(draggedItem, draggedContainer, this);
                    DragDrop.DoDragDrop(draggedContainer, data, DragDropEffects.Move);
                    draggedContainer = null;
                    somethingDragging = false;
                }
            }
        }
    }
    private readonly static string DragDataDicIndex = typeof(DragData).ToString();
    private void OnDrop(object sender, DragEventArgs e)
    {
        UpdateInsertionIndicator();
        if (e.Data.GetData(DragDataDicIndex) is not DragData dragData) return;
        var item = dragData.Item;
        var source = dragData.Source;
        var targetItem = ((DependencyObject)e.OriginalSource).FindParent<EListViewItem>();

        var sourceColl = source.ItemsSource as IList ?? source.Items;
        var targetColl = ItemsSource as IList ?? Items;
        int newIndex = insertionIndex;
        int oldIndex = source == this ? sourceColl.IndexOf(item) : -1;
        if (oldIndex == newIndex) return;
        sourceColl.Remove(item);
        if (source == this && oldIndex < newIndex) newIndex--;
        targetColl.Insert(newIndex, item);
        Dispatcher.BeginInvoke(() => ScrollIntoView(item));

        DragItemDropped?.Invoke(this, item, source);
        draggedItem = null;
        somethingDragging = false;
        e.Handled = true;
        source.ReleaseMouseCapture();
        CaptureMouse();
        //
    }

    public ItemsPresenter? PART_ItemsHandler { get; protected set; }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_ItemsHandler = GetTemplateChild(nameof(PART_ItemsHandler)) as ItemsPresenter;
    }
    private void UpdateInsertionIndicator(Point? relaPos = null)
    {
        if (relaPos is not null)
        {
            if (insertionAdorner is not null && insertionAdorner.AdornedElement == this)
            {
                insertionAdorner.Update(relaPos.Value, spaceSize);
            }
            else
            {
                clearAdorner();
                var layer = PART_ItemsHandler is not null ? AdornerLayer.GetAdornerLayer(PART_ItemsHandler) : null;
                if(layer is null) return;
                try
                {
                    insertionAdorner = new InsertionAdorner(PART_ItemsHandler!, relaPos.Value, spaceSize, new Pen(new SolidColorBrush(Color.DodgerBlue.ToMediaColor()), 2));
                    insertionAdorner.Attach(layer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating adorner: {ex.Message}");
                    insertionAdorner = null;
                }
            }
        }
        else clearAdorner();
        static void clearAdorner()
        {
            if (insertionAdorner != null)
            {
                insertionAdorner.Detach();
                insertionAdorner = null;
            }
        }
    }


    private class InsertionAdorner : Adorner
    {
        private AdornerLayer? adornerLayer;
        private bool _isAttached;

        private Point relativePosition = default;
        private Size renderingSize = default;
        private readonly Pen pen;
        public InsertionAdorner(UIElement adornedElement, Point relaPos, Size drawSize, Pen pen)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            relativePosition = relaPos;
            renderingSize = drawSize;
            this.pen = pen;
        }
        public void Update(Point relaPos, Size drawSize)
        {
            if (relaPos != relativePosition || drawSize != renderingSize)
            {
                relativePosition = relaPos;
                renderingSize = drawSize;
                InvalidateVisual();
            }
        }
        public void Attach(AdornerLayer layer)
        {
            if (!_isAttached)
            {
                adornerLayer = layer;
                adornerLayer.Add(this);
                _isAttached = true;
            }
        }
        public void Detach()
        {
            if (_isAttached)
            {
                try
                {
                    adornerLayer?.Remove(this);
                }
                finally
                {
                    _isAttached = false;
                }
            }
        }
        protected override void OnRender(DrawingContext dc)
        {
            const double triangleSize = 8;
            base.OnRender(dc);
            double _left = relativePosition.X, _right = _left + renderingSize.Width + triangleSize;
            double height = relativePosition.Y;
            dc.DrawLine(pen, new Point(_left, height), new Point(_right, height));

            // 绘制三角形指示器
            
            Point p1 = new(_left, height - triangleSize / 2);
            Point p2 = new(_left + triangleSize, height);
            Point p3 = new(_left, height + triangleSize / 2);

            StreamGeometry triangle = new();
            using (StreamGeometryContext ctx = triangle.Open())
            {
                ctx.BeginFigure(p1, true, true);
                ctx.LineTo(p2, true, false);
                ctx.LineTo(p3, true, false);
            }

            dc.DrawGeometry(Brushes.DodgerBlue, null, triangle);

            // 右侧三角形
            p1 = new Point(_right, height - triangleSize / 2);
            p2 = new Point(_right - triangleSize, height);
            p3 = new Point(_right, height + triangleSize / 2);

            triangle = new StreamGeometry();
            using (StreamGeometryContext ctx = triangle.Open())
            {
                ctx.BeginFigure(p1, true, true);
                ctx.LineTo(p2, true, false);
                ctx.LineTo(p3, true, false);
            }

            dc.DrawGeometry(Brushes.DodgerBlue, null, triangle);
        }
    }
}
