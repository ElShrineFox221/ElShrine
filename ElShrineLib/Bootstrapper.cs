using ElShrine.Commands;
using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Localization;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Plugin;
using System.Collections.Concurrent;

namespace ElShrine;

[Obsolete("Use MBootstrapper initialize instead")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InitializationInfoAttribute : ValidatableClassAttribute
{
    public bool PreInstantiate = true;

    protected override bool ValidateType(Type typeToValidate)
    {
        if (!typeToValidate.IsAbstract) 
            return true;
        ValidateFailedReason = $"Type <{typeToValidate.FullName}> is abstract.";
        return false;
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
        _ = CoreModuleAccessor.Localization;
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
            RegisterModule<ILocalizationManager, LocalizationManager>();
            RegisterModule<IPluginManager, PluginManager>();
            RegisterModule<IOptionManager, OptionManager>();
            RegisterModule<IParamParserManager, ParamParserManager>();
            RegisterModule<ICommandManager, CommandsManager>();
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
                    var logDir = CoreModuleAccessor.LogWriter.LogCurrentDirectory;
                    foreach (var item in _cachedServices)
                    {
                        if (item.TryGetTarget(out var obj) && obj is IDisposable disposable)
                            disposable.Dispose();
                    }
                    LogCommands.ReconstructInternal(logDir, false);
                    Environment.Exit(0);
                }
            });
        }
    }
    #endregion
}
