using ElShrine.Modules;
using ElShrine.Modules.Log;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace ElShrine;

public static class ModulesExtensions
{
    #region LocalizationExtensions;
    private static LocalizationManager Localization =>CoreModuleAccessor.localizationManager;
    public static string Translate(this string key, string? defaultS = null, params object?[] args)
            => Localization.Translate(key, defaultS, args);
    #endregion

    #region LogExtensions;
    #region Common

    #region Normal
    public static void Log(this ILogger logger, LogEntry entry)
        => logger.LogEntry(entry);
    public static void Log(this ILogger logger, EntryContent info)
        => logger.LogEntry(new InfoEntry(info));
    public static void Log(this ILogger logger, string msg)
        => logger.Log((EntryContent)msg);
    public static void Log(this ILogger logger, params LogItem[] items)
        => logger.Log((EntryContent)items);
    #endregion

    #region Exception
    public static void Error(this ILogger logger, Exception e, bool showTrace = true)
        => logger.LogEntry(new ErrorEntry(e, showTrace));
    public static void Error(this ILogger logger, string msg)
        => logger.Error(new Exception(msg));
    public static void Warning(this ILogger logger, Exception e, bool showTrace = false)
        => logger.LogEntry(new WarningEntry(e, showTrace));
    public static void Warning(this ILogger logger, string msg)
        => logger.Warning(new Exception(msg));
    #endregion

    #region Scope
    public static LogItem[] GetSummaryItems(this ILogger logger, EndConfiguration? config = null)
        => logger.GetCurrentScopeAccessor().GetSummaryItems(config);
    public static void ConfigEnd(this ILogger logger, EndConfiguration? config = null)
        => logger.GetCurrentScopeAccessor().EndConfig = config;
    public static void ConfigEnd(this ILogger logger, string text, bool showSuc = true, bool showError = true)
        => logger.ConfigEnd(new EndConfiguration(text, showSuc, showError));
    #endregion

    #endregion
    public static string GetStopwatchElapsed(this Stopwatch sw, bool restartStopwatch = false)
    {
        sw.Stop();
        var r = $"{sw.ElapsedMilliseconds} ms consumed";
        if (restartStopwatch) sw.Restart();
        return r;
    }
    #endregion

    #region PluginExtensions;
    public static string GetContextName(this AssemblyLoadContext ctx)
        => ctx == AssemblyLoadContext.Default ? "Default" : (ctx.Name ?? "Unknow");
    #endregion

    #region ClassesExtensions;
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
    public static bool IsImplementOf(this Type subType, Type baseType) => baseType.IsBaseOrInterfaceOf(subType);

    public static bool IsStaticClass(this Type type) => type.IsSealed && type.IsAbstract;

    public static IEnumerable<Type> GetImplements(this AssemblyLoadContext? ctx, Type baseTypeOrInterface)
    {
        ctx ??= AssemblyLoadContext.Default;
        var targetRange = ctx.Assemblies;
        foreach (var asb in targetRange)
        {
            var types = asb.GetTypes();
            foreach (var type in types)
            {
                if (baseTypeOrInterface.IsBaseOrInterfaceOf(type)) yield return type;
            }
        }
        yield break;
    }
    public static IEnumerable<KeyValuePair<Type, IReadOnlyList<TAttr>>> GetClassesByAttribute<TAttr>(this AssemblyLoadContext? ctx, bool inherit)
        where TAttr : Attribute
    {
        ctx ??= AssemblyLoadContext.Default;
        var targetRange = ctx.Assemblies;
        foreach (var asb in targetRange)
        {
            var types = asb.GetTypes();
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes<TAttr>(inherit).ToList();
                if (attrs.Count > 0)
                    yield return new(type, attrs);
            }
        }
    }
    #endregion
}
