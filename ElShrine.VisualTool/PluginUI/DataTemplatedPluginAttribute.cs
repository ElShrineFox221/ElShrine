using ElShrine.Modules.Plugin;

namespace ElShrine.VisualTool.PluginUI;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DataTemplatedPluginAttribute(string uri, string key) : PluginAttribute
{
    public readonly string URI = uri;
    public readonly string Key = key;

    public string Icon = string.Empty;
    public bool IsHeaderComponent = false;
    public bool IsTabComponent = true;

    protected override bool ValidateType(Type typeToValidate)
    {
        var suc = base.ValidateType(typeToValidate);
        if (suc && !typeToValidate.IsImplementOf(typeof(DataTemplatedPluginBase)))
        {
            suc = false;
            ValidateFailedReason = $"{typeToValidate.FullName} is not implement of {nameof(DataTemplatedPluginBase)}.";
        }
        return suc;
    }
    protected override PluginInfo SummarizePluginInfo(Type pluginType, string folder, string filesHash)
    {
        var info = base.SummarizePluginInfo(pluginType, folder, filesHash);
        return new DataTemplatedPluginInfo(info, URI, Key, Icon.IsEmpty() ? "" : Icon, IsHeaderComponent, IsTabComponent);
    }
}