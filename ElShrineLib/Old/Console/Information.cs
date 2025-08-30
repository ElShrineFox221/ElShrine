using ElShrine.Old.Command;

namespace ElShrine.Old.Console
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public enum InfosType
    {
        Normal = 0, Command, Error, Complete, Parameter, Method, Notice
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public enum ReportLevel
    {
        OnlyError = 0, OnlyComplete, All, None,
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct Information(string Text, InfosType Infos = InfosType.Normal, bool FullLine = true, int Priority = Information.DefaultPriority)
    {
        public const int DefaultPriority = 0;

        public readonly DateTime DateTime = DateTime.UtcNow;
        public static ConsoleColor GetColor(InfosType? type)
            => type switch
            {
                InfosType.Method => ConsoleOption.GetInstance().MethodColor,
                InfosType.Notice => ConsoleOption.GetInstance().NoticeColor,
                InfosType.Command => ConsoleOption.GetInstance().CommandColor,
                InfosType.Error => ConsoleOption.GetInstance().ErrorColor,
                InfosType.Complete => ConsoleOption.GetInstance().CompleteColor,
                InfosType.Parameter => ConsoleOption.GetInstance().ParameterColor,
                _ => ConsoleOption.GetInstance().DefaultColor
            };
        private static readonly Func<int, bool> defaultFilter = (priority) => priority >= DefaultPriority;
        public void Print(Func<int, bool>? filter = null)
        {
            bool check = (filter ?? defaultFilter).Invoke(Priority);
            if (!check) return;
            ConsoleColor foreColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = GetColor(Infos);
            if (FullLine) ConsoleManager.WriteLine(Text);
            else ConsoleManager.Write(Text);
            System.Console.ForegroundColor = foreColor;
        }
        public static string? Read(ConsoleColor? ForeColor = null)
        {
            ConsoleColor foreColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ForeColor ?? ConsoleOption.GetInstance().CommandColor;
            string? str = ConsoleManager.ReadLine();
            System.Console.ForegroundColor = foreColor;
            return str;
        }

        public static void ReportFinally(bool result, ReportLevel level = ReportLevel.All)
        {
            if (level == ReportLevel.None || level == ReportLevel.OnlyError) return;
            if (result) ConsoleManager.ListInfo(new($"[Complete]", InfosType.Complete));
        }
        public static void ReportCatch(Exception e, ReportLevel level = ReportLevel.All)
        {
            if (level == ReportLevel.OnlyComplete || level == ReportLevel.None) return;
            Exception? exception = e;
            List<Exception> exceptions = [];
            CommonHelper.Efor((i) =>
            {
                if (exception.InnerException is not null)
                {
                    exception = exception?.InnerException;
                    if (exception is EArgumentException) exceptions.Add(exception);
                }
            }, 10);
            e = exceptions.Count > 0 ? exceptions[^1] : e;
            ConsoleManager.ListInfo(new($"[Error]: ", InfosType.Error, false));
            ConsoleManager.ListInfo(new($"{e.Message}.", InfosType.Error));
            //ConsoleManager.ListErrorInfo(new($"{ConsoleOption.Space}Exception type <{e.GetType().ItemName}>", InfosType.Error));
            ConsoleManager.ListInfo(new($"{ConsoleOption.Space}Assembly <{e.Source}>", InfosType.Error));
            ConsoleManager.ListInfo(new($"{ConsoleOption.Space}Trace{e.StackTrace?.Replace("   at ", " at ")}", InfosType.Error));
            ConsoleManager.ListInfo(new($"Use the <Help> command to get all methods information.", InfosType.Notice));
        }
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class InfomationExtension
    {
        public static void Print(this IEnumerable<Information> informations, Func<int, bool>? filter = null)
        {
            foreach (Information information in informations) information.Print(filter);
        }
    }
}
