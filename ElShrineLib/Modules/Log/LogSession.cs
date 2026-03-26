using System.Collections.Concurrent;

namespace ElShrine.Modules.Log;

/// <summary>
/// Exception thrown when the log scope nesting depth exceeds the defined <see cref="LogSession.MaxDepth"/>.
/// </summary>
/// <param name="max">The maximum allowed depth.</param>
public class LogScopeTreeOverflowException(int max) : Exception($"Maximum scope depth reached: {max}");

/// <summary>
/// Encapsulates a method that handles entry updates within a specific session context.
/// </summary>
public delegate void SessionEntriesUpdatedHandler(LogScopeAccessor parentScopeAccessor, LogEntry newEntry);

/// <summary>
/// Manages a logging session, providing hierarchical scope management and entry dispatching.
/// Uses <see cref="AsyncLocal{T}"/> to track the current scope across asynchronous execution flows.
/// </summary>
public sealed class LogSession : ILogger, IDisposable
{
    /// <summary> Gets the maximum allowed nesting depth for scopes. </summary>
    public readonly int MaxDepth;

    private readonly AsyncLocal<LogScope> _currentScope;
    private readonly LogScope _rootScope;
    private readonly LogScopeAccessor _rootScopeAccessor;
    private readonly ConcurrentDictionary<long, LogScope> _scopes;


    public string Name { get; }
    public int Id { get; }
    public LogScopeAccessor RootScopeAccessor => new(_rootScope);

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSession"/> class.
    /// </summary>
    /// <param name="sessionName">The display name of the session.</param>
    /// <param name="sessionId">The unique ID for the session.</param>
    /// <param name="maxDepth">The maximum scope depth (defaults to 255).</param>
    public LogSession(string sessionName, int sessionId, int maxDepth = 255)
    {
        Name = sessionName;
        Id = sessionId;
        MaxDepth = maxDepth;
        _currentScope = new();
        _rootScope = new(this, null, string.Empty, true);
        _rootScopeAccessor = new(_rootScope);
        _currentScope.Value = _rootScope;
        _scopes = new();
    }

    #region Operations

    /// <summary>
    /// Gets the current active scope in the current async context, or the root scope if none is set.
    /// </summary>
    private LogScope CurrentScope => _currentScope.Value ?? _rootScope;

    /// <summary>
    /// Opens a new nested log scope.
    /// </summary>
    /// <param name="info">The description content for the new scope.</param>
    /// <param name="ignoreChildrenErrors">If true, errors from sub-scopes will not bubble up to this scope.</param>
    /// <returns>A <see cref="LogScopeAccessor"/> to manage the new scope, or <c>null</c> if the max depth is reached.</returns>
    public LogScopeAccessor OpenScope(EntryContent info, bool ignoreChildrenErrors = false)
    {
        var scope = CurrentScope;
        if (scope.Depth > MaxDepth - 2)
        {
            this.Error(new LogScopeTreeOverflowException(MaxDepth));
            return new(CurrentScope);
        }
        var newScope = new LogScope(this, scope, info, ignoreChildrenErrors);
        _scopes.TryAdd(newScope.Id, newScope);
        var scopeAccessor = new LogScopeAccessor(newScope, true);
        LogEntry(newScope);
        _currentScope.Value = newScope;
        return scopeAccessor;
    }

    /// <summary> Gets the current active scope. </summary>
    public LogScopeAccessor GetCurrentScopeAccessor() => new(CurrentScope);

    /// <summary> Retrieves a specific scope by its ID. </summary>
    public bool TryGetScope(long id, out LogScopeAccessor scopeAccessor)
    {
        var suc = _scopes.TryGetValue(id, out var scope);
        scopeAccessor = suc ? new(scope!) : new(_rootScope);
        return suc;
    }

    public void OnScopeEnded(LogScope endedScope)
    {
        if (CurrentScope == endedScope && endedScope.Parent is not null)
            _currentScope.Value = endedScope.Parent;
    }
    #endregion

    #region Log
    /// <summary> Occurs when a new entry is added to any scope within this session. </summary>
    public event SessionEntriesUpdatedHandler? SessionEntriesUpdated;

    /// <summary>
    /// Records a log entry into the current active scope and triggers session-level updates.
    /// </summary>
    /// <param name="entry">The entry to be recorded.</param>
    /// <returns><c>true</c> if the entry was successfully added; otherwise, <c>false</c>.</returns>
    public bool LogEntry(LogEntry entry)
    {
        var scope = CurrentScope;
        var r = scope.AddEntry(entry);
        SessionEntriesUpdated?.Invoke(new(scope), entry);
        return r;
    }

    
    #endregion

    /// <summary>
    /// Disposes the session by closing the root scope.
    /// </summary>
    public void Dispose() => _rootScopeAccessor.Dispose();
}