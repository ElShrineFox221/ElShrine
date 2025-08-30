using ElShrine.EGraphic;
using System.Drawing;

namespace ElShrine.ColorTransfer.Model
{
    public class ColorGreyTransfer(Bitmap bitmap)
    {
        private readonly Bitmap _bitmap = bitmap;
        public void Transfer(int tolerence)
        {
            int width = _bitmap.Width, height = _bitmap.Height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _bitmap.SetPixel(x, y, Rerender(_bitmap.GetPixel(x, y), tolerence));
                }
            }
        }
        public void Save(string path)
        {
            _bitmap.Save(path);
        }
        private static Color Rerender(Color color, int tolerence)
        {
            Color newColor = color;
            var v = color.R + color.B + color.G;
            if (!(v <= tolerence || v >= 765 - tolerence)) 
            {
                var (hv, _, _) = ColorHelper.RGBToHSV(color.R, color.G, color.B);
                byte h = hv ?? 0;
                newColor = Color.FromArgb(color.A, h, h, h);
            }
            return newColor;
        }
    }
}
