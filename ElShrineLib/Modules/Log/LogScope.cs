using System.Collections.Concurrent;

namespace ElShrine.Modules.Log;

internal sealed class LogScope : InfoEntry
{
    public LogScope? Parent { get; }
    public LogSession Session { get; }

    private readonly ConcurrentQueue<LogEntry> _entries = new();
    public IEnumerable<LogEntry> Entries => _entries;
    private int _activeChildrenCount = 0;

    public LogScope(LogSession session, LogScope? parent, InlineInfo info, bool ignoreChildrenErrors) : base(info)
    {
        Parent = parent;
        Session = session;
        IgnoreChildrenErrors = ignoreChildrenErrors;
        Depth = parent?.Depth + 1 ?? 0;
    }

    public bool AddEntry(LogEntry entry)
    {
        if (IsEnded) return false;
        entry.Depth = Depth + 1;
        _entries.Enqueue(entry);
        switch (entry)
        {
            case LogScope subScope:
                Interlocked.Increment(ref _activeChildrenCount);
                if (!subScope.IsClosed) _isWaitingEndForClose = false;
                break;
            case ErrorEntry error:
                if (!error.AddToErrors) break;
                var rec = new ErrorRecord(new(this), error.Exception, error.Exception.StackTrace);
                Errors.TryAdd(error.Exception, rec);
                break;
        }
        return true;
    }

    #region End & Close
    private bool _isWaitingEndForClose = true;
    public EndConfiguration? EndConfig { get; set; }
    public bool IsEnded { get; private set; }
    public bool IsClosed { get; private set; }
    public event EventHandler? Closed;
    public LogItem[] GetSummaryItems(EndConfiguration? config)
    {
        config ??= EndConfiguration.Default;

        var resultItems = new List<LogItem>();
        var isSuccess = Errors.IsEmpty;

        if (config.ShowSuc && isSuccess)
            resultItems.Add(LogItem.Header("Completed", LogItemStyle.Success));
        if (config.ShowError && !isSuccess)
        {
            string errorText;
            if (Errors.Count > 1)
                errorText = nameof(LogItemStyle.Error).GetPuralWithNum(Errors.Count);
            else
                errorText = ErrorEntry.GetShortErrorName(Errors.First().Key, nameof(LogItemStyle.Error));
            resultItems.Add(LogItem.Header($"Failed: {errorText}", LogItemStyle.Error));
        }

        if (config.ItemsBuilder is not null)
        {
            var customInfo = config.ItemsBuilder(new(this));
            resultItems.AddRange(customInfo.Items);
        }

        return [.. resultItems];
    }
    public void End()
    {
        lock (this)
        {
            if (IsEnded) return;
            var endConfig = EndConfig ?? EndConfiguration.Default;
            var items = GetSummaryItems(endConfig);
            var endEntry = new InfoEntry(items)
            {
                IsEndOfScope = true
            };
            Session.LogEntry(endEntry);
            IsEnded = true;
            if (Parent is not null) Session.RestoreScope(this, Parent);
            if (_isWaitingEndForClose) Close();
        }
    }

    private void Close()
    {
        if (IsClosed) return;
        IsClosed = true;
        Parent?.OnChildClosed();
        Closed?.Invoke(this, EventArgs.Empty);
        _isWaitingEndForClose = false;
        Parent?.ReciveChildErrors(Errors.Values);
    }

    private void OnChildClosed()
    {
        if (Interlocked.Decrement(ref _activeChildrenCount) == 0)
        {
            _isWaitingEndForClose = true;
            if (IsEnded) Close();
        }
    }
    #endregion

    #region Bubble
    public bool IgnoreChildrenErrors { get; }
    public ConcurrentDictionary<Exception, ErrorRecord> Errors = [];

    public void HandleErrors<TException>(Func<TException, ErrorRecord, bool> predicate) where TException : Exception
    {
        if (IsClosed) return;
        lock (Errors)
        {
            foreach (var errorKP in Errors)
            {
                if (errorKP.Key is not TException e || !predicate(e, errorKP.Value)) continue;
                Errors.TryRemove(errorKP.Key, out _);
            }
        }
    }

    private void ReciveChildErrors(IEnumerable<ErrorRecord> errors)
    {
        if (IgnoreChildrenErrors) return;
        lock (Errors)
        {
            foreach (var error in errors)
                Errors.TryAdd(error.Exception, error);
        }
    }
    #endregion
}
