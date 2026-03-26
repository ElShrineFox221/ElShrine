using ElShrine.Modules.StateListener;
using ElShrine.Modules.UITheme;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Collections.Generic;
using System.Windows;

namespace ElShrine.Modules;

internal class TestSLM : IStateListenerManager
{
    public StateGroup GetStateGroup(string stateName)
    {
        return StateGroup.FromEnum(typeof(MouseStateGroup));
    }

    public void RedoSetterTransitions(DependencyObject listener)
    {
        return;
    }

    public void Register(DependencyObject listener, IEnumerable<RuleSnapshot> listenRules)
    {
        return;
    }

    public void RevaluateStatus(DependencyObject listener)
    {
        return;
    }

    public void Unregister(DependencyObject listener)
    {
        return;
    }
}

internal class TestUITM : IUIThemeManager
{
    public Theme CurrentTheme => Theme.Default;

    public bool RegisterCoerceThemeDPs<T>(T control) where T : DependencyObject, IThemeControlBase
    {
        return false;
    }
}