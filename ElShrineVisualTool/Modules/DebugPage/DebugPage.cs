using ElShrine.EOption;
using ElShrine.Wpf.Controls;
using ElShrine.Wpf.ViewModel;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.Modules.DebugPage
{
    [ModuleRoot(Name = "Debug Page", Version = "0.3", Tags = ["Debug", "Test"], Description = "UI for controls, created a visual page for all testing control to do debug actions.", DataTemplateUri = "/ElShrienVisualTool;component/Modules/DebugPage/DebugPage.xaml", DataTemplateName = "DebugPageTemplate")]
    public class DebugPageVM : ViewModelBase, ISingleton<DebugPageVM>
    {
        private static DebugPageVM? Instance = null;
        public static DebugPageVM GetInstance() => Instance ??= new();
        private DebugPageVM() : base() { }

        public List<string> Suggestions { get; set; } =
        [
            "alpha", "arch", "activity",
            "beta", "gamma", "delta",
            "epsilon", "zeta", "theta",
            "lambda", "sigma", "omega",
            "architecture", "archive", "archer",
            "architect", "archway", "archaic",
            "active", "action", "actor",
            "activate", "actual", "acoustic",
            "algorithm", "algebra", "alphabet",
            "analysis", "analog", "android",
            "application", "apparatus", "approach",
            "database", "digital", "dynamic",
            "element", "energy", "engine",
            "function", "framework", "future"
        ];

        public VMC OpenDialog => new(parameter =>
        {
            if(parameter is EInWindowDialog iwd)
            {
                iwd.IsOpen = !iwd.IsOpen;
            }
        });
    }
}
