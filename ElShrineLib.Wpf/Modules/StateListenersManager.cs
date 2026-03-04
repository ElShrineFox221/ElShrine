using ElShrine.Common;
using ElShrine.Common.DataStructure;
using ElShrine.Common.Interpreter;
using ElShrine.Wpf.Common;
using ElShrine.Wpf.Controls;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.Converters;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElShrine.Modules
{
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

    #region StateGroupSources
    [StateGroupRule<UIElement>(
        nameof(UIElement.MouseDownEvent), nameof(UIElement.MouseUpEvent), 
        nameof(UIElement.PreviewMouseDownEvent), nameof(UIElement.PreviewMouseUpEvent), 
        nameof(UIElement.GotMouseCaptureEvent), nameof(UIElement.LostMouseCaptureEvent),
        nameof(UIElement.MouseEnterEvent), nameof(UIElement.MouseLeaveEvent))]
    public enum MouseStateGroup
    {
        MouseIn,
        MouseOut,
        MouseDown,
        MouseMove,
        Dragging
    }
    [StateGroupRule<ListBoxItem>(nameof(ListBoxItem.SelectedEvent), nameof(ListBoxItem.UnselectedEvent))]
    [StateGroupRule<ToggleButton>(nameof(ToggleButton.CheckedEvent), nameof(ToggleButton.UncheckedEvent))]
    public enum SelectionStateGroup
    {
        Selected,
        Unselected
    }
    [StateGroupRule<UIElement>(nameof(UIElement.GotFocusEvent), nameof(UIElement.LostFocusEvent))]
    public enum FocusStateGroup
    {
        Focused,
        UnFocused,
    }
    #endregion

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
    public sealed class StateParseException(string msg) : Exception(msg);
    //
    internal static class CommonStateCalculator
    {
        public static string CalculateMouseState(UIElement ele)
        {
            var @enum = MouseStateGroup.MouseOut;
            if (ele.IsMouseCaptured || (Mouse.LeftButton == MouseButtonState.Pressed && ele.IsMouseOver)) 
                @enum = MouseStateGroup.MouseDown;
            else if (ele.IsMouseOver) @enum = MouseStateGroup.MouseIn;
            return @enum.ToString();
        }
        public static string CalculateSelectionState(UIElement ele)
        {
            var selected = false;
            if (ele is ListBoxItem lbi) selected = lbi.IsSelected;
            else if (ele is TabItem ti) selected = ti.IsSelected;
            else if (ele is ToggleButton tb) selected = tb.IsChecked ?? false;
            var @enum = selected ? SelectionStateGroup.Selected : SelectionStateGroup.Unselected;
            return @enum.ToString();
        }
        public static string CalculateFocusState(UIElement ele)
        {
            var @enum = ele.IsFocused ? FocusStateGroup.Focused : FocusStateGroup.UnFocused;
            return @enum.ToString();
        }
    }
    [InitializationInfo(PreInstantiate = true)]
    public sealed class StateListenersManager : IInitializable<StateListenersManager>
    {
        #region Singleton
        private readonly static Lazy<StateListenersManager> instanceLazy = new(() => new());
        public static StateListenersManager Instance => Bootstrapper.GetInstance<StateListenersManager>();
        public static StateListenersManager Initialize() => instanceLazy.Value;
        #endregion

        private StateListenersManager()
        {
            ClassesManager.Instance.AssembliesUpdated += RecollectStateGroups;
            RecollectStateGroups([..ClassesManager.Instance.Assemblies]);
            GetStateGroup(MouseStateGroup.MouseIn).StateCalculator = CommonStateCalculator.CalculateMouseState;
            GetStateGroup(SelectionStateGroup.Selected).StateCalculator = CommonStateCalculator.CalculateSelectionState;
            GetStateGroup(FocusStateGroup.Focused).StateCalculator = CommonStateCalculator.CalculateFocusState;
        }

        #region state groups
        public readonly Dictionary<string, StateGroup> StateGroupsByStateName = [];
        private void RecollectStateGroups(Assembly[] range)
        {
            var gs = ClassesManager.Instance.GetClassesByAttribute<StateGroupRuleAttribute>(true, range);
            foreach (var (groupType, attrs) in gs)
            {
                var sg = StateGroup.FromEnum(groupType);
                foreach (var attr in attrs)
                {
                    var targetType = attr.RelativeType;
                    var events = attr.RelativeRoutedEventNames.Select(name => 
                            targetType.GetMember(name, BindingFlags.Public | BindingFlags.Static).First().GetMemberValue(null) as RoutedEvent)
                        .Where(e => e is not null)
                        .Select(e => e!);
                    sg.RelativeRoutedEventsInternal[targetType] = [.. events];
                }
                foreach(var stateName in sg.StateNames) StateGroupsByStateName[stateName] = sg;
            }
        }
        
        public StateGroup GetStateGroup(string stateName) => StateGroupsByStateName[stateName];
        public StateGroup GetStateGroup<TStateGroupSource>(TStateGroupSource state) => GetStateGroup(state?.ToString()!);
        #endregion

        #region rule str parser
        private readonly ITokenizer tokenizer = Tokenizer.CreateTokenizer(new TokenRegistry());
        private readonly PrattParser exprParser = PrattParser.CreateParser(new PrattParserRuleRegistry());

        private sealed record StateMapParseResult(Dictionary<string[], Dictionary<string, ExpressionNode>> KeyedSetters, HashSet<StateGroup> RelativeStateGroups, HashSet<string> RelativePropNames);
        private StateMapParseResult ParseStateMap(string stateMap)
        {
            //pre vars
            var mainDic = new Dictionary<string[], Dictionary<string, ExpressionNode>>();
            var relativeStateGroups = new HashSet<StateGroup>();
            var relativePropNames = new HashSet<string>();
            //
            var allTokens = tokenizer.Tokenize(stateMap);
            int currentPos = 0;
            while (currentPos < allTokens.Count)
            {
                int deduceIndex = -1;
                for (int i = currentPos; i < allTokens.Count; i++)
                {
                    if (allTokens[i].RegName == TokenRegistry.TOKEN_DEDUCE.RegName)
                    {
                        deduceIndex = i;
                        break;
                    }
                    if (allTokens[i].RegName == TokenRegistry.TOKEN_SPLIT.RegName)
                        throw new StateParseException("Invalid rule scheme: found split token before deduce token.");
                }
                if (deduceIndex == -1) break;
                int splitIndex = -1;
                for (int i = deduceIndex + 1; i < allTokens.Count; i++)
                {
                    if (allTokens[i].RegName == TokenRegistry.TOKEN_SPLIT.RegName)
                    {
                        splitIndex = i;
                        break;
                    }
                }
                int endPos = (splitIndex != -1) ? splitIndex : allTokens.Count;
                var stateTokens = allTokens.Slice(currentPos, deduceIndex);
                if (stateTokens.Length == 0) throw new StateParseException("Invalid rule scheme: empty state definition.");
                //
                var stateKey = new List<string>();
                foreach (var t in stateTokens)
                {
                    if (t is TOKEN_ID idToken)
                    {
                        var name = idToken.Name;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            stateKey.Add(name);
                            relativeStateGroups.Add(GetStateGroup(name));
                        }
                    }
                }
                if (stateKey.Count == 0) throw new StateParseException("Invalid rule scheme: no valid states found.");
                //
                var exprContentTokens = allTokens.Slice(deduceIndex + 1, endPos);
                if (exprContentTokens.Last().RegName == TokenRegistry.TOKEN_SPLIT.RegName) exprContentTokens = [.. exprContentTokens.Take(exprContentTokens.Length - 1)];
                if (exprContentTokens.Length == 0) throw new StateParseException("Invalid rule scheme: empty expression body.");
                var exprStream = new ValueStream<IToken>([TokenRegistry.TOKEN_LB, .. exprContentTokens, TokenRegistry.TOKEN_RB], endSign: TokenRegistry.TOKEN_END);
                var rootNode = exprParser.Parse(exprStream)
                    ?? throw new StateParseException("Invalid expr: parser returned null.");
                var setters = new Dictionary<string, ExpressionNode>();

                foreach (var cnode in rootNode.Children)
                {
                    var propName = string.Empty;
                    ExpressionNode? expr = null;
                    if (cnode is AssignmentExpressionNode assignNode && assignNode.Children.Count == 2)
                    {
                        if (assignNode.Children[0] is VariableExpressionNode varNode && varNode.Token is TOKEN_ID token_id) propName = token_id.Name;
                        if (assignNode.Children[1] is ExpressionNode exprNode) expr = exprNode;
                    }
                    else throw new StateParseException("Invalid expr: expected assignment (Prop=Expr).");
                    if (string.IsNullOrWhiteSpace(propName) || expr is null) throw new StateParseException("Invaild expr: expression group can not be parsed");
                    setters.Add(propName, expr);
                    relativePropNames.Add(propName);
                }
                mainDic.Add([.. stateKey], setters);
                currentPos = (splitIndex != -1) ? splitIndex + 1 : allTokens.Count;
            }
            return new StateMapParseResult(mainDic, relativeStateGroups, relativePropNames);
        }
        #endregion

        #region registration
        private static DependencyProperty? GetPropByName(string propName, Type? ownerType)
        {
            DependencyProperty? prop = null;
            if (typeof(Shape).IsAssignableFrom(ownerType) && propName == nameof(Shape.Stroke)) prop = Shape.StrokeProperty;
            else if (typeof(TextBlock).IsAssignableFrom(ownerType))
            {
                prop = propName switch
                {
                    "Back" => TextBlock.BackgroundProperty,
                    "Fore" => TextBlock.ForegroundProperty,
                    "Font" => TextBlock.FontSizeProperty,
                    _ => null
                };
            }
            else if (ownerType is null || typeof(Control).IsAssignableFrom(ownerType))
            {
                prop = propName switch
                {
                    "Back" => Control.BackgroundProperty,
                    "Fore" => Control.ForegroundProperty,
                    "Font" => Control.FontSizeProperty,
                    "Border" => Control.BorderBrushProperty,
                    _ => null
                };
            }
            else if (typeof(Border).IsAssignableFrom(ownerType))
            {
                prop = propName switch
                {
                    "Back" => Border.BackgroundProperty,
                    "Border" => Border.BorderBrushProperty,
                    _ => null
                };
            }
            else if (typeof(Panel).IsAssignableFrom(ownerType))
            {
                prop = propName switch
                {
                    "Back" => Panel.BackgroundProperty,
                    _ => null
                };
            }
            else if (typeof(Transform).IsAssignableFrom(ownerType))
            {
                prop = propName switch
                {
                    "ScaleX" => ScaleTransform.ScaleXProperty,
                    "ScaleY" => ScaleTransform.ScaleYProperty,
                    "Angle" => RotateTransform.AngleProperty,
                    _ => null
                };
            }
            prop ??= propName switch
            {
                "Opacity" => UIElement.OpacityProperty,
                _ => null
            };
            return prop;
           //throw new StateParseException($"Invaild propName: Failed get dependency property {propName} from {ownerType?.Name ?? "null"}");
        }
        private sealed class PropSetter(DependencyObject target, DependencyProperty targetProp, ExpressionNode expression, IASTContext context)
        {
            public readonly WeakReference<DependencyObject> TargetRef = new(target);
            public readonly DependencyProperty TargetProp = targetProp;
            public readonly ExpressionNode Expression = expression;
            public readonly IASTContext Context = context;
            public void DoSet()
            {
                if (!TargetRef.TryGetTarget(out var target)) return;
                var r = Expression.Evaluate(Context);
                if(r.Error is not null) return;
                TransitionsManager.Instance.TryDoTransition(target, TargetProp, r.Value, true);
            }
            public bool TargetIsSameTo(PropSetter setter)
                => TargetRef.TryGetTarget(out var tar) && setter.TargetRef.TryGetTarget(out var _tar) && tar == _tar && TargetProp == setter.TargetProp;
            public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();
            public override int GetHashCode()
            {
                TargetRef.TryGetTarget(out var tar);
                var hash = HashCode.Combine(tar, TargetProp, Expression, Context);
                return hash;
            }
        }
        private sealed class SetterRule(PropSetter[] setters, string[] status)
        {
            public readonly string[] Status = status;
            public readonly PropSetter[] Setters = setters;
        }
        private sealed class ListenerRegistration(UIElement element)
        {
            public readonly WeakReference<UIElement> ElementRef = new(element);
            public readonly Dictionary<StateGroup, string> Status = [];
            //for fresh
            public readonly Dictionary<DependencyProperty, ConditionalWeakTable<DependencyObject, PropSetter>> CurrentSettersByTarByTarProp = [];
            //main
            public readonly Dictionary<string, HashSet<PropSetter>> SettersByCombinedStatus = [];
            public readonly Dictionary<string, PropSetter[]> CachedSettersByCombinedKey = [];
            public event ValueChangedHandler<(StateGroup stateGroup, string state)>? StateChanged;
            public string EvaluateState(StateGroup sg, bool forceUpdate = false)
            {
                if (!ElementRef.TryGetTarget(out var element)) return string.Empty;
                if (forceUpdate || !Status.TryGetValue(sg, out var oldValue)) oldValue = string.Empty;
                var newValue = sg.StateCalculator(element);
                if (newValue != oldValue)
                {
                    Status[sg] = newValue;
                    StateChanged?.Invoke(element, new((sg, oldValue), (sg, newValue)));
                }
                return newValue;
            }
        }
        private readonly ConditionalWeakTable<UIElement, ListenerRegistration> registrations = [];
        private readonly ConditionalWeakTable<DependencyObject, IASTContext> contexts = [];
        public void Register(DependencyObject listener, IEnumerable<RuleSnapshot> listenRules)
        {
            if(listener is not UIElement element) return;
            if (!registrations.TryGetValue(element, out var reg)) 
            {
                registrations.TryAdd(element, reg = new(element));
                reg.StateChanged += (s, e) =>
                {
                    var status = reg.Status.Values.OrderBy(s => s).ToArray();
                    var newStatus = status.BuildString(split: "+");
                    var oldStatus = string.IsNullOrWhiteSpace(e.OldValue.state) ? string.Empty : status.Select(s => GetStateGroup(s) == e.NewValue.stateGroup ? e.OldValue.state : s).BuildString(split: "+");
                    if (!reg.CachedSettersByCombinedKey.TryGetValue(newStatus, out var newSetters))
                    {
                        newSetters = [.. reg.SettersByCombinedStatus.SelectMany(kv =>
                                {
                                    if (newStatus.Contains(kv.Key)) return kv.Value;
                                    return [];
                                })];
                        reg.CachedSettersByCombinedKey[newStatus] = newSetters;
                    }
                    if (!reg.CachedSettersByCombinedKey.TryGetValue(oldStatus, out var oldSetters))
                    {
                        oldSetters = [.. reg.SettersByCombinedStatus.SelectMany(kv =>
                                {
                                    if (oldStatus.ContainsIgnoreCase(kv.Key)) return kv.Value;
                                    return [];
                                })];
                        reg.CachedSettersByCombinedKey[oldStatus] = oldSetters;
                    }
                    foreach (var setter in newSetters)
                    {
                        if (!reg.CurrentSettersByTarByTarProp.TryGetValue(setter.TargetProp,
                            out var settersByTar)) settersByTar = reg.CurrentSettersByTarByTarProp[setter.TargetProp] = [];
                        if (setter.TargetRef.TryGetTarget(out var tar)) settersByTar.AddOrUpdate(tar, setter);
                        if (!oldSetters.Contains(setter)) setter.DoSet();
                    }
                };
            }
            var regSgs = new HashSet<StateGroup>();
            foreach (var sg in reg.Status.Keys) regSgs.Add(sg);
            foreach (var rule in listenRules)
            {
                if (!rule.DataSource.TryGetTarget(out var dataSource) || !rule.SettersTarget.TryGetTarget(out var settersTarget)) continue;
                if (!contexts.TryGetValue(dataSource, out var context)) contexts.Add(dataSource, context = BuildContext(dataSource));

                if (listener is EImage rtf)
                {

                }
                var parsed = ParseStateMap(rule.StateMap);
                //Refresh state event
                reg.CachedSettersByCombinedKey.Clear();
                foreach(var sg in parsed.RelativeStateGroups)
                {
                    if (reg.Status.ContainsKey(sg)) continue;
                    var eventsToReg = new HashSet<RoutedEvent>();
                    foreach (var (type, events) in sg.RelativeRoutedEvents)
                    {
                        if (type.IsAssignableFrom(element.GetType()))
                        {
                            foreach(var @event in events)
                            {
                                eventsToReg.Add(@event);
                            }
                        }
                    }
                    EventBatch.RegisterAction(element, eventsToReg, () => reg.EvaluateState(sg));
                    regSgs.Add(sg);
                }
                //Rebuild setters
                foreach(var (mkey, setters) in parsed.KeyedSetters)
                {
                    var _mkey = mkey.OrderBy(key => key).ToArray();
                    var combined_mkey = _mkey.BuildString(split: "+");
                    var _setters = setters.Select(s =>
                    {
                        var prop = GetPropByName(s.Key, settersTarget.GetType());
                        return prop is null ? null : new PropSetter(settersTarget, prop, s.Value, context);
                    }).Where(s => s is not null).ToList();
                    var sgs = _mkey.ToDictionary(key => key, GetStateGroup);
                    if (!reg.SettersByCombinedStatus.TryGetValue(combined_mkey, out var settersHashSet))
                        settersHashSet = reg.SettersByCombinedStatus[combined_mkey] = [];
                    foreach (var _setter in _setters) settersHashSet.Add(_setter!);
                }
                //
                
            }
            foreach (var sg in regSgs) reg.EvaluateState(sg, true);
        }
        public void Unregister(DependencyObject listener)
        {
            if(listener is not UIElement element) return;
            if(registrations.TryGetValue(element, out var reg))
            {
                var eventsToUnReg = new HashSet<RoutedEvent>();
                foreach(var (sg, _) in reg.Status)
                {
                    foreach (var (type, events) in sg.RelativeRoutedEvents)
                    {
                        if (type.IsAssignableFrom(element.GetType()))
                        {
                            foreach (var @event in events)
                            {
                                eventsToUnReg.Add(@event);
                            }
                        }
                    }
                }
                EventBatch.UnregisterAction(element, eventsToUnReg);
                registrations.Remove(element);
            }
        }
        public void RedoSetterTransitions(DependencyObject listener)
        {
            if(listener is not UIElement element) return;
            if(registrations.TryGetValue(element, out var reg))
            {
                foreach(var (tarProp, settersByTar) in reg.CurrentSettersByTarByTarProp)
                {
                    foreach (var (_, setter) in settersByTar)
                    {
                        setter.DoSet();
                    }
                }
            }
        }
        public void RevaluateStatus(DependencyObject listener)
        {
            if(listener is not UIElement element) return;
            if(registrations.TryGetValue(element, out var reg))
            {
                foreach(var (sg, _) in reg.Status)
                {
                    reg.EvaluateState(sg);
                }
            }
        }
        
        private static SetterContext BuildContext(DependencyObject dataSource) 
            => new(dataSource);

        private sealed class SetterContext(DependencyObject source) : IASTContext
        {
            private readonly WeakReference<DependencyObject> sourceRef = new(source);
            public object? GetConstantValue(string key)
            {
                throw new NotImplementedException();
            }

            public MethodInfo GetMethodInfo(string name)
            {
                if (methodInfos.TryGetValue(name, out var method)) return method;
                throw new NotImplementedException();
            }
            //private readonly static MethodInfo LerpMethodInfo = typeof(ColorDataExtensions).GetMethod("Lerp", BindingFlags.Static | BindingFlags.Public)!;
            private readonly static Dictionary<string, MethodInfo> methodInfos = typeof(ColorDataExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).ToDictionary(mi => mi.Name, mi => mi, StringComparer.OrdinalIgnoreCase);
            public object? GetVariableValue(string key)
            {
                if (namesMap.TryGetValue(key, out var name) && sourceRef.TryGetTarget(out var source))
                {
                    TransHelper.GetThemeControlParent(source, out _, out var tc);
                    var r = name switch
                    {
                        "PR" => tc?.PrimaryBrush,
                        "BC" => tc?.BackBrush,
                        "SE" => tc?.SecondaryBrush,
                        "FT" => tc?.FontBrush,
                        _ => null
                    };
                    var cd = CommonConverter.ToColor(r);
                    return cd;
                }
                throw new NotImplementedException();
            }
            private readonly static Dictionary<string, string> namesMap = new(StringComparer.OrdinalIgnoreCase)
            {
                ["PR"] = "PR",
                ["BC"] = "BC",
                ["SE"] = "SE",
                ["FT"] = "FT",
            };

            public object? SetConstantValue(string key, object? value)
            {
                throw new NotImplementedException();
            }
            public object? SetVariableValue(string key, object? value)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
