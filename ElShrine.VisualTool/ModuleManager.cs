using ElShrine.ECommand;
using ElShrine.EFile;
using ElShrine.EOption;
using System.IO;
using System.Reflection;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.VisualTool
{
    [StartupClass]
    [CommandCarrier(Name = "Module")]
    public sealed class ModuleManager : ISingleton<ModuleManager>
    {
        //Refresh: reload assemblies and get modules list, write list enabled prop by last saved moudules list(LSML);
        //Confrim: save and rebuilt LSNL;
        //Cancel: discard changes of current modules list(CML);
        private static ModuleManager? Instance = null;
        public static ModuleManager GetInstance() => Instance ??= new();
        private ModuleManager() : base() { }

        #region Modules Collections
        private class ModuleList : List<ModuleInfo> { }

        private readonly ModuleList AllModules = [];
        private readonly ModuleList LastSaveAllModules = [];

        public List<ModuleInfo> Modules => AllModules;
        #endregion

        private static int ModuleCompare(ModuleInfo m0, ModuleInfo m1) => Math.Sign(m0.Index - m1.Index);
        public bool IsDirty
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

        public delegate void ModuleListChangedHandler(bool itemsChanged);
        public event ModuleListChangedHandler? ModuleListChanged;
        public readonly static string ModulesDiectory = Environment.CurrentDirectory;
        private const string dllExtension = ".dll";
        private void RefreshModulesList()
        {
            //load asb;
            var files = Directory.GetFiles(ModulesDiectory);
            var dllFiles = files.Where(f => Path.GetExtension(f).EqualIgnoreCase(dllExtension));
            var assembies = AppDomain.CurrentDomain.GetAssemblies().MergeDistinctAssemblies().ToList();
            assembies.AddRange(dllFiles.Select(df =>
            {
                Assembly? assembly = null;
                try
                {
                    if (assembies.FindIndex(asb => asb.Location == df) == -1) assembly = Assembly.LoadFile(df);
                }
                catch (Exception e)
                {
                    ListErrorInfo(e);
                }
                return assembly;
            })
                .Where(asb => asb is not null)
                .Select(static asb => asb ?? throw new NullReferenceException()));
           
            //get modules list
            var moduleAttrs = ClassesManager.GetClassesByAttribute<ModuleRootAttribute>(true, [..assembies]).ToList();
            var modules = moduleAttrs.Select(ma => ma.attrs[0].ToModuleInfo(ma.type)).ToList();
            //get saved modules list data
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
            //
            ModuleListChanged?.Invoke(IsDirty);
            ListContentInfo($"Modules list refreshed, loaded {modules.Count} {"module".GetPural(modules.Count)}, set status of {sucReadCount} {"module".GetPural(sucReadCount)}.");
        }
        private void ConfrimModulesListChanges()
        {
            if (IsDirty)
            {
                AllModules.Sort(ModuleCompare);
                LastSaveAllModules.ReplaceAll(AllModules.Select(m => m.Clone()));
                DataHandler.Write(AllModules);
                ModuleListChanged?.Invoke(true);
                ListContentInfo($"Modules list saved.");
            }
        }
        private void DiscardModulesListChanges()
        {
            if (IsDirty)
            {
                AllModules.ReplaceAll(LastSaveAllModules.Select(m => m.Clone()));
                ModuleListChanged?.Invoke(false);
            }
        }

        public static void Refresh() => GetInstance().RefreshModulesList();
        public static void Confrim() => GetInstance().ConfrimModulesListChanges();
        public static void Discard() => GetInstance().DiscardModulesListChanges();
    }

    public enum ModulesUpate
    {
        Confrim, Refresh, Discard
    }
}
