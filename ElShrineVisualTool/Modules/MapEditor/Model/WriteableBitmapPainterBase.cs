using ElShrine.EGraphic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ElShrine.Modules.MapEditor.Model
{
    public abstract class BitmapPainterBase(Bitmap image) : GraphicsPainterBase
    {
        public readonly Bitmap Image = image;
        public bool UseGraphicsDraw = false;
        public bool ClearWhenGraphicsDraw = true;
        public SmoothingMode GraphicsSmoothingMode = SmoothingMode.HighQuality;
        public Color GraphicsClearColor = Color.Transparent;

        public void Draw()
        {
            PixelDraw();
            if (UseGraphicsDraw)
            {
                using Graphics graphics = Graphics.FromImage(Image);
                {
                    graphics.SmoothingMode = GraphicsSmoothingMode;
                    if (ClearWhenGraphicsDraw) graphics.Clear(GraphicsClearColor);
                    Draw(graphics);
                    graphics.Flush();
                };
            }
        }
        protected virtual void PixelDraw() { }
    }
}
