using System.Reflection;

namespace ElShrine.Modules.Command;

#region Attributes
[AttributeUsage(AttributeTargets.Class)]
public class CommandCarrierAttribute : ValidatableClassAttribute
{
    public string? OverrideName = string.Empty;

    protected override bool ValidateType(Type typeToValidate)
    {
        if (!typeToValidate.IsAbstract || typeToValidate.IsSealed)
            return true;
        ValidateFailedReason = $"Type <{typeToValidate.FullName}> should be a static or non-abstract class.";
        return false;
    }
}
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : ValidatableMemberAttribute
{
    public string? OverrideName { get; init; }
    public string Description { get; init; } = string.Empty;

    protected override bool Validate(MemberInfo extraInstance)
        => true;
}
[AttributeUsage(AttributeTargets.Method)]
public class IgnoreCommandAttribute : Attribute;
#endregion
