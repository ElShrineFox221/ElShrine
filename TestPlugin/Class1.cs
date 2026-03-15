using ElShrine.Modules.Option;
using ElShrine.Modules.Plugin;
using System.Runtime.Loader;

namespace TestPlugin;

public class PluginT01 : IPlugin
{
    public void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        Console.WriteLine($"Executed {nameof(PostLoad)} with plugin {nameof(PluginT01)}.");
    }

    public void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        Console.WriteLine($"Executed {nameof(PreUnload)} with plugin {nameof(PluginT01)}.");
    }
}

public class PluginT02 : IPlugin
{
    public void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        Console.WriteLine($"Executed {nameof(PostLoad)} with plugin {nameof(PluginT02)}.");
    }

    public void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        Console.WriteLine($"Executed {nameof(PreUnload)} with plugin {nameof(PluginT02)}.");
    }
}

public class TestOption : OptionBase
{
    [OptionItem]
    public string TestOptionStr
    {
        get => field; set => SetProperty(ref field, TestOptionStr, "TestOptionStr");
    }
}