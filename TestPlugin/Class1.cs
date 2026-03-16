using ElShrine;
using ElShrine.Modules.Command;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Plugin;
using System.Runtime.Loader;

namespace TestPlugin;

public class PluginT01(ILogManager log) : IPlugin
{
    private readonly ILogManager _log = log;
    public void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        _log.Main.Log(LogItem.Bracket($"Executed {nameof(PostLoad)} with plugin {nameof(PluginT01)}.", LogItemStyle.SubInfo));
    }

    public void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        _log.Main.Log(LogItem.Bracket($"Executed {nameof(PreUnload)} with plugin {nameof(PluginT01)}.", LogItemStyle.SubInfo));
    }
}

public class PluginT02(ILogManager log) : IPlugin
{
    private readonly ILogManager _log = log;
    public void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        _log.Main.Log(LogItem.Bracket($"Executed {nameof(PostLoad)} with plugin {nameof(PluginT02)}.", LogItemStyle.SubInfo));
    }

    public void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        _log.Main.Log(LogItem.Bracket($"Executed {nameof(PreUnload)} with plugin {nameof(PluginT02)}.", LogItemStyle.SubInfo));
    }
}

public class TestOption : OptionBase
{
    [OptionItem]
    public string TestOptionStr
    {
        get => field; set => SetProperty(ref field, TestOptionStr);
    } = "DefaultOptionStr";
}

[CommandCarrier]
public class TestXCommands(ILogManager log)
{
    private readonly ILogManager _log = log;
    [Command]
    public void Print(string msg)
    {
        _log.Main.Log(msg);
    }
    [Command]
    public void Print()
    {
        _log.Main.Log("Hello world!");
    }
}