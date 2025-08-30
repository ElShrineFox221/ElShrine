using ElShrine.EOption;
using Microsoft.VisualBasic.FileIO;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.ECommand
{
    [CommandCarrier(Name = "Log")]
    public class LogCommandCarrier
    {
        [Command(Description = "Delete all old log files in current log folder.")]
        public static void Clear()
        {
            var dir = CurrentInitializeDirectory;
            string[] ignoreFiles = CurrentInitializePath is null ? [] : [CurrentInitializePath];
            Command.ParseAndExcute($"{nameof(GlobalCommandCarrier)}.{nameof(GlobalCommandCarrier.ClearDir)} \"{dir}\" [{ignoreFiles.BuildString(f => $"\"{f}\"", CommonHelper.COMMA.ToString())}]", true);
        }
        public static void OpenDir()
            => Command.ParseAndExcute($"{nameof(GlobalCommandCarrier)}.{nameof(GlobalCommandCarrier.Open)} \"{FileOption.GetInstance().LogDir}\"", true);
    }
}
