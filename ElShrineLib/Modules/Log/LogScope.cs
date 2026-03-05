using System.Collections.Concurrent;

namespace ElShrine.Modules.Log;

/// <summary>
/// Defines an interface for read-only access to a log scope, including entry collection,
/// error management, and lifecycle state.
/// </summary>
public interface IScopeAccessor : IEntryAccessor
{
    /// <summary> Gets the parent scope, or null if this is a root scope. </summary>
    LogScope? Parent { get; }

    /// <summary> Gets the session this scope belongs to. </summary>
    LogSession Session { get; }

    /// <summary> Gets the collection of log entries recorded within this scope. </summary>
    IReadOnlyCollection<LogEntry> Entries { get; }

    /// <summary> Gets a value indicating whether the scope's logical execution has ended. </summary>
    bool IsEnded { get; }

    /// <summary> Gets a value indicating whether the scope and all its children are fully closed. </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Generates summary log items based on the scope's execution result (e.g., success/failure status).
    /// </summary>
    /// <param name="config">Optional configuration for summary generation. Uses <see cref="EndConfiguration.Default"/> if null.</param>
    /// <returns>An array of <see cref="LogItem"/> representing the summary.</returns>
    LogItem[] GetSummaryItems(EndConfiguration? config = null);

    /// <summary> Gets a value indicating whether errors from child scopes should be ignored (not bubbled up). </summary>
    bool IgnoreChildrenErrors { get; }

    /// <summary> Gets the dictionary of recorded errors, keyed by the exception instance. </summary>
    IReadOnlyDictionary<Exception, ErrorRecord> Errors { get; }

    /// <summary>
    /// Handles specific exceptions within the scope using a predicate. 
    /// If the predicate returns true, the error is removed from the collection.
    /// </summary>
    /// <typeparam name="TException">The type of exception to handle.</typeparam>
    /// <param name="predicate">A function to evaluate and potentially handle the error.</param>
    void HandleErrors<TException>(Func<TException, ErrorRecord, bool> predicate) where TException : Exception;
}

/// <summary>
/// Represents a hierarchical log scope that manages a collection of entries and handles error bubbling.
/// Inherits from <see cref="InfoEntry"/> to allow the scope itself to be treated as a log entry.
/// </summary>
public sealed class LogScope : InfoEntry, IScopeAccessor
{
    /// <inheritdoc/>
    public LogScope? Parent { get; }

    /// <inheritdoc/>
    public LogSession Session { get; }

    public const string ScopeEntryType = "Scope";
    public override string EntryType => ScopeEntryType;

    private readonly ConcurrentQueue<LogEntry> _entries = new();

    /// <inheritdoc/>
    public IReadOnlyCollection<LogEntry> Entries => _entries;

    /// <summary> Number of active (not yet closed) child scopes. </summary>
    private int _activeChildrenCount = 0;

    internal LogScope(LogSession session, LogScope? parent, EntryContent info, bool ignoreChildrenErrors) : base(info)
    {
        Parent = parent;
        Session = session;
        IgnoreChildrenErrors = ignoreChildrenErrors;
        Depth = parent?.Depth + 1 ?? 0;
    }

