using ElShrine.EConsole;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;

namespace ElShrine.VisualTool.Modules.Console.ViewModel
{
    public class InformationLineVM(InformationLine model) : ViewModelBase<InformationLine>(model)
    {
        private bool ignoreWarp = false;
        public bool IgnoreWarp
        {
            get => ignoreWarp;
            set
            {
                ignoreWarp = value;
                NoticePropertyChanged(nameof(FullLine), nameof(InLineInformations));
            }
        }
        public string FullLine => IgnoreWarp ? Model.ToString().Replace("\n", string.Empty) : Model.ToString();
        public bool NotIgnoreRecordTime => !Model.IgnoreTime;
        public string RecordTime { get; } = model.RecordTime.ToLocalTime().ToString($"[{Const.FullTimeFormat}]");
        public InformationLineType InfoLineType => Model.LineType;
        public ObservableCollection<InformationItemVM> InLineInformations
        {
            get
            {
                InformationItem intentItem = new(new(' ', 3 * Model.Intent));
                ObservableCollection<InformationItemVM> items = [.. Model.LineTextSources.Select(infos => new InformationItemVM(infos) { IgnoreWarp = IgnoreWarp})];
                var result = items.Count > 0 ? items : [new(new(Model.LineText ?? string.Empty, Model.BasePaintStyle))];
                result.Insert(0, new(intentItem));
                return result;
            }
        }
    }
}
