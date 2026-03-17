using System;
using System.Collections.Generic;
using System.Windows;

namespace ElShrine.Modules.StateListener;

public sealed class StateGroup
{
    public readonly Type SourceType;
    private readonly List<string> stateNames;
    internal readonly Dictionary<Type, IReadOnlyList<RoutedEvent>> RelativeRoutedEventsInternal = [];
    public IReadOnlyList<string> StateNames => stateNames;
    public IReadOnlyDictionary<Type, IReadOnlyList<RoutedEvent>> RelativeRoutedEvents => RelativeRoutedEventsInternal;

    private StateGroup(Type enumType)
    {
        SourceType = enumType;
        stateNames = [.. Enum.GetNames(enumType)];
        StateCalculator = ele => Enum.GetNames(enumType)[0];
    }
    internal static StateGroup FromEnum(Type enumType)
    {
        if (!enumType.IsEnum) throw new ArgumentException("[StateGroup Error]: The type must be an enum.");
        var sg = new StateGroup(enumType);
        return sg;
    }
    public bool IsGroupOf(string stateName) => stateNames.Contains(stateName);
    public bool IsGroupOf<TStateGroupSource>() => typeof(TStateGroupSource) == SourceType;

    #region Do register
    public Func<UIElement, string> StateCalculator { get; set; }
    #endregion
}