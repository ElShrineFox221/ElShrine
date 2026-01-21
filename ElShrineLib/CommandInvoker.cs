using ElShrine.Common;
using ElShrine.Modules;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ElShrine
{
    public enum CommandRoute
    {
        Cmd,
        Local,
        Log,
    }
    public interface ICommandResult
    {
        bool IsSuccess { get; }
        object? ReturnValue { get; }
        long ExecutionMilliseconds { get; }
    }


    public sealed class CommandInvoker
    {
        private sealed record CommandResult(bool IsSuccess, object? ReturnValue, long ExecutionMilliseconds) : ICommandResult;

        private const string CommandParse = nameof(CommandParse);
        private readonly static EndConfiguration endConfig = new(ShowSuc: true, ItemsBuilder: static view => view.ErrorCount > 0 ? "Execution failed." : "Execution completed.");
        private InlineInfo? errorLineInfo;
        private CommandItem? resultCommandItem = null;
        private object?[] parsedParameters = [];
        private string[] parsedParametersSourceStr = [];
        public string SourceStr { get; init; }
        public CommandRoute Route { get; init; }
        public string RoutedStr { get; private set; } = string.Empty;
        public bool IsCommandValid { get; private set; } = false;
        public bool IsRouteValid => !string.IsNullOrWhiteSpace(RoutedStr);
        public ICommandResult? ExecutionResult { get; private set; } = null;
        public bool Executed => ExecutionResult is not null;
        private static LogProducer Log => field ??= LogProducer.Instance;

        #region builders
        public static CommandInvoker Build(string str) => new(str);
        #region builder.helper
        private static void RouteStr(string str, out string routedStr, out CommandRoute route, out InlineInfo? errorLineInfo)
        {
            const string RouteFailed = $"{CommandParse}:{nameof(RouteFailed)}";
            routedStr = string.Empty;
            route = CommandRoute.Local;
            if (string.IsNullOrWhiteSpace(str))
            {
                errorLineInfo = LogItem.Headered(RouteFailed, "Empty command", LogPaintMode.Error);
                return;
            }
            var firstChar = str[0];
            if (!char.IsLetterOrDigit(firstChar))
            {
                //Parse command type
                switch (firstChar)
                {
                    case '/':
                    case '>':
                        route = CommandRoute.Log;
                        break;
                    case '@':
                    case '!':
                        route = CommandRoute.Cmd;
                        break;
                    default:
                        errorLineInfo = LogItem.Headered(RouteFailed, $"Invalid route info: '{firstChar}'.", LogPaintMode.Error);
                        return;
                }
                //GetSession command str
                var chrPos = 0;
                do
                {
                    if (++chrPos >= str.Length)
                    {
                        errorLineInfo = LogItem.Headered(RouteFailed, $"Found no valid command part starts with letter or digit.", LogPaintMode.Error);
                        return;
                    }
                } while (!char.IsLetterOrDigit(str[chrPos]));
                routedStr = str[chrPos..];
            }
            else routedStr = str;
            errorLineInfo = null;
        }
        #endregion
        #endregion

        #region .ctor
        private CommandInvoker(string sourceStr)
        {
            SourceStr = sourceStr;
            RouteStr(sourceStr, out var routedStr, out var route, out var errorLineInfo);
            RoutedStr = routedStr;
            Route = route;
            this.errorLineInfo = errorLineInfo;
            Parse();
        }
        #endregion

        private void Parse()
        {
            const string ParseFailed = $"{CommandParse}:{nameof(ParseFailed)}";
            if (errorLineInfo is not null || Route != CommandRoute.Local) return;
            //
            var tokens = ParamParserManager.TokenizeCommand(RoutedStr).ToArray();
            if (tokens.Length == 0)
            {
                errorLineInfo = LogItem.Headered(ParseFailed, "Empty command", LogPaintMode.Error);
                return;
            }
            //
            string cataName, itemName;
            string[] args;
            {
                if (tokens[0].Contains('.'))
                {
                    var dotIndex = tokens[0].LastIndexOf('.');
                    cataName = tokens[0][..dotIndex];
                    itemName = tokens[0][(dotIndex + 1)..];
                }
                else
                {
                    cataName = CommandsManager.GlobalCommandCarrierName;
                    itemName = tokens[0];
                }
                args = tokens.Length == 1 ? [] : tokens[1..];
            }
            if (string.IsNullOrEmpty(itemName))
            {
                errorLineInfo = LogItem.Headered(ParseFailed, "Invalid command item name, it can not be empty.", LogPaintMode.Error);
                return;
            }
            //
            var matches = CommandsManager.Instance.Get(cataName, itemName);
            if (matches.Count == 0)
            {
                errorLineInfo = LogItem.Headered(ParseFailed, $"Unknown command: {cataName}.{itemName}", LogPaintMode.Error);
                return;
            }
            //
            var index = 0;
            var parsedParams = new object?[args.Length];
            var parsedParamsSourceStr = new string[args.Length];
            var sucParsedParams = false;
            while (!sucParsedParams)
            {
                var target = matches[index++];
                var parameters = target.MethodInfo.GetParameters();
                var localSuc = parameters.Length == parsedParams.Length;
                if (localSuc)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (!ParamParserManager.Instance.TryConvert(args[i], parameters[i].ParameterType, out parsedParams[i]))
                        {
                            localSuc = false;
                            break;
                        }
                        else parsedParamsSourceStr[i] = args[i];
                    }
                }
                if (!(sucParsedParams = localSuc) && index >= matches.Count)
                {
                    errorLineInfo = LogItem.Headered(ParseFailed, $"Found no overrides with proper parameters for command: {cataName}.{itemName}", LogPaintMode.Error);
                    return;
                }
            }
            parsedParameters = parsedParams;
            parsedParametersSourceStr = parsedParamsSourceStr;
            resultCommandItem = matches[index - 1];
        }

        #region Operations
        public async Task<ICommandResult> ExecuteAsync(long timeoutMilliseconds = -1, SessionGetConfiguration? sessionConfig = null, bool notPrintLines = false)
        {
            var result = await ExecuteInternalAsync(notPrintLines, timeoutMilliseconds, sessionConfig);
            return result;
        }
        
        private async Task<ICommandResult> ExecuteInternalAsync(bool notPrintLines, long timeoutMilliseconds, SessionGetConfiguration? printSessionConfig)
        {
            var session = Log.GetSession(printSessionConfig);
            object? resultValue = null;
            if (errorLineInfo is not null)
            {
                session.Log(errorLineInfo);
                return BuildResult(false, null, 0);
            }
            var sw = Stopwatch.StartNew();
            var localSuc = true;
            LogItem[] items = [];
            switch (Route)
            {
                case CommandRoute.Log:
                    session.Log(RoutedStr);
                    break;
                case CommandRoute.Local:
                    var cataItem = LogItem.Normal($"{resultCommandItem!.VirtualCataName}.", LogPaintMode.Type);
                    var commandItem = LogItem.Normal($"{resultCommandItem!.VirtualItemName} ", LogPaintMode.Method);
                    var parametersItem = LogItem.Normal(parsedParametersSourceStr.BuildString(split: " "), LogPaintMode.Parameter);
                    items = [LogItem.Normal("Executing local command: "), cataItem, commandItem,  parametersItem, LogItem.Normal("...")];
                    using (session.OpenScope(items, notPrintLines)) 
                    {
                        session.ConfigEnd(endConfig);
                        try
                        {
                            resultValue = resultCommandItem.MethodInfo.Invoke(resultCommandItem.OwnerInstance, parsedParameters);
                            if (resultValue is Task task)
                            {
                                await task;
                                var property = task.GetType().GetProperty("Result");
                                if (property is not null) resultValue = property.GetValue(task);
                            }
                        }
                        catch (Exception ex)
                        {
                            session.Error(ex);
                            localSuc = false;
                        }
                    }
                    return BuildResult(localSuc, resultValue, sw.ElapsedMilliseconds);
                case CommandRoute.Cmd:
                    items = [LogItem.Normal("Executing cmd command: \'"), LogItem.Normal(RoutedStr, LogPaintMode.Method), LogItem.Normal("\'...")];
                    using (session.OpenScope(items, notPrintLines))
                    {
                        session.ConfigEnd(endConfig);
                        session.Log($"Pulling cmd console output...");
                        var cmdQueue = new ConcurrentQueue<DataRecevied>();
                        var linesCount = 0;
                        try
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));
                            var execTask = CmdHelper.ExecuteCommandAsync(RoutedStr, cmdQueue, cts.Token);
                            while (!execTask.IsCompleted || !cmdQueue.IsEmpty)
                            {
                                if (cmdQueue.TryDequeue(out var data))
                                {
                                    linesCount++;
                                    if (data.IsError) session.Error(data.Data ?? string.Empty);
                                    else session.Log(data.Data ?? string.Empty);
                                }
                                else await Task.Yield();
                            }
                            resultValue = await execTask;
                        }
                        catch (Exception ex)
                        {
                            session.Error(ex);
                            localSuc = false;
                        }
                        session.Log($"Pulling finished. Total lines: {linesCount}.");
                    }
                    return BuildResult(localSuc, resultValue, sw.ElapsedMilliseconds);
            }
            sw.Stop();
            return BuildResult(errorLineInfo is null, resultValue, sw.ElapsedMilliseconds);
            static ICommandResult BuildResult(bool suc, object? val, long ms)
            {
                var cr = new CommandResult(suc, val, ms);
                return cr;
            }
        }
        #endregion
    }
}
