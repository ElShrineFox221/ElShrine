using ElShrine.EConsole;
using ElShrine.EOption;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Modules.Console.ViewModel
{
    [ModuleRoot(Name = "Console", Version = "2.0", Tags = ["Common", "Console", "Command"], Description = "The advanced console, as implement of the IConsoleListener instead of System.Console.", DataTemplateUri = "/ElShrine.VisualTool;component/Modules/Console/Console.xaml", DataTemplateName = "ConsoleTemplate", DefaultEnabled = true)]
    public sealed class ConsoleVM : ViewModelBase, IConsoleListener, ISingleton<ConsoleVM>
    {
        private ConsoleVM()
        {
            CommandInputBarData = new([])
            {
                RelevantListBoxes = [("InfoLinesContainer", (lb) => LinesListBox = lb)]
            };
        }
        private static ConsoleVM? Instance = null;
        public static ConsoleVM GetInstance() => Instance ??= new();
        public SubConsoleVM SubConsole { get; } = new() { OverrideFilter = (l) => l.InfoLineType != InformationLineType.Normal };

        private ListBox? LinesListBox = null;
        public ObservableCollection<InformationLineVM> InfoLines { get; set; } = [];

        private bool recordTimeVisible = true;
        public bool RecordTimeVisible
        {
            get => recordTimeVisible;
            set
            {
                recordTimeVisible = value;
                NoticePropertyChanged(nameof(RecordTimeVisible));
            }
        }

        public CommandInputBarDataVM CommandInputBarData { get; set; }

        public bool PrintInfoLine(InformationLine line, int listenerIndex)
        {
            var lineVm0 = new InformationLineVM(line);
            var lineVm1 = new InformationLineVM(line);
            Application.Current.Dispatcher.Invoke(() =>
            {
                InfoLines.Add(lineVm0);
                SubConsole.AddInfoLine(lineVm1);
                LinesListBox?.ScrollIntoView(lineVm0);
            });
            return true;
        }
    }
}
