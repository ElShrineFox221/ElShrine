using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Size = System.Windows.Size;


namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class ElShrineStackPanel : StackPanel, IScrollInfo
    {
        private readonly TranslateTransform TransForm = new();
        public ElShrineStackPanel()
        {
            RenderTransform = TransForm;
        }
        #region Layout
        Size _screenSize;
        Size totalSize;
        public Size TotalSize
        {
            get { return totalSize; }
            private set
            {
                if (value != totalSize)
                {
                    totalSize = value;
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (_screenSize.Height < TotalSize.Height)
                        {
                            EntityOffset(0, double.PositiveInfinity);
                        }
                    });
                }
            }
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            _screenSize = availableSize;
            if (Orientation == Orientation.Horizontal)
            {
                availableSize = new Size(availableSize.Width, double.PositiveInfinity);
            }
            else
            {
                availableSize = new Size(double.PositiveInfinity, availableSize.Height);
            }

            TotalSize = base.MeasureOverride(availableSize);
            return TotalSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            _ = base.ArrangeOverride(finalSize);
            if (ScrollOwner != null)
            {
                var intance = Option.GetInstance();
                IEasingFunction easingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut, };
                var yOffsetAnimation = new DoubleAnimation() { To = -VerticalOffset, Duration = Math.Max(1, Math.Sqrt(Math.Abs((-TransForm.Y - VerticalOffset) / intance.LayTimeSpanFactorInPixel))) * TimeSpan.FromMilliseconds(intance.LayTimeSpanMS), EasingFunction = easingFunction, };
                TransForm.BeginAnimation(TranslateTransform.YProperty, yOffsetAnimation);
                var xOffsetAnimation = new DoubleAnimation() { To = -HorizontalOffset, Duration = Math.Max(1, Math.Sqrt(Math.Abs(-TransForm.X - HorizontalOffset / intance.LayTimeSpanFactorInPixel))) * TimeSpan.FromMilliseconds(intance.LayTimeSpanMS), EasingFunction = easingFunction, };
                TransForm.BeginAnimation(TranslateTransform.XProperty, xOffsetAnimation);
                ScrollOwner.InvalidateScrollInfo();
            }
            return _screenSize;
        }
        public void ReturnZeroPos()
        {
            EntityOffset(0, 0);
        }
        #endregion
        #region IScrollInfos
        public new ScrollViewer? ScrollOwner { get; set; }
        public new bool CanHorizontallyScroll { get; set; }
        public new bool CanVerticallyScroll { get; set; }
        public new double ExtentHeight { get { return TotalSize.Height; } }
        public new double ExtentWidth { get { return TotalSize.Width; } }
        public new double HorizontalOffset { get; private set; }
        public new double VerticalOffset { get; private set; }
        public new double ViewportHeight { get { return _screenSize.Height; } }
        public new double ViewportWidth { get { return _screenSize.Width; } }
        void EntityOffset(double x, double y)
        {
            var offset = new Vector(HorizontalOffset + x, VerticalOffset + y);
            offset.Y = Intermediate(offset.Y, 0, TotalSize.Height - _screenSize.Height);
            offset.X = Intermediate(offset.X, 0, TotalSize.Width - _screenSize.Width);
            HorizontalOffset = offset.X;
            VerticalOffset = offset.Y;
            InvalidateArrange();
        }
        static double Intermediate(double value, double value1, double value2)
        {
            var min = Math.Min(value1, value2);
            var max = Math.Max(value1, value2);
            value = Math.Max(value, min);
            value = Math.Min(value, max);
            return value;
        }
        public double LineOffset { get; set; } = 30;
        public double WheelOffset { get; set; } = 90;
        #region Interface Methods
        public new void LineDown() { EntityOffset(0, LineOffset); }
        public new void LineUp() { EntityOffset(0, -LineOffset); }
        public new void LineLeft() { EntityOffset(-LineOffset, 0); }
        public new void LineRight() { EntityOffset(LineOffset, 0); }
        public new Rect MakeVisible(Visual visual, Rect rectangle)
        {
            FrameworkElement frameworkElement = (FrameworkElement)visual;
            return new Rect(0, 0, frameworkElement.Width, frameworkElement.Height);
        }
        public new void MouseWheelDown() { EntityOffset(0, WheelOffset); }
        public new void MouseWheelUp() { EntityOffset(0, -WheelOffset); }
        public new void MouseWheelLeft() { EntityOffset(0, WheelOffset); }
        public new void MouseWheelRight() { EntityOffset(WheelOffset, 0); }
        public new void PageDown() { EntityOffset(0, _screenSize.Height); }
        public new void PageUp() { EntityOffset(0, -_screenSize.Height); }
        public new void PageLeft() { EntityOffset(-_screenSize.Width, 0); }
        public new void PageRight() { EntityOffset(_screenSize.Width, 0); }
        public new void SetVerticalOffset(double offset) { EntityOffset(HorizontalOffset, offset - VerticalOffset); }
        public new void SetHorizontalOffset(double offset) { EntityOffset(offset - HorizontalOffset, VerticalOffset); }
        #endregion
        #endregion
    }
}
