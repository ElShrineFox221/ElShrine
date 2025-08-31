using ElShrine.VisualTool.MapEditor.ViewModel;
using System.Drawing;
using System.Windows;
using PointD = System.Windows.Point;
using Size = System.Drawing.Size;

namespace ElShrine.VisualTool.MapEditor.Model
{
    public class DrawingElePainter : BitmapPainterBase
    {
        public bool AutoUpdate = false;
        private DrawingElement[] drawingElements;
        public Pen Pen = new(Color.Black, 20);

        public DrawingElePainter(Bitmap image, DrawingElement[] eles) : base(image)
        {
            drawingElements = eles;
            UseGraphicsDraw = true;
        }

        public DrawingElement[] DrawingElements
        {
            get => drawingElements;
            set
            {
                drawingElements = value;
                if (AutoUpdate) Update();
            }
        }

        public void Update()
            => Draw();
        protected override void Draw(Graphics graphics)
        {
            base.Draw(graphics);
            var size = Image.Size;
            foreach (var element in drawingElements)
            {
                PointF[] points = [..element.Points.Select(p => GetPoint(ref size, p))];
                switch (element.Action)
                {
                    case DrawingActionType.Normal:
                        
                        break;
                    case DrawingActionType.Line:
                        graphics.DrawLine(Pen, points[0], points[1]);
                        break;
                    case DrawingActionType.Curve:
                        graphics.DrawCurve(Pen, points);
                        break;
                    case DrawingActionType.Rectangle:
                        graphics.DrawRectangle(Pen, new RectangleF(Math.Min(points[0].X, points[1].X), Math.Min(points[0].Y, points[1].Y), Math.Abs(points[1].X - points[0].X), Math.Abs(points[1].Y - points[0].Y)));
                        break;
                    case DrawingActionType.Circle:
                        var vec = element.Points[0] - element.Points[1];
                        var p = new Vector(size.Width * Math.Abs(vec.X), size.Height * Math.Abs(vec.Y));
                        float radius = (float)p.Length;
                        graphics.DrawArc(Pen, points[0].X - radius, points[0].Y - radius, 2 * radius, 2 * radius, 0, 360);
                        break;
                }
            }
            static PointF GetPoint(ref Size size, PointD pointD)
                => new(size.Width * (float)pointD.X, size.Height * (float)pointD.Y);
        }
    }
}
