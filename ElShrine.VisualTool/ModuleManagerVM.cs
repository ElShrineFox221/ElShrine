using ElShrine.EConsole;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using static ElShrine.EConsole.ConsoleManager;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool
{
    public sealed class ModuleManagerVM : ViewModelBase
    {
        public ObservableCollection<ModuleInfoVM> EnabledModules { get; } = [];
        public ObservableCollection<ModuleInfoVM> DisabledModules { get; } = [];
        public ModuleManagerVM() : base()
        {
            var manager = ModuleManager.GetInstance();
            manager.ModuleListChanged += RefreshModulesList;
        }
        public TabControl? TabsController { get; set; } = null;
        public bool IsDirty => ModuleManager.GetInstance().IsDirty;

        private void RefreshModulesList(bool itemsChanged)
        {
            RefreshEnabilityList();
            if (itemsChanged) ReloadModules();
            NoticeDirtyChanged();
        }
        public static VMC Refresh => new(parameter => ModuleManager.Refresh());
        public VMC Confrim => new(parameter =>
        {
            if (parameter is TabControl tabControl) TabsController = tabControl;
            ModuleManager.Confrim();
        });
        public static VMC Discard => new(parameter => ModuleManager.Discard());

        #region VM props & methods
        
        

        private void NoticeDirtyChanged() => NoticePropertyChanged(nameof(IsDirty));
        
        
        public void RefreshIndexes()
        {
            for (int i = 0; i < EnabledModules.Count; i++) EnabledModules[i].Model.Index = i;
            for (int i = 0; i < DisabledModules.Count; i++) DisabledModules[i].Model.Index = i;
            NoticeDirtyChanged();
        }

        private void RefreshEnabilityList()
        {
            var modules = ModuleManager.GetInstance().Modules;
            EnabledModules.Clear();
            DisabledModules.Clear();
            foreach (var module in modules)
            {
                ObservableCollection<ModuleInfoVM> list = module.Enabled ? EnabledModules : DisabledModules;
                module.Index = list.Count;
                list.Add(new(module));
            }
        }
        private void ReloadModules()
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
                            ListErrorInfo(e);
                        }
                    }
                }
                NoticePropertyChanged();
                ListContentInfo($"Rebuild {EnabledModules.Count} tab {"item".GetPural(EnabledModules.Count)} with realoaded {"module".GetPural(EnabledModules.Count)}");
            }
            else ListWarnInfo([GetWarningItem(), new("Target tab controller is invalid.", InformationPaintStyle.Normal)]);
        }
        #endregion

    }
}
