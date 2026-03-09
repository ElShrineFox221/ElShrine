using ElShrine.Modules.Log;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;

namespace ElShrine.Modules.Plugin;

internal sealed class PluginManager(ILoggerManager log) : IPluginManager
{
    public sealed class PluginContainer(string folder)
    {
        private readonly Dictionary<string, PluginInfo> _plugins = [];
        private readonly Dictionary<string, IPlugin> _loadedPlugins = [];

        public readonly string Folder = folder;
        public AssemblyLoadContext? Context { get; private set; }
        public bool IsLoaded => Context is not null;
        public string FilesCombinedHash { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, PluginInfo> Plugins => _plugins;

        public IReadOnlyList<PluginInfo> ScanPlugins(ILogger logger, bool releaseCtx)
        {
            if (IsLoaded)
                return [.._plugins.Values];
            // get files & validate files hash
            var folder = Folder;
            var dllFiles = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
            dllFiles.Sort();
            FilesCombinedHash = dllFiles.BuildString(GetFileHash, split: string.Empty);
            // create ctx & load all reference + files
            var ctx = Context = new AssemblyLoadContext(Folder, true);
            foreach (var dllFile in dllFiles)
            {
                if (ctx.Assemblies.Any(a => a.Location == dllFile))
                    continue;
                if (ctx != AssemblyLoadContext.Default && AssemblyLoadContext.Default.Assemblies.Any(a => a.Location == dllFile))
                    continue;
                var asb = LoadAssemblyAndLog(ctx, dllFile, logger);
                if (asb is not null)
                    LoadAllReferenceAssemblies(ctx, asb, logger);
            }
            // scan plugins
            _plugins.Clear();
            var pluginTypes = ctx.GetImplements(typeof(IPlugin));
            foreach (var pluginType in pluginTypes)
            {
                if(pluginType.IsAbstract)
                    continue;
                var attr = pluginType.GetCustomAttribute<PluginAttribute>();
                var info = new PluginInfo(
                    Id: $"{folder}_{pluginType.Name}",
                    PluginFullName: pluginType.FullName!,
                    Folder: folder,
                    FileHash: FilesCombinedHash,
                    Name: attr?.Name ?? pluginType.Name,
                    Author: attr?.Author ?? string.Empty,
                    VersionInfo: attr?.Version ?? string.Empty,
                    Description: attr?.Description ?? string.Empty
                    );
                _plugins.Add(info.Id, info);
            }
            // release context
            if (releaseCtx)
            {
                ctx.Unload();
                Context = null;
            }
            return [.. _plugins.Values];
        }

        public bool TryLoadPlugin(PluginInfo pluginInfo, ILogger logger, out IPlugin? plugin)
        {
            plugin = null;
            if (!IsLoaded)
                ScanPlugins(logger, false);
            // check already loaded
            if (_loadedPlugins.TryGetValue(pluginInfo.Id, out plugin)) 
                return true;
            // load plugin
            if(!_plugins.TryGetValue(pluginInfo.Id, out var info))
            {
                logger.Error($"Plugin {pluginInfo.Name} is not found.");
                return false;
            }
            if(info.FileHash != pluginInfo.FileHash)
                logger.Warning($"Plugin {pluginInfo.Name} file hash is not matched.");
            var type = Context.GetImplements(typeof(IPlugin)).Where(t => t.FullName == pluginInfo.PluginFullName).FirstOrDefault();
            if (type is null)
            {
                logger.Error($"Plugin {pluginInfo.Name} entry point type {pluginInfo.PluginFullName} is not found.");
                return false;
            }
            plugin = (IPlugin)Activator.CreateInstance(type)!;
            plugin.PostLoad(AssemblyLoadContext.Default, Context!);
            _loadedPlugins.Add(pluginInfo.Id, plugin);
            return true;
        }
        public bool TryUnloadPlugin(PluginInfo pluginInfo)
        {
            if(!IsLoaded || Context is null) 
                return false;
            var id = pluginInfo.Id;
            if (_loadedPlugins.Remove(id, out var plugin))
            {
                plugin.PreUnload(AssemblyLoadContext.Default, Context);
                if (_loadedPlugins.Count == 0)
                {
                    Context.Unload();
                    Context = null;
                }
                return true;
            }
            return false;
        }

        #region helpers
        private static string GetFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "");
        }
        #endregion
    }

    #region Services
    private readonly ILogger _logger = log.Main;

    #endregion

    #region Plugin infos
    private const string BasePath = "Plugins";
    private readonly List<PluginInfo> _availableInfos = [];
    private readonly Dictionary<PluginInfo, IPlugin> _loadedPlugins = [];
    private readonly Dictionary<string, PluginContainer> _folderContainers = [];

    public IReadOnlyList<PluginInfo> UnloadPlugins => [.. _availableInfos.Except(_loadedPlugins.Keys)];
    public IReadOnlyDictionary<PluginInfo, IPlugin> LoadedPlugins => _loadedPlugins;

    public event PluginLoadedHandler? PluginLoaded;
    public event PluginPreUnloadHandler? PluginPreUnload;
    public event PluginUnloadedHandler? PluginUnloaded;
    #endregion

    #region load & unload plugin
    public IPlugin LoadPlugin(PluginInfo info)
    {
        throw new NotImplementedException();
    }
    public bool UnloadPlugin(PluginInfo info)
    {
        if (!_folderContainers.TryGetValue(info.Folder, out var container)) 
            return false;
        if (_loadedPlugins.Remove(info, out var plugin))
            PluginPreUnload?.Invoke(plugin, info);
        var suc = container.TryUnloadPlugin(info);
        if (suc)
            PluginUnloaded?.Invoke(info);
        return suc;
    }
    #endregion

    public void RescanPluginInfos()
    {
        _logger.OpenScope(OmitOrExecutingPattern("Rescan plugins from folder ", BasePath, LogItemStyle.NoticeBlue));
        if (!Directory.Exists(BasePath)) 
            Directory.CreateDirectory(BasePath);
        var folders = Directory.GetDirectories(BasePath);
        var newsCount = 0;
        var newPlugins = new Dictionary<string, List<PluginInfo>>();
        foreach (var folder in folders)
        {
            if(_folderContainers.TryGetValue(folder, out var container) && container.IsLoaded)
            {
                newPlugins[folder] = [.. container.Plugins.Values];
                continue;
            }
            _folderContainers[folder] = container = new PluginContainer(folder);
            var plugins = container.ScanPlugins(_logger, true).ToList();
            newPlugins.TryAdd(folder, plugins);
            newsCount++;
        }
        _logger.Log($"Loaded {"plugins package".GetPuralWithNum(newsCount)}.");
        //Remove old
        var removeKeys = _folderContainers.Where(kv => !kv.Value.IsLoaded).Select(kv => kv.Key).ToList();
        var count = 0;
        foreach (var key in removeKeys)
        {
            _folderContainers.Remove(key, out _);
            count++;
        }
        _logger.Log($"Cleaned cache of {"plugins package".GetPuralWithNum(count)}.");
    }

    private static Assembly? LoadAssemblyAndLog(AssemblyLoadContext ctx, string path, ILogger logger)
    {
        try
        {
            var asb = ctx.LoadFromAssemblyPath(path);
            logger.Log(LogItem.Header("Success", LogItemStyle.Success), LogItem.Normal($" Loaded assembly {path}."));
            return asb;
        }
        catch (Exception e)
        {
            logger.Error(e);
            return null;
        }
    }
    private static Assembly? LoadAssemblyAndLog(AssemblyLoadContext ctx, AssemblyName name, ILogger logger)
    {
        try
        {
            var asb = ctx.LoadFromAssemblyName(name);
            logger.Log(LogItem.Header("Success", LogItemStyle.Success), LogItem.Normal($" Loaded assembly {asb.Location}."));
            return asb;
        }
        catch (Exception e)
        {
            logger.Error(e);
            return null;
        }
    }
    private static void LoadAllReferenceAssemblies(AssemblyLoadContext ctx, Assembly assembly, ILogger logger)
    {
        using var sc = logger.OpenScope("Loading references...");
        LoadAllReferenceAssembliesInternal(ctx, assembly);
        void LoadAllReferenceAssembliesInternal(AssemblyLoadContext ctx, Assembly assembly)
        {
            var refs = assembly.GetReferencedAssemblies();
            foreach (AssemblyName refAssembly in refs)
            {
                if (ctx.Assemblies.Any(a => a.GetName() == refAssembly))
                    continue;
                try
                {
                    var newLoadedAssembly = LoadAssemblyAndLog(ctx, refAssembly, logger);
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
    
    private static bool DoValidate(AssemblyLoadContext? ctx, ILogger logger)
    {
        ctx ??= AssemblyLoadContext.Default;
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

    #region helpers
    private static LogItem[] OmitOrExecutingPattern(string text0, string text1, LogItemStyle text1Style, LogItemStyle text0Style = LogItemStyle.Info)
        => [LogItem.Normal(text0, text0Style), LogItem.Normal(text1, text1Style), LogItem.Normal("...", text0Style)];
    private static string GetAssembliesText(int count) => nameof(Assembly).GetPuralWithNum(count).ToLower();
    #endregion
}