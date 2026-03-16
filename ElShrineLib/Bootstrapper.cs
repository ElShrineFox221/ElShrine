using ElShrine.Commands;
using ElShrine.Modules;
using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ElShrine.Modules.Option;
using ElShrine.Modules.Command;

namespace ElShrine;

[Obsolete("Use MBootstrapper instead")]
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
    private static ILogger? session;
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
        var scope = session!.GetCurrentScopeAccessor();
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
    void RegisterModule<TService>(TService? instance = null) 
        where TService : class;
}
public interface IServiceContainer : IServiceProvider
{
    bool AddService(Type serviceType);
    object GetService(Type serviceType, bool cache);
    bool RemoveService(Type serviceType);
}
public static class MBootstrapper
{
    private static IServiceProvider? _serviceProvider;
    //

    private static ILogManager? _loggerManager;
    private static ILogger? _logger;

    public static void Initialize(Action<IModuleRegister>? builderConfig = null)
    {
        _serviceProvider ??= ModuleServiceBuilder.Build(builderConfig);
        FinalizeInitialization();
    }
    public static void Initialize(IServiceProvider externalProvider)
    {
        _serviceProvider ??= externalProvider;
        FinalizeInitialization();
    }
    private static void FinalizeInitialization()
    {
        CoreModuleAccessor.Initialize();
        _loggerManager = CoreModuleAccessor.Log; 
        _logger = _loggerManager.Main;
        _ = CoreModuleAccessor.Plugin;
        _ = CoreModuleAccessor.Option;
        _ = CoreModuleAccessor.ParamParser;
        _ = CoreModuleAccessor.Command;
    }

    #region Resolve
    public static TService Resolve<TService>() where TService : class
        => (TService)ResolveInternal(typeof(TService), disposeWhenExit: true);
    public static object Resolve(Type intanceType, bool disposeWhenExit)
        => ResolveInternal(intanceType, disposeWhenExit);
    private static object ResolveInternal(Type serviceType, bool disposeWhenExit)
    {
        if (_serviceProvider is null) Initialize();
        var service = _serviceProvider?.GetService(serviceType);
        if (service is null || !service.GetType().IsAssignableTo(serviceType)) 
            throw new InvalidOperationException($"Service {serviceType.Name} not registered or failed resolve in service provider {_serviceProvider}.");
        if (disposeWhenExit)
        {
            _cachedServices.RemoveWhere(static i => !i.TryGetTarget(out _));
            if (!_cachedServices.Any(i => i.TryGetTarget(out var v) && v == service))
                _cachedServices.Add(new WeakReference<object>(service!));
        }
        return service;
    }
    #endregion

    public static TInstance InstanceConstructorInvoker<TInstance>(
        IDictionary<Type, object> cachedInstances,
        IReadOnlyDictionary<Type, Type>? typeMap = null, 
        bool cacheUnregistered = true)
        => (TInstance)InstanceConstructorInvoker(typeof(TInstance), cachedInstances, typeMap, cacheUnregistered);
    public static object InstanceConstructorInvoker(
        Type instanceType,
        IDictionary<Type, object> cachedInstances,
        IReadOnlyDictionary<Type, Type>? typeMap = null, 
        bool cacheUnregistered = true)
    {
        var resolvingStack = new Stack<Type>();
        object Resolve(Type type)
        {
            if (cachedInstances.TryGetValue(type, out var cached))
                return cached;
            if (resolvingStack.Contains(type))
                throw new InvalidOperationException($"Circular dependency detected for type '{type.Name}'.");
            if (typeMap is null || !typeMap.TryGetValue(type, out var implementationType))
                implementationType = type;
            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new InvalidOperationException($"Cannot instantiate abstract type or interface '{implementationType.Name}'.");
            resolvingStack.Push(implementationType);
            try
            {
                var constructors = implementationType.GetConstructors().OrderByDescending(c => c.GetParameters().Length);
                foreach (var ctor in constructors)
                {
                    try
                    {
                        var parameters = ctor.GetParameters();
                        var args = new object?[parameters.Length];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var param = parameters[i];
                            if (param.HasDefaultValue)
                            {
                                args[i] = param.DefaultValue;
                                continue;
                            }
                            var paramType = param.ParameterType;
                            try
                            {
                                args[i] = Resolve(paramType);
                            }
                            catch
                            {
                                args[i] = null;
                            }
                        }
                        var instance = Activator.CreateInstance(implementationType, args)!;
                        if ((typeMap?.ContainsKey(type) ?? false) || cacheUnregistered)
                            cachedInstances[type] = instance;
                        return instance;
                    }
                    catch { }
                }
                throw new InvalidOperationException($"No suitable constructor found for {implementationType.Name}");
            }
            finally
            {
                resolvingStack.Pop();
            }
        }
        return Resolve(instanceType);
    }
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
            RegisterModule<ILogWriter, LogWriter>();
            RegisterModule<ILogManager, LogManager>();
            RegisterModule<IPluginManager, PluginManager>();
            RegisterModule<IOptionManager, OptionManager>();
            RegisterModule<IParamParserManager, ParamParserManager>();
            RegisterModule<ICommandManager, CommandsManager>();
            //old modules
            RegisterModule<BeatTimer>();
            RegisterModule<LocalizationManager>();
        }

        #region IServiceProvider implementations
        public object? GetService(Type serviceType)
        {
            return InstanceConstructorInvoker(serviceType, _instancesCached, _servicesRegistered, false);
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
                if (implementationType.IsAbstract)
                    throw new InvalidOperationException($"Cannot register abstract type {implementationType.Name}.");
                _servicesRegistered[serviceType] = implementationType;
            }
            else
                _instancesCached[serviceType] = instance;
        }
        public void RegisterModule<TService>(TService? instance = null)
            where TService : class
        {
            RegisterModule<TService, TService>(instance);
        }
        #endregion
    }

    #region Manage instances
    private readonly static HashSet<WeakReference<object>> _cachedServices = [];
    public static void Exit(bool force)
    {
        if (!force && _logger is not null)
        {
            _logger.Warning(new ProcessException("The process will be closed in 3 seconds if there are no more waiting tasks."));
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                while (true)
                {
                    if (!force && !_loggerManager!.TemporarilyNoEntriesToUpdate)
                        await Task.Yield();
                    foreach (var item in _cachedServices)
                    {
                        if (item.TryGetTarget(out var obj) && obj is IDisposable disposable)
                            disposable.Dispose();
                    }
                    LogCommands.Reconstruct();
                    Environment.Exit(0);
                }
            });
        }
    }
    #endregion
}
