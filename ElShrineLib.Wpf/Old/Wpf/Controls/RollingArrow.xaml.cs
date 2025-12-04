using ElShrine.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Methods = ElShrine.Old.Wpf.Methods;

namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class RollingArrow : Button
    {

        public double SineAngle { get { return SineAngleHPIFactor * Math.PI / 2d; } }
        public int SineAngleHPIFactor = 0;
        public double ActualSineAngle = 0;
        public readonly DispatcherTimer AnimationTimer = new() { Interval = TimeSpan.FromSeconds(1) / 60d, };
        private int AnimationCount = 0;
        private bool ATimerInitialized = false;
        private TimeSpan timeSpan;
        private int totalCount;
        private double nowAngle;
        private double targetAngle;
        private bool rolling = true;
        public bool Rolling
        {
            get => rolling;
            set
            {
                rolling = value;
                if (value && !AnimationTimer.IsEnabled) Roll();
            }
        }
        public bool ForceRolling { get; set; } = false;

        public readonly DoubleAnimation SineAngleAnimation = new()
        {
            EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, },
        };
        public double StartAngle = Math.PI / 2d;
        public Polygon? polygon;
        private double arrowLength = 0;
        public double ArrowLength { get { return arrowLength; } }
        private double arrowYOffset = 0;
        public double ArrowYOffset { get { return arrowYOffset; } }

        public delegate void SineAngleChangedHandler(double angle);
        public event SineAngleChangedHandler? SineAngleChanged;
        public void InitailizeRolling()
        {
            if (ATimerInitialized) return;
            ATimerInitialized = true;
            RefreshArrowRender();
            SizeChanged += delegate
            {
                arrowYOffset = ActualHeight / 2;
            };
            RefreshAttributes_();
            AnimationTimer.Tick += delegate
            {
                if (AnimationCount++ < totalCount)
                {
                    ActualSineAngle = nowAngle + AnimationCount / (double)totalCount * (targetAngle - nowAngle);
                    SineAngleChanged?.Invoke(ActualSineAngle);
                }
                else
                {
                    AnimationCount = 0;
                    SineAngleChanged?.Invoke(targetAngle);
                    Roll();
                }
            };
            Roll();
        }
        public void RefreshArrowRender()
        {
            arrowLength = FontSize * 2;
            arrowYOffset = ActualHeight / 2d;
        }
        private void RefreshAttributes_()
        {
            AnimationCount = 0;
            var instance = Option.GetInstance();
            timeSpan = TimeSpan.FromMilliseconds(instance.LayTimeSpanMS) * instance.ArrowRollingTimeSpanFactor;
            totalCount = (int)(timeSpan.TotalSeconds * 60);
            nowAngle = SineAngleHPIFactor * Math.PI / 2d + StartAngle;
            SineAngleHPIFactor += 2;
            targetAngle = SineAngleHPIFactor * Math.PI / 2d + StartAngle;
            SineAngleHPIFactor %= 4;
        }
        private void Roll()
        {
            if (Rolling || ForceRolling)
            {
                AnimationTimer.Stop();
                RefreshAttributes_();
                AnimationTimer.Start();
            }
            else AnimationTimer.Stop();
        }
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public partial class RollingArrowEvent
    {
        private void RollingArrow_Loaded(object sender, RoutedEventArgs e)
        {
            RollingArrow arrow = (RollingArrow)sender;
            arrow.polygon = (Polygon?)ElShrine.Wpf.Methods.FindChild(arrow, "FillArrow");
            PointCollection? points = null;
            arrow.InitailizeRolling();
            arrow.Rolling = false;
            arrow.SineAngleChanged += delegate (double value)
            {
                if (arrow.polygon == null) return;
                arrow.polygon.MinWidth = arrow.ArrowLength;
                points = arrow.polygon.Points;
                if (points == null || points.Count < 1) return;
                double reuslt = Math.Sin(value) * 2 * arrow.FontSize / 7;
                arrow.polygon.Opacity = Math.Pow((-Math.Cos(value * 2) + 1) / 2, 0.25);
                bool IsRight = arrow.HorizontalAlignment == HorizontalAlignment.Right;
                points[0] = new Point(IsRight ? 0 : arrow.ArrowLength, arrow.ArrowYOffset);
                points[1] = new(!IsRight ? 0 : arrow.ArrowLength, reuslt + arrow.ArrowYOffset);
                points[2] = new(!IsRight ? 0 : arrow.ArrowLength, -reuslt + arrow.ArrowYOffset);
            };
        }
        private void RollingArrow_MouseEnter(object sender, MouseEventArgs e)
        {
            RollingArrow arrow = (RollingArrow)sender;
            arrow.Rolling = true;
        }
        private void RollingArrow_MouseLeave(object sender, MouseEventArgs e)
        {
            RollingArrow arrow = (RollingArrow)sender;
            arrow.Rolling = false;
        }
        private void RollingArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RollingArrow arrow = (RollingArrow)sender;
            arrow.ForceRolling = !arrow.ForceRolling;
        }
    }
}
