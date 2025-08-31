using ElShrine.ECommand;
using ElShrine.EFile;
using ElShrine.VisualTool.ProcessLauncher.EFile;
using ElShrine.VisualTool.ProcessLauncher.Model;
using ElShrine.Wpf.ViewModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool.ProcessLauncher.ViewModel
{
    public class ExeInfoVM(ExeInfo model) : ViewModelBase<ExeInfo>(model)
    {
        #region Basic info
        public bool Valid { get; private set; } = false;
        public bool Activated { get; private set; } = false;
        public string Status { get; private set; } = string.Empty;
        public MediaColor StatusColor { get; private set; } = Color.Transparent.ToMediaColor();
        public string FileName { get; private set; } = string.Empty;
        public ImageSource Icon {  get; private set; } = SystemIcons.Application.ToImageSource();

        #region Source
        private string oldTargetPath = string.Empty;
        public string TargetPath
        {
            get => Model.TargetPath;
            set
            {
                oldTargetPath = Model.TargetPath;
                Model.TargetPath = value;
                UpdateStatus();
            }
        }
        public string Arguments
        {
            get => Model.Arguments;
            set
            {
                Model.Arguments = value;
                NoticePropertyChanged(nameof(Arguments));
            }
        }
        public string Name
        {
            get => Model.Name;
            set
            {
                Model.Name = value;
                NoticePropertyChanged(nameof(Name));
            }
        }
        public string Description
        {
            get => Model.Description;
            set
            {
                Model.Description = value;
                NoticePropertyChanged(nameof(Description));
            }
        }
        public int LaunchModeIndex
        {
            get => (int)Model.LaunchMode;
            set
            {
                Model.LaunchMode = (ExeLaunchMode)(value % 3);
                NoticePropertyChanged(nameof(LaunchModeIndex), nameof(LaunchButtonEnabled), nameof(ExitButtonEnabled));
            }
        }
        public int PermissionIndex
        {
            get => Model.RunAsAdmin ? 0 : 1;
            set
            {
                Model.RunAsAdmin = value == 0;
                NoticePropertyChanged(nameof(PermissionIndex));
            }
        }
        public int MaxRetryCount
        {
            get => Model.MaxRetryCount;
            set
            {
                Model.MaxRetryCount = value;
                NoticePropertyChanged(nameof(MaxRetryCount));
            }
        }
        public string IconPath
        {
            get => Model.IconPath;
            set
            {
                Model.IconPath = value;
                UpdateIcon();
            }
        }
        public int IconIndex
        {
            get => Model.IconIndex;
            set
            {
                Model.IconIndex = value;
                UpdateIcon();
            }
        }
        #endregion

        #endregion
        private int retryCount = 0;
        public int RetryCount
        {
            get => retryCount;
            set
            {
                retryCount = value;
                NoticePropertyChanged(nameof(RetryCount));
            }
        }
        

        #region VM Command
        public bool LaunchButtonEnabled
        {
            get => Valid && Model.LaunchMode == ExeLaunchMode.Once && !Activated;
        }
        public VMC Launch => new(parameter =>
        {
            if (Valid)
            {
                Command.ParseAndExcute($"{nameof(ProcessCommandCarrier)}.{nameof(ProcessCommandCarrier.Launch)} \"{TargetPath}\" \"{Arguments}\" \"{Model.WorkingDirectory}\" {Model.RunAsAdmin}");
            }
        });
        public bool ExitButtonEnabled
        {
            get => Valid && Model.LaunchMode != ExeLaunchMode.Keep && Activated;
        }
        public VMC Close => new(parameter =>
        {
            if (Valid) Command.ParseAndExcute($"{nameof(ProcessCommandCarrier)}.{nameof(ProcessCommandCarrier.Close)} \"{TargetPath}\" true");
        });
        public VMC Kill => new(parameter =>
        {
            if (Valid) Command.ParseAndExcute($"{nameof(ProcessCommandCarrier)}.{nameof(ProcessCommandCarrier.Kill)} \"{TargetPath}\" true");
        });
        #endregion

        private void UpdateIcon()
        {
            Icon = Model.GetExeIcon().ToImageSource();
            NoticePropertyChanged(nameof(IconPath), nameof(IconIndex), nameof(Icon));
        }
        private void UpdateStatus(bool subActivated)
        {
            var pathChanged = oldTargetPath != TargetPath;
            if (pathChanged)
            {
                oldTargetPath = TargetPath;
                Valid = File.Exists(Model.TargetPath) && Path.GetExtension(Model.TargetPath).EqualIgnoreCase(".exe");
                FileName = Path.GetFileName(Model.TargetPath);
                Application.Current.Dispatcher.BeginInvoke(UpdateIcon);
                NoticePropertyChanged(nameof(TargetPath), nameof(FileName), nameof(Valid));
            }
            var activeChanged = subActivated ^ Activated;
            if (pathChanged || activeChanged)
            {
                Activated = subActivated;
                Status = Valid ? Activated ? "Active" : "Deactive" : "Invalid";
                StatusColor = Status switch
                {
                    "Active" => Color.LightGreen.ToMediaColor(),
                    "Invalid" => MediaColor.FromRgb(238, 144, 144),
                    _ => Color.Transparent.ToMediaColor(),
                };
                NoticePropertyChanged(nameof(Activated), nameof(Status), nameof(StatusColor), nameof(LaunchButtonEnabled), nameof(ExitButtonEnabled));
            }
        }
        public void UpdateStatus()
        {
            var subActivated = Valid && ProcessHelper.IsProcessRunning(TargetPath, true);
            UpdateStatus(subActivated);
        }
        private bool autoLaunched = false;
        private bool lastActive = false;
        public void LaunchUpdate(bool subActivated)
        {
            UpdateStatus(subActivated);
            var mode = Model.LaunchMode;
            if (Valid)
            {
                if (Activated)
                {
                    autoLaunched = true;
                    lastActive = true;
                }
                else
                {
                    if (mode == ExeLaunchMode.Auto && !autoLaunched) notIgnoreRetryLaunch();
                    else if (mode == ExeLaunchMode.Keep) notIgnoreRetryLaunch();
                    void notIgnoreRetryLaunch()
                    {
                        if (lastActive) RetryCount = 0;
                        RetryCount++;
                        if (RetryCount > MaxRetryCount) LaunchModeIndex = (int)ExeLaunchMode.Once;
                        else Launch.Execute(null);
                    }
                }
            }
        }
    }
}
