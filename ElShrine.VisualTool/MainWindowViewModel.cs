using ElShrine.VisualTool.PluginUI;
using ElShrine.Wpf;
using System.Windows;

namespace ElShrine.VisualTool;

public sealed class MainWindowViewModel(Window ownerWindow) : EWindowViewModelBase(ownerWindow)
{
    public PluginManagerUIVM PluginManagerUI { get; } = Bootstrapper.Resolve<PluginManagerUIVM>();
}
