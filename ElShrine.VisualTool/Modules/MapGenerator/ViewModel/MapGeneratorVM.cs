using ElShrine.EOption;
using ElShrine.Wpf;

namespace ElShrine.VisualTool.Modules.MapGenerator.ViewModel
{
    [WpfPageRootVM(Name = "Map Generator", Version = "1.0", Tags = ["Common", "Graph", "Map"], Description = "Intergrated map editor and renderer, generate height map with perlin 2d noise.", DataTemplateUri = "/ElShrine.VisualTool;component/Modules/MapGenerator/MapGenerator.xaml", DataTemplateName = "MapGeneratorTemplate")]
    public class MapGeneratorVM : ViewModelBase, ISingleton<MapGeneratorVM>
    {
        private static MapGeneratorVM? Instance = null;
        public static MapGeneratorVM GetInstance() => Instance ??= new();
        private MapGeneratorVM() : base() { }
        
    }
}
