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
[DataContract]
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
    private SerializerBase _serializer = new JsonSerializer();
    public void RefreshData(IEnumerable<OptionItem> optionItems)
    {
        var data = optionItems.Select(OptionData.FromOptionItem);
        foreach (var item in data)
        {
            var key = $"{item.Class}.{item.Key}";
            var valueText = item.Value is null ? null : _serializer.SerializeToString(item.Value);
            _cachedRawData[key] = new(item.Class, item.Key, valueText);
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

internal sealed class OptionManager : PluginResourceTracker<OptionBase>, IOptionManager
{
    public const string OptionFileName = "Options";
    private readonly ILogManager _log;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<AssemblyLoadContext, List<OptionItem>> _optionItems;
    private readonly OptionDataSet _optionDataSet;
    private CataItemIndexer<OptionItem> _optionItemsIndexer;
    public event ValueChangedHandler<object?>? OptionChanged;

    public OptionManager(ILogManager log, IPluginManager plugins) : base(plugins, false)
    {
        _log = log;
        _logger = _log.Main;
        _optionItems = [];
        _optionDataSet = new();
        _optionItemsIndexer = new([]);
        //
        DoCollectResources(AssemblyLoadContext.Default);
    }

    #region overrides
    protected override void DoCollectResources(AssemblyLoadContext ctx)
    {
        using var sc = OpenRecollectTextScope(_logger, ctx);
        base.DoCollectResources(ctx);
        _logger.Log($"Collected {GetOptionAllText(Resources[ctx].Values.Count, _optionItems[ctx].Count)}.");
        var count = LoadInternal();
        _logger.Log($"Reloaded {GetOptionItemsText(count)}.");
        RebuildOptionItemsIndexer();
    }
    protected override void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx)
    {
        if (unloadingCtx && Resources.TryGetValue(ctx, out var options))
        {
            foreach (var option in options)
            {
                option.Value.PropertyChanged -= OnOptionChanged;
            }
        }
        base.OnPluginUnloading(plugin, info, ctx, unloadingCtx);
        RebuildOptionItemsIndexer();
    }
    protected override void OnResourceCreated(OptionBase resource, AssemblyLoadContext ctx)
    {
        resource.PropertyChanged += OnOptionChanged;
        var items = ExtractOptionItems(resource);
        _optionItems.GetOrAdd(ctx, _ => []).AddRange(items);
    }
    protected override void OnResourceReleasing(OptionBase resource, AssemblyLoadContext ctx)
    {
        resource.PropertyChanged -= OnOptionChanged;
        _optionItems.TryRemove(ctx, out _);
    }
    #endregion

    #region callbacks
    private void OnOptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        var items = sender is not OptionBase option ? [] : Get(option.OptionCataName);
        var item = items.FirstOrDefault(i => i.ActualItemName == e.PropertyName);
        OptionChanged?.Invoke(item, new(e.OldValue, e.NewValue));
    }
    #endregion

    #region core
    private static IEnumerable<OptionItem> ExtractOptionItems(OptionBase option)
    {
        var instance = option;
        var optionClass = option.GetType();
        var name = TransOptionsName(instance.OptionCataName);
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
            yield return oi;
        }
    }
    private void RebuildOptionItemsIndexer()
    {
        var items = _optionItems.Values.SelectMany(i => i).ToList();
        _optionItemsIndexer = new(items);
        _logger.Log("Rebuilt option items indexer.");
        _optionDataSet.RefreshData(items);
        _logger.Log("Refreshed cached option data.");
    }
    #endregion

    #region view operations
    public IEnumerable<Cata> Get(bool onlyChanged = false)
        => _optionItemsIndexer.GetAllCatas(filter: onlyChanged ? ListAnyHasChangedPredicate : null);
    public IEnumerable<OptionItem> Get(string virtualCata, bool onlyChanged = false)
        => _optionItemsIndexer.QueryCata(
            cataName: string.IsNullOrWhiteSpace(virtualCata) ? OptionBase.GlobalOptionName : virtualCata,
            cataType: CataType.Virtual,
            filter: onlyChanged ? HasChangedPredicate : null);
    public IReadOnlyDictionary<Cata, IReadOnlyList<OptionItem>> GetAll(bool onlyChanged = false)
        => _optionItemsIndexer.GetAll(filter: onlyChanged ? ListAnyHasChangedPredicate : null);
    public TOption GetOption<TOption>() where TOption : OptionBase
    {
        var option = Resources.Values.SelectMany(o => o.Values).FirstOrDefault(o => o.GetType() == typeof(TOption));
        return (option as TOption)!;
    }

    private readonly static Predicate<OptionItem> HasChangedPredicate = static item => item.HasChanged;
    private readonly static Predicate<IReadOnlyList<OptionItem>> ListAnyHasChangedPredicate = static l => l.Any(static i => HasChangedPredicate(i));
    #endregion

    #region edit operations
    public void Set(OptionItem item, object? value) => SetInner(item, value);
    private static bool SetInner(OptionItem item, object? value)
    {
        var suc = item.SetValue(value);
        return suc;
    }
    public void Reset(OptionItem item)
    {
        SetInner(item, item.DefaultValue);
    }
    #endregion

    public void Save()
    {
        using var scope = _logger.OpenScope("Saving options...");
        RebuildOptionItemsIndexer();
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
        if (sucReadCount > 0) 
            RebuildOptionItemsIndexer();
        return sucReadCount;
    }

    #region helpers

    private static string TransOptionsName(string name)
    {
        var newName = name.RemovePartsIgnoreCase("Options", "Option");
        if (newName.IsEmpty()) newName = OptionBase.GlobalOptionName;
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
