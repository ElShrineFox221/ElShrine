using ElShrine.EGraphic;
using System.Drawing;

namespace ElShrine.EOption
{
    [Option(Name = "Draw")]
    [StartupClass]
    public class DrawingOption : ISingleton<DrawingOption>
    {
        public static DrawingOption? Instance { get; set; }
        public static DrawingOption GetInstance() => Instance ??= new();

        public bool SmoothingMode { get; set; } = true;
        public bool TransparentMode { get; set; } = true;

        public int ImageHeight { get; set; } = 1080;
        public int ImageWidth { get; set; } = 1920;

        public bool AutoSaveImage { get; set; } = true;
        public string DefaultBackColorCode { get; set; } = Color.White.ToHexARGB();
        public string DefaultForeColorCode { get; set; } = Color.Black.ToHexARGB();

        public string ImageFormat { get; set; } = "png";
    }
}
