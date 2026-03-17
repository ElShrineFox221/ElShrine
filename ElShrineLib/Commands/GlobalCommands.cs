using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Options;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace ElShrine.Commands;

[CommandCarrier]
public static class GlobalCommands
{
    private static ILogger Logger => field ??= CoreModuleAccessor.Log.Main;
    private static IOptionManager Option => field ??= CoreModuleAccessor.Option;
    private static void AddColsRow<T>(List<T>[] cols, Func<T> defaultFactory, params T?[] rowItems)
    {
        for (var i = 0; i < cols.Length; i++)
        {
            cols[i].Add(i >= rowItems.Length || rowItems[i] is null ? defaultFactory() : rowItems[i]!);
        }
    }
    private static string CIToParametersText(CommandItem ci)
        => ci.MethodInfo.GetParameters().BuildString(p => $"{p.ParameterType.Name} {p.Name}");

    #region Help
    [Command]
    public static void Help(bool groupedByCata)
    {
        List<LogItem> cataCol = [], itemCol = [], paramCol = [], descCol = [], implCol = [];
        List<LogItem>[] cols = [cataCol, itemCol, paramCol, descCol, implCol];
        void addLine(params LogItem[] rowItems) => AddColsRow(cols, LogItem.Empty, rowItems);

        var total = 0;
        addLine(
            LogItem.Normal("Category", LogItemStyle.NoticeDarkYellow),
            LogItem.Normal("Item", LogItemStyle.NoticeDarkYellow),
            LogItem.Normal("Parameters", LogItemStyle.NoticeDarkYellow),
            LogItem.Normal("Description", LogItemStyle.NoticeDarkYellow),
            LogItem.Normal("Implementation", LogItemStyle.NoticeDarkYellow));
        var allItems = CoreModuleAccessor.Command.GetAll().OrderBy(i => i.Key.CataName);
        if (groupedByCata)
        {
            foreach (var (cata, items) in allItems)
            {
                var count = cata.ItemsCount;
                if (count == 0) continue;
                total += count;
                addLine(
                    LogItem.Normal($"[{cata.CataName}]", LogItemStyle.NoticePurple),
                    LogItem.Empty(),
                    LogItem.Normal("command".GetPuralWithNum(count), LogItemStyle.Info));
                var sortedGroup = items.OrderBy(static ci => ci.ActualCataName)
                    .ThenBy(static ci => ci.VirtualItemName)
                    .ThenBy(static ci => ci.ActualItemName);
                foreach (var ci in sortedGroup)
                {
                    addLine(
                        LogItem.Normal(ci.VirtualCataName, LogItemStyle.NoticePaleGreen),
                        LogItem.Normal(ci.VirtualItemName, LogItemStyle.NoticeDarkYellow),
                        LogItem.Normal(CIToParametersText(ci), LogItemStyle.NoticeCyan),
                        LogItem.Normal(ci.Description, LogItemStyle.Info),
                        LogItem.Normal(ci.ToFullName(), LogItemStyle.SubInfo)); 
                }
            }
        }
        else
        {
            var items = allItems.SelectMany(static i => i.Value);
            total = allItems.Sum(static i => i.Key.ItemsCount);
            var sortedCis = items.OrderBy(static ci => ci.VirtualCataName)
                .ThenBy(static ci => ci.ActualCataName)
                .ThenBy(static ci => ci.VirtualItemName)
                .ThenBy(static ci => ci.ActualItemName);
            foreach (var ci in sortedCis)
            {
                addLine(
                    LogItem.Normal(ci.VirtualCataName, LogItemStyle.NoticePaleGreen),
                    LogItem.Normal(ci.VirtualItemName, LogItemStyle.NoticeDarkYellow),
                    LogItem.Normal(CIToParametersText(ci), LogItemStyle.NoticeCyan),
                    LogItem.Normal(ci.Description, LogItemStyle.Info),
                    LogItem.Normal(ci.ToFullName(), LogItemStyle.SubInfo));
            }
        }
        EntryContent.BuildTable(LogItem.Normal($"There are {"command".GetPuralWithNum(total)} in total:"), out var entry, 4, [.. cataCol], [.. itemCol], [.. paramCol], [.. descCol], [.. implCol]);
        if (entry is not null) Logger.Log(entry);
    }
    [Command] public static void Help() => Help(true);
    #endregion

