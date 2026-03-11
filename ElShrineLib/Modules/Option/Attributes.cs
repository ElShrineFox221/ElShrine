using System.Reflection;

namespace ElShrine.Modules.Option;

#region Attributes
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionItemAttribute : ValidatableMemberAttribute
{
    public string OverrideName = string.Empty;
    public string Description = string.Empty;
    protected override bool Validate(MemberInfo target)
    {
        var suc = target is PropertyInfo p && (!p.CanRead || !p.CanWrite) || target is FieldInfo f && f.IsLiteral;
        if (!suc)
            ValidateFailedReason = $"Member <{target.Name}> is not a readable and writable property or field.";
        return suc;
    }
}
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreOptionItemAttribute : Attribute;
#endregion