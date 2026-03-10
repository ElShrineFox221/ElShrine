using System.Reflection;

namespace ElShrine.Modules.Option;

#region Attributes
public class OptionAttribute : SingletonAttribute
{
    public string? OverrideName = string.Empty;
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class OptionItemAttribute : SingletonItemAttribute<OptionAttribute>
{
    public string OverrideName = string.Empty;
    public string Description = string.Empty;
    protected override bool Validate(MemberInfo target)
    {
        var suc = base.Validate(target);
        if (suc && (target is PropertyInfo p && (!p.CanRead || !p.CanWrite) || target is FieldInfo f && f.IsLiteral))
            ValidateFailedReason = $"Member <{target.Name}> is not a readable and writable property or field.";
        return suc;
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class IgnoreOptionItemAttribute : Attribute;
#endregion