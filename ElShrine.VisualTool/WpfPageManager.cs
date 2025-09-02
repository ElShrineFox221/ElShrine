using ElShrine.ECommand;
using ElShrine.EFile;
using ElShrine.EOption;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.VisualTool
{
    [StartupClass]
    [CommandCarrier(Name = "Module")]
    public sealed class WpfPageManager : ISingleton<WpfPageManager>
    {
        //Refresh: reload assemblies and get pageInfos list, write list enabled prop by last saved moudules list(LSML);
        //Confrim: save and rebuilt LSNL;
        //Cancel: discard changes of current pageInfos list(CML);
        private static WpfPageManager? Instance = null;
        public static WpfPageManager GetInstance() => Instance ??= new();
        private WpfPageManager() : base() { }

        #region Pages Collections
        private class PagesList : List<WpfPageInfo> { }

        private readonly PagesList AllPages = [];
        private readonly PagesList LastSavedAllPages = [];

        public List<WpfPageInfo> Pages => AllPages;
        #endregion

        #region Props & Fields
        public delegate void PageListChangedHandler(bool needToReload);
        public event PageListChangedHandler? PageListChanged;
        public readonly static string ModulesDiectory = Environment.CurrentDirectory;
        #endregion

        private static int PageCompare(WpfPageInfo p0, WpfPageInfo p1) => Math.Sign(p0.Index - p1.Index);
        public bool IsDirty
        {
            get
            {
                bool dirty = AllPages.Count != LastSavedAllPages.Count;
                for (int i = 0; i < AllPages.Count && !dirty; i++)
                {
                    if (!AllPages[i].MemberValueEqual(LastSavedAllPages[i])) dirty = true;
                }
                return dirty;
            }
        }

        
        private void RefreshPagesList()
        {
            ListBeginInfo([new("Refreshing Pages List...")]);
            try
            {
                //get new pages list
                var attributedPages = ClassesManager.GetClassesByAttribute<WpfPageRootVMAttribute>(true).ToList();
                var pageInfos = attributedPages.Select(item => item.attrs[0].ToModuleInfo(item.type)).ToList();
                ListContentInfo($"Loaded {pageInfos.Count} {"page".GetPural(pageInfos.Count)} from assemblies.");
                //read saved list enabilities and indexes to cur;
                int sucReadCount = 0;
                var r = DataHandler.Read<PagesList>();
                if (r.Success && r.Data is not null)
                {
                    foreach (var dataPage in r.Data)
                    {
                        int index = pageInfos.FindIndex(p => p.Name.EqualIgnoreCase(dataPage.Name) && p.Version.EqualIgnoreCase(dataPage.Version));
                        if (index != -1)
                        {
                            sucReadCount++;
                            pageInfos[index].Enabled = dataPage.Enabled;
                            pageInfos[index].Index = dataPage.Index;
                        }
                    }
                    pageInfos.Sort(PageCompare);
                }
                ListContentInfo($"Loaded saved status of {sucReadCount} {"page".GetPural(pageInfos.Count)}.");
                //replace cur pages.
                AllPages.ReplaceAll(pageInfos);
                //validate dirty
                if (IsDirty)
                {
                    ListContentInfo($"Pages list changed. Rebuild tab pannel.");
                    ConfrimPagesListChanges();
                }
                else
                {
                    ListContentInfo($"Pages list has no changes.");
                    PageListChanged?.Invoke(false);
                }
            }
            catch(Exception e)
            {
                ListErrorInfo(e);
                ListEndInfo([GetCompleteItem(false)]);
                throw;
            }
            ListEndInfo([GetCompleteItem(true)]);
        }
        private void ConfrimPagesListChanges()
        {
            AllPages.Sort(PageCompare);
            LastSavedAllPages.ReplaceAll(AllPages.Select(m => m.Clone()));
            DataHandler.Write(AllPages);
            PageListChanged?.Invoke(true);
            ListContentInfo($"Saved pages list with {LastSavedAllPages.Count} {"page".GetPural(LastSavedAllPages.Count)}.");
        }
        private void DiscardPagesListChanges()
        {
            AllPages.ReplaceAll(LastSavedAllPages.Select(m => m.Clone()));
            PageListChanged?.Invoke(false);
        }

        public static void Refresh() => GetInstance().RefreshPagesList();
        public static void Confrim() => GetInstance().ConfrimPagesListChanges();
        public static void Discard() => GetInstance().DiscardPagesListChanges();
    }
}
