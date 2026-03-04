using ElShrine.Wpf;
using System.Collections.ObjectModel;

namespace ElShrine.VisualTool.Pages.OptionsHub
{
    public sealed class OptionGroupVM : ViewModelBase
    {
        public string GroupName { get; init; }
        public string GroupShortName { get; init; }
        public int ItemsCount => Items.Count;
        public ObservableCollection<OptionItemVM> Items { get; init; } = [];

        public OptionGroupVM(string groupName)
        {
            GroupName = groupName;
            GroupShortName = groupName.RemovePartsIgnoreCase("Options", "Option");
            Items.CollectionChanged += (s, e) => NotifyPropertiesChanged(nameof(ItemsCount));
        }
    }
}
