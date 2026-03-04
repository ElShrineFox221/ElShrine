using ElShrine.Modules;
using ElShrine.Modules.Log;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ElShrine;

public static class Bootstrapper
{
    #region Const Priorities
    public const int PRIO_TIMER = int.MinValue;
    public const int PRIO_LOGPRODUCER = int.MaxValue -1;
    public const int PRIO_CLASSES = int.MaxValue - 2;
    public const int PRIO_OPTIONS = int.MaxValue - 3;
    public const int PRIO_PARAMPARSERS = int.MaxValue - 4;
    public const int PRIO_COMMANDS = int.MaxValue - 5;

    public const int PRIO_LOGCONSUMER = int.MinValue;
    #endregion

    private readonly static ConcurrentDictionary<Type, object> instances = [];
    private static ClassesManager? classesManager;
    private static LogSession? session;
    private static BeatTimer? timer;
    public readonly static DateTime InitializedTime = DateTime.Now;
    public readonly static string InitializeTimeText = InitializedTime.ToLocalTime().ToString(Const.FullDateTimeFormat).Replace(':', '\'');
    private static bool initialized = false;
    public static void ManualInitialize()
    {
        if (initialized) return;
        initialized = true;
        var sw = Stopwatch.StartNew();
        try
        {
            //initialize core services
            timer = GetInstance<BeatTimer>();
            //initialize logs producer
            session = GetInstance<LogProducer>().CoreSession;
            //initialize classes manager
            classesManager = GetInstance<ClassesManager>();
            //initialize options manager
            GetInstance<OptionsManager>();
            //initialize param parsers manager
            GetInstance<ParamParserManager>();
            //initialize commands manager
            GetInstance<CommandsManager>();
            //initialize modules
            var modules = classesManager.GetClassesByAttribute<InitializationInfoAttribute>(false)
                .Where(m => m.Value[0].PreInstantiate).OrderBy(m => m.Value[0].Priority);
            var getInstanceMethod = typeof(Bootstrapper).GetMethod(nameof(GetInstance), genericParameterCount: 1, [])
                ?? throw new InvalidOperationException("GetInstance method not found.");
            foreach (var module in modules)
            {
                var type = module.Key;
                if(type.IsStaticClass()) RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                else getInstanceMethod.MakeGenericMethod(type).Invoke(null, null);
            }
        }
        catch (Exception ex)
        {
            session!.Error(ex);
        }
        var scope = session!.GetCurrentScope();
        var items = scope.GetSummaryItems();
        session.Log([..items, LogItem.Normal($"Modules initialized, {sw.GetStopwatchElapsed()}.")]);
        if (scope.Errors.Count > 0) Exit();
    }
    static Bootstrapper() => ManualInitialize();

    public static TIns GetInstance<TIns>() where TIns : class, IInitializable<TIns>
    {
        var ins = (instances.GetOrAdd(typeof(TIns), k =>
        {
            using var _ = session?.OpenScope(GetModuleInitializationContent(typeof(TIns)));
            return TIns.Initialize();
        }) as TIns)!;
        return ins;
    }
    public static object? GetInstance(Type type)
    {
        if (!type.IsImplementOf(typeof(IInitializable<>))) return null;
        return instances.GetOrAdd(type, type =>
        {
            using var _ = session?.OpenScope(GetModuleInitializationContent(type));
            var mi = type.GetMethod(nameof(IInitializable<>.Initialize), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var ins = mi?.Invoke(null, []);
            return ins!;
        });
    }
    private static LogItem[] GetModuleInitializationContent(Type type)
        => [LogItem.Normal($"Initializing "), LogItem.Normal(type.Name, LogItemStyle.NoticeCyan), LogItem.Normal("...")];

    public static void Exit()
    {
        foreach (var instance in instances.Values)
        {
            if(instance is IDisposable ins) ins.Dispose();
        }
        Environment.Exit(0);
    }
}
public interface IInitializable<TIns> where TIns : class
{
    public static abstract TIns Initialize();
    public abstract static TIns Instance { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InitializationInfoAttribute : ValidatableClassAttribute
{
    public bool PreInstantiate = true;
    public int Priority = 0;

    protected override bool ValidateType(Type t)
    {
        var suc = t.IsStaticClass() || typeof(IInitializable<>).IsBaseOrInterfaceOf(t);
        if (!suc) ValidateFailedReason = $"Not implement the interface {nameof(IInitializable<>)}<T>, or not a static class";
        return suc;
    }
}
