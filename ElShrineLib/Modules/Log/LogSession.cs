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
public delegate void SessionEntriesUpdatedHandler(LogScope parentScope, LogEntry newEntry);

/// <summary>
/// Manages a logging session, providing hierarchical scope management and entry dispatching.
/// Uses <see cref="AsyncLocal{T}"/> to track the current scope across asynchronous execution flows.
/// </summary>
public sealed class LogSession : IDisposable
{
    /// <summary> Gets the unique name of this session. </summary>
    public readonly string SessionName;
    /// <summary> Gets the unique numeric ID of this session. </summary>
    public readonly int SessionId;
    /// <summary> Gets the maximum allowed nesting depth for scopes. </summary>
    public readonly int MaxDepth;

    private readonly AsyncLocal<LogScope> _currentScope;
    private readonly LogScope _rootScope;
    private readonly LogScopeAccessor _rootScopeAccessor;
    private readonly ConcurrentDictionary<long, LogScope> _scopes;

    /// <summary> Gets the root scope of this session. </summary>
    public LogScope RootScope => _rootScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSession"/> class.
    /// </summary>
    /// <param name="sessionName">The display name of the session.</param>
    /// <param name="sessionId">The unique ID for the session.</param>
    /// <param name="maxDepth">The maximum scope depth (defaults to 255).</param>
    public LogSession(string sessionName, int sessionId, int maxDepth = 255)
    {
        SessionName = sessionName;
        SessionId = sessionId;
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
    public LogScopeAccessor? OpenScope(EntryContent info, bool ignoreChildrenErrors = false)
    {
        var scope = CurrentScope;
        if (scope.Depth > MaxDepth - 2)
        {
            // Note: Assuming 'this.Error' is an extension method or part of the session's logging capability
            this.Error(new LogScopeTreeOverflowException(MaxDepth));
            return null;
        }
        var newScope = new LogScope(this, scope, info, ignoreChildrenErrors);
        _scopes.TryAdd(newScope.Id, newScope);
        var scopeAccessor = new LogScopeAccessor(newScope, true);
        LogEntry(newScope);
        _currentScope.Value = newScope;
        return scopeAccessor;
    }

    /// <summary> Gets the current active scope. </summary>
    public LogScope GetCurrentScope() => CurrentScope;

    /// <summary> Retrieves a specific scope by its ID. </summary>
    public LogScope? GetScope(long id)
        => _scopes.TryGetValue(id, out var scope) ? scope : null;

    /// <summary>
    /// Restores the current scope pointer to a parent scope. Used internally when a scope ends.
    /// </summary>
    internal void RestoreScope(LogScope from, LogScope to)
    {
        if (CurrentScope == from) _currentScope.Value = to;
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
        SessionEntriesUpdated?.Invoke(scope, entry);
        return r;
    }

    #region Table
    /// <summary>
    /// Helper method to build a tabular data representation as an <see cref="InfoEntry"/>.
    /// </summary>
    /// <param name="title">The title log item for the table.</param>
    /// <param name="entry">The output info entry containing the rendered table.</param>
    /// <param name="extraPad">Additional padding spaces between columns.</param>
    /// <param name="itemCols">A params array of column data, where each column is an array of <see cref="LogItem"/>.</param>
    public static void BuildTable(LogItem title, out InfoEntry? entry, int extraPad = 1, params LogItem[][] itemCols)
    {
        entry = null;
        if (itemCols.Length == 0) return;

        var rowCount = itemCols.Max(c => c?.Length ?? 0);
        if (rowCount == 0) return;

        var colWidths = new int[itemCols.Length];
        for (var i = 0; i < itemCols.Length; i++)
            colWidths[i] = (itemCols[i]?.Max(i => i.ToString().Length) ?? 0) + extraPad;

        title = LogItem.Normal($"{title}\n", title.Style);
        var tableItems = new List<LogItem>();
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < itemCols.Length; j++)
            {
                var col = itemCols[j];
                var targetWidth = colWidths[j];
                var isRowLastItem = j == itemCols.Length - 1;
                if (i < col.Length)
                {
                    var originalItem = col[i];
                    var text = originalItem.ToString().PadRight(targetWidth);
                    if (isRowLastItem) text += '\n';
                    var item = LogItem.Normal(text, originalItem.Style);
                    tableItems.Add(item);
                }
                else
                {
                    var text = new string(' ', targetWidth);
                    if (isRowLastItem) text += '\n';
                    tableItems.Add(LogItem.Normal(text));
                }
            }
        }
        entry = new InfoEntry(new([title, .. tableItems]));
    }
    #endregion
    #endregion

    /// <summary>
    /// Disposes the session by closing the root scope.
    /// </summary>
    public void Dispose() => _rootScopeAccessor.Dispose();
}