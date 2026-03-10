using ElShrine.Modules;

namespace ElShrine.Commands;

[CommandCarrier]
public static class TestCommands
{
    [Command]
    public static void RescanPlugins()
    {
        var pm = CoreModuleAccessor.Plugin;
        pm.RescanPluginInfos();
    }
}
