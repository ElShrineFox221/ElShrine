namespace ElShrine.Modules.Command;

#region Attributes
public class CommandCarrierAttribute : SingletonAttribute
{
    public string? OverrideName = string.Empty;
}
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : SingletonItemAttribute<CommandCarrierAttribute>
{
    public string? OverrideName { get; init; }
    public string Description { get; init; } = string.Empty;
}
[AttributeUsage(AttributeTargets.Method)]
public class IgnoreCommandAttribute : Attribute;
#endregion
