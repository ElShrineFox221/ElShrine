using System.Windows;
using System.Windows.Media.Imaging;

namespace ElShrine.Modules.MapEditor
{
    public static class MapEditorHelper
    {
        

        public static System.Drawing.Size GetImageSize(this WriteableBitmap image)
            => new((int)image.Width, (int)image.Height);
        public static Int32Rect GetImageFullRect(this WriteableBitmap image)
            => new(0, 0, (int)image.Width, (int)image.Height);
    }
}
