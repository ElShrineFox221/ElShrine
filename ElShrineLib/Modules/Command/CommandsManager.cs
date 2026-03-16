using ElShrine.Common.DataStructure;
using ElShrine.Modules.Log;
using ElShrine.Modules.Plugin;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace ElShrine.Modules.Command;

internal sealed class CommandsManager : PluginAwareServiceBase, ICommandManager
{
    public const string GlobalCommandCarrierName = "Global";

    private readonly ILogger _logger;
    public CommandsManager(IPluginManager plugin, ILogManager log) : base(plugin)
    {
        _logger = log.Main;
        //
        DoCollectCommands(AssemblyLoadContext.Default);
    }

    private readonly ConcurrentDictionary<AssemblyLoadContext, ConcurrentDictionary<Type, ConcurrentBag<CommandItem>>> _commands = [];
    private CataItemIndexer<CommandItem> commandQueryIndex = new([]);

    private void CollectCommands(AssemblyLoadContext ctx)
    {
        if (!_commands.TryGetValue(ctx, out var commandsDict))
            commandsDict = _commands[ctx] = [];
        var types = ctx.GetClassesByAttribute<CommandCarrierAttribute>(true);
        foreach (var (type, attrs) in types)
        {
            if (attrs.Count == 0 || commandsDict.TryGetValue(type, out var commands))
                continue;
            var attr = attrs[0];
            commands = commandsDict[type] = [];

            var name = (string.IsNullOrWhiteSpace(attr.OverrideName) ? type.Name : attr.OverrideName)
                .RemovePartsIgnoreCase("CommandsCarrier", "Commands", "Carrier");
            if (string.IsNullOrEmpty(name)) name = GlobalCommandCarrierName;

            var instance = type.IsStaticClass() ? null : MBootstrapper.Resolve(type, disposeWhenExit: true);
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<CommandAttribute>() is not null && m.GetCustomAttribute<IgnoreCommandAttribute>() is null);
            foreach (var method in methods)
            {
                var cmdAttr = method.GetCustomAttribute<CommandAttribute>()!;
                bool isStatic = method.IsStatic;
                var ci = new CommandItem()
                {
                    ActualCataName = type.FullName!,
                    VirtualCataName = name,
                    ActualItemName = method.Name,
                    VirtualItemName = string.IsNullOrWhiteSpace(cmdAttr.OverrideName) ? method.Name : cmdAttr.OverrideName,
                    Description = cmdAttr.Description,
                    MethodInfo = method,
                    OwnerInstance = isStatic ? null : instance
                };
                commands.Add(ci);
            }
        }
    }
    private void DoCollectCommands(AssemblyLoadContext ctx)
    {
        using var sc = OpenRecollectTextScope(_logger, ctx);
        CollectCommands(AssemblyLoadContext.Default);
        if (ctx != AssemblyLoadContext.Default) 
            CollectCommands(ctx);
        var commandDicts = _commands.Values.SelectMany(static i => i);
        var commandsCount = commandDicts.Sum(static i => i.Value.Count);
        _logger.Log($"Collected {GetCommandAllText(commandsCount, commandDicts.Count())}.");
        RebuildIndexes();
    }
    private void RebuildIndexes()
    {
        var commandCollections = _commands.SelectMany(static i => i.Value.Values);
        var commands = commandCollections.SelectMany(static i => i);
        commandQueryIndex = new(commands);
        _logger.Log($"Rebuilt commands indexes.");
    }

    protected override void OnPluginLoaded(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedNewCtx)
    {
        if (loadedNewCtx) 
            DoCollectCommands(ctx);
    }
    protected override void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx)
    {
        if (unloadingCtx)
        {
            _commands.Remove(ctx, out _);
            RebuildIndexes();
        }
    }

    #region texts
    public static string GetCommandCarrierText(int count) => $"{"command carrier".GetPuralWithNum(count)}";
    private static string GetCommandsText(int count) => $"{"command".GetPuralWithNum(count)}";
    private static string GetCommandAllText(int commandsCount, int carriersCount) => $"{GetCommandsText(commandsCount)} in {GetCommandCarrierText(carriersCount)}";
    private static LogScopeAccessor OpenRecollectTextScope(ILogger logger, AssemblyLoadContext ctx)
    {
        var item0 = LogItem.Normal("Recollecting commands from context ");
        var item1 = LogItem.Normal(ctx.Name ?? "Unknown", LogItemStyle.NoticeBlue);
        var item2 = LogItem.Normal("...");
        return logger.OpenScope(new([item0, item1, item2]));
    }
    #endregion

    #region Operations
    public IReadOnlyList<CommandItem> Get(string virtualCata)
        => commandQueryIndex.QueryCata(virtualCata, CataType.Virtual);
    public IReadOnlyList<CommandItem> Get(string virtualCata, string virtualItemName, int paramsCount = -1)
        => commandQueryIndex.Query(virtualCata, virtualItemName, paramsCount >= 0 ? ((CommandItem ci) => ci.MethodInfo.GetParameters().Length == paramsCount) : null);
    public IReadOnlyDictionary<Cata, IReadOnlyList<CommandItem>> GetAll()
        => commandQueryIndex.GetAll();
    #endregion
}