    /// <summary>
    /// Internal method to add a log entry to this scope.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    /// <returns><c>true</c> if the entry was added; <c>false</c> if the scope has already ended.</returns>
    internal bool AddEntry(LogEntry entry)
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
                errors.TryAdd(error.Exception, rec);
                break;
        }
        return true;
    }

    #region End & Close
    private bool _isWaitingEndForClose = true;

    /// <summary> Gets or sets the configuration used when the scope ends. </summary>
    public EndConfiguration? EndConfig { get; set; }

    /// <inheritdoc/>
    public bool IsEnded { get; private set; }

    /// <inheritdoc/>
    public bool IsClosed { get; private set; }

    /// <summary> Event triggered when the scope is fully closed. </summary>
    [Obsolete("Use lifecycle management methods instead of direct event subscription.")]
    private event EventHandler? Closed;

    /// <inheritdoc/>
    public LogItem[] GetSummaryItems(EndConfiguration? config = null)
    {
        config ??= EndConfiguration.Default;
        var resultItems = new List<LogItem>();
        var isSuccess = errors.IsEmpty;

        if (config.ShowSuc && isSuccess)
            resultItems.Add(LogItem.Header("Completed", LogItemStyle.Success));

        if (config.ShowError && !isSuccess)
        {
            string errorText;
            if (errors.Count > 1)
                errorText = nameof(LogItemStyle.Error).GetPuralWithNum(errors.Count);
            else
                errorText = ErrorEntry.GetShortErrorName(errors.First().Key, nameof(LogItemStyle.Error));

            resultItems.Add(LogItem.Header($"Failed: {errorText}", LogItemStyle.Error));
        }

        if (config.ItemsBuilder is not null)
        {
            var customInfo = config.ItemsBuilder(this);
            resultItems.AddRange(customInfo.LogItems);
        }

        return [.. resultItems];
    }

    /// <summary>
    /// Marks the scope as logically ended and triggers the creation of an end-of-scope entry.
    /// If all child scopes are already closed, this will also call <see cref="Close"/>.
    /// </summary>
    internal void End()
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

    /// <summary>
    /// Fully closes the scope, notifies the parent, and bubbles up errors.
    /// </summary>
    private void Close()
    {
        if (IsClosed) return;
        IsClosed = true;
        Parent?.OnChildClosed();
        Closed?.Invoke(this, EventArgs.Empty);
        _isWaitingEndForClose = false;
        Parent?.ReciveChildErrors(errors.Values);
    }

    /// <summary>
    /// Callback triggered when a child scope is closed. 
    /// Manages the reference count and closes this scope if it's already ended.
    /// </summary>
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
    /// <inheritdoc/>
    public bool IgnoreChildrenErrors { get; }

    private readonly ConcurrentDictionary<Exception, ErrorRecord> errors = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<Exception, ErrorRecord> Errors => errors;

    /// <inheritdoc/>
    public void HandleErrors<TException>(Func<TException, ErrorRecord, bool> predicate) where TException : Exception
    {
        if (IsClosed) return;
        lock (errors)
        {
            foreach (var errorKP in errors)
            {
                if (errorKP.Key is not TException e || !predicate(e, errorKP.Value)) continue;
                errors.TryRemove(errorKP.Key, out _);
            }
        }
    }

    /// <summary>
    /// Receives errors bubbled up from child scopes unless <see cref="IgnoreChildrenErrors"/> is set.
    /// </summary>
    private void ReciveChildErrors(IEnumerable<ErrorRecord> errors)
    {
        if (IgnoreChildrenErrors) return;
        lock (this.errors)
        {
            foreach (var error in errors)
                this.errors.TryAdd(error.Exception, error);
        }
    }
    #endregion
}

/// <summary>
/// A disposable wrapper for <see cref="LogScope"/> that facilitates the 'using' pattern.
/// Ensures that <see cref="LogScope.End"/> is called when the scope goes out of context.
/// </summary>
public readonly struct LogScopeAccessor : IScopeAccessor, IDisposable
{
    /// <summary> The underlying scope being accessed. </summary>
    public readonly LogScope scope;

    private readonly bool _isScopeOwner;

    /// <summary> Gets an accessor for the parent scope. </summary>
    public LogScopeAccessor? ParentAccessor => scope.Parent is null ? null : new(scope.Parent);

    internal LogScopeAccessor(LogScope sourceScope, bool isScopeOwner = false)
    {
        scope = sourceScope;
        _isScopeOwner = isScopeOwner;
    }

    #region entry's methods & properties
    /// <inheritdoc/>
    public long Id => scope.Id;
    /// <inheritdoc/>
    public long Timestamp => scope.Timestamp;
    /// <inheritdoc/>
    public int ThreadId => scope.ThreadId;
    /// <inheritdoc/>
    public int Depth => scope.Depth;
    /// <inheritdoc/>
    public bool IsEndOfScope => false;
    /// <inheritdoc/>
    public string EntryType => scope.EntryType;
    /// <inheritdoc/>
    public EntryContent Content => scope.Content;
    /// <inheritdoc/>
    public string GetSummary() => scope.GetSummary();
    #endregion

    #region scope's methods & properties
    /// <inheritdoc/>
    public LogSession Session => scope.Session;
    /// <inheritdoc/>
    public LogScope? Parent => scope.Parent;
    /// <inheritdoc/>
    public bool IsEnded => scope.IsEnded;
    /// <inheritdoc/>
    public bool IsClosed => scope.IsClosed;
    /// <inheritdoc/>
    public IReadOnlyCollection<LogEntry> Entries => scope.Entries;

    /// <summary> Gets or sets the configuration used when the scope ends. </summary>
    public EndConfiguration? EndConfiguration { get => scope.EndConfig; set => scope.EndConfig = value; }

    /// <inheritdoc/>
    public LogItem[] GetSummaryItems(EndConfiguration? config = null)
        => scope.GetSummaryItems(config);

    /// <inheritdoc/>
    public bool IgnoreChildrenErrors => scope.IgnoreChildrenErrors;

    /// <inheritdoc/>
    public IReadOnlyDictionary<Exception, ErrorRecord> Errors => scope.Errors;

    /// <inheritdoc/>
    public void HandleErrors<TException>(Func<TException, ErrorRecord, bool> predicate) where TException : Exception
        => scope.HandleErrors(predicate);
    #endregion

    /// <summary>
    /// Disposes the accessor. If this accessor is the owner, it triggers the <see cref="LogScope.End"/> method.
    /// </summary>
    public void Dispose()
    {
        if (!_isScopeOwner) return;
        scope.End();
    }
}