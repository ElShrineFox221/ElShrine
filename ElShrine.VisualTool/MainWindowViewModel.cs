using ElShrine.Wpf.ViewModel;
using System.Windows;

namespace ElShrine.VisualTool
{
    public sealed class MainWindowViewModel : EWindowViewModelBase
    {
        public MainWindowViewModel(Window ownerWindow) : base(ownerWindow)
        {
            VisualTool.ModuleManager.Refresh();
            VisualTool.ModuleManager.Confrim();
        }

        public ModuleManagerVM ModuleManager { get; } = new ModuleManagerVM();
    }
}
