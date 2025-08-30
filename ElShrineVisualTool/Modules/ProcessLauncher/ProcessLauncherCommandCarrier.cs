using ElShrine.ECommand;
using ElShrine.EConsole;
using ElShrine.EFile;
using ElShrine.Modules.ProcessLauncher.Model;
using ElShrine.Modules.ProcessLauncher.ViewModel;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using static ElShrine.EConsole.ConsoleManager;
using File = System.IO.File;

namespace ElShrine.Modules.ProcessLauncher
{
    [CommandCarrier(Name = "ProcessLauncher")]
    public static class ProcessLauncherCommandCarrier
    {
        private static List<ExeInfo> GetExeFiles()
        {
            var list = new List<ExeInfo>();
            var paths = FileSelector.SelectFilesOrFolder("应用程序|*.exe;*.lnk|可执行文件|*.exe|快捷方式|*.lnk|所有文件|*.*");
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var lnkInfo = new ExeInfo(path);
                    var extension = Path.GetExtension(path);
                    if (extension.EqualIgnoreCase(".Lnk") || extension.EqualIgnoreCase(".Exe"))
                    {
                        LnkReader.ReadLnkFile(path, lnkInfo);
                        list.Add(lnkInfo);
                    }
                }
            }
            return list;
        }
        public static void Import()
        {
            var lnkInfos = GetExeFiles();
            if (lnkInfos.Count == 0) ListWarnInfo([GetWarningItem(), new("Selected no files.", InformationPaintStyle.Normal)]);
            else ListContentInfo($"Selected {lnkInfos.Count} {"files".GetPural(lnkInfos.Count)}.");
            Application.Current.Dispatcher.Invoke(() =>
            {
                var instance = ProcessLauncherVM.GetInstance();
                foreach (var lnkInfo in lnkInfos)
                {
                    instance.AddExeInfo(lnkInfo);
                }
            });
        }
        private static void GenerateLnkFile(string filePath, ExeInfo info)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(filePath);
            shortcut.TargetPath = info.TargetPath;
            shortcut.Arguments = info.Arguments;
            shortcut.WorkingDirectory = info.WorkingDirectory;
            shortcut.Description = info.Description;
            shortcut.IconLocation = $"{info.IconPath},{info.IconIndex}";
            shortcut.Save();

            if (info.RunAsAdmin) setAdminPermission(filePath);
            static void setAdminPermission(string filePath)
            {
                try
                {
                    byte[] content = File.ReadAllBytes(filePath);
                    if (content.Length > 21)
                    {
                        content[21] |= 0x20; // 设置第21字节的0x20标志位
                        File.WriteAllBytes(filePath, content);
                    }
                }
                catch { }
            }
        }
        public static void Export()
        {
            var instance = ProcessLauncherVM.GetInstance();
            var list = instance.SelectedExeInfos.Where(i => i is not null).ToList();
            var canceled = false;
            if (list.Count > 1)
            {
                var dir = FileSelector.SelectFolder();
                if (dir.IsNotEmpty())
                {
                    int count = list.Count;
                    foreach (var info in list)
                    {
                        if(info is not null)
                        {
                            try
                            {
                                var name = info.Name;
                                if (name.IsEmpty()) name = Path.GetFileNameWithoutExtension(info.TargetPath);
                                GenerateLnkFile($"{dir}\\{name}.lnk", info);
                            }
                            catch
                            {
                                count--;
                            }
                        }
                    }
                    ListContentInfo($"{count} lnk {"files".GetPural(count)} export completed.");
                }
                else canceled = true;
            }
            else if(list.Count == 1)
            {
                var dialog = new SaveFileDialog()
                {
                    Title = "Export as lnk files",
                    Filter = "快捷方式|*.lnk",
                    DefaultExt = ".lnk",
                };
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == true && instance.SelectedLastExeInfo is not null)
                {
                    GenerateLnkFile(dialog.FileName, instance.SelectedLastExeInfo.Model);
                    ListContentInfo("Lnk file export completed.");
                }
                else canceled = true;
            }
            else ListWarnInfo([GetWarningItem(), new("Selected no infos to export.", InformationPaintStyle.Normal)]);
            if (canceled) ListWarnInfo([GetWarningItem(), new("Canceled export.", InformationPaintStyle.Normal)]);
        }
    }
}
