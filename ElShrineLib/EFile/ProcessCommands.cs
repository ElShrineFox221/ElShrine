using ElShrine.ECommand;
using System.Diagnostics;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.EFile
{
    [CommandCarrier(Name = "Process")]
    public static class ProcessCommands
    {
        public static void Launch(string exePath, string arguments, string workingDir, bool adminMode = false)
        {
            if (File.Exists(exePath))
            {
                var extension = Path.GetExtension(exePath);
                if (extension.EqualIgnoreCase(".Lnk") || extension.EqualIgnoreCase(".Exe"))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = true,
                        WorkingDirectory = workingDir.IsNotEmpty() ? workingDir : Path.GetDirectoryName(exePath),
                    };
                    if (adminMode) startInfo.Verb = "runas";
                    Process.Start(startInfo);
                    ListContentInfo("Process started.");
                }
                else throw new("Found no excutable file.");
            }
        }
        public static void Kill(string exePath, bool isLauncherPath = false) => ExitProcess(exePath, isLauncherPath, true);
        public static void Close(string exePath, bool isLauncherPath = false) => ExitProcess(exePath, isLauncherPath, false);
        private static void ExitProcess(string exePath, bool isLauncherPath, bool forceExit)
        {
            if (File.Exists(exePath))
            {
                var extension = Path.GetExtension(exePath);
                if (extension.EqualIgnoreCase(".Lnk") || extension.EqualIgnoreCase(".Exe"))
                {
                    ProcessHelper.TryExitProcess(exePath, isLauncherPath, forceExit);
                }
                else throw new("Found no excutable file.");
            }
        }
    }
}
