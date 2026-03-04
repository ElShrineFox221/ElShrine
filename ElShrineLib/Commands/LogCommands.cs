using ElShrine.Modules;
using ElShrine.Modules.Log;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LogCommands
{
    private static LogSession Session => LogProducer.Instance.CoreSession;
    private static LogProducer LogProducer => LogProducer.Instance;

    [Command(Description = "Delete all old logs in log folder.")]
    public static void Clear()
    {
        string ignoreName = LogProducer.LogPath;
        CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.ClearDir)} \"{LogProducer.LogBasePath}\" [\"{ignoreName}\"]").ExecuteAsync().Wait();
    }
    [Command]
    public static void OpenDir()
        => CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.Open)} \"{LogProducer.LogBasePath}\"").ExecuteAsync().Wait();

    [Command]
    public static void Reconstruct(string rawLogPath)
    {
        Path.GetFullPath(rawLogPath);
        if (!File.Exists(rawLogPath)) Session.Error($"File not exists: {rawLogPath}");
        LogStructureRepairer.Reconstruct(rawLogPath);
        Session.Log($"Reconstructed: {rawLogPath}");
    }
    [Command]
    public static void Reconstruct()
    {
        var path = LogProducer.LogFullPath;
        var files = Directory.GetFiles(path, "*.raw.log");
        Session.Log($"{"file".GetPuralWithNum(files.Length)} found.");
        foreach (var file in files)
        {
            try
            {
                LogStructureRepairer.Reconstruct(file);
                Session.Log($"Reconstructed: {file}");
            }
            catch (Exception ex)
            {
                Session.Error(ex);
            }
        }
    }
}
