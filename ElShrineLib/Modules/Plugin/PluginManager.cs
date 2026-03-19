using ElShrine.Modules.Log;
using ElShrine.Options;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;

namespace ElShrine.Modules.Plugin;

internal sealed class PluginManager : IPluginManager
{
    private sealed class PluginLoadContext(string folder) : AssemblyLoadContext(folder, true)
    {
        private readonly AssemblyDependencyResolver _resolver = new(folder);
        public readonly string Folder = folder;
        public int RefCount = 0;
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // get path by resolver
            var p = _resolver.ResolveAssemblyToPath(assemblyName);
            if (p is not null)
            {
                try
                {
                    var asm = LoadFromAssemblyPath(p);
                    return asm;
                }
                catch { }
            }
            // try load from default context
            try
            {
                var asm = Default.LoadFromAssemblyName(assemblyName);
                if (asm is not null)
                    return asm;
            }
            catch { }
            try
            {
                var path = Path.Combine(Folder, assemblyName.Name + ".dll");
                path = Path.GetFullPath(path);
                var asm = LoadFromAssemblyPath(path);
                return asm;
            }
            catch { }
            return null;
        }
    }

    private const string PLUGIN_FOLDER = "Plugins";
    private readonly ILogger _logger;
    private readonly Dictionary<string, PluginInfo> _availablePluginInfos; // by id
    private readonly Dictionary<string, IPlugin> _loadedPlugins; // by id
    private readonly Dictionary<string, PluginLoadContext> _loadedContexts; // by folder

    public IReadOnlyList<PluginInfo> AvailablePlugins
        => [.. _availablePluginInfos.Values];
    public IReadOnlyDictionary<PluginInfo, IPlugin> LoadedPlugins
        => _loadedPlugins.ToDictionary(kv => _availablePluginInfos[kv.Key], kv => kv.Value);

    public event PluginLoadedHandler? PluginLoaded;
    public event PluginUnloadingHandler? PluginUnloading;
    public event PluginUnloadedHandler? PluginUnloaded;

    

    public void ScanPluginInfos()
    {
        _availablePluginInfos.Clear();
        Directory.CreateDirectory(PLUGIN_FOLDER);
        var folders = Directory.GetDirectories(PLUGIN_FOLDER);
        var folderValidCount = 0;
        var pluginValidCount = 0;
        foreach (var folder in folders)
        {
            var ctx = LoadPluginAllReferences(folder, out var combinedHash);
            // do validate
            if (DoValidate(ctx))
            {
                folderValidCount++;
                // do scan
                var pts = ctx.GetImplements(typeof(IPlugin));
                var infos = pts.Where(pt => !pt.IsAbstract).Select(pt =>
                {
                    pluginValidCount++;
                    var attr = pt.GetCustomAttribute<PluginAttribute>();
                    var info = attr is not null ? attr.DoSummarizePluginInfo(pt, folder, combinedHash)
                     : PluginAttribute.SummarizePluginInfoDefalut(null, pt, folder, combinedHash);
                    return info;
                });
                foreach (var info in infos)
                    _availablePluginInfos.Add(info.Id, info);
            }
            ctx.Unload();
        }
        var title = $"There are {"plugin".GetPuralWithNum(pluginValidCount)} in {"folder".GetPuralWithNum(folderValidCount)}.";
        var ec = PluginInfo.BuildPluginTable(LogItem.Normal(title), AvailablePlugins.Select((i, index) => (i, LoadedPlugins.ContainsKey(i), index)), true);
        _logger.Log(ec);
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
            var createNewCtx = !_loadedContexts.TryGetValue(pluginInfo.Folder, out ctx);
            if (createNewCtx)
            {
                var newCtx = LoadPluginAllReferences(pluginInfo.Folder, out var combinedHash);
                if (pluginInfo.FileHash != combinedHash)
                    throw new PluginException($"Plugin {info.PluginFullName} is modified.");
                ctx = _loadedContexts[pluginInfo.Folder] = newCtx;
            }
            //
            var type = ctx.GetImplements(typeof(IPlugin)).Where(t => t.FullName == info.PluginFullName).FirstOrDefault()
                ?? throw new PluginException($"Plugin {info.Name} entry point type {info.PluginFullName} is not found.");
            ctx!.RefCount++;
            plugin = (IPlugin)Bootstrapper.Resolve(type, disposeWhenExit: false);
            PluginLoaded?.Invoke(plugin, info, ctx, createNewCtx);
            _loadedPlugins[info.Id] = plugin;
            plugin.PostLoad(AssemblyLoadContext.Default, ctx);
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
        
        
        if (_loadedContexts.TryGetValue(info.Folder, out var ctx))
        {
            ctx.RefCount--;
            var unloadingCtx = ctx.RefCount == 0;
            if (unloadingCtx)
            {
                _loadedContexts.Remove(info.Folder);
                ctx.Unload();
            }
            plugin.PreUnload(AssemblyLoadContext.Default, ctx);
            PluginUnloading?.Invoke(plugin, info, ctx, unloadingCtx);
        }
        else
        {
            var _ctx = AssemblyLoadContext.Default;
            plugin.PreUnload(_ctx, _ctx);
            PluginUnloading?.Invoke(plugin, info, _ctx, false);
        }
        PluginUnloaded?.Invoke(info);

        return true;
    }

    public PluginManager(ILogManager log)
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
            _logger.Error($"{nameof(PluginManager)} initialize failed.");
        Bootstrapper.RegisterFinalization(() =>
        {
            using var sc = _logger.OpenScope("Try do plugins initial loading from config...");
            ScanPluginInfos();
            Load();
            sc.HandleErrors<Exception>((ex, r) => true);
        });
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

    private Assembly? LoadAssemblyAndLog(AssemblyLoadContext ctx, AssemblyName name, out bool isLoaded)
    {
        var logger = _logger;
        isLoaded = true;
        bool predict(Assembly asm)
            => AssemblyName.ReferenceMatchesDefinition(asm.GetName(), name);
        if (AssemblyLoadContext.Default.Assemblies.FirstOrDefault(predict) is Assembly asm_default)
            return asm_default;
        if (ctx != AssemblyLoadContext.Default && ctx.Assemblies.FirstOrDefault(predict) is Assembly asm)
            return asm;
        isLoaded = false;
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
            var asm = LoadAssemblyAndLog(ctx, name, out var isLoaded);
            if (asm is null || isLoaded) 
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
                    var newLoadedAssembly = LoadAssemblyAndLog(ctx, refAssembly, out var isLoaded);
                    if (newLoadedAssembly is null || isLoaded)
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
        var ctxName = ctx.GetContextName();
        using var sc = logger.OpenScope(EntryContent.OmitOrExecutingPattern("Scanning assembly context ", ctxName, LogItemStyle.NoticeCyan));
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
    private static string GetFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "");
    }
    private static string GetAssembliesText(int count) => nameof(Assembly).GetPuralWithNum(count).ToLower();

    #region persistence

    public void Save()
    {
        using var sc = _logger.OpenScope(EntryContent.OmitOrExecutingPattern("Saving ", nameof(PluginConfig), LogItemStyle.NoticeCyan));
        var r = PluginConfigWirter.Write(LoadedPlugins.Keys, AvailablePlugins.Except(LoadedPlugins.Keys));
        if (!r.Success)
            _logger.Error(r.FailedSource ?? new Exception());
    }
    public void Load()
    {
        using var sc = _logger.OpenScope(EntryContent.OmitOrExecutingPattern("Loading ", nameof(PluginConfig), LogItemStyle.NoticeCyan));
        
        var r = PluginConfigWirter.Read(AvailablePlugins, out var enableds, out var disableds);
        if (r.Success)
        {
            var loadeds = LoadedPlugins.Keys.ToList();
            if (loadeds.SequenceEqual(enableds))
            {
                _logger.Log(LogItem.Normal("Enabled plugins list from config has been loaded."));
                return;
            }
            using var sc1 = _logger.OpenScope($"Reloading {"enabled plugin".GetPuralWithNum(enableds.Count)}...");
            using (var _ = _logger.OpenScope("Unloading all plugins..."))
            {
                foreach (var info in loadeds)
                    UnloadPlugin(info);
            }
            using (var _ = _logger.OpenScope("Loading plugins..."))
            {
                foreach (var info in enableds)
                    LoadPlugin(info);
            }
            var title = LogItem.Normal($"Reloaded {"enabled plugin".GetPuralWithNum(enableds.Count)} from config.");
            var availables = AvailablePlugins.ToList();
            var ec = PluginInfo.BuildPluginTable(title, enableds.Select(i => (i, false, availables.IndexOf(i))), false);
            _logger.Log(ec);
        }
        else
            _logger.Error(r.FailedSource ?? new Exception());
    }
    #endregion
}
