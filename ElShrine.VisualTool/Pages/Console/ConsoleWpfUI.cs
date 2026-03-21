using ElShrine.Modules;
using ElShrine.VisualTool.Pages.Console.ViewModel;
using ElShrine.VisualTool.PluginUI;
using ElShrine.Wpf;
using System.Runtime.Loader;

namespace ElShrine.VisualTool.Pages.Console;

[DataTemplatedPlugin("/ElShrine.VisualTool;component/Pages/Console/Console.xaml", "ConsoleTemplate",
    Name = "Console",
    Version = "2.0",
    Author = "ElShrine",
    Description = "The advanced console, as implement of the IConsoleListener instead of System.Console.",
    Icon = "Console.png",
    IsHeaderComponent = true,
    IsTabComponent = false)]
public class ConsoleWpfUI() : DataTemplatedPluginBase()
{
    protected override ViewModelBase GetViewModel()
    {
        var vm = ConsoleVM.Instance;
        return vm;
    }
}
