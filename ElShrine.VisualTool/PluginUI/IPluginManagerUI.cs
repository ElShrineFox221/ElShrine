using ElShrine.Modules.Plugin;
using System.Windows.Controls;

namespace ElShrine.VisualTool.PluginUI;

public delegate void DataTemplatedPluginUpdatedHandler();

public interface IPluginManagerUI
{
    /* public IReadOnlyList<PluginInfo> Availables { get; }
     public IReadOnlyDictionary<PluginInfo, IPlugin> Loadeds { get; }
     public void RefreshListLocal();
     public void DoListLoad(IEnumerable<PluginInfo> infos);*/
    IReadOnlyList<PluginInfo> Enableds { get; }
    IReadOnlyDictionary<DataTemplatedPluginInfo, ContentControl> EnabledTabComps { get; }
    IReadOnlyDictionary<DataTemplatedPluginInfo, ContentControl> EnabledTitleComps { get; }
    IReadOnlyList<PluginInfo> Disableds { get; }

    event DataTemplatedPluginUpdatedHandler? PluginUpdated;

    void RefreshList(bool rescanPlugins);
}
