using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Log;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LogCommands
{
    private static ILogger Logger => Log.Main;
    private static ILogWriter LogWriter => CoreModuleAccessor.LogWriter;
    private static ILogManager Log => CoreModuleAccessor.Log;

    [Command(Description = "Delete all old logs in log folder.")]
    public static void Clear()
    {
        string ignoreName = LogWriter.LogCurrentFolder;
        CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.ClearDir)} \"{LogWriter.LogBaseDirectory}\" [\"{ignoreName}\"]").ExecuteAsync().Wait();
    }
    [Command]
    public static void OpenDir()
        => CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.Open)} \"{LogWriter.LogCurrentDirectory}\"").ExecuteAsync().Wait();

    /*[Command]
    public static void Reconstruct(string rawLogPath)
    {
        Path.GetFullPath(rawLogPath);
        if (!File.Exists(rawLogPath)) Logger.Error($"File not exists: {rawLogPath}");
        LogStructureRepairer.Reconstruct(rawLogPath);
        Logger.Log($"Reconstructed: {rawLogPath}");
    }*/
    [Command]
    public static void Reconstruct(string rawLogPath)
        => ReconstructInternal(rawLogPath, true);
    [Command]
    public static void Reconstruct()
        => Reconstruct(LogWriter.LogCurrentDirectory);
    internal static void ReconstructInternal(string path, bool doLog)
    {
        var logger = doLog ? Logger : null;
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            try
            {
                LogStructureRepairer.Reconstruct(fullPath);
                logger?.Log($"Reconstructed: {fullPath}");
            }
            catch (Exception ex)
            {
                logger?.Error(ex);
            }
        }
        else if (Directory.Exists(fullPath))
        {
            var files = Directory.GetFiles(fullPath, "*.raw.log", SearchOption.TopDirectoryOnly);
            logger?.Log($"{files.Length} file(s) found in directory '{fullPath}'.");
            foreach (var file in files)
            {
                try
                {
                    LogStructureRepairer.Reconstruct(file);
                    logger?.Log($"Reconstructed: {file}");
                }
                catch (Exception ex)
                {
                    logger?.Error(ex);
                }
            }
        }
        else
            logger?.Error($"Path does not exist: {path}");
    }
}
