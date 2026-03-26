using System.Drawing;

namespace ElShrine;

public static class PointHelper
{
    public static SizeF ToSizeF(this ref PointF point)
        => new(point.X, point.Y);
    public static PointF ToPointF(this ref SizeF sizeF)
        => new(sizeF.Width, sizeF.Height);

    public static Size ToFloorSize(this ref PointF pointF)
        => new((int)Math.Floor(pointF.X), (int)Math.Floor(pointF.Y));
    public static Size ToCelingSize(this ref PointF pointF)
        => new((int)Math.Ceiling(pointF.X), (int)Math.Ceiling(pointF.Y));

    public static Size ToFloorSize(this ref SizeF sizeF)
        => new((int)Math.Floor(sizeF.Width), (int)Math.Floor(sizeF.Height));
    public static Size ToCelingSize(this ref SizeF sizeF)
        => new((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
    
    public static PointF ToPointF(this ref Size size)
        => new(size.Width, size.Height);

    public static PointF ToOffset(this ref PointF p, ref PointF delta)
        => new(p.X + delta.X, p.Y + delta.Y);
}
