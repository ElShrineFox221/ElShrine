using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Options;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace ElShrine.Commands
{
    [CommandCarrier]
    public static class GlobalCommands
    {
        private static LogSession Session => LogProducer.Instance.CoreSession;
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
            void addLine(params LogItem?[] rowItems) => AddColsRow(cols, LogItem.Empty, rowItems);

            var total = 0;
            addLine(
                LogItem.Normal("Category", LogPaintMode.Method),
                LogItem.Normal("Item", LogPaintMode.Method),
                LogItem.Normal("Parameters", LogPaintMode.Method),
                LogItem.Normal("Description", LogPaintMode.Method),
                LogItem.Normal("Implementation", LogPaintMode.Method));
            var allItems = CommandsManager.Instance.GetAll();
            if (groupedByCata)
            {
                var sortedGroups = allItems.GroupBy(static ci => ci.VirtualCataName)
                    .OrderBy(static g => g.Key);
                foreach (var group in sortedGroups)
                {
                    var count = group.Count();
                    if (count == 0) continue;
                    total += count;
                    addLine(
                        LogItem.Normal($"[{group.Key}]", LogPaintMode.Keyword),
                        null,
                        LogItem.Normal("command".GetPuralWithNum(count), LogPaintMode.Normal));
                    var sortedGroup = group.OrderBy(static ci => ci.ActualCataName)
                        .ThenBy(static ci => ci.VirtualItemName)
                        .ThenBy(static ci => ci.ActualItemName);
                    foreach (var ci in sortedGroup)
                    {
                        addLine(
                            LogItem.Normal(ci.VirtualCataName, LogPaintMode.Type),
                            LogItem.Normal(ci.VirtualItemName, LogPaintMode.Method),
                            LogItem.Normal(CIToParametersText(ci), LogPaintMode.Parameter),
                            LogItem.Normal(ci.Description, LogPaintMode.Normal),
                            LogItem.Normal(ci.ToFullName(), LogPaintMode.NormalTip)); 
                    }
                }
            }
            else
            {
                total = allItems.Count;
                var sortedCis = allItems.OrderBy(static ci => ci.VirtualCataName)
                    .ThenBy(static ci => ci.ActualCataName)
                    .ThenBy(static ci => ci.VirtualItemName)
                    .ThenBy(static ci => ci.ActualItemName);
                foreach (var ci in sortedCis)
                {
                    addLine(
                        LogItem.Normal(ci.VirtualCataName, LogPaintMode.Type),
                        LogItem.Normal(ci.VirtualItemName, LogPaintMode.Method),
                        LogItem.Normal(CIToParametersText(ci), LogPaintMode.Parameter),
                        LogItem.Normal(ci.Description, LogPaintMode.Normal),
                        LogItem.Normal(ci.ToFullName(), LogPaintMode.NormalTip));
                }
            }

            Session.Table(4, [.. cataCol], [.. itemCol], [.. paramCol], [.. descCol], [.. implCol]);
            Session.Log($"There are {"command".GetPuralWithNum(total)} in total:");
        }
        [Command] public static void Help() => Help(true);
        #endregion

        #region Exit
        [Command]
        public async static Task Exit(bool force)
        {
            if (!force)
            {
                Session.HeaderedWarning("The process will be closed in 3 seconds if there are no more waiting tasks.", "ProcessClose");
                await Task.Delay(3000);
            }
            while (true)
            {
                //LOG if (!force && !Session.NoLinesToConsume) await Task.Yield();
                Bootstrapper.Exit();
            }
        }
        [Command] public async static Task Exit() => await Exit(false);
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
                        Session.Warning($"Failed to open file: {ex.Message}");
                    }
                }
            }
            if (Directory.Exists(path))
            {
                Process.Start(Const.ExplorerName, path);
                Session.Log("Directory located.");
                suc = true;
            }
            if (!suc) Session.Warning("Exist no directory or file.");
        }
        [Command]
        public static void Del(string path)
        {
            bool suc = false;
            var sign = FileOption.DirectlyDel;
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
                    Session.Warning($"Delete action failed: {ex.Message}");
                    return;
                }
            }
            if (suc) Session.Log("Completed delete.");
            else Session.Warning("Found no directory or file with the path.");
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
                var ignoreSet = ignoreNames.Select(name => Path.GetFullPath(Path.Combine(dir, name)))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                int fileDelCount = 0, dirDelCount = 0;
                int fileIgnCount = 0, dirIgnCount = 0;
                int totalFound = entries.Length;
                Session.Log($"{totalFound} {"item".GetPural(totalFound)} found in directory.");
                var sign = FileOption.DirectlyDel;
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
                    Session.Log(msg);
                }
            }
            else failedReason = "Directory does not exist.";
            if (failedReason is not null) Session.Warning($"Failed delete. {failedReason}");
        }
    }
}
