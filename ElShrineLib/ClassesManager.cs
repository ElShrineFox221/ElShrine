using ElShrine.ECommand;
using ElShrine.EConsole;
using ElShrine.EException;
using System.Reflection;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine
{
    [StartupClass]
    public static class ClassesManager
    {
        #region Vars
        public static Assembly[] Assemblies => [.. assemblies];
        private static readonly List<Assembly> assemblies = [];
        private static readonly string DllFileExtension = ".dll";
        #endregion

        #region Initialization
        static ClassesManager()
        {
            DefaultSub = true;
            ListBeginInfo([new("Initializing...")], true);
            try
            {
                assemblies = LoadAllAssemblies();
                var extraFiles = GetDllFilesFrom(Environment.CurrentDirectory);
                if (extraFiles.Length != 0) LoadAssembliesFromFiles(extraFiles, false);
                int count = assemblies.Count;
                ListContentInfo($"Loaded {count} {"assembly".GetPural(count)}.", true);
                DoLoad(assemblies);
            }
            catch(Exception e)
            {
                ListErrorInfo(e, true);
            }
            var result = GetListInfoListener();
            ListEndInfo([GetCompleteItem(result.Errors.Count == 0)]);
            DefaultSub = false;
            Paused = false;
            if (result.Errors.Count > 0) GlobalCommandCarrier.Exit();
        }
        private static List<Assembly> LoadAllAssemblies()
        {
            var assemblies = MergeDistinctAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            List<Assembly> resultAssemblies = [];
            foreach (Assembly assembly in assemblies) resultAssemblies.Add(assembly);
            foreach (Assembly assembly in assemblies) assembly.LoadAllReferenceAssemblies(ref resultAssemblies);
            return resultAssemblies;
        }
        #endregion

        #region Utills - independent
        public static bool IsDistinct(this Assembly assembly, string filePath)
            => assembly.Location != filePath;
        public static bool IsDistinct(this Assembly assembly, AssemblyName assemblyName)
            => assembly.GetName().FullName == assemblyName.FullName;
        public static bool IsDistinct(this Assembly a, Assembly b)
        {
            var result = a.FullName == b.FullName;
            if (result)
            {
                if (a.IsDynamic || b.IsDynamic) result &= a.ManifestModule.ModuleVersionId == b.ManifestModule.ModuleVersionId;
                else result &= a.Location == b.Location;
            }
            return !result;
        }

        public static Assembly? LoadDistinctAssembly(List<Assembly> assemblies, string filePath)
        {
            Assembly? assembly = null;
            if (File.Exists(filePath) && Path.GetExtension(filePath).EqualIgnoreCase(DllFileExtension) && assemblies.FindIndex(asb => !asb.IsDistinct(filePath)) == -1) 
            {
                assembly = Assembly.LoadFile(filePath);
                assemblies.Add(assembly);
            }
            return assembly;
        }
        public static Assembly? LoadDistinctAssembly(List<Assembly> assemblies, AssemblyName assemblyName)
        {
            Assembly? assembly = null;
            if (assemblies.FindIndex(asb => !asb.IsDistinct(assemblyName)) == -1)
            {
                assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            return assembly;
        }
        public static void LoadAllReferenceAssemblies(this Assembly source, ref List<Assembly> assemblies, int searchDepth = -1)
        {
            if (searchDepth == 0) return;
            var asbNames = source.GetReferencedAssemblies().ToList();
            foreach (var asbName in asbNames)
            {
                if (assemblies.FindIndex(asb => !asb.IsDistinct(asbName)) == -1)
                {
                    try
                    {
                        var asb = Assembly.Load(asbName);
                        assemblies.Add(asb);
                        LoadAllReferenceAssemblies(asb, ref assemblies, searchDepth - 1);
                    }
                    catch (Exception e)
                    {
                        ListErrorInfo(e);
                    }
                }
            }
        }
        #endregion

        public delegate void AssembliesLoadedHandler(Assembly[] assemblies);
        public static event AssembliesLoadedHandler? AssembliesLoaded = null;

        private static string[] GetDllFilesFrom(string directory)
        {
            string[] dllFiles = [];
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory);
                dllFiles = [.. files.Where(f => Path.GetExtension(f).EqualIgnoreCase(DllFileExtension))];
            }
            return dllFiles;
        }
        private static List<Assembly> LoadAssembliesFromFiles(IEnumerable<string> files, bool showFailed)
        {
            List<Assembly> fileAssemblies = [.. assemblies];
            foreach (var path in files)
            {
                var fileAsb = LoadDistinctAssembly(fileAssemblies, path);
                if(showFailed || fileAsb is not null)
                {
                    ListContentInfo([GetCompleteItem(fileAsb is not null), new($" Load file from: {path}")]);
                }
            }
            var suc = fileAssemblies.Count != assemblies.Count;
            List<Assembly> distinctAssemblies = [];
            if (suc)
            {
                distinctAssemblies = [.. fileAssemblies];
                foreach (var fileAsb in fileAssemblies)
                {
                    if (!assemblies.Exists(asb => !asb.IsDistinct(fileAsb)))
                    {
                        fileAsb.LoadAllReferenceAssemblies(ref distinctAssemblies);
                    }
                }
                distinctAssemblies = [.. distinctAssemblies.Where(asb => !assemblies.Exists(asb1 => !asb1.IsDistinct(asb)))];
                assemblies.AddRange(distinctAssemblies);
            }
            return distinctAssemblies;
        }
        public static bool LoadExtraAssemblies(string directory)
        {
            var files = GetDllFilesFrom(directory);
            var suc = files.Length != 0;
            if (suc) suc = LoadExtraAssemblies(files);
            return suc;
        }
        public static bool LoadExtraAssemblies(IEnumerable<string> modulePaths)
        {
            ListBeginInfo([new($"Loading Modules...")]);
            var count = assemblies.Count;
            var asbs = LoadAssembliesFromFiles(modulePaths, true);
            count = assemblies.Count - count;
            var suc = count != 0;
            ListContentInfo($"Loaded {count} {"assembly".GetPural(count)}.");
            DoLoad(asbs);
            ListEndInfo([GetCompleteItem(suc)]);
            return suc;
        }
        private static void DoLoad(List<Assembly> assemblies)
        {
            DoValidate(assemblies);
            AssembliesLoaded?.Invoke([..assemblies]);
        }
        private static void DoValidate(List<Assembly> assemblies)
        {
            ListBeginInfo([new("Validating...")]);
            var attributedTypes = GetClassesByAttribute<ValidatableAttribute>(true, [..assemblies]);
            int atts = attributedTypes.Sum(at => at.attrs.Count), ats = attributedTypes.Count();
            ListContentInfo([new($"There are {atts} {"attr".GetPural(atts)} in {ats} attributed {"type".GetPural(ats)}.")]);
            var initializingClasses = attributedTypes.Where(at => at.attrs.ToList().FindIndex(a => a is StartupClassAttribute) != -1);
            if (initializingClasses.Any())
            {
                List<InformationItem> items = [.. initializingClasses.Select(ic => new InformationItem(ic.type.Name, InformationPaintStyle.SubParameterMethod))];
                InformationItem splitItem = new(", ");
                for (int i = items.Count - 1; i > 0; i--)
                {
                    items.Insert(i, splitItem);
                }
                ListContentInfo([new($"Startup {"class".GetPural(items.Count)}: "), .. items, new("...")]);
            }
            foreach (var (type, attrs) in attributedTypes)
            {
                foreach (var attr in attrs)
                {
                    if (!attr.Validate(type))
                    {
                        var exception = new ValidateFailedException(type.Name, $"The attribute require is not met. {attr.ValidateFaliedReason}");
                        ListErrorInfo(exception, true);
                    }
                }
            }
            var result = GetListInfoListener();
            ListEndInfo([GetCompleteItem(result.Errors.Count == 0)]);
        }

        #region Classes manage in current assemblies
        public static List<Assembly> MergeDistinctAssemblies(this IEnumerable<Assembly> assemblies, params IEnumerable<Assembly>[] assembliesCollections)
        {
            var list = new List<Assembly>();
            foreach(var asb in assemblies)
            {
                if (list.FindIndex(asb0 => !asb0.IsDistinct(asb)) == -1) list.Add(asb); 
            }
            foreach (var assemblies0 in assembliesCollections)
            {
                foreach (var asb in assemblies0)
                {
                    if (list.FindIndex(asb0 => !asb0.IsDistinct(asb)) == -1) list.Add(asb);
                }
            }
            return list;
        }
        public static IEnumerable<(Type type, List<Attribute> attr)> GetClassesByAttribute(this Type attribute, bool inherit = false, IEnumerable<Assembly>? range = null)
        {
            range ??= [..assemblies];
            foreach (Assembly assembly in range)
            {
                var preTypes = assembly.GetTypes().Where((t) => t.GetCustomAttributes(attribute, inherit).Length != 0);
                foreach (var type in preTypes)
                {
                    var attrs = type.GetCustomAttributes(attribute, inherit).Select(o => (Attribute)o).ToList();
                    if (attrs.Count > 0) yield return (type, attrs);
                }
            }
            yield break;
        }
        public static IEnumerable<(Type type, List<A> attrs)> GetClassesByAttribute<A>(bool inherit = false, IEnumerable<Assembly>? range = null) where A : Attribute
        {
            List<(Type type, List<A> attrs)> result = [];
            range ??= [..assemblies];
            foreach (Assembly assembly in range)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var attrs = type.GetCustomAttributes<A>(inherit).ToList();
                    if (attrs.Count > 0) result.Add((type, attrs));
                }
            }
            return result;
        }
        public static List<Type> GetImplements(this Type parentType, IEnumerable<Assembly>? range = null)
        {
            List<Type> result = [];
            range ??= [.. assemblies];
            foreach (Assembly assembly in range)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (parentType.IsBaseOrInterfaceOf(type)) result.Add(type);
                }
            }
            result.Remove(parentType);
            result.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
            return [.. result];
        }
        public static Type GetClassesByName(string typeName, bool ignoreCase = false, IEnumerable<Assembly>? range = null)
        {
            Type? result = null;
            range ??= [.. assemblies];
            foreach (Assembly assembly in range)
            {
                if (result is not null) break;
                foreach (Type type in assembly.GetTypes())
                {
                    if (result is not null) break;
                    else
                    {
                        if ((ignoreCase && (type.FullName?.EqualIgnoreCase(typeName) ?? false)) || (type.FullName?.Equals(typeName) ?? false))
                        {
                            result = type;
                        }
                        else result = null;
                    }
                }
            }
            return result ?? throw new($"Cannot get class named {typeName}.(Ignore case: {ignoreCase})");
        }
        public static MethodInfo GetGenericClassMethod(this Type genericClass, string methodName, Type constructType, BindingFlags flags, params Type[] paramTypes)
        {
            Type className = genericClass.MakeGenericType(constructType);
            MethodInfo? mi = className.GetMethod(methodName, flags, paramTypes);
            return mi ?? throw new($"Cannot get a method named {methodName} from generic class build from {genericClass} by types({paramTypes.BuildString()})");
        }
        #endregion

        #region Simple relativeless extensions of Type
        public static bool NameEqual(this Type type, string matchName, bool ignoreCase = false)
            => (ignoreCase && type.Name.EqualIgnoreCase(matchName)) || type.Name.Equals(matchName) ||
            (ignoreCase && (type.FullName?.EqualIgnoreCase(matchName) ?? false)) || (type.FullName?.Equals(matchName) ?? false);
        public static void SetMemberValue(this Type ownerClass, object? owner, object? value, string memberName)
        {
            var pio = ownerClass.GetProperty(memberName);
            if (pio is not null)
            {
                pio.SetValue(owner, value);
                return;
            }
            var fio = ownerClass.GetField(memberName);
            if (fio is not null)
            {
                fio.SetValue(owner, value);
                return;
            }
            throw new($"Cannot find property and field named {memberName}");
        }
        public static object? GetMemberValue(this Type ownerClass, object? owner, string memberName)
        {
            object? result;
            var pio = ownerClass.GetProperty(memberName);
            if (pio is not null)
            {
                result = pio.GetValue(owner);
                return result;
            }
            var fio = ownerClass.GetField(memberName);
            if (fio is not null)
            {
                result = fio.GetValue(owner);
                return result;
            }
            throw new($"Cannot find property and field named {memberName}");
        }
        public static void SetMemberValue(this MemberInfo memberInfo, object? owner, object? value)
        {
            if (memberInfo is PropertyInfo pio)
            {
                pio.SetValue(owner, value);
                return;
            }
            else if (memberInfo is FieldInfo fio)
            {
                fio.SetValue(owner, value);
                return;
            }
            throw new($"MemberInfo is invalid for setting as property or field.");
        }
        public static object? GetMemberValue(this MemberInfo memberInfo, object? owner)
        {
            object? result;
            if (memberInfo is PropertyInfo pio)
            {
                result = pio.GetValue(owner);
                return result;
            }
            if (memberInfo is FieldInfo fio)
            {
                result = fio.GetValue(owner);
                return result;
            }
            throw new($"MemberInfo is invalid for setting as property or field.");
        }

        public static bool IsBaseOrInterfaceOf(this Type baseOrIType, Type targetType)
        {
            bool result = false;
            if (baseOrIType.IsGenericTypeDefinition)
            {
                if (baseOrIType.IsInterface) result = targetType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseOrIType);
                else if (baseOrIType.IsClass)
                {
                    for (Type? t = targetType; t is not null && t != typeof(object); t = t.BaseType)
                    {
                        if (t.IsGenericType && t.GetGenericTypeDefinition() == baseOrIType)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            else result = baseOrIType.IsAssignableFrom(targetType);
            return result;
        }
        public static bool IsImplementOf(this Type implClass, Type targetClass)
            => targetClass.IsBaseOrInterfaceOf(implClass);

        public static bool IsStaticClass(this Type type)
            => type.IsClass && type.IsSealed && type.IsAbstract;
        #endregion
    }
}
