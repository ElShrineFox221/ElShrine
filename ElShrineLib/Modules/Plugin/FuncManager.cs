using ElShrine.Modules.Log;
using System.Reflection;
using System.Runtime.Loader;

namespace ElShrine.Modules.Plugin;

internal sealed class PluginManagerF : IPluginManager
{
    private sealed class PluginLoadContext(string folder) : AssemblyLoadContext(folder, true)
    {
        public int RefCount = 0;
    }

    private const string PLUGIN_FOLDER = "Plugins";
    private readonly ILogger _logger;
    private readonly Dictionary<string, PluginInfo> _availablePluginInfos; // by id
    private readonly Dictionary<string, IPlugin> _loadedPlugins; // by id
    private readonly Dictionary<string, PluginLoadContext> _loadedContexts; // by folder

    public IReadOnlyList<PluginInfo> UnloadPlugins 
        => [.. _availablePluginInfos.Where(kv => !_loadedPlugins.ContainsKey(kv.Key)).Select(kv => kv.Value)];
    public IReadOnlyDictionary<PluginInfo, IPlugin> LoadedPlugins
        => _loadedPlugins.ToDictionary(kv => _availablePluginInfos[kv.Key], kv => kv.Value);

    public event PluginLoadedHandler? PluginLoaded;
    public event PluginPreUnloadHandler? PluginPreUnload;
    public event PluginUnloadedHandler? PluginUnloaded;

    

    public void RescanPluginInfos()
    {
        _availablePluginInfos.Clear();
        Directory.CreateDirectory(PLUGIN_FOLDER);
        var folders = Directory.GetDirectories(PLUGIN_FOLDER);
        foreach (var folder in folders)
        {
            var ctx = LoadPluginAllReferences(folder, out var combinedHash);
            // do validate
            if (DoValidate(ctx))
            {
                // do scan
                var pts = ctx.GetImplements(typeof(IPlugin));
                var infos = pts.Where(pt => !pt.IsAbstract).Select(pt =>
                {
                    var attr = pt.GetCustomAttribute<PluginAttribute>();
                    return new PluginInfo(
                        Id: $"{folder}_{pt.Name}",
                        PluginFullName: pt.FullName!,
                        Folder: folder,
                        FileHash: combinedHash,
                        Name: attr?.Name ?? pt.Name,
                        Author: attr?.Author ?? string.Empty,
                        VersionInfo: attr?.Version ?? string.Empty,
                        Description: attr?.Description ?? string.Empty
                        );
                });
                foreach (var info in infos)
                    _availablePluginInfos.Add(info.Id, info);
            }
            ctx.Unload();
        }
    }
    public IPlugin LoadPlugin(PluginInfo info)
    {
        if(_loadedPlugins.TryGetValue(info.Id, out var plugin))
            return plugin;
        if (!_availablePluginInfos.TryGetValue(info.Id, out var pluginInfo))
            throw new PluginException($"Plugin {info.PluginFullName} is not found.");
        PluginLoadContext? ctx = null;
        try
        {
            if (!_loadedContexts.TryGetValue(pluginInfo.Folder, out ctx))
            {
                var newCtx = LoadPluginAllReferences(pluginInfo.Folder, out var combinedHash);
                if (pluginInfo.FileHash != combinedHash)
                    throw new PluginException($"Plugin {info.PluginFullName} is modified.");
                ctx = _loadedContexts[pluginInfo.Folder] = newCtx;
            }
            //
            var type = ctx.GetImplements(typeof(IPlugin)).Where(t => t.FullName == info.PluginFullName).FirstOrDefault()
                ?? throw new PluginException($"Plugin {info.Name} entry point type {info.PluginFullName} is not found.");
            ctx.RefCount++;
            plugin = (IPlugin)Activator.CreateInstance(type)!;
            _loadedPlugins[info.Id] = plugin;
            plugin.PostLoad(AssemblyLoadContext.Default, ctx);
            PluginLoaded?.Invoke(plugin, info);
        }
        finally
        {
            if (plugin is null || (ctx is not null && ctx.RefCount == 0)) 
                ctx?.Unload();
        }
        return plugin;
    }
    public bool UnloadPlugin(PluginInfo info)
    {
        if (!_loadedPlugins.Remove(info.Id, out var plugin)) 
            return false;
        PluginPreUnload?.Invoke(plugin, info);
        
        if (_loadedContexts.TryGetValue(info.Folder, out var ctx))
        {
            ctx.RefCount--;
            if (ctx.RefCount == 0)
            {
                _loadedContexts.Remove(info.Folder);
                ctx.Unload();
            }
            plugin.PreUnload(AssemblyLoadContext.Default, ctx);
        }
        else plugin.PreUnload(AssemblyLoadContext.Default, AssemblyLoadContext.Default);
        PluginUnloaded?.Invoke(info);
        return true;
    }



