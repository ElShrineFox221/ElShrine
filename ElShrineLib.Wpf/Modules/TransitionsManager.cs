using ElShrine.Wpf.Controls.Transitions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ElShrine.Modules;

public sealed class TransitionsManager
{
    public TransitionsManager()
    {
        Transitions.Add(typeof(Brush), new ColorTransition());
        Transitions.Add(typeof(double), new DoubleTransition());
    }

    private readonly Dictionary<Type, ITransition> Transitions = [];
    public bool TryDoTransition(DependencyObject tar, DependencyProperty tarDpProp, object? tarValue, bool isIn)
    {
        if(!Transitions.TryGetValue(tarDpProp.PropertyType, out var transition)) return false;
        if (!transition.TryDoTransition(tar, tarDpProp, tarValue, isIn)) return false;
        return true;
    }
}
