using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

public interface IPlugin
{
    void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx);
    void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx);
}

public delegate void PluginLoadedHandler(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedNewCtx);
public delegate void PluginUnloadingHandler(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx);
public delegate void PluginUnloadedHandler(PluginInfo info);

public interface IPluginManager
{
    void RescanPluginInfos();
    IReadOnlyList<PluginInfo> AvailablePlugins { get; }
    IReadOnlyDictionary<PluginInfo, IPlugin> LoadedPlugins { get; }
    IPlugin LoadPlugin(PluginInfo info);
    bool UnloadPlugin(PluginInfo info);
    event PluginLoadedHandler? PluginLoaded;
    event PluginUnloadingHandler? PluginUnloading;
    event PluginUnloadedHandler? PluginUnloaded;
}