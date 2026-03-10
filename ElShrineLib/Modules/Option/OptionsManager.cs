using ElShrine;
using ElShrine.Common;
using ElShrine.Common.DataStructure;
using ElShrine.Common.Serialization;
using ElShrine.Modules.Log;
using ElShrine.Modules.Plugin;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;

namespace ElShrine.Modules.Option;

#region Serialization
internal sealed record OptionRawData
{
    public OptionRawData(string classFullName, string itemName, string? valueText)
    {
        Class = classFullName;
        Item = itemName;
        ValueText = valueText;
    }
    [DataMember] public string Class { get; init; }
    [DataMember] public string Item { get; init; }
    [DataMember] public string? ValueText { get; init; }
}

[DataContract(Name = "Options")]
internal sealed class OptionDataSet
{
    private sealed record OptionData
    {
        public OptionData(string ownerClass, string itemName, object? value = null)
        {
            Class = ownerClass;
            Key = itemName;
            Value = value;
        }
        public readonly string Class;
        public readonly string Key;
        public readonly object? Value = null;

        public static OptionData FromOptionItem(OptionItem optionItem)
            => new(optionItem.MemberInfo.DeclaringType!.FullName!, optionItem.ActualItemName, optionItem.GetValue());
    }

    [DataMember] private List<OptionRawData> rawData = [];
    public IReadOnlyList<OptionRawData> RawData => rawData;
    private Dictionary<string, OptionRawData> _cachedRawData = [];
    private SerializerBase _serializer = new XmlSerializer();
    public void RefreshData(IEnumerable<OptionItem> optionItems)
    {
        var data = optionItems.Select(OptionData.FromOptionItem);
        foreach (var item in data)
        {
            var key = $"{item.Class}.{item.Key}";
            var valueText = item.Value is null ? null : _serializer.SerializeToString(item.Value);
            _cachedRawData[key] = _cachedRawData[key] with { ValueText = valueText };
        }
    }
    public bool TryParseRawData(OptionItem optionItem, out object? data)
    {
        data = null;
        var d = OptionData.FromOptionItem(optionItem);
        var key = $"{d.Class}.{d.Key}";
        if (!_cachedRawData.TryGetValue(key, out var od)) return false;
        try
        {
            data = od.ValueText is null ? null : _serializer.DeserializeFromString(od.ValueText, optionItem.ValueType);
        }
        catch
        {
            return false;
        }
        return true;
    }




    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        rawData = [.. _cachedRawData.Select(kv => kv.Value)];
    }
    [OnDeserializing]
    private void OnDeserializing(StreamingContext context)
    {
        _serializer = new XmlSerializer();
        _cachedRawData = [];
    }
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        _cachedRawData = rawData.ToDictionary(od => $"{od.Class}.{od.Item}", od => od);
    }
}
#endregion

public sealed class OptionManager : IOptionManager
{
    public const string GlobalOptionName = "Global";
    public const string OptionFileName = "Options";
    private readonly ILogManager _log;
    private readonly ILogger _logger;
    private readonly IPluginManager _plugins;
    private CataItemIndexer<OptionItem> _optionItemsIndexer;
    private readonly ConcurrentDictionary<AssemblyLoadContext, List<OptionItem>> _optionItems;
    private readonly OptionDataSet _optionDataSet;
    public event ValueChangedHandler<object>? OptionChanged;
    public event CollectionChangedHandler<Cata>? CataChanged;
    public OptionManager(ILogManager log, IPluginManager plugins)
    {
        _log = log;
        _logger = _log.Main;
        _plugins = plugins;
        _optionItems = [];
        _optionDataSet = new();
        // initialize
        var mainOptionItems = CollectOptionItems(AssemblyLoadContext.Default);
        _optionItemsIndexer = new(mainOptionItems);

        // subscribe
        _plugins.PluginLoaded += OnPluginLoaded;
        _plugins.PluginPreUnload += OnPluginPreUnload;
    }

    #region callbacks
    private void OnPluginLoaded(IPlugin plugin, PluginInfo pluginInfo, AssemblyLoadContext ctx)
    {
        CollectOptionItems(AssemblyLoadContext.Default);
        if (ctx != AssemblyLoadContext.Default)
            CollectOptionItems(ctx);
        var count = LoadInternal();
        _logger.Log($"Loaded {GetOptionItemsText(count)}.");
        RebuildOptionItemsIndexer();
    }
    private void OnPluginPreUnload(IPlugin plugin, PluginInfo pluginInfo, AssemblyLoadContext ctx)
    {
        _optionItems.Remove(ctx, out _);
        RebuildOptionItemsIndexer();
    }
    #endregion

