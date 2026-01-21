using ElShrine.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ElShrine.Wpf.Controls.Extensions
{
    public sealed class Rule : Freezable
    {

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target), typeof(DependencyObject), typeof(Rule));
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(nameof(Context), typeof(DependencyObject), typeof(Rule));
        public static readonly DependencyProperty StateMapProperty = DependencyProperty.Register(nameof(StateMap), typeof(string), typeof(Rule));

        public DependencyObject Target
        {
            get => (DependencyObject)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
        public DependencyObject Context
        {
            get => (DependencyObject)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }
        public string StateMap
        {
            get => (string)GetValue(StateMapProperty);
            set => SetValue(StateMapProperty, value);
        }

        public Rule() { }

        protected override Freezable CreateInstanceCore() => new Rule();
    }
    public sealed class RuleCollection : FreezableCollection<Rule> { }
    public static class StateListener
    {
        #region DPs
        public static readonly DependencyProperty ListenerProperty = DependencyProperty.RegisterAttached(
            nameof(ListenerProperty).ToPropRegName(),
            typeof(DependencyObject),
            typeof(StateListener),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnListenerChanged
        ));
        public static readonly DependencyProperty RulesProperty = DependencyProperty.RegisterAttached(
            nameof(RulesProperty).ToPropRegName(),
            typeof(RuleCollection),
            typeof(StateListener),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnRulesChanged
        ));
        #endregion

        #region dp methods
        public static DependencyObject GetListener(DependencyObject d) => (DependencyObject)d.GetValue(ListenerProperty);
        public static void SetListener(DependencyObject d, DependencyObject value) => d.SetValue(ListenerProperty, value);
        public static DependencyObject GetRules(DependencyObject d) => (DependencyObject)d.GetValue(RulesProperty);
        public static void SetRules(DependencyObject d, DependencyObject value) => d.SetValue(RulesProperty, value);
        #endregion

        #region callbacks

        private static void OnListenerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckAndRegister(d);
        }
        private static void OnRulesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckAndRegister(d);
        }
        private static void CheckAndRegister(DependencyObject d)
        {
            if (d is FrameworkElement fe)
            {
                if (fe.IsLoaded)
                {
                    DoRegister(fe);
                }
                else
                {
                    fe.Loaded -= Fe_Loaded;
                    fe.Loaded += Fe_Loaded;
                }
            }
            else
            {
                DoRegister(d);
            }
        }
        private static void Fe_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                fe.Loaded -= Fe_Loaded;
                DoRegister(fe);
            }
        }
        private static readonly ConditionalWeakTable<DependencyObject, DependencyObject> listnerByRegister = [];
        private static void DoRegister(DependencyObject d)
        {
            //Set default target, listener and data source, if not set
            var listener = GetListener(d);
            if (listener is null)
            {
                SetListener(d, d);
                listener = d;
            }
            if (listnerByRegister.TryGetValue(d, out var _listener)) StateListenersManager.Instance.Unregister(_listener);
            else listnerByRegister.Add(d, listener);
            //Validate target and data source equality, warning if equals.
            var rulesDpo = GetRules(d);
            if(rulesDpo is RuleCollection rc)
            {
                var ruleSnapshots = new List<RuleSnapshot>();
                foreach(var rule in rc)
                {
                    var dataSource = rule.Context ?? d;
                    var settersTarget = rule.Target ?? d;
                    if(dataSource == rule) dataSource = rule.Context = d;
                    TransHelper.GetThemeControlParent(dataSource, out var _, out var tc);
                    if (settersTarget == tc)
                    {
                        TraceSource ts = PresentationTraceSources.DataBindingSource;
                        ts.TraceEvent(TraceEventType.Error, 0, $"[Listener Warning]: The target({settersTarget}) is same obj of dataSource, it may cause cycle invokes.");
                    }
                    ruleSnapshots.Add(new(new(settersTarget), new(dataSource), rule.StateMap));
                }
                StateListenersManager.Instance.Register(listener, ruleSnapshots);
            }
            //
        }
        #endregion
    }
    public sealed record RuleSnapshot(WeakReference<DependencyObject> SettersTarget, WeakReference<DependencyObject> DataSource, string StateMap);
}
