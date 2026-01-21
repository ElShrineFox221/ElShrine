using System.Collections.Concurrent;
using System.Diagnostics;

namespace ElShrine.Common
{
    public static class ProcessHelper
    {
        private static readonly ConcurrentDictionary<int, string> _pathCache = new();
        public static (Process? launcher, Process[] mainResults) GetProcess(string exePath, bool isLauncherPath = false)
        {
            (Process? launcher, Process[] mainResults) = (null, []);
            var processes = Process.GetProcesses();
            if (!isLauncherPath)
            {
                var resultProcesses = processes.AsParallel().Where(p => GetProcessPath(p).EqualIgnoreCase(exePath));
                launcher = resultProcesses.FirstOrDefault();
            }
            else
            {
                ConcurrentBag<Process> candidateProcesses = [];
                var exeDir = Path.GetDirectoryName(exePath);
                Parallel.ForEach(Process.GetProcesses(), process =>
                {
                    try
                    {
                        string processPath = GetProcessPath(process);
                        if (string.IsNullOrWhiteSpace(processPath)) return;
                        if (processPath.EqualIgnoreCase(exePath)) launcher = process;
                        else
                        {
                            var processDir = Path.GetDirectoryName(processPath);
                            //if (processDir.NullableEqualIgnoreCase(exeDir)) candidateProcesses.Add(process);
                            if (processDir?.Contains(exeDir ?? throw new("Failed to get directory.")) ?? false) candidateProcesses.Add(process);
                        }
                    }
                    catch { }
                });
                string fileNamePattern = GetFileNamePattern(exePath);
                int minDistance = int.MaxValue;
                Process? matchedProcess = null;
                foreach (var candidate in candidateProcesses)
                {
                    string candidateName = Path.GetFileNameWithoutExtension(GetProcessPath(candidate));
                    int distance = candidateName.GetDeletionDistance(fileNamePattern);
                    if (distance < minDistance || distance == minDistance && candidate.Id < matchedProcess?.Id)
                    {
                        minDistance = distance;
                        matchedProcess = candidate;
                    }
                }
                if (matchedProcess is not null) mainResults = [matchedProcess];
            }
            return (launcher, mainResults);
        }
        public static (Process? launcher, Process[] mainResults)[] GetProcesses(string?[] exePaths, bool isLauncherPath = false)
        {
            var processes = Process.GetProcesses();
            var resultPairs = new (Process? launcher, Process[] mainResults)[exePaths.Length];

            if (!isLauncherPath)
            {
                Parallel.ForEach(exePaths, (exePath, _, index) =>
                {
                    if(exePath is not null)
                    {
                        var singleLauncherResult = processes.AsParallel().Where(p => GetProcessPath(p).EqualIgnoreCase(exePath));
                        if (singleLauncherResult.Count() > 0) resultPairs[index].launcher = singleLauncherResult.First();
                    }
                });
            }
            else
            {
                Parallel.ForEach(exePaths, (exePath, _, index) =>
                {
                    resultPairs[index].mainResults = [];
                    if (exePath is not null)
                    {
                        var pattern = GetFileNamePattern(exePath);
                        var exeDir = Path.GetDirectoryName(exePath);
                        var candidateProcesses = processes.AsParallel().Where(p => Path.GetDirectoryName(GetProcessPath(p))?.Contains(exeDir ?? throw new("Failed to get directory.")) ?? false);
                        //var candidateProcesses = processes.AsParallel().Where(p => Path.GetDirectoryName(GetProcessPath(p)).NullableEqualIgnoreCase(exeDir));
                        int min = int.MaxValue; Process? matched = null;
                        foreach (var process in candidateProcesses)
                        {
                            int dis = Path.GetFileNameWithoutExtension(GetProcessPath(process)).GetDeletionDistance(pattern);
                            if (dis < min)
                            {
                                min = dis;
                                matched = process;
                            }
                        }
                        if (matched != null) resultPairs[index].mainResults = [matched];
                    }
                });
            }
            return [.. resultPairs];
        }
        private static string GetProcessPath(Process process)
        {
            return _pathCache.GetOrAdd(process.Id, id =>
            {
                try
                {
                    return process.MainModule?.FileName ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            });
        }
        private readonly static List<string> patterns = ["launcher", "launch", "starter", "start", "setup", "bootstrapper"];
        private static string GetFileNamePattern(string exePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(exePath);
            var result = fileName;
            foreach (var pattern in patterns)
            {
                int index = result.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index != -1) result = result.Remove(index, pattern.Length);
            }
            return result;
        }
        private static bool IsNotNull(this (Process? launcher, Process[] mainResults) value)
            => value.launcher is not null || value.mainResults.Length > 0;
        public static bool IsProcessRunning(string exePath, bool isLauncherPath = false)
            => GetProcess(exePath, isLauncherPath).IsNotNull();
        public static bool[] IsProcessesRunning(string?[] exePaths, bool isLauncherPath = false)
            => [..GetProcesses(exePaths, isLauncherPath).Select(r => r.IsNotNull())];
        public static bool TryExitProcess(string exePath, bool isLauncherPath, bool forceExitWindows)
        {
            var (launcher, mainResults) = GetProcess(exePath, isLauncherPath);
            Process?[] processes = [launcher, ..mainResults];

            var uiProcesses = new List<Process>();
            var nonUiProcesses = new List<Process>();
            foreach (var process in processes)
            {
                try
                {
                    if (process is null || process.HasExited) continue;
                    if (process.MainWindowHandle != nint.Zero) uiProcesses.Add(process);
                    else nonUiProcesses.Add(process);
                }
                catch { }
            }

            bool allExited = true;
            allExited &= TryExitProcesses(uiProcesses, forceExitWindows);
            allExited &= TryExitProcesses(nonUiProcesses, true);
            return allExited;
        }
        public static bool TryExitProcesses(List<Process> processes, bool forceClose = true)
        {
            if (processes.Count == 0) return true;
            bool groupExited = true;
            var exitTasks = new List<Task>();
            foreach (var process in processes)
            {
                exitTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (process.HasExited) return;
                        if (!forceClose && process.MainWindowHandle != nint.Zero)
                        {
                            process.CloseMainWindow();
                            return;
                        }
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit();
                            return;
                        }
                    }
                    catch { groupExited = false; }
                }));
            }
            try
            {
                Task.WaitAll([.. exitTasks]);
            }
            catch (AggregateException)
            {
                //ListWarnInfo([GetWarningItem(), new("Aggregated wait.")]);
            }

            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited) groupExited = false;
                }
                catch
                {
                    groupExited = false;
                }
            }
            return groupExited;
        }
    }
}
