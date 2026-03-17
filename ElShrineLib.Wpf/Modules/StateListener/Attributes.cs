using System;
using System.Collections.Generic;
using System.Windows;

namespace ElShrine.Modules.StateListener;

#region Attributes
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public abstract class StateGroupRuleAttribute() : ValidatableClassAttribute
{
    public abstract Type RelativeType { get; }
    public abstract IReadOnlyList<string> RelativeRoutedEventNames { get; }
}
public sealed class StateGroupRuleAttribute<TUIElement>(params string[] relativeRoutedEventNames) : StateGroupRuleAttribute
    where TUIElement : UIElement
{
    public override Type RelativeType => typeof(TUIElement);
    public override IReadOnlyList<string> RelativeRoutedEventNames => relativeRoutedEventNames;

    private static readonly HashSet<string> registeredStateNames = [];
    protected override bool ValidateType(Type typeToValidate)
    {
        var names = Enum.GetNames(typeToValidate);
        var suc = true;
        var failedName = string.Empty;
        foreach (var name in names)
        {
            suc = registeredStateNames.Add(name);
            if (!suc)
            {
                failedName = name;
                break;
            }
        }
        if (!suc) ValidateFailedReason = $"[StateGroup Error]: The state name({typeToValidate.FullName}.{failedName}) already exists.";
        return suc;
    }
}
#endregion