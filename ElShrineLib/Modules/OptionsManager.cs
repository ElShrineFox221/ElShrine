using ElShrine.Common;
using ElShrine.Common.DataStructure;
using ElShrine.Common.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace ElShrine.Modules
{
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

    #region Serialization
    [DataContract]
    internal sealed record OptionData
    {
        public OptionData(string className, string itemName, object? value = null)
        {
            Class = className;
            Key = itemName;
            Value = value;
        }
        [DataMember] public readonly string Class;
        [DataMember] public readonly string Key;
        [DataMember] public readonly object? Value = null;
    }
    [DataContract(Name = "Options")]
    [KnownType(nameof(GetKnownTypes))]
    internal sealed class OptionDataSet
    {
        private static readonly HashSet<Type> knownTypes = [];
        private static List<Type> GetKnownTypes() => [..knownTypes];
        [DataMember] public readonly List<OptionData> data = [];
        public void RefreshData(List<OptionItem> items)
        {
            data.Clear();
            knownTypes.Clear();
            foreach (var item in items)
            {
                var dataItem = item.ToOptionData();
                knownTypes.Add(item.ValueType);
                data.Add(dataItem);
            }
        }
    }
    #endregion

    public sealed class OptionItem : ICataItem
    {
        public required string ActualCataName { get; init; }
        public string VirtualCataName { get; init; } = string.Empty;
        public required string ActualItemName { get; init; }
        public string VirtualItemName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public required object? DefaultValue { get; init; }
        public required Type ValueType { get; init; }
        public required MemberInfo MemberInfo { get; init; }
        public object? OwnerInstance { get; init; }


        public bool HasChanged => !Equals(GetValue(), DefaultValue);
        public object? GetValue() => MemberInfo.GetMemberValue(OwnerInstance);
        public bool SetValue(object? value)
        {
            if (Equals(value, GetValue())) return false;
            MemberInfo.SetMemberValue(OwnerInstance, value);
            return true;
        }

        internal OptionData ToOptionData()
            => new(ActualCataName, ActualItemName, GetValue());
        internal bool ReadOptionData(OptionData optionData)
        {
            var matched = optionData.Class == ActualCataName && optionData.Key == ActualItemName;
            if (matched) SetValue(optionData.Value);
            return matched;
        }
    }

    [InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_OPTIONS)]
    public sealed class OptionsManager : IInitializable<OptionsManager>
    {
        #region Singleton
        private static readonly Lazy<OptionsManager> instanceLazy = new(() => new());
        public static OptionsManager Instance => Bootstrapper.GetInstance<OptionsManager>();
        public static OptionsManager Initialize() => instanceLazy.Value;
        #endregion

        public const string GlobalOptionName = "Global";
        public const string OptionFileName = "Options";
        private static LogSession Session => LogProducer.Instance.CoreSession;
        private OptionsManager()
        {
            var sw = Stopwatch.StartNew();
            using var _ = Session.OpenScope("Initializing options manager...");
            //Register a recalculation delegate
            ClassesManager.Instance.AssembliesUpdated += RecollectOptionItems;
            //Instant recalculate
            RecollectOptionItems([.. ClassesManager.Instance.Assemblies]);
            //Do initial load
            Session.Log("Try initial loading.");
            Load();
            Session.ConfigEnd($"Options manager initialized, {sw.GetStopwatchElapsed()}");
        }

        private readonly OptionDataSet optionDataSet = new();
        private readonly List<OptionItem> optionItems = [];
        private readonly List<Type> optionClasses = [];
        private CataItemIndexer<OptionItem> optionItemQueryIndex = new([]);
        private void RecollectOptionItems(Assembly[] range)
        {
            using var _ = Session.OpenScope("Recollecting option items...");
            var optionClasses = ClassesManager.Instance.GetClassesByAttribute<OptionAttribute>(inherit: false, range);
            Session.Log($"{GetOptionItemsText(optionClasses.Count())} found.");
            foreach (var (optionClass, optionClassAttrs) in optionClasses)
            {
                var attr = optionClassAttrs[0];
                var name = (string.IsNullOrWhiteSpace(attr.OverrideName) ? optionClass.Name : attr.OverrideName)
                    .Replace("Options", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("Options", string.Empty, StringComparison.OrdinalIgnoreCase);
                if (string.IsNullOrWhiteSpace(name)) name = GlobalOptionName;
                var instance = optionClass.IsStaticClass() ? null : optionClass.GetProperty(nameof(IInitializable<>.Instance))?.GetValue(null);
                //
                var memberInfos = optionClass.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<OptionItemAttribute>() is not null && m.GetCustomAttribute<IgnoreOptionItemAttribute>() is null);
                foreach (var memberInfo in memberInfos)
                {
                    var info = memberInfo.GetCustomAttribute<OptionItemAttribute>()!;
                    var oi = new OptionItem()
                    {
                        ActualCataName = optionClass.FullName!,
                        VirtualCataName = name,
                        ActualItemName = memberInfo.Name,
                        VirtualItemName = info.OverrideName == string.Empty ? memberInfo.Name : info.OverrideName,
                        Description = info.Description,
                        DefaultValue = memberInfo.GetMemberValue(instance),
                        OwnerInstance = instance,
                        ValueType = memberInfo switch
                        {
                            FieldInfo fieldInfo => fieldInfo.FieldType,
                            PropertyInfo propertyInfo => propertyInfo.PropertyType,
                            _ => typeof(object)
                        },
                        MemberInfo = memberInfo
                    };
                    optionItems.Add(oi);
                }
            }
            optionDataSet.RefreshData(optionItems);
            Session.Log($"Recollected {GetOptionAllText()}.");
            optionItemQueryIndex = new(optionItems);
            Session.Log($"Rebuilt options indexes.");
            Session.ConfigEnd($"Recollected option items.");
        }
        private static string GetOptionsText(int count) => $"{"option".GetPuralWithNum(count)}";
        private static string GetOptionItemsText(int count) => $"{"item".GetPuralWithNum(count)}";
        private string GetOptionAllText() => $"{GetOptionItemsText(optionItems.Count)} in {GetOptionsText(optionClasses.Count)}";


        public event CollectionChangedHandler<object?>? OptionChanged;
        #region Operations

        #region IO operations
        public void Save()
        {
            using var scope = Session.OpenScope("Saving options...");
            Session.Log($"Processing {GetOptionAllText()}...");
            //
            var r = DataHandler.Write(optionDataSet, new FileDetails() { FileName = OptionFileName });
            //
            if (!r.Success) Session.Error(r.FailedSource!); 
            Session.ConfigEnd(r.Success ? $"Successfully saved {GetOptionAllText()}." : "Failed to save options.");
        }
        public void Load()
        {
            using var scope = Session.OpenScope("Loading options...");
            var r = DataHandler.Read<OptionDataSet>(null, new FileDetails() { FileName = OptionFileName });
            string endMsg;
            if (!r.Success)
            {
                Session.Warning(r.FailedSource!);
                Session.Log("The option files issues can be override by execute options saving.");
                endMsg = "Failed to load options.";
            }
            else
            {
                var dataLookup = r.Data!.data.ToDictionary(d => $"{d.Class}.{d.Key}", d => d);
                var sucReadCount = 0;
                foreach (var oi in optionItems)
                {
                    var lookupKey = $"{oi.ActualCataName}.{oi.ActualItemName}";
                    if (dataLookup.TryGetValue(lookupKey, out var matchedData))
                    {
                        try
                        {
                            if (oi.ReadOptionData(matchedData)) sucReadCount++;
                        }
                        catch (Exception ex)
                        {
                            Session.Warning(ex);
                        }
                    }
                }
                endMsg = $"Loaded {GetOptionItemsText(sucReadCount)} from option file.";
            }
            Session.ConfigEnd(endMsg);
        }
        #endregion

        #region View operations
        public IEnumerable<Cata> Get(bool onlyChanged = false) 
            => optionItemQueryIndex.GetAllCatas(filter: onlyChanged ? ListAnyHasChangedPredicate : null);
        public IEnumerable<OptionItem> Get(string virtualCata, bool onlyChanged = false) 
            => optionItemQueryIndex.QueryCata(
                cataName: string.IsNullOrWhiteSpace(virtualCata) ? GlobalOptionName : virtualCata, 
                cataType: CataType.Virtual, 
                filter: onlyChanged ? HasChangedPredicate : null);
        public IReadOnlyDictionary<Cata, IReadOnlyList<OptionItem>> GetAll(bool onlyChanged = false) 
            => optionItemQueryIndex.GetAll(filter: onlyChanged ? ListAnyHasChangedPredicate : null);

        private readonly static Predicate<OptionItem> HasChangedPredicate = static item => item.HasChanged;
        private readonly static Predicate<IReadOnlyList<OptionItem>> ListAnyHasChangedPredicate = static l => l.Any(static i => HasChangedPredicate(i));
        #endregion

        #region Edit operations
        public bool Set(OptionItem item, object? value) => SetInner(item, value, true);
        private bool SetInner(OptionItem item, object? value, bool notify)
        {
            var suc = item.SetValue(value);
            if (notify) OptionChanged?.Invoke(item, new(null, null, modified: [item]));
            return suc;
        }
        public void Reset(OptionItem item) => ResetInner(item, true);
        private bool ResetInner(OptionItem item, bool notify)
        {
            var suc = item.SetValue(item.DefaultValue);
            if (notify) OptionChanged?.Invoke(item, new(null, null, modified: [item]));
            return suc;
        }
        public IReadOnlyList<OptionItem> Reset(string virtualCata)
        {
            virtualCata = string.IsNullOrWhiteSpace(virtualCata) ? GlobalOptionName : virtualCata;
            var items = optionItems.Where(i =>
            {
                var matched = i.VirtualCataName.EqualIgnoreCase(virtualCata);
                if (matched) matched = ResetInner(i, false);
                return matched;
            }).ToList();
            OptionChanged?.Invoke(null, new(null, null, modified: items));
            return items;
        }
        public IReadOnlyList<OptionItem> Reset()
        {
            var items = optionItems.Where(i => ResetInner(i, false)).ToList();
            OptionChanged?.Invoke(null, new(null, null, modified: items));
            return items;
        }
        #endregion

        #endregion
    }
}