    #region core
    private List<OptionItem> CollectOptionItems(AssemblyLoadContext ctx)
    {
        using var sc = _logger.OpenScope(EntryContent.OmitOrExecutingPattern("Collecting option items from context ", ctx.GetContextName(), LogItemStyle.NoticeBlue));
        var optionItems = new List<OptionItem>();
        var optionClasses = AssemblyLoadContext.Default.GetClassesByAttribute<OptionAttribute>(true);
        _logger.Log($"{GetOptionItemsText(optionClasses.Count())} found.");
        foreach (var (optionClass, optionClassAttrs) in optionClasses)
        {
            var attr = optionClassAttrs[0];
            var name = TransOptionsName(string.IsNullOrWhiteSpace(attr.OverrideName) ? optionClass.Name : attr.OverrideName);
            var instance = optionClass.IsStaticClass() ? null :
                MBootstrapper.Resolve(optionClass, cache: true);
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
        _logger.Log($"Collected {GetOptionAllText(optionClasses.Count(), optionItems.Count)}.");
        _optionItems[ctx] = optionItems;
        _logger.Log("Refreshed cached option items.");
        return optionItems;
    }
    private void RebuildOptionItemsIndexer()
    {
        _optionItemsIndexer = new(_optionItems.Values.SelectMany(i => i));
        _logger.Log("Rebuilt option items indexer.");
    }
    #endregion

    #region view operations
    public IEnumerable<Cata> Get(bool onlyChanged = false)
        => _optionItemsIndexer.GetAllCatas(filter: onlyChanged ? ListAnyHasChangedPredicate : null);
    public IEnumerable<OptionItem> Get(string virtualCata, bool onlyChanged = false)
        => _optionItemsIndexer.QueryCata(
            cataName: string.IsNullOrWhiteSpace(virtualCata) ? GlobalOptionName : virtualCata,
            cataType: CataType.Virtual,
            filter: onlyChanged ? HasChangedPredicate : null);
    public IReadOnlyDictionary<Cata, IReadOnlyList<OptionItem>> GetAll(bool onlyChanged = false)
        => _optionItemsIndexer.GetAll(filter: onlyChanged ? ListAnyHasChangedPredicate : null);

    private readonly static Predicate<OptionItem> HasChangedPredicate = static item => item.HasChanged;
    private readonly static Predicate<IReadOnlyList<OptionItem>> ListAnyHasChangedPredicate = static l => l.Any(static i => HasChangedPredicate(i));
    #endregion

    /*#region edit operations
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
    #endregion*/

    public void Save()
    {
        using var scope = _logger.OpenScope("Saving options...");
        var items = _optionItems.SelectMany(i => i.Value).ToList();
        _optionDataSet.RefreshData(items);
        _logger.Log($"Processing {GetOptionItemsText(_optionDataSet.RawData.Count)}...");
        var r = DataHandler.Write(_optionDataSet, new FileDetails(OptionFileName));
        //
        if (!r.Success) _logger.Error(r.FailedSource!);
        _logger.ConfigEnd(r.Success ? $"Successfully saved {GetOptionItemsText(_optionDataSet.RawData.Count)}." : "Failed to save options.");
    }
    public void Load()
    {
        using var scope = _logger.OpenScope("Loading options...");
        var r = DataHandler.Read<OptionDataSet>(null, new FileDetails(OptionFileName));
        string endMsg;
        if (!r.Success)
        {
            _logger.Warning(r.FailedSource!);
            _logger.Log("The option files issues can be override by execute options saving.");
            endMsg = "Failed to load options.";
        }
        else
        {
            var sucReadCount = LoadInternal();
            endMsg = $"Loaded {GetOptionItemsText(sucReadCount)} from option file.";
        }
        _logger.ConfigEnd(endMsg);
    }
    private int LoadInternal()
    {
        var sucReadCount = 0;
        var items = _optionItems.SelectMany(i => i.Value);
        foreach (var item in items)
        {
            var suc = _optionDataSet.TryParseRawData(item, out var data);
            if (suc)
            {
                item.SetValue(data);
                sucReadCount++;
            }
        }
        return sucReadCount;
    }

    #region helpers

    private static string TransOptionsName(string name)
    {
        var newName = name.RemovePartsIgnoreCase("Options", "Option");
        if (newName.IsEmpty()) newName = GlobalOptionName;
        return newName;
    }

    #region log helpers
    private static string GetOptionsText(int count) => $"{"option".GetPuralWithNum(count)}";
    private static string GetOptionItemsText(int count) => $"{"item".GetPuralWithNum(count)}";
    private static string GetOptionAllText(int optionsCount, int optionItemsCount)
        => $"{GetOptionItemsText(optionItemsCount)} in {GetOptionsText(optionsCount)}";
    #endregion

    #endregion
}
