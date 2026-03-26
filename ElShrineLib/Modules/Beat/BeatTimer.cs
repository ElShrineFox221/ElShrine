using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ElShrine.Modules;

[Obsolete("To be deleted.")]
/// <summary>
/// 提供全局高精度的节拍计时服务。
/// 基于 Windows 多媒体定时器解析度调整与 <see cref="PeriodicTimer"/> 实现。
/// </summary>
internal sealed partial class BeatTimer : IDisposable
{
    #region Win32 API
    [LibraryImport("winmm.dll")]
    private static partial uint timeBeginPeriod(uint uPeriod);

    [LibraryImport("winmm.dll")]
    private static partial uint timeEndPeriod(uint uPeriod);
    #endregion

    private readonly PeriodicTimer timer;
    private readonly CancellationTokenSource cts;
    private readonly ConcurrentDictionary<long, BeatSubscriber> subscribers;

    private long autoSubscriberId = 0;
    private long tickCounter;
    private bool isDisposed;

    public BeatTimer()
    {
        _ = timeBeginPeriod(1);
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
        cts = new CancellationTokenSource();
        subscribers = [];
        tickCounter = 0;
        _ = Task.Factory.StartNew(ProcessLoopAsync, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
    private async Task ProcessLoopAsync()
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cts.Token))
            {
                tickCounter++;
                ExecuteSubscribers();
            }
        }
        catch (OperationCanceledException) { }
    }

    private void ExecuteSubscribers()
    {
        if (subscribers.IsEmpty) return;
        foreach (var subscriber in subscribers.Values)
        {
            if (tickCounter % subscriber.IntervalTicks == 0) Task.Run(subscriber.Action);
        }
    }

    /// <summary>
    /// 订阅节拍事件。
    /// </summary>
    /// <param name="intervalTicks">触发间隔的节拍数（1节拍 = 10ms）。</param>
    /// <param name="action">触发时执行的任务。</param>
    /// <returns>订阅唯一标识 ID。</returns>
    public long Subscribe(int intervalTicks, Action action)
    {
        var id = Interlocked.Increment(ref autoSubscriberId);
        subscribers.TryAdd(id, new BeatSubscriber(intervalTicks, action));
        return id;
    }

    /// <summary>
    /// 取消订阅节拍事件。
    /// </summary>
    /// <param name="id">订阅时返回的唯一 ID。</param>
    /// <param name="action">（可选）如果 ID 匹配失败，将尝试通过 Action 引用移除订阅。</param>
    /// <returns>是否成功移除订阅。</returns>
    public bool Unsubscribe(long id, Action? action = null)
    {
        if (subscribers.TryRemove(id, out _)) return true;

        if (action is not null)
        {
            var target = subscribers.FirstOrDefault(kv => kv.Value.Action == action);
            if (target.Key != default)
            {
                return subscribers.TryRemove(target.Key, out _);
            }
        }
        return false;
    }

    /// <summary>
    /// 释放计时器资源并还原系统时钟设置。
    /// </summary>
    public void Dispose()
    {
        if (isDisposed) return;

        cts.Cancel();
        timer.Dispose();
        cts.Dispose();
        _ = timeEndPeriod(1);

        isDisposed = true;
    }

    private readonly record struct BeatSubscriber(int IntervalTicks, Action Action);
}