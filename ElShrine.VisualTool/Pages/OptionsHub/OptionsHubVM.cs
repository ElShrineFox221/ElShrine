using ElShrine.Modules;
using ElShrine.Modules.Option;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElShrine.VisualTool.Pages.OptionsHub
{
    [InitializationInfo(PreInstantiate = false)]
    [WpfPageRootVM(Name = "Options", Version = "1.0", Tags = ["Common", "Option"], 
        Description = "UI for options, created a visual page for user viewing and changing settings.",
        DataTemplateUri = "/ElShrine.VisualTool;component/Pages/OptionsHub/OptionsHub.xaml", 
        DataTemplateName = "OptionsHubTemplate", DefaultEnabled = true)]
    public sealed class OptionsHubVM : ViewModelBase
    {

        private readonly Dictionary<OptionItem, OptionItemVM> optionItemVMByModel = [];
        private readonly List<OptionItemVM> modifiedOptionItemVMs = [];
        public bool Updated { get; private set; } = false;
        public ObservableCollection<OptionGroupVM> OptionGroups { get; init; } = [];
        private readonly Dictionary<string, OptionGroupVM> OptionGroupByName = [];

        public OptionsHubVM()
        {
            RefreshGroups(false);
            ClassesManager.Instance.AssembliesUpdated += (asbs) => RefreshGroups(true);
            OptionsManager.Instance.OptionChanged += (s, e) =>
            { 
                
            };
        }
        private void RefreshGroups(bool incremental)
        {
            if (!incremental)
            {
                OptionGroups.Clear();
                OptionGroupByName.Clear();
            }
            var om = OptionsManager.Instance;
            var allGroupedItems = om.GetAll();
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var groupedItems in allGroupedItems)
                {
                    var groupItems = groupedItems.Value;
                    var key = groupedItems.Key;
                    var got = OptionGroupByName.TryGetValue(key, out var og);
                    if (!incremental || !got || og is null)
                    {
                        og = new OptionGroupVM(key);
                        OptionGroups.Add(og);
                        OptionGroupByName.Add(key, og);
                    }
                    //items
                    foreach (var item in groupItems)
                    {
                        if (!incremental || !og.Items.Any(i => i.Model == item))
                        {
                            var oivm = new OptionItemVM(item, UpdateValueMethod);
                            optionItemVMByModel[item] = oivm;
                            oivm.RefreshPreviewModeInfo(PreviewEnabled);
                            og.Items.Add(oivm);
                        }
                    }
                }
            });
        }
        private void UpdateValueMethod(OptionItemVM itemVM, object? newValue)
        {
            var oldValue = itemVM.OldValue;
            var index = modifiedOptionItemVMs.FindIndex(i => i == itemVM);
            var isToOldValue = Equals(oldValue, newValue);
            if (index == -1 && !isToOldValue) modifiedOptionItemVMs.Add(itemVM);
            else if(isToOldValue) modifiedOptionItemVMs.Remove(itemVM);
            RefreshModifiedList();
        }
        public bool PreviewEnabled
        {
            get => field;
            set
            {
                if (field == value) return; 
                field = value;
                foreach (var oivm in optionItemVMByModel.Values) oivm.RefreshPreviewModeInfo(PreviewEnabled);
                NotifyPropertiesChanged(nameof(PreviewEnabled));
            }
        }
        public bool IsOptionsChanged
        {
            get => field;
            private set
            {
                if (field == value) return;
                field = value;
                NotifyPropertiesChanged(nameof(IsOptionsChanged));
            }
        }
        public void SaveChanges()
        {
            var list = modifiedOptionItemVMs.ToArray();
            foreach (var oivm in list)
            {
                oivm.Confrim();
            }
            modifiedOptionItemVMs.Clear();
            OptionsManager.Instance.Save();
            RefreshModifiedList();
        }
        public void CancelChanges()
        {
            var list = modifiedOptionItemVMs.ToArray();
            foreach(var oivm in list)
            {
                oivm.Cancel();
            }
            modifiedOptionItemVMs.Clear();
            RefreshModifiedList();
        }
        private void RefreshModifiedList()
        {
            if ((modifiedOptionItemVMs.Count != 0) ^ IsOptionsChanged)
                IsOptionsChanged = modifiedOptionItemVMs.Count != 0;
        }

        public static VMCommand UpdateCommand => new(parameter => Instance.RefreshGroups(false));
        public static VMCommand SaveCommand => new(parameter => Instance.SaveChanges());
        public static VMCommand CancelCommand => new(parameter => Instance.CancelChanges());
        public static VMCommand ResetCommand => new(parameter =>
        {
            OptionsManager.Instance.Reset();
            Instance.RefreshGroups(true);
        });
    }
}
