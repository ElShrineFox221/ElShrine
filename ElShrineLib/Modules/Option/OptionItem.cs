using ElShrine.Common.DataStructure;
using System.Reflection;

namespace ElShrine.Modules.Option;

public sealed class OptionItem : ICataItem
{
    public required string ActualCataName { get; init; }
    public string VirtualCataName { get; init; } = string.Empty;
    public required string ActualItemName { get; init; }
    public string VirtualItemName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public required object? DefaultValue { get; init; }
    public required Type ValueType { get; init; }
    public required MemberInfo MemberInfo { get; init; }
    public object? OwnerInstance { get; init; }

    public bool HasChanged => !Equals(GetValue(), DefaultValue);
    public object? GetValue() => MemberInfo.GetMemberValue(OwnerInstance);
    internal bool SetValue(object? value)
    {
        if (Equals(value, GetValue())) return false;
        MemberInfo.SetMemberValue(OwnerInstance, value);
        return true;
    }
}
