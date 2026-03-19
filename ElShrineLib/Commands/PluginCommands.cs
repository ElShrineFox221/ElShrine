using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Plugin;

namespace ElShrine.Commands;

[CommandCarrier]
public static class PluginCommands
{
    private static ILogger Logger => field ??= CoreModuleAccessor.Log.Main;
    private static IPluginManager Plugin => field ??= CoreModuleAccessor.Plugin;
    private static IOptionManager Option => field ??= CoreModuleAccessor.Option;
    
    [Command]
    public static void Scan()
    {
        Plugin.ScanPluginInfos();
    }
    [Command]
    public static void View()
    {
        var ec = PluginInfo.BuildPluginTable(
            LogItem.Normal($"Infos of {"plugin".GetPuralWithNum(Plugin.AvailablePlugins.Count)} have been collected."),
            Plugin.AvailablePlugins.Select(i => (i, Plugin.LoadedPlugins.ContainsKey(i))), true);
        Logger.Log(ec);
    }


    [Command]
    public static void LoadAt(int index)
    {
        var plugins = Plugin.AvailablePlugins;
        if(index >= plugins.Count)
        {
            Logger.Error(new IndexOutOfRangeException());
            return;
        }
        Plugin.LoadPlugin(plugins[index]);
    }
    [Command]
    public static void Load(string name)
    {
        var plugins = Plugin.AvailablePlugins;
        foreach (var info in plugins)
        {
            if(info.Name == name)
            {
                Plugin.LoadPlugin(info);
                return;
            }
        }
        foreach (var info in plugins)
        {
            if (info.Name.EqualIgnoreCase(name))
            {
                Plugin.LoadPlugin(info);
                return;
            }
        }
        Logger.Error(new PluginException($"Found no plugin named {name}"));
    }
    [Command]
    public static void UnloadAt(int index)
    {
        var plugins = Plugin.AvailablePlugins;
        if(index >= plugins.Count)
        {
            Logger.Error(new IndexOutOfRangeException());
            return;
        }
        Plugin.UnloadPlugin(plugins[index]);
    }
    [Command]
    public static void Unload(string name)
    {
        var plugins = Plugin.AvailablePlugins;
        foreach (var info in plugins)
        {
            if(info.Name == name)
            {
                Plugin.UnloadPlugin(info);
                return;
            }
        }
        Logger.Error(new PluginException($"Found no plugin named {name}"));
    }

    [Command]
    public static void SaveConfig()
    {
        Plugin.Save();
    }
    [Command]
    public static void LoadConfig()
    {
        Plugin.Load();
    }
}
