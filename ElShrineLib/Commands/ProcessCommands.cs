using ElShrine.Common;
using ElShrine.Modules;
using System.Diagnostics;

namespace ElShrine.Commands
{
    [CommandCarrier]
    public static class ProcessCommands
    {
        [Command]
        public static void Launch(string exePath, string arguments, string workingDir, bool adminMode = false)
        {
            const string errorMsg = "Executable file not found.";
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
                        WorkingDirectory = !string.IsNullOrWhiteSpace(workingDir) ? workingDir : Path.GetDirectoryName(exePath),
                    };
                    if (adminMode) startInfo.Verb = "runas";
                    Process.Start(startInfo);
                    return;
                }
                throw new(errorMsg);
            }
           throw new(errorMsg);
        }
        [Command]
        public static void Kill(string exePath, bool isLauncherPath = false) => ExitProcess(exePath, isLauncherPath, true);
        [Command]
        public static void Close(string exePath, bool isLauncherPath = false) => ExitProcess(exePath, isLauncherPath, false);
        [Command]
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
