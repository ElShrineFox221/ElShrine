using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

public interface IPlugin
{
    void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx);
    void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx);
}

public delegate void PluginLoadedHandler(IPlugin plugin, PluginInfo info);
public delegate void PluginPreUnloadHandler(IPlugin plugin, PluginInfo info);
public delegate void PluginUnloadedHandler(PluginInfo info);

public interface IPluginManager
{
    void RescanPluginInfos();
    IReadOnlyList<PluginInfo> UnloadPlugins { get; }
    IReadOnlyDictionary<PluginInfo, IPlugin> LoadedPlugins { get; }
    IPlugin LoadPlugin(PluginInfo info);
    bool UnloadPlugin(PluginInfo info);
    event PluginLoadedHandler? PluginLoaded;
    event PluginPreUnloadHandler? PluginPreUnload;
    event PluginUnloadedHandler? PluginUnloaded;
}