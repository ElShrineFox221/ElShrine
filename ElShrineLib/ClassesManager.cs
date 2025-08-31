using ElShrine.EConsole;
using ElShrine.ECommand;
using ElShrine.EException;
using System.Reflection;
using System.Runtime.CompilerServices;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine
{
    [StartupClass]
    public static class ClassesManager
    {
        #region Vars
        private readonly static List<Assembly> assemblies = [];
        private static int assembliesSearchDepth = 1;
        public static int AssembliesSearchDepth
        {
            get => assembliesSearchDepth;
            set
            {
                assembliesSearchDepth = value;
                assemblies.ReplaceAll(GetCurrentAllAssemblies());
            }
        }
        #endregion

        static ClassesManager()
        {
            ListBeginInfo([new("Try Initializing...")], true);
            try
            {
                assemblies = GetCurrentAllAssemblies();
                ValidateAttributedClasses();
            }
            catch(Exception e)
            {
                ListErrorInfo(e, true);
            }
            var result = GetListInfoListener();
            ListEndInfo([GetCompleteItem(result.Errors.Count == 0, true)], true);
            Paused = false;
            if (result.Errors.Count > 0) GlobalCommandCarrier.Exit();
        }

        #region Initializing methods
        [Obsolete]
        private static void ClassesStaticStartup()
        {
            var attributedTypes = typeof(StartupClassAttribute).GetClassesByAttribute(true);
            foreach (var attributedType in attributedTypes)
            {
                var type = attributedType.type;
                if (type.IsClass && type.IsAbstract && type.IsSealed)
                {
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                else throw new($"The attribute cannot be applied on class {type.FullName}");
            }
        }
        private static void ValidateAttributedClasses()
        {
            var attributedTypes = GetClassesByAttribute<ValidatableAttribute>(true);

            var initializingClasses = attributedTypes.Where(at => at.attrs.ToList().FindIndex(a => a is StartupClassAttribute) != -1);
            List<InformationItem> items = [..initializingClasses.Select(ic => new InformationItem(ic.type.Name, InformationPaintStyle.SubParameterMethod))];
            InformationItem splitItem = new(", ");
            for (int i = items.Count - 1; i > 0; i--) 
            {
                items.Insert(i, splitItem);
            }
            ListContentInfo([new($"Initializing "), ..items, new("...")], true);

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
        }
        #endregion

        #region Classes manage in current domain
        public static bool EqualAssembly(this Assembly a, Assembly b)
        {
            var result = a.FullName == b.FullName;
            if (result)
            {
                if (a.IsDynamic || b.IsDynamic) result &= a.ManifestModule.ModuleVersionId == b.ManifestModule.ModuleVersionId;
                else return result &= a.Location == b.Location;
            }
            return result;
        }

        public static List<Assembly> MergeDistinctAssemblies(this IEnumerable<Assembly> assemblies, params IEnumerable<Assembly>[] assembliesCollections)
        {
            var list = new List<Assembly>();
            foreach(var asb in assemblies)
            {
                if (list.FindIndex(asb0 => asb0.EqualAssembly(asb)) == -1) list.Add(asb); 
            }
            foreach (var assemblies0 in assembliesCollections)
            {
                foreach (var asb in assemblies0)
                {
                    if (list.FindIndex(asb0 => asb0.EqualAssembly(asb)) == -1) list.Add(asb);
                }
            }
            return list;
        }
        public static List<Assembly> GetCurrentAllAssemblies(int depth = 1, List<Assembly>? extraSources = null)
        {
            List<Assembly> assemblies = MergeDistinctAssemblies(AppDomain.CurrentDomain.GetAssemblies(), extraSources ?? []);
            List<Assembly> resultAssemblies = [];
            foreach (Assembly assembly in assemblies)
            {
                if (resultAssemblies.FindIndex(asb => asb.EqualAssembly(assembly)) == -1) resultAssemblies.Add(assembly);
                assembly.GetAllReferenceAssemblies(depth, ref resultAssemblies);
            }
            return resultAssemblies;
        }
        public static void GetAllReferenceAssemblies(this Assembly source, int searchDepth, ref List<Assembly> assemblies)
        {
            if (searchDepth < 1) return;
            var asbNames = source.GetReferencedAssemblies().ToList();
            foreach (var asbName in asbNames)
            {
                if (asbNames.FindIndex(an => an == asbName) == -1) 
                {
                    try
                    {
                        var asb = Assembly.Load(asbName);
                        assemblies.Add(asb);
                        GetAllReferenceAssemblies(asb, searchDepth - 1, ref assemblies);
                    }
                    catch { }
                }
            }
        }

        public static IEnumerable<(Type type, List<Attribute> attr)> GetClassesByAttribute(this Type attribute, bool inherit = false, Assembly[]? range = null)
        {
            range ??= [..MergeDistinctAssemblies(assemblies)];
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
        public static IEnumerable<(Type type, List<A> attrs)> GetClassesByAttribute<A>(bool inherit = false, Assembly[]? range = null) where A : Attribute
        {
            List<(Type type, List<A> attrs)> result = [];
            range ??= [.. MergeDistinctAssemblies(assemblies)];
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
        public static List<Type> GetImplements(this Type parentType, List<Assembly>? range = null)
        {
            List<Type> result = [];
            range ??= [.. MergeDistinctAssemblies(assemblies)];
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
        public static Type GetClassesByName(string typeName, bool ignoreCase = false, Assembly[]? range = null)
        {
            Type? result = null;
            range ??= [.. MergeDistinctAssemblies(assemblies)];
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
