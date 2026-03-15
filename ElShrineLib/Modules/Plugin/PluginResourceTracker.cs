using System.Collections.Concurrent;
using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

public abstract class PluginResourceTracker<TResourceBase> : PluginAwareServiceBase 
    where TResourceBase : class
{
    protected readonly ConcurrentDictionary<AssemblyLoadContext, ConcurrentDictionary<Type, TResourceBase>> Resources = [];

    public PluginResourceTracker(IPluginManager plugin) : base(plugin)
    {
        if (!typeof(TResourceBase).IsAbstract) 
            throw new InvalidOperationException($"{nameof(TResourceBase)} must be abstract.");
    }

    protected virtual void CollectResources(AssemblyLoadContext ctx)
    {
        var contextResources = Resources.GetOrAdd(ctx, ctx => new ConcurrentDictionary<Type, TResourceBase>());
        var types = ctx.GetImplements(typeof(TResourceBase)).Where(t =>
        {
            if (t.IsAbstract)
                return false;
            var parentType = t.BaseType!;
            while (parentType.IsAbstract)
            {
                if (parentType == typeof(TResourceBase)) return true;
                parentType = parentType.BaseType!;
            }
            return false;
        });
        foreach (var type in types)
        {
            if (contextResources.ContainsKey(type)) 
                continue;
            if (MBootstrapper.Resolve(type, disposeWhenExit: true) is TResourceBase instance && contextResources.TryAdd(type, instance))
                OnResourceCreated(instance, ctx);
        }
    }
    protected override void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx)
    {
        if (unloadingCtx && Resources.TryRemove(ctx, out var contextResources))
        {
            foreach (var resource in contextResources.Values)
                OnResourceReleasing(resource, ctx);
            contextResources.Clear(); 
        }
    }
    protected override void OnPluginLoaded(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedCtx)
    {
        CollectResources(AssemblyLoadContext.Default);
        if (ctx != AssemblyLoadContext.Default)
            CollectResources(ctx);
    }
    protected virtual void OnResourceCreated(TResourceBase resource, AssemblyLoadContext ctx) { }
    protected virtual void OnResourceReleasing(TResourceBase resource, AssemblyLoadContext ctx) { }
}