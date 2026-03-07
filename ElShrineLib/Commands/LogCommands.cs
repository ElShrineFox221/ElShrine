using ElShrine.Modules;
using ElShrine.Modules.Log;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LogCommands
{
    private static ILogger Logger => Log.Main;
    private static ILogWriter LogWriter => MBootstrapper.Resolve<ILogWriter>();
    private static ILoggerManager Log => MBootstrapper.Resolve<ILoggerManager>();

    [Command(Description = "Delete all old logs in log folder.")]
    public static void Clear()
    {
        string ignoreName = LogWriter.LogCurrentFolder;
        CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.ClearDir)} \"{LogWriter.LogBaseDirectory}\" [\"{ignoreName}\"]").ExecuteAsync().Wait();
    }
    [Command]
    public static void OpenDir()
        => CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.Open)} \"{LogWriter.LogCurrentDirectory}\"").ExecuteAsync().Wait();

    [Command]
    public static void Reconstruct(string rawLogPath)
    {
        Path.GetFullPath(rawLogPath);
        if (!File.Exists(rawLogPath)) Logger.Error($"File not exists: {rawLogPath}");
        LogStructureRepairer.Reconstruct(rawLogPath);
        Logger.Log($"Reconstructed: {rawLogPath}");
    }
    [Command]
    public static void Reconstruct()
    {
        var path = LogWriter.LogCurrentDirectory;
        var files = Directory.GetFiles(path, "*.raw.log");
        Logger.Log($"{"file".GetPuralWithNum(files.Length)} found.");
        foreach (var file in files)
        {
            try
            {
                LogStructureRepairer.Reconstruct(file);
                Logger.Log($"Reconstructed: {file}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
