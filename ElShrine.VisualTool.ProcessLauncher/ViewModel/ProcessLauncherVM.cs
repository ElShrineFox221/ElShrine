using ElShrine.ECommand;
using ElShrine.EFile;
using ElShrine.EOption;
using ElShrine.ETimer;
using ElShrine.VisualTool.ProcessLauncher.Model;
using ElShrine.Wpf;
using System.Collections;
using System.Collections.ObjectModel;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool.ProcessLauncher.ViewModel
{
    [WpfPageRootVM(Name = "Process Launcher", Version = "1.0", Tags = ["Common", "Process"], Description = "Manager of softwares, Import .lnk and .exe files to create software info.", DataTemplateUri = "/ElShrine.VisualTool.ProcessLauncher;component/ProcessLauncher.xaml", DataTemplateName = "ProcessLauncherTemplate")]
    [StartupClass]
    public sealed class ProcessLauncherVM : ViewModelBase, ISingleton<ProcessLauncherVM>
    {
        private ProcessLauncherVM() : base() { }
        private static ProcessLauncherVM? Instance = null;
        public static ProcessLauncherVM GetInstance() => Instance ??= new();

        #region Data
        public ObservableCollection<ExeInfoVM> ExeInfos { get; set; } = [];
        public void AddExeInfo(ExeInfo info)
        {
            if (!ExeInfos.Any(i => i.TargetPath == info.TargetPath)) ExeInfos.Add(new(info));
        }
        private class ExecutableFiles : List<ExeInfo> { }
        public void Save()
        {
            try
            {
                ExecutableFiles exeInfos = [.. ExeInfos.Select(i => i.Model)];
                var result = DataHandler.Write(exeInfos);
            }
            catch
            {
                ActionNotice = "Save failed.";
            }
        }
        public void Load()
        {
            try
            {
                var result = DataHandler.Read<ExecutableFiles>();
                ExeInfos.Clear();
                if (result.Success && result.Data is not null)
                {
                    foreach (var item in result.Data)
                    {
                        AddExeInfo(item);
                    }
                }
            }
            catch
            {
                ActionNotice = "Load Failed.";
            }
        }
        #endregion

        private NamedTimer? UpdateStatusTimer { get; set; }
        protected override void Initialize()
        {
            Load();
            UpdateStatusTimer = NamedTimerManager.CreateTimer(2000, true, "ProcessStatusListener");
            UpdateStatusTimer.Elapsed += (_, _) =>
            {
                var paths = ExeInfos.AsParallel().Select(info => info.Valid ? info.TargetPath : null).ToArray();
                //var acs = ProcessHelper.GetProcesses(paths, true).Select(r => r.mainResults.Length > 0 || r.launcher is not null).ToArray();
                var acs = ProcessHelper.IsProcessesRunning(paths, true);
                for (int i = 0; i < ExeInfos.Count; i++)
                {
                    ExeInfos[i].LaunchUpdate(acs[i]);
                }
            };
            UpdateStatusTimer.Start();
        }
        public string ActionNotice = string.Empty;

        #region Selection
        private readonly List<object?> selectedItems = [];
        public IList? SelectedItems
        {
            get => selectedItems;
            set
            {
                if(value is not null)
                {
                    selectedItems.Clear();
                    foreach (var i in value) selectedItems.Add(i);
                    NoticePropertyChanged(nameof(SelectedItems), nameof(SelectedExeInfos), nameof(SelectedLastExeInfo));
                }
            }
        }
        public List<ExeInfo?> SelectedExeInfos => [.. selectedItems.Select(i => (i as ExeInfoVM)?.Model)];
        public ExeInfoVM? SelectedLastExeInfo => SelectedItems is not null && SelectedItems.Count > 0 ? SelectedItems[^1] as ExeInfoVM : null;
        #endregion

        #region VM Commands
        public VMC AddInfo => new(parameter =>
        {
            ExeInfos.Add(new(new(string.Empty)));
        });
        public VMC DeleteInfo => new(parameter =>
        {
            if(parameter is ExeInfoVM eivm)
            {
                ExeInfos.Remove(eivm);
            }
            
        });
        
        public VMC ImportInfo => new(parameter =>
        {
            Command.ParseAndExcute($"{nameof(ProcessLauncherCommandCarrier)}.{nameof(ProcessLauncherCommandCarrier.Import)}");
        });
        public VMC ExportInfo => new(parameter =>
        {
            Command.ParseAndExcute($"{nameof(ProcessLauncherCommandCarrier)}.{nameof(ProcessLauncherCommandCarrier.Export)}");
        });

        public VMC LoadList => new(parameter =>
        {
            Load();
        });
        public VMC SaveList => new(parameter =>
        {
            Save();
        });
        #endregion
    }
}
