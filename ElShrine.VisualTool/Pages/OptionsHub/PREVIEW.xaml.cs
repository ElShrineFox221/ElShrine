using ElShrine.Wpf.Controls;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ElShrine.VisualTool.Pages.OptionsHub
{
    public partial class PREVIEW : UserControl
    {
        public PREVIEW()
        {
            InitializeComponent();
            //
        }
        public void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region Scroll Navigation
        private readonly ConditionalWeakTable<OptionGroupVM, EExpander> optionGroupElements = [];
        private bool isScrolling = false;
        private readonly List<WeakReference<EExpander>> Expanders = [];

        private void Navigations_ViewLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is EScrollViewer esv) PART_ContentScrollViewer = esv;
            else if (sender is EListBox elb) PART_NavigationListBox = elb;
            else if (sender is EExpander eep && eep.DataContext is OptionGroupVM group) optionGroupElements.TryAdd(group, eep);

            // 初始化滚动位置
            if (PART_NavigationListBox?.SelectedItem is not null && PART_ContentScrollViewer is not null)
            {
                ScrollToGroup((OptionGroupVM)PART_NavigationListBox.SelectedItem);
            }
        }
        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isScrolling || PART_NavigationListBox is null || PART_ContentScrollViewer is null) return;
            var visibleGroup = FindMostVisibleGroup();
            if (visibleGroup is not null && PART_NavigationListBox.SelectedItem != (object)visibleGroup)
            {
                isScrolling = true;
                PART_NavigationListBox.SelectedItem = visibleGroup;
                PART_NavigationListBox.ScrollIntoView(visibleGroup);
                isScrolling = false;
            }
        }
        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isScrolling || PART_NavigationListBox?.SelectedItem is null || PART_ContentScrollViewer is null) return;
            var selectedGroup = (OptionGroupVM)PART_NavigationListBox.SelectedItem;
            ScrollToGroup(selectedGroup);
        }
        private void RefreshElementsCache()
        {
            if (PART_ContentScrollViewer is null) return;
            var list = optionGroupElements.ToList();
            var removeList = optionGroupElements.Select(i => PART_ContentScrollViewer.IsAncestorOf(i.Value) ? null : i.Key).Where(i => i is not null);
            foreach (var i in removeList)
            {
                if (optionGroupElements.TryGetValue(i!, out var expander))
                    Expanders.RemoveAll(i => !i.TryGetTarget(out var expander1) || expander == expander1);
                optionGroupElements.Remove(i!);
            }
        }
        private OptionGroupVM? FindMostVisibleGroup()
        {
            RefreshElementsCache();
            EExpander? resultExpander = null;
            if (PART_ContentScrollViewer is not null && optionGroupElements.Any())
            {
                var viewport = PART_ContentScrollViewer.ViewportHeight;
                var expandersVisibilities = new Dictionary<EExpander, double>();
                foreach (var ele in optionGroupElements)
                {
                    var expander = ele.Value;
                    var position = expander.TransformToAncestor(PART_ContentScrollViewer).Transform(new(0, 0));
                    double visibleHeight = CalculateVisibleHeight(position.Y, expander.ActualHeight, viewport);
                    double visibilityRatio = visibleHeight / expander.ActualHeight;
                    expandersVisibilities[expander] = Math.Round(visibilityRatio, 2);
                    static double CalculateVisibleHeight(double top, double height, double viewportHeight)
                    {
                        double bottom = top + height;
                        double viewportTop = 0;
                        double viewportBottom = viewportTop + viewportHeight;
                        double visibleTop = Math.Max(top, viewportTop);
                        double visibleBottom = Math.Min(bottom, viewportBottom);
                        return Math.Max(0, visibleBottom - visibleTop);
                    }
                }
                var maxRatio = expandersVisibilities.Max(e => e.Value);
                var maxRatioExpanders = expandersVisibilities.Where(e => e.Value == maxRatio).Select(e => e.Key);
                if (maxRatioExpanders.Count() > 1 && Expanders.Count > 0)
                {
                    EExpander? expanededMaxRatioExpander = null, recentExpandedMaxRatioExpander = null;
                    for (var i = Expanders.Count - 1; i >= 0; i--)
                    {
                        if (!Expanders[i].TryGetTarget(out var expander)) continue;
                        if (expandersVisibilities[expander] == maxRatio)
                        {
                            if (expander.IsExpanded) expanededMaxRatioExpander ??= expander;
                            else recentExpandedMaxRatioExpander ??= expander;
                            if (expanededMaxRatioExpander is not null) break;
                        }
                    }
                    resultExpander = (expanededMaxRatioExpander ?? recentExpandedMaxRatioExpander) ?? throw new();
                }
                else resultExpander = maxRatioExpanders.First();
            }
            var result = resultExpander?.DataContext as OptionGroupVM;
            return result;
        }

        private void ScrollToGroup(OptionGroupVM group)
        {
            if (isScrolling || !optionGroupElements.TryGetValue(group, out var element)) return;
            isScrolling = true;

            if (element is EExpander expander && !expander.IsExpanded) expander.IsExpanded = true;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                element.BringIntoView();
                isScrolling = false;
            }, DispatcherPriority.ApplicationIdle);
        }

        private void EExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is EExpander expander && expander.DataContext is OptionGroupVM)
            {
                Expanders.RemoveAll(i => !i.TryGetTarget(out var element) || element == expander);
                Expanders.Add(new(expander));
            }
        }
        public void ClearViewCache()
        {
            optionGroupElements.Clear();
            Expanders.Clear();
            isScrolling = false;
        }
        #endregion
    }
}
