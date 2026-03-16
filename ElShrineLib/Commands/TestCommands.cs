using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Plugin;
using ElShrine.Options;

namespace ElShrine.Commands;

[CommandCarrier]
public static class TestCommands
{
    private static ILogger Logger => field ??= CoreModuleAccessor.Log.Main;
    private static IPluginManager Plugin => field ??= CoreModuleAccessor.Plugin;
    private static IOptionManager Option => field ??= CoreModuleAccessor.Option;
    
    [Command]
    public static void RescanPlugins()
    {
        Plugin.RescanPluginInfos();
    }
    [Command]
    public static void LoadPlugin(int index)
    {
        var plugins = Plugin.UnloadPlugins;
        if(index >= plugins.Count)
        {
            Logger.Error(new IndexOutOfRangeException());
            return;
        }
        Plugin.LoadPlugin(plugins[index]);
    }
    [Command]
    public static void UnloadPlugin(int index)
    {
        var plugins = Plugin.LoadedPlugins.Keys.ToList();
        if(index >= plugins.Count)
        {
            Logger.Error(new IndexOutOfRangeException());
            return;
        }
        Plugin.UnloadPlugin(plugins[index]);
    }
    [Command]
    public static void ChangeOpt()
    {
        Option.GetOption<CommonOption>().DirectlyDel = !Option.GetOption<CommonOption>().DirectlyDel;
    }
}
