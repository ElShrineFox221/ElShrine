namespace ElShrine.Modules.Log;

public readonly struct LogScopeAccessor : IEntryAccessor, IDisposable
{
    private readonly LogScope _sourceScope;
    private readonly bool _isScopeOwner;
    public LogScopeAccessor? Parent => _sourceScope.Parent is null ? null : new(_sourceScope.Parent);
    public LogSession Session => _sourceScope.Session;
    public long Id => _sourceScope.Id;
    public long Timestamp => _sourceScope.Timestamp;
    public int ThreadId => _sourceScope.ThreadId;
    public int Depth => _sourceScope.Depth;
    public bool IsEndOfScope => false;
    public InlineInfo Info => _sourceScope.GetInlineInfo();
    public string GetSummary() => _sourceScope.GetSummary();
    public string GetEntryType() => _sourceScope.GetEntryType();
    public bool IsEnded => _sourceScope.IsEnded;
    public bool IsClosed => _sourceScope.IsClosed;
    public EndConfiguration? EndConfiguration { get => _sourceScope.EndConfig; set => _sourceScope.EndConfig = value; }
    public IReadOnlyDictionary<Exception, ErrorRecord> Errors => _sourceScope.Errors;
    internal LogScopeAccessor(LogScope sourceScope, bool isScopeOwner = false)
    {
        _sourceScope = sourceScope;
        _isScopeOwner = isScopeOwner;
    }
    public void HandleErrors<TException>(Func<TException, ErrorRecord, bool> predicate) where TException : Exception
        => _sourceScope.HandleErrors(predicate);
    public LogItem[] GetSummaryItems(EndConfiguration? config = null)
        => _sourceScope.GetSummaryItems(config);
    public void Dispose()
    {
        if (!_isScopeOwner) return;
        _sourceScope.End();
    }
}
