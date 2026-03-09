using ElShrine.Modules.Log;
using ElShrine.Modules.TBD;
using System.Diagnostics;
using System.Reflection;

namespace ElShrine.Modules;

[Obsolete(TBDMsg.TBD_MSG)]
public sealed class ClassesManager
{
    private readonly HashSet<Assembly> assemblies = [];
    private readonly Queue<Assembly> unvalidatedAssemblies = new();

    public Func<Assembly, bool>? AssembliesFilter { get; set; }
    public IEnumerable<Assembly> FilteredAssemblies => AssembliesFilter is null ? assemblies : assemblies.Where(AssembliesFilter);
    public IReadOnlyList<Assembly> Assemblies => [.. assemblies];
    private readonly ILogger _logger;
    public ClassesManager(ILoggerManager log)
    {
        _logger = log.Main;

        var initialAsbs = LoadAllAssemblies();
        _logger.Log($"Loaded referenced all {GetAssembliesText(initialAsbs.Count)}.");
        UpdateAssemblies(initialAsbs);
        //
        LoadExtraAssemblies(Environment.CurrentDirectory);
        if (unvalidatedAssemblies.Count > 0) DoValidate();
    }
    private static string GetAssembliesText(int count) => nameof(Assembly).GetPuralWithNum(count).ToLower();

    #region Loading Logic
    public delegate void AssembliesUpdatedHandler(Assembly[] assemblies);
    public event AssembliesUpdatedHandler? AssembliesUpdated;
    private void UpdateAssemblies(IEnumerable<Assembly> newAsbs)
    {
        foreach (var asb in newAsbs)
        {
            if (assemblies.Add(asb)) unvalidatedAssemblies.Enqueue(asb);
        }
        _logger.Log($"There are {GetAssembliesText(unvalidatedAssemblies.Count)} to be vaildated.");
    }

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
            if (currentSet.Any(asb => asb.Location == path)) continue;
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
            EntryContent.BuildTable(LogItem.Normal(string.Empty), out var entry, 3, [.. processedPaths], [.. results]);
            if (entry is not null) _logger.Log(entry);
        }
        _logger.ConfigEnd($"Loaded extra {GetAssembliesText(newLoaded.Count)}.");
        if (newLoaded.Count > 0)
        {
            UpdateAssemblies(newLoaded);
            DoValidate();
        }
        return newLoaded;
    }
    private void LoadDependencies(Assembly root, HashSet<Assembly> currentSet, List<Assembly> newlyAdded)
    {
        var queue = new Queue<AssemblyName>(root.GetReferencedAssemblies());
        while (queue.TryDequeue(out var name))
        {
            if (currentSet.Any(asb => asb.GetName() == name)) continue;
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
            catch(Exception ex) 
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
                if (resultSet.Any(a => a.GetName() == name)) continue;
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
    #endregion

    #region Type Query
    public IEnumerable<KeyValuePair<Type, IReadOnlyList<TAttr>>> GetClassesByAttribute<TAttr>(bool inherit = false, IEnumerable<Assembly>? range = null)
        where TAttr : Attribute
    {
        var targetRange = range ?? FilteredAssemblies;
        foreach (var asb in targetRange)
        {
            var types = asb.GetTypes();
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes<TAttr>(inherit).ToList();
                if (attrs.Count > 0) yield return new(type, attrs);
            }
        }
    }
    public Type GetClassByName(string typeName, bool ignoreCase = false)
    {
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var type = FilteredAssemblies
            .SelectMany(asb => asb.GetTypes())
            .FirstOrDefault(t => string.Equals(t.FullName, typeName, comparison) || string.Equals(t.Name, typeName, comparison));
        return type ?? throw new TypeLoadException($"Cannot find type: {typeName}");
    }
    public IEnumerable<Type> GetImplements(Type baseTypeOrInterface, IEnumerable<Assembly>? range = null)
    {
        var targetRange = range ?? FilteredAssemblies;
        foreach (var asb in targetRange)
        {
            var types = asb.GetTypes();
            foreach (var type in types)
            {
                if (baseTypeOrInterface.IsBaseOrInterfaceOf(type))  yield return type;
            }
        }
        yield break;
    }
    #endregion

    private void DoValidate()
    {
        var sw = Stopwatch.StartNew();
        var toValidate = new List<Assembly>();
        while (unvalidatedAssemblies.TryDequeue(out var asb)) toValidate.Add(asb);
        if (toValidate.Count == 0) return;
        //
        using var scope = _logger.OpenScope($"Validating {GetAssembliesText(toValidate.Count)}...");
        try
        {
            var attributedTypes = GetClassesByAttribute<ValidatableBaseAttribute>(true, toValidate);
            foreach (var (type, attrs) in attributedTypes)
            {
                foreach (var attr in attrs)
                {
                    if (!attr.DoValidate(type, null))
                    {
                        var e = new ValidationFailedException(attr.ValidateFailedReason ?? string.Empty);
                        _logger.Error(e);
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
                                    _logger.Error(e);
                                }
                            }
                        }
                    }
                }
            }
            AssembliesUpdated?.Invoke([.. toValidate]);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
        finally
        {
            _logger.ConfigEnd($"Validation process completed, {sw.GetStopwatchElapsed()}");
        }
    }
}
