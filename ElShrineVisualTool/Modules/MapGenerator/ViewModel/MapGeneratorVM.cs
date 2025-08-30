using ElShrine.EOption;
using ElShrine.Wpf.ViewModel;

namespace ElShrine.Modules.MapGenerator.ViewModel
{
    [ModuleRoot(Name = "Map Generator", Version = "1.0", Tags = ["Common", "Graph", "Map"], Description = "Intergrated map editor and renderer, generate height map with perlin 2d noise.", DataTemplateUri = "/ElShrienVisualTool;component/Modules/MapGenerator/MapGenerator.xaml", DataTemplateName = "MapGeneratorTemplate")]
    public class MapGeneratorVM : ViewModelBase, ISingleton<MapGeneratorVM>
    {
        private static MapGeneratorVM? Instance = null;
        public static MapGeneratorVM GetInstance() => Instance ??= new();
        private MapGeneratorVM() : base() { }
        
    }
}
