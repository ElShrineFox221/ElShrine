using System.Drawing;

namespace ElShrine.Old
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class CoordinateHelper
    {
        public static PointF ToPointF(this Coordinate2D coordinate)
            => new((float)coordinate.X, (float)coordinate.Y);

        public static SizeF ToSizeF(this Coordinate2D coordinate)
            => new((float)coordinate.X, (float)coordinate.Y);
        public static Size ToFloorSize(this Coordinate2D coordinate)
            => new((int)Math.Floor(coordinate.X), (int)Math.Floor(coordinate.Y));
        public static Size ToCelingSize(this Coordinate2D coordinate)
            => new((int)Math.Ceiling(coordinate.X), (int)Math.Ceiling(coordinate.Y));

        public static PointF ToOffset(this PointF p, PointF delta)
            => new(p.X + delta.X, p.Y + delta.Y);
    }
}
