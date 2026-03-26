using ElShrine.Wpf.Controls.Extensions;
using System.Collections.Generic;
using System.Windows;

namespace ElShrine.Modules.StateListener;

public interface IStateListenerManager
{
    StateGroup GetStateGroup(string stateName);
    StateGroup GetStateGroup<TStateGroupSource>(TStateGroupSource state) => GetStateGroup(state?.ToString()!);

    void Register(DependencyObject listener, IEnumerable<RuleSnapshot> listenRules);
    void Unregister(DependencyObject listener);
    void RedoSetterTransitions(DependencyObject listener);
    void RevaluateStatus(DependencyObject listener);
}
