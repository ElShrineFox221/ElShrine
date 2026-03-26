using ElShrine.Common.DataStructure;

namespace ElShrine.Modules.Option;

public abstract class OptionBase : DirtyTrackableObject
{
    public const string GlobalOptionName = "Global";
    public virtual string OptionCataName => GetType().Name;
}

