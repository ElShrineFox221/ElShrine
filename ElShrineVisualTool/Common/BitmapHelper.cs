using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Int32Point = System.Drawing.Point;
using Int32Size = System.Drawing.Size;

namespace ElShrine.Common
{
    public static class BitmapHelper
    {
        public static void AddDirty(this WriteableBitmap bitmap, Int32Rect rect)
        {
            bitmap.Lock();
            bitmap.AddDirtyRect(rect);
            bitmap.Unlock();
        }
        public readonly static Int32Size DefaultDpi = new(96, 96);
        public static WriteableBitmap NewWriteableBitMap(Int32Size size, Int32Size? dpi = null)
        {
            var rdpi = dpi is null ? DefaultDpi : dpi.Value;
            return new(size.Width, size.Height, rdpi.Width, rdpi.Height, PixelFormats.Bgra32, null);
        }
        public static Bitmap CreateBitmap(this WriteableBitmap writeableBitmap)
            => new(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, writeableBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, writeableBitmap.BackBuffer);


        public static void CopyToWriteableBitmap(this Bitmap source, WriteableBitmap target)
        {
            var data = source.LockBits(new(new Int32Point(0, 0), source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            target.WritePixels(new Int32Rect(0, 0, source.Size.Width, source.Size.Height), data.Scan0, data.Height * data.Stride, data.Stride);
            source.UnlockBits(data);
        }
    }
}
