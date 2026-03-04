using System.Collections.Concurrent;

namespace ElShrine.Async;

sealed record PreloadTask(Task Task, CancellationTokenSource Cts, int OuterPageIndex)
{
    public void Cancel()
    {
        Cts.Cancel();
        Cts.Dispose();
    }
}
public class PaginationCacheManager<TValidatableData>(Func<ConcurrentDictionary<int, TValidatableData>, object, PageInfoRecord, int, CancellationToken, Task<int>> getDataFromSourceAsyncFunc)
    where TValidatableData : class, IValidatable
{
    private sealed class CacheData : ConcurrentDictionary<int, TValidatableData>, IValidatable
    {
        private int itemsCount;
        public int ItemsCount
        {
            get => itemsCount;
            set
            {
                itemsCount = value;
                InitializeValidator();
            }
        }
        public RefContainer<PreloadTask> PreloadTaskContainer { get; init; } = new();
        #region PreloadTask

        #endregion

        #region Validation
        long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        public long ValidateSeconds = 60;
        public bool IsValidated => DateTimeOffset.Now.ToUnixTimeSeconds() - unixTime < ValidateSeconds;
        public void InitializeValidator() => unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        #endregion
    }
    private readonly ConcurrentDictionary<object, CacheData> cacheDataByQueryInfo = [];
    public async Task<IEnumerable<TValidatableData>> GetData(bool refreshCache, object cacheKey, IPageInfo uiContentPage, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cacheData = cacheDataByQueryInfo.GetOrAdd(cacheKey, []);
        var doInitalize = false;
        lock(cacheData) doInitalize = cacheData.IsEmpty || refreshCache;
        GetOuterPageInfo(uiContentPage, out var outerPage, out var loc);
        if (doInitalize)
        {
            // do initalQuery async
            await GetDataFromSource(cacheData, cacheKey, outerPage, CancellationToken.None);
            cacheData.InitializeValidator();
        }
        //get data from cache
        cancellationToken.ThrowIfCancellationRequested();
        var index = uiContentPage.PIndex * uiContentPage.PCapacity;
        var endIndex = Math.Min(index + uiContentPage.PCapacity, cacheData.ItemsCount) - 1;
        var data = new List<TValidatableData>();
        getDataFromCache();
        void getDataFromCache()
        {
            if (!cacheData.IsValidated) return;
            for (; index <= endIndex; index++)
            {
                if (cacheData.TryGetValue(index, out var item) && item.IsValidated) data.Add(item);
                else break;
            }
        }
        //validate data-cache
        //missing data, reload current outer page
        cancellationToken.ThrowIfCancellationRequested();
        if (index <= endIndex) 
        {
            //reload current outer page
            await GetDataFromSource(cacheData, cacheKey, outerPage, CancellationToken.None);
            //get data from new chache
            getDataFromCache();
        }
        //data-cache finished, preload next outer page
        else if (PreloadPageEnabled && loc >= PreloadAt)
        {
            var preloadPage = new PageInfoRecord(outerPage.PCount, outerPage.PIndex + 1, outerPage.PCapacity);
            //Here do preload without wait for result
            bool doPreload = !CheckIfOuterPageCached(cacheData, preloadPage);
            if (doPreload)
            {
                var container = cacheData.PreloadTaskContainer;
                lock (container)
                {
                    var content = container.Content;
                    if (content is not null)
                    {
                        doPreload = content.OuterPageIndex != preloadPage.PIndex;
                        if (doPreload) content.Cancel();
                    }
                    if (doPreload)
                    {
                        var newCts = new CancellationTokenSource();
                        var newTask = PreloadOuterPageAsync(cacheKey, preloadPage, newCts.Token);
                        var preloadTask = new PreloadTask(newTask, newCts, preloadPage.PIndex);
                        container.Content = preloadTask;
                    }
                }
            }
        }
        //final validate
        if (index <= endIndex) throw new Exception("Data failed");
        return data;
    }

    private async Task GetDataFromSource(CacheData cacheData, object cacheKey, PageInfoRecord outerPageInfo, CancellationToken cancellationToken)
    {
        var dic = new ConcurrentDictionary<int, TValidatableData>();
        var outerCapa = outerPageInfo.PCapacity;
        var packCount = (outerCapa + NetpackageMaxSize - 1) / NetpackageMaxSize;
        var packStartIndex = (outerPageInfo.PIndex * outerCapa) / NetpackageMaxSize;
        var adjustedPCount = outerPageInfo.PCount * packCount;
        for (int i = 0; i < packCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPackCapa = Math.Min(NetpackageMaxSize, outerCapa - i * NetpackageMaxSize);
            var netPackPage = new PageInfoRecord(adjustedPCount, packStartIndex + i, currentPackCapa);
            if (currentPackCapa == 0) break;
            cacheData.ItemsCount = await GetDataFromSourceAsyncFunc(dic, cacheKey, netPackPage, i, cancellationToken);
            foreach (var item in dic)
            {
                cacheData[item.Key] = item.Value;
                item.Value.InitializeValidator();
            }
            dic.Clear();
        }
    }

    public Func<ConcurrentDictionary<int, TValidatableData>, object, PageInfoRecord, int, CancellationToken, Task<int>>GetDataFromSourceAsyncFunc { get; init; } = getDataFromSourceAsyncFunc;

    private async Task PreloadOuterPageAsync(object cacheKey, PageInfoRecord preloadPage, CancellationToken cancellationToken)
    {
        if (cacheDataByQueryInfo.TryGetValue(cacheKey, out var cacheData))
        {
            try
            {
                await GetDataFromSource(cacheData, cacheKey, preloadPage, cancellationToken);
            }
            catch (OperationCanceledException) { }
            finally
            {
                var container = cacheData.PreloadTaskContainer;
                lock (container)
                {
                    var content = container.Content;
                    if (content != null && content.OuterPageIndex == preloadPage.PIndex) container.Content = null;
                }
            }
        }
    }
    private static bool CheckIfOuterPageCached(ConcurrentDictionary<int, TValidatableData> cacheData, PageInfoRecord outerPageInfo)
    {
        var startIndex = outerPageInfo.PIndex * outerPageInfo.PCapacity;
        var endIndex = startIndex + outerPageInfo.PCapacity - 1;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (!cacheData.TryGetValue(i, out var value) || !value.IsValidated) return false;
        }
        return true;
    }
    private void GetOuterPageInfo(IPageInfo pageInfo, out PageInfoRecord outerPageInfo, out float loc)
    {
        var startIndex = pageInfo.PIndex * pageInfo.PCapacity;
        var outerCapacity = (int)Math.Ceiling(pageInfo.PCapacity * Math.Max(OuterPageRelaSize, 1) / NetpackageMaxSize) * NetpackageMaxSize;
        var outerIndex = startIndex / outerCapacity;
        var totalOuterPages = (pageInfo.PCount * pageInfo.PCapacity + outerCapacity - 1) / outerCapacity;
        outerPageInfo = new PageInfoRecord(totalOuterPages, outerIndex, outerCapacity);
        loc = startIndex % outerCapacity / (float)pageInfo.PCapacity;
    }
    public bool PreloadPageEnabled { get; set; } = true;
    public float OuterPageRelaSize { get; set; } = 4f;
    public float PreloadAt { get; set; } = 0.5f;
    //Separation
    public int NetpackageMaxSize { get; set; } = 100;
}
