using ElShrine.VisualTool.MapEditor.Common;
using ElShrine.Wpf.ViewModel;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ElShrine.VisualTool.MapEditor
{
    public class ResenderProcessVM(Dispatcher dispatcher) : ViewModelBase
    {
        public double Value { get; set; } = 0;
        public Dispatcher Dispatcher { get; set; } = dispatcher;

        public static async Task<Bitmap> Begin(WriteableBitmap source, Action<Bitmap> action)
        {
            var size = source.GetImageSize();
            Bitmap bitmap = source.CreateBitmap();
            return await Task.Run(() =>
            {
                action.Invoke(bitmap);
                return bitmap;
            });
        }
    }
}
