using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool
{
    public sealed class WpfPageManagerVM : ViewModelBase
    {
        public ObservableCollection<WpfPageInfoVM> EnabledModules { get; } = [];
        public ObservableCollection<WpfPageInfoVM> DisabledModules { get; } = [];
        public WpfPageManagerVM() : base()
        {
            var manager = WpfPageManager.GetInstance();
            manager.PageListChanged += RefreshPagesList;
        }
        public TabControl? TabsController { get; set; } = null;
        public static bool IsDirty => WpfPageManager.GetInstance().IsDirty;

        
        public static VMC Refresh => new(parameter => WpfPageManager.Refresh());
        public VMC Confrim => new(parameter =>
        {
            if (parameter is TabControl tabControl) TabsController = tabControl;
            WpfPageManager.Confrim();
        });
        public static VMC Discard => new(parameter => WpfPageManager.Discard());

        #region VM props & methods
        private void NoticeDirtyChanged() => NotifyPropertiesChanged(nameof(IsDirty));
        
        public void RefreshIndexes()
        {
            for (int i = 0; i < EnabledModules.Count; i++) EnabledModules[i].Model.Index = i;
            for (int i = 0; i < DisabledModules.Count; i++) DisabledModules[i].Model.Index = i;
            NoticeDirtyChanged();
        }
        private void RefreshPagesList(bool needToReload)
        {
            RefreshEnabilityList();
            if (needToReload) ReloadPages();
            NoticeDirtyChanged();
        }
        private void RefreshEnabilityList()
        {
            var modules = WpfPageManager.GetInstance().Pages;
            EnabledModules.Clear();
            DisabledModules.Clear();
            foreach (var module in modules)
            {
                ObservableCollection<WpfPageInfoVM> list = module.Enabled ? EnabledModules : DisabledModules;
                module.Index = list.Count;
                list.Add(new(module));
            }
        }
        private void ReloadPages()
        {
            var tabControl = TabsController;
            if (tabControl is not null)
            {
                tabControl.Items.Clear();
                foreach (var module in EnabledModules)
                {
                    if (module.DataTemplateUri.IsNotEmpty() && module.DataTemplateName.IsNotEmpty())
                    {
                        try
                        {
                            ResourceDictionary resourceDict = new()
                            {
                                Source = new Uri(module.DataTemplateUri, UriKind.RelativeOrAbsolute)
                            };
                            var currentAppDics = Application.Current.Resources.MergedDictionaries;
                            if (!currentAppDics.Any((rd) => rd.Source.OriginalString == resourceDict.Source.OriginalString)) Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                            var dataTemplate = resourceDict[module.DataTemplateName] as DataTemplate;
                            if (dataTemplate is not null)
                            {
                                var viewModel = module.ViewModel;
                                ContentControl content = new()
                                {
                                    Content = viewModel,
                                    ContentTemplate = dataTemplate,
                                };
                                TabItem tabItem = new()
                                {
                                    Header = module.Name,
                                    Content = content,
                                };
                                tabControl.Items.Add(tabItem);
                            }
                        }
                        catch(Exception e)
                        {
                            //LOG ListErrorInfo(e);
                        }
                    }
                }
                NotifyPropertiesChanged();
                //LOG ListContentInfo($"Rebuild {EnabledModules.Count} tab {"item".GetPural(EnabledModules.Count)} with realoaded {"module".GetPural(EnabledModules.Count)}");
            }
            //LOG else ListWarnInfo([GetWarningItem(), new(" Target tab controller is invalid.")]);
        }
        #endregion

    }
}
