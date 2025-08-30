using ElShrine.EOption;
using ElShrine.Wpf.Controls;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static ElShrine.Modules.OptionHub.OptionHubVM;

namespace ElShrine.Modules.OptionHub
{
    public partial class OptionHubView
    {
        public OptionHubView()
        {
            InitializeComponent();
            OptionHubs.Add(this);
        }
        private static readonly List<OptionHubView> OptionHubs = [];
        #region Option Item Edit
        public void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is ContentControl contentContainer && contentContainer.Content is OptionItem opti)
            {
                FrameworkElement? control = null;
                Action<object?> action = o => noticeChange(opti, o);
                MethodInfo? methodInfo = null;
                if (opti.ValueType.IsEnum || OptionItemEditorBuilderBase.BuilderMethodsDictionary.TryGetValue(opti.ValueType, out methodInfo))
                {
                    if(opti.ValueType.IsEnum) methodInfo = OptionItemEditorBuilderBase.BuilderMethodsDictionary[typeof(LoadMode)];
                    if(methodInfo is not null)
                    {
                        var instance = Activator.CreateInstance(methodInfo.DeclaringType ?? throw new("Found no class."));
                        var result = methodInfo.Invoke(instance, [opti, action]);
                        if (result is FrameworkElement element) control = element;
                    }
                }
                else
                {
                    var textBox = new ETextBox
                    {
                        Text = opti.Value?.ToString(),
                        IsEnabled = false,
                    };
                    textBox.TextChanged += (_, _) => action.Invoke(textBox.Text);
                    control = textBox;
                }
                if(control is not null) contentContainer.Content = control;
            }
            static void noticeChange(OptionItem optionItem, object? newValue)
            {
                var instance = GetInstance();
                instance.RecordOptionChange(optionItem, newValue);
            }
        }

        #endregion

        #region Scroll Navigation
        private readonly Dictionary<OptionGroup, EExpander> optionGroupElements = [];
        private bool isScrolling = false;
        private EListBox? navigationListBox;
        private EScrollViewer? contentScrollViewer;
        private readonly List<EExpander> Expanders = [];

        private void Navigations_ViewLoaded(object sender, RoutedEventArgs e)
        {
            if(sender is EScrollViewer esv) contentScrollViewer = esv;
            else if (sender is EListBox elb) navigationListBox = elb;
            else if (sender is EExpander eep && eep.DataContext is OptionGroup group) optionGroupElements.TryAdd(group, eep);

            // 初始化滚动位置
            if (navigationListBox?.SelectedItem is not null && contentScrollViewer is not null)
            {
                ScrollToGroup((OptionGroup)navigationListBox.SelectedItem);
            }
        }
        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isScrolling || navigationListBox is null || contentScrollViewer is null) return;
            var visibleGroup = FindMostVisibleGroup();
            if (visibleGroup is not null && navigationListBox.SelectedItem != (object)visibleGroup)
            {
                isScrolling = true;
                navigationListBox.SelectedItem = visibleGroup;
                navigationListBox.ScrollIntoView(visibleGroup);
                isScrolling = false;
            }
        }
        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isScrolling || navigationListBox?.SelectedItem is null || contentScrollViewer is null) return;
            var selectedGroup = (OptionGroup)navigationListBox.SelectedItem;
            ScrollToGroup(selectedGroup);
        }
        private OptionGroup? FindMostVisibleGroup()
        {
            EExpander? resultExpander = null;
            if (contentScrollViewer is not null && optionGroupElements.Count > 0) 
            {
                var viewport = contentScrollViewer.ViewportHeight;
                var expandersVisibilities = new Dictionary<EExpander, double>();
                foreach (var ele in optionGroupElements)
                {
                    var expander = ele.Value;
                    var position = expander.TransformToAncestor(contentScrollViewer).Transform(new(0, 0));
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
                        var expander = Expanders[i];
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
            var result = resultExpander?.DataContext as OptionGroup;
            return result;
        }

        private void ScrollToGroup(OptionGroup group)
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
            if (sender is EExpander expander && expander.DataContext is OptionGroup)
            {
                Expanders.Remove(expander);
                Expanders.Add(expander);
            }
        }
        public static void ClearViewCache()
        {
            foreach(var oh in OptionHubs)
            {
                var ele = oh;
                ele.optionGroupElements.Clear();
                ele.Expanders.Clear();
                ele.isScrolling = false;
            }
        }
        #endregion
    }
}
