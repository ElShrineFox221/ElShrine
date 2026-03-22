using ElShrine.Modules;
using ElShrine.VisualTool.PluginUI;
using System.Windows;

namespace ElShrine.VisualTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Bootstrapper.Initialize(builder =>
        {
            builder.RegisterModule<IPluginManagerUI, PluginManagerUI>();
            WpfModuleAccessor.RegisterWpfModules(builder);
            CoreModuleAccessor.RegisterCoreModules(builder);
        });
        base.OnStartup(e);
    }
    protected override void OnActivated(EventArgs e)
    {
        
        base.OnActivated(e);
    }
}
