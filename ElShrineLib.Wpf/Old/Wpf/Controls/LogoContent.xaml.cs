using ElShrine.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static ElShrine.Wpf.Methods;

namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class ElShrineLogoContent : ContentControl
    {
        public ElShrineLogoContent() { }
        //TempValues
        #region Param//DrawingTimeLine
        private double drawingTimeLine = 0;
        public double DrawingTimeLine
        {
            get => drawingTimeLine; private set
            {
                if (value > 1) drawingTimeLine = 1;
                else if (value < 0) drawingTimeLine = 0;
                else drawingTimeLine = value;
            }
        }
        #endregion
        #region Param//DrawingRateRanges
        public record struct RateRange(double Start, double End) { public readonly double Range => End - Start; };
        private readonly record struct RateRanges(
            RateRange Round0TurnRange,
            RateRange Round1TurnRange,
            RateRange LineDBRange,
            RateRange LineCBRange,
            RateRange LineFE1Range,
            RateRange LineEF2Range,
            RateRange Round1Turn2Range,
            RateRange LineDCRange,
            RateRange Round2TurnRange,
            RateRange LineTORange
            );
        private readonly static RateRanges DrawingRateRanges =
        new(
            new(0.000d, 0.700d),
            new(0.500d, 0.850d),
            new(0.351d, 0.700d),
            new(0.630d, 0.850d),
            new(0.635d, 0.850d),
            new(0.800d, 0.900d),
            new(0.800d, 1.000d),
            new(0.351d, 0.630d),
            new(0.800d, 1.000d),
            new(0.900d, 1.000d)
        );
        public static RateRange[] DrawingRateRange => [
            DrawingRateRanges.Round0TurnRange,
            DrawingRateRanges.Round1TurnRange,
            DrawingRateRanges.Round1Turn2Range,
            DrawingRateRanges.LineDBRange,
            DrawingRateRanges.LineCBRange,
            DrawingRateRanges.LineFE1Range,
            DrawingRateRanges.LineEF2Range,
            DrawingRateRanges.LineDCRange,
            DrawingRateRanges.Round2TurnRange,
            DrawingRateRanges.LineTORange,
        ];
        #endregion

        //Methods
        #region PrivateMethod//GetRateFromTimeLine
        public static TimeSpan GetBeginTime(RateRange rateRange, Duration total)
                => total.TimeSpan * rateRange.Start;
        public static Duration GetDuration(RateRange rateRange, Duration total)
                => total.Factor(rateRange.Range);
        //WaitTime
        //Duration
        //To/RecordFrom
        #endregion

        #region Methods
        private bool HasLoadedPre = true;
        public bool Drawn { get; private set; } = false;
        private static readonly double pai = Math.PI;
        public int FlashPerSecond { get; set; } = 60;

        #region Names
        private const string LogoContainer_CanvasName = "LogoContainer_Canvas";
        private const string Canvas_Arc0Name = "Canvas_Arc0";
        private const string Canvas_Arc1Name = "Canvas_Arc1";
        private const string Canvas_Arc2Name = "Canvas_Arc2";
        private const string Canvas_Arc3Name = "Canvas_Arc3";
        private const string Canvas_Arc4Name = "Canvas_Arc4";
        private const string Canvas_Arc5Name = "Canvas_Arc5";
        private const string Canvas_Line0Name = "Canvas_Line0";
        private const string Canvas_Line1Name = "Canvas_Line1";
        private const string Canvas_Line2Name = "Canvas_Line2";
        private const string Canvas_Line3Name = "Canvas_Line3";
        private const string Canvas_Line4Name = "Canvas_Line4";
        private const string Canvas_Line5Name = "Canvas_Line5";
        private const string Canvas_Line6Name = "Canvas_Line6";
        private const string Canvas_Line7Name = "Canvas_Line7";
        #endregion

        public static void DrawLogos(Duration duration, object sender, string name)
        {
#pragma warning disable CS8602

            #region Step 1 - Confrim Elements
            List<UIElement?> UIElements = [];

            ElShrineLogoContent? LogoContainer = (ElShrineLogoContent?)((FrameworkElement)sender).FindChild(name);

            Canvas? LogoContainer_Canvas = (Canvas?)LogoContainer?.FindChild(LogoContainer_CanvasName);

            Arc? Arc0 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc0Name);
            Arc? Arc1 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc1Name);

            Line? Line0 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line0Name);
            Line? Line1 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line1Name);
            Line? Line2 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line2Name);
            Line? Line3 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line3Name);

            Arc? Arc2 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc2Name);
            Line? Line4 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line4Name);
            Arc? Arc3 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc3Name);
            Arc? Arc4 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc4Name);
            Arc? Arc5 = (Arc?)LogoContainer_Canvas?.FindChild(Canvas_Arc5Name);
            Line? Line5 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line5Name);
            Line? Line6 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line6Name);
            Line? Line7 = (Line?)LogoContainer_Canvas?.FindChild(Canvas_Line7Name);

            UIElements.AddRange([
                LogoContainer, LogoContainer_Canvas,
                Arc0, Arc1,
                Line0, Line1, Line2, Line3,
                Arc2, Line4, Arc3, Arc4, Arc5, Line5, Line6, Line7,
            ]);
            foreach (var element in UIElements) if (element is null) return;//null reference check

            if (!LogoContainer.HasLoadedPre) return;
            #endregion

            #region Step 2 - Initalize Elements
            //
            LogoContainer.DrawingTimeLine = 0;
            //LogoContainer

            LogoContainer_Canvas.Visibility = Visibility.Visible;
            SimpleDoubleAnimation(duration.TimeSpan / 10, 1, null, null, LogoContainer_Canvas, OpacityProperty);
            //Canvas

            foreach (var item in UIElements)
            {
                if (item is Shape shape)
                {
                    shape.StrokeEndLineCap = PenLineCap.Round;
                    shape.StrokeStartLineCap = PenLineCap.Round;
                }
            }
            //StrokeLineCap

            double interval = 1000 / LogoContainer.FlashPerSecond;
            int limit = (int)(duration.TimeSpan.TotalMilliseconds / interval);
            LogoContainer.HasLoadedPre = false;
            LogoContainer.Drawn = false;
            int count = 0;
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(interval), };

            double tickMethod()
            {
                if (count >= limit)
                {
                    count = limit;
                    LogoContainer.HasLoadedPre = true;
                    LogoContainer.Drawn = true;
                    timer.Stop();
                }
                count++;
                double returnValue = (1 + Math.Sin(pai * (-0.5 + count / (double)limit))) / 2;
                return returnValue;
            }
            //TimeLine - DispatcherTimer
            #endregion

            #region Step 3 - Drawing Method

            #region -Stanard Data
            float side = (float)(0.06f * Math.Min(LogoContainer.Height, LogoContainer.Width));
            double max = Math.Max(LogoContainer.Height - LogoContainer.Width, LogoContainer.Width - LogoContainer.Height);
            float startX = (float)((max == LogoContainer.Width - LogoContainer.Height ? max : 0) / 2f + side);
            float startY = (float)((max == LogoContainer.Height - LogoContainer.Width ? max : 0) / 2f + side);
            float roundR = (float)(LogoContainer.Height / 2 - side);
            float roundD = (float)(LogoContainer.Height - 2 * side);
            Point TOD = new(startX + 1.888f * roundR, startY + 0.54f * roundR);
            Point TOB = new(startX + roundR, startY + 1.572f * roundR);
            Point TOC = new(startX + 0.112f * roundR, startY + 0.54f * roundR);
            Point TOE1 = new(startX + 0.834f * roundR, 1.38f * roundR + side);
            Point TOF1 = new(startX + 1.166f * roundR, 1.38f * roundR + side);
            Point TOE2 = new(startX + 0.88f * roundR, 1.428f * roundR + side);
            Point TOF2 = new(startX + 1.122f * roundR, 1.428f * roundR + side);
            Rect rect = new(startX + 5 * roundR / 7f, startY + 10 * roundR / 11f - 2 * roundR / 7f, 4 * roundR / 7f, 4 * roundR / 7f);
            Point TOH = new(startX + 0.752f * roundR, startY + 1.052f * roundR);
            Point TOI = new(startX + 1.248f * roundR, startY + 1.052f * roundR);
            Point TOG = new(startX + roundR, startY + (10 / 11f - 2 / 7f) * roundR);
            #endregion
            timer.Tick += delegate
            {
                LogoContainer.DrawingTimeLine = tickMethod();
                RefreshElements();
            };
            timer.Start();
            void RefreshElements()
            {
                #region Data Inner 0 -Stanard Data
                side = (float)(0.06f * Math.Min(LogoContainer.Height, LogoContainer.Width));
                double max = Math.Max(LogoContainer.Height - LogoContainer.Width, LogoContainer.Width - LogoContainer.Height);
                startX = (float)((max == LogoContainer.Width - LogoContainer.Height ? max : 0) / 2f + side);
                startY = (float)((max == LogoContainer.Height - LogoContainer.Width ? max : 0) / 2f + side);
                roundR = (float)(LogoContainer.Height / 2 - side);
                roundD = (float)(LogoContainer.Height - 2 * side);
                rect = new(startX + 5 * roundR / 7f, startY + 10 * roundR / 11f - 2 * roundR / 7f, 4 * roundR / 7f, 4 * roundR / 7f);
                #endregion
                #region Data Inner 1 -Draw
                foreach (var item in UIElements)
                {
                    if (item is Shape shape)
                    {
                        if (shape is Line l)
                        {
                            if (l.X1 == l.X2 && l.Y1 == l.Y2) l.Visibility = Visibility.Hidden;
                            else l.Visibility = Visibility.Visible;
                        }
                        if (shape is Arc a)
                        {
                            if (a.EndAngle == 0) a.Visibility = Visibility.Hidden;
                            else a.Visibility = Visibility.Visible;
                        }
                    }
                }
                RefreshArcArea(Arc0, new Rect(startX, side, roundD, roundD));
                RefreshArcArea(Arc1, new Rect(startX + 2f * roundR / 3f, side + (int)(4 * roundR / 3), 2f * roundR / 3f, 2f * roundR / 3f));
                RefreshArcArea(Arc2, new Rect(startX + 2f * roundR / 3f, side + (int)(4 * roundR / 3), 2f * roundR / 3f, 2f * roundR / 3f));
                RefreshArcArea(Arc3, rect);
                RefreshArcArea(Arc4, rect);
                RefreshArcArea(Arc5, rect);
                void RefreshArcArea(Arc? arc, Rect rectangleF)
                {
                    if (arc is not null) arc.Rect = rectangleF;
                }
                #endregion
            }

            ArcAnimation(Arc0, DrawingRateRanges.Round0TurnRange, 260, 360, 70, 0);
            ArcAnimation(Arc1, DrawingRateRanges.Round1TurnRange, 90, 150.42, 90, 0);
            ArcAnimation(Arc2, DrawingRateRanges.Round1Turn2Range, 300, 150.42, 200, 0);
            ArcAnimation(Arc3, DrawingRateRanges.Round2TurnRange, 210, -120, 330, 0);
            ArcAnimation(Arc4, DrawingRateRanges.Round2TurnRange, 90, -120, 210, 0);
            ArcAnimation(Arc5, DrawingRateRanges.Round2TurnRange, -30, -120, 90, 0);
            void ArcAnimation(Arc? arc, RateRange rateRange, double startTo, double endTo,
                double? startFrom = null, double? endFrom = null)
            {
                TimeSpan? bt = GetBeginTime(rateRange, duration);
                Duration d = GetDuration(rateRange, duration);
                if (arc is not null)
                {
                    SimpleDoubleAnimation(d, startTo, startFrom, null, arc, Arc.StartAngleProperty, bt);
                    SimpleDoubleAnimation(d, endTo, endFrom, null, arc, Arc.EndAngleProperty, bt);
                }
            }
            LineAnimation(Line0, DrawingRateRanges.LineDBRange, TOD, TOB, TOD, TOD);
            LineAnimation(Line1, DrawingRateRanges.LineCBRange, TOC, TOB, TOC, TOC);
            LineAnimation(Line2, DrawingRateRanges.LineFE1Range, TOF1, TOE1, TOF1, TOF1);
            LineAnimation(Line3, DrawingRateRanges.LineEF2Range, TOE2, TOF2, TOE2, TOE2);
            LineAnimation(Line4, DrawingRateRanges.LineDCRange, TOD, TOC, TOD, TOD);
            LineAnimation(Line5, DrawingRateRanges.LineTORange, TOH, TOI, TOH, TOH);
            LineAnimation(Line6, DrawingRateRanges.LineTORange, TOI, TOG, TOI, TOI);
            LineAnimation(Line7, DrawingRateRanges.LineTORange, TOG, TOH, TOG, TOG);
            void LineAnimation(Line? line, RateRange rateRange, Point point1To, Point point2To,
                Point? point1From = null, Point? point2From = null)
            {
                TimeSpan? bt = GetBeginTime(rateRange, duration);
                Duration d = GetDuration(rateRange, duration);
                if (line is not null)
                {
                    SimpleDoubleAnimation(d, point1To.X, point1From?.X, null, line, Line.X1Property, bt);
                    SimpleDoubleAnimation(d, point2To.X, point2From?.X, null, line, Line.X2Property, bt);
                    SimpleDoubleAnimation(d, point1To.Y, point1From?.Y, null, line, Line.Y1Property, bt);
                    SimpleDoubleAnimation(d, point2To.Y, point2From?.Y, null, line, Line.Y2Property, bt);
                }
            }
            #endregion

#pragma warning restore CS8602
        }
        public static void SetOutTimer(Duration duration, object sender, string name)
        {
            ElShrineLogoContent? LogoContainer = (ElShrineLogoContent?)((FrameworkElement)sender).FindChild(name);
            Canvas? LogoContainer_Canvas = (Canvas?)LogoContainer?.FindChild(LogoContainer_CanvasName);
            if (LogoContainer is null || LogoContainer_Canvas is null) return;
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(20), };
            timer.Tick += delegate
            {
                if (LogoContainer.Drawn)
                {
                    LogoContainer.HasLoadedPre = false;
                    timer.Stop();
                    SimpleDoubleAnimation(duration, 1, null, () =>
                    {
                        LogoContainer.Drawn = false;
                        LogoContainer.HasLoadedPre = true;
                    }, LogoContainer_Canvas, OpacityProperty);
                }
            };
            timer.Start();
        }
        #endregion
    }
}
