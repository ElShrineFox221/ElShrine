using ElShrine.EOption;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.ECommand
{
    [CommandCarrier(Name = "Log")]
    public class LogCommands
    {
        [Command(Description = "Delete all old log files in current log folder.")]
        public static void Clear()
        {
            var dir = CurrentInitializeDirectory;
            string[] ignoreFiles = CurrentInitializePath is null ? [] : [CurrentInitializePath];
            Command.ParseAndExcute($"{nameof(GlobalCommands)}.{nameof(GlobalCommands.ClearDir)} \"{dir}\" [{ignoreFiles.BuildString(f => $"\"{f}\"", CommonHelper.COMMA.ToString())}]", true);
        }
        public static void OpenDir()
            => Command.ParseAndExcute($"{nameof(GlobalCommands)}.{nameof(GlobalCommands.Open)} \"{FileOption.GetInstance().LogDir}\"", true);
    }
}
