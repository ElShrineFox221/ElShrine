using ElShrine.ETimer;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using static ElShrine.EConsole.ConsoleManager;
using static ElShrine.ECommand.CommandManager;
using ElShrine.EConsole;
using ElShrine.EOption;
using ElShrine.EFile;

namespace ElShrine.ECommand
{
    [CommandCarrier(Name = Const.EmptyStr)]
    public static class GlobalCommandCarrier
    {
        public static void Help()
        {
            //Order
            var sortedCommands = CommandInfos
                .OrderBy(ci => ci.CarrierInfo.OverrideName ?? ci.CarrierInfo.Type.Name)
                .ThenBy(ci => ci.CarrierInfo.Type.Name)
                .ThenBy(ci => ci.Name)
                .ToList();

            ListContentInfo([new($"{CommandInfos.Length} commands as follows:")]);
            var nameCol = sortedCommands.Select(ci => new InformationItem(ci.Name, InformationPaintStyle.ParameterMethod));
            var carrierCol = sortedCommands.Select(ci => new InformationItem(ci.CarrierInfo.Name, InformationPaintStyle.SubComplete));
            var carrierClassCol = sortedCommands.Select(ci => new InformationItem(ci.CarrierInfo.Type.Name, InformationPaintStyle.Addition));
            var usageCol = sortedCommands.Select(ci => new InformationItem(ci.UsageTooltip, InformationPaintStyle.Sub));
            var descriptionCol = sortedCommands.Select(ci => new InformationItem(ci.Description, InformationPaintStyle.Normal));

            ListTableInfo(
                (3, [new("Command", InformationPaintStyle.Warning), .. nameCol]),
                (3, [new("Carrier", InformationPaintStyle.Warning), .. carrierCol]),
                (3, [new("Type", InformationPaintStyle.Warning), .. carrierClassCol]),
                (3, [new("Usage", InformationPaintStyle.Warning), .. usageCol]),
                (3, [new("Description", InformationPaintStyle.Warning), .. descriptionCol])
            );
        }
        public static void Exit()
        {
            NamedTimerManager.SetInterval(() => Environment.Exit(0), 3500, false);
            NamedTimerManager.SetInterval(() =>
            {
                ListInfo(new("The process will be closed in 3 seconds..."));
            }, 500, false);
        }

        public static void Open(string path)
        {
            if (path.IsEmpty()) path = Environment.CurrentDirectory;
            bool suc = false;
            if (File.Exists(path)) 
            {
                var extension = Path.GetExtension(path);
                if (extension.EqualIgnoreCase(".Lnk") || extension.EqualIgnoreCase(".Exe"))
                {
                    Command.ParseAndExcute($"{nameof(ProcessCommandCarrier)}.{nameof(ProcessCommandCarrier.Launch)} \"{path}\" \"\" \"\" false", true);
                    suc = true;
                }
                else
                {

                }
            }
            if (Directory.Exists(path))
            {
                Process.Start(Const.ExplorerName, path);
                ListContentInfo("Directory located.");
                suc = true;
            }
            if (!suc) ListWarnInfo([GetWarningItem(),new(" Matched no directory or file.")]);
        }
        public static void Del(string path)
        {
            bool suc = false;
            //Directly delete files or move to recycle bin.
            var sign = FileOption.GetInstance().DirectlyDel;
            if (File.Exists(path) || Directory.Exists(path))
            {
                suc = true;
                if (sign) File.Delete(path);
                else FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            if (suc) ListContentInfo("Completed delete.");
            else ListWarnInfo([GetWarningItem(), new(" Found no directory or file with the path.")]);
        }
        public static void ClearDir(string dir) => ClearDir(dir, []);
        public static void ClearDir(string dir, string[] ignoreFiles)
        {
            string? failedReason = null;
            if (dir is not null)
            {
                var files = Directory.GetFiles(dir);
                ListContentInfo($"{files.Length} {"file".GetPural(files.Length)} found.");
                //
                var deleteCount = 0;
                //Directly delete files or move to recycle bin.
                var sign = FileOption.GetInstance().DirectlyDel;
                //Delete action
                try
                {
                    foreach (var file in files)
                    {
                        if (!ignoreFiles.Contains(file))
                        {
                            deleteCount++;
                            if (sign) File.Delete(file);
                            else FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedReason = $"Delete failed: {ex.Message}";
                }
                if (deleteCount == 0) failedReason = "No files to delete.";
            }
            if (failedReason is null) ListContentInfo("Completed delete.");
            else ListWarnInfo([GetWarningItem(), new(" Failed delete. "), new(failedReason, InformationPaintStyle.Normal)]);
        }
    }
}
