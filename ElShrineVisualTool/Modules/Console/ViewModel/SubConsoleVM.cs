using ElShrine;
using ElShrine.ETimer;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElShrine.Modules.Console.ViewModel
{
    public class SubConsoleVM : ViewModelBase
    {
        public SubConsoleVM()
        {
            InfoLineStayTimer.Elapsed += (_, _) =>
            {
                if (RecentInfoLines.Count > InfoLinesCacheMinMount)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RecentInfoLines.RemoveAt(0);
                    });
                }
                else InfoLineStayTimer.Stop();
            };
        }
        public enum FilterMode { BlackList, WhiteList, All }
        public int InfoLinesCacheMaxMount { get; set; } = 6;
        public int InfoLinesCacheMinMount { get; set; } = 2;
        public NamedTimer InfoLineStayTimer = NamedTimerManager.CreateTimer(3000, true, nameof(InfoLineStayTimer));
        public FilterMode FilterStyle { get; set; } = FilterMode.WhiteList;
        public List<string> BlackList { get; set; } = [];
        public List<string> WhiteList { get; set; } = ["[Success]"];
        public Predicate<InformationLineVM>? OverrideFilter;
        private static bool DefaultFilter(SubConsoleVM sender, InformationLineVM il)
        {
            bool result = false;
            switch (sender.FilterStyle)
            {
                case FilterMode.WhiteList:
                    result = whiteListMeet(il);
                    break;
                case FilterMode.BlackList:
                    result = blackListMeet(il);
                    break;
                case FilterMode.All:
                    result = whiteListMeet(il) && blackListMeet(il);
                    break;
            }
            bool whiteListMeet(InformationLineVM il)
            {
                return sender.WhiteList.Any((li) =>
                {
                    string line = il.FullLine ?? Const.EmptyStr;
                    return line.Contains(li, StringComparison.CurrentCulture);
                });
            }
            bool blackListMeet(InformationLineVM il)
            {
                return !sender.BlackList.Any((li) =>
                {
                    string line = il.FullLine ?? Const.EmptyStr;
                    return line.Contains(li, StringComparison.CurrentCulture);
                });
            }
            return result;
        }
        public ObservableCollection<InformationLineVM> RecentInfoLines { get; } = [];
        public void AddInfoLine(InformationLineVM informationLine)
        {
            if (OverrideFilter?.Invoke(informationLine) ?? DefaultFilter(this, informationLine)) 
            {
                informationLine.IgnoreWarp = true;
                if (RecentInfoLines.Count >= InfoLinesCacheMaxMount) RecentInfoLines.RemoveAt(0);
                RecentInfoLines.Add(informationLine);
                if (RecentInfoLines.Count > InfoLinesCacheMinMount) InfoLineStayTimer.Start();
            }
        }
    }
}
