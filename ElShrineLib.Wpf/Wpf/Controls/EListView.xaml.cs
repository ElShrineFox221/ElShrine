using ElShrine.Wpf.UITheme;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.Controls
{
    public class EListView : ListView
    {
        #region UI

        public EScrollViewer? ScrollViewer { get; protected set; }

        #region DPs

        #region Theme
        public CornerRadius BorderCornerRadius
        {
            get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
            set => SetValue(BorderCornerRadiusProperty, value);
        }
        public Brush FontBrush
        {
            get => (Brush)GetValue(FontBrushProperty);
            set => SetValue(FontBrushProperty, value);
        }
        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }
        public Brush ClickBrush
        {
            get => (Brush)GetValue(ClickBrushProperty);
            set => SetValue(ClickBrushProperty, value);
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(EListView), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(EListView), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(EListView), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(EListView), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        #region ScrollBar

        public object CornerContent
        {
            get => GetValue(CornerContentProperty);
            set => SetValue(CornerContentProperty, value);
        }
        public ScrollBarBtnVisibility VerticalScrollBarBtnVisibility
        {
            get => (ScrollBarBtnVisibility)GetValue(VerticalScrollBarBtnVisibilityProperty);
            set => SetValue(VerticalScrollBarBtnVisibilityProperty, value);
        }
        public ScrollBarBtnVisibility HorizontalScrollBarBtnVisibility
        {
            get => (ScrollBarBtnVisibility)GetValue(HorizontalScrollBarBtnVisibilityProperty);
            set => SetValue(HorizontalScrollBarBtnVisibilityProperty, value);
        }
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }
        public double VerticalScrollBarWidth
        {
            get => (double)GetValue(VerticalScrollBarWidthProperty);
            set => SetValue(VerticalScrollBarWidthProperty, value);
        }
        public double HorizontalScrollBarHeight
        {
            get => (double)GetValue(HorizontalScrollBarHeightProperty);
            set => SetValue(HorizontalScrollBarHeightProperty, value);
        }
        public Thickness VerticalScrollBarMargin
        {
            get => (Thickness)GetValue(VerticalScrollBarMarginProperty);
            set => SetValue(VerticalScrollBarMarginProperty, value);
        }
        public Thickness HorizontalScrollBarMargin
        {
            get => (Thickness)GetValue(HorizontalScrollBarMarginProperty);
            set => SetValue(HorizontalScrollBarMarginProperty, value);
        }

        public static readonly DependencyProperty CornerContentProperty = DependencyProperty.Register(nameof(CornerContent), typeof(object), typeof(EListView), new(null));
        public static readonly DependencyProperty VerticalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarBtnVisibility), typeof(ScrollBarBtnVisibility), typeof(EListView), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty HorizontalScrollBarBtnVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarBtnVisibility), typeof(ScrollBarBtnVisibility), typeof(EListView), new(ScrollBarBtnVisibility.All));
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(EListView), new(ScrollBarVisibility.Auto));
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(EListView), new(ScrollBarVisibility.Auto));
        public static readonly DependencyProperty VerticalScrollBarWidthProperty = DependencyProperty.Register(nameof(VerticalScrollBarWidth), typeof(double), typeof(EListView), new(10d));
        public static readonly DependencyProperty HorizontalScrollBarHeightProperty = DependencyProperty.Register(nameof(HorizontalScrollBarHeight), typeof(double), typeof(EListView), new(10d));
        public static readonly DependencyProperty VerticalScrollBarMarginProperty = DependencyProperty.Register(nameof(VerticalScrollBarMargin), typeof(Thickness), typeof(EListView), new(new Thickness(0)));
        public static readonly DependencyProperty HorizontalScrollBarMarginProperty = DependencyProperty.Register(nameof(HorizontalScrollBarMargin), typeof(Thickness), typeof(EListView), new(new Thickness(0)));

        #endregion

        #region Items
        public double SelectedItemLineWidth
        {
            get => (double)GetValue(SelectedItemLineWidthProperty);
            set => SetValue(SelectedItemLineWidthProperty, value);
        }
        public double SelectedItemLineHeightRate
        {
            get => (double)GetValue(SelectedItemLineHeightRateProperty);
            set => SetValue(SelectedItemLineHeightRateProperty, value);
        }
        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }
        public Thickness ItemPadding
        {
            get => (Thickness)GetValue(ItemPaddingProperty);
            set => SetValue(ItemPaddingProperty, value);
        }
        public Thickness ItemBorderThickness
        {
            get => (Thickness)GetValue(ItemBorderThicknessProperty);
            set => SetValue(ItemBorderThicknessProperty, value);
        }
        public CornerRadius ItemBorderCornerRadius
        {
            get => (CornerRadius)GetValue(ItemBorderCornerRadiusProperty);
            set => SetValue(ItemBorderCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SelectedItemLineWidthProperty = DependencyProperty.Register(nameof(SelectedItemLineWidth), typeof(double), typeof(EListView), new(4d));
        public static readonly DependencyProperty SelectedItemLineHeightRateProperty = DependencyProperty.Register(nameof(SelectedItemLineHeightRate), typeof(double), typeof(EListView), new(0.8d));
        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(EListView), new(new Thickness(2)));
        public static readonly DependencyProperty ItemPaddingProperty = DependencyProperty.Register(nameof(ItemPadding), typeof(Thickness), typeof(EListView), new(new Thickness(4)));
        public static readonly DependencyProperty ItemBorderThicknessProperty = DependencyProperty.Register(nameof(ItemBorderThickness), typeof(Thickness), typeof(EListView), new(new Thickness(1)));
        public static readonly DependencyProperty ItemBorderCornerRadiusProperty = DependencyProperty.Register(nameof(ItemBorderCornerRadius), typeof(CornerRadius), typeof(EListView), new(Theme.Default.CornerRadius));

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ScrollViewer = GetTemplateChild("contentContainer") as EScrollViewer;
        }
        #endregion


        private record DragData(object Item, ListViewItem Container, ListView Source);

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
        private ListViewItem? draggedContainer;
        private bool somethingDragging;
        public bool Dragging => somethingDragging;

        public delegate void EListViewDroppedDragItemHandler(object sender, object item, ListView source);
        [Category("Behavior")] public event EListViewDroppedDragItemHandler? DragItemDropped;

        static EListView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EListView), new FrameworkPropertyMetadata(typeof(ListView)));
        public EListView()
        {
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

            var itemContainer = ((DependencyObject)e.OriginalSource).FindParent<ListViewItem>();
            var item = itemContainer?.DataContext;
            var ePosY = e.GetPosition(this).Y;
            insertionIndex = 0;
            if (item is not null && itemContainer is not null) insertionIndex = Items.IndexOf(item) + (e.GetPosition(itemContainer).Y >= itemContainer.ActualHeight / 2 ? 1 : 0);
            else if (Items.Count == 0) insertionIndex = 0; 
            else if (ItemContainerGenerator.ContainerFromIndex(Items.Count - 1) is ListViewItem lastContainer && ePosY > (lastContainer.TransformToVisual(this).Transform(new Point(0, lastContainer.ActualHeight)).Y)) insertionIndex = Items.Count;
            else
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (ItemContainerGenerator.ContainerFromIndex(i) is not ListViewItem container) continue;
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
                ListViewItem? relaItem0, relaItem1;
                double spaceHeight = 0;
                if (insertionIndex == 0)
                {
                    relaItem0 = ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                    relativePos = relaItem0?.GetPositionRelativeTo(this) ?? default;
                }
                else if (insertionIndex == Items.Count)
                {
                    relaItem0 = ItemContainerGenerator.ContainerFromIndex(Items.Count - 1) as ListViewItem;
                    var pos = relaItem0?.GetPositionRelativeTo(this) ?? default;
                    relativePos = new(pos.X, pos.Y + relaItem0?.RenderSize.Height ?? 0);
                }
                else
                {
                    relaItem0 = ItemContainerGenerator.ContainerFromIndex(insertionIndex - 1) as ListViewItem;
                    relaItem1 = ItemContainerGenerator.ContainerFromIndex(insertionIndex) as ListViewItem;
                    var pos0 = relaItem0?.GetPositionRelativeTo(this) ?? default;
                    var pos1 = relaItem1?.GetPositionRelativeTo(this) ?? default;
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
            draggedContainer = ((DependencyObject)e.OriginalSource).FindParent<ListViewItem>();
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
            var targetItem = ((DependencyObject)e.OriginalSource).FindParent<ListViewItem>();

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
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                    try
                    {
                        insertionAdorner = new InsertionAdorner(this, relaPos.Value, spaceSize, new Pen(new SolidColorBrush(Color.DodgerBlue.ToMediaColor()), 2));
                        insertionAdorner.Attach(layer);
                    }
                    catch (Exception ex)
                    {
                        // 处理可能的异常
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
                base.OnRender(dc);
                double _left = relativePosition.X, _right = _left + renderingSize.Width;
                double height = relativePosition.Y;
                dc.DrawLine(pen, new Point(_left, height), new Point(_right, height));

                // 绘制三角形指示器
                double triangleSize = 8;
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
}
