using ElShrine.Modules.Plugin;

namespace ElShrine.VisualTool.PluginUI;

public record DataTemplatedPluginInfo : PluginInfo
{
    public readonly string URI;
    public readonly string Key;
    public readonly string Icon;
    public readonly bool IsHeaderComponent;
    public readonly bool IsTabComponent;
    public DataTemplatedPluginInfo(PluginInfo info, string uri, string key, string icon, bool isHeaderComp, bool isTabComp) : base(info)
    {
        URI = uri;
        Key = key;
        Icon = icon;
        IsHeaderComponent = isHeaderComp;
        IsTabComponent = isTabComp;
    }
}
