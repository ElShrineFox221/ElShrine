using ElShrine.EOption;
using ElShrine.Wpf;
using ElShrine.Wpf.Controls;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElShrine.VisualTool.OptionHub
{
    [WpfPageRootVM(Name = "Option Hub", Version = "1.0", Tags = ["Common", "Option"], Description = "UI for options, created a visual page for user viewing and changing settings.", DataTemplateUri = "/ElShrine.VisualTool.OptionHub;component/OptionHub.xaml", DataTemplateName = "OptionHubTemplate")]
    public sealed class OptionHubVM : ViewModelBase, ISingleton<OptionHubVM>
    {
        private static OptionHubVM? Instance = null;
        public static OptionHubVM GetInstance() => Instance ??= new();
        private OptionHubVM() : base() { }
        public ObservableCollection<OptionGroup> OptionGroups { get; } = [];

        private bool previewEnabled = false;
        public bool PreviewEnabled
        {
            get => previewEnabled;
            set
            {
                previewEnabled = value;
                NoticePropertyChanged(nameof(PreviewEnabled));
            }
        }

        private record ValueChangeRecord(object? Old, object? New, ControlDataUpdates UpdateMode);
        private readonly Dictionary<OptionItem, ValueChangeRecord> optionItemRecords = [];
        public bool IsOptionsChanged => optionItemRecords.Where(v => v.Value.New is not null).Any();
        public void Update()
        {
            //get option items
            var instance = OptionManager.GetInstance();
            instance.RefreshOptionItemsCache();
            var items = instance.OptionItemsCache;
            var groups = items.OrderBy(i => i.ItemName).GroupBy(getGroupName).Select(g => new OptionGroup(g.Key, [.. g], g.Select(i => i.ClassName).Distinct(StringComparer.OrdinalIgnoreCase).Count())).OrderBy(g => g.GroupName);
            static string getGroupName(OptionItem item)
                => item.ClassOverrideName is null ? item.ClassName : item.ClassOverrideName.IsEmpty() ? "Global" : item.ClassOverrideName;

            //update changes records
            optionItemRecords.Clear();
            foreach (var item in items) optionItemRecords.Add(item, new(item.Value, null, ControlDataUpdates.Update));
            NoticePropertyChanged(nameof(IsOptionsChanged));

            //update view
            OptionHubView.ClearViewCache();
            Application.Current.Dispatcher.BeginInvoke(() => OptionGroups.ReplaceAll(groups));
        }
        public void SaveChanges()
        {
            //
            //Saves
            //
            var changes = optionItemRecords.Where(i => i.Value.New is not null);
            foreach (var item in changes) optionItemRecords[item.Key] = new(item.Value.Old, null, ControlDataUpdates.Confrim);
        }
        public void CancelChanges()
        {
            //
            //Cancel
            //
            var changes = optionItemRecords.Where(i => i.Value.New is not null);
            foreach (var item in changes) optionItemRecords[item.Key] = new(item.Value.Old, null, ControlDataUpdates.Cancel);
        }
        public void RecordOptionChange(OptionItem option, object? newValue)
        {
            optionItemRecords[option] = optionItemRecords[option] with { New = newValue };
            NoticePropertyChanged(nameof(IsOptionsChanged));
        }

        public static VMCommand UpdateCommand => new(parameter => GetInstance().Update());
        public static VMCommand SaveCommand => new(parameter => GetInstance().SaveChanges());
        public static VMCommand CancelCommand => new(parameter => GetInstance().CancelChanges());
        public static VMCommand RecordChangeCommand => new(parameter =>
        {
            if (parameter is OptionItem option)
            {
                object? newValue = option.Value;
                GetInstance().RecordOptionChange(option, newValue);
            }
        });

        public sealed record OptionGroup(string GroupName, List<OptionItem> Items, int ClassesCount) { }
    }
}