    public PluginManagerF(ILoggerManager log)
    {
        _logger = log.Main;
        _availablePluginInfos = [];
        _loadedPlugins = [];
        _loadedContexts = [];
        // load all main references;
        var ctx = AssemblyLoadContext.Default;
        var names = ctx.Assemblies.Select(asm => asm.GetName());
        LoadAllReferences(ctx, names);
        if (!DoValidate(ctx))
            _logger.Error($"{nameof(PluginManagerF)} initialize failed.");
    }


    private PluginLoadContext LoadPluginAllReferences(string folder, out string combinedHash)
    {
        var dllFiles = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
        dllFiles.Sort();
        combinedHash = dllFiles.BuildString(GetFileHash, split: string.Empty);
        var ctx = new PluginLoadContext(folder);
        var names = dllFiles.Select(AssemblyName.GetAssemblyName);
        LoadAllReferences(ctx, names);
        return ctx;
    }

    private Assembly? LoadAssemblyAndLog(AssemblyLoadContext ctx, AssemblyName name)
    {
        var logger = _logger;
        if (ctx.Assemblies.FirstOrDefault(asm => AssemblyName.ReferenceMatchesDefinition(asm.GetName(), name)) is Assembly asm)
            return asm;
        try
        {
            asm = ctx.LoadFromAssemblyName(name);
            logger.Log(LogItem.Header("Success", LogItemStyle.Success), LogItem.Normal($" Loaded assembly {asm.Location}."));
            return asm;
        }
        catch (Exception e)
        {
            logger.Error(e);
            return null;
        }
    }
    private void LoadAllReferences(AssemblyLoadContext ctx, IEnumerable<AssemblyName> roots)
    {
        var logger = _logger;
        using var sc = logger.OpenScope("Loading references...");
        foreach(var name in roots)
        {
            var asm = LoadAssemblyAndLog(ctx, name);
            if (asm is null) 
                continue;
            LoadAllReferenceAssembliesInternal(ctx, asm);
        }
        void LoadAllReferenceAssembliesInternal(AssemblyLoadContext ctx, Assembly assembly)
        {
            var refs = assembly.GetReferencedAssemblies();
            foreach (AssemblyName refAssembly in refs)
            {
                if (ctx.Assemblies.Any(a => a.GetName() == refAssembly))
                    continue;
                try
                {
                    var newLoadedAssembly = LoadAssemblyAndLog(ctx, refAssembly);
                    if (newLoadedAssembly is null)
                        continue;
                    LoadAllReferenceAssembliesInternal(ctx, newLoadedAssembly);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }
        logger.ConfigEnd($"Loaded {GetAssembliesText(ctx.Assemblies.Count())}.");
    }
    private bool DoValidate(AssemblyLoadContext ctx)
    {
        var logger = _logger;
        var ctxName = ctx == AssemblyLoadContext.Default ? "Default" : (ctx.Name ?? "Unknown");
        using var sc = logger.OpenScope(OmitOrExecutingPattern("Scanning assembly context ", ctxName, LogItemStyle.NoticeCyan));
        try
        {
            var attributedTypes = ctx.GetClassesByAttribute<ValidatableBaseAttribute>(true).ToList();
            logger.Log($"Found {"type".GetPuralWithNum(attributedTypes.Count)} to be validated.");
            foreach (var (type, attrs) in attributedTypes)
            {
                foreach (var attr in attrs)
                {
                    try
                    {
                        if (!attr.DoValidate(type, null))
                        {
                            var e = new ValidationFailedException(attr.ValidateFailedReason ?? string.Empty);
                            logger.Error(e);
                        }
                        else
                        {
                            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                            foreach (var member in members)
                            {
                                var memberAttrs = member.GetCustomAttributes<ValidatableBaseAttribute>(true);
                                foreach (var mAttr in memberAttrs)
                                {
                                    if (!mAttr.DoValidate(typeof(MemberInfo), member))
                                    {
                                        var e = new ValidationFailedException(mAttr.ValidateFailedReason ?? string.Empty);
                                        logger.Error(e);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
        finally
        {
            logger.ConfigEnd($"Validation process completed.");
        }
        var suc = sc.Errors.Count == 0;
        return suc;
    }
    private static string GetFileHash(string path)
    {
        throw new NotImplementedException();
    }
    private static LogItem[] OmitOrExecutingPattern(string text0, string text1, LogItemStyle text1Style, LogItemStyle text0Style = LogItemStyle.Info)
        => [LogItem.Normal(text0, text0Style), LogItem.Normal(text1, text1Style), LogItem.Normal("...", text0Style)];
    private static string GetAssembliesText(int count) => nameof(Assembly).GetPuralWithNum(count).ToLower();
}
