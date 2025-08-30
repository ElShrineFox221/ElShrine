using ElShrine.ECommand;
using ElShrine.EConsole;
using ElShrine.EFile;
using ElShrine.EOption;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using static ElShrine.EConsole.ConsoleManager;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.Modules
{
    [StartupClass]
    [CommandCarrier(Name = "Module")]
    public sealed class ModuleManagerVM : ViewModelBase, ISingleton<ModuleManagerVM>
    {
        private static ModuleManagerVM? Instance  = null;
        public static ModuleManagerVM GetInstance() => Instance ??= new();
        private ModuleManagerVM() : base() { }

        private class ModuleList : List<ModuleInfo> { }
        private readonly ModuleList AllModules = [];
        private readonly ModuleList LastSaveAllModules = [];
        public static TabControl? TabsController { get; set; } = null;
        public ObservableCollection<ModuleInfoVM> EnabledModules { get; } = [];
        public ObservableCollection<ModuleInfoVM> DisabledModules { get; } = [];

        private static int ModuleCompare(ModuleInfo m0, ModuleInfo m1) => Math.Sign(m0.Index - m1.Index);
        private void NoticeDirtyChanged() => NoticePropertyChanged(nameof(IsReloadRequired), nameof(CurrentModulesListIsDirty));

        public bool CurrentModulesListIsDirty
        {
            get
            {
                bool dirty = AllModules.Count != LastSaveAllModules.Count;
                for (int i = 0; i < AllModules.Count && !dirty; i++)
                {
                    if (!AllModules[i].MemberValueEqual(LastSaveAllModules[i])) dirty = true;
                }
                return dirty;
            }
        }
        public bool IsReloadRequired
        {
            get
            {
                var lastEnableds = LastSaveAllModules.Where(m => m.Enabled).ToList();
                bool irr = lastEnableds.Count == EnabledModules.Count;
                for (int i = 0; i < lastEnableds.Count && irr; i++)
                {
                    irr = lastEnableds[i].MemberValueEqual(EnabledModules[i].Model);
                } 
                return !irr;
            }
        }
        //Refresh "AllModules" and "LastSaveAllModules".
        private void RefreshAllModules()
        {
            var moduleClassInfos = ClassesManager.GetClassesByAttribute<ModuleRootAttribute>();
            var modules = moduleClassInfos.Select(ci => ci.attrs[0].ToModuleInfo(ci.type)).ToList();
            var r = DataHandler.Read<ModuleList>();
            var sucReadCount = 0;
            if (r.Success && r.Data is not null)
            {
                sucReadCount = r.Data.Count;
                LastSaveAllModules.Clear();
                foreach (var dataModule in r.Data)
                {
                    int index = modules.FindIndex(m => m.Name.EqualIgnoreCase(dataModule.Name) && m.Version.EqualIgnoreCase(dataModule.Version));
                    if (index != -1)
                    {
                        modules[index].Enabled = dataModule.Enabled;
                        modules[index].Index = dataModule.Index;
                        LastSaveAllModules.Add(modules[index].Clone());
                    }
                }
            }
            modules.Sort(ModuleCompare);
            AllModules.ReplaceAll(modules);
            ListContentInfo($"Modules list refreshed, loaded {modules.Count} {"module".GetPural(modules.Count)}, set status of {sucReadCount} {"module".GetPural(sucReadCount)}.");
            NoticeDirtyChanged();
        }
        //Confrim and reorder "AllModules", set "LastSaveAllModules".
        private void ConfrimCurrentModulesList()
        {
            if (CurrentModulesListIsDirty)
            {
                AllModules.Sort(ModuleCompare);
                LastSaveAllModules.ReplaceAll(AllModules.Select(m => m.Clone()));
                DataHandler.Write(AllModules);
                ListContentInfo($"Modules list saved.");
                NoticeDirtyChanged();
            }
        }
        private void ReloadModulesFromCurrentModulesList()
        {
            var tabControl = TabsController;
            if (tabControl is not null)
            {
                tabControl.Items.Clear();
                foreach (var module in EnabledModules)
                {
                    if (module.DataTemplateUri.IsNotEmpty() && module.DataTemplateName.IsNotEmpty())
                    {
                        ResourceDictionary resourceDict = new()
                        {
                            Source = new Uri(module.DataTemplateUri, UriKind.RelativeOrAbsolute)
                        };
                        var currentAppDics = Application.Current.Resources.MergedDictionaries;
                        if (!currentAppDics.Any((ResourceDictionary rd) => rd.Source.OriginalString == resourceDict.Source.OriginalString)) Application.Current.Resources.MergedDictionaries.Add(resourceDict);
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
                }
                NoticePropertyChanged();
                ListContentInfo($"Rebuild {EnabledModules.Count} tab {"item".GetPural(EnabledModules.Count)} with realoaded {"module".GetPural(EnabledModules.Count)}");
            }
            else ListWarnInfo([GetWarningItem(), new("Target tab controller is invalid.", InformationPaintStyle.Normal)]);
        }
        private void RefreshCurrentModulesList()
        {
            var modules = AllModules;
            EnabledModules.Clear();
            DisabledModules.Clear();
            foreach (var module in modules)
            {
                ObservableCollection<ModuleInfoVM> list = module.Enabled ? EnabledModules : DisabledModules;
                module.Index = list.Count;
                list.Add(new(module));
            }
        }
        private void RefreshCurrentModulesListIndexes()
        {
            for (int i = 0; i < EnabledModules.Count; i++) EnabledModules[i].Model.Index = i;
            for (int i = 0; i < DisabledModules.Count; i++) DisabledModules[i].Model.Index = i;
            NoticeDirtyChanged();
        }

        public static void Refresh()
        {
            var instance = GetInstance();
            instance.RefreshAllModules();
            instance.RefreshCurrentModulesList();
            instance.RefreshCurrentModulesListIndexes();
        }
        public static void Save()
        {
            var instance = GetInstance();
            instance.ConfrimCurrentModulesList();
        }
        public static void Reload()
        {
            var instance = GetInstance();
            instance.ReloadModulesFromCurrentModulesList();
        }
        [Command(Ignored = true)]
        public static void RefreshIndexes()
        {
            var instance = GetInstance();
            instance.RefreshCurrentModulesListIndexes();
        }
        public VMC RefreshModulesList => new(parameter => Refresh());
        public VMC ConfrimModulesList => new(parameter => Save());
        public VMC ReloadModulesList => new(parameter =>
        {
            if (CurrentModulesListIsDirty) ConfrimCurrentModulesList();
            if (parameter is TabControl tabControl) TabsController = tabControl;
            ReloadModulesFromCurrentModulesList();
        });
    }
}
