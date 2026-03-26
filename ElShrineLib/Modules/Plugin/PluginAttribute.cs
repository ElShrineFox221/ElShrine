using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PluginAttribute : ValidatableClassAttribute
{
    public string Name = string.Empty;
    public string Author = string.Empty;
    public string Version = string.Empty;
    public string Description = string.Empty;
    protected override bool ValidateType(Type typeToValidate)
    {
        var hasImplement = typeToValidate.IsImplementOf(typeof(IPlugin));
        if (!hasImplement)
            ValidateFailedReason = $"{typeToValidate.FullName} is not implement of {nameof(IPlugin)}.";
        return hasImplement;
    }

    protected virtual PluginInfo SummarizePluginInfo(Type pluginType, string folder, string filesHash)
        => SummarizePluginInfoDefalut(this, pluginType, folder, filesHash);
    internal PluginInfo DoSummarizePluginInfo(Type pluginType, string folder, string filesHash)
        => SummarizePluginInfo(pluginType, folder, filesHash);
    internal static PluginInfo SummarizePluginInfoDefalut(PluginAttribute? attr, Type pluginType, string folder, string filesHash)
    {
        return new PluginInfo(
                        Id: $"{folder}_{pluginType.Name}",
                        FromHost: AssemblyLoadContext.Default.Assemblies.Contains(pluginType.Assembly),
                        PluginFullName: pluginType.FullName!,
                        Folder: folder,
                        FileHash: filesHash,
                        Name: attr?.Name ?? pluginType.Name,
                        Author: attr?.Author ?? string.Empty,
                        VersionInfo: attr?.Version ?? string.Empty,
                        Description: attr?.Description ?? string.Empty
                        );
    }
}
