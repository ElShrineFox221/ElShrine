using ElShrine.Commands;
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
        LogCommands.Reconstruct();
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


public interface IModuleRegister
{
    void RegisterModule<TService, TImplementation>(TImplementation? instance = null)
        where TImplementation : class, TService
        where TService : notnull;
}
public static class MBootstrapper
{
    private sealed class ModuleServiceBuilder : IServiceProvider, IModuleRegister
    {
        private readonly ConcurrentDictionary<Type, object> _instancesCached = [];
        private readonly ConcurrentDictionary<Type, Type> _servicesRegistered = [];
        public static ModuleServiceBuilder Build(Action<IModuleRegister>? builderConfig = null)
        {
            var builder = new ModuleServiceBuilder();
            builder.RegisterDefaultModules();
            builderConfig?.Invoke(builder);
            return builder;
        }
        private void RegisterDefaultModules()
        {
            throw new NotImplementedException();
        }

        #region IServiceProvider implementations
        public object? GetService(Type serviceType)
        {
            if (_instancesCached.TryGetValue(serviceType, out var instance))
                return instance;
            if (!_servicesRegistered.TryGetValue(serviceType, out var registeredType))
                return null;
            instance = CreateInstance(registeredType);
            if (instance is not null)
                _instancesCached.TryAdd(serviceType, instance);
            return instance;
        }
        private object? CreateInstance(Type type)
        {
            var resolvingStack = new Stack<Type>();
            return CreateInstanceInternal(type, resolvingStack);
        }
        private object? CreateInstanceInternal(Type type, Stack<Type> resolvingStack)
        {
            resolvingStack.Push(type);
            try
            {
                var constructors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length);
                foreach (var ctor in constructors)
                {
                    var parameters = ctor.GetParameters();
                    var args = new object?[parameters.Length];
                    bool canConstruct = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        if (parameter.HasDefaultValue)
                        {
                            args[i] = parameter.DefaultValue;
                            continue;
                        }
                        var paramType = parameters[i].ParameterType;
                        args[i] = ResolveType(paramType, resolvingStack);
                        if (args[i] == null && !parameters[i].IsOptional)
                        {
                            canConstruct = false;
                            break;
                        }
                    }

                    if (canConstruct)
                    {
                        // 尝试创建实例
                        try
                        {
                            return ctor.Invoke(args);
                        }
                        catch
                        {
                            // 构造失败，继续尝试下一个构造函数
                            continue;
                        }
                    }
                }
                // 所有构造函数均无法解析
                return null;
            }
            finally
            {
                resolvingStack.Pop();
            }
            object? ResolveType(Type type, Stack<Type> resolvingStack)
            {
                if (_instancesCached.TryGetValue(type, out var instance))
                    return instance;
                if (!_servicesRegistered.TryGetValue(type, out var implementationType))
                    return null; // 未注册
                if (resolvingStack.Contains(implementationType))
                    throw new InvalidOperationException($"Circular dependency detected for type '{implementationType.Name}'.");
                instance = CreateInstanceInternal(implementationType, resolvingStack);
                if (instance is not null)
                    _instancesCached.TryAdd(type, instance);
                return instance;
            }
        }
        #endregion

        #region IModuleRegister implementations
        public void RegisterModule<TService, TImplementation>(TImplementation? instance = null)
            where TImplementation : class, TService
            where TService : notnull
        {
            var serviceType = typeof(TService);
            _instancesCached.Remove(serviceType, out _);
            if (instance is null)
            {
                var implementationType = typeof(TImplementation);
                if(implementationType.IsAbstract)
                    throw new InvalidOperationException($"Cannot register abstract type {implementationType.Name}.");
                _servicesRegistered[serviceType] = implementationType;
            }
            else
                _instancesCached[serviceType] = instance;
        }
        #endregion
    }

    private static IServiceProvider? _serviceProvider;

    public static void Initialize(Action<IModuleRegister>? builderConfig = null)
        => _serviceProvider ??= ModuleServiceBuilder.Build(builderConfig);
    public static void Initialize(IServiceProvider externalProvider)
        => _serviceProvider ??= externalProvider;

    public static TService Resolve<TService>() where TService : class
    {
        if (_serviceProvider is null) Initialize();
        var service = _serviceProvider?.GetService(typeof(TService));
        return service as TService 
            ?? throw new InvalidOperationException($"Service {typeof(TService).Name} not registered or failed resolve in service provider {_serviceProvider}.");
    }
}
