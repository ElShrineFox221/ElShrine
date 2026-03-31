using System.Collections.Concurrent;

namespace ElShrine.Async;

public sealed class TaskCoordinator<TKey, TParam, TTask> : IDisposable
    where TKey : notnull
    where TTask : Task
{
    private readonly ConcurrentDictionary<TKey, (CancellationTokenSource linkedCts, TTask task)> _entries = new();

    public TTask? Get(TKey key)
    {
        return _entries.TryGetValue(key, out var entry) ? entry.task : null;
    }
    public TTask GetOrBegin(
        TKey key,
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        lock (_entries)
        {
            var got = _entries.TryGetValue(key, out var entry);
            if (!got || entry.task.IsCompleted)
            {
                if (got)
                {
                    entry.linkedCts.Dispose();
                    _entries.TryRemove(key, out _);
                }
                var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
                entry = (cts, taskBuilder.Invoke(param, cts.Token));
                _entries[key] = entry;
            }
            return entry.task;
        }
    }
    public async Task CancelAsync(TKey key)
    {
        if (_entries.TryRemove(key, out var entry))
        {
            entry.linkedCts.Cancel();
            try
            {
                await entry.task;
            }
            catch (Exception e) when (ContainsOnlyCancelExpection(e)) { }
            finally
            {
                entry.linkedCts.Dispose();
            }
        }
    }

    private static bool ContainsOnlyCancelExpection(Exception e)
    {
        if (e is TaskCanceledException || e is OperationCanceledException)
            return e.InnerException is null || ContainsOnlyCancelExpection(e.InnerException);
        if (e is AggregateException ae)
            return ae.InnerExceptions.All(ContainsOnlyCancelExpection);
        return false;
    }
    public async Task<TTask> CancelAndBeginAsync(
        TKey key,
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        await CancelAsync(key);
        return GetOrBegin(key, param, taskBuilder, externalCt);
    }
    public async Task<TTask> WaitAndBeginAsync(
        TKey key,
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        if (_entries.TryGetValue(key, out var entry) && !entry.task.IsCompleted)
        {
            await entry.task;
        }
        return GetOrBegin(key, param, taskBuilder, externalCt);
    }

    public void Dispose()
    {
        lock (_entries)
        {
            foreach (var (linkedCts, _) in _entries.Values)
            {
                linkedCts.Cancel();
                linkedCts.Dispose();
            }
            _entries.Clear();
        }
    }
}

public sealed class SingleTaskCoordinator<TParam, TTask> : IDisposable
    where TTask : Task
{
    private (CancellationTokenSource linkedCts, TTask task)? _current;
    private readonly object _sync = new();

    public TTask? Get()
    {
        if (_current.HasValue)
            return _current.Value.task;
        return null;
    }
    public TTask GetOrBegin(
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        lock (_sync)
        {
            if (_current.HasValue && !_current.Value.task.IsCompleted)
                return _current.Value.task;

            if (_current.HasValue)
            {
                _current.Value.linkedCts.Dispose();
                _current = null;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            var task = taskBuilder(param, cts.Token);
            _current = (cts, task);
            return task;
        }
    }

    public async Task CancelAsync()
    {
        (CancellationTokenSource linkedCts, TTask task) entry;
        lock (_sync)
        {
            if (!_current.HasValue)
                return;
            entry = _current.Value;
            _current = null;
        }

        entry.linkedCts.Cancel();
        try
        {
            await entry.task.ConfigureAwait(false);
        }
        catch (Exception e) when (ContainsOnlyCancelException(e)) { }
        finally
        {
            entry.linkedCts.Dispose();
        }
    }
    public async Task<TTask> CancelAndBeginAsync(
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        await CancelAsync().ConfigureAwait(false);
        return GetOrBegin(param, taskBuilder, externalCt);
    }
    public async Task<TTask> WaitAndBeginAsync(
        TParam param,
        Func<TParam, CancellationToken, TTask> taskBuilder,
        CancellationToken externalCt = default)
    {
        TTask? existingTask = null;
        lock (_sync)
        {
            if (_current.HasValue && !_current.Value.task.IsCompleted)
                existingTask = _current.Value.task;
        }

        if (existingTask is not null)
        {
            try
            {
                await existingTask.ConfigureAwait(false);
            }
            catch (Exception e) when (ContainsOnlyCancelException(e)) { }
        }

        return GetOrBegin(param, taskBuilder, externalCt);
    }

    private static bool ContainsOnlyCancelException(Exception e)
    {
        if (e is TaskCanceledException || e is OperationCanceledException)
            return e.InnerException is null || ContainsOnlyCancelException(e.InnerException);
        if (e is AggregateException ae)
            return ae.InnerExceptions.All(ContainsOnlyCancelException);
        return false;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_current.HasValue)
            {
                _current.Value.linkedCts.Cancel();
                _current.Value.linkedCts.Dispose();
                _current = null;
            }
        }
    }
}
