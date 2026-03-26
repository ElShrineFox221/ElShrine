using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ElShrine.Wpf.Converters;

/// <summary>
/// 将进度值、内外半径和角度信息转换为用于绘制圆弧进度的 PathGeometry。
/// </summary>
public sealed class CircularProgressToPathConverter : IMultiValueConverter
{
    private static Point GetPoint(double radius, double angleInDegrees, Point center)
    {
        var angle = angleInDegrees - 90;
        var angleInRadians = angle * (Math.PI / 180.0);

        return new Point(
            center.X + radius * Math.Cos(angleInRadians),
            center.Y + radius * Math.Sin(angleInRadians)
        );
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 5 || values.Any(v => v == DependencyProperty.UnsetValue) ||
            values[0] is not double progress ||
            values[1] is not double outerRadius ||
            values[2] is not double innerRadius ||
            values[3] is not double startAngle ||
            values[4] is not double sweepAngle) return Geometry.Empty;
        if (outerRadius <= innerRadius || innerRadius < 0 || outerRadius <= 0) return Geometry.Empty;
        progress = Math.Clamp(progress, 0, 1);
        //
        if (parameter?.ToString()?.EqualIgnoreCase("rwidth") ?? false) innerRadius = outerRadius - innerRadius; 

        // 确定绘制参数
        var radius = (outerRadius + innerRadius) / 2.0;
        var center = new Point(outerRadius, outerRadius);
        var actualSweepAngle = progress * sweepAngle;
        var pathGeometry = new PathGeometry();
        var isClockwise = sweepAngle >= 0;
        var sweepDirection = isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
        var absActualSweepAngle = Math.Abs(actualSweepAngle);
        var startPoint = GetPoint(radius, startAngle, center);
        var pathFigure = new PathFigure { StartPoint = startPoint, IsClosed = false };

        if (absActualSweepAngle <= 180.0)
        {
            var endAngleNormalized = startAngle + actualSweepAngle;
            var endPoint = GetPoint(radius, endAngleNormalized, center);
            var arcSegment = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = false,
                SweepDirection = sweepDirection,
                IsStroked = true
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
        }
        else
        {
            var middleAngleNormalized = startAngle + (isClockwise ? 180.0 : -180.0);
            var middlePoint = GetPoint(radius, middleAngleNormalized, center);
            var arcSegment1 = new ArcSegment
            {
                Point = middlePoint,
                Size = new Size(radius, radius),
                IsLargeArc = false, 
                SweepDirection = sweepDirection,
                IsStroked = true
            };
            pathFigure.Segments.Add(arcSegment1);
            var remainingAngle = absActualSweepAngle - 180.0;

            var endAngleNormalized = startAngle + actualSweepAngle;
            var endPoint = GetPoint(radius, endAngleNormalized, center);

            var arcSegment2 = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = false,
                SweepDirection = sweepDirection,
                IsStroked = true
            };

            pathFigure.Segments.Add(arcSegment2);
            pathGeometry.Figures.Add(pathFigure);
        }

        return pathGeometry;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Conversion Back Not Supported.");
    }
}