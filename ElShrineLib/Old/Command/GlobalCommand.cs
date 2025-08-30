using ElShrine.Old.Console;
using ElShrine.ETimer;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [CommandCarrier(IsGlobalCarrier = true)]
    public abstract class GlobalCommand
    {
        private static void GetIEnumItem(IEnumerable ienum)
        {
            IEnumerable<object> items = (IEnumerable<object>)ienum;
            int maxValue = items.Select((i) => i.ToString()?.Length ?? 0).ToList().Max() + 7;
            ConsoleManager.ListInfo(new($"{items.Count()} items as follows:", InfosType.Normal));
            foreach (object item in ienum)
            {
                ConsoleManager.ListInfo(new($"{ConsoleOption.Space}{item}".PadRight(maxValue), InfosType.Method, false));
                ConsoleManager.ListInfo(new($"<{item.GetType().Name}>", InfosType.Normal));
            }
        }
        [Command(Description = "Get all available methods.")]
        public static void Help()
        {
            ConsoleManager.ListInfo(new($"{CommandManager.CommandInfoCollection.Length} command as follows:", InfosType.Normal));
            foreach (CommandInfo ci in CommandManager.CommandInfoCollection)
            {
                ConsoleManager.ListInfo(new($"{ConsoleOption.Space}{ci.FullDefaultName}", InfosType.Method, false));
                ConsoleManager.ListInfo(new($"({ci.Method.GetParameters().Select((pi) => $"{pi.ParameterType.Name} {pi.Name}").BuildString()})", InfosType.Parameter));
                string[]? descriptions = ci.GetDescriptions();
                if (descriptions != null) foreach (string desc in descriptions) ConsoleManager.ListInfo(new($"{ConsoleOption.Space}{ConsoleOption.Space}>> {desc}", InfosType.Normal));
            }
        }
        public static void Help(object target, bool? methodsNorProperties = null)
        {
            Type type = target.GetType();
            if (methodsNorProperties is null || methodsNorProperties.Value)
            {
                MethodInfo[] methodInfos = [.. type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Where((mi) => !mi.IsSpecialName)];
                if (methodInfos.Length == 0) return;
                int maxReturnType = methodInfos.Select((mi) => mi.ReturnType.Name.Length).ToList().Max() + 7;
                int maxName = methodInfos.Select((mi) => mi.Name.Length).ToList().Max() + 4;
                ConsoleManager.ListInfo(new($"{methodInfos.Length} methods as follows:", InfosType.Normal));
                foreach (MethodInfo methodInfo in methodInfos)
                {
                    ConsoleManager.ListInfo(new($"{ConsoleOption.Space}<{methodInfo.ReturnType.Name}>".PadRight(maxReturnType), InfosType.Parameter, false));
                    ConsoleManager.ListInfo(new($"{methodInfo.Name}".PadRight(maxName), InfosType.Method, false));
                    ConsoleManager.ListInfo(new($"({methodInfo.GetParameters().Select((pi) => $"{pi.ParameterType.Name} {pi.Name}").BuildString()})", InfosType.Parameter));
                }
            }
            if (methodsNorProperties is null || !methodsNorProperties.Value)
            {
                if (target is IEnumerable ienum && target is not string)
                {
                    GetIEnumItem(ienum);
                }
                else
                {
                    PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    int maxName = propertyInfos.Select((pi) => pi.Name.Length).ToList().Max() + 7;
                    List<object?> values = [];
                    List<PropertyInfo> newPIs = [];
                    foreach (PropertyInfo pi in propertyInfos)
                    {
                        try
                        {
                            values.Add(pi.GetValue(target));
                            newPIs.Add(pi);
                        }
                        catch { }
                    }
                    propertyInfos = [.. newPIs];
                    int maxValue = values.Select((v) => v?.ToString()?.Length ?? 0).ToList().Max() + 4;
                    ConsoleManager.ListInfo(new($"{propertyInfos.Length} properties as follows:", InfosType.Normal));
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        ConsoleManager.ListInfo(new($"{ConsoleOption.Space}{propertyInfo.Name}".PadRight(maxName), InfosType.Method, false));
                        ConsoleManager.ListInfo(new($"{propertyInfo.GetValue(target)}".PadRight(maxValue), InfosType.Method, false));
                        ConsoleManager.ListInfo(new($"<{propertyInfo.PropertyType}>", InfosType.Normal));
                    }
                }
            }
        }
        public static void Exit()
        {
            NamedTimerManager.SetInterval(() => Environment.Exit(0), 2500, false);
            NamedTimerManager.SetInterval(() =>
            {
                ConsoleManager.ListInfo(new("The console will be closed in 2 seconds...", InfosType.Normal));
                ConsoleManager.ListInfo(new("Press enter to colse the console directly...", InfosType.Notice));
            }, 500, false);
        }
        public static void Open(string? path)
        {
            path ??= Environment.CurrentDirectory;
            if (File.Exists(path)) Process.Start(path);
            if (Directory.Exists(path)) Process.Start(Const.ExplorerName, path);
        }
    }
}
