using ElShrine.Modules;
using ElShrine.Modules.Log;
using System.Diagnostics;
using System.Reflection;

namespace ElShrine;

public static class ModulesExtensions
{
    #region LocalizationExtensions;
    public static string Translate(this string key, string? defaultS = null, params object?[] args)
            => LocalizationManager.Instance.Translate(key, defaultS, args);
    #endregion

    #region LogExtensions;
    #region Common

    #region Normal
    public static void Log(this LogSession session, LogEntry entry)
        => session.LogEntry(entry);
    public static void Log(this LogSession session, InlineInfo info)
        => session.LogEntry(new InfoEntry(info));
    public static void Log(this LogSession session, string msg)
        => session.Log((InlineInfo)msg);
    public static void Log(this LogSession session, params LogItem[] items)
        => session.Log((InlineInfo)items);
    #endregion

    #region Exception
    public static void Error(this LogSession session, Exception e, bool showTrace = true)
        => session.LogEntry(new ErrorEntry(e, showTrace));
    public static void Error(this LogSession session, string msg)
        => session.Error(new Exception(msg));
    public static void Warning(this LogSession session, Exception e, bool showTrace = false)
        => session.LogEntry(new WarningEntry(e, showTrace));
    public static void Warning(this LogSession session, string msg)
        => session.Warning(new Exception(msg));
    #endregion

    #region Scope
    public static LogItem[] GetSummaryItems(this LogSession session, EndConfiguration? config = null)
        => session.GetCurrentScopeAccessor().GetSummaryItems(config);
    public static void ConfigEnd(this LogSession session, EndConfiguration? config = null)
        => session.GetCurrentScopeAccessor().EndConfiguration = config;
    public static void ConfigEnd(this LogSession session, string text, bool showSuc = true, bool showError = true)
        => session.ConfigEnd(new EndConfiguration(text, showSuc, showError));
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
    #endregion
}
