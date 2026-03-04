using System.Collections.Concurrent;

namespace ElShrine.Modules.Log;

public sealed class LogSession : IDisposable
{
    public readonly string SessionName;
    public readonly int SessionId;
    public readonly int MaxDepth;
    private readonly AsyncLocal<LogScope> _currentScope;
    private readonly LogScope _rootScope;
    private readonly LogScopeAccessor _rootScopeAccessor;
    private readonly ConcurrentDictionary<long, LogScope> _scopes;

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
    
    private LogScope CurrentScope => _currentScope.Value ?? _rootScope;

    public LogScopeAccessor? OpenScope(InlineInfo info, bool ignoreChildrenErrors = false)
    {
        var scope = CurrentScope;
        if (scope.Depth > MaxDepth - 2)
        {
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

    public LogScopeAccessor GetCurrentScopeAccessor() => new(CurrentScope);
    public LogScopeAccessor? GetScopeAccessor(long id)
        => _scopes.TryGetValue(id, out var scope) ? new LogScopeAccessor(scope) : null;

    internal void RestoreScope(LogScope from, LogScope to)
    {
        if (CurrentScope == from) _currentScope.Value = to;
    }
    #endregion

    #region Log
    internal event LogEntriesUpdatedHandler? SessionEntriesUpdated;

    public bool LogEntry(LogEntry entry)
    {
        var scope = CurrentScope;
        var r = scope.AddEntry(entry);
        SessionEntriesUpdated?.Invoke(this, new(scope), entry);
        return r;
    }
    #region Table
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

    public void Dispose() => _rootScopeAccessor.Dispose();
}
