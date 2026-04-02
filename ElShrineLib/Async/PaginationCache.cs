using System.Collections.Concurrent;

namespace ElShrine.Async;

public interface IPaginationInfo
{
    public int Capacity { get; }
    public bool Is0BasedIndex { get; }
}

public delegate Task<(IEnumerable<TData> responsePageContent, int dataBaseItemsCount)> CacheUpdateHandler<TQueryKey, TData>(
    TQueryKey key,
    int pageIndex,
    IPaginationInfo pagination,
    CancellationToken ct);
public class InvaildPaginationException(string msg) : Exception(msg);
public static class PaginationExtensions
{
    public static (int start, int end) To0BasedGlobalIndex(this int pageIndex, IPaginationInfo pagination)
    {
        var pindex = pageIndex + (pagination.Is0BasedIndex ? 0 : -1);
        if (pindex < 0)
            throw new InvaildPaginationException("Invaild page index.");
        var startIndex = pindex * pagination.Capacity;
        return (startIndex, startIndex + pagination.Capacity);
    }
    public static int GetPages(this int items, int capacity)
        => (items + capacity - 1) / capacity;
}
public record PaginationCacheConfiguration(
    int PreloadFrames,
    int FrameItems, 
    float FrameSizeFactor,  
    float PreloadAt,
    int NetpackMaxItems);
