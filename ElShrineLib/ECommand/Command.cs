using ElShrine.EConsole;
using ElShrine.EException;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.ECommand
{
    public class Command(string commadnStr)
    {
        public string CommandStr { get; protected set; } = commadnStr;

        private class CommandResult : ICommandResult
        {
            public bool Parsed { get; set; } = false;
            public bool Excuted { get; set; } = false;
            public bool HasWarning => Warnings.Length > 0;
            public bool HasError => TerminateException is not null;
            public string[] Warnings { get; set; } = [];
            public Exception? TerminateException { get; set; } = null;
        }
        public static Task<ICommandResult> ParseAndExcute(string commandStr, bool waitResult = false)
        {
            var command = new Command(commandStr);
            var task = Task.Run(command.ParseAndExcuteToResult);
            CommandManager.RunningCommands.Add(task);
            Task.Run(() =>
            {
                task.Wait();
                CommandManager.RunningCommands.Remove(task);
            });
            if (waitResult) task.Wait();
            return task;
        }
        public Task<ICommandResult> ParseAndExcute(bool waitResult = false)
        {
            var task = Task.Run(ParseAndExcuteToResult);
            if (waitResult) task.Wait();
            return task;
        }
        private ICommandResult ParseAndExcuteToResult()
        {
            CommandResult result = new();
            ListBeginInfo(BuildParseItems(CommandStr));
            try
            {
                Parse();
                result.Parsed = true;
            }
            catch (Exception e)
            {
                result.TerminateException = e;
                ListErrorInfo(e);
            }
            //
            if (result.Parsed)
            {
                try
                {
                    Excute();
                    result.Excuted = true;
                }
                catch (Exception e)
                {
                    result.TerminateException = e;
                    ListErrorInfo(e);
                }
            }
            var consoleResult = GetListInfoListener();
            result.Warnings = [.. consoleResult.Warnings];
            ListEndInfo(BuildEndItems(result));

            return result;
            static InformationItem[] BuildParseItems(string commandStr)
                => [new($"{CommonHelper.ArrowIntent(1)}Begin to parse and excute command ["), new(commandStr, InformationPaintStyle.ParameterMethod), new("]")];
            static InformationItem[] BuildEndItems(ICommandResult result)
            {
                InformationItem parsedItem = GetCompleteItem(result.Parsed);
                InformationItem excutedItem = GetCompleteItem(result.Excuted);
                InformationItem? warningItem = result.HasWarning ? GetWarningItem() : null;
                string summary = "Completed.";
                if (result.TerminateException is not null)
                {
                    string exceptionName = result.TerminateException.GetType().Name;
                    if (result.Parsed) summary = $"Teminated in Excute[{exceptionName}]";
                    else summary = $"Teminated in Parse[{exceptionName}]";
                }
                InformationItem summaryItem = new(summary, InformationPaintStyle.Sub);
                InformationItem[] lineItems = warningItem is not null ? [parsedItem, excutedItem, warningItem, summaryItem] : [parsedItem, excutedItem, summaryItem];
                return lineItems;
            }
        }

        public ICommandInfo? Info;
        public object?[]? Parameters { get; protected set; } = null;

        private void Parse()
        {
            var commandStr = CommandStr;
            string commandSection, paramSection;
            //Split instruction section and parameters section
            if (!string.IsNullOrWhiteSpace(commandStr))
            {  
                int firstSpaceIndex = commandStr.IndexOf(' ');
                //
                commandSection = firstSpaceIndex >= 0 ? commandStr[..firstSpaceIndex] : commandStr;
                validateCommandSection(commandSection);
                //
                paramSection = firstSpaceIndex >= 0 ? commandStr[(firstSpaceIndex + 1)..] : string.Empty;
            }
            else throw new ArgumentException("Command cannot be null or whitespace", nameof(commandStr));
            //Search command info by carrier and method name.
            var lastDotIndex = commandSection.LastIndexOf('.');
            var carrierName = lastDotIndex >= 0 ? commandSection[..lastDotIndex] : string.Empty;
            var instructionName = lastDotIndex >= 0 ? commandSection[(lastDotIndex + 1)..] : commandSection;
            var commandInfos = CommandManager.FindCommandInfo(carrierName, instructionName);
            if (commandInfos.Count <= 0) throw new CommandNoFoundException(carrierName, instructionName);
            //Parameters parse
            List<string> tokens = [];
            var matchedCommandInfoIndex = -1;
            for (int i = 0; i < commandInfos.Count; i++)
            {
                bool suc = true;
                var commandInfo = commandInfos[i];
                try
                {
                    tokens = commandInfo.Tokenize(paramSection, CommonHelper.SPACE);
                    suc = commandInfo.Parameters.Length == tokens.Count;
                }
                catch
                {
                    suc = false;
                }
                if (suc)
                {
                    matchedCommandInfoIndex = i;
                    break;
                }
            }
            Info = matchedCommandInfoIndex != -1?commandInfos[matchedCommandInfoIndex] : throw new CommandNoFoundException(carrierName, instructionName, -2);
            Parameters = [.. Info.ParseParams(tokens)];
            //
            static void validateCommandSection(string commandSection)
            {
                if (commandSection.StartsWith('.') || commandSection.EndsWith('.'))
                    throw new FormatException("Command segment cannot start or end with dot");
                if (commandSection.Contains(".."))
                    throw new FormatException("Consecutive dots are not allowed");
                string[] parts = commandSection.Split('.');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(part)) throw new FormatException("Empty command segment part");
                    if (char.IsDigit(part[0])) throw new FormatException($"Command part '{part}' cannot start with digit");
                    if (!part.All(c => char.IsLetterOrDigit(c) || c == '.')) throw new FormatException($"Invalid character in command part '{part}'");
                }
            }
        }
        private void Excute()
        {
            if (Info is null) throw new ArgumentNullException(nameof(Info));
            else Info?.MethodInfo.Invoke(null, Parameters);
        }
    }
}
