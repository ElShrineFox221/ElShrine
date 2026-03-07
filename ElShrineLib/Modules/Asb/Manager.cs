using ElShrine.Modules.Log;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace ElShrine.Modules.Asb;

public interface IPugin
{
    void PostLoad(AssemblyLoadContext ctx);
    void PreUnload(AssemblyLoadContext ctx);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PuginAttribute : ValidatableClassAttribute
{
    public string Name = string.Empty;
    public string Description = string.Empty;
    public string Version = string.Empty;

    protected override bool ValidateType(Type typeToValidate)
    {
        var hasImplement = typeToValidate.IsImplementOf(typeof(IPugin));
        if (!hasImplement)
            ValidateFailedReason = $"{typeToValidate.FullName} is not implement of {nameof(IPugin)}.";
        return hasImplement;
    }
}

public record PuginInfo(string Name, string FullName, string VersionInfo, string Description, string Folder);

public sealed class NM
{
    public NM()
    {

    }
    /*#region Loading Logic
    public delegate void AssembliesUpdatedHandler(Assembly[] assemblies);
    public event AssembliesUpdatedHandler? AssembliesUpdated;


    private const string dllExtension = ".dll";
    public List<Assembly> LoadExtraAssemblies(string directory)
    {
        if (!Directory.Exists(directory)) return [];
        var files = Directory.GetFiles(directory).Where(f => Path.GetExtension(f).Equals(dllExtension, StringComparison.OrdinalIgnoreCase));
        return LoadExtraAssemblies(files);
    }
    public List<Assembly> LoadExtraAssemblies(IEnumerable<string> modulePaths)
    {
        using var scope = _logger.OpenScope("Loading extra assemblies...");
        var processedPaths = new List<LogItem>();
        var results = new List<LogItem>();
        var newLoaded = new List<Assembly>();
        var currentSet = new HashSet<Assembly>(assemblies);
        foreach (var path in modulePaths)
        {
            if (!File.Exists(path)) continue;
            if (currentSet.Any(asb => !asb.IsDistinct(path))) continue;
            processedPaths.Add(LogItem.Normal(path, LogItemStyle.SubInfo));
            try
            {
                var asb = Assembly.LoadFrom(path);
                if (asb is not null && currentSet.Add(asb))
                {
                    newLoaded.Add(asb);
                    LoadDependencies(asb, currentSet, newLoaded);
                    results.Add(LogItem.Normal(nameof(LogItemStyle.Success), LogItemStyle.Success));
                }
                else results.Add(LogItem.Header("Skipped", LogItemStyle.Warning));
            }
            catch (Exception e)
            {
                _logger.Error(e);
                results.Add(LogItem.Normal(ErrorEntry.GetShortErrorName(e, nameof(LogItemStyle.Error)), LogItemStyle.Error));
            }
        }
        if (processedPaths.Count > 0)
        {
            LogSession.BuildTable(LogItem.Normal(string.Empty), out var entry, 3, [.. processedPaths], [.. results]);
            if (entry is not null) _logger.Log(entry);
        }
        _logger.ConfigEnd($"Loaded extra {GetAssembliesText(newLoaded.Count)}.");
        if (newLoaded.Count > 0)
        {
            DoValidate();
        }
        return newLoaded;
    }
    private static void LoadDependencies(Assembly root, HashSet<Assembly> currentSet, List<Assembly> newlyAdded)
    {
        var queue = new Queue<AssemblyName>(root.GetReferencedAssemblies());
        while (queue.TryDequeue(out var name))
        {
            if (currentSet.Any(asb => !asb.IsDistinct(name))) continue;
            try
            {
                var loaded = Assembly.Load(name);
                if (currentSet.Add(loaded))
                {
                    newlyAdded.Add(loaded);
                    foreach (var refName in loaded.GetReferencedAssemblies())
                    {
                        queue.Enqueue(refName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
    private static List<Assembly> LoadAllAssemblies()
    {
        var roots = AppDomain.CurrentDomain.GetAssemblies();
        var resultSet = new HashSet<Assembly>(roots);
        var resultList = roots.ToList();

        foreach (var asb in roots)
        {
            var queue = new Queue<AssemblyName>(asb.GetReferencedAssemblies());
            while (queue.TryDequeue(out var name))
            {
                if (resultSet.Any(a => !a.IsDistinct(name))) continue;
                try
                {
                    var loaded = Assembly.Load(name);
                    if (resultSet.Add(loaded))
                    {
                        resultList.Add(loaded);
                        foreach (var r in loaded.GetReferencedAssemblies()) queue.Enqueue(r);
                    }
                }
                catch { }
            }
        }
        return resultList;
    }
    #endregion*/


    private const string PluginsDirectory = "Plugins";
    private readonly ConcurrentDictionary<string, PuginInfo> ctxByDir = [];
    private void LoadAllReferenceAssemblies()
    {

    }
    private void Foo()
    {
        Directory.CreateDirectory(PluginsDirectory);
        var folders = Directory.GetDirectories(PluginsDirectory);
        foreach (var folder in folders)
        {
            if (!ctxByDir.ContainsKey(folder)) 
            {
                var context = new AssemblyLoadContext(folder, true);
                LoadPugin(context);
            }
           
            
        } 
    }
    private void LoadPugin(AssemblyLoadContext ctx)
    {

        var files = Directory.GetFiles(PluginsDirectory, "*.dll");
    }
}
