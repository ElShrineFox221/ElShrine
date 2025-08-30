using ElShrine.Common;
using ElShrine;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using System.Windows;
using Color = System.Drawing.Color;

namespace ElShrine.Modules.MapEditor.Model
{
    public class ContourPainter : MapPainterBase
    {
        public ContourPainter(Bitmap image, Matrix<double> altitudeMatrix, int contours = 10) : base(image, altitudeMatrix)
        {
            Contours = contours; UseGraphicsDraw = true;
        }
        public (double alti, bool[] b)[,]? ElevationMatrix { get; private set; }
        private int contours = 10;
        public int Contours
        {
            get => contours;
            set
            {
                contours = value;
                RegenerateContourArray();
            }
        }
        private double[] ContourArray = [];
        private int RegenerateContourArray()
        {
            AltitudeMatrix.GetLimit(out double max, out double min);
            double range = (max - min) / Contours;

            var contourArray = new double[Contours];
            for (int i = 0; i < Contours; i++) contourArray[i] = i * range + min;
            ContourArray = contourArray;
            return Contours;
        }
        public int Scale { get; set; } = 1;

        private int Emw => (int)Math.Ceiling(ImageWidth / (double)Scale);
        private int Emh => (int)Math.Ceiling(ImageHeight / (double)Scale);
        private int ImageWidth => Image.Width;
        private int ImageHeight => Image.Height;

        public Color LineColor { get; set; } = Color.Black;
        public float LineWidth { get; set; } = 2f;
        private void GetMatrix()
        {
            int emh = Emh, emw = Emw;
            ElevationMatrix = new (double alti, bool[] b)[emh, emw];
            int iwro = ImageWidth - 1;
            int ihro = ImageHeight - 1;
            for(int y = 0; y < emh; y++)
            {
                for(int x = 0; x < emw; x++)
                {
                    int amx = x > 0 ? x * Scale - 1 : 0;
                    int amy = y > 0 ? y * Scale - 1 : 0;
                    double d = AltitudeMatrix[Math.Min(amy, ihro), Math.Min(amx, iwro)];
                    ElevationMatrix[y, x].alti = d;
                    ElevationMatrix[y, x].b = [.. ContourArray.Select(item => d >= item)];
                }
            }
        }
        private record class Line(Vector P0, Vector P1);
        private Line[] GetLineElements()
        {
            List<Line> lineElements = [];
            double halfWidth = ImageWidth / 2d;
            double halfHeight = ImageHeight / 2d;
            if (ElevationMatrix is not null) CommonHelper.Efor((y, x) =>
            {
                Vector c2d1 = new((x + 0.5d) * Scale, y * Scale);
                Vector c2d2 = new((x + 1.0d) * Scale, (y + 0.5d) * Scale);
                Vector c2d3 = new((x + 0.5d) * Scale, (y + 1.0d) * Scale);
                Vector c2d4 = new(x * Scale, (y + 0.5d) * Scale);
                for (int l = 0; l < contours; l++)
                {
                    //b1 b2 b3 b4
                    //     c2d1
                    //c2d4 c2d0 c2d2
                    //     c2d3
                    (bool b1, bool b2, bool b3, bool b4) = (ElevationMatrix[y, x].b[l], ElevationMatrix[y, x + 1].b[l], ElevationMatrix[y + 1, x + 1].b[l], ElevationMatrix[y + 1, x].b[l]);
                    int value = (b1 ? 8 : 0) + (b2 ? 4 : 0) + (b3 ? 2 : 0) + (b4 ? 1 : 0);
                    switch (value)
                    {
                        default:
                        case 0:
                        case 15:
                            break;
                        case 1:
                        case 14:
                            lineElements.Add(new(c2d4, c2d3));
                            break;
                        case 2:
                        case 13:
                            lineElements.Add(new(c2d3, c2d2));
                            break;
                        case 3:
                        case 12:
                            lineElements.Add(new(c2d4, c2d2));
                            break;
                        case 4:
                        case 11:
                            lineElements.Add(new(c2d2, c2d1));
                            break;
                        case 5:
                            lineElements.Add(new(c2d4, c2d1));
                            lineElements.Add(new(c2d3, c2d2));
                            break;
                        case 10:
                            lineElements.Add(new(c2d4, c2d3));
                            lineElements.Add(new(c2d2, c2d1));
                            break;
                        case 6:
                        case 9:
                            lineElements.Add(new(c2d3, c2d1));
                            break;
                        case 7:
                        case 8:
                            lineElements.Add(new(c2d4, c2d1));
                            break;
                    }
                }
            }, Emh - 1, Emw - 1);
            return [.. lineElements];
        }
        protected override void Draw(Graphics graphics)
        {
            Pen pen = new(LineColor, LineWidth);
            GetMatrix();
            Line[] drawingElements = GetLineElements();
            foreach (Line line in drawingElements)
            {
                graphics.DrawLine(pen, (float)line.P0.X, (float)line.P0.Y, (float)line.P1.X, (float)line.P1.Y);
            }
        }
    }
}
