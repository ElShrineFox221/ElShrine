using ElShrine.Common.Interface;
using ElShrine.Old.Console;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public partial record class Command(string CommandStr)
    {
        public CommandType CommandType { get; private set; }
        public string? CarrierName { get; private set; } = null;
        public string MethodName { get; private set; } = string.Empty;
        public string ParameterStr { get; private set; } = string.Empty;

        public CommandInfo CommandInfo { get; private set; }
        public object?[] Parameters { get; private set; } = [];
        public object? Target { get; private set; }
        public object? Result { get; private set; }

        private static class StrSign
        {
            public const string Help = nameof(Help);
            public const string Get = nameof(Get);
            public const string Set = nameof(Set);
            public const string Calc = nameof(Calc);
            public const string Calculation = nameof(Calculation);
            public const string Special = nameof(Special);
        }

        [GeneratedRegex($"(\"[^\"]+\")|\\S+")]
        private static partial Regex GetSectionRegex();
        [GeneratedRegex("[a-zA-Z_]([a-z0-9A-Z_]+)?")]
        private static partial Regex GetNounRegex();
        private readonly static Regex SectionRegex = GetSectionRegex(), NounRegx = GetNounRegex();

        public Command? Parser(out bool success, ReportLevel level = ReportLevel.OnlyError)
        {
            success = true;
            var matchedSections = SectionRegex.Matches(CommandStr);
            Match[] matches;
            try
            {
                Exception exception_WrongCommand = new($"Cannot parse the command. Command: \"{CommandStr}\"");
                {
                    if (matchedSections.Count == 0) throw exception_WrongCommand;
                    string header = matchedSections[0].Value;

                    string[] strSign = [StrSign.Set, StrSign.Get, StrSign.Help, StrSign.Calc, StrSign.Calculation, StrSign.Special];
                    int r = strSign.ToList().FindIndex((s) => s.Equals(header, StringComparison.CurrentCultureIgnoreCase));
                    CommandType = (CommandType)(r > 3 ? r : r + 1);

                    int startIndex = 1;
                    if(CommandType == CommandType.Special)
                    {
                        if (matchedSections.Count < 2) throw exception_WrongCommand;
                        else header = matchedSections[1].Value;
                        startIndex = 2;
                    }
                    matches = matchedSections.Count == startIndex ? [] : matchedSections.Slice(startIndex, matchedSections.Count - 1);
                    ParameterStr = matches.Length == 0 ? Const.EmptyStr : matches.Select(m => m.Value).BuildString(split: string.Empty);

                    var headerSectionResults = NounRegx.Matches(header);
                    if (headerSectionResults.Count == 0) throw exception_WrongCommand;
                    MethodName = headerSectionResults[^1].Value;
                    CarrierName = headerSectionResults.Count switch
                    {
                        1 => null,
                        2 => headerSectionResults[0].Value,
                        _ => throw exception_WrongCommand
                    };
                }//Header
                Exception exception_MissCommand = new($"Cannot find the command operation. Command: \"{CarrierName ?? "Global"}.{MethodName}\"");
                CommandInfo[] cis;
                {
                    cis = [.. CommandManager.CommandInfoCollection.ToList().FindAll((ci) => ci.Matched(MethodName, CarrierName))];
                    if (cis.Length == 0) throw exception_MissCommand;
                }//Find
                Exception exception_WrongParameterFormat = new($"Cannot format the parameters. Parameter string: \"{ParameterStr}\"");
                Exception exception_ParameterCountMismatch = new($"Cannot find or specify proper override method with the parameters, paramter count mismatche");
                {
                    if (CommandType == CommandType.Calculation)
                    {
                        ConsoleManager.ListInfo(new(nameof(CommandType.Calculation)));
                    }
                    else if (CommandType == CommandType.Special)
                    {
                        SpecialCommandParse(cis, out object?[] parameters, out CommandInfo commandInfo);
                        Parameters = parameters;
                        CommandInfo = commandInfo;
                    }
                    else
                    {
                        object?[] parameters = matches.Length == 0 ? [] : new object?[matches.Length];
                        try
                        {
                            if (matches.Length > 0)
                            {
                                for (int i = 0; i < matches.Length; i++)
                                {
                                    parameters[i] = CommandManager.StrToParamDefault(matches[i].Value);
                                }
                            }
                        }
                        catch { throw exception_WrongParameterFormat; }
                        Parameters = parameters;
                        CommandInfo[] resultCI = [.. cis.Where((ci0) => ci0.Method.GetParameters().Length == parameters.Length)];
                        if (resultCI.Length != 1) throw exception_ParameterCountMismatch;
                        CommandInfo = resultCI[0];
                    }
                }//ParamsFormat
            }
            catch (Exception e)
            {
                Information.ReportCatch(e, level);
                success = false;
            }
            finally
            {
                Information.ReportFinally(success, level);
            }
            return success ? this : null;
        }
        public Command? Parser(ReportLevel level = ReportLevel.OnlyError) => Parser(out _, level);

        public object? Execute(out bool success, ReportLevel level = ReportLevel.All)
        {
            string infos; MethodBase method = CommandInfo.Method;
            success = true;
            object? result = null;

            if (method.IsStatic) infos = $"{method.DeclaringType?.Name}";
            else infos = Target is IEName ien ? $"{ien.Name}" : $"{Target?.GetType().Name}";
            infos = $"{infos}.{method.Name}";

            ConsoleManager.ListInfo(new($"Processing formation: {infos}", InfosType.Method, false));
            ConsoleManager.ListInfo(new($"({Parameters.BuildString()})", InfosType.Parameter, false));
            ConsoleManager.ListInfo(new("...", InfosType.Method));
            TimeSpan? time = ConsoleRelevantInvoke(() =>
            {
                if (method is ConstructorInfo ci) result = ci.Invoke(Parameters);
                else if (method is MethodInfo mi) result = mi.Invoke(Target, Parameters);
                else result = method.Invoke(Target ?? this, Parameters);
            }, out success, level, true);
            if (time is not null) ConsoleManager.ListInfo(new($"{time.Value.TotalMilliseconds} ms consumed.", InfosType.Normal));
            Result = result;
            return result;
        }
        public object? Execute(ReportLevel level = ReportLevel.All) => Execute(out _, level);
        public static TimeSpan? ConsoleRelevantInvoke(Action tryAction, out bool success, ReportLevel reportLevel = ReportLevel.OnlyError, bool useWatcher = false, Action<Exception>? catchAction = null, Action? finallyAction = null)
        {
            TimeSpan? result = null;
            Stopwatch? watch = null;
            success = true;
            if (useWatcher)
            {
                watch = new();
                watch.Start();
            }
            try
            {
                tryAction?.Invoke();
            }
            catch (Exception e)
            {
                catchAction?.Invoke(e);
                success = false;
                Information.ReportCatch(e, reportLevel);
            }
            finally
            {
                watch?.Stop();
                result = watch?.Elapsed;
                finallyAction?.Invoke();
                Information.ReportFinally(success, reportLevel);
            }
            return result;
        }

        private readonly static Exception exception = new($"Cannot praser the command type.");
        protected virtual void SpecialCommandParse(CommandInfo[] nameMatchedpreCommandInfos, out object?[] parameters, out CommandInfo commandInfo)
        {
            throw exception;
        }
        protected virtual object? SpecialCommandExcute()
        {
            throw exception;
        }

        
    }
}
