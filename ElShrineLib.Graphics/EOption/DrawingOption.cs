using ElShrine.Graphics;
using ElShrine.Modules;
using System.Drawing;

namespace ElShrine.EOption
{
    [Option]
    public static class DrawingOption
    {

        public static bool SmoothingMode { get; set; } = true;
        public static bool TransparentMode { get; set; } = true;

        public static int ImageHeight { get; set; } = 1080;
        public static int ImageWidth { get; set; } = 1920;

        public static bool AutoSaveImage { get; set; } = true;
        public static string DefaultBackColorCode { get; set; } = Color.White.ToHexARGB();
        public static string DefaultForeColorCode { get; set; } = Color.Black.ToHexARGB();

        public static string ImageFormat { get; set; } = "png";
    }
}
