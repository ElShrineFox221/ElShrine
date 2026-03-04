using ElShrine.Common.DataStructure;
using ElShrine.Modules.Log;
using System.Reflection;

namespace ElShrine.Modules;

#region Attributes
public sealed class CommandCarrierAttribute : SingletonAttribute
{
    public string? OverrideName = string.Empty;
}
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute : SingletonItemAttribute<CommandCarrierAttribute>
{
    public string? OverrideName { get; init; }
    public string Description { get; init; } = string.Empty;
}
[AttributeUsage(AttributeTargets.Method)]
public sealed class IgnoreCommandAttribute : Attribute;
#endregion

public sealed class CommandItem : ICataItem
{
    public required string ActualCataName { get; init; }
    public string VirtualCataName { get; init; } = string.Empty;
    public required string ActualItemName { get; init; }
    public string VirtualItemName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public required MethodInfo MethodInfo { get; init; }
    public object? OwnerInstance { get; init; }
}


[InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_COMMANDS)]
public sealed class CommandsManager : IInitializable<CommandsManager>
{
    #region Singleton
    private static readonly Lazy<CommandsManager> instanceLazy = new(() => new());
    public static CommandsManager Instance => Bootstrapper.GetInstance<CommandsManager>();
    public static CommandsManager Initialize() => instanceLazy.Value;
    #endregion

    public const string GlobalCommandCarrierName = "Global";
    private static LogSession Session => LogProducer.Instance.CoreSession;
    private CommandsManager()
    {
        //Register a recollecting delegate
        ClassesManager.Instance.AssembliesUpdated += RecollectCommands;
        //Instant recollect
        RecollectCommands([.. ClassesManager.Instance.Assemblies]);
    }

    private readonly List<CommandItem> commands = [];
    private readonly List<Type> commandCarrierClasses = [];
    private CataItemIndexer<CommandItem> commandQueryIndex = new([]);
    private void RecollectCommands(Assembly[] range)
    {
        using var _ = Session.OpenScope("Recollecting commands...");
        var discoveredClasses = ClassesManager.Instance.GetClassesByAttribute<CommandCarrierAttribute>(inherit: false, range);
        Session.Log($"{GetCommandCarrierText(discoveredClasses.Count())} found.");
        foreach (var (carrierClass, carrierClassAttrs) in discoveredClasses)
        {
            if (!commandCarrierClasses.Contains(carrierClass)) commandCarrierClasses.Add(carrierClass);
            var attr = carrierClassAttrs[0];
            var className = (string.IsNullOrWhiteSpace(attr.OverrideName) ? carrierClass.Name : attr.OverrideName)
                .Replace("CommandsCarrier", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("Commands", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("Carrier", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(className)) className = GlobalCommandCarrierName;
            var instance = carrierClass.IsStaticClass() ? null : carrierClass.GetProperty(nameof(IInitializable<>.Instance))?.GetValue(null);
            var methods = carrierClass.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<CommandAttribute>() is not null && m.GetCustomAttribute<IgnoreCommandAttribute>() is null);
            foreach (var method in methods)
            {
                var cmdAttr = method.GetCustomAttribute<CommandAttribute>()!;
                bool isStatic = method.IsStatic;
                var ci = new CommandItem()
                {
                    ActualCataName = carrierClass.FullName!,
                    VirtualCataName = className,
                    ActualItemName = method.Name,
                    VirtualItemName = string.IsNullOrWhiteSpace(cmdAttr.OverrideName) ? method.Name : cmdAttr.OverrideName,
                    Description = cmdAttr.Description,
                    MethodInfo = method,
                    OwnerInstance = isStatic ? null : instance
                };
                commands.Add(ci);
            }
        }
        Session.Log($"Recollected {GetCommandAllText()}.");
        commandQueryIndex = new(commands);
        Session.Log($"Rebuilt commands indexes.");
        Session.ConfigEnd($"Successfully recollected commands.");
    }

    public static string GetCommandCarrierText(int count) => $"{"command carrier".GetPuralWithNum(count)}";
    private static string GetCommandsText(int count) => $"{"command".GetPuralWithNum(count)}";
    private string GetCommandAllText() => $"{GetCommandsText(commands.Count)} in {GetCommandCarrierText(commandCarrierClasses.Count)}";

    #region Operations
    public IReadOnlyList<CommandItem> Get(string virtualCata)
        => commandQueryIndex.QueryCata(virtualCata, CataType.Virtual);
    public IReadOnlyList<CommandItem> Get(string virtualCata, string virtualItemName, int paramsCount = -1)
        => commandQueryIndex.Query(virtualCata, virtualItemName, paramsCount >= 0 ? ((CommandItem ci) => ci.MethodInfo.GetParameters().Length == paramsCount) : null);
    public IReadOnlyList<CommandItem> GetAll()
        => commands;
    #endregion
}
