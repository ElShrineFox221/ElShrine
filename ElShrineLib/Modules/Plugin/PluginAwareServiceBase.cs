using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

public abstract class PluginAwareServiceBase
{
    protected readonly IPluginManager _plugin;

    public PluginAwareServiceBase(IPluginManager plugin)
    {
        _plugin = plugin;
        _plugin.PluginLoaded += OnPluginLoaded;
        _plugin.PluginUnloading += OnPluginUnloading;
        _plugin.PluginUnloaded += OnPluginUnloaded;
    }

    protected abstract void OnPluginLoaded(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedNewCtx);
    protected abstract void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx);
    protected virtual void OnPluginUnloaded(PluginInfo info) { }
}
