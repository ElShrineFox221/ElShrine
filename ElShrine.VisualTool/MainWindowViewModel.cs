using ElShrine.Wpf;
using System.Windows;

namespace ElShrine.VisualTool
{
    public sealed class MainWindowViewModel(Window ownerWindow) : EWindowViewModelBase(ownerWindow)
    {
        public WpfPageManagerVM ModuleManager { get; } = new WpfPageManagerVM();
    }
}
