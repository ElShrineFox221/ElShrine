using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace ElShrine.VisualTool.MapEditor.Model
{
    public class PixelPainter(Bitmap image, Matrix<double> altitudeMatrix) : MapPainterBase(image, altitudeMatrix)
    {
        public Func<int, int, Matrix<double>, Color?[,]>? ColorMatrix { get; set; }
        public readonly static FillColor[] DefaultFillColors = [new(Color.White, 1), new(Color.Black, 0)];
        public List<FillColor> FillColors = [..DefaultFillColors];
        public readonly static Func<int, int, Matrix<double>, Color?[,]> GrayPaintStyle = (w, h, im) =>
        {
            Color?[,] colorMatrix = new Color?[h, w];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int ci = (byte)(im[y, x] * 255);
                    colorMatrix[y, x] = Color.FromArgb(255, ci, ci, ci);
                }
            }
            //index range: 0 to 255
            return colorMatrix;
        };
        public Func<int, int, Matrix<double>, Color?[,]> GradientPaintStyle => (w, h, im) =>
        {
            Color?[,] colorMatrix = new Color?[h, w];
            FillColors.Sort((a, b) => Math.Sign(a.Value - b.Value));
            for(int x = 0; x < w; x++)
            {
                for(int y = 0; y < h; y++)
                {
                    double d = im[y, x];
                    FillColor color0 = FillColors.FindLast(fc => fc.Value <= d) ?? FillColors[0];
                    FillColor color1 = FillColors.Find(fc => fc.Value >= d) ?? FillColors[^1];
                    double sub = color1.Value - color0.Value;
                    double rate = (d - color0.Value) / sub;
                    Color color = Color.FromArgb(255,
                        lerp(color0.Color.R, color1.Color.R, rate),
                        lerp(color0.Color.G, color1.Color.G, rate),
                        lerp(color0.Color.B, color1.Color.B, rate));
                    colorMatrix[y, x] = color;
                    static byte lerp(byte n1, byte n2, double rate)
                    {
                        double r;
                        if (!rate.IsFinite()) r = 0;
                        else r = double.Pow(rate, 5) * 6 - double.Pow(rate, 4) * 15 + double.Pow(rate, 3) * 10;
                        return (byte)(r * (n2 - n1) + n1);
                    }
                }
            }
            //index range: 0 to 255
            return colorMatrix;
        };

        private const int PixelBytes = 4;
        protected override void PixelDraw()
        {
            Int32Rect imageRect = new(0, 0, Image.Width, Image.Height), drawRect = imageRect;
            int size = drawRect.Width * drawRect.Height * PixelBytes;
            byte[] bytes = new byte[size];
            int drawRectBottomY = drawRect.Height + drawRect.Y, drawRectRightX = drawRect.Width + drawRect.X; 
            Color?[,] colorMatrix = GetColorMatrix(imageRect.Width, imageRect.Height, AltitudeMatrix);
            for (int y = drawRect.Y; y < drawRectBottomY; y++)
            {
                for (int x = drawRect.X; x < drawRectRightX; x++)
                {
                    int p = Math.Max(4 * (drawRect.Width * (y - drawRect.Y) + (x - drawRect.X)), 0);
                    Color color = colorMatrix[y, x] ?? Color.Transparent;
                    bytes[p] = color.B;
                    bytes[p + 1] = color.G;
                    bytes[p + 2] = color.R;
                    bytes[p + 3] = color.A;
                }
            }
            BitmapData srcBmpData = Image.LockBits(new(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            nint srcPtr = srcBmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, srcPtr, bytes.Length);
            Image.UnlockBits(srcBmpData);
        }
        private Color?[,] GetColorMatrix(int width, int height, Matrix<double> rateMatrix) => (ColorMatrix ?? GradientPaintStyle).Invoke(width, height, rateMatrix);
    }
}
