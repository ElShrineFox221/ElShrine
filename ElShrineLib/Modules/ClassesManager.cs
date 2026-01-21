using System.Diagnostics;
using System.Reflection;

namespace ElShrine.Modules
{
    [InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_CLASSES)]
    public sealed class ClassesManager : IInitializable<ClassesManager>
    {
        #region Singleton
        private static readonly Lazy<ClassesManager> instanceLazy = new(() => new());
        public static ClassesManager Instance => Bootstrapper.GetInstance<ClassesManager>();
        public static ClassesManager Initialize() => instanceLazy.Value;
        #endregion

        private readonly HashSet<Assembly> assemblies = [];
        private readonly Queue<Assembly> unvalidatedAssemblies = new();

        public Func<Assembly, bool>? AssembliesFilter { get; set; }
        public IEnumerable<Assembly> FilteredAssemblies => AssembliesFilter is null ? assemblies : assemblies.Where(AssembliesFilter);
        public IReadOnlyList<Assembly> Assemblies => [.. assemblies];
        private static LogSession Session => LogProducer.Instance.CoreSession;
        private ClassesManager()
        {
            using (Session.OpenScope($"Initializing {nameof(ClassesManager)}..."))
            {
                var sw = Stopwatch.StartNew();
                //
                var initialAsbs = LoadAllAssemblies();
                Session.Log($"Loaded referenced all {GetAssembliesText(initialAsbs.Count)}.");
                UpdateAssemblies(initialAsbs);
                //
                LoadExtraAssemblies(Environment.CurrentDirectory);
                if (unvalidatedAssemblies.Count > 0) DoValidate();
                //
                Session.ConfigEnd($"Initialized {nameof(ClassesManager)}, {sw.GetStopwatchElapsed()}");
            }
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
            Session.Log($"There are {GetAssembliesText(unvalidatedAssemblies.Count)} to be vaildated.");
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
            using var scope = Session.OpenScope("Loading extra assemblies...");
            var processedPaths = new List<LogItem>();
            var results = new List<LogItem>();
            var newLoaded = new List<Assembly>();
            var currentSet = new HashSet<Assembly>(assemblies);
            foreach (var path in modulePaths)
            {
                if (!File.Exists(path)) continue;
                if (currentSet.Any(asb => !asb.IsDistinct(path))) continue;
                processedPaths.Add(LogItem.Normal(path, LogPaintMode.Parameter));
                try
                {
                    var asb = Assembly.LoadFrom(path);
                    if (asb is not null && currentSet.Add(asb))
                    {
                        newLoaded.Add(asb);
                        LoadDependencies(asb, currentSet, newLoaded);
                        results.Add(LogItem.SuccessHeader());
                    }
                    else results.Add(LogItem.Header("Skipped", LogPaintMode.Warning));
                }
                catch (Exception e)
                {
                    Session.Error(e);
                    results.Add(LogItem.ErrorHeader(e));
                }
            }
            if (processedPaths.Count > 0) Session.Table(3, [.. processedPaths], [.. results]);
            Session.ConfigEnd($"Loaded extra {GetAssembliesText(newLoaded.Count)}.");
            if (newLoaded.Count > 0)
            {
                UpdateAssemblies(newLoaded);
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
                catch(Exception ex) 
                {
                    Session.Error(ex);
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
            using var scope = Session.OpenScope($"Validating {GetAssembliesText(toValidate.Count)}...");
            try
            {
                var attributedTypes = GetClassesByAttribute<ValidatableAttribute>(true, toValidate);
                foreach (var (type, attrs) in attributedTypes)
                {
                    foreach (var attr in attrs)
                    {
                        if (!attr.DoValidate(type))
                        {
                            Session.Error(true, LogItem.Header(type.Name, LogPaintMode.Type),
                                LogItem.Normal("Validation failed: ", LogPaintMode.Normal),
                                LogItem.Normal(attr.ValidateFailedReason ?? string.Empty, LogPaintMode.Warning)
                            );
                        }
                        else
                        {
                            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                            foreach (var member in members)
                            {
                                var memberAttrs = member.GetCustomAttributes<ValidatableAttribute>(true);
                                foreach (var mAttr in memberAttrs)
                                {
                                    if (!mAttr.DoValidate(member))
                                    {
                                       Session.Error(true, LogItem.Header($"{type.Name}.{member.Name}", LogPaintMode.Type),
                                            LogItem.Normal("Validation failed: ", LogPaintMode.Normal),
                                            LogItem.Normal(attr.ValidateFailedReason ?? string.Empty, LogPaintMode.Warning));
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
                Session.Error(e);
            }
            finally
            {
                Session.ConfigEnd($"Validation process completed, {sw.GetStopwatchElapsed()}");
            }
        }
    }
    public static class ClassesHelper
    {
        public static bool NameEqual(this Type type, string matchName, bool ignoreCase = false)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(type.Name, matchName, comparison) || string.Equals(type.FullName, matchName, comparison);
        }
        public static void SetMemberValue(this Type ownerClass, object? owner, object? value, string memberName)
        {
            var member = ((MemberInfo?)ownerClass.GetProperty(memberName) ?? ownerClass.GetField(memberName)) ?? throw new MissingMemberException(ownerClass.Name, memberName);
            member.SetMemberValue(owner, value);
        }
        public static void SetMemberValue(this MemberInfo member, object? owner, object? value)
        {
            switch (member)
            {
                case PropertyInfo p: p.SetValue(owner, value); break;
                case FieldInfo f: f.SetValue(owner, value); break;
                default: throw new InvalidOperationException("Member must be Property or Field.");
            }
        }
        public static object? GetMemberValue(this MemberInfo member, object? owner) => member switch
        {
            PropertyInfo p => p.GetValue(owner),
            FieldInfo f => f.GetValue(owner),
            _ => throw new InvalidOperationException("Member must be Property or Field.")
        };
        public static bool IsBaseOrInterfaceOf(this Type baseType, Type targetType)
        {
            if (!baseType.IsGenericTypeDefinition) return baseType.IsAssignableFrom(targetType);
            if (baseType.IsInterface)
            {
                var suc = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == baseType;
                if (suc) return true;
                else return targetType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);
            }

            var current = targetType;
            while (current is not null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == baseType) return true;
                current = current.BaseType;
            }
            return false;
        }
        public static bool IsStaticClass(this Type type) => type.IsSealed && type.IsAbstract;
        public static bool IsDistinct(this Assembly a, string filePath) =>
            !string.Equals(a.Location, filePath, StringComparison.OrdinalIgnoreCase);
        public static bool IsDistinct(this Assembly a, AssemblyName bName) =>
            !string.Equals(a.GetName().FullName, bName.FullName, StringComparison.Ordinal);
        public static bool IsDistinct(this Assembly a, Assembly b)
        {
            if (a.FullName != b.FullName) return true;
            return a.IsDynamic || b.IsDynamic
                ? a.ManifestModule.ModuleVersionId != b.ManifestModule.ModuleVersionId
                : a.Location != b.Location;
        }
    }
}