public class PaginationCache<TData, TQueryKey>(CacheUpdateHandler<TQueryKey, TData> updateHandler, PaginationCacheConfiguration configuration)
    where TData : IValidatable
    where TQueryKey : notnull
{
    #region Nested type
    private sealed record Frame(int Index, int Capacity, bool Is0BasedIndex = true) : IPaginationInfo;
    private sealed record NetFrame(int Index, int Capacity, bool Is0BasedIndex = true) : IPaginationInfo;
    private sealed class CacheData : ConcurrentDictionary<int, TData>, IValidatable
    {
        public int DataBaseItemsCount
        {
            get => field;
            set
            {
                field = value;
                UpdateValidator();
            }
        } = -1;

        #region Validation
        private long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        public long ValidateSeconds = 60;
        public bool IsValidated => DateTimeOffset.Now.ToUnixTimeSeconds() - unixTime < ValidateSeconds;
        public void UpdateValidator() => unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        #endregion
    }
    private sealed class FrameLoadTaskContainer(Task<IReadOnlyDictionary<int, TData>> frameLoadTask, CancellationTokenSource cts) : IDisposable
    {
        public readonly CancellationTokenSource cts = cts;
        public readonly Task<IReadOnlyDictionary<int, TData>> frameLoadTask = frameLoadTask;
        public void Dispose()
        {
            if (!frameLoadTask.IsCompleted) 
                cts.Cancel();
            cts.Dispose();
        }
    }
    #endregion

    protected readonly CacheUpdateHandler<TQueryKey, TData> updateHandler = updateHandler;
    private readonly ConcurrentDictionary<TQueryKey, CacheData> caches = [];
    private readonly SemaphoreSlim cahcesSemaphore = new(1);
    private readonly ConcurrentDictionary<int, FrameLoadTaskContainer> frameLoadTasks = [];
    private readonly SemaphoreSlim fltsSemaphore = new(1);
    public PaginationCacheConfiguration Configuration
    {
        get => field;
        set
        {
            if (field == value) return;
            fltsSemaphore.Wait();
            field = value;
            foreach (var flt in frameLoadTasks.Values)
            {
                flt.Dispose();
            }
            frameLoadTasks.Clear();
            fltsSemaphore.Release();
        }
    } = configuration;

    public async Task<IEnumerable<TData>> GetDataAsync(TQueryKey key, int uiPageIndex, IPaginationInfo uiPagination, bool forceRefreshCache = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        // get cache dict
        await cahcesSemaphore.WaitAsync(ct);
        if (!caches.TryGetValue(key, out var cache) || !cache.IsValidated || forceRefreshCache) 
        {
            cache?.Clear();
            cache?.UpdateValidator();
            caches[key] = cache ??= [];
        }
        cahcesSemaphore.Release();
        // get ui page and frame info
        GetFrame(uiPageIndex, uiPagination, out var frame, out var loc);
        var (uiPageStart, uiPageEnd) = uiPageIndex.To0BasedGlobalIndex(uiPagination);
        var (_, frameEnd) = frame.Index.To0BasedGlobalIndex(frame);

        bool uiPageCached = IsRangeCached(cache, uiPageStart, uiPageEnd, out var cachedData);
        bool frameCached = uiPageCached && IsRangeCached(cache, uiPageEnd, frameEnd, out _);

        Task currentLoadTask = Task.CompletedTask;
        // init current frame load task
        if (!frameCached)
            currentLoadTask = TriggerFrameLoadTask(key, cache, frame, CancellationToken.None);
        // init next frame preload task
        if (loc > Configuration.PreloadAt)
        {
            var nextFrame = frame with { Index = frame.Index + 1 };
            var (nextStart, nextEnd) = nextFrame.Index.To0BasedGlobalIndex(nextFrame);
            if (!IsRangeCached(cache, nextStart, nextEnd, out _))
                _ = TriggerFrameLoadTask(key, cache, nextFrame, CancellationToken.None);
        }
        // return ui page content
        if (uiPageCached)
            return cachedData;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var gotUIPageContent = IsRangeCached(cache, uiPageStart, uiPageEnd, out cachedData);
            if (gotUIPageContent)
                return cachedData;

            if (currentLoadTask.IsCompleted)
            {
                await currentLoadTask; 
                throw new InvaildPaginationException("Failed to load current frame data. Data is missing from source.");
            }
            await Task.Delay(100, ct);
        }
        throw new InvaildPaginationException("Failed to load current frame data.");
    }
    private static bool IsRangeCached(CacheData cache, int start, int end, out List<TData> cachedData)
    {
        cachedData = [];
        for (int i = start; i < end; i++)
        {
            if (cache.DataBaseItemsCount != -1 && i >= cache.DataBaseItemsCount) 
                break;
            if (!cache.TryGetValue(i, out var data) || !data.IsValidated) 
                return false;
            cachedData.Add(data);
        }
        return true;
    }
    private async Task<IReadOnlyDictionary<int, TData>> TriggerFrameLoadTask(TQueryKey key, CacheData cache, Frame frame, CancellationToken ct)
    {
        await fltsSemaphore.WaitAsync(ct);
        try
        {
            var container = frameLoadTasks.GetOrAdd(frame.Index, index =>
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var task = GetFrameDataFromSource(key, cache, frame, linkedCts.Token);
                task.ContinueWith(t =>
                {
                    fltsSemaphore.Wait();
                    frameLoadTasks.TryRemove(frame.Index, out _);
                    fltsSemaphore.Release();
                });
                return new FrameLoadTaskContainer(task, linkedCts);
            });
            return await container.frameLoadTask;
        }
        finally
        {
            fltsSemaphore.Release();
        }
    }

    private void GetFrame(int uiPageIndex, IPaginationInfo uiPagination, out Frame frame, out float loc)
    {
        var (startIndex, _) = uiPageIndex.To0BasedGlobalIndex(uiPagination);
        var frameCapacity = (int)(Configuration.FrameItems < 1 ?
            Math.Ceiling(Configuration.FrameSizeFactor * uiPagination.Capacity) : Configuration.FrameItems);
        var frameCapacityAlignedToNetPack = (int)Math.Ceiling(frameCapacity / (float)Configuration.NetpackMaxItems) * Configuration.NetpackMaxItems;
        var frameIndex = startIndex / frameCapacityAlignedToNetPack;

        frame = new Frame(frameIndex, frameCapacityAlignedToNetPack);
        loc = startIndex % frameCapacityAlignedToNetPack / (float)uiPagination.Capacity;
    }

    private async Task<IReadOnlyDictionary<int, TData>> GetFrameDataFromSource(TQueryKey key, CacheData cache, Frame frame, CancellationToken ct)
    {
        var frameCapacity = frame.Capacity;
        var maxFrameNetPacks = frameCapacity.GetPages(Configuration.NetpackMaxItems); //if frame all netpack-data relation exists
        var netPackStartIndex = maxFrameNetPacks * frame.Index; // start page index request to database
        var frameNetPacks = maxFrameNetPacks; // inital netpack-data relation count
        var netPagination = new NetFrame(netPackStartIndex, Configuration.NetpackMaxItems); 
        var resultDict = new ConcurrentDictionary<int, TData>();
        for (int i = 0; i < frameNetPacks; i++)
        {
            netPagination = netPagination with { Index = netPackStartIndex + i };
            // get netpack data
            var (responsePageContent, dataBaseItemsCount) = await updateHandler.Invoke(key, netPagination.Index, netPagination, ct);
            // update cache
            var index = 0;
            foreach (var item in responsePageContent)
            {
                var rIndex = netPagination.Index * netPagination.Capacity + index++;
                cache[rIndex] = item;
                resultDict[rIndex] = item;
                item.UpdateValidator();
            }
            cache.DataBaseItemsCount = dataBaseItemsCount;
            // update pagination
            var totalNetPacks = dataBaseItemsCount.GetPages(netPagination.Capacity);
            frameNetPacks = Math.Min(maxFrameNetPacks, totalNetPacks - netPackStartIndex);
        }
        return resultDict;
    }
}