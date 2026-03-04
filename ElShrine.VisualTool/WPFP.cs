using ElShrine.Wpf;
using System.Runtime.Serialization;

namespace ElShrine.VisualTool;

[DataContract]
public class WpfPageInfo
{
    [DataMember] public string Name { get; set; } = "New Module";
    [DataMember] public string Version { get; set; } = "1.0";
    public string[] Tags = [];
    public string Description = "Empty module with no description.";
    public string TemplateUri = string.Empty;
    public string TemplateName = string.Empty;
    [DataMember] public int Index { get; set; } = -1;
    [DataMember] public bool Enabled { get; set; } = false;

    public Type RootViewModelClass = typeof(object);
    public WpfPageInfo Clone()
    {
        var module = this;
        var copyTags = new string[module.Tags.Length];
        module.Tags.CopyTo(copyTags, 0);
        var moduleCopy = new WpfPageInfo()
        {
            Name = module.Name,
            Version = module.Version,
            Tags = copyTags,
            Description = module.Description,
            TemplateName = module.TemplateName,
            TemplateUri = module.TemplateUri,
            Index = module.Index,
            Enabled = module.Enabled,
        };
        return moduleCopy;
    }
    public bool MemberValueEqual(WpfPageInfo other)
        => Name == other.Name
        && Version == other.Version
        && Tags.SequenceEqual(other.Tags)
        && Description == other.Description
        && TemplateUri == other.TemplateUri
        && TemplateName == other.TemplateName
        && Index == other.Index
        && Enabled == other.Enabled;
}

[AttributeUsage(AttributeTargets.Class)]
public class WpfPageRootVMAttribute() : ValidatableBaseAttribute
{
    public string? Name;
    public string? Version;
    public string[] Tags = [];
    public string? Description = null;
    public string? DataTemplateUri;
    public string? DataTemplateName;
    public bool DefaultEnabled = false;
    protected override bool Validate(Type attributedTargetType, object? extraInstance)
        => attributedTargetType.IsImplementOf(typeof(ViewModelBase));
    public WpfPageInfo ToModuleInfo(Type implement)
    {
        WpfPageInfo moduleInfo = new()
        {
            Name = Name ?? implement.Name,
            Tags = Tags,
            RootViewModelClass = implement,
            Enabled = DefaultEnabled,
        };
        if (Version is not null) moduleInfo.Version = Version;
        if (Description is not null) moduleInfo.Description = Description;
        if (DataTemplateUri is not null) moduleInfo.TemplateUri = DataTemplateUri;
        if (DataTemplateName is not null) moduleInfo.TemplateName = DataTemplateName;
        return moduleInfo;
    }
}