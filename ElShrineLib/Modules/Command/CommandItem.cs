using ElShrine.Common.DataStructure;
using System.Reflection;

namespace ElShrine.Modules.Command;

public sealed class CommandItem : ICataItem
{
    public required string ActualCataName { get; init; }
    public string VirtualCataName { get; init; } = string.Empty;
    public required string ActualItemName { get; init; }
    public string VirtualItemName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public required MethodInfo MethodInfo { get; init; }
    public object? OwnerInstance { get; init; }
}