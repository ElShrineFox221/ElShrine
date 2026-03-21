using ElShrine.Commands;
using ElShrine.Modules;
using System.Collections.Concurrent;
using System.Reflection;

namespace ElShrine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InitializationInfoAttribute : ValidatableClassAttribute
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
    bool RegisterModule<TService, TImplementation>(TImplementation? instance = null, bool overrides = true)
        where TImplementation : class, TService
        where TService : notnull;
    bool RegisterModule<TService>(TService? instance = null, bool overrides = true) 
        where TService : class;
    void FinalizeRegistration();
}
public static class Bootstrapper
{
    private static IServiceProvider? _serviceProvider;
    private static IModuleRegister? _moduleRegister;
    //
    private readonly static ConcurrentBag<Finalizer> _finalizers = [];
    private sealed record Finalizer(int Priority, Action Action);
    public static void RegisterFinalization(Action action, int priority = -1)
        => _finalizers.Add(new(priority, action));

    public static void Initialize(Action<IModuleRegister>? builderConfig = null)
    {
        if(_serviceProvider is null || _moduleRegister is null)
        {
            var defaultBuilder = new ModuleServiceBuilder();
            _moduleRegister = defaultBuilder;
            _serviceProvider = defaultBuilder;
        }
        builderConfig?.Invoke(_moduleRegister);
        FinalizeInitialization();
    }
    public static bool Initialize(IServiceProvider externalProvider, IModuleRegister moduleRegister)
    {
        var suc = _serviceProvider is null || _moduleRegister is null;
        if (suc)
        {
            _serviceProvider = externalProvider;
            _moduleRegister = moduleRegister;
            FinalizeInitialization();
        }
        return suc;
    }

    private static bool _finalizedInitialization = false;
    private static void FinalizeInitialization()
    {
        if (_finalizedInitialization) 
            return;
        _finalizedInitialization = true;
        _moduleRegister?.FinalizeRegistration();
        foreach (var f in _finalizers.OrderByDescending(static f => f.Priority))
            f.Action.Invoke();
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

    #region InstanceConstructor
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
    #endregion
    private sealed class ModuleServiceBuilder : IServiceProvider, IModuleRegister
    {
        private readonly ConcurrentDictionary<Type, object> _instancesCached = [];
        private readonly ConcurrentDictionary<Type, Type> _servicesRegistered = [];

        

        #region IServiceProvider implementations
        public object? GetService(Type serviceType)
        {
            return InstanceConstructorInvoker(serviceType, _instancesCached, _servicesRegistered, false);
        }
        #endregion

        #region IModuleRegister implementations
        public bool RegisterModule<TService, TImplementation>(TImplementation? instance = null, bool overrides = false)
            where TImplementation : class, TService
            where TService : notnull
        {
            var serviceType = typeof(TService);
            if (!overrides && (_servicesRegistered.ContainsKey(serviceType) || _instancesCached.ContainsKey(serviceType)))
                return false;
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
            return true;
        }
        public bool RegisterModule<TService>(TService? instance = null, bool overrides = false)
            where TService : class
            => RegisterModule<TService, TService>(instance, overrides);
        public void FinalizeRegistration()
        {
            foreach(var serviceType in _servicesRegistered.Keys)
            {
                if (_instancesCached.ContainsKey(serviceType))
                    continue;
                var attr = serviceType.GetCustomAttribute<InitializationInfoAttribute>(true);
                if (attr is not null && !attr.PreInstantiate)
                    continue;
                else _ = Resolve(serviceType, disposeWhenExit: true);
            }
        }
        #endregion
    }

    #region Manage instances
    private readonly static HashSet<WeakReference<object>> _cachedServices = [];
    public static void Exit(bool force)
    {
        var log = CoreModuleAccessor.Log;
        var logger = log.Main;
        if (!force && logger is not null)
        {
            logger.Warning(new ProcessException("The process will be closed in 3 seconds if there are no more waiting tasks."));
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                while (true)
                {
                    if (!force && !log!.TemporarilyNoEntriesToUpdate)
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
