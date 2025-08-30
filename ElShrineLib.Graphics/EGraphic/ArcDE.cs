/*using ElShrine.Old;
using System.Drawing;

namespace ElShrine.EGraphic
{
    public record ArcDE(PointF Center, Coordinate2D CircleSize, Color Color, double InnerRadius, float StartAngle, double SweepAngle) : DrawingElementBase()
    {
        public override void Draw(Graphics graphics)
        {
            float penWidth = (float)(CircleSize.X / 2d - InnerRadius);
            SizeF drawingSize = new((float)CircleSize.X - penWidth, (float)CircleSize.Y - penWidth);
            RectangleF drawingRec = new(new(0, 0), drawingSize);

            PointF p = MainCor.ToPointF(canvasSize);
            p = p.Offset(new(-drawingSize.Width / 2f, -drawingSize.Height / 2));
            Pen pen = new(Color, penWidth) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            OffsetDraw((g) => g.DrawArc(pen, drawingRec, startAngle, sweepAngle), graphics, p);
        }
    }
}
*/