    #region Exit
    [Command]
    public static void Exit(bool force) => Bootstrapper.Exit(force);
    [Command] public static void Exit() => Exit(false);
    #endregion

    [Command]
    public static void Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) path = Environment.CurrentDirectory;
        bool suc = false;
        if (File.Exists(path))
        {
            var extension = Path.GetExtension(path);
            if (extension.EqualIgnoreCase(".Lnk") || extension.EqualIgnoreCase(".Exe"))
            {
                var cmdStr = $"{nameof(ProcessCommands)}.{nameof(ProcessCommands.Launch)} \"{path}\" \"\" \"\" false";
                var cmdInvoker = CommandInvoker.Build(cmdStr);
                cmdInvoker.ExecuteAsync().Wait();
                suc = true;
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    suc = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to open file: {ex.Message}");
                }
            }
        }
        if (Directory.Exists(path))
        {
            Process.Start(Const.ExplorerName, $"\"{path}\"");
            Logger.Log("Directory located.");
            suc = true;
        }
        if (!suc) Logger.Warning("Exist no directory or file.");
    }
    [Command]
    public static void Del(string path)
    {
        bool suc = false;
        var sign = Option.GetOption<CommonOption>().DirectlyDel;
        if (File.Exists(path) || Directory.Exists(path))
        {
            try
            {
                var attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (sign) Directory.Delete(path, recursive: true);
                    else FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                else
                {
                    if (sign) File.Delete(path);
                    else FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                suc = true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Delete action failed: {ex.Message}");
                return;
            }
        }
        if (suc) Logger.Log("Completed delete.");
        else Logger.Warning("Found no directory or file with the path.");
    }
    [Command]
    public static void ClearDir(string dir) => ClearDir(dir, []);
    [Command]
    public static void ClearDir(string dir, string[] ignoreNames)
    {
        string? failedReason = null;
        if (dir is not null && Directory.Exists(dir))
        {
            var entries = Directory.GetFileSystemEntries(dir);
            var ignoreSet = ignoreNames.Select(name => Path.GetFullPath(Path.Combine(dir, name.RemovePartsIgnoreCase("\""))))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            int fileDelCount = 0, dirDelCount = 0;
            int fileIgnCount = 0, dirIgnCount = 0;
            int totalFound = entries.Length;
            Logger.Log($"{totalFound} {"item".GetPural(totalFound)} found in directory.");
            var sign = Option.GetOption<CommonOption>().DirectlyDel;
            try
            {
                foreach (var entry in entries)
                {
                    var fullPath = Path.GetFullPath(entry);
                    var isDir = File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
                    if (ignoreSet.Contains(fullPath))
                    {
                        if (isDir) dirIgnCount++;
                        else fileIgnCount++;
                        continue;
                    }
                    if (isDir)
                    {
                        if (sign) Directory.Delete(fullPath, recursive: true);
                        else FileSystem.DeleteDirectory(fullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        dirDelCount++;
                    }
                    else
                    {
                        if (sign) File.Delete(fullPath);
                        else FileSystem.DeleteFile(fullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        fileDelCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                failedReason = $"Delete process interrupted: {ex.Message}";
            }
            if (fileDelCount == 0 && dirDelCount == 0 && fileIgnCount == 0 && dirIgnCount == 0) failedReason ??= "Directory is already empty.";
            else
            {
                var msg = $"Deleted: {fileDelCount} {"file".GetPural(fileDelCount)}, {dirDelCount} {"folder".GetPural(dirDelCount)}.";
                if (fileIgnCount > 0 || dirIgnCount > 0)
                {
                    msg += $" Ignored: {fileIgnCount} file(s), {dirIgnCount} folder(s).";
                }
                Logger.Log(msg);
            }
        }
        else failedReason = "Directory does not exist.";
        if (failedReason is not null) Logger.Warning($"Failed delete. {failedReason}");
    }
}
