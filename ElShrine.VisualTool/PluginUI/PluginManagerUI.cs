using ElShrine.Modules.Log;
using ElShrine.Modules.Plugin;
using System.Collections.Concurrent;
using System.Runtime.Loader;
using System.Windows.Controls;

namespace ElShrine.VisualTool.PluginUI;

public class PluginManagerUI(IPluginManager plugin, ILogManager log) : PluginAwareServiceBase(plugin), IPluginManagerUI
{
    private readonly ILogManager _log = log;
    private readonly ILogger _logger = log.Main;
    private readonly ConcurrentDictionary<DataTemplatedPluginInfo, ContentControl> _controls = [];
    private readonly List<PluginInfo> _enableds = [];
    private readonly List<PluginInfo> _disableds = [];

    #region overrides
    protected override void OnPluginLoaded(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedNewCtx)
    {
        if (plugin is not DataTemplatedPluginBase dataTemplatedPlugin || info is not DataTemplatedPluginInfo dataTemplatedInfo)
            return;
        if (_controls.TryGetValue(dataTemplatedInfo, out _))
            return;
        var control = dataTemplatedPlugin.GetUIContent();
        if (control is null)
            _logger.Warning(new PluginException($"Failed build ui element from plugin {info.Name}."));
        else
            _controls.TryAdd(dataTemplatedInfo, control);
    }
    protected override void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx)
    {
        if(info is DataTemplatedPluginInfo di)
            _controls.TryRemove(di, out _);
    }
    #endregion

    #region implements
    public IReadOnlyList<PluginInfo> Enableds => _enableds;
    public IReadOnlyList<PluginInfo> Disableds => _disableds;
    public IReadOnlyDictionary<DataTemplatedPluginInfo, ContentControl> EnabledTabComps => _controls.Where(kv => kv.Key.IsTabComponent).ToDictionary();
    public IReadOnlyDictionary<DataTemplatedPluginInfo, ContentControl> EnabledTitleComps => _controls.Where(kv => kv.Key.IsHeaderComponent).ToDictionary();
    public event DataTemplatedPluginUpdatedHandler? PluginUpdated;
    public void RefreshList(bool rescanPlugins)
    {
        if (rescanPlugins) 
            _plugin.ScanPluginInfos();
        var enabledsUpdated = !_enableds.SequenceEqual(_plugin.LoadedPlugins.Keys);
        if (enabledsUpdated) 
        {
            _enableds.ReplaceAll(_plugin.LoadedPlugins.Keys);
        }
        var disabledsUpdated = !_disableds.SequenceEqual(_plugin.AvailablePlugins.Except(_enableds));
        if (disabledsUpdated)
        {
            _disableds.ReplaceAll(_plugin.AvailablePlugins.Except(_enableds));
        }
        //
        if (enabledsUpdated || disabledsUpdated)
            PluginUpdated?.Invoke();
    }
    #endregion
}